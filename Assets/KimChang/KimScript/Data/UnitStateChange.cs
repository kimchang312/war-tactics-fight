
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class UnitStateChange
{
    //유닛 능력치 정상화 유산, 사기, 강화 적용/ 스테이지 선택 시, 보상 획득 시 발동
    public static void ChangeStateMyUnits()
    {
        var myTeam = RogueLikeData.Instance.GetMyTeam();

        foreach (var team in myTeam)
        {
            team.NormalizeStateModifiers();
        }

        //유닛 계산
        ApplyUnitAbility();

        //사기 계산
        ApplyMoralState();

        //유산 계산
        RelicManager.RunStateRelic();

        //강화 계산
        UpgradeManager.Instance.ProcessUpgrade();
        
    }
    //사기 계산 함수
    public static void ApplyMoralState()
    {
        List<RogueUnitDataBase> myTeam = RogueLikeData.Instance.GetMyTeam();
        float m = GetMoraleMultiplier(RogueLikeData.Instance.GetMorale());

        foreach (var unit in myTeam)
        {
            // 기존 사기 효과 제거
            unit.stats.RemoveModifiersBySource(SourceType.Morale);

            // 사기 하락은 용기(bravery) 유닛에 미적용
            if (m < 0f && unit.bravery) continue;
            if (m == 0f) continue;

            // 퍼센트 수정자 적용
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = m,
                source = SourceType.Morale,
                isPercent = false
            });
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Health,
                value = m,
                source = SourceType.Morale,
                isPercent = false
            });
        }
    }

    // 사용처: 사기 구간별 배수 산출
    private static float GetMoraleMultiplier(int morale)
    {
        if (morale >= 90) return 0.20f;
        if (morale >= 70) return 0.10f;
        if (morale <= 30) return -0.10f;
        return 0f;
    }


    public static RogueUnitDataBase CalculateRunMorale()
    {
        int morale = RogueLikeData.Instance.GetMorale();
        if (morale > 10) return null;
        var myUnits = RogueLikeData.Instance.GetMyUnits();

        // 탈주할 유닛이 있으면 무작위로 한 유닛 선택하여 제거
        if (myUnits.Count > 0)
        {
            RogueUnitDataBase leavingUnit = myUnits[UnityEngine.Random.Range(0, myUnits.Count)];
            if(leavingUnit.bravery) return null;
            var myTeam = RogueLikeData.Instance.GetMyTeam();
            myUnits.Remove(leavingUnit);
            myTeam.Remove(leavingUnit);
            RogueLikeData.Instance.SetAllMyUnits(myUnits);
            RogueLikeData.Instance.SetMyTeam(myTeam);
            return leavingUnit;
        }
        return null;
    }

    public static StringBuilder GetUnitStatusDetail(RogueUnitDataBase unit, int stateId)
    {
        StringBuilder result = new();
        StatBlock statBlock = unit.stats;
        IEnumerable<StatModifier> modifiers = statBlock.GetAllModifiers();

        // stateId를 StatType으로 변환
        StatType statType = stateId switch
        {
            0 => StatType.Mobility,
            1 => StatType.Health,
            2 => StatType.Armor,
            3 => StatType.AttackDamage,
            4 => StatType.Range,
            _ => throw new ArgumentOutOfRangeException(nameof(stateId), $"Invalid stateId: {stateId}")
        };

        // 기본값
        float baseValue = statType switch
        {
            StatType.Health => statBlock.baseHealth,
            StatType.AttackDamage => statBlock.baseAttackDamage,
            StatType.Armor => statBlock.baseArmor,
            StatType.Range => statBlock.baseRange,
            StatType.Mobility => statBlock.baseMobility,
            _ => 0
        };

        // 원천별 flat 보너스 계산
        Dictionary<SourceType, float> sourceValues = new();
        float flatBonus = 0;

        foreach (var mod in modifiers)
        {
            if (mod.stat != statType || mod.isPercent) continue;

            if (!sourceValues.ContainsKey(mod.source))
                sourceValues[mod.source] = 0;

            sourceValues[mod.source] += mod.value;
            flatBonus += mod.value;
        }

        float percentBonus = modifiers
            .Where(m => m.stat == statType && m.isPercent)
            .Sum(m => m.value);

        float finalValue = (baseValue + flatBonus) * (1 + percentBonus);

        // 결과 조립 (괄호는 한 번만)
        result.Append($"{GameTextData.GetLocalizedTextFull(stateId + 100).Name}: {finalValue:0.#} (");
        result.Append($"{baseValue:0.#}");

        foreach (var kvp in sourceValues)
        {
            result.Append($"+{kvp.Value:0.#}");
        }

        result.Append(")");

        return result;
    }

    private static void ApplyUnitAbility()
    {
        RogueUnitDataBase.PassiveBizarreBishop();
    }

}
