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
    [SerializeField] private Image unitFrame; // 레어도에 따른 유닛 테두리
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
            var sprite = SpriteCacheManager.GetSprite($"UnitImages/Unit_Img_{unit.idx}");
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
        
        // 아군 유닛일 때만 레어도에 따른 테두리 설정
        if (ctx == Context.Lineup || ctx == Context.Placed)
        {
            SetupUnitFrame(unit);
        }
    }
    
    /// <summary>
    /// 레어도에 따라 유닛 테두리를 설정합니다 (아군 유닛만)
    /// </summary>
    private void SetupUnitFrame(RogueUnitDataBase unit)
    {
        if (unit == null)
        {
            Debug.LogWarning("UnitUIPrefab.SetupUnitFrame: unit is null!");
            return;
        }
        
        // unitFrame이 null이면 자동으로 찾기 시도
        if (unitFrame == null)
        {
            // 1) unitImage의 첫 번째 자식에서 찾기 (AutoBattleUI와 동일한 구조)
            if (unitImage != null && unitImage.transform.childCount > 0)
            {
                Transform childFrame = unitImage.transform.GetChild(0);
                unitFrame = childFrame.GetComponent<Image>();
                if (unitFrame != null)
                {
                    Debug.Log($"[UnitUIPrefab] unitFrame을 자식 오브젝트에서 찾았습니다: {childFrame.name}");
                }
            }
            
            // 2) 여전히 null이면 전체 하위에서 Image 컴포넌트 찾기 (unitImage 제외)
            if (unitFrame == null)
            {
                Image[] allImages = GetComponentsInChildren<Image>(true);
                foreach (var img in allImages)
                {
                    if (img != unitImage)
                    {
                        unitFrame = img;
                        Debug.Log($"[UnitUIPrefab] unitFrame을 하위 오브젝트에서 찾았습니다: {img.name}");
                        break;
                    }
                }
            }
            
            // 3) 여전히 null이면 경고
            if (unitFrame == null)
            {
                Debug.LogWarning("UnitUIPrefab.SetupUnitFrame: unitFrame을 찾을 수 없습니다! 인스펙터에서 할당하거나 프리팹 구조를 확인해주세요.");
                return;
            }
        }
        
        if (unitImage == null)
        {
            Debug.LogWarning("UnitUIPrefab.SetupUnitFrame: unitImage is null!");
            return;
        }
        
        // unitFrame 활성화 확인
        if (!unitFrame.gameObject.activeSelf)
        {
            unitFrame.gameObject.SetActive(true);
            Debug.Log("[UnitUIPrefab] unitFrame을 활성화했습니다.");
        }
        
        // 레어도에 따른 테두리 스프라이트 설정
        Sprite frameSprite = SpriteCacheManager.GetFrameByRarity(unit.rarity);
        if (frameSprite == null)
        {
            Debug.LogError($"[UnitUIPrefab] 레어도 {unit.rarity}에 대한 프레임 스프라이트를 찾을 수 없습니다! 경로: KIcon/Frame/border_rarity_{unit.rarity}");
            return;
        }
        
        unitFrame.sprite = frameSprite;
        Debug.Log($"[UnitUIPrefab] 레어도 {unit.rarity} 프레임 적용 완료: {frameSprite.name}");
        
        // 유닛 이미지의 크기 가져오기
        RectTransform unitImageRect = unitImage.rectTransform;
        float unitSize = unitImageRect.rect.width;
        if (unitSize <= 0f)
        {
            // rect가 0이면 sizeDelta 사용
            unitSize = unitImageRect.sizeDelta.x;
            if (unitSize <= 0f)
            {
                // sizeDelta도 0이면 기본값 사용 (일반적인 유닛 크기)
                unitSize = 100f;
                Debug.LogWarning("UnitUIPrefab.SetupUnitFrame: unitSize를 계산할 수 없어 기본값 100을 사용합니다.");
            }
        }
        
        // 테두리 크기 계산 (레어도 4는 1.185f, 그 외는 1.17f)
        float frameSize = unitSize * (unit.rarity == 4 ? 1.185f : 1.17f);
        
        // 테두리 RectTransform 가져오기
        RectTransform frameRect = unitFrame.rectTransform;
        
        // 테두리 크기 설정
        frameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, frameSize);
        frameRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, frameSize);
        
        Debug.Log($"[UnitUIPrefab] 테두리 크기 설정 완료: {frameSize} (유닛 크기: {unitSize}, 레어도: {unit.rarity})");
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
            // 배치/적 유닛은 기력을 표시하지 않음
            if (PrefabType == Context.Enemy || PrefabType == Context.Placed)
            {
                energyText.gameObject.SetActive(false);
            }
            else
            {
                energyText.gameObject.SetActive(true);
                energyText.text = $"{unit.Energy}";
            }
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

                    if (!place.CanAddUnit())
                    {
                        Debug.Log("⚠️ 최대 배치 수에 도달했습니다.");
                        return;
                    }

                    // ① 배치판에 추가 → 반환된 순서로 UI 갱신
                    int order = place.AddUnitToBattle(unitData);
                    if (order <= 0)
                        return;

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

