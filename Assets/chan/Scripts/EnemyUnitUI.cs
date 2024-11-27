using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyUnitUI : MonoBehaviour
{
    public TextMeshProUGUI unitNameText;    // 유닛 이름 표시
    public Image unitImage;      // 유닛 이미지 표시    
    public TextMeshProUGUI enemyNumberText; //적 유닛 번호 표시

    private int unitIndex;

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
    // 유닛 인덱스 설정
    public void SetUnitIndex(int index)
    {
        enemyNumberText.text = $"{index + 1}"; // 인덱스는 1부터 표시하도록 설정
    }
    public void SetHidden(Sprite hiddenSprite)
    {
        unitImage.sprite = hiddenSprite;
        unitNameText.text = "???"; // 숨김 처리 시 이름 숨김
    }
}
