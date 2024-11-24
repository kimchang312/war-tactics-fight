using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitUI : MonoBehaviour
{
    public TextMeshProUGUI unitNameText;    // 유닛 이름 표시
    public TextMeshProUGUI unitPriceText;   // 유닛 가격 표시
    public Image unitImage;      // 유닛 이미지 표시
    public Button buyButton;     // 구매 버튼

    private UnitDataBase unitData;

    public void SetUnitData(UnitDataBase unit)
    {
        unitData = unit;

       

        // 유닛 이름과 가격 텍스트 설정
        unitNameText.text = unit.unitName;
        unitPriceText.text = unit.unitPrice.ToString()+"G";

        // 유닛 이미지 설정
        Sprite loadedSprite = Resources.Load<Sprite>("UnitImages/" + unit.unitImg);
        if (loadedSprite != null)
        {
            unitImage.sprite = loadedSprite;
            
        }
        

        // 구매 버튼의 클릭 이벤트 처리
        buyButton.onClick.AddListener(() => BuyUnit());
    }

    private void BuyUnit()
    {
        // ShopManager에서 유닛을 구매하는 메서드 호출
        ShopManager.Instance.BuyUnit(unitData);
        
    }
}
