using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class UIMaker
{
    //유닛의 데이터, 만들 유닛UI 오브젝트
    public static void CreateSelectUnitEnergy(RogueUnitDataBase unit, GameObject selectedUnit)
    {
        // 유닛 이미지 세팅
        Image img = selectedUnit.GetComponent<Image>();
        img.sprite = SpriteCacheManager.GetSprite($"UnitImages/Unit_Img_{unit.idx}");

        // Energy UI 찾기
        Transform energyObj = selectedUnit.transform.Find("Energy");
        if (energyObj != null)
        {
            TextMeshProUGUI energyText = energyObj.GetComponent<TextMeshProUGUI>();
            if (energyText != null)
            {
                energyText.text = $"{unit.energy}/{unit.maxEnergy}";
            }
        }

        // UnitName UI 찾기
        Transform nameObj = selectedUnit.transform.Find("UnitName");
        if (nameObj != null)
        {
            TextMeshProUGUI nameText = nameObj.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = unit.unitName;
            }
        }
    }



}
