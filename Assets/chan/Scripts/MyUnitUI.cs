using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MyUnitUI : MonoBehaviour
{
    [SerializeField] private Image unitImage;               // ���� �̹��� ǥ��
    [SerializeField] private TextMeshProUGUI unitText;   // ���� �̸� , �������� ǥ��
    [SerializeField] private Button actionButton;              // ���� �Ǹ� ��ư
    


    private UnitDataBase unitData;         // �ش� ������ ������

    // ���� �����͸� �ܺο��� ������ �� �ֵ��� getter ����
    public UnitDataBase UnitData => unitData;

    public void Setup(UnitDataBase unit)

    {   unitData = unit;

        int unitCount = PlayerData.Instance.GetUnitCount(unitData);
        if (unit == null)
        {
            Debug.LogError("���޵� ���� �����Ͱ� null�Դϴ�.");
            return;
        }
        
        Debug.Log($"���� �̸�: {unit.unitName}, ���� �̹���: {unit.unitImg}");

        
        //unitText.text = $"{unit.unitName} x {unitCount}";  // ������ �̸� ����

        // ���� �̹��� �ε� �� ����
        unitImage.sprite = Resources.Load<Sprite>("UnitImages/" + unit.unitImg); // unitImg ��ο� ���� �̹��� �ε�
        if (unitImage.sprite == null)
        {
            Debug.LogError("���� �̹��� �ε� ����: " + unit.unitImg);
            unitImage.sprite = Resources.Load<Sprite>("UnitImages/Default");  // �⺻ �̹��� ���� (�ɼ�)
        }


        // ���� ������ ������Ʈ
        UpdateUnitCount();

        // �Ǹ� ��ư Ŭ�� �̺�Ʈ ó��
        actionButton.onClick.AddListener(OnActionButtonClicked);
    }
    private void OnActionButtonClicked()
    {
        // ShopManager���� ��ġ ��ư ������ �� ���� ��ġ ȣ��
        ShopManager.Instance.OnUnitClicked(unitData);
    }

    // ���� ������ ������Ʈ�ϴ� �޼���
    public void UpdateUnitCount()
    {
        int unitCount = PlayerData.Instance.GetUnitCount(unitData);
        unitText.text = $"{unitData.unitName} x {unitCount}";

        // ���� ������ 0�̸� �� UI �׸��� ����
        if (unitCount == 0)
        {
            Destroy(gameObject);
        }
    }

    // ���� �Ǹ��ϴ� �޼���
    public void SellUnit()
    {
        PlayerData.Instance.SellUnit(unitData);  // PlayerData���� ���� �Ǹ�
        UpdateUnitCount();  // ���� ������Ʈ
        ShopManager.Instance.UpdateCurrencyDisplay();  // �ڱ� UI ������Ʈ
    }
}
