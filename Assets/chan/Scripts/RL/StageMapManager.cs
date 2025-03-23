using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Map;  // MapData, MapConfig, MapLayer, NodeBlueprint, NodeType 등이 포함됨

public class StageMapManager : MonoBehaviour
{
    public StageUIManager stageUIManager; // StageUIManager의 참조 (Inspector에서 할당)
    public MapConfig config;  // 설정 데이터 (ScriptableObject)
    public float horizontalSpacing = 200f;  // 레벨(열) 간격 (x축)
    public float verticalSpacing = 150f;      // 행 간격 (y축)
    public GameObject stageNodePrefab; // Inspector에 할당할 StageNode 프리팹
    public MapData CurrentMap { get; private set; }

    // 최종 경로를 병합하여 구성한 노드들: 각 레벨별 StageNode 리스트 (총 15 레벨, 각 레벨 최대 7행)
    private List<List<StageNode>> columnNodes = new List<List<StageNode>>();

    // 독립 경로들을 임시 저장 (각 경로는 Level 1~15의 StageNode 시퀀스)
    private List<List<StageNode>> generatedPaths = new List<List<StageNode>>();

    private System.Random rnd = new System.Random();

    public static event Action<List<StageNode>> OnMapGenerated;
    public static event Action<StageNode> OnStageChanged;

    private void Start()
    {
        if (config == null)
        {
            Debug.LogError("Config is not assigned.");
            return;
        }
        GeneratePaths();
        AssignEncounters();
        ConnectBossRoom();

        if (columnNodes == null || columnNodes.Count == 0)
        {
            Debug.LogError("❌ 맵 노드가 생성되지 않았습니다.");
            return;
        }

        List<StageNode> nodesList = columnNodes.SelectMany(list => list).ToList();

        string bossName = config.nodeBlueprints.Where(b => b.nodeType == NodeType.Boss).ToList().Random().name;
        List<Node> dataNodes = nodesList.Select(n => n.ToNode()).ToList();
        CurrentMap = new MapData(config.name, bossName, dataNodes, new List<Vector2Int>());
        Debug.Log(CurrentMap.ToJson());

        OnMapGenerated?.Invoke(nodesList);
    }

    public StageNode GetCurrentStage()
    {
        return columnNodes.FirstOrDefault()?.FirstOrDefault();
    }

    public void MoveToStage(StageNode newStage)
    {
        if (newStage == null)
        {
            Debug.LogError("MoveToStage() 호출 실패: newStage가 null입니다!");
            return;
        }
        Debug.Log($"스테이지 이동: {newStage.nodeName}");
        OnStageChanged?.Invoke(newStage);
    }

    /// <summary>
    /// 슬레이 더 스파이어 스타일 경로 생성 방식 (가로15, 세로7)
    /// - Level 1: 7칸(행 a~g) 중 6개의 경로 시작 (최소 2개의 서로 다른 시작 행 보장)
    /// - Level 2~14: 이전 레벨의 노드와 인접한 행(같은, 위 대각, 아래 대각) 후보 중 무작위 선택
    /// - Level 15: 강제적으로 D칸(행 'd', 인덱스 3) 선택
    /// </summary>
    private void GeneratePaths()
    {
        generatedPaths.Clear();
        int numPaths = 6;
        int totalRows = config.GridWidth; // 예: 7
        int totalLevels = config.layers.Count; // 예: 15

        // Level 1 시작 노드 선택: 행(0~6) 무작위 선택
        List<int> startRows = new List<int>();
        for (int i = 0; i < numPaths; i++)
        {
            int row = UnityEngine.Random.Range(0, totalRows);
            startRows.Add(row);
        }
        // 최소 2개의 서로 다른 시작 행 보장
        if (startRows.Distinct().Count() < 2)
        {
            int newVal;
            do { newVal = UnityEngine.Random.Range(0, totalRows); }
            while (newVal == startRows[0]);
            startRows[1] = newVal;
        }

        // 각 경로 생성: Level 1부터 Level 15까지
        for (int p = 0; p < numPaths; p++)
        {
            List<StageNode> path = new List<StageNode>();
            int currentRow = startRows[p];
            // Level 1:
            StageNode node = CreatePathNode(1, currentRow);
            path.Add(node);
            // Level 2 ~ 14:
            for (int level = 2; level < totalLevels; level++)
            {
                List<int> candidates = new List<int> { currentRow };
                if (currentRow - 1 >= 0) candidates.Add(currentRow - 1);
                if (currentRow + 1 < totalRows) candidates.Add(currentRow + 1);
                int nextRow = candidates.Random();
                StageNode nextNode = CreatePathNode(level, nextRow);
                path.Add(nextNode);
                currentRow = nextRow;
            }
            // Level 15: 강제 D칸 → 행 인덱스 3 ("d")
            int forcedRow = 3;
            StageNode finalNode = CreatePathNode(totalLevels, forcedRow);
            path.Add(finalNode);

            generatedPaths.Add(path);
        }

        // 경로 통합: 각 Level(1~15)별로, 동일한 행(알파벳)에서 중복 제거
        columnNodes.Clear();
        for (int level = 1; level <= totalLevels; level++)
        {
            List<StageNode> levelNodes = new List<StageNode>();
            foreach (var path in generatedPaths)
            {
                StageNode n = path.FirstOrDefault(node => node.floor == level);
                if (n != null && !levelNodes.Any(x => x.indexOnFloor == n.indexOnFloor))
                {
                    levelNodes.Add(n);
                }
            }
            // 정렬 (행(알파벳) 순: a, b, c, ...)
            levelNodes = levelNodes.OrderBy(n => n.indexOnFloor).ToList();
            columnNodes.Add(levelNodes);
        }
    }

    /// <summary>
    /// Level와 row에 따라 StageNode 생성.
    /// 여기서 Level은 왼쪽→오른쪽(1~15)이며, row는 0~6 (a~g)
    /// gridID는 "{알파벳}-{Level}" 형식, 예: "a-1", "b-1", ..., "g-15"
    /// </summary>
    private StageNode CreatePathNode(int level, int row)
    {
        // stageContainer(또는 GridGenerator 오브젝트)를 부모로 지정하여 Instantiate
        Transform parent = stageUIManager.stageContainer;
        StageNode node = Instantiate(stageNodePrefab, parent).GetComponent<StageNode>();
        node.name = $"StageNode_{level}_{row}";
        node.floor = level;
        node.indexOnFloor = row;
        node.nodeName = $"Level {level} Row {(char)('a' + row)}";
        node.gridID = $"{level}-{(char)('a' + row)}";
        // 위치 계산:
        float totalWidth = horizontalSpacing * config.layers.Count;
        float xOffset = totalWidth / 2f;
        float posX = -xOffset + (level - 1) * horizontalSpacing + horizontalSpacing / 2f;
        float totalHeight = verticalSpacing * config.GridWidth;
        float yOffset = totalHeight / 2f;
        float posY = yOffset - row * verticalSpacing;
        node.position = new Vector2(posX, posY);
        node.nodeType = NodeType.Monster; // 초기 설정 (나중에 AssignEncounters에서 조정)
        return node;
    }


    private void AssignEncounters()
    {
        // 예시 규칙: Level 1는 Monster, Level 15는 Rest, Level 9는 Treasure, 나머지는 Monster/Elite 랜덤
        for (int level = 1; level <= columnNodes.Count; level++)
        {
            foreach (StageNode node in columnNodes[level - 1])
            {
                if (node.floor == 1)
                    node.nodeType = NodeType.Monster;
                else if (node.floor == config.layers.Count)
                    node.nodeType = NodeType.Rest;
                else if (node.floor == 9)
                    node.nodeType = NodeType.Treasure;
                else
                {
                    int roll = rnd.Next(100);
                    node.nodeType = roll < 80 ? NodeType.Monster : NodeType.Elite;
                }
            }
        }
    }

    private void ConnectBossRoom()
    {
        // 보스 노드는 Level 16에 생성 (Level 15 아래)
        float bossX = horizontalSpacing * config.layers.Count;
        // StageNode 프리팹을 사용하여 보스 노드를 생성합니다.
        StageNode boss = Instantiate(stageNodePrefab).GetComponent<StageNode>();
        boss.name = "StageNode_Boss";
        boss.floor = config.layers.Count + 1; // 예: 16
        boss.nodeName = "Boss";
        boss.indexOnFloor = 3;  // 강제 D칸 (행 'd', 인덱스 3)
                                // gridID는 격자 컨테이너와 일치하도록 "Level-문자" 형식 (예: "16-d")
        boss.gridID = $"{boss.floor}-{(char)('a' + 3)}";

        float totalWidth = horizontalSpacing * config.layers.Count;
        float xOffset = totalWidth / 2f;
        float posX = -xOffset + (boss.floor - 1) * horizontalSpacing + horizontalSpacing / 2f;

        float totalHeight = verticalSpacing * config.GridWidth;
        float yOffset = totalHeight / 2f;
        // 보스 노드는 중앙(예: y=0)으로 배치하거나, 다른 기준에 따라 설정합니다.
        float posY = 0;
        boss.position = new Vector2(posX, posY);
        boss.nodeType = NodeType.Boss;

        // Level 15의 노드들을 가져와 보스 노드와 연결합니다.
        List<StageNode> lastLevel = columnNodes.Last();
        foreach (StageNode node in lastLevel)
        {
            node.outgoing.Add(boss.point);
            boss.incoming.Add(node.point);
        }
        columnNodes.Add(new List<StageNode> { boss });
    }

    private void RemoveCrossConnections()
    {
        // 교차 제거 로직은 추가 구현 필요 시 작성
    }

    private StageNode GetNode(Vector2Int p)
    {
        return columnNodes.SelectMany(list => list).FirstOrDefault(n => n.point.Equals(p));
    }
}
