using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 전장효과 텍스트에 마우스 오버 시 툴팁을 띄우기 위한 컴포넌트.
/// 기존 ExplainItem/ItemInformation에 의존하지 않고 간단히 텍스트만 표시한다.
/// </summary>
public class BattlefieldEffectTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject tooltipRoot; // 보통 GameManager.Instance.itemToolTip
    [SerializeField] private TextMeshProUGUI tooltipText;

    private string _text;

    public void SetText(string text)
    {
        _text = text ?? "";
    }

    private void EnsureRefs()
    {
        if (tooltipRoot == null)
            tooltipRoot = GameManager.Instance != null ? GameManager.Instance.itemToolTip : null;
        if (tooltipRoot != null && tooltipText == null)
            tooltipText = tooltipRoot.GetComponentInChildren<TextMeshProUGUI>(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EnsureRefs();
        if (tooltipRoot == null || tooltipText == null) return;

        tooltipText.text = _text;
        tooltipRoot.SetActive(true);
        tooltipRoot.transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EnsureRefs();
        if (tooltipRoot == null) return;
        tooltipRoot.SetActive(false);
    }
}


