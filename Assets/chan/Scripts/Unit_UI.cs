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
        // UnitDataManager에서 유닛 데이터 가져오기
        List<UnitDataBase> units = UnitDataManager.Instance.GetAllUnits();

        foreach (var unit in units)
        {
            unitNameText.text = unit.unitName;
            unitPriceText.text = unit.unitPrice.ToString() + "G";
            unitImage.sprite = Resources.Load<Sprite>("UnitImages/" + unit.unitImg);
        }
    }
}
