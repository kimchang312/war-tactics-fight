using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class OneUnitUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject energyObj;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI unitNameText;
    private UnitDetailExplain unitDetail;
    public RogueUnitDataBase unit;

    public void SetOneUnit(RogueUnitDataBase _unit)
    {
        SetAbleUI();
        unit = _unit;
        energyText.text = $"{unit.energy}/{unit.maxEnergy}";
        unitNameText.text = $"{unit.unitName}";
        UIMaker.CreateSelectUnitEnergy(unit, this.gameObject);
        SetDisableUI();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if(unitDetail ==null) unitDetail = GameManager.Instance.unitDetail;
            unitDetail.unit = unit;
            unitDetail.gameObject.SetActive(true);
        }
    }
    public void SetAbleUI()
    {
        energyObj.SetActive(true);
        energyText.gameObject.SetActive(true);
        unitNameText.gameObject.SetActive(true);
    }

    public void SetDisableUI()
    {
        energyObj.SetActive(false);
        energyText.gameObject.SetActive(false);
        unitNameText.gameObject.SetActive(false);
    }
}
