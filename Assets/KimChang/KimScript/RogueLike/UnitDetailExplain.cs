using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitDetailExplain : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI tagText;
    [SerializeField] private TextMeshProUGUI branchText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI unitExplain;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI armorText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI mobilityText;
    [SerializeField] private TextMeshProUGUI ranageText;
    //[SerializeField] private TextMeshProUGUI anitText;
    [SerializeField] private TextMeshProUGUI maxEnergyText;
    [SerializeField] private Image unitFrame;

    [SerializeField] private Image unitImg;

    [SerializeField] private Transform traitBox;
    [SerializeField] private Transform skillBox;
    [SerializeField] private Button xBtn;

    [SerializeField] private ObjectPool objectPool;

    [SerializeField] private GameObject itemToolTip;

    public RogueUnitDataBase unit;
    private RogueUnitDataBase cacheData;

    private const int UnitTagLabelTextId = 32;
    private const int UnitRarityLabelTextId = 33;
    private const int UnitBranchLabelTextId = 34;

    private const int EnergyTooltipTextId = 176;

    private static readonly HashSet<int> TraitAbilityIds = new()
    {
        105, 106, 107, 108, 109, 110, 111, 112, 113,
        139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149
    };

    private void Awake()
    {
        xBtn.onClick.AddListener(ClickXBtn);
    }

    private void OnEnable()
    {
        foreach (var unit in objectPool.GetActiveAbilitys())
            objectPool.ReturnAbility(unit);

        if (cacheData == unit) return;

        if (unitFrame == null)
        {
            unitFrame = transform.GetChild(1).GetChild(6).transform.Find("UnitFrame").GetComponent<Image>();
        }

        int unitTitleKey = GameTextDB.GetIdxByForeignKey(TextKind.Unit, unit.idx);

        nameText.text = GameTextDB.GetByForeignKey(TextKind.Unit, unit.idx);
        tagText.text = $"{GameTextDB.Get(UnitTagLabelTextId)}: {GetTagName(unit.tagIdx)}";
        branchText.text = $"{GameTextDB.Get(UnitBranchLabelTextId)}: {GetBranchName(unit.branchIdx)}";
        rarityText.text = $"{GameTextDB.Get(UnitRarityLabelTextId)}: {GetRarityName(unit.rarity)}";
        energyText.text = $"{GameTextDB.GetById("UI_NAME_CURRENT_STAMINA", "현재 기력")}: {unit.Energy}";
        unitExplain.text = GameTextDB.Get(TextKind.Unit, unitTitleKey, unit.idx);

        healthText.text = UnitStateChange.GetUnitStatusDetail(unit, 100).ToString();
        armorText.text = UnitStateChange.GetUnitStatusDetail(unit, 101).ToString();
        attackText.text = UnitStateChange.GetUnitStatusDetail(unit, 102).ToString();
        mobilityText.text = UnitStateChange.GetUnitStatusDetail(unit, -1).ToString();
        ranageText.text = UnitStateChange.GetUnitStatusDetail(unit, 103).ToString();
        //anitText.text = $"대기병: {unit.antiCavalry}";
        maxEnergyText.text = $"{GameTextDB.GetById("UI_NAME_STAMINA", "기력")}: {unit.MaxEnergy}";

        unitImg.sprite = SpriteCacheManager.GetSprite($"UnitImages/Unit_Img_{unit.idx}");
        unitFrame.sprite = SpriteCacheManager.GetFrameByRarity(unit.rarity);

        BindTextTooltipById(healthText, "STAT_HEALTH_TOOLTIP");
        BindTextTooltipById(armorText, "STAT_ARMOR_TOOLTIP");
        BindTextTooltipById(attackText, "STAT_ATTACK_DAMAGE_TOOLTIP");
        BindTextTooltipById(mobilityText, "STAT_MOBILITY_TOOLTIP");
        BindTextTooltipById(ranageText, "STAT_RANGE_TOOLTIP");
        BindTextTooltip(energyText, EnergyTooltipTextId);
        BindTextTooltip(maxEnergyText, EnergyTooltipTextId);

        BindForeignTooltip(tagText, TextKind.Tag, unit.tagIdx);
        BindForeignTooltip(branchText, TextKind.Branch, unit.branchIdx);

        float frameSize = unit.rarity == 4 ? 200 * 1.185f : 200 * 1.17f;
        RectTransform frameRect = unitFrame.rectTransform;
        frameRect.sizeDelta = new Vector2(frameSize, frameSize);

        var boolAttributes = unit.GetType().GetFields()
            .Where(f => f.FieldType == typeof(bool) && (bool)f.GetValue(unit))
            .Select(f => new { Name = f.Name });

        foreach (var attr in boolAttributes)
        {
            if (attr.Name == "alive" || attr.Name == "fStriked")
                continue;

            GameObject ability = objectPool.GetAbility();
            ItemInformation itemInfo = ability.GetComponent<ItemInformation>();
            ExplainItem explainItem = ability.GetComponent<ExplainItem>();

            Sprite sp = SpriteCacheManager.GetSprite($"KIcon/AbilityIcon/{attr.Name}");
            if (sp == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[OnEnable] 아이콘 스프라이트 없음: {attr.Name}");
#endif
                objectPool.ReturnAbility(ability);
                continue;
            }

            Image img = ability.GetComponent<Image>();
            img.sprite = sp;

            itemInfo.data.isItem = false;

            int? idx = AbilityIdMap.GetIdx(attr.Name);
            itemInfo.data.abilityId = idx ?? -1;

            explainItem.ItemToolTip = itemToolTip;

            int abilityId = idx ?? -1;

            itemInfo.SetAbility(abilityId);
            explainItem.ItemToolTip = itemToolTip;

            Transform abilityBox = IsTraitAbility(abilityId) ? traitBox : skillBox;
            ability.transform.SetParent(abilityBox, false);
        }

        cacheData = unit;
        transform.SetAsLastSibling();
    }

    private void ClickXBtn()
    {
        gameObject.SetActive(false);
    }

    // 사용처: 유닛 상세 UI에서 태그 이름을 GameTextDB 기준으로 가져옴
    private static string GetTagName(int tagIdx)
    {
        string text = GameTextDB.GetByForeignKey(TextKind.Tag, tagIdx);
        if (!string.IsNullOrEmpty(text)) return text;

        if (tagIdx == 0)
            return GameTextDB.GetById("UI_NONE", "없음");

        return string.Empty;
    }

    // 사용처: 유닛 상세 UI에서 병종 이름을 GameTextDB 기준으로 가져옴
    private static string GetBranchName(int branchIdx)
    {
        string text = GameTextDB.GetByForeignKey(TextKind.Branch, branchIdx);
        return string.IsNullOrEmpty(text) ? string.Empty : text;
    }

    // 사용처: 유닛 상세 UI에서 희귀도 이름을 GameTextDB 기준으로 가져옴
    private static string GetRarityName(int rarity)
    {
        string text = GameTextDB.GetByForeignKey(TextKind.UnitRarity, rarity);
        return string.IsNullOrEmpty(text) ? rarity.ToString() : text;
    }

    // 사용처: 유닛 상세 텍스트에 실제 설명이 있을 때만 마우스오버 툴팁을 연결
    private void BindTextTooltip(TextMeshProUGUI target, int gameTextId)
    {
        if (target == null || gameTextId < 0 || string.IsNullOrWhiteSpace(GameTextDB.Get(gameTextId)))
        {
            UnbindTextTooltip(target);
            return;
        }

        target.raycastTarget = true;

        ItemInformation itemInfo = target.GetComponent<ItemInformation>();
        if (itemInfo == null)
            itemInfo = target.gameObject.AddComponent<ItemInformation>();

        itemInfo.SetGameText(gameTextId);

        ExplainItem explainItem = target.GetComponent<ExplainItem>();
        if (explainItem == null)
            explainItem = target.gameObject.AddComponent<ExplainItem>();

        explainItem.enabled = true;
        explainItem.ItemToolTip = itemToolTip;
    }

    // 사용처: 설명이 없는 유닛 상세 텍스트에서 이전 마우스오버 데이터를 제거
    private static void UnbindTextTooltip(TextMeshProUGUI target)
    {
        if (target == null) return;

        ItemInformation itemInfo = target.GetComponent<ItemInformation>();
        if (itemInfo != null)
            itemInfo.Clear();

        ExplainItem explainItem = target.GetComponent<ExplainItem>();
        if (explainItem != null)
            explainItem.enabled = false;

        target.raycastTarget = false;
    }

    // 사용처: 유닛 상세 스탯 툴팁을 엑셀 ID 기준으로 연결하고, 없으면 마우스오버를 제거
    private void BindTextTooltipById(TextMeshProUGUI target, string gameTextId)
    {
        int idx = GameTextDB.GetIdxById(gameTextId, -1);
        BindTextTooltip(target, idx);
    }

    // 사용처: 병종/태그 텍스트에 설명이 있을 때만 마우스오버 툴팁을 연결
    private void BindForeignTooltip(TextMeshProUGUI target, TextKind kind, int foreignKey)
    {
        int tooltipIdx = GameTextDB.GetTooltipIdxByForeignKey(kind, foreignKey);
        BindTextTooltip(target, tooltipIdx);
    }

    // 사용처: 특성/기술 ID가 특성 영역에 들어갈 대상인지 판정
    private static bool IsTraitAbility(int abilityId)
    {
        return TraitAbilityIds.Contains(abilityId);
    }
}
