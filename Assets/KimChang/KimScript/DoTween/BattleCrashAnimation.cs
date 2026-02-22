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

    // 사용처: PlayCrashAsync 무기 시작/도착 각도 튜닝 (스프라이트 기본 각도 기준)
    private float mySwordStartZ = 45f;   // 시작: 수직(검끝 위)로 보이게 만드는 각도(대부분 45가 맞음)
    private float mySwordEndZ = 0f;      // 끝: "이미지처럼" 보이게(보통 0)

    // 사용처: PlayCrashAsync 충돌 지점 보정(중앙에서 약간 위/아래로 맞추고 싶을 때)
    private Vector2 crashPointOffset = Vector2.zero;

    // 사용처: PlayCrashAsync 충돌 이펙트 스케일
    private float crashEffectScale = 1f;

    // 사용처: PlayCrashAsync에서 무기가 유닛 중심이 아니라 전방에서 시작하도록 하는 거리
    private float weaponFrontStartDistance = 250f;
    private float crashEffectYOffset = -20f;
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

    // 사용처: 충돌(유닛 뒤로가기 시간 동안 검 정지 → 유닛 앞으로 가는 시간 동안 검 접근/회전 → 마지막에 이펙트 → 회수)
    private async Task PlayCrashAsync(
        RectTransform myAttach,
        RectTransform enemyAttach,
        ObjectPool pool,
        List<int> myStyleIds,
        List<int> enemyStyleIds)
    {
        if (myAttach == null || enemyAttach == null || pool == null) return;

        // 사용처: AutoBattleUI의 공격 시퀀스(뒤로 0.05 + 대기 0.2 + 앞으로 0.2)에 맞춘 타이밍
        // 500ms 기준: preDelay=0.25s / approach=0.2s
        float preDelaySec = Mathf.Clamp(waitingTime * 0.0005f, 0.01f, 2.0f);
        float approachSec = Mathf.Clamp(waitingTime * 0.0004f, 0.05f, 2.0f);
        float crashHoldSec = Mathf.Clamp(waitingTime * 0.0002f, 0.05f, 1.0f);

        // 1) 오브젝트 풀에서 이미지 꺼내기
        GameObject myGo = pool.GetWeaponImage();
        GameObject enemyGo = pool.GetWeaponImage();
        GameObject crashGo = pool.GetCrashEffect();

        // 사용처: 예외 시 풀 반환
        if (myGo == null || enemyGo == null || crashGo == null)
        {
            if (myGo != null) pool.ReturnWeaponImage(myGo);
            if (enemyGo != null) pool.ReturnWeaponImage(enemyGo);
            if (crashGo != null) pool.ReturnCrashEffect(crashGo);
            return;
        }

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
        var canvas = myAttach.GetComponentInParent<Canvas>();
        RectTransform container = canvas ? (RectTransform)canvas.transform
                                         : (myAttach.parent ? (RectTransform)myAttach.parent : myAttach);

        var myRT = (RectTransform)myGo.transform;
        var enemyRT = (RectTransform)enemyGo.transform;
        var crashRT = (RectTransform)crashGo.transform;

        // 사용처: 트윈 잔여분 정리(완료 처리 X)
        myRT.DOKill(false);
        enemyRT.DOKill(false);
        crashRT.DOKill(false);
        DOTween.Kill(myGo, false);
        DOTween.Kill(enemyGo, false);
        DOTween.Kill(crashGo, false);

        // 공통 부모로 부착
        myRT.SetParent(container, false);
        enemyRT.SetParent(container, false);
        crashRT.SetParent(container, false);

        // 앵커: 중앙 고정
        myRT.anchorMin = myRT.anchorMax = new Vector2(0.5f, 0.5f);
        enemyRT.anchorMin = enemyRT.anchorMax = new Vector2(0.5f, 0.5f);
        crashRT.anchorMin = crashRT.anchorMax = new Vector2(0.5f, 0.5f);

        // 피벗: 손잡이 쪽(회전 중심)
        myRT.pivot = enemyRT.pivot = new Vector2(0.5f, 0f);

        // 4) 유닛 위치(attach)를 container 로컬 좌표로 변환
        // 사용처: UI라면 InverseTransformPoint가 빠르고 안정적
        Vector2 myAttachLocal = (Vector2)container.InverseTransformPoint(myAttach.position);
        Vector2 enemyAttachLocal = (Vector2)container.InverseTransformPoint(enemyAttach.position);

        // 방향(내 -> 적)
        Vector2 dir = enemyAttachLocal - myAttachLocal;
        float dist = dir.magnitude;
        if (dist < 0.001f) dir = Vector2.right;
        else dir /= dist;

        // 사용처: 시작점이 충돌점을 넘어가지 않게 클램프
        float frontDist = Mathf.Min(weaponFrontStartDistance, dist * 0.45f);

        // 충돌 지점은 중간 + 보정
        Vector2 crashPoint = (myAttachLocal + enemyAttachLocal) * 0.5f + crashPointOffset;

        // 시작 지점: 유닛 중심이 아니라 "전방"에서 시작
        Vector2 myStart = myAttachLocal + dir * frontDist;
        Vector2 enemyStart = enemyAttachLocal - dir * frontDist;

        // 5) 시작 상태(전방에서 생성 + 수직 각도) → preDelay 동안 정지
        myRT.anchoredPosition = myStart;
        enemyRT.anchoredPosition = enemyStart;

        myRT.localScale = new Vector3(1f, 1f, 1f);
        enemyRT.localScale = new Vector3(-1f, 1f, 1f);

        myRT.localRotation = Quaternion.Euler(0f, 0f, mySwordStartZ);
        enemyRT.localRotation = Quaternion.Euler(0f, 0f, -mySwordStartZ);

        // 사용처: 유닛이 뒤로 가는 동안 + 대기 동안은 검이 가만히 있어야 함
        int preDelayMs = Mathf.RoundToInt(preDelaySec * 1000f);
        if (preDelayMs > 0) await Task.Delay(preDelayMs);

        // 6) 유닛이 앞으로 가는 시간 동안만 접근/회전
        Tween myMove = myRT.DOAnchorPos(crashPoint, approachSec).SetEase(Ease.OutCubic);
        Tween myRot = myRT.DOLocalRotate(new Vector3(0f, 0f, mySwordEndZ), approachSec).SetEase(Ease.OutCubic);

        Tween enMove = enemyRT.DOAnchorPos(crashPoint, approachSec).SetEase(Ease.OutCubic);
        Tween enRot = enemyRT.DOLocalRotate(new Vector3(0f, 0f, -mySwordEndZ), approachSec).SetEase(Ease.OutCubic);

        await Task.WhenAll(Await(myMove), Await(myRot), Await(enMove), Await(enRot));

        // 7) 마지막에 충돌 이펙트 출력
        crashRT.anchoredPosition = crashPoint + new Vector2(0f, crashEffectYOffset);
        crashRT.localRotation = Quaternion.identity;
        crashRT.localScale = Vector3.one * Mathf.Max(0.01f, crashEffectScale);

        int holdMs = Mathf.RoundToInt(crashHoldSec * 1000f);
        if (crashImg != null)
        {
            crashImg.enabled = true;
            if (holdMs > 0) await Task.Delay(holdMs);
            crashImg.enabled = false;
        }
        else
        {
            if (holdMs > 0) await Task.Delay(holdMs);
        }

        // 8) 회수
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


    // 사용처: myAttach/enemyAttach의 월드 좌표를 container(RectTransform) 기준 로컬 좌표로 변환
    private static Vector2 WorldToContainerLocal(RectTransform container, Vector3 worldPos, Camera cam)
    {
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(container, screen, cam, out Vector2 local);
        return local;
    }


}
