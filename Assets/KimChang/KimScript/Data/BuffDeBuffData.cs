using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffDebuffData
{
    public int EffectType { get; set; } // 0: 버프, 1: 디버프
    public int EffectId { get; set; }   // 효과 ID
    public int EffectGrade { get; set; } // 효과 등급
    public int Duration { get; set; }   // 지속 시간

    public BuffDebuffData(int effectId, int effectType,  int effectGrade, int duration)
    {
        EffectId = effectId;
        EffectType = effectType;
        EffectGrade = effectGrade;
        Duration = duration;
    }
}
