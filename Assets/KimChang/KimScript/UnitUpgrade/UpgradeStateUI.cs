using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeStateUI : MonoBehaviour
{
    private const int MaxUpgradeLevel = 5;

    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI spearAttackText;
    [SerializeField] private TextMeshProUGUI spearDeffenseText;
    [SerializeField] private TextMeshProUGUI swordAttackText;
    [SerializeField] private TextMeshProUGUI swaorDeffenseText;
    [SerializeField] private TextMeshProUGUI bowAttackText;
    [SerializeField] private TextMeshProUGUI bowDeffenseText;
    [SerializeField] private TextMeshProUGUI heavyAttackText;
    [SerializeField] private TextMeshProUGUI heavyDeffenseText;
    [SerializeField] private TextMeshProUGUI assasinAttackText;
    [SerializeField] private TextMeshProUGUI assasinDeffenseText;
    [SerializeField] private TextMeshProUGUI lightAttackText;
    [SerializeField] private TextMeshProUGUI lightDeffenseText;
    [SerializeField] private TextMeshProUGUI heavyCAttackText;
    [SerializeField] private TextMeshProUGUI heavyCDeffenseText;
    [SerializeField] private TextMeshProUGUI supAttackText;
    [SerializeField] private TextMeshProUGUI supDeffenseText;

    [Header("게이지 스프라이트")]
    [SerializeField] private Sprite emptyGaugeSprite;
    [SerializeField] private Sprite attackGaugeSprite;
    [SerializeField] private Sprite defenseGaugeSprite;
    [SerializeField] private Sprite maxAttackLevelSprite;
    [SerializeField] private Sprite maxDefenseLevelSprite;
    [SerializeField] private string maxIconObjectName = "MaxIcon";

    [Header("Spear 게이지 슬롯(각 5칸)")]
    [SerializeField] private Image[] spearAttackGaugeSlots;
    [SerializeField] private Image[] spearDefenseGaugeSlots;
    [Header("Sword 게이지 슬롯(각 5칸)")]
    [SerializeField] private Image[] swordAttackGaugeSlots;
    [SerializeField] private Image[] swordDefenseGaugeSlots;
    [Header("Bow 게이지 슬롯(각 5칸)")]
    [SerializeField] private Image[] bowAttackGaugeSlots;
    [SerializeField] private Image[] bowDefenseGaugeSlots;
    [Header("HeavyInfantry 게이지 슬롯(각 5칸)")]
    [SerializeField] private Image[] heavyAttackGaugeSlots;
    [SerializeField] private Image[] heavyDefenseGaugeSlots;
    [Header("Assassin 게이지 슬롯(각 5칸)")]
    [SerializeField] private Image[] assasinAttackGaugeSlots;
    [SerializeField] private Image[] assasinDefenseGaugeSlots;
    [Header("LightCavalry 게이지 슬롯(각 5칸)")]
    [SerializeField] private Image[] lightAttackGaugeSlots;
    [SerializeField] private Image[] lightDefenseGaugeSlots;
    [Header("HeavyCavalry 게이지 슬롯(각 5칸)")]
    [SerializeField] private Image[] heavyCAttackGaugeSlots;
    [SerializeField] private Image[] heavyCDefenseGaugeSlots;
    [Header("Support 게이지 슬롯(각 5칸)")]
    [SerializeField] private Image[] supAttackGaugeSlots;
    [SerializeField] private Image[] supDefenseGaugeSlots;

    [SerializeField] private Button xBtn;

    [Header("좌측 병종 아이콘 툴팁 대상")]
    [SerializeField] private Graphic spearIcon;
    [SerializeField] private Graphic swordIcon;
    [SerializeField] private Graphic bowIcon;
    [SerializeField] private Graphic heavyIcon;
    [SerializeField] private Graphic assasinIcon;
    [SerializeField] private Graphic lightIcon;
    [SerializeField] private Graphic heavyCIcon;
    [SerializeField] private Graphic supIcon;

    private void Awake()
    {
        if (xBtn != null)
            xBtn.onClick.AddListener(() => gameObject.SetActive(false));

        BindUpgradeBranchSummaryInfos();
    }

    private void OnEnable()
    {
        RefreshFromData();
    }


    // 사용처: 전술 개량 수치 텍스트를 RogueLikeData와 동기화
    public void RefreshFromData()
    {
        UnitUpgrade[] upgrades = RogueLikeData.Instance.GetUpgradeValue();
        if (upgrades == null || upgrades.Length < 8)
            return;

        SetLevelUI(spearAttackText, spearAttackGaugeSlots, upgrades[0].attackLevel, true);
        SetLevelUI(spearDeffenseText, spearDefenseGaugeSlots, upgrades[0].defenseLevel, false);

        SetLevelUI(swordAttackText, swordAttackGaugeSlots, upgrades[1].attackLevel, true);
        SetLevelUI(swaorDeffenseText, swordDefenseGaugeSlots, upgrades[1].defenseLevel, false);

        SetLevelUI(bowAttackText, bowAttackGaugeSlots, upgrades[2].attackLevel, true);
        SetLevelUI(bowDeffenseText, bowDefenseGaugeSlots, upgrades[2].defenseLevel, false);

        SetLevelUI(heavyAttackText, heavyAttackGaugeSlots, upgrades[3].attackLevel, true);
        SetLevelUI(heavyDeffenseText, heavyDefenseGaugeSlots, upgrades[3].defenseLevel, false);

        SetLevelUI(assasinAttackText, assasinAttackGaugeSlots, upgrades[4].attackLevel, true);
        SetLevelUI(assasinDeffenseText, assasinDefenseGaugeSlots, upgrades[4].defenseLevel, false);

        SetLevelUI(lightAttackText, lightAttackGaugeSlots, upgrades[5].attackLevel, true);
        SetLevelUI(lightDeffenseText, lightDefenseGaugeSlots, upgrades[5].defenseLevel, false);

        SetLevelUI(heavyCAttackText, heavyCAttackGaugeSlots, upgrades[6].attackLevel, true);
        SetLevelUI(heavyCDeffenseText, heavyCDefenseGaugeSlots, upgrades[6].defenseLevel, false);

        SetLevelUI(supAttackText, supAttackGaugeSlots, upgrades[7].attackLevel, true);
        SetLevelUI(supDeffenseText, supDefenseGaugeSlots, upgrades[7].defenseLevel, false);
    }

    private void SetLevelUI(TextMeshProUGUI levelText, Image[] gaugeSlots, int level, bool isAttack)
    {
        if (levelText != null)
            ApplyLevelTextOrMaxSprite(levelText, level, isAttack);

        ApplyGauge(gaugeSlots, level, isAttack);
    }

    private void ApplyLevelTextOrMaxSprite(TextMeshProUGUI levelText, int level, bool isAttack)
    {
        int clampedLevel = Mathf.Clamp(level, 0, MaxUpgradeLevel);
        bool isMaxLevel = clampedLevel >= MaxUpgradeLevel;
        Sprite maxSprite = isAttack ? maxAttackLevelSprite : maxDefenseLevelSprite;

        Image maxIcon = FindMaxIcon(levelText);
        bool canShowMaxIcon = isMaxLevel && maxIcon != null && maxSprite != null;

        levelText.gameObject.SetActive(!canShowMaxIcon);
        if (!canShowMaxIcon)
        {
            levelText.text = FormatLevelText(clampedLevel);
        }

        if (maxIcon != null)
        {
            maxIcon.gameObject.SetActive(canShowMaxIcon);
            if (canShowMaxIcon)
                maxIcon.sprite = maxSprite;
        }
    }

    private Image FindMaxIcon(TextMeshProUGUI levelText)
    {
        if (levelText == null || string.IsNullOrEmpty(maxIconObjectName))
            return null;

        Transform t = levelText.transform;
        Transform icon = t.parent != null ? t.parent.Find(maxIconObjectName) : null;
        if (icon == null)
            icon = t.Find(maxIconObjectName);

        return icon != null ? icon.GetComponent<Image>() : null;
    }

    private void ApplyGauge(Image[] slots, int level, bool isAttack)
    {
        if (slots == null || slots.Length == 0)
            return;

        int clampedLevel = Mathf.Clamp(level, 0, MaxUpgradeLevel);
        Sprite fillSprite = isAttack ? attackGaugeSprite : defenseGaugeSprite;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
                continue;

            slots[i].sprite = (i < clampedLevel) ? fillSprite : emptyGaugeSprite;
        }
    }

    // 사용처: 전술 개량 설명을 텍스트, MAX 아이콘, 게이지 칸에 동일하게 연결
    private void BindUpgradeInfo(TextMeshProUGUI levelText, Image[] gaugeSlots, int branchIdx, bool isAttack)
    {
        if (levelText != null)
        {
            EnsureUpgradeInfo(levelText.gameObject, branchIdx, isAttack);

            Image maxIcon = FindMaxIcon(levelText);
            if (maxIcon != null)
                EnsureUpgradeInfo(maxIcon.gameObject, branchIdx, isAttack);
        }

        if (gaugeSlots == null)
            return;

        for (int i = 0; i < gaugeSlots.Length; i++)
        {
            if (gaugeSlots[i] == null)
                continue;

            EnsureUpgradeInfo(gaugeSlots[i].gameObject, branchIdx, isAttack);
        }
    }

    // 사용처: 툴팁이 필요한 UI 오브젝트에 설명 데이터와 마우스 이벤트 컴포넌트를 보장
    private static void EnsureUpgradeInfo(GameObject target, int branchIdx, bool isAttack)
    {
        if (target == null)
            return;

        ItemInformation itemInformation = target.GetComponent<ItemInformation>();
        if (itemInformation == null)
            itemInformation = target.AddComponent<ItemInformation>();

        itemInformation.SetUpgrade(branchIdx, isAttack);

        if (target.GetComponent<ExplainItem>() == null)
            target.AddComponent<ExplainItem>();

        Graphic graphic = target.GetComponent<Graphic>();
        if (graphic != null)
            graphic.raycastTarget = true;
    }

    private static string FormatLevelText(int level)
    {
        return $"{Mathf.Clamp(level, 0, MaxUpgradeLevel)}/{MaxUpgradeLevel}";
    }

    // 사용처: 좌측 병종 아이콘에 병종별 전술 개량 상태 툴팁을 연결
    private void BindUpgradeBranchSummaryInfos()
    {
        BindUpgradeBranchSummaryInfo(spearIcon, 0);
        BindUpgradeBranchSummaryInfo(swordIcon, 1);
        BindUpgradeBranchSummaryInfo(bowIcon, 2);
        BindUpgradeBranchSummaryInfo(heavyIcon, 3);
        BindUpgradeBranchSummaryInfo(assasinIcon, 4);
        BindUpgradeBranchSummaryInfo(lightIcon, 5);
        BindUpgradeBranchSummaryInfo(heavyCIcon, 6);
        BindUpgradeBranchSummaryInfo(supIcon, 7);
    }

    // 사용처: 병종 아이콘 오브젝트에 ItemInformation, ExplainItem, RaycastTarget을 보장
    private static void BindUpgradeBranchSummaryInfo(Graphic targetGraphic, int branchIdx)
    {
        if (targetGraphic == null)
            return;

        GameObject target = targetGraphic.gameObject;

        ItemInformation itemInformation = target.GetComponent<ItemInformation>();
        if (itemInformation == null)
            itemInformation = target.AddComponent<ItemInformation>();

        itemInformation.SetUpgradeBranchSummary(branchIdx);

        if (target.GetComponent<ExplainItem>() == null)
            target.AddComponent<ExplainItem>();

        targetGraphic.raycastTarget = true;
    }

}
