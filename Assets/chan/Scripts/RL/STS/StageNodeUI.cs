using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup), typeof(Button), typeof(Image))]
public class StageNodeUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Node Data")]
    public int level;
    public int row;
    public StageType stageType;

    [Header("Sprites (assign in Inspector)")]
    public Sprite combatSprite;
    public Sprite eliteSprite;
    public Sprite eventSprite;
    public Sprite shopSprite;
    public Sprite restSprite;
    public Sprite treasureSprite;
    public Sprite bossSprite;


    [Header("Connections")]
    // 이 스테이지와 연결된 다음 스테이지 UI 객체들
    public List<StageNodeUI> connectedStages = new List<StageNodeUI>();



    // 내부 잠금 상태
    private bool isLocked = false;
    public bool IsLocked => isLocked;

    // 클릭 가능/불가능 제어
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button button;
    [SerializeField] private Image image;

    [SerializeField] private int presetID;
    public int PresetID => presetID;

    private GameObject effectObject;
    private Tween pulseTween;

    private void Awake()
    {
        // 컴포넌트가 무조건 붙어 있으므로 GetComponent로 캐시
        CacheComponents();

        effectObject = transform.Find("SelectableEffect")?.gameObject;
        if (effectObject != null)
            effectObject.SetActive(false);
    }

    private void OnEnable()
    {
        // 다시 활성화될 때마다 깨진 참조 복구
        if (canvasGroup == null || button == null || image == null)
            CacheComponents();
    }
    private void CacheComponents()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        button = GetComponent<Button>();
        image = GetComponent<Image>();
    }


    /// MapGenerator에서 할당한 노드 데이터를 기반으로 초기화

    public void Setup(StageNode node)
    {
        level = node.level;
        row = node.row;
        stageType = node.stageType;
        // 1) StageType별로 스프라이트 교체
        image.sprite = stageType switch
        {
            StageType.Combat => combatSprite,
            StageType.Elite => eliteSprite,
            StageType.Event => eventSprite,
            StageType.Shop => shopSprite,
            StageType.Rest => restSprite,
            StageType.Treasure => treasureSprite,
            StageType.Boss => bossSprite,
            _ => image.sprite
        };
        // presetID 할당
        presetID = node.presetID;
    }


    /// 클릭 이벤트 처리 (IPointerClickHandler)
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isLocked)
        {
            Debug.Log("이 스테이지는 잠겨 있습니다.");
            return;
        }
        GameManager.Instance.OnStageClicked(this);
    }

    
    /// 이 스테이지를 잠급니다.
    
    public void LockStage()
    {
        isLocked = true;
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        if (button != null) button.interactable = false;
        StopSelectableEffect(); // 🔽 효과 중지
    }

    
    /// 이 스테이지 잠금을 해제합니다.
   
    public void UnlockStage()
    {
        isLocked = false;
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
        if (button != null) button.interactable = true;
        PlaySelectableEffect(); // 🔽 효과 시작
    }


    /// 이 스테이지가 other와 연결되어 있는지 확인합니다.

    public bool IsConnectedTo(StageNodeUI other)
    {
        return connectedStages.Contains(other);
    }

    // ✅ 선택 가능 효과 시작
    public void PlaySelectableEffect()
    {
        transform.localScale = Vector3.one;

        pulseTween?.Kill();
        pulseTween = transform
            .DOScale(1.1f, 0.6f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetId(this);
    }

    // ✅ 선택 효과 제거
    public void StopSelectableEffect()
    {
        pulseTween?.Kill();
        pulseTween = null;
        transform.localScale = Vector3.one;
    }
}
