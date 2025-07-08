
using System.Collections.Generic;

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

        //사기 계산
        ApplyMoralState();

        //유산 계산
        RelicManager.RunStateRelic();

        //강화 계산
        UpgradeManager.Instance.ProcessUpgrade();
        
    }

    public static void ApplyMoralState()
    {
        int id = 1;
        List<RogueUnitDataBase> myTeam = RogueLikeData.Instance.GetMyTeam();
        int morale = RogueLikeData.Instance.GetMorale();

        float multiplier = 0f;
        if (morale >= 90) multiplier = 0.2f;
        else if (morale >= 70) multiplier = 0.1f;
        else if (morale <= 30) multiplier = -0.1f;
        foreach (var unit in myTeam)
        {
            unit.stats.RemoveModifiersBySource(SourceType.Morale);
            if (unit.bravery && multiplier < 0) continue;
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = unit.baseAttackDamage * multiplier,
                source = SourceType.Field,
                modifierId = id,
                isPercent = false
            });
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Health,
                value = unit.baseHealth * multiplier,
                source = SourceType.Field,
                modifierId = id,
                isPercent = false
            });
        }
    }

    public static void CalculateRunMorale()
    {
        int morale = RogueLikeData.Instance.GetMorale();
        if (morale > 10) return;
        var myTeam = RogueLikeData.Instance.GetMyTeam();

        // 탈주할 유닛이 있으면 무작위로 한 유닛 선택하여 제거
        if (myTeam.Count > 0)
        {
            RogueUnitDataBase leavingUnit = myTeam[UnityEngine.Random.Range(0, myTeam.Count)];
            if(leavingUnit.bravery) return;
            myTeam.Remove(leavingUnit);
            RogueLikeData.Instance.SetMyTeam(myTeam);
        }
    }

}
