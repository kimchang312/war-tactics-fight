using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlacedUnit : MonoBehaviour
{
    public TextMeshProUGUI unitNameText;    // 유닛 이름 표시
    public Image unitImage;      // 유닛 이미지 표시
    public Button ReturnButton;     // 구매 버튼

    private UnitDataBase unitData;

    public void SetUnitData(UnitDataBase unit)
    {
        unitData = unit;

        // 로그 찍어보자
        Debug.Log($"유닛 이름: {unit.unitName}");

        // 유닛 이름과 가격 텍스트 설정
        unitNameText.text = unit.unitName;

        // 유닛 이미지 설정
        Sprite loadedSprite = Resources.Load<Sprite>("UnitImages/" + unit.unitImg);
        if (loadedSprite != null)
        {
            unitImage.sprite = loadedSprite;
            Debug.Log("유닛 이미지 로드 성공: " + unit.unitImg);
        }
        else
        {
            Debug.LogError("유닛 이미지 로드 실패: " + unit.unitImg);
        }

        // 구매 버튼의 클릭 이벤트 처리
        ReturnButton.onClick.AddListener(() => ReturnUnit());
    }

    private void ReturnUnit()
    {
            // ShopManager에서 유닛을 구매하는 메서드 호출
            ShopManager.Instance.ReturnUnit(unitData);
            Debug.Log($"유닛 되돌림: {unitData.unitName}");
        
            // 배치된 유닛 게임 오브젝트 삭제
            Destroy(gameObject);
        
    }
}