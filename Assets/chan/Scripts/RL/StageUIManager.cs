using System.Collections.Generic;
using UnityEngine;

public class StageUIManager : MonoBehaviour
{
    public GameObject stageButtonPrefab;
    public Transform stageContainer;
    public StageTooltip stageTooltip;
    public GameObject marker; // ✅ 현재 위치를 표시할 마커

    private StageNode currentStage;
    public StageMapManager stageMapManager; // ✅ StageMapManager 참조

    public StageNode GetCurrentStage()
    {
        return stageMapManager != null ? stageMapManager.GetCurrentStage() : null;
    }

    private void Awake()
    {
        Debug.Log("🟡 StageUIManager Awake() 실행됨");

        // ✅ StageButton 이벤트 구독
        StageButton.OnStageButtonClicked += MoveToStage;

        StageMapManager.OnStageGenerated += GenerateStageUI;
        StageMapManager.OnStageChanged += UpdateStageUI;
    }


    private void OnDestroy()
    {
        // ✅ 이벤트 해제 (메모리 누수 방지)
        StageButton.OnStageButtonClicked -= MoveToStage;
    }
    private void OnDisable()
    {
        StageMapManager.OnStageGenerated -= GenerateStageUI;
        StageMapManager.OnStageChanged -= UpdateStageUI;
    }

    void GenerateStageUI(List<StageNode> allStages)
    {
        Debug.Log("🔵 Stage UI 생성 시작");

        foreach (StageNode stage in allStages)
        {
            Debug.Log($"🟡 스테이지 버튼 생성: 레벨 {stage.level}, 위치 {stage.position}");

            GameObject buttonObj = Instantiate(stageButtonPrefab, stageContainer);
            buttonObj.transform.localPosition = stage.position;
            StageButton button = buttonObj.GetComponent<StageButton>();

            if (button == null)
            {
                Debug.LogError("❌ StageButton 컴포넌트가 없습니다! Prefab을 확인하세요.");
            }
            else
            {
                button.SetStage(stage, stageTooltip, this);
            }

            // ✅ 시작 노드(레벨 1)에서 마커를 초기 위치로 배치
            if (stage.level == 1)
            {
                currentStage = stage;
                UpdateMarkerPosition(stage.position);
            }
        }

        Debug.Log("🟢 Stage UI 생성 완료");
    }


    void UpdateStageUI(StageNode newStage)
    {
        Debug.Log("🔵 스테이지 이동: " + newStage.level);

        currentStage = newStage;
        UpdateMarkerPosition(newStage.position);
        UpdateStageOpacity();
    }

    // ✅ 현재 위치 마커 이동 함수
    void UpdateMarkerPosition(Vector2 position)
    {
        if (marker == null)
        {
            Debug.LogError("❌ 마커가 null 상태입니다! Unity Inspector에서 연결되었는지 확인하세요.");
            return;
        }

        Vector2 adjustedPosition = position + new Vector2(0f, 80f); // ✅ 버튼보다 위쪽으로 조정
        marker.transform.localPosition = adjustedPosition;
        Debug.Log("📍 마커 위치 초기화 완료: " + adjustedPosition);
    }

    // ✅ 같은 노드 및 이전 노드의 오퍼시티 낮추기
    void UpdateStageOpacity()
    {
        foreach (Transform child in stageContainer)
        {
            StageButton button = child.GetComponent<StageButton>();

            if (button != null)
            {
                float opacity = (button.GetStageData() == currentStage || currentStage.nextStages.Contains(button.GetStageData())) ? 1f : 0.5f;
                CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();

                if (canvasGroup == null)
                {
                    canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
                }

                canvasGroup.alpha = opacity;
            }
        }
    }
    void MoveToStage(StageNode newStage)
    {
        if (stageMapManager != null)
        {
            Debug.Log($"✅ StageUIManager에서 MoveToStage() 호출됨: {newStage.level}");
            stageMapManager.MoveToStage(newStage);
        }
    }
}
