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
    //[SerializeField] private TextMeshProUGUI unitExplainText;
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
        // 기존 특성/기술 오브젝트 정리
        foreach (var unit in objectPool.GetActiveAbilitys())
        {
            objectPool.ReturnAbility(unit);
        }
        if (cacheData == unit) return;

        nameText.text = unit.unitName;
        tagText.text = $"태그: {unit.tag}";
        branchText.text = $"병종: {branchName[unit.branchIdx]}";
        rarityText.text = $"희귀도: {unit.rarity}";
        energyText.text = $"현재 기력: {unit.energy}";
        healthText.text = $"체력: {unit.maxHealth}";
        armorText.text = $"장갑: {unit.armor}";
        attackText.text = $"공격력: {unit.attackDamage}";
        mobilityText.text = $"기동력: {unit.mobility}";
        ranageText.text = $"사거리: {unit.range}";
        anitText.text = $"대기병: {unit.antiCavalry}";
        maxEnergyText.text = $"기력: {unit.maxEnergy}";
        unitImg.sprite = SpriteCacheManager.GetSprite($"UnitImages/{unit.unitImg}");

        var boolAttributes = unit.GetType().GetFields()
            .Where(f => f.FieldType == typeof(bool) && (bool)f.GetValue(unit))
            .Select(f => new { Name = f.Name });

        foreach (var attr in boolAttributes)
        {
            if (attr.Name == "alive" || attr.Name == "fStriked") continue;

            GameObject ability = objectPool.GetAbility();
            ItemInformation itemInfo = ability.GetComponent<ItemInformation>();
            ExplainItem explainItem = ability.GetComponent<ExplainItem>();

            Image img = ability.GetComponent<Image>();
            img.sprite = SpriteCacheManager.GetSprite($"KIcon/AbilityIcon/{attr.Name}");
            itemInfo.isItem = false;

            int? idx = GameTextData.GetIdxFromString(attr.Name);
            // itemInfo.abilityId = attr.Name == "defense" ? 129 : idx ?? -1;
            // 수비태세 툴팁 돌격으로 뜨는 버그 수정
            itemInfo.abilityId = idx ?? -1;
            explainItem.ItemToolTip = itemToolTip;

            Transform abilityBox = idx < 128 ? traitBox : skillBox;
            ability.transform.SetParent(abilityBox, false);
        }

        transform.SetAsLastSibling();
    }

    private void ClickXBtn()
    {
        gameObject.SetActive(false);
    }

}
