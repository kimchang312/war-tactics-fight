using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// EffectCD를 재생하는 플레이어 컴포넌트
/// - 프레임 애니메이션 (정방향/역방향/루프)
/// - EffectType 기반 위치·크기 자동 설정
///   · CasterTop / TargetTop : 초상화 정중앙 기준 높이 2/3 위, sizeDelta 1:1
///   · TargetCenter           : 초상화 정중앙, sizeDelta 1:1
///   · ScreenCenter           : 화면 전체
///   · ScreenMoveRL           : 화면 오른쪽 바깥→왼쪽 바깥 이동, 이동 중 프레임 루프
/// - 유닛 사망(Transform null) 방어 코드 내장
/// </summary>
public class EffectCDPlayer : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Canvas effectCanvas;

    [SerializeField] public bool flipX = false;
    [SerializeField] private RectTransform flipRoot;

    [Header("화면 기준 RectTransform (ScreenCenter / ScreenMoveRL 타입용)")]
    [SerializeField] private RectTransform screenCenter;

    private Sequence seq;
    private Tween moveTween;                              // ScreenMoveRL 전용 이동 트윈
    private Vector2 originalPosition;
    private Transform originalParent;
    private Vector3 originalLocalScale;
    private Vector2 originalSizeDelta;
    private int originalSortingOrder;
    private TaskCompletionSource<bool> playbackCompletion;

    private bool suppressOnDisableCompletion;
    // ─────────────────────────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// EffectCD를 비동기로 재생 (await 가능)
    /// </summary>
    public async Task Play(
        EffectCD cd,
        RectTransform targetTransform = null,
        RectTransform casterTransform = null,
        System.Action onComplete = null)
    {
        if (cd == null || cd.frames == null || cd.frames.Length == 0)
        {
            Debug.LogWarning("[EffectCDPlayer] CD가 비어있거나 프레임이 없습니다.");
            onComplete?.Invoke();
            return;
        }

        if (image == null)
        {
            Debug.LogError($"[EffectCDPlayer] Play(): image가 null입니다 ({gameObject.name}).");
            onComplete?.Invoke();
            return;
        }

        Debug.Log($"[EffectCDPlayer] Play: {cd.name} | effectType={cd.effectType}");

        // 이전 재생 정리
        seq?.Kill();
        moveTween?.Kill();
        playbackCompletion?.TrySetResult(false);
        playbackCompletion = new TaskCompletionSource<bool>();

        RectTransform rectTransform = image.rectTransform;

        // 위치·부모·크기 설정 (유닛 사망 방어 포함)
        if (!SetupPositionAndParent(cd, targetTransform, casterTransform, rectTransform))
        {
            Debug.LogWarning($"[EffectCDPlayer] SetupPositionAndParent 실패 — 이펙트 건너뜀: {cd.name}");
            onComplete?.Invoke();
            playbackCompletion.TrySetResult(false);
            return;
        }

        // 좌우 반전
        ApplyFlipX(flipX && cd.AllowFlip);

        image.enabled = true;
        Color originalColor = image.color;
        image.color = cd.playColor;

        SetupSortingOrder(cd);

        // ──── ScreenMoveRL 전용 경로 ────
        if (cd.effectType == EffectType.ScreenMoveRL)
        {
            await PlayScreenMoveRL(cd, rectTransform, originalColor, onComplete);
            return;
        }

        // ──── 일반 프레임 애니메이션 ────
        seq = DOTween.Sequence();

        int frameCount = cd.frames.Length;
        float frameTime = Mathf.Max(0f, cd.totalDuration) / frameCount;

        for (int i = 0; i < frameCount; i++)
        {
            int fi = cd.playReverse ? (frameCount - 1 - i) : i;
            Sprite sp = cd.frames[fi];
            seq.AppendCallback(() => { if (image != null) image.sprite = sp; });
            if (frameTime > 0f) seq.AppendInterval(frameTime);
        }

        seq.SetLoops(cd.loop ? -1 : 1, LoopType.Restart);

        if (!cd.loop)
        {
            seq.OnComplete(() =>
            {
                if (image != null)
                {
                    image.color = originalColor;

                    suppressOnDisableCompletion = true;
                    try
                    {
                        RestoreOriginalState(rectTransform);
                    }
                    finally
                    {
                        suppressOnDisableCompletion = false;
                    }

                    image.enabled = false;
                }

                onComplete?.Invoke();
                playbackCompletion?.TrySetResult(true);
            });
        }
        else
        {
            // 루프 모드 — 외부에서 Stop() 호출 필요
            Debug.LogWarning("[EffectCDPlayer] 루프 모드: Stop()을 호출해 종료하세요.");
            playbackCompletion.TrySetResult(true);
        }

        seq.Play();
        await playbackCompletion.Task;
    }

    /// <summary>
    /// 타임아웃 포함 재생 (true: 정상 완료, false: 타임아웃)
    /// </summary>
    public async Task<bool> PlayWithTimeout(
        EffectCD cd,
        float timeoutSeconds = 5f,
        RectTransform targetTransform = null,
        RectTransform casterTransform = null,
        System.Action onComplete = null)
    {
        Task playTask = Play(cd, targetTransform, casterTransform, onComplete);
        Task timeoutTask = Task.Delay((int)(timeoutSeconds * 1000));

        Task completed = await Task.WhenAny(playTask, timeoutTask);

        if (completed == timeoutTask)
        {
            Debug.LogWarning($"[EffectCDPlayer] 타임아웃 ({timeoutSeconds}초). 강제 종료.");
            Stop(resetVisual: true);
            return false;
        }
        return true;
    }

    /// <summary>
    /// 재생 중단
    /// </summary>
    public void Stop(bool resetVisual = true)
    {
        seq?.Kill();
        moveTween?.Kill();

        if (resetVisual && image != null)
        {
            suppressOnDisableCompletion = true;
            try
            {
                RestoreOriginalState(image.rectTransform);
            }
            finally
            {
                suppressOnDisableCompletion = false;
            }

            image.enabled = false;
        }

        playbackCompletion?.TrySetResult(false);
    }

    // ─────────────────────────────────────────────────────────────
    // ScreenMoveRL
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// 화면 우→좌 이동 이펙트. 이동 중 프레임을 루프 재생하고,
    /// 화면 바깥으로 나가면 비활성화.
    /// </summary>
    private async Task PlayScreenMoveRL(
        EffectCD cd,
        RectTransform rectTransform,
        Color originalColor,
        System.Action onComplete)
    {
        RectTransform canvasRect = GetCanvasRectTransform();
        float halfScreen = canvasRect != null ? canvasRect.rect.width / 2f : Screen.width / 2f;
        float halfEffect = rectTransform.sizeDelta.x / 2f;

        Vector2 startPos = new Vector2(halfScreen + halfEffect, 0f);
        Vector2 endPos   = new Vector2(-halfScreen - halfEffect, 0f);

        rectTransform.anchoredPosition = startPos;

        // 프레임 애니메이션 (이동 완료까지 루프)
        seq = DOTween.Sequence();
        int frameCount = cd.frames.Length;
        float frameTime = frameCount > 0 ? Mathf.Max(0.05f, cd.totalDuration / frameCount) : 0.1f;

        for (int i = 0; i < frameCount; i++)
        {
            int fi = cd.playReverse ? (frameCount - 1 - i) : i;
            Sprite sp = cd.frames[fi];
            seq.AppendCallback(() => { if (image != null) image.sprite = sp; });
            if (frameTime > 0f) seq.AppendInterval(frameTime);
        }
        seq.SetLoops(-1, LoopType.Restart);
        seq.Play();

        // 이동 트윈 (totalDuration을 2~3초로 클램프)
        float moveDuration = Mathf.Clamp(cd.totalDuration, 2f, 3f);
        var moveCompletion = new TaskCompletionSource<bool>();

        moveTween = rectTransform.DOAnchorPos(endPos, moveDuration).SetEase(Ease.Linear);
        moveTween.OnComplete(() => moveCompletion.TrySetResult(true));
        moveTween.OnKill(() => moveCompletion.TrySetResult(false));   // 강제 Kill 시에도 해제
        moveTween.Play();

        await moveCompletion.Task;

        // 정리
        seq?.Kill();
        moveTween?.Kill();

        if (image != null)
        {
            image.color = originalColor;

            suppressOnDisableCompletion = true;
            try
            {
                RestoreOriginalState(rectTransform);
            }
            finally
            {
                suppressOnDisableCompletion = false;
            }

            image.enabled = false;
        }

        onComplete?.Invoke();
        playbackCompletion?.TrySetResult(true);
    }

    // ─────────────────────────────────────────────────────────────
    // Setup helpers
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// EffectType에 따라 부모·위치·크기 설정.
    /// 유닛 사망 등으로 Transform이 없으면 false 반환.
    /// </summary>
    private bool SetupPositionAndParent(
        EffectCD cd,
        RectTransform targetTransform,
        RectTransform casterTransform,
        RectTransform rectTransform)
    {
        // 원래 상태 저장
        originalParent    = rectTransform.parent;
        originalPosition  = rectTransform.anchoredPosition;
        originalLocalScale = rectTransform.localScale;
        originalSizeDelta  = rectTransform.sizeDelta;

        RectTransform newParent = null;
        Vector2 offset = Vector2.zero;

        switch (cd.effectType)
        {
            case EffectType.CasterTop:
                if (casterTransform == null)
                {
                    Debug.LogWarning("[EffectCDPlayer] CasterTop: casterTransform이 null (유닛 사망?).");
                    return false;
                }
                newParent = casterTransform;
                offset = new Vector2(0f, casterTransform.sizeDelta.y * (2f / 3f));
                break;

            case EffectType.TargetCenter:
                if (targetTransform == null)
                {
                    Debug.LogWarning("[EffectCDPlayer] TargetCenter: targetTransform이 null (유닛 사망?).");
                    return false;
                }
                newParent = targetTransform;
                offset = Vector2.zero;
                break;

            case EffectType.TargetTop:
                if (targetTransform == null)
                {
                    Debug.LogWarning("[EffectCDPlayer] TargetTop: targetTransform이 null (유닛 사망?).");
                    return false;
                }
                newParent = targetTransform;
                offset = new Vector2(0f, targetTransform.sizeDelta.y * (2f / 3f));
                break;

            case EffectType.ScreenCenter:
            case EffectType.ScreenMoveRL:
                newParent = screenCenter;
                if (newParent == null)
                {
                    Canvas rc = FindObjectOfType<Canvas>();
                    newParent = rc != null ? rc.GetComponent<RectTransform>() : null;
                }
                if (newParent == null)
                {
                    Debug.LogWarning("[EffectCDPlayer] Screen 타입: 루트 Canvas RectTransform을 찾을 수 없음.");
                    return false;
                }
                break;
        }

        // 부모 변경
        if (newParent != null && newParent != (rectTransform.parent as RectTransform))
            rectTransform.SetParent(newParent, worldPositionStays: false);

        // 앵커·피벗 중앙 고정 후 크기 적용
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot     = new Vector2(0.5f, 0.5f);

        switch (cd.effectType)
        {
            case EffectType.CasterTop:
            case EffectType.TargetCenter:
            case EffectType.TargetTop:
                // 초상화와 동일 크기 (1:1)
                rectTransform.sizeDelta = newParent.sizeDelta;
                rectTransform.localScale = Vector3.one;
                break;

            case EffectType.ScreenCenter:
                // 화면 전체 크기
                rectTransform.sizeDelta = newParent.rect.size;
                rectTransform.localScale = Vector3.one;
                break;

            case EffectType.ScreenMoveRL:
                // 에셋 고유 크기 유지 (Sprite 크기 기준)
                rectTransform.localScale = Vector3.one;
                break;
        }

        rectTransform.anchoredPosition = offset;


        return true;
    }

    private void RestoreOriginalState(RectTransform rectTransform)
    {
        if (originalParent != null && rectTransform.parent != originalParent)
            rectTransform.SetParent(originalParent, worldPositionStays: false);

        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localScale       = originalLocalScale;
        rectTransform.sizeDelta        = originalSizeDelta;

        if (effectCanvas != null)
            effectCanvas.sortingOrder = originalSortingOrder;
    }

    private void SetupSortingOrder(EffectCD cd)
    {
        if (effectCanvas == null)
        {
            effectCanvas = GetComponentInParent<Canvas>();
            if (effectCanvas == null)
            {
                effectCanvas = gameObject.AddComponent<Canvas>();
                effectCanvas.overrideSorting = true;
            }
        }

        originalSortingOrder = effectCanvas.sortingOrder;

        switch (cd.effectType)
        {
            case EffectType.ScreenCenter:
            case EffectType.ScreenMoveRL:
                effectCanvas.sortingOrder = 100;
                break;
            default:
                effectCanvas.sortingOrder = 50;
                break;
        }
    }

    private void ApplyFlipX(bool flip)
    {
        var rt = flipRoot != null ? flipRoot : image.rectTransform;
        var s = rt.localScale;
        float absX = Mathf.Abs(s.x);
        rt.localScale = new Vector3(flip ? -absX : absX, s.y, s.z);
    }

    private RectTransform GetCanvasRectTransform()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        return canvas != null ? canvas.GetComponent<RectTransform>() : null;
    }

    // ─────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
            if (image == null)
                Debug.LogError("[EffectCDPlayer] Image 컴포넌트를 찾을 수 없습니다.");
            else
                Debug.Log($"[EffectCDPlayer] Awake: image 자동 할당 ({gameObject.name})");
        }

        if (effectCanvas == null)
            effectCanvas = GetComponentInParent<Canvas>();

        if (screenCenter == null)
        {
            Canvas rootCanvas = FindObjectOfType<Canvas>();
            if (rootCanvas != null)
                screenCenter = rootCanvas.GetComponent<RectTransform>();
        }
    }

    private void OnDisable()
    {
        seq?.Kill();
        moveTween?.Kill();

        if (suppressOnDisableCompletion)
            return;

        playbackCompletion?.TrySetResult(false);
    }
}
