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
        if (unit == null)
        {
            Debug.LogError("UnitUIPrefab.SetupIMG: unit is null!");
            return;
        }
        
        unitData = unit;
        unitId = unit.idx;
        this.uniqueId = unit.UniqueId;
        PrefabType = ctx;
        
        // unitImage null 체크
        if (unitImage != null)
        {
            var sprite = SpriteCacheManager.GetSprite($"UnitImages/{unit.unitImg}");
            if (sprite != null)
            {
                unitImage.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"Sprite not found: UnitImages/{unit.unitImg}");
            }
        }
        else
        {
            Debug.LogError("UnitUIPrefab.SetupIMG: unitImage is null!");
        }
        
        // canvasGroup null 체크
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            Debug.LogError("UnitUIPrefab.SetupIMG: canvasGroup is null!");
        }
        
        // numberTextObject null 체크
        if (numberTextObject != null)
        {
            numberTextObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("UnitUIPrefab.SetupIMG: numberTextObject is null!");
        }
    }
    public void SetupEnergy(RogueUnitDataBase unit)
    {
        if (unit == null)
        {
            Debug.LogError("UnitUIPrefab.SetupEnergy: unit is null!");
            return;
        }
        
        // 기력 텍스트 "현재/최대"
        if (energyText != null)
        {
            energyText.text = $"{unit.energy}";
        }
        else
        {
            Debug.LogError("UnitUIPrefab.SetupEnergy: energyText is null!");
        }
    }
    // 유닛 머리 위 생성 순서 텍스트
    public void SetNumber(int idx)
    {
        if (unitNumbering != null)
        {
            unitNumbering.text = idx.ToString();
        }
        else
        {
            Debug.LogError("UnitUIPrefab.SetNumber: unitNumbering is null!");
        }
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
            if (!GameManager.Instance.IsPlaceMode || !GameManager.Instance.PlacePanelComponent.gameObject.activeSelf) 
                return;
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
                    
                case Context.Enemy:
                    // ③ 적 유닛 클릭 시 상세 정보 표시 (클릭만 가능, 배치 불가)
                    Debug.Log($"적 유닛 정보: {unitData.unitName} (ID: {unitData.idx})");
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

