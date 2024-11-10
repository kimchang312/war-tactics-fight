using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlacedUnit : MonoBehaviour
{
    public TextMeshProUGUI unitNameText;    // ���� �̸� ǥ��
    public Image unitImage;      // ���� �̹��� ǥ��
    public Button ReturnButton;     // ���� ��ư

    private UnitDataBase unitData;

    public void SetUnitData(UnitDataBase unit)
    {
        unitData = unit;

        // �α� ����
        Debug.Log($"���� �̸�: {unit.unitName}");

        // ���� �̸��� ���� �ؽ�Ʈ ����
        unitNameText.text = unit.unitName;

        // ���� �̹��� ����
        Sprite loadedSprite = Resources.Load<Sprite>("UnitImages/" + unit.unitImg);
        if (loadedSprite != null)
        {
            unitImage.sprite = loadedSprite;
            Debug.Log("���� �̹��� �ε� ����: " + unit.unitImg);
        }
        else
        {
            Debug.LogError("���� �̹��� �ε� ����: " + unit.unitImg);
        }

        // ���� ��ư�� Ŭ�� �̺�Ʈ ó��
        ReturnButton.onClick.AddListener(() => ReturnUnit());
    }

    private void ReturnUnit()
    {
            // ShopManager���� ������ �����ϴ� �޼��� ȣ��
            ShopManager.Instance.ReturnUnit(unitData);
            Debug.Log($"���� �ǵ���: {unitData.unitName}");
        
            // ��ġ�� ���� ���� ������Ʈ ����
            Destroy(gameObject);
        
    }
}