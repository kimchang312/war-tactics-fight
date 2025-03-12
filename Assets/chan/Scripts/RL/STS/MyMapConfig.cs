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
}
