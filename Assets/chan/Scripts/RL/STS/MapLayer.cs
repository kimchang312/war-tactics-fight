using UnityEngine;

namespace Map
{
    [System.Serializable]
    public class MapLayer
    {
        [Tooltip("노드 간 간격")]
        public float nodesApartDistance;
        [Tooltip("이 층의 이전 층과의 거리")]
        public FloatMinMax distanceFromPreviousLayer;
        [Tooltip("해당 층에 기본으로 배치할 노드 타입")]
        public NodeType nodeType;
        [Tooltip("랜덤 노드 배치 확률 (0 ~ 1)")]
        public float randomizeNodes;
        [Tooltip("노드 위치 무작위화 정도 (0 ~ 1)")]
        public float randomizePosition;
    }
}
