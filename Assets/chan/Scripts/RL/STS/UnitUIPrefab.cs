using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

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

    [SerializeField] public Context PrefabType;     // Inspector에 표시하도록 변경
    
    private CanvasGroup canvasGroup;
    [SerializeField] private GameObject numberTextObject; // 번호 표시용 오브젝트
    [SerializeField] private TextMeshProUGUI numberText; // 번호 표시 텍스트
    [Header("유닛 식별용")]
    public int unitId;     // 유닛 타입 식별자
    public int uniqueId;

    [HideInInspector] public RogueUnitDataBase unitData;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

    }
    
    public void SetupIMG(RogueUnitDataBase unit,Context ctx, int uniqueId)
    {
        unitData = unit;
        unitId = unit.idx;
        this.uniqueId = unit.UniqueId;
        Debug.Log(unit.UniqueId);
        PrefabType = ctx;
        
        unitImage.sprite = Resources.Load<Sprite>($"UnitImages/{unit.unitImg}");     // data에 sprite 프로퍼티가 있다고 가정                                                 
                                                                                                        
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        numberTextObject.SetActive(false);

    }
    public void SetupEnergy(RogueUnitDataBase unit)
    {
        // 기력 텍스트 "현재/최대"
        energyText.text = $"{unit.energy}";
    }
    // 유닛 머리 위 생성 순서 텍스트
    public void SetNumber(int idx)
    {
        unitNumbering.text = idx.ToString();
    }
    // 보유 유닛 클릭 후 이미지 알파값 변경과 배치 순서 숫자표시
    public void SetOrderNumber(int idx)
    {
        if(idx>0) 
        {
            numberTextObject.SetActive(true);
            numberText.text = idx.ToString();
        }
        else
        {
            numberTextObject.SetActive(false);
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button.Equals(PointerEventData.InputButton.Left))
        {
            if (!GameManager.Instance.IsPlaceMode) return;
            var place = GameManager.Instance.PlacePanelComponent;
            var lineup = GameManager.Instance.LineUpBarComponent;

            switch (PrefabType)
            {
                case Context.Lineup:

                    if (place.PlacedUniqueIds.Count >= RogueLikeData.Instance.GetMaxUnits())
                    {
                        Debug.Log("⚠️ 최대 배치 수에 도달했습니다.");
                        return;
                    }

                    // ① 배치판에 추가 → 반환된 순서로 UI 갱신
                    int order = place.AddUnitToBattle(unitData);

                    canvasGroup.alpha = 0.5f;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                    numberTextObject.SetActive(true);
                    numberText.text = order.ToString();
                    break;

                case Context.Placed:
                    // ② 배치판에서 제거 → UI 정리 & 라인업 재정렬
                    place.RemoveUnitFromBattle(unitData);
                    break;
            }
            lineup.UpdateLineupNumbers(place.PlacedUniqueIds);
        }
        else if(eventData.button.Equals(PointerEventData.InputButton.Right))
        {
            UnitDetailExplain unitDetail = GameManager.Instance.unitDetail;
            unitDetail.unit = unitData;
            unitDetail.gameObject.SetActive(true);            

        }

    }
    public void RestoreFromPlaced()
    {
       numberTextObject.SetActive(false);
       canvasGroup.alpha = 1f;
       canvasGroup.interactable = true;
       canvasGroup.blocksRaycasts = true;
    }
}

