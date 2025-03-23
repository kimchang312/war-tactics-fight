using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageButton : MonoBehaviour
{
    public TMP_Text label;

    private StageNode stageNode;

    public void SetStageNode(StageNode node)
    {
        stageNode = node;
        if (label != null)
            label.text = node.nodeName + "\n" + node.nodeType.ToString();
    }

    public StageNode GetStageData()
    {
        return stageNode;
    }

    public void UpdateButtonState()
    {
        // 버튼 상태 업데이트 (텍스트, 색상 등 추가 구현)
    }

    public void SetInteractable(bool value)
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
            btn.interactable = value;
    }

    public void OnClick()
    {
        OnStageButtonClicked?.Invoke(stageNode);
    }

    public static event System.Action<StageNode> OnStageButtonClicked;
}
