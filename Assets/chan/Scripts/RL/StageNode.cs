using System.Collections.Generic;
using UnityEngine;

public class StageNode
{
    public int level;             // 스테이지의 레벨
    public Vector2 position;      // UI에서 표시할 기본 위치 (계산된 기준 좌표)
    public string name;
    public string gridID;         // 예: "1-a", "1-b", ... "15-g"
    public int creationIndex;     // 생성 순서 (전체 스테이지 번호)

    public List<StageNode> nextStages = new List<StageNode>();       // 연결된 다음 스테이지
    public List<StageNode> previousStages = new List<StageNode>();   // 연결된 이전 스테이지
    public bool isCleared = false;      // 해당 스테이지 클리어 여부
    public bool isLocked = true;        // 기본 잠금 상태
    public bool isClickable = false;    // 클릭 가능 여부


    private StageUIComponent stageUIComponent;
    public StageButton stageButton;

    public StageNode(int level, Vector2 position,string name)
    {
        this.level = level;
        this.position = position;
        this.name = name;
        // gridID와 creationIndex는 외부에서 할당할 예정
        nextStages = new List<StageNode>();
    }
    // UI 컴포넌트 설정 메서드
    public void SetUIComponent(StageUIComponent component)
    {
        if (component != null)
        {
            stageUIComponent = component;
            Debug.Log($"✅ {name}의 UI 컴포넌트가 정상적으로 연결됨.");
        }
        else
        {
            Debug.LogError($"❌ {name}의 UI 컴포넌트가 존재하지 않습니다!");
        }
    }

    // ✅ UI 상태 업데이트 메서드 추가
    public void UpdateUI()
    {
        if (stageButton != null)
        {
            stageButton.UpdateButtonState();
        }
    }
    // ✅ 상태 변경 메서드 추가
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
        {
            stageButton.SetInteractable(clickable); // ✅ 버튼의 interactable on/off 조절
        }
        UpdateUI();
    }

    public StageUIComponent GetUIComponent()
    {
        return stageUIComponent;
    }
}
