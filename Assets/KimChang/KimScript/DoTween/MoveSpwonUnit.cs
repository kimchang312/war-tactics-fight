using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DoTween 사용을 위해 필요

public class MoveSpwonUnit : MonoBehaviour
{
    private Tween moveTween; // DoTween 트윈 객체
    private RectTransform rectTransform;

    private void Awake()
    {
        // RectTransform 캐싱 (UI 요소일 경우)
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        Debug.Log(rectTransform.anchoredPosition.y);
        // 현재 위치에서 y -= 80으로 이동 (anchoredPosition 사용)
        moveTween = rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y - 80, 0.5f)
            .SetEase(Ease.OutQuad); // 부드러운 이동
    }

    private void OnDisable()
    {
        // 비활성화 시 트윈 중지 및 해제
        if (moveTween != null && moveTween.IsActive())
        {
            moveTween.Kill();
        }
    }
}
