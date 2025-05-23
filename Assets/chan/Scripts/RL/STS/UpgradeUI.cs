using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    [SerializeField] private RectTransform optionContainer;
    [SerializeField] private GameObject optionButtonPrefab;
    [SerializeField] private Button rerollButton;
    //[SerializeField] private TextMeshProUGUI nameText;
    //[SerializeField] private TextMeshProUGUI costText;

    // 내부 클래스: 이제 Level / Cost 는 생성자 인자로 한 번만 계산
    public class UpgradeOption
    {
        public static readonly string[] UnitTypeNames = 
        {
        "창병", "전사", "궁병", "중보병",
        "암살자", "경기병", "중기병", "지원"
        };

        public int unitType;
        public bool isAttack;
        public int currentLevel;
        public int nextLevel;
        public int cost; // 보여지는 비용
        public int originalCost; // 실제 적용되는 업그레이드 비용
        public string upgradeName;
        public string upgradeCost;

        public UpgradeOption(int unitType, bool isAttack, int currentLevel, int cost)
        {
            this.unitType = unitType;
            this.isAttack = isAttack;
            this.currentLevel = currentLevel;
            this.nextLevel = currentLevel + 1;
            this.cost = cost;
            this.originalCost = cost;

            var typeName = UnitTypeNames[unitType];
            var upgradeType = isAttack ? "공격" : "방어";
            upgradeName = $"{typeName} {upgradeType} {nextLevel}";
            upgradeCost = cost.ToString();
        }
    }

    private List<UpgradeOption> _currentChoices;

    private void Awake()
    {
        // 리롤 버튼 리스너 등록
        rerollButton.onClick.RemoveAllListeners();
        rerollButton.onClick.AddListener(OnRerollClicked);
    }

    private void Start()
    {
        ShowRandomChoices();
        UpdateRerollButton();
    }

    public void ShowRandomChoices()
    {
        // 1) 후보 리스트 다시 구성할 때, 매번 GetUpgrade 호출
        var options = new List<UpgradeOption>();
        for (int type = 0; type < 8; type++)
        {
            int atkLvl = RogueLikeData.Instance.GetUpgrade(type, true);
            if (atkLvl < 5)
            {
                int cost = RogueLikeData.Instance.GetCostTable(atkLvl);
                options.Add(new UpgradeOption(type, true, atkLvl, cost));
            }

            int defLvl = RogueLikeData.Instance.GetUpgrade(type, false);
            if (defLvl < 5)
            {
                int cost = RogueLikeData.Instance.GetCostTable(defLvl);
                options.Add(new UpgradeOption(type, false, defLvl, cost));
            }
        }

        // 3) 랜덤 셔플 후 최대 3개 선택
        _currentChoices = options
            .OrderBy(_ => Random.value)
            .Take(3)
            .ToList();
        // 무료업그레이드 라면
        if (RogueLikeData.Instance.isFreeUpgrade == true)
        {
            foreach (var opt in _currentChoices)
            {
                opt.cost = 0;
                opt.upgradeCost = "0";
            }
        }

        // 4) UI 갱신
        foreach (Transform t in optionContainer)
            Destroy(t.gameObject);

        foreach (var opt in _currentChoices)
        {
            var go = Instantiate(optionButtonPrefab, optionContainer);
            var btn = go.GetComponent<Button>();
            var iconImage = go.GetComponent<Image>();
            if (iconImage != null)
            {
                string spriteName = UpgradeOption.UnitTypeNames[opt.unitType];
                Debug.Log(spriteName);
                string path = opt.isAttack
                    ? $"UpgradeIcons/Upgrade_{spriteName}_aggressive"
                : $"UpgradeIcons/Upgrade_{spriteName}_defensive";
                var sprite = Resources.Load<Sprite>(path);
                if (sprite != null)
                iconImage.sprite = sprite;
                else
                    Debug.LogWarning($"[UpgradeUI] 스프라이트 못찾음: UpgradeIcons/{spriteName}"); 
            }
            var nameTxt = go.transform.Find("UpgradeName")?.GetComponent<TextMeshProUGUI>();
            var costTxt = go.transform.Find("UpgradeCost")?.GetComponent<TextMeshProUGUI>();
            nameTxt.text = opt.upgradeName;
            costTxt.text = opt.upgradeCost;

            // 클릭 리스너
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnOptionClicked(opt));
        }
        UpdateRerollButton();
    }

    private void OnOptionClicked(UpgradeOption opt)
    {
        int cost = opt.cost;
        if (cost > 0)
        {
            int currentGold = RogueLikeData.Instance.GetCurrentGold();
            if (currentGold < cost)
            {
                Debug.Log("골드가 부족합니다.");
                return;
            }
        }

        // 기존 레벨 확인
        int before = RogueLikeData.Instance.GetUpgrade(opt.unitType, opt.isAttack);
        Debug.Log($"🛠️ 업그레이드 전: {UpgradeOption.UnitTypeNames[opt.unitType]} {(opt.isAttack ? "공격" : "방어")} 레벨 {before}");

        // 2) 강화 수행 (isPurchase=false 로 내부 중복 차감 방지)
        RogueLikeData.Instance.IncreaseUpgrade(opt.unitType, opt.isAttack, true);

        // 이후 레벨 확인
        int after = RogueLikeData.Instance.GetUpgrade(opt.unitType, opt.isAttack);
        Debug.Log($"✅ 업그레이드 후: {UpgradeOption.UnitTypeNames[opt.unitType]} {(opt.isAttack ? "공격" : "방어")} 레벨 {after}");


        // 버튼 찾기: 현재 선택된 opt와 동일한 버튼 찾아서 비활성화
        foreach (Transform child in optionContainer)
        {
            var nameTxt = child.Find("UpgradeName")?.GetComponent<TextMeshProUGUI>();
            var costTxt = child.Find("UpgradeCost")?.GetComponent<TextMeshProUGUI>();
            var btn = child.GetComponent<Button>();

            if (btn != null && btn.interactable && nameTxt.text != opt.upgradeName)
            {
                var matched = _currentChoices.FirstOrDefault(o => o.upgradeName == nameTxt.text);
                if (matched != null && matched.cost == 0)
                {
                    matched.cost = matched.originalCost;
                    matched.upgradeCost = matched.originalCost.ToString();
                    costTxt.text = matched.upgradeCost;
                }
            }
        }
        foreach (Transform child in optionContainer)
        {
            var nameTxt = child.Find("UpgradeName")?.GetComponent<TextMeshProUGUI>();
            if (nameTxt != null && nameTxt.text == opt.upgradeName)
            {
                var btn = child.GetComponent<Button>();
                if (btn != null)
                    btn.interactable = false;
                break;
            }
        }
        // 3) UI 갱신
        UIManager.Instance.UIUpdateAll();
    }
    private void OnRerollClicked()
    {
        int rr = RogueLikeData.Instance.GetRerollChance();
        if (rr <= 0)
        {
            Debug.Log("리롤 횟수가 없습니다.");
            return;
        }

        // 리롤 차감
        RogueLikeData.Instance.SetRerollChance(rr - 1);
        ShowRandomChoices();        // 옵션만 갱신
        UIManager.Instance.UIUpdateAll();
    }

    private void UpdateRerollButton()
    {
        // 남은 리롤이 1 이상일 때만 활성화
        rerollButton.interactable = RogueLikeData.Instance.GetRerollChance() > 0;
    }
}
