using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    [SerializeField] private RectTransform optionContainer;
    [SerializeField] private GameObject optionButtonPrefab;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    
    // 내부 클래스: 이제 Level / Cost 는 생성자 인자로 한 번만 계산
    private class UpgradeOption
    {
        private static readonly string[] UnitTypeNames = 
        {
        "창병", "전사", "궁병", "중보병",
        "암살자", "경기병", "중기병", "지원"
        };

        public int unitType;
        public bool isAttack;
        public int currentLevel;
        public int nextLevel;
        public int cost;
        public string upgradeName;
        public string upgradeCost;

        public UpgradeOption(int unitType, bool isAttack, int currentLevel, int cost)
        {
            this.unitType = unitType;
            this.isAttack = isAttack;
            this.currentLevel = currentLevel;
            this.nextLevel = currentLevel + 1;
            this.cost = cost;

            var typeName = UnitTypeNames[unitType];
            var upgradeType = isAttack ? "공격" : "방어";
            upgradeName = $"{typeName} {upgradeType} {nextLevel}";
            upgradeCost = cost.ToString();
        }
    }

    private List<UpgradeOption> _currentChoices;


    private void Start()
    {
        ShowRandomChoices();
    }

    private void ShowRandomChoices()
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

        // 4) UI 갱신
        foreach (Transform t in optionContainer)
            Destroy(t.gameObject);

        foreach (var opt in _currentChoices)
        {
            var go = Instantiate(optionButtonPrefab, optionContainer);
            var btn = go.GetComponent<Button>();

            var nameTxt = go.transform.Find("UpgradeName")?.GetComponent<TextMeshProUGUI>();
            var costTxt = go.transform.Find("UpgradeCost")?.GetComponent<TextMeshProUGUI>();
            nameTxt.text = opt.upgradeName;
            costTxt.text = opt.upgradeCost;

            // 클릭 리스너
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnOptionClicked(opt));
        }
    }

    private void OnOptionClicked(UpgradeOption opt)
    {
        int cost = opt.cost;
        int currentGold = RogueLikeData.Instance.GetCurrentGold();
        if (currentGold < cost)
        {
            // 골드 부족 시 아무 동작 없이 즉시 리턴
            Debug.Log("골드가 부족합니다.");
            return;
        }

        // 2) 강화 수행 (isPurchase=false 로 내부 중복 차감 방지)
        RogueLikeData.Instance.IncreaseUpgrade(opt.unitType, opt.isAttack, true);

        // 3) UI 갱신
        ShowRandomChoices();
        UIManager.Instance.UIUpdateAll();
    }
}
