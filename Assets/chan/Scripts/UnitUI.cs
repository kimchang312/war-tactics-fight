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

        // ���� �̸��� ���� �ؽ�Ʈ ����
        unitNameText.text = unit.unitName;
        unitPriceText.text = unit.unitPrice.ToString()+"G";

        // ���� �̹��� ���� (���� �̹��� ID�κ��� ���� �̹����� �ҷ����� ���)
        unitImage.sprite = Resources.Load<Sprite>("UnitImages/" + unit.unitImg);

        // ���� ��ư�� Ŭ�� �̺�Ʈ ó��
        buyButton.onClick.AddListener(() => BuyUnit());
    }

    private void BuyUnit()
    {
        // ShopManager���� ������ �����ϴ� �޼��� ȣ��
        FindObjectOfType<ShopManager>().BuyUnit(unitData);
    }
}
