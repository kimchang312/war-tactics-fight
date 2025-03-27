using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StageNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public int level;
    public int row;
    public StageType stageType;
    public Text stageLabel;
    // Tooltip 대신 MapTooltip를 사용합니다.
    public MapTooltip mapTooltip;

    public void Setup(StageNode nodeData)
    {
        level = nodeData.level;
        row = nodeData.row;
        stageType = nodeData.stageType;
        char rowChar = (char)('A' + row);
        if (stageLabel != null)
            stageLabel.text = $"L{level + 1}{rowChar}\n{stageType}";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (mapTooltip != null)
        {
            // UI 요소의 위치는 RectTransform의 anchoredPosition (Vector2)을 사용합니다.
            RectTransform rt = GetComponent<RectTransform>();
            Vector2 pos = rt.anchoredPosition;
            mapTooltip.ShowTooltip(pos, $"Stage Info:\nLevel: {level + 1}\nRow: {(char)('A' + row)}\nType: {stageType}");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (mapTooltip != null)
        {
            mapTooltip.HideTooltip();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Clicked on Stage: L{level + 1}{(char)('A' + row)}");
        // 추가적인 스테이지 이동 로직 구현 가능
    }
}
