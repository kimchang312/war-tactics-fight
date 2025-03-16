using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Map;  // MapData, MapConfig, MapLayer, NodeBlueprint, NodeType 등이 포함됨

public class StageMapManager : MonoBehaviour
{
    public MapConfig config;  // 설정 데이터 (ScriptableObject)
    public float horizontalSpacing = 200f;  // 열 간격 (x축)
    public float verticalSpacing = 150f;      // 행 간격 (y축)

    public MapData CurrentMap { get; private set; }

    // 각 행별 StageNode들을 저장하는 리스트
    private List<List<StageNode>> rowNodes = new List<List<StageNode>>();

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
        GenerateMap();
        ConnectMap();
        AssignEncounters();
        ConnectBossRoom();

        if (rowNodes == null || rowNodes.Count == 0)
        {
            Debug.LogError("❌ 맵 노드가 생성되지 않았습니다.");
            return;
        }

        List<StageNode> nodesList = rowNodes.SelectMany(list => list).ToList();

        string bossName = config.nodeBlueprints.Where(b => b.nodeType == NodeType.Boss).ToList().Random().name;
        List<Node> dataNodes = nodesList.Select(n => n.ToNode()).ToList();
        CurrentMap = new MapData(config.name, bossName, dataNodes, new List<Vector2Int>());
        Debug.Log(CurrentMap.ToJson());

        OnMapGenerated?.Invoke(nodesList);
    }

    public StageNode GetCurrentStage()
    {
        return rowNodes.FirstOrDefault()?.FirstOrDefault();
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

    private void GenerateMap()
    {
        rowNodes.Clear();

        // config.layers.Count를 행 수로 사용, config.GridWidth를 열 수로 사용
        for (int row = 0; row < config.layers.Count; row++)
            PlaceRow(row);

        // 무작위 오프셋 적용 안 함
    }

    private void PlaceRow(int rowIndex)
    {
        MapLayer layer = config.layers[rowIndex];
        List<StageNode> nodesInRow = new List<StageNode>();

        // 각 행의 너비 = horizontalSpacing * config.GridWidth, 중앙 정렬 offset 계산
        float totalWidth = horizontalSpacing * config.GridWidth;
        float offset = totalWidth / 2f;

        for (int col = 0; col < config.GridWidth; col++)
        {
            List<NodeType> supportedTypes = config.randomNodes.Where(t => config.nodeBlueprints.Any(b => b.nodeType == t)).ToList();
            NodeType nodeType = UnityEngine.Random.Range(0f, 1f) < layer.randomizeNodes && supportedTypes.Count > 0
                ? supportedTypes.Random()
                : layer.nodeType;
            string blueprintName = config.nodeBlueprints.Where(b => b.nodeType == nodeType).ToList().Random().name;

            StageNode node = new GameObject($"StageNode_{rowIndex}_{col}").AddComponent<StageNode>();
            node.floor = rowIndex + 1;  // 행 번호
            node.indexOnFloor = col;    // 열 번호
            node.nodeName = $"Row {rowIndex + 1} Column {col + 1}";
            node.gridID = $"{rowIndex + 1}-{(char)('a' + col)}";
            node.nodeType = nodeType;

            // x 좌표: -offset + col * horizontalSpacing + (horizontalSpacing/2)
            // y 좌표: -rowIndex * verticalSpacing (행마다 일정한 간격)
            float posX = -offset + col * horizontalSpacing + horizontalSpacing / 2f;
            float posY = -rowIndex * verticalSpacing;
            node.position = new Vector2(posX, posY);

            nodesInRow.Add(node);
        }
        rowNodes.Add(nodesInRow);
    }

    private void ConnectMap()
    {
        // 행별 연결 (위에서 아래로 진행)
        for (int row = 0; row < rowNodes.Count - 1; row++)
        {
            List<StageNode> currentRow = rowNodes[row];
            List<StageNode> nextRow = rowNodes[row + 1];
            foreach (StageNode current in currentRow)
            {
                List<StageNode> candidates = new List<StageNode>();
                int col = current.indexOnFloor;
                if (col >= 0 && col < nextRow.Count)
                    candidates.Add(nextRow[col]);
                if (col - 1 >= 0)
                    candidates.Add(nextRow[col - 1]);
                if (col + 1 < nextRow.Count)
                    candidates.Add(nextRow[col + 1]);

                candidates = candidates.Distinct().ToList();

                foreach (StageNode next in candidates)
                {
                    if (!current.outgoing.Contains(next.point))
                    {
                        current.outgoing.Add(next.point);
                        next.incoming.Add(current.point);
                    }
                }
            }
        }
    }

    private void AssignEncounters()
    {
        foreach (var row in rowNodes)
        {
            foreach (StageNode node in row)
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
        // 보스 노드는 마지막 행 아래에 추가
        float bossY = verticalSpacing * config.layers.Count;
        StageNode boss = new GameObject("StageNode_Boss").AddComponent<StageNode>();
        boss.floor = config.layers.Count + 1;
        boss.nodeName = "Boss";
        boss.indexOnFloor = 0;
        boss.gridID = $"1-{boss.floor}";
        float posX = 0; // 중앙 정렬
        float posY = -bossY;
        boss.position = new Vector2(posX, posY);
        boss.nodeType = NodeType.Boss;

        List<StageNode> lastRow = rowNodes.Last();
        foreach (StageNode node in lastRow)
        {
            node.outgoing.Add(boss.point);
            boss.incoming.Add(node.point);
        }
        rowNodes.Add(new List<StageNode> { boss });
    }

    private void RemoveCrossConnections()
    {
        // 필요에 따라 수정 (위아래 진행 방식에 맞게)
        for (int i = 0; i < config.GridWidth - 1; i++)
        {
            for (int j = 0; j < config.layers.Count - 1; j++)
            {
                StageNode node = GetNode(new Vector2Int(i, j));
                if (node == null || (node.incoming.Count == 0 && node.outgoing.Count == 0))
                    continue;
                StageNode right = GetNode(new Vector2Int(i + 1, j));
                if (right == null || (right.incoming.Count == 0 && right.outgoing.Count == 0))
                    continue;
                StageNode top = GetNode(new Vector2Int(i, j + 1));
                if (top == null || (top.incoming.Count == 0 && top.outgoing.Count == 0))
                    continue;
                StageNode topRight = GetNode(new Vector2Int(i + 1, j + 1));
                if (topRight == null || (topRight.incoming.Count == 0 && topRight.outgoing.Count == 0))
                    continue;

                if (!node.outgoing.Any(p => p.Equals(topRight.point))) continue;
                if (!right.outgoing.Any(p => p.Equals(top.point))) continue;

                node.outgoing.Add(top.point);
                top.incoming.Add(node.point);

                right.outgoing.Add(topRight.point);
                topRight.incoming.Add(right.point);

                float r = UnityEngine.Random.Range(0f, 1f);
                if (r < 0.2f)
                {
                    node.outgoing.Remove(topRight.point);
                    topRight.incoming.Remove(node.point);
                    right.outgoing.Remove(top.point);
                    top.incoming.Remove(right.point);
                }
                else if (r < 0.6f)
                {
                    node.outgoing.Remove(topRight.point);
                    topRight.incoming.Remove(node.point);
                }
                else
                {
                    right.outgoing.Remove(top.point);
                    top.incoming.Remove(right.point);
                }
            }
        }
    }

    private StageNode GetNode(Vector2Int p)
    {
        return rowNodes.SelectMany(list => list).FirstOrDefault(n => n.point.Equals(p));
    }
}
