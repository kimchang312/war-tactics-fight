using UnityEngine;
using UnityEngine.EventSystems;

public class URC : MonoBehaviour, IPointerClickHandler
{
    private UnitDataBase unitData; // 이 유닛의 데이터

    [SerializeField] private UnitDetailUI unitDetailUI; // 여전히 Inspector에서 설정 가능

    // unitDetailUI를 외부에서 읽고 설정할 수 있는 속성
    public UnitDetailUI UnitDetailUI
    {
        get => unitDetailUI;
        set => unitDetailUI = value;
    }



    private void Start()
    {
        // unitDetailUI가 null일 경우만 FindObjectOfType로 검색
        if (unitDetailUI == null)
        {
            unitDetailUI = FindObjectOfType<UnitDetailUI>();
            
        }
    }

    public void SetUnitData(UnitDataBase data)
    {
        unitData = data;
    }
    // IPointerClickHandler 인터페이스 구현
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right) // 우클릭 감지
        {
            Debug.Log("Right Click Detected");
            if (unitData == null)
            {
                Debug.LogWarning("unitData is null!");
            }
            if (unitData != null && unitDetailUI != null)
            {
                unitDetailUI.ShowUnitDetails(unitData); // 유닛 정보 표시
            }
            
        }
    }
}
