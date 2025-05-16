using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public enum Context
{
    Lineup,    // LineUpBar 에 뜨는 내 유닛
    Placed,    // PlacePanel 에 배치된 유닛
    Enemy      // EnemyInfoPanel 에 뜨는 적 유닛
}

public class UnitUIPrefab : MonoBehaviour, IPointerClickHandler
{
    public Image unitImage;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI unitNumbering;
    public TextMeshProUGUI placeOrder;

    private Context PrefabType;
    public Context ContextType => PrefabType;
    private int _unitIdx;
    public int UnitIdx => _unitIdx;
    private CanvasGroup canvasGroup;
    [SerializeField] private GameObject numberTextObject;
    [SerializeField] private TextMeshProUGUI numberText;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void SetupIMG(RogueUnitDataBase unit, Context ctx)
    {
        _unitIdx = unit.idx;
        PrefabType = ctx;
        // 아이콘
        unitImage.sprite = Resources.Load<Sprite>($"UnitImages/{unit.unitImg}");     // data에 sprite 프로퍼티가 있다고 가정
        numberTextObject.gameObject.SetActive(false);                                                                // 초기 상태
        switch (ctx)
        {
            case Context.Lineup:
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                break;
            case Context.Placed:
                canvasGroup.alpha = 0.5f;
                canvasGroup.interactable = true;
                break;
            default: // Enemy
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = false;
                break;
        }
        unitNumbering.text = "";

    }
    public void SetupEnergy(RogueUnitDataBase unit)
    {
        // 기력 텍스트 "현재/최대"
        energyText.text = $"{unit.energy}";
    }
    public void SetNumber(int idx)
    {
        unitNumbering.text = idx.ToString();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        var gm = GameManager.Instance;
        if (!gm.IsPlaceMode) return;

        var place = gm.PlacePanelComponent;
        var lineup = gm.LineUpBarComponent;
        switch (PrefabType)
        {
            case Context.Lineup:
                // ① 배치판에 추가 → 반환된 순서로 UI 갱신
                int order = place.AddUnitToBattle(_unitIdx);
                PrefabType = Context.Placed;
                UpdateSelectionNumber(order);
                break;

            case Context.Placed:
                // ② 배치판에서 제거 → UI 정리 & 라인업 재정렬
                place.RemoveUnitFromBattle(_unitIdx);
                PrefabType = Context.Lineup;
                break;
        }
        //배치판·라인업바 모두 다시 그림
        place.UpdateBattleNumbers();
        lineup.UpdateLineupNumbers(place.PlacedUnits);
    }
    public void UpdateSelectionNumber(int number)
    {
        if (number > 0)
        {
            numberTextObject.SetActive(true);
            numberText.text = number.ToString();
            canvasGroup.alpha = 0.5f;
        }
        else
        {
            numberTextObject.SetActive(false);
            canvasGroup.alpha = 1f;
        }
    }
}

