using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public TextMeshProUGUI tooltipText; // 툴팁 텍스트
    public GameObject tooltipPanel; // 툴팁 UI 패널
    private RectTransform tooltipRect;

    private void Awake()
    {
        tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        HideTooltip(); // 시작 시 툴팁 숨김
    }

    // 툴팁 텍스트 설정
    public void SetTooltip(string description)
    {
        tooltipText.text = description;
    }

    // 툴팁 표시 (마우스 위치 기준 왼쪽에 표시)
    public void ShowTooltip(Vector3 position)
    {
        tooltipPanel.SetActive(true);
        position.x -= tooltipRect.sizeDelta.x * -0.6f; // 마우스 왼쪽에 배치
        tooltipPanel.transform.position = position;
    }

    // 툴팁 숨김
    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}