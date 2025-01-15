using UnityEngine;
using DG.Tweening;

public class BlacksmithUI : MonoBehaviour
{
    public RectTransform uiPanel; // ✅ 대장간 UI 패널
    public RectTransform blacksmithButton; // ✅ 대장간 버튼
    public float animationDuration; // ✅ 애니메이션 지속 시간 증가 (더 부드럽게)
    private Vector2 hiddenPosition; // ✅ UI 숨겨진 위치 (오른쪽)
    private Vector2 visiblePosition; // ✅ UI 표시된 위치 (왼쪽)
    private Vector2 buttonOriginalPosition; // ✅ 버튼 원래 위치
    private Vector2 buttonMoveOffset = new Vector2(-427f, 0); // ✅ 버튼 이동 거리
    private bool isVisible = false; // ✅ 현재 UI 상태

    private void Start()
    {
        if (uiPanel == null || blacksmithButton == null)
        {
            Debug.LogError("❌ uiPanel 또는 blacksmithButton이 할당되지 않았습니다! Unity Inspector에서 연결하세요.");
            return;
        }

        // ✅ 현재 UI 기본 위치 저장
        visiblePosition = uiPanel.anchoredPosition;
        hiddenPosition = visiblePosition + new Vector2(600f, 0); // ✅ 기본 위치에서 오른쪽으로 600px 이동

        // ✅ 버튼 원래 위치 저장
        buttonOriginalPosition = blacksmithButton.anchoredPosition;

        // ✅ UI를 숨겨진 위치로 초기화 (버튼은 원래 위치 유지)
        uiPanel.anchoredPosition = hiddenPosition;
    }

    public void ToggleUI()
    {
        if (uiPanel == null || blacksmithButton == null)
        {
            Debug.LogError("❌ uiPanel 또는 blacksmithButton이 할당되지 않았습니다! Unity Inspector에서 연결하세요.");
            return;
        }

        if (isVisible)
        {
            // ✅ 슬라이드 아웃 (왼쪽 → 오른쪽으로 숨기기, 감속 효과 개선)
            uiPanel.DOAnchorPos(hiddenPosition, animationDuration).SetEase(Ease.OutExpo); // ✅ 더 부드러운 감속 적용
            blacksmithButton.DOAnchorPos(buttonOriginalPosition, animationDuration + 0.1f).SetEase(Ease.OutExpo); // ✅ 버튼도 동일한 감속 적용
        }
        else
        {
            // ✅ 슬라이드 인 (오른쪽 → 왼쪽으로 나타내기, 감속 효과 개선)
            uiPanel.DOAnchorPos(visiblePosition, animationDuration).SetEase(Ease.OutExpo);  
            blacksmithButton.DOAnchorPos(buttonOriginalPosition + buttonMoveOffset, animationDuration+0.1f).SetEase(Ease.OutExpo);
        }

        isVisible = !isVisible;
    }
}
