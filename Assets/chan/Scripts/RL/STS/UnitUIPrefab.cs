using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitUIPrefab : MonoBehaviour
{
    public Image unitImage;
    public TextMeshProUGUI energyText;

    public void SetupIMG(RogueUnitDataBase unit)
    {
        // 아이콘
        unitImage.sprite = Resources.Load<Sprite>($"UnitImages/{unit.unitImg}");     // data에 sprite 프로퍼티가 있다고 가정
    }
    public void SetupEnergy(RogueUnitDataBase unit)
    {

        // 기력 텍스트 "현재/최대"
        energyText.text = $"{unit.energy}";
    }
}

