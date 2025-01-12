using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StageTooltip : MonoBehaviour
{
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;
    private RectTransform tooltipRect;

    private void Awake()
    {
        

        if (tooltipPanel == null)
        {
            tooltipPanel = this.gameObject;
            Debug.LogWarning("⚠️ tooltipPanel이 Inspector에서 설정되지 않아 자동으로 할당되었습니다.");
        }
        else
        {
            Debug.Log("✅ tooltipPanel이 Inspector에서 정상적으로 설정됨.");
        }

        if (tooltipText == null)
        {
            tooltipText = GetComponentInChildren<TextMeshProUGUI>();
            if (tooltipText == null)
            {
                Debug.LogError("❌ tooltipText가 할당되지 않았습니다! TextMeshProUGUI를 확인하세요.");
            }
        }

        if (tooltipPanel != null)
        {
            tooltipRect = tooltipPanel.GetComponent<RectTransform>();
            if (tooltipRect == null)
            {
                Debug.LogError("❌ tooltipPanel에 RectTransform이 없습니다! UI 요소인지 확인하세요.");
            }
        }
        else
        {
            Debug.LogError("❌ tooltipPanel 자체가 null 상태입니다! Inspector에서 확인하세요.");
        }

        if (tooltipPanel != null && tooltipRect != null && tooltipText != null)
        {
            Debug.Log("✅ StageTooltip 초기화 완료: tooltipPanel, tooltipRect 정상 할당됨.");
        }
        else
        {
            Debug.LogError("❌ StageTooltip 초기화 실패: tooltipPanel 또는 tooltipRect 또는 tooltipText가 null 상태입니다!");
        }

        HideTooltip();
    }

    public void ShowTooltip(string message, RectTransform buttonRect)
    {
        

        if (tooltipPanel == null || tooltipRect == null)
        {
            Debug.LogError("❌ ShowTooltip 실행 불가: tooltipPanel 또는 tooltipRect가 할당되지 않았습니다!");
            return;
        }

        tooltipPanel.SetActive(true);
        tooltipText.text = message;

        // ✅ 버튼의 화면 좌표 가져오기
        Vector3[] buttonCorners = new Vector3[4];
        buttonRect.GetWorldCorners(buttonCorners);
        Vector2 topRightCorner = (Vector2)buttonCorners[2]; // 버튼의 오른쪽 상단

        // ✅ 툴팁 위치 조정 (버튼의 오른쪽 상단)
        Vector2 adjustedPosition = topRightCorner + new Vector2(30f, 30f);

        // ✅ 화면 경계 보정
        float panelWidth = tooltipRect.rect.width;
        float panelHeight = tooltipRect.rect.height;

        if (adjustedPosition.x + panelWidth > Screen.width)
        {
            adjustedPosition.x = Screen.width - panelWidth - 10f;
        }
        if (adjustedPosition.y - panelHeight < 0)
        {
            adjustedPosition.y = panelHeight + 10f;
        }

        tooltipRect.anchoredPosition = adjustedPosition; // ✅ `position` 대신 `anchoredPosition` 사용
    }

    public void HideTooltip()
    {
        
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
}
