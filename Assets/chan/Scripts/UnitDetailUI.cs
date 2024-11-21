using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitDetailUI : MonoBehaviour
{
    [Header("�⺻ ����")]
    public GameObject detailPanel;            // ���� �� UI �г�
    public TextMeshProUGUI unitNameText;      // ���� �̸�
    public TextMeshProUGUI unitBranchText;    // ���� �̸�
    public TextMeshProUGUI unitFactionText;   // ���� �̸�
    public Image unitIMG;                     // ���� �ʻ�ȭ

    [Header("���� ����")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI armorText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI mobilityText;
    public TextMeshProUGUI rangeText;
    public TextMeshProUGUI antiCavalryText;
    //public TextMeshProUGUI evasionRateText; // �⵿�� �������� ǥ���� ȸ����

    [Header("���� Ư�� �� ���")]
    public Transform traitsParent;            // Ư�� ����Ʈ �θ�
    public Transform skillsParent;            // ��� ����Ʈ �θ�
    public GameObject traitPrefab;            // Ư�� UI ������
    public GameObject skillPrefab;            // ��� UI ������

    [Header("���� �Ұ�")]
    public TextMeshProUGUI unitTooltipText;

    

    private void Awake()
    {
        detailPanel.SetActive(false); // ���� �� ��Ȱ��ȭ
    }

    // ���� ������ ������Ʈ �� ǥ��
    public void ShowUnitDetails(UnitDataBase unit)
    {
        if (unit == null) return;

        detailPanel.SetActive(true); // �г� Ȱ��ȭ
        // �⺻ ���� ������Ʈ
        unitNameText.text = unit.unitName;
        unitBranchText.text = unit.unitBranch;
        unitFactionText.text = unit.unitFaction;
        unitIMG.sprite = Resources.Load<Sprite>("UnitImages/" + unit.unitImg);

        // ���� ���� ǥ�� 11.17 ��ġ�� ǥ��
        healthText.text = unit.health.ToString();
        armorText.text = unit.armor.ToString();
        attackText.text = unit.attackDamage.ToString();
        mobilityText.text = unit.mobility.ToString();
        rangeText.text = unit.range.ToString();
        antiCavalryText.text = unit.antiCavalry.ToString();
        //evasionRateText.text = $"ȸ����: {unit.evasionRate}";

        // ���� �Ұ� ������Ʈ -> �ٸ� ������ �����Ͽ� ���� �߰��� ����
        //unitTooltipText.text = unit.unitDescription;

        // �г� Ȱ��ȭ
        detailPanel.SetActive(true);

        // Ư�� �� ��� ������Ʈ
        UpdateTraits(unit);
        UpdateSkills(unit);
    }
    private void UpdateTraits(UnitDataBase unit)
    {
        // ���� Ư�� ����
        foreach (Transform child in traitsParent)
        {
            Destroy(child.gameObject);
        }

        // Ư�� �߰�
        if (unit.lightArmor) AddTrait("�氩");
        if (unit.heavyArmor) AddTrait("�߰�");
        if (unit.rangedAttack) AddTrait("���Ÿ� ����");
        if (unit.bluntWeapon) AddTrait("�б�");
        if (unit.pierce) AddTrait("����");
        if (unit.agility) AddTrait("����");
        if (unit.strongCharge) AddTrait("���� ����");
        if (unit.perfectAccuracy) AddTrait("����");
        if (unit.slaughter) AddTrait("����");
    }

    private void UpdateSkills(UnitDataBase unit)
    {
        // ���� ��� ����
        foreach (Transform child in skillsParent)
        {
            Destroy(child.gameObject);
        }

        // ��� �߰�
        if (unit.charge) AddSkill("����");
        if (unit.defense) AddSkill("���� �¼�");
        if (unit.throwSpear) AddSkill("��â");
        if (unit.guerrilla) AddSkill("����");
        if (unit.guard) AddSkill("��ȣ");
        if (unit.assassination) AddSkill("�ϻ�");
        if (unit.drain) AddSkill("����");
        if (unit.overwhelm) AddSkill("����");
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


    // ���� �� UI�� ��Ȱ��ȭ
    public void HideUnitDetails()
    {
        detailPanel.SetActive(false);
    }
}