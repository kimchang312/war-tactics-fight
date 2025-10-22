using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class OneUnitUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image unitImg;
    [SerializeField] private Image energyImg;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI unitNameText;
    [SerializeField] private Image unitFramImg;
    public GameObject selectFrame;

    private UnitDetailExplain unitDetail;
    public RogueUnitDataBase unit;

    private CanvasGroup cg;
    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
    }

    public void SetOneUnit(RogueUnitDataBase _unit)
    {
        unit = _unit;
        energyText.text = $"{unit.Energy}/{unit.MaxEnergy}";
        unitNameText.text = $"{unit.unitName}";
        UIMaker.CreateSelectUnitEnergy(unit, this.gameObject);
        if(selectFrame != null)
        {
            selectFrame.SetActive(false);

        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (unitDetail == null) unitDetail = GameManager.Instance.unitDetail;
            unitDetail.unit = unit;
            unitDetail.gameObject.SetActive(true);
        }
    }
    public void SetDisableEnergyName()
    {
        energyImg.gameObject.SetActive(false);
        energyText.gameObject.SetActive(false);
        unitNameText.gameObject.SetActive(false);
    }
    public void SetAbleEnergyName()
    {
        energyImg.gameObject.SetActive(true);
        energyText.gameObject.SetActive(true);
        unitNameText.gameObject.SetActive(true);
    }

}
