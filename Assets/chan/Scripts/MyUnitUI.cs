using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MyUnitUI : MonoBehaviour
{
    [SerializeField] private Image unitImage;               // 유닛 이미지 표시
    [SerializeField] private TextMeshProUGUI unitText;   // 유닛 이름 , 소지개수 표시
    [SerializeField] private Button actionButton;              // 유닛 판매 버튼
    


    private UnitDataBase unitData;         // 해당 유닛의 데이터

    // 유닛 데이터를 외부에서 접근할 수 있도록 getter 제공
    public UnitDataBase UnitData => unitData;

    public void Setup(UnitDataBase unit)

    {   unitData = unit;

        int unitCount = PlayerData.Instance.GetUnitCount(unitData);
        if (unit == null)
        {
            Debug.LogError("전달된 유닛 데이터가 null입니다.");
            return;
        }
        
        Debug.Log($"유닛 이름: {unit.unitName}, 유닛 이미지: {unit.unitImg}");

        
        //unitText.text = $"{unit.unitName} x {unitCount}";  // 유닛의 이름 설정

        // 유닛 이미지 로드 및 설정
        unitImage.sprite = Resources.Load<Sprite>("UnitImages/" + unit.unitImg); // unitImg 경로에 맞춰 이미지 로드
        if (unitImage.sprite == null)
        {
            Debug.LogError("유닛 이미지 로드 실패: " + unit.unitImg);
            unitImage.sprite = Resources.Load<Sprite>("UnitImages/Default");  // 기본 이미지 설정 (옵션)
        }


        // 유닛 개수를 업데이트
        UpdateUnitCount();

        // 판매 버튼 클릭 이벤트 처리
        actionButton.onClick.AddListener(OnActionButtonClicked);
    }
    private void OnActionButtonClicked()
    {
        // ShopManager에서 배치 버튼 눌렀을 때 유닛 배치 호출
        ShopManager.Instance.OnUnitClicked(unitData);
    }

    // 유닛 개수를 업데이트하는 메서드
    public void UpdateUnitCount()
    {
        int unitCount = PlayerData.Instance.GetUnitCount(unitData);
        unitText.text = $"{unitData.unitName} x {unitCount}";

        // 유닛 개수가 0이면 이 UI 항목을 삭제
        if (unitCount == 0)
        {
            Destroy(gameObject);
        }
    }

    // 유닛 판매하는 메서드
    public void SellUnit()
    {
        PlayerData.Instance.SellUnit(unitData);  // PlayerData에서 유닛 판매
        UpdateUnitCount();  // 개수 업데이트
        ShopManager.Instance.UpdateCurrencyDisplay();  // 자금 UI 업데이트
    }
}
