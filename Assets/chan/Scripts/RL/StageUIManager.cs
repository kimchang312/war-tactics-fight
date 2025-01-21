using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageUIManager : MonoBehaviour
{
    public GameObject stageButtonPrefab;
    public RectTransform stageContainer;
    public RectTransform content; // Scroll View의 Content
    public StageTooltip stageTooltip;
    public GameObject markerPrefab; // ✅ 현재 위치를 표시할 마커
    public ScrollRect scrollRect; //scroll rect 참조

    private int levels = 15; // 총 레벨 수
    private float screenWidth = 1920f; // 화면 너비
    private float ySpacing = 200f; // Y축 간격

    private StageNode currentStage;
    public StageMapManager stageMapManager; // ✅ StageMapManager 참조

    public StageNode GetCurrentStage()
    {
        return stageMapManager != null ? stageMapManager.GetCurrentStage() : null;
    }

    private void Awake()
    {

        Debug.Log(markerPrefab == null ? "🟢 Marker is null" : "🔴 Marker already exists");
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }

        StageButton.OnStageButtonClicked += MoveToStage;
        StageMapManager.OnStageGenerated += GenerateStageUI;
        StageMapManager.OnStageChanged += UpdateStageUI;
    }
    private void Start()
    {
        SetupScrollView();

        if (currentStage != null)
        {
            UpdateMarkerPosition(currentStage.position); // 초기 위치에 마커 설정
        }
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

        GenerateMarker(); // 마커 생성

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
    private void SetupScrollView()
    {
        if (scrollRect != null)
        {
            // 초기 스크롤 위치를 가장 왼쪽으로 설정
            scrollRect.horizontalNormalizedPosition = 0f;
        }

        

        Canvas.ForceUpdateCanvases(); // Canvas 강제 업데이트
    }
    /*
    void GenerateStageUI(List<StageNode> allStages)
    {
        Debug.Log("🔵 Stage UI 생성 시작");

        // Content 크기 설정
        content.sizeDelta = new Vector2(levels * screenWidth, content.sizeDelta.y);

        foreach (StageNode stage in allStages)
        {
            Debug.Log($"🟡 스테이지 버튼 생성: 레벨 {stage.level}, 위치 {stage.position}");

            GameObject buttonObj = Instantiate(stageButtonPrefab, content);
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();

            // X축은 레벨에 따라 이동, Y축은 스테이지 배치
            Vector2 position = new Vector2((stage.level - 1) * screenWidth, 425f - stage.position.y);
            buttonRect.anchoredPosition = position;

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
                UpdateMarkerPosition(position);
            }
        }

        Debug.Log("🟢 Stage UI 생성 완료");
    }*/

    void UpdateStageUI(StageNode newStage)
    {
        Debug.Log("🔵 스테이지 이동: " + newStage.level);

        currentStage = newStage;

        // 스크롤된 Content의 위치를 반영하여 마커 위치 업데이트
        UpdateMarkerPosition(newStage.position);
        UpdateStageOpacity();
    }

    // ✅ 현재 위치 마커 이동 함수
    void UpdateMarkerPosition(Vector2 stagePosition)
    {
        if (markerPrefab == null || stageContainer == null)
        {
            Debug.LogError("❌ StageContainer 또는 마커가 설정되지 않았습니다.");
            return;
        }

        // StageContainer 좌표계를 기준으로 마커 위치 설정
        RectTransform markerRect = markerPrefab.GetComponent<RectTransform>();
        markerRect.anchoredPosition = stagePosition;

        Debug.Log($"📍 마커 위치 업데이트 완료: {stagePosition}");
    }

    public void OnScrollValueChanged(Vector2 scrollPosition)
    {
        if (currentStage == null || markerPrefab == null)
        {
            Debug.LogWarning("❌ 현재 스테이지 또는 마커가 설정되지 않았습니다.");
            return;
        }

        // 선택된 스테이지의 위치로 마커 이동
        UpdateMarkerPosition(currentStage.position);
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
        if (newStage == null)
        {
            Debug.LogWarning("❌ 이동할 스테이지가 없습니다!");
            return;
        }
        stageMapManager.MoveToStage(newStage);
        currentStage = newStage;

        // 스크롤된 Content의 위치를 반영하여 마커 위치 업데이트
        UpdateMarkerPosition(newStage.position);
    }
    void GenerateMarker()
    {
        

        if (markerPrefab == null)
        {
            Debug.LogError("❌ Marker Prefab이 설정되지 않았습니다.");
            return;
        }

        // 마커 프리팹 생성
        markerPrefab = Instantiate(markerPrefab, stageContainer);

        RectTransform markerRect = markerPrefab.GetComponent<RectTransform>();
        markerRect.anchorMin = new Vector2(0.5f, 0.5f);
        markerRect.anchorMax = new Vector2(0.5f, 0.5f);
        markerRect.pivot = new Vector2(0.5f, 0.5f);
        markerRect.anchoredPosition = Vector2.zero;

        Debug.Log($"✅ 새로운 마커 생성 완료: {markerPrefab.name}");
    }

}
