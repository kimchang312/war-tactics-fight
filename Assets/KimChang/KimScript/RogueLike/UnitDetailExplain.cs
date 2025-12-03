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
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI armorText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private TextMeshProUGUI mobilityText;
    [SerializeField] private TextMeshProUGUI ranageText;
    [SerializeField] private TextMeshProUGUI anitText;
    [SerializeField] private TextMeshProUGUI maxEnergyText;

    [SerializeField] private Image unitImg;
    
    [SerializeField] private Transform traitBox;
    [SerializeField] private Transform skillBox;
    [SerializeField] private Button xBtn;

    [SerializeField] private ObjectPool objectPool;

    [SerializeField] private GameObject itemToolTip;

    public RogueUnitDataBase unit;
    private RogueUnitDataBase cacheData;

    private readonly Dictionary<int, string> branchName = new Dictionary<int, string>()
    {
        {0,"창병" },
        {1,"전사" },
        {2,"궁병" },
        {3,"중보병" },
        {4,"암살자" },
        {5,"경기병" },
        {6,"중기병" },
        {7,"지원" },
        {8,"영웅" }
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

        nameText.text = unit.unitName;
        tagText.text = $"{GameTextDB.Get(32)}: {GameTextDB.GetByForeignKey(TextKind.Tag,unit.tagIdx)}";
        branchText.text = $"{GameTextDB.Get(34)}: {branchName[unit.branchIdx]}";
        rarityText.text = $"{GameTextDB.Get(33)}: {unit.rarity}";
        energyText.text = $"현재 기력: {unit.Energy}";
        healthText.text = UnitStateChange.GetUnitStatusDetail(unit, 100).ToString();
        armorText.text = UnitStateChange.GetUnitStatusDetail(unit, 101).ToString();
        attackText.text = UnitStateChange.GetUnitStatusDetail(unit, 102).ToString();
        mobilityText.text = UnitStateChange.GetUnitStatusDetail(unit, -1).ToString();
        ranageText.text = UnitStateChange.GetUnitStatusDetail(unit, 103).ToString();
        //anitText.text = $"대기병: {unit.antiCavalry}";
        maxEnergyText.text = $"기력: {unit.MaxEnergy}";
        unitImg.sprite = SpriteCacheManager.GetSprite($"UnitImages/Unit_Img_{unit.idx}");

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

            Transform abilityBox = (idx.HasValue && idx.Value < 128) ? traitBox : skillBox;
            ability.transform.SetParent(abilityBox, false);
        }

        transform.SetAsLastSibling();
    }

    private void ClickXBtn()
    {
        gameObject.SetActive(false);
    }

}
