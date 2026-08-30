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

    // 사용처: 유닛 상세창에서 생성한 특성/기술 아이콘만 별도로 추적
    private readonly List<GameObject> detailAbilityIcons = new();

    private const int UnitTagLabelTextId = 32;
    private const int UnitRarityLabelTextId = 33;
    private const int UnitBranchLabelTextId = 34;

    private const int EnergyTooltipTextId = 176;

    // 사용처: 유닛 상세창의 특성/기술 아이콘을 기본 크기의 2/3로 표시
    private const float DetailAbilityIconScale = 2f / 3f;

    private static readonly HashSet<int> TraitAbilityIds = new()
    {
        105, 106, 107, 108, 109, 110, 111, 112, 113,
        139, 140, 141, 142, 143, 144, 145, 146, 147, 148, 149
    };

    private void Awake()
    {
        if (xBtn != null)
            xBtn.onClick.AddListener(ClickXBtn);
    }

    private void OnEnable()
    {
        GameTextDB.LanguageChanged += RefreshLocalizedText;
        RefreshView();
    }

    private void OnDisable()
    {
        GameTextDB.LanguageChanged -= RefreshLocalizedText;

        // 사용처: 상세창을 닫을 때 이 상세창에서 생성한 아이콘만 풀에 반환
        ClearDetailAbilityIcons();

        // 다시 열었을 때 같은 유닛이라도 화면을 정상적으로 다시 구성
        cacheData = null;
    }

    private void RefreshLocalizedText()
    {
        cacheData = null;
        RefreshView();
    }

    private void RefreshView()
    {
        if (unit == null || objectPool == null)
            return;

        if (cacheData == unit)
        {
            transform.SetAsLastSibling();
            return;
        }

        // 사용처: 다른 유닛으로 상세 정보가 변경될 때 이전 상세창 아이콘만 정리
        ClearDetailAbilityIcons();

        if (unitFrame == null)
        {
            Transform unitFrameTransform =
                transform.GetChild(1)
                    .GetChild(6)
                    .transform
                    .Find("UnitFrame");

            if (unitFrameTransform != null)
                unitFrame = unitFrameTransform.GetComponent<Image>();
        }

        int unitTitleKey =
            GameTextDB.GetIdxByForeignKey(TextKind.Unit, unit.idx);

        nameText.text =
            GameTextDB.GetByForeignKey(TextKind.Unit, unit.idx);

        tagText.text =
            $"{GameTextDB.Get(UnitTagLabelTextId)}: {GetTagName(unit.tagIdx)}";

        branchText.text =
            $"{GameTextDB.Get(UnitBranchLabelTextId)}: {GetBranchName(unit.branchIdx)}";

        rarityText.text =
            $"{GameTextDB.Get(UnitRarityLabelTextId)}: {GetRarityName(unit.rarity)}";

        energyText.text =
            $"{GameTextDB.GetById("UI_NAME_CURRENT_STAMINA", "현재 기력")}: {unit.Energy}";

        unitExplain.text =
            GameTextDB.Get(TextKind.Unit, unitTitleKey, unit.idx);

        healthText.text =
            UnitStateChange.GetUnitStatusDetail(unit, 100).ToString();

        armorText.text =
            UnitStateChange.GetUnitStatusDetail(unit, 101).ToString();

        attackText.text =
            UnitStateChange.GetUnitStatusDetail(unit, 102).ToString();

        mobilityText.text =
            UnitStateChange.GetUnitStatusDetail(unit, -1).ToString();

        ranageText.text =
            UnitStateChange.GetUnitStatusDetail(unit, 103).ToString();

        //anitText.text = $"대기병: {unit.antiCavalry}";

        maxEnergyText.text =
            $"{GameTextDB.GetById("UI_NAME_STAMINA", "기력")}: {unit.MaxEnergy}";

        if (unitImg != null)
        {
            unitImg.sprite =
                SpriteCacheManager.GetSprite(
                    $"UnitImages/Unit_Img_{unit.idx}");
        }

        if (unitFrame != null)
        {
            unitFrame.sprite =
                SpriteCacheManager.GetFrameByRarity(unit.rarity);
        }

        BindTextTooltipById(
            healthText,
            "STAT_HEALTH_TOOLTIP");

        BindTextTooltipById(
            armorText,
            "STAT_ARMOR_TOOLTIP");

        BindTextTooltipById(
            attackText,
            "STAT_ATTACK_DAMAGE_TOOLTIP");

        BindTextTooltipById(
            mobilityText,
            "STAT_MOBILITY_TOOLTIP");

        BindTextTooltipById(
            ranageText,
            "STAT_RANGE_TOOLTIP");

        BindTextTooltip(
            energyText,
            EnergyTooltipTextId);

        BindTextTooltip(
            maxEnergyText,
            EnergyTooltipTextId);

        BindForeignTooltip(
            tagText,
            TextKind.Tag,
            unit.tagIdx);

        BindForeignTooltip(
            branchText,
            TextKind.Branch,
            unit.branchIdx);

        ResizeUnitFrame();

        var boolAttributes = unit.GetType()
            .GetFields()
            .Where(f =>
                f.FieldType == typeof(bool) &&
                (bool)f.GetValue(unit))
            .Select(f => new
            {
                Name = f.Name
            });

        foreach (var attr in boolAttributes)
        {
            if (attr.Name == "alive" ||
                attr.Name == "fStriked")
            {
                continue;
            }

            Sprite sp =
                SpriteCacheManager.GetSprite(
                    $"KIcon/AbilityIcon/{attr.Name}");

            if (sp == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning(
                    $"[UnitDetailExplain] 아이콘 스프라이트 없음: {attr.Name}");
#endif
                continue;
            }

            GameObject ability =
                objectPool.GetAbility();

            if (ability == null)
                continue;

            Image img =
                ability.GetComponent<Image>();

            ItemInformation itemInfo =
                ability.GetComponent<ItemInformation>();

            ExplainItem explainItem =
                ability.GetComponent<ExplainItem>();

            if (img == null ||
                itemInfo == null ||
                explainItem == null)
            {
                objectPool.ReturnAbility(ability);
                continue;
            }

            img.sprite = sp;

            itemInfo.data.isItem = false;

            int? idx =
                AbilityIdMap.GetIdx(attr.Name);

            int abilityId =
                idx ?? -1;

            itemInfo.data.abilityId =
                abilityId;

            itemInfo.SetAbility(
                abilityId);

            explainItem.ItemToolTip =
                itemToolTip;

            Transform abilityBox =
                IsTraitAbility(abilityId)
                    ? traitBox
                    : skillBox;

            if (abilityBox == null)
            {
                objectPool.ReturnAbility(ability);
                continue;
            }

            ability.transform.SetParent(
                abilityBox,
                false);

            // 사용처: 상세창의 특성/기술 아이콘만 기존 크기의 2/3로 축소
            ability.transform.localScale =
                Vector3.one * DetailAbilityIconScale;

            detailAbilityIcons.Add(
                ability);
        }

        cacheData = unit;

        transform.SetAsLastSibling();
    }

    // 사용처: OneUnitUI와 동일하게 실제 유닛 이미지 크기를 기준으로 희귀도 프레임 크기 계산
    private void ResizeUnitFrame()
    {
        if (unitImg == null ||
            unitFrame == null ||
            unit == null)
        {
            return;
        }

        RectTransform imgRt =
            unitImg.rectTransform;

        Vector2 imgSize =
            imgRt.sizeDelta;

        // Stretch Anchor 등으로 sizeDelta가 정상 크기를 주지 않는 경우 실제 Rect 사용
        if (imgSize.x <= 0f ||
            imgSize.y <= 0f)
        {
            imgSize =
                imgRt.rect.size;
        }

        if (imgSize.x <= 0f)
            return;

        float frameScale =
            unit.rarity == 4
                ? 1.185f
                : 1.17f;

        float frameSize =
            imgSize.x * frameScale;

        Vector2 resultSize =
            new Vector2(
                frameSize,
                frameSize);

        RectTransform frameRt =
            unitFrame.rectTransform;

        // 사용처: LayoutGroup이 프레임 크기를 관리하는 경우 preferredSize를 통해 크기 적용
        LayoutElement layout =
            unitFrame.GetComponent<LayoutElement>();

        if (layout != null)
        {
            layout.preferredWidth =
                resultSize.x;

            layout.preferredHeight =
                resultSize.y;
        }
        else
        {
            frameRt.sizeDelta =
                resultSize;
        }
    }

    // 사용처: 유닛 상세창에서 생성한 특성/기술 아이콘만 ObjectPool에 반환
    private void ClearDetailAbilityIcons()
    {
        if (detailAbilityIcons.Count == 0)
            return;

        if (objectPool == null)
        {
            detailAbilityIcons.Clear();
            return;
        }

        for (int i = detailAbilityIcons.Count - 1;
             i >= 0;
             i--)
        {
            GameObject ability =
                detailAbilityIcons[i];

            if (ability == null)
                continue;

            objectPool.ReturnAbility(
                ability);
        }

        detailAbilityIcons.Clear();
    }

    private void ClickXBtn()
    {
        gameObject.SetActive(false);
    }

    // 사용처: 유닛 상세 UI에서 태그 이름을 GameTextDB 기준으로 가져옴
    private static string GetTagName(int tagIdx)
    {
        string text =
            GameTextDB.GetByForeignKey(
                TextKind.Tag,
                tagIdx);

        if (!string.IsNullOrEmpty(text))
            return text;

        if (tagIdx == 0)
        {
            return GameTextDB.GetById(
                "UI_NONE",
                "없음");
        }

        return string.Empty;
    }

    // 사용처: 유닛 상세 UI에서 병종 이름을 GameTextDB 기준으로 가져옴
    private static string GetBranchName(int branchIdx)
    {
        string text =
            GameTextDB.GetByForeignKey(
                TextKind.Branch,
                branchIdx);

        return string.IsNullOrEmpty(text)
            ? string.Empty
            : text;
    }

    // 사용처: 유닛 상세 UI에서 희귀도 이름을 GameTextDB 기준으로 가져옴
    private static string GetRarityName(int rarity)
    {
        string text =
            GameTextDB.GetByForeignKey(
                TextKind.UnitRarity,
                rarity);

        return string.IsNullOrEmpty(text)
            ? rarity.ToString()
            : text;
    }

    // 사용처: 유닛 상세 텍스트에 실제 설명이 있을 때만 마우스오버 툴팁을 연결
    private void BindTextTooltip(
        TextMeshProUGUI target,
        int gameTextId)
    {
        if (target == null ||
            gameTextId < 0 ||
            string.IsNullOrWhiteSpace(
                GameTextDB.Get(gameTextId)))
        {
            UnbindTextTooltip(target);
            return;
        }

        target.raycastTarget = true;

        ItemInformation itemInfo =
            target.GetComponent<ItemInformation>();

        if (itemInfo == null)
        {
            itemInfo =
                target.gameObject
                    .AddComponent<ItemInformation>();
        }

        itemInfo.SetGameText(
            gameTextId);

        ExplainItem explainItem =
            target.GetComponent<ExplainItem>();

        if (explainItem == null)
        {
            explainItem =
                target.gameObject
                    .AddComponent<ExplainItem>();
        }

        explainItem.enabled = true;

        explainItem.ItemToolTip =
            itemToolTip;
    }

    // 사용처: 설명이 없는 유닛 상세 텍스트에서 이전 마우스오버 데이터를 제거
    private static void UnbindTextTooltip(
        TextMeshProUGUI target)
    {
        if (target == null)
            return;

        ItemInformation itemInfo =
            target.GetComponent<ItemInformation>();

        if (itemInfo != null)
            itemInfo.Clear();

        ExplainItem explainItem =
            target.GetComponent<ExplainItem>();

        if (explainItem != null)
            explainItem.enabled = false;

        target.raycastTarget = false;
    }

    // 사용처: 유닛 상세 스탯 툴팁을 엑셀 ID 기준으로 연결하고 없으면 마우스오버를 제거
    private void BindTextTooltipById(
        TextMeshProUGUI target,
        string gameTextId)
    {
        int idx =
            GameTextDB.GetIdxById(
                gameTextId,
                -1);

        BindTextTooltip(
            target,
            idx);
    }

    // 사용처: 병종/태그 텍스트에 설명이 있을 때만 마우스오버 툴팁을 연결
    private void BindForeignTooltip(
        TextMeshProUGUI target,
        TextKind kind,
        int foreignKey)
    {
        int tooltipIdx =
            GameTextDB.GetTooltipIdxByForeignKey(
                kind,
                foreignKey);

        BindTextTooltip(
            target,
            tooltipIdx);
    }

    // 사용처: 특성/기술 ID가 특성 영역에 들어갈 대상인지 판정
    private static bool IsTraitAbility(
        int abilityId)
    {
        return TraitAbilityIds.Contains(
            abilityId);
    }
}