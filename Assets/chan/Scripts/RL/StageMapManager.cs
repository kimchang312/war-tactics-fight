using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Map;  // MapData, MapConfig, MapLayer, NodeBlueprint, NodeType 등이 포함됨

public class StageMapManager : MonoBehaviour
{
    public MapConfig config;  // 설정 데이터 (ScriptableObject)
    public float verticalSpacing = 150f;
    public float horizontalSpacing = 200f;

    // 생성된 맵 데이터를 MapData 타입으로 보관
    public MapData CurrentMap { get; private set; }

    // 각 층별 StageNode들을 저장하는 리스트
    private List<List<StageNode>> layersNodes = new List<List<StageNode>>();
    // 각 층 간 거리 값
    private List<float> layerDistances;

    // 재사용 가능한 System.Random 인스턴스
    private System.Random rnd = new System.Random();
    private int stageCreationCounter = 0;

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

        if (layersNodes == null || layersNodes.Count == 0)
        {
            Debug.LogError("❌ 맵 노드가 생성되지 않았습니다.");
            return;
        }

        // 모든 StageNode들을 평탄화하여 하나의 리스트로 모음
        List<StageNode> nodesList = layersNodes.SelectMany(list => list)
            .Where(n => n.incoming.Count > 0 || n.outgoing.Count > 0 || n.floor == 1)
            .ToList();

        // 보스 노드 이름은 config.nodeBlueprints에서 Boss 타입 중 랜덤 선택
        string bossName = config.nodeBlueprints.Where(b => b.nodeType == NodeType.Boss).ToList().Random().name;

        // StageNode를 데이터 모델 Node로 변환
        List<Node> dataNodes = nodesList.Select(n => n.ToNode()).ToList();
        CurrentMap = new MapData(config.name, bossName, dataNodes, new List<Vector2Int>());
        Debug.Log(CurrentMap.ToJson());

        OnMapGenerated?.Invoke(nodesList);
    }

    public StageNode GetCurrentStage()
    {
        return layersNodes.FirstOrDefault()?.FirstOrDefault();
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

    // 맵 생성: 각 층별 StageNode 생성 및 배치 후 무작위 오프셋 적용
    private void GenerateMap()
    {
        layersNodes.Clear();
        GenerateLayerDistances();

        for (int i = 0; i < config.layers.Count; i++)
            PlaceLayer(i);

        RandomizeNodePositions();
    }

    private void GenerateLayerDistances()
    {
        layerDistances = new List<float>();
        foreach (MapLayer layer in config.layers)
            layerDistances.Add(layer.distanceFromPreviousLayer.GetValue());
    }

    private void PlaceLayer(int layerIndex)
    {
        MapLayer layer = config.layers[layerIndex];
        List<StageNode> nodesOnLayer = new List<StageNode>();

        // 중앙 정렬을 위한 offset 계산
        float offset = layer.nodesApartDistance * config.GridWidth / 2f;

        for (int i = 0; i < config.GridWidth; i++)
        {
            // 랜덤 노드 타입 선택
            List<NodeType> supportedTypes = config.randomNodes.Where(t => config.nodeBlueprints.Any(b => b.nodeType == t)).ToList();
            NodeType nodeType = UnityEngine.Random.Range(0f, 1f) < layer.randomizeNodes && supportedTypes.Count > 0
                ? supportedTypes.Random()
                : layer.nodeType;
            string blueprintName = config.nodeBlueprints.Where(b => b.nodeType == nodeType).ToList().Random().name;

            // StageNode 생성 (GameObject 생성 후 컴포넌트 추가)
            StageNode node = new GameObject($"StageNode_{layerIndex}_{i}").AddComponent<StageNode>();
            node.floor = layerIndex + 1;
            node.nodeName = $"Floor {layerIndex + 1} Node {i}";
            node.indexOnFloor = i;
            node.gridID = $"{node.floor}-{(char)('a' + i)}";
            node.nodeType = nodeType; // 노드 타입 설정

            float posX = -offset + i * layer.nodesApartDistance;
            float posY = layerDistances.Take(layerIndex + 1).Sum();
            node.position = new Vector2(posX, posY);

            nodesOnLayer.Add(node);
        }
        layersNodes.Add(nodesOnLayer);
    }

    private void RandomizeNodePositions()
    {
        for (int i = 0; i < layersNodes.Count; i++)
        {
            List<StageNode> list = layersNodes[i];
            MapLayer layer = config.layers[i];
            float nextDist = i + 1 < layerDistances.Count ? layerDistances[i + 1] : 0f;
            float prevDist = layerDistances[i];

            foreach (StageNode node in list)
            {
                float xRnd = UnityEngine.Random.Range(-0.5f, 0.5f);
                float yRnd = UnityEngine.Random.Range(-0.5f, 0.5f);
                float x = xRnd * layer.nodesApartDistance;
                float y = (yRnd < 0 ? prevDist : nextDist) * yRnd;
                node.position += new Vector2(x, y) * layer.randomizePosition;
            }
        }
    }

    private void ConnectMap()
    {
        for (int layer = 0; layer < layersNodes.Count - 1; layer++)
        {
            List<StageNode> currentNodes = layersNodes[layer];
            List<StageNode> nextNodes = layersNodes[layer + 1];
            foreach (StageNode current in currentNodes)
            {
                List<StageNode> candidates = new List<StageNode>();
                int idx = current.indexOnFloor;
                if (idx >= 0 && idx < nextNodes.Count)
                    candidates.Add(nextNodes[idx]);
                if (idx - 1 >= 0)
                    candidates.Add(nextNodes[idx - 1]);
                if (idx + 1 < nextNodes.Count)
                    candidates.Add(nextNodes[idx + 1]);

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
        foreach (var layer in layersNodes)
        {
            foreach (StageNode node in layer)
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
        float bossY = layerDistances.Sum();
        StageNode boss = new GameObject("StageNode_Boss").AddComponent<StageNode>();
        boss.floor = config.layers.Count + 1;
        boss.nodeName = "Boss";
        boss.indexOnFloor = 0;
        boss.gridID = $"{boss.floor}-a";
        boss.position = new Vector2(0, bossY);
        boss.nodeType = NodeType.Boss;

        List<StageNode> lastLayer = layersNodes.Last();
        foreach (StageNode node in lastLayer)
        {
            node.outgoing.Add(boss.point);
            boss.incoming.Add(node.point);
        }
        layersNodes.Add(new List<StageNode> { boss });
        List<StageNode> allNodesFlat = layersNodes.SelectMany(list => list).ToList();
    }

    private void RemoveCrossConnections()
    {
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
        return layersNodes.SelectMany(list => list).FirstOrDefault(n => n.point.Equals(p));
    }
}
