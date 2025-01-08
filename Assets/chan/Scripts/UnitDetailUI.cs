using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UnitDetailUI : MonoBehaviour
{
    private DescriptionManager descriptionManager;
    public Tooltip tooltip; // Tooltip 스크립트 참조

    [Header("기본 정보")]
    public GameObject detailPanel;            // 유닛 상세 UI 패널
    public TextMeshProUGUI unitNameText;      // 유닛 이름
    public TextMeshProUGUI unitBranchText;    // 병종 이름
    public TextMeshProUGUI unitFactionText;   // 진영 이름 (한글 변환)
    public Image unitFactionIcon;             // 진영 아이콘 이미지
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
        descriptionManager = FindObjectOfType<DescriptionManager>();
        if (descriptionManager == null)
        {
            Debug.LogError("DescriptionManager를 찾을 수 없습니다. 씬에 추가했는지 확인하세요.");
        }
        tooltip.HideTooltip(); // 시작 시 툴팁 숨김

    }

    // 유닛 정보를 업데이트 및 표시
    public void ShowUnitDetails(UnitDataBase unit)
    {
        if (unit == null) return;

        detailPanel.SetActive(true); // 패널 활성화
        // 기본 정보 업데이트
        unitNameText.text = unit.unitName;
        unitBranchText.text = GetBranchName(unit.unitBranch);
        //unitFactionText.text = unit.unitFaction;
        unitFactionText.text = GetFactionName(unit.unitFaction); // 한글 변환 적용
        unitIMG.sprite = Resources.Load<Sprite>("UnitImages/" + unit.unitImg);

        // 진영 아이콘 적용
        unitFactionIcon.sprite = Resources.Load<Sprite>("FactionIcons/" + unit.unitFaction);

        // 스탯 정보 표시 11.17 수치로 표시
        healthText.text = unit.health.ToString();
        armorText.text = unit.armor.ToString();
        attackText.text = unit.attackDamage.ToString();
        mobilityText.text = unit.mobility.ToString();
        rangeText.text = unit.range.ToString();
        antiCavalryText.text = unit.antiCavalry.ToString();
        //evasionRateText.text = $"회피율: {unit.evasionRate}";

        // 유닛 소개 업데이트 -> 다른 엑셀로 관리하여 추후 추가할 툴팁
        unitTooltipText.text = descriptionManager.GetUnitDescription(unit.unitName);

        // 패널 활성화
        detailPanel.SetActive(true);

        // 특성 및 기술 업데이트
        UpdateTraits(unit);
        UpdateSkills(unit);
    }
    // 영어 진영명을 한글로 변환하는 메서드
    private string GetFactionName(string faction)
    {
        switch (faction)
        {
            case "empire": return "제국";
            case "heptarchy": return "칠왕연합";
            case "divinitas": return "신성국";
            default: return "공용";
        }
    }
    private string GetBranchName(string branch)
    {
        switch (branch)
        {
            case "Branch_Spearman": return "창병";
            case "Branch_Warrior": return "전사";
            case "Branch_Bowman": return "궁병";
            case "Branch_Heavy_Infantry": return "중병";
            case "Branch_Assassin": return "암살자";
            case "Branch_Light_Calvary": return "경기병";
            case "Branch_Heavy_Calvary": return "중기병";
            default: return "알 수 없음";
        }
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
        // 툴팁 적용
        string traitDescription = descriptionManager.GetTraitDescription(traitName);
        Debug.Log($"Trait: {traitName}, Description: {traitDescription}"); // 디버깅

        // 마우스 이벤트 추가
        EventTrigger trigger = traitObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entryEnter.callback.AddListener((data) =>
        {
            if (tooltip != null)
            {
                tooltip.SetTooltip(traitDescription);
                tooltip.ShowTooltip(Input.mousePosition);
            }
        });

        EventTrigger.Entry entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entryExit.callback.AddListener((data) =>
        {
            if (tooltip != null)
            {
                tooltip.HideTooltip();
            }
        });

        trigger.triggers.Add(entryEnter);
        trigger.triggers.Add(entryExit);
    }

    private void AddSkill(string skillName)
    {
        GameObject skillObject = Instantiate(skillPrefab, skillsParent);
        TextMeshProUGUI skillText = skillObject.GetComponentInChildren<TextMeshProUGUI>();
        //skillText.text = skillName;
        // 툴팁 적용
        string skillDescription = descriptionManager.GetSkillDescription(skillName);
        EventTrigger trigger = skillObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entryEnter.callback.AddListener((data) =>
        {
            if (tooltip != null)
            {
                tooltip.SetTooltip(skillDescription);
                tooltip.ShowTooltip(Input.mousePosition);
            }
        });

        EventTrigger.Entry entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entryExit.callback.AddListener((data) =>
        {
            if (tooltip != null)
            {
                tooltip.HideTooltip();
            }
        });

        trigger.triggers.Add(entryEnter);
        trigger.triggers.Add(entryExit);
    }


    // 유닛 상세 UI를 비활성화
    public void HideUnitDetails()
    {
        detailPanel.SetActive(false);
    }
}