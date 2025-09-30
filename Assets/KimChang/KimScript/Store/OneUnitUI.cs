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

    private UnitDetailExplain unitDetail;
    public RogueUnitDataBase unit;

    // 추가: 페이드에 사용할 CanvasGroup 캐시
    private CanvasGroup cg;
    private void Awake()
    {
        // 유닛 셀에 CanvasGroup이 없으면 생성
        cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
    }

    public void SetOneUnit(RogueUnitDataBase _unit)
    {
        unit = _unit;
        energyText.text = $"{unit.energy}/{unit.maxEnergy}";
        unitNameText.text = $"{unit.unitName}";
        UIMaker.CreateSelectUnitEnergy(unit, this.gameObject);
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

}
