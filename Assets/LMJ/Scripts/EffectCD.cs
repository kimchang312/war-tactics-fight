using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이펙트가 표시될 위치 타입
/// </summary>
public enum EffectType
{
    Target,     // 피격자 위에 표시
    Caster,     // 시전자 위에 표시
    Screen      // 전장 전체(화면 중앙 또는 고정 위치)
}

/// <summary>
/// 이펙트의 이동 패턴
/// </summary>
public enum EffectMoveType
{
    Static,         // 고정 (제자리에서 재생)
    RightToLeft,    // 우→좌 이동 (아군 → 적군)
    LeftToRight     // 좌→우 이동 (적군 → 아군)
}

[CreateAssetMenu(menuName = "Effect/Effect CD", fileName = "NewEffectCD")]
public class EffectCD : ScriptableObject
{
    [Tooltip("재생할 이미지(프레임) 리스트")]
    public Sprite[] frames;

    [Tooltip("전체 재생 시간 (초)")]
    public float totalDuration = 1.0f;

    [Header("재생 옵션")]
    [Tooltip("체크 시 역재생(마지막 프레임 → 첫 프레임)")]
    public bool playReverse = false;

    [Tooltip("체크 시 무한 루프 재생")]
    public bool loop = false;

    [Tooltip("재생 동안 이미지 색상")]
    public Color playColor = Color.white;

    [Tooltip("이 이펙트가 좌우 반전을 허용하는지 여부(기본: 허용)")]
    [SerializeField] private bool allowFlip = true;
    public bool AllowFlip => allowFlip; // 읽기 전용 공개

    [Header("위치 및 이동 설정")]
    [Tooltip("이펙트 표시 위치 타입")]
    public EffectType effectType = EffectType.Target;

    [Tooltip("이펙트 이동 패턴")]
    public EffectMoveType moveType = EffectMoveType.Static;

    [Tooltip("이동 거리 (Static이 아닐 때 적용, 픽셀 단위)")]
    public float moveDistance = 200f;
}