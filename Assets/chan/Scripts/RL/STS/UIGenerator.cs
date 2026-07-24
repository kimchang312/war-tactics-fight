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



    /// <summary>RLmap이 언로드되면 씬에만 있던 MapGenerator가 파괴되고, DDOL에 남은 UIGenerator의 참조가 끊깁니다. 씬 재진입 후 호출하세요.</summary>
    public void EnsureMapGeneratorReference()
    {
        if (mapGenerator == null)
            mapGenerator = FindAnyObjectByType<MapGenerator>();
    }

    /// <summary>
    /// GameManager/UIGenerator가 DDOL이라 맵 UI는 남고, RLmap만 다시 로드되면 새 MapGenerator의 nodeDict는 비어 있습니다.
    /// 남아 있는 <see cref="StageNodeUI"/>로 <see cref="MapGenerator"/> 노드 그래프를 다시 채웁니다(보물지도·저장 등).
    /// </summary>
    public void RehydrateMapFromExistingUIIfNeeded()
    {
        EnsureMapGeneratorReference();
        if (mapGenerator == null || mapPanel == null)
            return;
        if (mapGenerator.NodeDictionary != null && mapGenerator.NodeDictionary.Count > 0)
            return;

        StageNodeUI[] uis = mapPanel.GetComponentsInChildren<StageNodeUI>(true);
        if (uis == null || uis.Length == 0)
            return;

        var dict = new Dictionary<string, StageNode>();
        foreach (var ui in uis)
        {
            if (ui == null)
                continue;
            string key = $"{ui.level}_{ui.row}";
            var node = new StageNode(ui.level, ui.row, ui.stageType);
            node.presetID = ui.PresetID;
            node.battlefieldEffect = ui.battlefieldEffect;
            dict[key] = node;
        }

        foreach (var ui in uis)
        {
            if (ui == null)
                continue;
            if (!dict.TryGetValue($"{ui.level}_{ui.row}", out StageNode node))
                continue;
            foreach (var nxt in ui.connectedStages)
            {
                if (nxt == null)
                    continue;
                string nk = $"{nxt.level}_{nxt.row}";
                if (dict.TryGetValue(nk, out StageNode nextNode))
                    node.connectedNodes.Add(nextNode);
            }
        }

        mapGenerator.OverrideNodeDict(dict);
    }

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

        EnsureMapGeneratorReference();
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
    public void RegenerateMapFromSaveFull(StageFullSaveData savedData)
    {
        EnsureMapGeneratorReference();
        mapGenerator.ClearAll();
        mapGenerator.LoadSpecialPresetData(savedData.GetSpecialPresetData());
        var dict = new Dictionary<string, StageNode>();

        // 노드 생성
        foreach (var entry in savedData.allNodes)
        {
            StageNode node = new(entry.level, entry.row, entry.stageType);
            node.presetID = entry.presetID;
            node.battlefieldEffect = entry.battlefieldEffect;
            string key = $"{node.level}_{node.row}";
            dict[key] = node;
        }

        // 연결 복원
        foreach (var entry in savedData.allNodes)
        {
            string key = $"{entry.level}_{entry.row}";
            StageNode node = dict[key];
            foreach (var conn in entry.connections)
            {
                string connKey = $"{conn.level}_{conn.row}";
                if (dict.TryGetValue(connKey, out var next))
                    node.connectedNodes.Add(next);
            }
        }

        mapGenerator.OverrideNodeDict(dict);

        ClearUI();
        CreateUIMap();
        LinkUIConnections();
        DrawAllConnectionLines();
        GameManager.Instance.InitializeStageLocks();
        EnsurePlayerMarker();

        Debug.Log("🔁 저장된 맵으로 복원 완료");
    }

    private void ClearUI()
    {
        for (int i = mapPanel.childCount - 1; i >= 0; i--)
        {
            var child = mapPanel.GetChild(i);
            if (child.TryGetComponent<PlayerMarkerTag>(out _))
                continue; // 마커는 유지

            if (DOTween.IsTweening(child))
                DOTween.Kill(child, true);

            Destroy(child.gameObject);
        }

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
