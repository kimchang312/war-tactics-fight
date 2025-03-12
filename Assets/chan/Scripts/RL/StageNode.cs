using Map;
using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    None,
    //Event, 이벤트 스테이지 표현 방법 정하기
    Monster,
    Elite,
    Treasure,
    Rest,
    Shop,
    Boss
}

public class StageNode : MonoBehaviour
{
    // 기본 정보 (슬레이 더 스파이어 예시와 유사)
    public int floor;             // 층 번호 (1부터 시작)
    public string nodeName;       // 노드 이름 (예: "Start", "Floor 3 Node 1")
    public string gridID;         // UI 배치를 위한 ID (예: "1-a", "1-b", ...)
    public int creationIndex;     // 생성 순서 번호

    // 맵 생성 알고리즘에서 사용하는 연결 정보
    public List<Vector2Int> incoming = new List<Vector2Int>();
    public List<Vector2Int> outgoing = new List<Vector2Int>();

    // 상태 플래그
    public bool isCleared = false;
    public bool isLocked = true;
    public bool isClickable = false;

    // UI 배치 정보 (맵 내 실제 위치)
    public Vector2 position;      // UI 상에서의 좌표 (맵 생성 시 설정됨)
    public int indexOnFloor;      // 해당 층 내 수평 인덱스

    // 추가: 노드의 타입 (EncounterType 대신 NodeType 사용)
    public NodeType nodeType = NodeType.None;

    // UI 관련 참조
    public StageUIComponent uiComponent;
    public StageButton stageButton;

    // 편의 프로퍼티: 그리드 좌표 (x = indexOnFloor, y = floor)
    public Vector2Int point => new Vector2Int(indexOnFloor, floor);

    // 데이터 모델(Node)로 변환하는 메서드
    public Node ToNode()
    {
        Node nodeData = new Node(this.nodeType, this.nodeName, this.point);
        nodeData.position = this.position;
        // 추가적으로 필요한 필드를 설정할 수 있습니다.
        return nodeData;
    }

    // UI 업데이트 메서드
    public void UpdateUI()
    {
        if (stageButton != null)
            stageButton.UpdateButtonState();
    }

    public void SetCleared(bool cleared)
    {
        isCleared = cleared;
        UpdateUI();
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
        UpdateUI();
    }

    public void SetClickable(bool clickable)
    {
        isClickable = clickable;
        if (stageButton != null)
            stageButton.SetInteractable(clickable);
        UpdateUI();
    }

    // UI 컴포넌트 설정
    public void SetUIComponent(StageUIComponent component)
    {
        if (component != null)
        {
            uiComponent = component;
            Debug.Log($"✅ {nodeName}의 UI 컴포넌트가 연결됨.");
        }
        else
        {
            Debug.LogError($"❌ {nodeName}의 UI 컴포넌트가 없습니다!");
        }
    }
}
