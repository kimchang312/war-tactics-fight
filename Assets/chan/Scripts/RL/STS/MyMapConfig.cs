using UnityEngine;
using System.Collections.Generic;
using Map;

[CreateAssetMenu(fileName = "NewMapConfig", menuName = "Map/MapConfig")]
public class MapConfig : ScriptableObject
{
    public List<NodeBlueprint> nodeBlueprints;
    public List<NodeType> randomNodes = new List<NodeType> { NodeType.Monster, NodeType.Elite, NodeType.Treasure, NodeType.Rest, NodeType.Shop, NodeType.Boss };
    public IntMinMax numOfPreBossNodes;
    public IntMinMax numOfStartingNodes;
    public int extraPaths;
    public List<MapLayer> layers;

    // GridWidth 프로퍼티: 보스 전 노드 개수와 시작 노드 개수 중 큰 값을 반환
    public int GridWidth => Mathf.Max(numOfPreBossNodes.max, numOfStartingNodes.max);
}
