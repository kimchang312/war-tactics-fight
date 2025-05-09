using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        button = GetComponent<Button>();
        image = GetComponent<Image>();
    }
    private void Init()
    {
        // null 체크 후에만 GetComponent
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        if (button == null)
            button = GetComponent<Button>();
        if (image == null)
            image = GetComponent<Image>();
    }

    /// <summary>
    /// MapGenerator에서 할당한 노드 데이터를 기반으로 초기화
    /// </summary>
    public void Setup(StageNode node)
    {
        level = node.level;
        row = node.row;
        stageType = node.stageType;
        // 1) StageType별로 스프라이트 교체
        switch (stageType)
        {
            case StageType.Combat: image.sprite = combatSprite; break;
            case StageType.Elite: image.sprite = eliteSprite; break;
            case StageType.Event: image.sprite = eventSprite; break;
            case StageType.Shop: image.sprite = shopSprite; break;
            case StageType.Rest: image.sprite = restSprite; break;
            case StageType.Treasure: image.sprite = treasureSprite; break;
            case StageType.Boss: image.sprite = bossSprite; break;
        }
        // presetID 할당
        this.presetID = node.presetID;
    }

    /// <summary>
    /// 클릭 이벤트 처리 (IPointerClickHandler)
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isLocked)
        {
            Debug.Log("이 스테이지는 잠겨 있습니다.");
            return;
        }
        GameManager.Instance.OnStageClicked(this);
    }

    /// <summary>
    /// 이 스테이지를 잠급니다.
    /// </summary>
    public void LockStage()
    {
        isLocked = true;
        // 클릭 이벤트 차단W 이거 해결해야함
        if(canvasGroup == null) {
            this.gameObject.AddComponent<CanvasGroup>();
            canvasGroup =GetComponent<CanvasGroup>();
            Debug.Log(canvasGroup);
        }
      
        canvasGroup.blocksRaycasts = false;
        // 하이라이트, 포커스 등 UI 인터랙션 모두 차단
        canvasGroup.interactable = false;

        // Button 인터랙션도 차단
        if (button != null)
            button.interactable = false;
    }

    /// <summary>
    /// 이 스테이지 잠금을 해제합니다.
    /// </summary>
    public void UnlockStage()
    {
        isLocked = false;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        if (button != null)
            button.interactable = true;
    }

    /// <summary>
    /// 이 스테이지가 other와 연결되어 있는지 확인합니다.
    /// </summary>
    public bool IsConnectedTo(StageNodeUI other)
    {
        return connectedStages.Contains(other);
    }
}
