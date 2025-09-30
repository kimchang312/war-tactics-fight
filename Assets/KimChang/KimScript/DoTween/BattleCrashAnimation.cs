using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 전투 애니메이션 총괄 컴포넌트.
/// 사용처: 페이즈별(입장, 체크, 시작, 준비, 충돌, 지원, 사망) 애니메이션을 실행하고, 완료될 때까지 대기시킴.
/// </summary>
public sealed class BattleCrashAnimation : MonoBehaviour
{
    [Header("속도 제어(ms)")]
    [Tooltip("500 = 기본, 250 = 2배속, 1000 = 0.5배속")]
    public float waitingTime = 500;

    // 내부 상수(위치 오프셋)
    private static readonly Vector2 MyStartOffset = new Vector2(-50f, 0f);
    private static readonly Vector2 EnemyStartOffset = new Vector2(+50f, 0f);

    // 사용처: Tween 완료 대기
    private static Task Await(Tween t)
    {
        var tcs = new TaskCompletionSource<bool>();
        if (t == null || !t.IsActive()) { tcs.TrySetResult(true); return tcs.Task; }
        t.OnComplete(() => tcs.TrySetResult(true));
        return tcs.Task;
    }

    // 사용처: 페이즈별 애니메이션 공용 진입점
    public async Task PlayPhaseAsync(
        int phase,                 // 0=입장,1=체크,2=시작,3=준비,4=충돌,5=지원,6=사망
        RectTransform myAttach,    // 내 유닛 기준 부착 지점
        RectTransform enemyAttach, // 적 유닛 기준 부착 지점
        ObjectPool pool,           // 이미지 풀
        List<int> myStyleIds = null,   // 연출용 id (충돌 등에서 스프라이트 선택)
        List<int> enemyStyleIds = null // 연출용 id
    )
    {
        switch (phase)
        {
            case 0: await PlayEnterAsync(myAttach, enemyAttach, pool); break;
            case 1: await PlayCheckAsync(myAttach, enemyAttach, pool); break;
            case 2: await PlayStartAsync(myAttach, enemyAttach, pool); break;
            case 3: await PlayPreparationAsync(myAttach, enemyAttach, pool); break;
            case 4: await PlayCrashAsync(myAttach, enemyAttach, pool, myStyleIds, enemyStyleIds); break;
            case 5: await PlaySupportAsync(myAttach, enemyAttach, pool); break;
            case 6: await PlayDeathAsync(myAttach, enemyAttach, pool); break;
        }
    }

    // 사용처: 입장(전투 씬 들어올 때 간단한 등장 연출)
    private async Task PlayEnterAsync(RectTransform myAttach, RectTransform enemyAttach, ObjectPool pool)
    {
        float sec = Mathf.Max(0.05f, waitingTime * 0.001f);
        // 경량: 페이드/살짝 이동 정도의 간단 연출 (이미지 풀 사용 X)
        await Task.Delay((int)(sec * 1000f));
    }

    // 사용처: 체크(전투 시작 전 확인 단계)
    private async Task PlayCheckAsync(RectTransform myAttach, RectTransform enemyAttach, ObjectPool pool)
    {
        float sec = Mathf.Max(0.05f, waitingTime * 0.001f);
        await Task.Delay((int)(sec * 1000f));
    }

    // 사용처: 시작(전투 당 1회)
    private async Task PlayStartAsync(RectTransform myAttach, RectTransform enemyAttach, ObjectPool pool)
    {
        float sec = Mathf.Max(0.05f, waitingTime * 0.001f);
        await Task.Delay((int)(sec * 1000f));
    }

    // 사용처: 준비(사이드 흔들림 등 간단한 텔레그래프 연출)
    private async Task PlayPreparationAsync(RectTransform myAttach, RectTransform enemyAttach, ObjectPool pool)
    {
        float sec = Mathf.Max(0.05f, waitingTime * 0.001f);
        await Task.Delay((int)(sec * 1000f));
    }

    // 사용처: 충돌(양 무기가 서로 접근 → 회전 해제 → 중앙 충돌 이펙트 → 회수)
    private async Task PlayCrashAsync(
        RectTransform myAttach,
        RectTransform enemyAttach,
        ObjectPool pool,
        List<int> myStyleIds,
        List<int> enemyStyleIds)
    {
        if (myAttach == null || enemyAttach == null || pool == null) return;

        // 1) 오브젝트 풀에서 이미지 꺼내기
        GameObject myGo = pool.GetWeaponImage();
        GameObject enemyGo = pool.GetWeaponImage();
        GameObject crashGo = pool.GetCrashEffect();

        var myImg = myGo.GetComponent<Image>();
        var enemyImg = enemyGo.GetComponent<Image>();
        var crashImg = crashGo.GetComponent<Image>();

        // 2) 스프라이트 세팅 (현재는 첫 번째 id만 사용)
        int myId = (myStyleIds != null && myStyleIds.Count > 0) ? myStyleIds[0] : 0;
        int enemyId = (enemyStyleIds != null && enemyStyleIds.Count > 0) ? enemyStyleIds[0] : 0;

        if (myImg) { myImg.sprite = SpriteCacheManager.GetSprite($"BattleAnimation/Crash_{myId}"); myImg.enabled = true; }
        if (enemyImg) { enemyImg.sprite = SpriteCacheManager.GetSprite($"BattleAnimation/Crash_{enemyId}"); enemyImg.enabled = true; }
        if (crashImg) crashImg.enabled = false;

        // 3) 공통 부모 컨테이너(전투용 Canvas)로 정렬
        //    - 중앙(0,0)을 기준으로 모두 같은 좌표계에서 애니메이션
        var canvas = myAttach.GetComponentInParent<Canvas>();
        RectTransform container = canvas ? (RectTransform)canvas.transform
                                         : (myAttach.parent ? (RectTransform)myAttach.parent : myAttach);

        var myRT = (RectTransform)myGo.transform;
        var enemyRT = (RectTransform)enemyGo.transform;
        var crashRT = (RectTransform)crashGo.transform;

        // 트윈 잔여분 정리
        myRT.DOKill(false);
        enemyRT.DOKill(false);
        crashRT.DOKill(false);

        // 공통 부모로 부착
        myRT.SetParent(container, false);
        enemyRT.SetParent(container, false);
        crashRT.SetParent(container, false);

        // 앵커/피벗: 중앙 고정
        myRT.anchorMin = myRT.anchorMax = new Vector2(0.5f, 0.5f);
        enemyRT.anchorMin = enemyRT.anchorMax = new Vector2(0.5f, 0.5f);
        crashRT.anchorMin = crashRT.anchorMax = new Vector2(0.5f, 0.5f);

        myRT.pivot = enemyRT.pivot = new Vector2(0.5f, 0f);

        // 4) 시작 상태: 내 무기(-50,0, rot 45, scale.x=1), 적 무기(+50,0, rot -45, scale.x=-1)
        myRT.anchoredPosition = new Vector2(-50f, 0f);
        enemyRT.anchoredPosition = new Vector2(+50f, 0f);

        myRT.localScale = new Vector3(1f, 1f, 1f);
        enemyRT.localScale = new Vector3(-1f, 1f, 1f);

        myRT.localRotation = Quaternion.Euler(0f, 0f, 45f);
        enemyRT.localRotation = Quaternion.Euler(0f, 0f, -45f);

        // 5) 이동/회전 트윈: 둘 다 중앙(0,0), 회전 0도로
        float totalSec = Mathf.Max(0.05f, waitingTime * 0.001f); // 기본 0.5초
        float moveSec = Mathf.Clamp(totalSec - 0.2f, 0.05f, 2f); // 마지막 0.2초는 크래시 유지

        var myMove = myRT.DOAnchorPos(Vector2.zero, moveSec).SetEase(Ease.OutCubic);
        var myRotate = myRT.DOLocalRotate(Vector3.zero, moveSec).SetEase(Ease.OutCubic);

        var enMove = enemyRT.DOAnchorPos(Vector2.zero, moveSec).SetEase(Ease.OutCubic);
        var enRotate = enemyRT.DOLocalRotate(Vector3.zero, moveSec).SetEase(Ease.OutCubic);

        await Task.WhenAll(Await(myMove), Await(myRotate), Await(enMove), Await(enRotate));

        // 6) 크래시 이펙트: 중심(0,0)에 0.2초 표시
        crashRT.anchoredPosition = Vector2.zero;
        crashRT.localScale = Vector3.one;
        crashRT.localRotation = Quaternion.identity;

        if (crashImg != null)
        {
            crashImg.enabled = true;
            await Task.Delay(200);
            crashImg.enabled = false;
        }

        // 7) 회수
        pool.ReturnWeaponImage(myGo);
        pool.ReturnWeaponImage(enemyGo);
        pool.ReturnCrashEffect(crashGo);
    }


    // 사용처: 지원(원거리 투사체 등은 추후 확장, 지금은 타이밍만 유지)
    private async Task PlaySupportAsync(RectTransform myAttach, RectTransform enemyAttach, ObjectPool pool)
    {
        float sec = Mathf.Max(0.05f, waitingTime * 0.001f);
        await Task.Delay((int)(sec * 1000f));
    }

    // 사용처: 사망(사망자 흔들림/알파 다운 등 추후 확장 가능)
    private async Task PlayDeathAsync(RectTransform myAttach, RectTransform enemyAttach, ObjectPool pool)
    {
        float sec = Mathf.Max(0.05f, waitingTime * 0.001f);
        await Task.Delay((int)(sec * 1000f));
    }

    public void ChangeWattingTime(float time)
    {
        waitingTime *= time;
    }
}
