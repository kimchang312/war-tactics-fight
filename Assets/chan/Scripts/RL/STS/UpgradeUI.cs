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

    private void OnEnable()
    {
        // 활성화될 때 isFreeUpgrade 상태를 확인하고 UI 업데이트
        if (RogueLikeData.Instance.isFreeUpgrade && _currentChoices != null && _currentChoices.Count > 0)
        {
            // 기존 옵션들의 가격을 0으로 업데이트
            foreach (var opt in _currentChoices)
            {
                if (opt.cost != 0)
                {
                    opt.cost = 0;
                    opt.upgradeCost = "0";
                }
            }
            // UI 텍스트 업데이트
            UpdateCostTexts();
        }
    }

    private void UpdateCostTexts()
    {
        // 현재 표시된 옵션들의 가격 텍스트를 업데이트
        foreach (Transform child in optionContainer)
        {
            var nameTxt = child.Find("UpgradeName")?.GetComponent<TextMeshProUGUI>();
            var costTxt = child.Find("UpgradeCost")?.GetComponent<TextMeshProUGUI>();
            if (nameTxt != null && costTxt != null && _currentChoices != null)
            {
                var matched = _currentChoices.FirstOrDefault(o => o.upgradeName == nameTxt.text);
                if (matched != null)
                {
                    costTxt.text = matched.upgradeCost;
                }
            }
        }
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

        var random = RogueLikeData.Instance.GetRandomBySeed();
        _currentChoices = options
            .OrderBy(_ => (float)random.NextDouble())
            .Take(3)
            .ToList();
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
        if (cost > 0 && !RogueLikeData.Instance.CanSpendGold(cost))
        {
            return;
        }

        int before = RogueLikeData.Instance.GetUpgrade(opt.unitType, opt.isAttack);

        RogueLikeData.Instance.IncreaseUpgrade(opt.unitType, opt.isAttack, true);

        int after = RogueLikeData.Instance.GetUpgrade(opt.unitType, opt.isAttack);

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
        UIManager.Instance.UIUpdateAll();

        foreach (var stateUi in FindObjectsOfType<UpgradeStateUI>(true))
        {
            stateUi.RefreshFromData();
        }
    }
    private void OnRerollClicked()
    {
        (int, bool) rr = RogueLikeData.Instance.GetRerollChance();
        if (rr.Item1 <= 0 && !rr.Item2)
        {
            return;
        }

        // 리롤 차감
        RogueLikeData.Instance.AddReroll(- 1);
        ShowRandomChoices();
        UIManager.Instance.UIUpdateAll();
    }

    private void UpdateRerollButton()
    {
        (int, bool) reroll = RogueLikeData.Instance.GetRerollChance();
        rerollButton.interactable = (reroll.Item1 > 0 && reroll.Item2);
    }
}
