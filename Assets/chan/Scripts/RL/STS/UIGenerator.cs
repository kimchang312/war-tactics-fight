using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using DG.Tweening;

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
    public Vector2 startMargin = new Vector2(80f, -70f);

    // key: "level_row" -> value: StageNodeUI 인스턴스
    private Dictionary<string, StageNodeUI> stageUIMap = new Dictionary<string, StageNodeUI>();



    private void Start()
    {

        if (!GameManager.Instance._hasInitialized)
        {
            GameManager.Instance._hasInitialized = true;

            RegenerateMap();
        }

    }
    /// 외부에서 언제든 호출해서 맵 전체를 지우고 다시 생성합니다.
    /// - New Game 버튼
    /// - 보스 클리어 후
    /// - 게임 재시작
    public void RegenerateMap()
    {
        // 0) 기존 UI 모두 제거
        ClearUI();
        if(mapGenerator ==null)
            mapGenerator = FindAnyObjectByType<MapGenerator>();
        // 1) 경로 생성
        mapGenerator.GeneratePathsNonCrossing();

        // 2) 노드 UI 생성
        CreateUIMap();

        // 3) 노드 연결
        LinkUIConnections();

        // 4) 연결선 그리기
        DrawAllConnectionLines();

        // 5) 잠금/언락 초기화
        if (GameManager.Instance != null)
            GameManager.Instance.InitializeStageLocks();
        EnsurePlayerMarker();
    }
    /*public void RegenerateMapFromSave(StageSaveData savedData)
    {
        mapGenerator.ClearNodes();
        mapGenerator.NodeDictionary.Clear();

        StageNode current = new()
        {
            level = savedData.level,
            row = savedData.row,
            stageType = Enum.Parse<StageType>(savedData.stageType)
        };
        string key = $"{current.level}_{current.row}";
        mapGenerator.NodeDictionary.Add(key, current);

        foreach (var conn in savedData.connections)
        {
            StageNode next = new()
            {
                level = conn.level,
                row = conn.row,
                stageType = StageType.Combat // 또는 저장 필요
            };
            mapGenerator.NodeDictionary.Add($"{next.level}_{next.row}", next);
            current.connectedNodes.Add(next);
        }

        uIGenerator.RegenerateMap();
        GameManager.Instance.currentStage = current;
    }*/

    private void ClearUI()
    {
        // mapPanel 하위의 모든 자식 오브젝트 검사
        for (int i = mapPanel.childCount - 1; i >= 0; i--)
        {
            var child = mapPanel.GetChild(i);
            var isMarker = child.gameObject.GetComponent<PlayerMarkerTag>() != null;
            if (DOTween.IsTweening(child))
            {
                DOTween.Kill(child, true);  // true: 완전히 제거
            }
            Destroy(child.gameObject);
        }

        // ② 연결선 전부 파괴
        for (int i = connectionParent.childCount - 1; i >= 0; i--)
            Destroy(connectionParent.GetChild(i).gameObject);
        
    }
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
    public void EnsurePlayerMarker()
    {
        if (GameManager.Instance.playerMarker != null) return;

        var prefab = GameManager.Instance.playerMarkerPrefab;
        var markerGO = Instantiate(prefab, mapPanel);
        var markerRT = markerGO.GetComponent<RectTransform>();

        markerRT.SetAsLastSibling();
        markerRT.anchoredPosition = Vector2.zero; // 기본 위치 (후에 SetCurrentStage에서 이동)

        GameManager.Instance.playerMarker = markerRT;

        // ✅ 생성 직후 비활성화
        markerRT.gameObject.SetActive(false);

        Debug.Log("✅ PlayerMarker 프리팹 생성 및 mapPanel에 부착됨");
    }
}
