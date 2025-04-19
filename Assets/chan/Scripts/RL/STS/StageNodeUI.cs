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
    public Color stageColor;

    [Header("Connections")]
    // 이 스테이지와 연결된 다음 스테이지 UI 객체들
    public List<StageNodeUI> connectedStages = new List<StageNodeUI>();

    // 내부 잠금 상태
    private bool isLocked = false;
    public bool IsLocked => isLocked;

    private Image stageImage;

    private void Awake()
    {
        // Image 컴포넌트를 찾아두고
        stageImage = GetComponent<Image>();
        if (stageImage == null)
            Debug.LogWarning("StageNodeUI: Image 컴포넌트가 없습니다.");
    }

    /// <summary>
    /// MapGenerator에서 할당한 노드 데이터를 기반으로 초기화
    /// </summary>
    public void Setup(StageNode node)
    {
        level = node.level;
        row = node.row;
        stageType = node.stageType;
        stageColor = node.stageColor;

        if (stageImage != null)
            stageImage.color = stageColor;
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
        if (stageImage != null)
            stageImage.color = GameManager.Instance.lockedColor;
    }

    /// <summary>
    /// 이 스테이지 잠금을 해제합니다.
    /// </summary>
    public void UnlockStage()
    {
        isLocked = false;
        if (stageImage != null)
            stageImage.color = stageColor;
    }

    /// <summary>
    /// 이 스테이지가 other와 연결되어 있는지 확인합니다.
    /// </summary>
    public bool IsConnectedTo(StageNodeUI other)
    {
        return connectedStages.Contains(other);
    }
}
