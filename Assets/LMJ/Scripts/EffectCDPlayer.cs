using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// EffectCD를 재생하는 플레이어 컴포넌트
/// - 프레임 애니메이션 (정방향/역방향)
/// - 색상 변경
/// - 좌우 반전
/// - 이동 애니메이션 (Static/RightToLeft/LeftToRight)
/// - 비동기 재생 지원 (async/await)
/// - 동적 위치/크기 조절 (Target/Caster/Screen)
/// </summary>
public class EffectCDPlayer : MonoBehaviour
{
    [SerializeField] private Image image; // 프레임을 보여줄 이미지 슬롯
    [SerializeField] private Canvas effectCanvas; // 이펙트 전용 Canvas (Sorting Order 관리용)

    [SerializeField] public bool flipX = false; // 적 초상화 위에 생성할 경우 좌우반전 활성화
    [SerializeField] private RectTransform flipRoot; // 반전 적용할 RectTransform (null이면 image.rectTransform 사용)

    [Header("화면 중앙 참조 (Screen 타입용)")]
    [SerializeField] private RectTransform screenCenter; // Screen 타입 이펙트의 중앙 위치

    private Sequence seq;
    private Vector2 originalPosition; // 이동 애니메이션 후 복원용
    private Transform originalParent; // 원래 부모 (복원용)
    private Vector3 originalScale; // 원래 스케일 (복원용)
    private int originalSortingOrder; // 원래 Sorting Order (복원용)
    private TaskCompletionSource<bool> playbackCompletion; // 비동기 재생 완료 신호용

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
    /// EffectCD를 비동기로 재생 (await 가능)
    /// </summary>
    /// <param name="cd">재생할 EffectCD 데이터</param>
    /// <param name="targetTransform">Target 타입일 때 피격 유닛의 Transform</param>
    /// <param name="casterTransform">Caster 타입일 때 시전자 유닛의 Transform</param>
    /// <param name="onComplete">재생 완료 시 호출될 콜백 (옵션)</param>
    /// <returns>재생 완료를 대기할 수 있는 Task</returns>
    public async Task Play(EffectCD cd, RectTransform targetTransform = null, RectTransform casterTransform = null, System.Action onComplete = null)
    {
        if (cd == null || cd.frames == null || cd.frames.Length == 0)
        {
            Debug.LogWarning("[EffectCDPlayer] CD가 비어있거나 프레임이 없습니다.");
            onComplete?.Invoke();
            return;
        }

        // 이전 시퀀스 및 Task 정리
        seq?.Kill();
        playbackCompletion?.TrySetResult(false); // 이전 대기 중인 Task 취소
        playbackCompletion = new TaskCompletionSource<bool>();

        seq = DOTween.Sequence();

        // 위치 및 부모 설정 (EffectType에 따라)
        RectTransform rectTransform = image.rectTransform;
        SetupPositionAndParent(cd, targetTransform, casterTransform, rectTransform);

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

        // Sorting Order 설정 (Screen 타입은 최상위)
        SetupSortingOrder(cd);

        // 이동 애니메이션 설정
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
                    // 색상, 위치, 부모, 스케일, Sorting Order 복원
                    image.color = originalColor;
                    RestoreOriginalState(rectTransform);
                    image.enabled = false;
                }

                // 콜백 실행
                onComplete?.Invoke();

                // Task 완료 신호
                playbackCompletion?.TrySetResult(true);
            });
        }
        else
        {
            // 루프 모드는 즉시 완료 (무한 재생이므로 대기하지 않음)
            Debug.LogWarning("[EffectCDPlayer] 루프 모드는 자동 완료되지 않습니다. Stop()을 호출하세요.");
            playbackCompletion.TrySetResult(true);
        }

        // 시퀀스 재생
        seq.Play();

        // 재생 완료 대기
        await playbackCompletion.Task;
    }

    /// <summary>
    /// EffectCD를 타임아웃과 함께 비동기로 재생
    /// </summary>
    /// <param name="cd">재생할 EffectCD 데이터</param>
    /// <param name="timeoutSeconds">최대 대기 시간 (초, 기본값 5초)</param>
    /// <param name="targetTransform">Target 타입일 때 피격 유닛의 Transform</param>
    /// <param name="casterTransform">Caster 타입일 때 시전자 유닛의 Transform</param>
    /// <param name="onComplete">재생 완료 시 호출될 콜백</param>
    /// <returns>재생 완료 여부 (true: 정상 완료, false: 타임아웃)</returns>
    public async Task<bool> PlayWithTimeout(EffectCD cd, float timeoutSeconds = 5f, RectTransform targetTransform = null, RectTransform casterTransform = null, System.Action onComplete = null)
    {
        Task playTask = Play(cd, targetTransform, casterTransform, onComplete);
        Task timeoutTask = Task.Delay((int)(timeoutSeconds * 1000));

        Task completedTask = await Task.WhenAny(playTask, timeoutTask);

        if (completedTask == timeoutTask)
        {
            Debug.LogWarning($"[EffectCDPlayer] 이펙트 재생 타임아웃 ({timeoutSeconds}초 초과). 강제 종료합니다.");
            Stop(resetVisual: true);
            return false; // 타임아웃
        }

        return true; // 정상 완료
    }

    /// <summary>
    /// 원래 상태로 복원 (부모, 위치, 스케일, Sorting Order)
    /// </summary>
    private void RestoreOriginalState(RectTransform rectTransform)
    {
        // 부모 복원
        if (originalParent != null && rectTransform.parent != originalParent)
        {
            rectTransform.SetParent(originalParent, worldPositionStays: false);
        }

        // 위치 복원
        rectTransform.anchoredPosition = originalPosition;

        // 스케일 복원
        rectTransform.localScale = originalScale;

        // Sorting Order 복원
        if (effectCanvas != null)
        {
            effectCanvas.sortingOrder = originalSortingOrder;
        }
    }

    /// <summary>
    /// EffectType에 따라 위치, 부모, 스케일 설정
    /// </summary>
    private void SetupPositionAndParent(EffectCD cd, RectTransform targetTransform, RectTransform casterTransform, RectTransform rectTransform)
    {
        // 원래 상태 저장
        originalParent = rectTransform.parent;
        originalPosition = rectTransform.anchoredPosition;
        originalScale = rectTransform.localScale;

        RectTransform newParent = null;

        switch (cd.effectType)
        {
            case EffectType.Target:
                newParent = targetTransform;
                break;

            case EffectType.Caster:
                newParent = casterTransform;
                break;

            case EffectType.Screen:
                // 화면 중앙 (screenCenter가 없으면 원래 부모 유지)
                newParent = screenCenter != null ? screenCenter : rectTransform.parent as RectTransform;
                break;
        }

        // 부모 변경
        if (newParent != null && newParent != rectTransform.parent)
        {
            rectTransform.SetParent(newParent, worldPositionStays: false);
        }

        // 위치 초기화 (부모 기준 중앙)
        rectTransform.anchoredPosition = Vector2.zero;
        originalPosition = Vector2.zero;

        // 동적 스케일링: 부모 크기에 맞춰 조절
        ApplyDynamicScaling(cd, newParent, rectTransform);
    }

    /// <summary>
    /// 부모 크기에 맞춰 동적 스케일링
    /// </summary>
    private void ApplyDynamicScaling(EffectCD cd, RectTransform parentRect, RectTransform rectTransform)
    {
        if (parentRect == null)
            return;

        // 부모의 크기 가져오기
        Vector2 parentSize = parentRect.rect.size;

        // Screen 타입은 부모 크기에 맞추지 않음 (고정 크기 사용)
        if (cd.effectType == EffectType.Screen)
        {
            rectTransform.localScale = Vector3.one;
            return;
        }

        // Target/Caster 타입: 부모 초상화 크기에 맞춰 조절
        // 기준 크기를 200x200으로 가정하고 비율 계산
        float baseSize = 200f;
        float scaleX = parentSize.x / baseSize;
        float scaleY = parentSize.y / baseSize;
        float uniformScale = Mathf.Min(scaleX, scaleY); // 비율 유지

        rectTransform.localScale = Vector3.one * uniformScale;
    }

    /// <summary>
    /// Sorting Order 설정 (레이어 관리)
    /// </summary>
    private void SetupSortingOrder(EffectCD cd)
    {
        if (effectCanvas == null)
        {
            // Canvas 컴포넌트 찾기 (없으면 생성)
            effectCanvas = GetComponentInParent<Canvas>();
            if (effectCanvas == null)
            {
                effectCanvas = gameObject.AddComponent<Canvas>();
                effectCanvas.overrideSorting = true;
            }
        }

        // 원래 Sorting Order 저장
        originalSortingOrder = effectCanvas.sortingOrder;

        // Screen 타입은 최상위 레이어 (초상화 위)
        if (cd.effectType == EffectType.Screen)
        {
            effectCanvas.sortingOrder = 100; // 전장 효과는 가장 위
        }
        else
        {
            effectCanvas.sortingOrder = 50; // Target/Caster는 중간 레이어
        }
    }

    /// <summary>
    /// 이동 타입에 따른 애니메이션 적용 (강화 버전)
    /// </summary>
    private void ApplyMovementAnimation(EffectCD cd, RectTransform rectTransform)
    {
        if (cd.moveType == EffectMoveType.Static)
        {
            // 고정: 이동하지 않음
            return;
        }

        Vector2 startPos = Vector2.zero;
        Vector2 endPos = Vector2.zero;

        // Screen 타입이고 이동이 있을 때: 화면 끝에서 끝으로 이동
        if (cd.effectType == EffectType.Screen)
        {
            // Canvas의 너비를 기준으로 화면 끝 계산
            RectTransform canvasRect = GetCanvasRectTransform();
            float screenWidth = canvasRect != null ? canvasRect.rect.width : Screen.width;

            switch (cd.moveType)
            {
                case EffectMoveType.RightToLeft:
                    // 화면 오른쪽 끝 → 왼쪽 끝
                    startPos.x = screenWidth / 2f;
                    endPos.x = -screenWidth / 2f;
                    break;

                case EffectMoveType.LeftToRight:
                    // 화면 왼쪽 끝 → 오른쪽 끝
                    startPos.x = -screenWidth / 2f;
                    endPos.x = screenWidth / 2f;
                    break;
            }
        }
        else
        {
            // Target/Caster 타입: 상대적 이동 (moveDistance 사용)
            switch (cd.moveType)
            {
                case EffectMoveType.RightToLeft:
                    // 우→좌 이동
                    startPos.x = cd.moveDistance;
                    endPos.x = -cd.moveDistance;
                    break;

                case EffectMoveType.LeftToRight:
                    // 좌→우 이동
                    startPos.x = -cd.moveDistance;
                    endPos.x = cd.moveDistance;
                    break;
            }
        }

        // 시작 위치 설정
        rectTransform.anchoredPosition = startPos;

        // 이동 애니메이션 추가 (Ease 적용으로 자연스러운 이동)
        Ease moveEase = cd.effectType == EffectType.Screen ? Ease.Linear : Ease.OutQuad;
        seq.Insert(0, rectTransform.DOAnchorPos(endPos, cd.totalDuration).SetEase(moveEase));
    }

    /// <summary>
    /// Canvas의 RectTransform 가져오기
    /// </summary>
    private RectTransform GetCanvasRectTransform()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            // 최상위 Canvas 찾기
            canvas = FindObjectOfType<Canvas>();
        }
        return canvas != null ? canvas.GetComponent<RectTransform>() : null;
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
            // 모든 상태 복원
            RestoreOriginalState(image.rectTransform);
            image.enabled = false;
        }

        // 대기 중인 Task 취소 (타임아웃 등)
        playbackCompletion?.TrySetResult(false);
    }

    private void OnDisable()
    {
        seq?.Kill();
        playbackCompletion?.TrySetResult(false);
    }

    private void Awake()
    {
        // 초기화
        if (effectCanvas == null)
        {
            effectCanvas = GetComponentInParent<Canvas>();
        }

        // screenCenter가 없으면 자동 탐색 (Canvas 중앙)
        if (screenCenter == null)
        {
            Canvas rootCanvas = FindObjectOfType<Canvas>();
            if (rootCanvas != null)
            {
                screenCenter = rootCanvas.GetComponent<RectTransform>();
            }
        }
    }
}
