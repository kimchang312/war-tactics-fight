using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class UIMaker
{
    //유닛의 데이터, 만들 유닛UI 오브젝트
    public static void CreateSelectUnitEnergy(RogueUnitDataBase unit,GameObject selectedUnit)
    {
        Image img = selectedUnit.GetComponent<Image>();
        img.sprite = SpriteCacheManager.GetSprite($"UnitImages/Unit_Img_{unit.idx}");
        TextMeshProUGUI textMeshProUGUI = selectedUnit.GetComponentInChildren<TextMeshProUGUI>();
        textMeshProUGUI.text = $"{unit.energy}/{unit.maxEnergy}";
        selectedUnit.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = $"{unit.unitName}";
    }


}
