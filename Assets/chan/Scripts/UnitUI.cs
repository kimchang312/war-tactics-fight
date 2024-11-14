using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitUI : MonoBehaviour
{
    public TextMeshProUGUI unitNameText;    // ���� �̸� ǥ��
    public TextMeshProUGUI unitPriceText;   // ���� ���� ǥ��
    public Image unitImage;      // ���� �̹��� ǥ��
    public Button buyButton;     // ���� ��ư

    private UnitDataBase unitData;

    public void SetUnitData(UnitDataBase unit)
    {
        unitData = unit;

        // �α� ����
        Debug.Log($"���� �̸�: {unit.unitName}, ����: {unit.unitPrice}");

        // ���� �̸��� ���� �ؽ�Ʈ ����
        unitNameText.text = unit.unitName;
        unitPriceText.text = unit.unitPrice.ToString()+"G";

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
        buyButton.onClick.AddListener(() => BuyUnit());
    }

    private void BuyUnit()
    {
        // ShopManager���� ������ �����ϴ� �޼��� ȣ��
        ShopManager.Instance.BuyUnit(unitData);
        Debug.Log("���� ����");
    }
}
