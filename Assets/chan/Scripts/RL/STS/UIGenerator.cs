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
    public RectTransform connectionParent;      // 연결 선들을 배치할 부모 (예: mapPanel 하위)
    public float lineThickness = 5f;            // 연결 선의 두께

    [Header("Spacing Settings")]
    public float horizontalSpacing = 150f;      // 스테이지 간 수평 간격
    public float verticalSpacing = 150f;        // 스테이지 간 수직 간격

    // 각 스테이지 UI 요소의 RectTransform을 "level_row" 키로 저장하는 Dictionary
    private Dictionary<string, RectTransform> stageUIPositions = new Dictionary<string, RectTransform>();

    void Start()
    {
        if (mapGenerator != null)
        {
            mapGenerator.GenerateMap();
            CreateUIMap();
            DrawAllConnectionLines();
        }
    }

    /// <summary>
    /// MapGenerator의 데이터를 기반으로 StageNodeUI 프리팹을 인스턴스화하고 배치합니다.
    /// </summary>
    void CreateUIMap()
    {
        stageUIPositions.Clear();

        // MapGenerator에서 생성한 모든 노드를 순회
        foreach (var kvp in mapGenerator.NodeDictionary)
        {
            StageNode nodeData = kvp.Value;
            // StageNodeUI 프리팹 생성
            GameObject stageGO = Instantiate(stageNodePrefab, mapPanel);
            StageNodeUI stageUI = stageGO.GetComponent<StageNodeUI>();
            // MapGenerator의 level, row, stageType 정보를 StageNodeUI에 전달
            stageUI.Setup(nodeData);

            // 위치 계산: 레벨에 따라 수평, 행에 따라 수직 배치
            RectTransform rt = stageGO.GetComponent<RectTransform>();
            float x = nodeData.level * horizontalSpacing;
            float y = -(nodeData.row * verticalSpacing);
            rt.anchoredPosition = new Vector2(x, y);

            // Dictionary에 저장 (키 예: "3_2")
            string key = nodeData.level + "_" + nodeData.row;
            if (!stageUIPositions.ContainsKey(key))
                stageUIPositions.Add(key, rt);
        }
    }

    /// <summary>
    /// MapGenerator의 NodeDictionary를 순회하며, 각 노드 간 연결 선을 그립니다.
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
                DrawConnectionLine(currentRect.anchoredPosition, nextRect.anchoredPosition);
            }
        }
    }

    /// <summary>
    /// 두 UI 요소 사이에 선을 그립니다.
    /// </summary>
    /// <param name="startPos">시작 UI 요소의 anchoredPosition</param>
    /// <param name="endPos">종료 UI 요소의 anchoredPosition</param>
    void DrawConnectionLine(Vector2 startPos, Vector2 endPos)
    {
        // 연결 선 프리팹 인스턴스화
        GameObject lineGO = Instantiate(connectionLinePrefab, connectionParent);
        RectTransform lineRect = lineGO.GetComponent<RectTransform>();

        // 두 점 사이의 벡터 및 거리 계산
        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;

        // 선의 크기 설정 (너비 = 두 점 사이 거리, 높이 = 선 두께)
        lineRect.sizeDelta = new Vector2(distance, lineThickness);

        // 선의 중앙 위치 (두 점의 중간)
        lineRect.anchoredPosition = startPos + direction / 2f;

        // 선의 회전 설정 (아크탄젠트를 이용하여 각도 계산)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        lineRect.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
