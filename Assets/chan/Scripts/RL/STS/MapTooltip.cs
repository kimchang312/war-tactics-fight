using UnityEngine;
using TMPro;

public class MapTooltip : MonoBehaviour
{
    // Tooltip 패널의 자식에 있는 TextMeshProUGUI 컴포넌트를 할당하세요.
    public TMP_Text tooltipText;

    // (선택사항) 툴팁 배경의 RectTransform. 텍스트 길이에 맞게 배경 크기를 조정할 때 사용.
    public RectTransform backgroundRectTransform;

    // (선택사항) Tooltip의 부모 Canvas의 RectTransform. 필요에 따라 좌표 변환에 사용.
    public RectTransform canvasRectTransform;

    void Awake()
    {
        HideTooltip();
    }

    /// <summary>
    /// 주어진 위치(position, Vector2)와 메시지를 이용해 Tooltip을 표시합니다.
    /// </summary>
    /// <param name="position">캔버스 내에서의 위치 (anchoredPosition 기준)</param>
    /// <param name="message">표시할 메시지</param>
    public void ShowTooltip(Vector2 position, string message)
    {
        tooltipText.text = message;
        // 배경 크기를 텍스트 크기에 맞게 자동 조정하고 싶다면 아래와 같이 조절할 수 있습니다.
        if (backgroundRectTransform != null)
        {
            // 약간의 여백을 추가합니다.
            Vector2 padding = new Vector2(10f, 10f);
            backgroundRectTransform.sizeDelta = tooltipText.GetPreferredValues(message) + padding;
        }

        gameObject.SetActive(true);

        // 캔버스 좌표계를 고려하여 Tooltip 위치를 지정합니다.
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchoredPosition = position;
    }

    /// <summary>
    /// Tooltip을 숨깁니다.
    /// </summary>
    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}
