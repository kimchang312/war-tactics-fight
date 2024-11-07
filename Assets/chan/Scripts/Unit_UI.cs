using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Unit_UI : MonoBehaviour
{
    public TextMeshProUGUI unitNameText;
    public TextMeshProUGUI unitPriceText;
    public Image unitImage;

    private void Start()
    {
        // UnitDataManager���� ���� ������ ��������
        List<UnitDataBase> units = UnitDataManager.Instance.GetAllUnits();

        foreach (var unit in units)
        {
            unitNameText.text = unit.unitName;
            unitPriceText.text = unit.unitPrice.ToString() + "G";
            unitImage.sprite = Resources.Load<Sprite>("UnitImages/" + unit.unitImg);
        }
    }
}
