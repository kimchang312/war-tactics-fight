using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectCDPlayer : MonoBehaviour
{
    [SerializeField] private Image image; // 프레임을 보여줄 이미지 슬롯

    [SerializeField] public bool flipX = false; // 적 초상화 위에 생성할 경우 좌우반전 활성화
    [SerializeField] private RectTransform flipRoot;

    private Sequence seq;

    /// <summary>
    /// CD를 받아 재생한다.
    /// </summary>

    private void ApplyFlipX(bool flip)
    {
        var rt = flipRoot != null ? flipRoot : image.rectTransform;
        var s = rt.localScale;
        float absX = Mathf.Abs(s.x);
        rt.localScale = new Vector3(flip ? -absX : absX, s.y, s.z);
    }


    public void Play(EffectCD cd, System.Action onComplete = null)
    {
        if (cd == null || cd.frames == null || cd.frames.Length == 0)
        {
            Debug.LogWarning("CD가 비어있음");
            return;
        }

        // 이전 시퀀스 종료
        seq?.Kill();
        seq = DOTween.Sequence();

        bool effectiveFlip = flipX && cd.AllowFlip;
        ApplyFlipX(effectiveFlip);

        int frameCount = cd.frames.Length;
        float frameTime = Mathf.Max(0f, cd.totalDuration) / frameCount;

        // 시작 상태 세팅
        image.enabled = true;

        // 색상 적용 (원래 색 저장)
        Color originalColor = image.color;
        image.color = cd.playColor;

        // 프레임 추가(정방향/역방향)
        for (int i = 0; i < frameCount; i++)
        {
            int frameIndex = cd.playReverse ? (frameCount - 1 - i) : i;
            seq.AppendCallback(() =>
            {
                image.sprite = cd.frames[frameIndex];
                // 필요 시 SetNativeSize 등:
                // image.SetNativeSize();
            });
            // 프레임 간 간격
            if (frameTime > 0f)
                seq.AppendInterval(frameTime);
        }

        // 루프 설정
        seq.SetLoops(cd.loop ? -1 : 1, LoopType.Restart);

        if (!cd.loop)
        {
            // 재생 완료 후 처리(1회 재생 때만 유효)
            seq.OnComplete(() =>
            {
                // 이미지 복원 및 숨김
                image.color = originalColor;
                image.enabled = false;
                onComplete?.Invoke();
            });
        }

        // 메모리/상태 안전용
        seq.Play();
    }

    public void Stop(bool resetVisual = true)
    {
        if (seq != null && seq.IsActive())
        {
            seq.Kill();
        }
        if (resetVisual && image != null)
        {
            image.enabled = false;
        }
    }

    private void OnDisable()
    {
        seq?.Kill();
    }
}
