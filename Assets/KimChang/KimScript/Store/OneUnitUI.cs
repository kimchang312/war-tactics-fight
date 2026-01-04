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
    [SerializeField] private Image unitFrameImg;
    public GameObject selectFrame;

    private UnitDetailExplain unitDetail;
    public RogueUnitDataBase unit;
    //private RectTransform _unitFrameRt;


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
        unitNameText.text = GameTextDB.GetByForeignKey(TextKind.Unit, unit.idx);
        UIMaker.CreateSelectUnitEnergy(unit, gameObject);

        unitFrameImg.sprite = SpriteCacheManager.GetFrameByRarity(unit.rarity);

        RectTransform frameRt = unitFrameImg.rectTransform;

        // unitImg의 RectTransform 크기 기준으로 프레임 크기 결정 (+93, +87)
        Vector2 imgSize = Vector2.zero;
        if (unitImg != null)
        {
            var imgRt = unitImg.rectTransform;

            imgSize = imgRt.sizeDelta;
            if (imgSize.x <= 0f || imgSize.y <= 0f)
                imgSize = imgRt.rect.size;
        }
        float unitFrame = unit.rarity == 4 ? imgSize.x * 1.185f : imgSize.x * 1.17f;
        Vector2 frameSize = new Vector2(unitFrame, unitFrame);

        // 레이아웃 그룹이 sizeDelta를 덮어쓰는 구조면 LayoutElement로 주는 게 더 안정적임
        var layout = unitFrameImg.GetComponent<UnityEngine.UI.LayoutElement>();
        if (layout != null)
        {
            layout.preferredWidth = frameSize.x;
            layout.preferredHeight = frameSize.y;
        }
        else
        {
            frameRt.sizeDelta = frameSize;
        }

        if (selectFrame != null)
            selectFrame.SetActive(false);
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
