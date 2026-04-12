using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// EffectCD를 재생하는 플레이어 컴포넌트
/// - 프레임 애니메이션 (정방향/역방향)
/// - 색상 변경
/// - 좌우 반전
/// - 이동 애니메이션 (Static/RightToLeft/LeftToRight)
/// </summary>
public class EffectCDPlayer : MonoBehaviour
{
    [SerializeField] private Image image; // 프레임을 보여줄 이미지 슬롯

    [SerializeField] public bool flipX = false; // 적 초상화 위에 생성할 경우 좌우반전 활성화
    [SerializeField] private RectTransform flipRoot; // 반전 적용할 RectTransform (null이면 image.rectTransform 사용)

    private Sequence seq;
    private Vector2 originalPosition; // 이동 애니메이션 후 복원용

    /// <summary>
    /// 좌우 반전 적용
    /// </summary>
    private void ApplyFlipX(bool flip)
    {
        var rt = flipRoot != null ? flipRoot : image.rectTransform;
        var s = rt.localScale;
        float absX = Mathf.Abs(s.x);
        rt.localScale = new Vector3(flip ? -absX : absX, s.y, s.z);
    }

    /// <summary>
    /// EffectCD를 받아 재생
    /// </summary>
    /// <param name="cd">재생할 EffectCD 데이터</param>
    /// <param name="onComplete">재생 완료 시 호출될 콜백</param>
    public void Play(EffectCD cd, System.Action onComplete = null)
    {
        if (cd == null || cd.frames == null || cd.frames.Length == 0)
        {
            Debug.LogWarning("[EffectCDPlayer] CD가 비어있거나 프레임이 없습니다.");
            onComplete?.Invoke();
            return;
        }

        // 이전 시퀀스 종료
        seq?.Kill();
        seq = DOTween.Sequence();

        // 좌우 반전 적용
        bool effectiveFlip = flipX && cd.AllowFlip;
        ApplyFlipX(effectiveFlip);

        // 프레임 계산
        int frameCount = cd.frames.Length;
        float frameTime = Mathf.Max(0f, cd.totalDuration) / frameCount;

        // 이미지 초기화
        image.enabled = true;
        Color originalColor = image.color;
        image.color = cd.playColor;

        // 위치 초기화 및 이동 애니메이션 설정
        RectTransform rectTransform = image.rectTransform;
        originalPosition = rectTransform.anchoredPosition;

        ApplyMovementAnimation(cd, rectTransform);

        // 프레임 애니메이션 추가 (정방향/역방향)
        for (int i = 0; i < frameCount; i++)
        {
            int frameIndex = cd.playReverse ? (frameCount - 1 - i) : i;
            Sprite frameSprite = cd.frames[frameIndex]; // 클로저 문제 방지용 로컬 변수

            seq.AppendCallback(() =>
            {
                if (image != null) // null 체크 추가 (재생 중 파괴될 수 있음)
                {
                    image.sprite = frameSprite;
                }
            });

            // 프레임 간 간격
            if (frameTime > 0f)
                seq.AppendInterval(frameTime);
        }

        // 루프 설정
        seq.SetLoops(cd.loop ? -1 : 1, LoopType.Restart);

        if (!cd.loop)
        {
            // 1회 재생 완료 후 처리
            seq.OnComplete(() =>
            {
                if (image != null)
                {
                    // 색상 및 위치 복원
                    image.color = originalColor;
                    rectTransform.anchoredPosition = originalPosition;
                    image.enabled = false;
                }
                onComplete?.Invoke();
            });
        }

        // 시퀀스 재생
        seq.Play();
    }

    /// <summary>
    /// 이동 타입에 따른 애니메이션 적용
    /// </summary>
    private void ApplyMovementAnimation(EffectCD cd, RectTransform rectTransform)
    {
        if (cd.moveType == EffectMoveType.Static)
        {
            // 고정: 이동하지 않음
            return;
        }

        Vector2 startPos = originalPosition;
        Vector2 endPos = originalPosition;

        switch (cd.moveType)
        {
            case EffectMoveType.RightToLeft:
                // 우→좌 이동 (아군 → 적군)
                startPos.x += cd.moveDistance;
                endPos.x -= cd.moveDistance;
                break;

            case EffectMoveType.LeftToRight:
                // 좌→우 이동 (적군 → 아군)
                startPos.x -= cd.moveDistance;
                endPos.x += cd.moveDistance;
                break;
        }

        // 시작 위치 설정
        rectTransform.anchoredPosition = startPos;

        // 이동 애니메이션 추가 (전체 재생 시간과 동일하게)
        seq.Insert(0, rectTransform.DOAnchorPos(endPos, cd.totalDuration).SetEase(Ease.Linear));
    }

    /// <summary>
    /// 재생 중단
    /// </summary>
    public void Stop(bool resetVisual = true)
    {
        if (seq != null && seq.IsActive())
        {
            seq.Kill();
        }

        if (resetVisual && image != null)
        {
            // 위치 및 상태 복원
            image.rectTransform.anchoredPosition = originalPosition;
            image.enabled = false;
        }
    }

    private void OnDisable()
    {
        seq?.Kill();
    }
}
