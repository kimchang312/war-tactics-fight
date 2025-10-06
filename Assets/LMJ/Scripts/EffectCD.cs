using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}