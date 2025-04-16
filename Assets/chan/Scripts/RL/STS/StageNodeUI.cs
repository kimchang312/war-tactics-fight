using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StageNodeUI : MonoBehaviour
{
    public int level;
    public int row;
    public StageType stageType;
    public Text stageLabel;
    // Tooltip 대신 MapTooltip를 사용합니다.
    public MapTooltip mapTooltip;

    public void StageColor()
    {
        Image img = gameObject.GetComponent<Image>();
        if(stageType==StageType.Boss) img.color = Color.red;
    }

    public void Setup(StageNode nodeData)
    {
        level = nodeData.level;
        row = nodeData.row;
        stageType = nodeData.stageType;
        char rowChar = (char)('A' + row);
        if (stageLabel != null)
            stageLabel.text = $"L{level + 1}{rowChar}\n{stageType}";
    }

}
