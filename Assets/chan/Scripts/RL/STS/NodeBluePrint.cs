using UnityEngine;

[CreateAssetMenu(fileName = "NewNodeBlueprint", menuName = "Map/NodeBlueprint")]
public class NodeBlueprint : ScriptableObject
{
    [Tooltip("청사진의 이름")]
    public string blueprintName;

    [Tooltip("노드의 타입")]
    public NodeType nodeType;

    [Tooltip("노드에 사용할 스프라이트")]
    public Sprite sprite;

    // 필요에 따라 추가적인 속성을 정의할 수 있습니다.
}
