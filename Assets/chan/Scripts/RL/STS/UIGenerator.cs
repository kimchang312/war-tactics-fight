using UnityEngine;
using System.Collections.Generic;

public class UIGenerator : MonoBehaviour
{
    [Header("Map & UI References")]
    public MapGenerator mapGenerator;         // MapGenerator 스크립트를 참조 (에디터에서 할당)
    public RectTransform mapPanel;              // 스테이지 UI들을 배치할 부모 Panel (Canvas 내)
    public GameObject stageNodePrefab;          // StageNodeUI 프리팹 (에디터에서 할당)

    [Header("Connection Line Settings")]
    public GameObject connectionLinePrefab;     // 연결 선 프리팹 (얇은 UI Image 오브젝트)
    public RectTransform connectionParent;      // 연결 선들을 배치할 부모 (예: mapPanel 하위 오브젝트)
    public float lineThickness = 5f;            // 연결 선의 두께

    [Header("Spacing Settings")]
    public float horizontalSpacing = 150f;      // 스테이지 간 수평 간격
    public float verticalSpacing = 150f;        // 스테이지 간 수직 간격

    [Header("Margin Settings")]
    // 좌상단 기준으로 오프셋 (mapPanel의 Pivot이 좌상단(0,1)인 경우, 
    // x 값은 오른쪽으로, y 값은 아래쪽으로 이동)
    public Vector2 startMargin = new Vector2(100f, -50f);

    // 각 스테이지 UI 요소의 RectTransform을 "level_row" 키로 저장하는 Dictionary
    private Dictionary<string, RectTransform> stageUIPositions = new Dictionary<string, RectTransform>();

    void Start()
    {
        // MapGenerator는 경로 생성만 담당하므로, UIGenerator는 MapGenerator의 NodeDictionary를 이용해 UI만 생성합니다.
        if (mapGenerator != null)
        {
            // MapGenerator가 Start()에서 경로를 생성했다고 가정합니다.
            CreateUIMap();
            DrawAllConnectionLines();
        }
    }

    /// <summary>
    /// MapGenerator의 데이터를 기반으로 StageNodeUI 프리팹을 인스턴스화하고 mapPanel에 배치합니다.
    /// </summary>
    void CreateUIMap()
    {
        stageUIPositions.Clear();

        foreach (var kvp in mapGenerator.NodeDictionary)
        {
            StageNode nodeData = kvp.Value;
            GameObject stageGO = Instantiate(stageNodePrefab, mapPanel);
            StageNodeUI stageUI = stageGO.GetComponent<StageNodeUI>();
            stageUI.Setup(nodeData);

            // 위치 계산: 기존 계산에 startMargin 추가
            RectTransform rt = stageGO.GetComponent<RectTransform>();
            float x = startMargin.x + nodeData.level * horizontalSpacing;
            float y = startMargin.y - nodeData.row * verticalSpacing;
            rt.anchoredPosition = new Vector2(x, y);

            // "level_row" 형식의 키로 저장 (예: "3_2")
            string key = nodeData.level + "_" + nodeData.row;
            if (!stageUIPositions.ContainsKey(key))
                stageUIPositions.Add(key, rt);
        }
    }

    /// <summary>
    /// MapGenerator의 NodeDictionary를 순회하여, 각 노드 간 연결 선을 그립니다.
    /// </summary>
    void DrawAllConnectionLines()
    {
        foreach (var kvp in mapGenerator.NodeDictionary)
        {
            StageNode currentNode = kvp.Value;
            string currentKey = kvp.Key;
            if (!stageUIPositions.ContainsKey(currentKey))
                continue;

            RectTransform currentRect = stageUIPositions[currentKey];

            foreach (var nextNode in currentNode.connectedNodes)
            {
                string nextKey = nextNode.level + "_" + nextNode.row;
                if (!stageUIPositions.ContainsKey(nextKey))
                    continue;

                RectTransform nextRect = stageUIPositions[nextKey];
                DrawEdgeConnectionLine(currentRect, nextRect);
            }
        }
    }

    /// <summary>
    /// 두 UI 요소의 엣지를 연결하는 선을 그립니다.
    /// 왼쪽에 있는 버튼은 오른쪽 끝, 오른쪽에 있는 버튼은 왼쪽 끝을 사용합니다.
    /// </summary>
    /// <param name="fromRect">시작 UI 요소의 RectTransform</param>
    /// <param name="toRect">종료 UI 요소의 RectTransform</param>
    void DrawEdgeConnectionLine(RectTransform fromRect, RectTransform toRect)
    {
        // 계산할 때, 버튼의 엣지 좌표를 먼저 월드 좌표로 구합니다.
        Vector3 fromCenter = fromRect.TransformPoint(fromRect.rect.center);
        Vector3 toCenter = toRect.TransformPoint(toRect.rect.center);

        Vector3 fromEdgeWorld, toEdgeWorld;

        // 두 버튼의 x 좌표 비교해서, 왼쪽 버튼은 오른쪽 끝, 오른쪽 버튼은 왼쪽 끝을 사용
        if (fromCenter.x < toCenter.x)
        {
            // fromRect: 오른쪽 끝 → fromCenter + (width/2, 0) (월드 좌표로 변환)
            fromEdgeWorld = fromRect.TransformPoint(new Vector3(fromRect.rect.xMax, fromRect.rect.center.y, 0));
            // toRect: 왼쪽 끝 → toCenter - (width/2, 0)
            toEdgeWorld = toRect.TransformPoint(new Vector3(toRect.rect.xMin, toRect.rect.center.y, 0));
        }
        else
        {
            fromEdgeWorld = fromRect.TransformPoint(new Vector3(fromRect.rect.xMin, fromRect.rect.center.y, 0));
            toEdgeWorld = toRect.TransformPoint(new Vector3(toRect.rect.xMax, toRect.rect.center.y, 0));
        }

        // 월드 좌표를 connectionParent의 로컬 좌표로 변환
        Vector2 startPos = WorldToLocal(connectionParent, fromEdgeWorld);
        Vector2 endPos = WorldToLocal(connectionParent, toEdgeWorld);

        DrawConnectionLine(startPos, endPos);
    }

    Vector2 WorldToLocal(RectTransform parent, Vector3 worldPos)
    {
        Vector2 localPos;
        // 카메라를 지정해 주거나 null이면 Screen Space - Overlay의 경우 동작
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, Camera.main.WorldToScreenPoint(worldPos), Camera.main, out localPos);
        return localPos;
    }


    /// <summary>
    /// 두 점 사이에 선을 그립니다.
    /// </summary>
    /// <param name="startPos">시작 점 (버튼의 엣지 위치)</param>
    /// <param name="endPos">종료 점 (버튼의 엣지 위치)</param>
    void DrawConnectionLine(Vector2 startPos, Vector2 endPos)
    {
        GameObject lineGO = Instantiate(connectionLinePrefab, connectionParent);
        RectTransform lineRect = lineGO.GetComponent<RectTransform>();

        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;

        lineRect.sizeDelta = new Vector2(distance, lineThickness);
        lineRect.anchoredPosition = startPos + direction / 2f;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        lineRect.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
