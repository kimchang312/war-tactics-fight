using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    [CreateAssetMenu(fileName = "NewMapConfig", menuName = "Map/MapConfig")]
    public class MapConfig : ScriptableObject
    {
        // 맵에 사용될 노드 청사진 목록
        public List<NodeBlueprint> nodeBlueprints;

        [Tooltip("Nodes that will be used on layers with Randomize Nodes > 0")]
        public List<NodeType> randomNodes = new List<NodeType>
            { NodeType.Mystery, NodeType.Store, NodeType.Treasure, NodeType.MinorEnemy, NodeType.RestSite };

        // 보스 전 노드와 시작 노드 중 큰 값을 기준으로 그리드 가로 길이를 결정
        public int GridWidth => Mathf.Max(numOfPreBossNodes.max, numOfStartingNodes.max);

        // 보스 전 노드 개수의 최소/최대값
        public IntMinMax numOfPreBossNodes;

        // 시작 노드 개수의 최소/최대값
        public IntMinMax numOfStartingNodes;

        [Tooltip("Increase this number to generate more paths")]
        public int extraPaths;

        // 각 층(MapLayer)에 대한 설정 목록
        public List<MapLayer> layers;
    }
}
