using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 버튼 클릭으로 EffectCD를 재생하는 테스트용 컴포넌트
/// </summary>
public class EffectCDButton : MonoBehaviour
{
    [SerializeField] private EffectCDPlayer player; // 플레이어 지정
    [SerializeField] private EffectCD cd;           // 재생할 CD 지정

    public bool flipX = false;

    [Header("타임아웃 설정")]
    [SerializeField] private bool useTimeout = true;
    [SerializeField] private float timeoutSeconds = 5f;

    [Header("위치 테스트 (옵션)")]
    [SerializeField] private RectTransform testTargetTransform; // 테스트용 Target
    [SerializeField] private RectTransform testCasterTransform; // 테스트용 Caster

    // 버튼 클릭 시 호출 (동기)
    public void PlayCD()
    {
        _ = PlayCDAsync(); // Fire-and-forget (결과를 기다리지 않음)
    }

    // 비동기 재생 메서드
    public async Task PlayCDAsync()
    {
        if (player == null || cd == null)
        {
            Debug.LogWarning("[EffectCDButton] player 또는 cd가 비어 있음!");
            return;
        }

        player.flipX = this.flipX;

        if (useTimeout)
        {
            bool completed = await player.PlayWithTimeout(
                cd,
                timeoutSeconds,
                testTargetTransform,
                testCasterTransform
            );
            if (!completed)
            {
                Debug.Log($"[EffectCDButton] 이펙트 재생 타임아웃 ({timeoutSeconds}초)");
            }
        }
        else
        {
            await player.Play(cd, testTargetTransform, testCasterTransform);
        }
    }
}
