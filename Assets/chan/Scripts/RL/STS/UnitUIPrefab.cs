using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitUIPrefab : MonoBehaviour
{
    public Image unitImage;
    public TextMeshProUGUI energyText;

    public void Setup(RogueUnitDataBase unit)
    {
        // 1) 아이콘
        unitImage.sprite = Resources.Load<Sprite>($"UnitImages/{unit.unitImg}");     // data에 sprite 프로퍼티가 있다고 가정
        Debug.Log(unit.unitImg);
        // 2) 기력 텍스트 "현재/최대"
        energyText.text = $"{unit.energy}";
    }
}

