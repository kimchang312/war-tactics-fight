using UnityEngine;
using System.Collections.Generic;

public class UIGenerator : MonoBehaviour
{
    [Header("Map & UI References")]
    public MapGenerator mapGenerator;    // 에디터에서 할당
    public RectTransform mapPanel;       // Canvas 내 배치 패널
    public GameObject stageNodePrefab;   // StageNodeUI 프리팹

    [Header("Connection Line Settings")]
    public GameObject connectionLinePrefab;
    public RectTransform connectionParent;
    public float lineThickness = 5f;

    [Header("Spacing Settings")]
    public float horizontalSpacing = 150f;
    public float verticalSpacing = 150f;

    [Header("Margin Settings")]
    public Vector2 startMargin = new Vector2(100f, -50f);

    // key: "level_row" -> value: StageNodeUI 인스턴스
    private Dictionary<string, StageNodeUI> stageUIMap = new Dictionary<string, StageNodeUI>();

    private void Awake()
    {
        mapGenerator.GeneratePathsNonCrossing();

        CreateUIMap();
        LinkUIConnections();
        DrawAllConnectionLines();
    }
    void Start()
    {
        if (mapGenerator == null) return;


        // ← UI 생성 직후, 반드시 초기 잠금/언락을 수행
        if (GameManager.Instance != null)
            GameManager.Instance.InitializeStageLocks();
    }

    /// <summary>
    /// 1) 모든 스테이지 UI 생성 및 배치, stageUIMap에 저장
    /// </summary>
    void CreateUIMap()
    {
        stageUIMap.Clear();

        foreach (var kvp in mapGenerator.NodeDictionary)
        {
            var node = kvp.Value;
            string key = $"{node.level}_{node.row}";

            // 프리팹 인스턴스화
            var go = Instantiate(stageNodePrefab, mapPanel);
            var ui = go.GetComponent<StageNodeUI>();
            ui.Setup(node);
            // **여기서 이름을 변경합니다.**
            go.name = $"Stage_{node.level}_{node.row}";
            // 위치
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(
                startMargin.x + node.level * horizontalSpacing,
                startMargin.y - node.row * verticalSpacing
            );

            stageUIMap[key] = ui;
        }
    }

    /// <summary>
    /// 2) mapGenerator의 연결정보를 읽어 StageNodeUI.connectedStages에 할당
    /// </summary>
    void LinkUIConnections()
    {
        foreach (var kvp in mapGenerator.NodeDictionary)
        {
            string fromKey = $"{kvp.Value.level}_{kvp.Value.row}";
            if (!stageUIMap.TryGetValue(fromKey, out var fromUI))
                continue;

            foreach (var nextNode in kvp.Value.connectedNodes)
            {
                string toKey = $"{nextNode.level}_{nextNode.row}";
                if (stageUIMap.TryGetValue(toKey, out var toUI))
                {
                    fromUI.connectedStages.Add(toUI);
                }
            }
        }
    }

    /// <summary>
    /// 3) UI들 간 연결선을 그림
    /// </summary>
    void DrawAllConnectionLines()
    {
        foreach (var ui in stageUIMap.Values)
        {
            var fromRt = ui.GetComponent<RectTransform>();
            foreach (var toUI in ui.connectedStages)
            {
                var toRt = toUI.GetComponent<RectTransform>();
                DrawEdgeConnectionLine(fromRt, toRt);
            }
        }
    }

    void DrawEdgeConnectionLine(RectTransform fromRect, RectTransform toRect)
    {
        // (기존에 쓰시던 엣지 계산 코드 그대로)
        Vector3 fromCenter = fromRect.TransformPoint(fromRect.rect.center);
        Vector3 toCenter = toRect.TransformPoint(toRect.rect.center);

        Vector3 fromEdge = fromCenter.x < toCenter.x
            ? fromRect.TransformPoint(new Vector3(fromRect.rect.xMax, fromRect.rect.center.y, 0))
            : fromRect.TransformPoint(new Vector3(fromRect.rect.xMin, fromRect.rect.center.y, 0));

        Vector3 toEdge = fromCenter.x < toCenter.x
            ? toRect.TransformPoint(new Vector3(toRect.rect.xMin, toRect.rect.center.y, 0))
            : toRect.TransformPoint(new Vector3(toRect.rect.xMax, toRect.rect.center.y, 0));

        Vector2 start = WorldToLocal(connectionParent, fromEdge);
        Vector2 end = WorldToLocal(connectionParent, toEdge);
        DrawConnectionLine(start, end);
    }

    Vector2 WorldToLocal(RectTransform parent, Vector3 worldPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent,
            Camera.main.WorldToScreenPoint(worldPos),
            Camera.main,
            out var local
        );
        return local;
    }

    void DrawConnectionLine(Vector2 a, Vector2 b)
    {
        var lineGO = Instantiate(connectionLinePrefab, connectionParent);
        var rt = lineGO.GetComponent<RectTransform>();
        var dir = b - a;
        rt.sizeDelta = new Vector2(dir.magnitude, lineThickness);
        rt.anchoredPosition = a + dir * 0.5f;
        rt.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }
}
