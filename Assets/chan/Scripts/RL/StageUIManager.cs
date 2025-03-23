using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Map;  // MapData, MapConfig, MapLayer, NodeBlueprint, NodeType 등이 포함됨
using DG.Tweening;
using System;

public class StageUIManager : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform stageContainer;       // 노드 버튼들을 배치할 컨테이너
    // stageButtonPrefab는 더 이상 사용하지 않습니다.
    public GameObject uiLinePrefab;              // UI Image 기반 선 프리팹
    public Transform lineParent;

    [Header("Scroll View & Marker")]
    public RectTransform content;              // Scroll View의 Content
    public ScrollRect scrollRect;
    public GameObject markerPrefab;            // 현재 위치를 표시할 Marker 프리팹

    [Header("Other UI Elements")]
    public StageTooltip stageTooltip;

    private List<StageNode> stageNodes = new List<StageNode>();
    private List<StageNode> allStages;
    private StageNode currentStage;

    public StageMapManager stageMapManager;

    private readonly List<LineConnection> lineConnections = new List<LineConnection>();

    // 중복 구독 방지를 위한 플래그
    private bool eventsSubscribed = false;

    private void Awake()
    {
        // 만약 lineParent가 할당되지 않았다면, stageContainer의 자식으로 새로 생성
        if (lineParent == null)
        {
            GameObject lp = new GameObject("LineParent");
            lp.transform.SetParent(stageContainer, false);
            lineParent = lp.transform;
            Debug.LogWarning("StageUIManager: lineParent가 할당되지 않음. 새 LineParent 생성됨.");
        }

        if (scrollRect != null)
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);

        // 중복 구독 방지를 위해 플래그 확인
        if (!eventsSubscribed)
        {
            StageButton.OnStageButtonClicked += MoveToStage;
            StageMapManager.OnMapGenerated += GenerateStageUI;
            StageMapManager.OnStageChanged += UpdateStageUI;
            eventsSubscribed = true;
        }

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

    /// <summary>
    /// StageMapManager에서 생성한 StageNode 오브젝트들을, GridGenerator에서 생성한 격자 셀에 재배치합니다.
    /// 별도의 프리팹 인스턴스화를 하지 않고, StageNode 오브젝트 자체를 재사용합니다.
    /// </summary>
    void GenerateStageUI(List<StageNode> nodes)
    {
        Debug.Log("Stage UI 생성 시작");

        // stageContainer의 자식들 중 "GridGenerator" 오브젝트는 유지하고, 나머지는 삭제
        List<Transform> childrenToDelete = new List<Transform>();
        foreach (Transform child in stageContainer)
        {
            if (child.name != "GridGenerator")
                childrenToDelete.Add(child);
        }
        foreach (Transform child in childrenToDelete)
            Destroy(child.gameObject);
        foreach (Transform child in lineParent)
            Destroy(child.gameObject);

        allStages = nodes;
        stageNodes = nodes;

        // StageUIManager는 stageContainer 내의 "GridGenerator" 오브젝트 아래에 있는 격자 셀을 참조합니다.
        Transform gridGen = stageContainer.Find("GridGenerator");
        if (gridGen == null)
        {
            Debug.LogWarning("GridGenerator 오브젝트를 stageContainer에서 찾을 수 없습니다.");
        }
        else
        {
            Debug.Log("GridGenerator 오브젝트를 찾았습니다: " + gridGen.name);
        }

        foreach (StageNode node in stageNodes)
        {
            Debug.Log($"[Before Reparent] {node.name} 현재 부모: {(node.transform.parent != null ? node.transform.parent.name : "null")}, gridID: {node.gridID}, 위치: {node.position}");

            // 레벨(행)이 config.layers.Count보다 큰 경우(예: Boss 등)는 기본 stageContainer 사용
            Transform gridContainer = null;
            if (node.floor > stageMapManager.config.layers.Count)
            {
                gridContainer = stageContainer;
                Debug.Log($"노드 {node.name}은(는) Boss 등 격자 범위를 벗어남. 기본 stageContainer 사용.");
            }
            else
            {
                gridContainer = gridGen != null ? gridGen.Find(node.gridID) : stageContainer;
            }

            if (gridContainer == null)
            {
                Debug.LogError($"그리드 컨테이너 '{node.gridID}'를 찾을 수 없습니다.");
                continue;
            }
            else
            {
                Debug.Log($"노드 {node.name}의 gridContainer로 '{gridContainer.name}'를 찾았습니다.");
            }

            // StageNode 오브젝트를 해당 격자 셀의 자식으로 재배치
            node.transform.SetParent(gridContainer, false);
            Debug.Log($"[After Reparent] {node.name}의 새 부모: {node.transform.parent.name}");

            // 격자 셀의 중앙에 배치 (anchoredPosition = (0,0))
            RectTransform nodeRect = node.GetComponent<RectTransform>();
            if (nodeRect != null)
            {
                nodeRect.anchoredPosition = Vector2.zero;
                Debug.Log($"노드 {node.name}의 anchoredPosition 재설정됨.");
            }
            else
            {
                Debug.LogWarning($"노드 {node.name}에 RectTransform이 없습니다.");
            }

            // Level 1 노드를 currentStage로 지정하고 마커 위치 업데이트
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

        // 선 연결 정보 저장 (여기서는 실제 LineRenderer, UILineRenderer는 사용하지 않으므로 null)
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
        // 부드러운 이동을 위해 Tween 사용 (주석 처리되어 있으면 직접 업데이트)
        // markerRect.DOAnchorPos(pos, 0.3f);
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
