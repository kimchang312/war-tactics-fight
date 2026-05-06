using System.Collections;
using System.Collections.Generic;
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

    private void Awake()
    {
        xBtn.onClick.AddListener(()=>gameObject.SetActive(false));  
    }

    private void OnEnable()
    {
        RefreshFromData();
    }

    /// <summary>전술 개량 수치 텍스트를 RogueLikeData와 동기화합니다.</summary>
    public void RefreshFromData()
    {
        UnitUpgrade[] upgrades = RogueLikeData.Instance.GetUpgradeValue();

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

        // MAX 스프라이트를 쓸 수 있으면 숫자 텍스트를 숨기고 아이콘으로 대체
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

    private static string FormatLevelText(int level)
    {
        return $"{Mathf.Clamp(level, 0, MaxUpgradeLevel)}/{MaxUpgradeLevel}";
    }


}
