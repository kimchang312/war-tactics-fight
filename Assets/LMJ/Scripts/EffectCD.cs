using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이펙트가 표시될 위치 타입
/// </summary>
public enum EffectType
{
    CasterTop,      // 시전자 초상화 상단 (정중앙 기준 높이 2/3 위)
    TargetCenter,   // 대상 초상화 정중앙
    TargetTop,      // 대상 초상화 상단 (정중앙 기준 높이 2/3 위)
    ScreenCenter,   // 화면 중앙 전체 화면
    ScreenMoveRL    // 화면 오른쪽 바깥 → 왼쪽 바깥 이동 (폭풍우, 흡혈 등)
}

[CreateAssetMenu(menuName = "Effect/Effect CD", fileName = "NewEffectCD")]
public class EffectCD : ScriptableObject
{
    [Tooltip("재생할 이미지(프레임) 리스트")]
    public Sprite[] frames;

    [Tooltip("전체 재생 시간 (초) / ScreenMoveRL의 경우 화면 횡단 시간 (2~3초 권장)")]
    public float totalDuration = 1.0f;

    [Header("재생 옵션")]
    [Tooltip("체크 시 역재생 (마지막 프레임 → 첫 프레임)")]
    public bool playReverse = false;

    [Tooltip("체크 시 무한 루프 재생 (ScreenMoveRL은 이동 중 자동 루프)")]
    public bool loop = false;

    [Tooltip("재생 동안 이미지 색상")]
    public Color playColor = Color.white;

    [Tooltip("이 이펙트가 좌우 반전을 허용하는지 여부 (기본: 허용)")]
    [SerializeField] private bool allowFlip = true;
    public bool AllowFlip => allowFlip;

    [Header("위치 설정")]
    [Tooltip("이펙트 표시 위치 타입")]
    public EffectType effectType = EffectType.TargetCenter;
}
