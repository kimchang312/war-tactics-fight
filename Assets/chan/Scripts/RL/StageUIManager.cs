using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Map;

public class StageUIManager : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform stageContainer;
    public GameObject stageButtonPrefab;
    public GameObject uiLinePrefab; // UI Image 기반 선 프리팹
    public Transform lineParent;

    [Header("Scroll View & Marker")]
    public RectTransform content;
    public ScrollRect scrollRect;
    public GameObject markerPrefab;

    [Header("Other UI Elements")]
    public StageTooltip stageTooltip;

    private List<StageNode> stageNodes = new List<StageNode>();
    private List<StageNode> allStages;
    private StageNode currentStage;

    public StageMapManager stageMapManager;

    private readonly List<LineConnection> lineConnections = new List<LineConnection>();

    private void Awake()
    {
        if (lineParent == null)
        {
            GameObject lp = new GameObject("LineParent");
            lp.transform.SetParent(stageContainer, false);
            lineParent = lp.transform;
            Debug.LogWarning("StageUIManager: lineParent가 할당되지 않음. 새 LineParent 생성됨.");
        }

        if (scrollRect != null)
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);

        StageButton.OnStageButtonClicked += MoveToStage;
        StageMapManager.OnMapGenerated += GenerateStageUI;
        StageMapManager.OnStageChanged += UpdateStageUI;

        Debug.Log(markerPrefab == null ? "🟢 Marker is null" : "🔴 Marker already exists");
    }

    private void Start()
    {
        SetupScrollView();
        if (currentStage != null)
            UpdateMarkerPosition(currentStage.position);
    }

    private void OnDestroy()
    {
        StageButton.OnStageButtonClicked -= MoveToStage;
    }

    private void OnDisable()
    {
        StageMapManager.OnMapGenerated -= GenerateStageUI;
        StageMapManager.OnStageChanged -= UpdateStageUI;
    }

    public StageNode GetCurrentStage()
    {
        return stageMapManager != null ? stageMapManager.GetCurrentStage() : null;
    }

    public void InitializeUI(List<StageNode> nodes)
    {
        if (nodes == null || nodes.Count == 0)
        {
            Debug.LogError("StageUIManager.InitializeUI() 실패: 전달된 StageNode 리스트가 null이거나 비어 있음.");
            return;
        }
        allStages = nodes;
        Debug.Log($"StageUIManager: {allStages.Count}개의 스테이지 정상 수신.");
    }

    void GenerateStageUI(List<StageNode> nodes)
    {
        Debug.Log("Stage UI 생성 시작");

        foreach (Transform child in stageContainer)
            Destroy(child.gameObject);
        foreach (Transform child in lineParent)
            Destroy(child.gameObject);

        allStages = nodes;
        stageNodes = nodes;

        foreach (StageNode node in stageNodes)
        {
            Debug.Log($"스테이지 버튼 생성: 층 {node.floor}, gridID {node.gridID}, 위치 {node.position}");

            Transform gridContainer = stageContainer.Find(node.gridID);
            if (gridContainer == null)
            {
                Debug.LogWarning($"그리드 컨테이너 '{node.gridID}'를 찾을 수 없음. 기본 stageContainer 사용.");
                gridContainer = stageContainer;
            }

            GameObject btnObj = Instantiate(stageButtonPrefab, gridContainer);
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            Vector2 randomOffset = new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f));
            rt.anchoredPosition = randomOffset;

            StageButton sb = btnObj.GetComponent<StageButton>();
            if (sb != null)
                sb.SetStageNode(node);
            else
                Debug.LogError("StageButton 컴포넌트 없음! 프리팹 확인.");

            StageUIComponent uiComp = btnObj.GetComponent<StageUIComponent>();
            if (uiComp != null)
                node.uiComponent = uiComp;
            else
                Debug.LogError($"{node.nodeName}의 StageUIComponent 없음! 프리팹 확인.");

            if (node.floor == 1)
            {
                currentStage = node;
                UpdateMarkerPosition(node.position);
            }
        }
        DrawAllConnections();
        Debug.Log("Stage UI 생성 완료");
    }

    void DrawAllConnections()
    {
        foreach (Transform child in lineParent)
            Destroy(child.gameObject);
        foreach (StageNode node in stageNodes)
        {
            foreach (Vector2Int p in node.outgoing)
            {
                StageNode target = allStages.FirstOrDefault(n => n.point.Equals(p));
                if (target != null)
                    AddLineConnection(node, target);
            }
        }
    }

    void AddLineConnection(StageNode from, StageNode to)
    {
        if (uiLinePrefab == null) return;
        GameObject lineObj = Instantiate(uiLinePrefab, lineParent);
        RectTransform rt = lineObj.GetComponent<RectTransform>();
        Vector2 fromPos = from.position;
        Vector2 toPos = to.position;
        Vector2 diff = toPos - fromPos;
        float distance = diff.magnitude;
        float thickness = rt.sizeDelta.y;
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = fromPos;
        rt.sizeDelta = new Vector2(distance, thickness);
        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        rt.rotation = Quaternion.Euler(0, 0, angle);

        // 선 연결 정보 저장 (여기서는 LineRenderer는 사용하지 않으므로 null)
        lineConnections.Add(new LineConnection(null, null, from, to));
    }

    void SetupScrollView()
    {
        if (scrollRect != null)
            scrollRect.horizontalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
    }

    void UpdateMarkerPosition(Vector2 pos)
    {
        if (markerPrefab == null || stageContainer == null)
        {
            Debug.LogError("StageContainer 또는 Marker 미설정.");
            return;
        }
        RectTransform markerRect = markerPrefab.GetComponent<RectTransform>();
        markerRect.anchoredPosition = pos;
        Debug.Log($"마커 위치 업데이트: {pos}");
    }

    public void OnScrollValueChanged(Vector2 scrollPos)
    {
        if (currentStage == null || markerPrefab == null)
        {
            Debug.LogWarning("현재 스테이지 또는 Marker 미설정.");
            return;
        }
        UpdateMarkerPosition(currentStage.position);
    }

    public void UpdateStageUI(StageNode newStage)
    {
        if (newStage == null)
        {
            Debug.LogError("UpdateStageUI() 실패: newStage가 null.");
            return;
        }
        Debug.Log("스테이지 이동: " + newStage.nodeName);
        currentStage = newStage;
        if (currentStage == null)
        {
            Debug.LogError("UI 업데이트 실패: currentStage가 null.");
            return;
        }
        StageUIComponent comp = newStage.uiComponent;
        if (comp == null)
        {
            Debug.LogError($"{newStage.nodeName}의 UI 컴포넌트가 존재하지 않음!");
            return;
        }
        UpdateMarkerPosition(currentStage.position);
        foreach (StageNode node in stageNodes)
        {
            bool active = (node == currentStage || currentStage.outgoing.Contains(node.point));
            node.uiComponent?.SetInteractable(active);
        }
        UpdateStageOpacity();
    }

    void UpdateStageOpacity()
    {
        foreach (Transform container in stageContainer)
        {
            foreach (Transform child in container)
            {
                StageButton button = child.GetComponent<StageButton>();
                if (button != null)
                {
                    bool isActive = (button.GetStageData() == currentStage || currentStage.outgoing.Contains(button.GetStageData().point));
                    float opacity = isActive ? 1f : 0.5f;
                    CanvasGroup cg = button.GetComponent<CanvasGroup>();
                    if (cg == null)
                        cg = button.gameObject.AddComponent<CanvasGroup>();
                    cg.alpha = opacity;
                }
            }
        }
    }

    public void MoveToStage(StageNode newStage)
    {
        if (newStage == null)
        {
            Debug.LogWarning("이동할 스테이지 없음!");
            return;
        }
        if (stageMapManager != null)
            stageMapManager.MoveToStage(newStage);
        currentStage = newStage;
        UpdateMarkerPosition(newStage.position);
    }
}
