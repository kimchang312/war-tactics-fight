using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.UI;

public class UnitDetailExplain : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI tagText;
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


    private void Awake()
    {
        xBtn.onClick.AddListener(()=>gameObject.SetActive(false));
    }

    private void OnEnable()
    {
        if (cacheData == unit) return;

        nameText.text = unit.unitName;
        tagText.text =$"태그: {unit.tag}";
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
            .Where(f => f.FieldType == typeof(bool))
            .Select(f => new { Name = f.Name, Value = (bool)f.GetValue(unit) });
        foreach (var attr in boolAttributes)
        {
            GameObject ability = objectPool.GetAbility();
            ItemInformation itemInfo = ability.GetComponent<ItemInformation>();
            ExplainItem explainItem = ability.GetComponent<ExplainItem>();

            Image img = ability.GetComponent<Image>();
            img.sprite = SpriteCacheManager.GetSprite($"KIcon/AbilityIcon/{attr.Name}");
            itemInfo.isItem = false;

            // 이름 설정
            itemInfo.isItem = false;
            int? idx = GameTextData.GetIdxFromString(attr.Name);
            if (attr.Name == "defense")
            {
                itemInfo.abilityId = 129;
            }
            else if (idx.HasValue)
            {
                itemInfo.abilityId = idx.Value;
            }
            else if (int.TryParse(attr.Name, out int parsedId))
            {
                itemInfo.abilityId = parsedId;
            }
            else
            {
                Debug.LogWarning($"abilityId 파싱 실패: {attr.Name}");
                itemInfo.abilityId = -1; // 혹은 예외 처리 또는 기본값 지정
            }

            explainItem.ItemToolTip = itemToolTip;
            Transform abilityBox = idx<128 ?  traitBox: skillBox;

            ability.transform.SetParent(abilityBox, false);
        }
    }
    private void OnDisable()
    {
        foreach (GameObject ability in traitBox)
        {
            objectPool.ReturnAbility(ability);
        }
        foreach (GameObject ability in skillBox)
        {
            objectPool.ReturnAbility(ability);
        }
    }

}
