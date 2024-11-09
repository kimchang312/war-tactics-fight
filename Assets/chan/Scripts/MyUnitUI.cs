using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MyUnitUI : MonoBehaviour
{
    public Image unitImage;
    public TextMeshProUGUI unitNameText;
    public TextMeshProUGUI unitCountText;

    private UnitDataBase unitData;
    

    public void SellUnit()
    {
        PlayerData.Instance.SellUnit(unitData);
        UpdateUnitCount();
        ShopManager.Instance.UpdateCurrencyDisplay();
    }

    private void UpdateUnitCount()
    {
        int unitCount = PlayerData.Instance.GetUnitCount(unitData);
        unitCountText.text = "x" + unitCount.ToString();
        if (unitCount == 0)
        {
            Destroy(gameObject);
        }
    }
}