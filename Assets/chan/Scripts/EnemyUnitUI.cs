using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyUnitUI : MonoBehaviour
{
    public TextMeshProUGUI unitNameText;    // 유닛 이름 표시
    public Image unitImage;      // 유닛 이미지 표시    

    private UnitDataBase unitData;

    public void SetUnitData(UnitDataBase unit)
    {
        unitData = unit;

        // 유닛 이름 텍스트 설정
        unitNameText.text = unit.unitName;
 

        // 유닛 이미지 설정
        Sprite loadedSprite = Resources.Load<Sprite>("UnitImages/" + unit.unitImg);
        if (loadedSprite != null)
        {
            unitImage.sprite = loadedSprite;
            
        }
        

        
    }

}
