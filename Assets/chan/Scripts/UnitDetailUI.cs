using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitDetailUI : MonoBehaviour
{
    [Header("기본 정보")]
    public GameObject detailPanel;            // 유닛 상세 UI 패널
    public TextMeshProUGUI unitNameText;      // 유닛 이름
    public TextMeshProUGUI unitBranchText;    // 병종 이름
    public TextMeshProUGUI unitFactionText;   // 진영 이름
    public Image unitIMG;                     // 유닛 초상화

    [Header("유닛 스탯")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI armorText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI mobilityText;
    public TextMeshProUGUI rangeText;
    public TextMeshProUGUI antiCavalryText;
    //public TextMeshProUGUI evasionRateText; // 기동력 툴팁으로 표시할 회피율

    [Header("유닛 특성 및 기술")]
    public Transform traitsParent;            // 특성 리스트 부모
    public Transform skillsParent;            // 기술 리스트 부모
    public GameObject traitPrefab;            // 특성 UI 프리팹
    public GameObject skillPrefab;            // 기술 UI 프리팹

    [Header("유닛 소개")]
    public TextMeshProUGUI unitTooltipText;

    

    private void Awake()
    {
        detailPanel.SetActive(false); // 시작 시 비활성화
    }

    // 유닛 정보를 업데이트 및 표시
    public void ShowUnitDetails(UnitDataBase unit)
    {
        if (unit == null) return;

        detailPanel.SetActive(true); // 패널 활성화
        // 기본 정보 업데이트
        unitNameText.text = unit.unitName;
        unitBranchText.text = unit.unitBranch;
        unitFactionText.text = unit.unitFaction;
        unitIMG.sprite = Resources.Load<Sprite>("UnitImages/" + unit.unitImg);

        // 스탯 정보 표시 11.17 수치로 표시
        healthText.text = unit.health.ToString();
        armorText.text = unit.armor.ToString();
        attackText.text = unit.attackDamage.ToString();
        mobilityText.text = unit.mobility.ToString();
        rangeText.text = unit.range.ToString();
        antiCavalryText.text = unit.antiCavalry.ToString();
        //evasionRateText.text = $"회피율: {unit.evasionRate}";

        // 유닛 소개 업데이트 -> 다른 엑셀로 관리하여 추후 추가할 툴팁
        //unitTooltipText.text = unit.unitDescription;

        // 패널 활성화
        detailPanel.SetActive(true);

        // 특성 및 기술 업데이트
        UpdateTraits(unit);
        UpdateSkills(unit);
    }
    private void UpdateTraits(UnitDataBase unit)
    {
        // 기존 특성 제거
        foreach (Transform child in traitsParent)
        {
            Destroy(child.gameObject);
        }

        // 특성 추가
        if (unit.lightArmor) AddTrait("경갑");
        if (unit.heavyArmor) AddTrait("중갑");
        if (unit.rangedAttack) AddTrait("원거리 공격");
        if (unit.bluntWeapon) AddTrait("둔기");
        if (unit.pierce) AddTrait("관통");
        if (unit.agility) AddTrait("날쌤");
        if (unit.strongCharge) AddTrait("강한 돌격");
        if (unit.perfectAccuracy) AddTrait("필중");
        if (unit.slaughter) AddTrait("도살");
    }

    private void UpdateSkills(UnitDataBase unit)
    {
        // 기존 기술 제거
        foreach (Transform child in skillsParent)
        {
            Destroy(child.gameObject);
        }

        // 기술 추가
        if (unit.charge) AddSkill("돌격");
        if (unit.defense) AddSkill("수비 태세");
        if (unit.throwSpear) AddSkill("투창");
        if (unit.guerrilla) AddSkill("유격");
        if (unit.guard) AddSkill("수호");
        if (unit.assassination) AddSkill("암살");
        if (unit.drain) AddSkill("착취");
        if (unit.overwhelm) AddSkill("위압");
    }

    private void AddTrait(string traitName)
    {
        GameObject traitObject = Instantiate(traitPrefab, traitsParent);
        TextMeshProUGUI traitText = traitObject.GetComponentInChildren<TextMeshProUGUI>();
        //traitText.text = traitName;
    }

    private void AddSkill(string skillName)
    {
        GameObject skillObject = Instantiate(skillPrefab, skillsParent);
        TextMeshProUGUI skillText = skillObject.GetComponentInChildren<TextMeshProUGUI>();
        //skillText.text = skillName;
    }


    // 유닛 상세 UI를 비활성화
    public void HideUnitDetails()
    {
        detailPanel.SetActive(false);
    }
}