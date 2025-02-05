using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RelicManager
{
    // 보유한 유물 중 ID 24, 25, 26을 모두 가지고 있는지 확인하고, 있으면 유물 27 추가
    public static void CheckFusion(ref List<WarRelic> relics)
    {
        // 이미 유물 27을 보유하고 있으면 실행할 필요 없음
        if (relics.Any(relic => relic.id == 27)) return;

        HashSet<int> requiredRelicIds = new HashSet<int> { 24, 25, 26 };
        HashSet<int> ownedRelicIds = new HashSet<int>(relics.Select(relic => relic.id));

        // 모든 필수 유물(24, 25, 26)을 보유하고 있는지 확인
        if (requiredRelicIds.All(id => ownedRelicIds.Contains(id)))
        {
            // RogueLikeData에 유물 27 추가
            RogueLikeData.Instance.AcquireRelic(27);

            // 유물이 정상적으로 존재하는 경우만 추가
            relics.Add(WarRelicDatabase.GetRelicById(27));
            return;
        }

        return;
    }

    //스탯 유산 발동
    public static void RunStateRelic(List<WarRelic> relics, bool curseBlock)
    {
        List<WarRelic> stateRelics = relics.Where(relic => relic.type == RelicType.StateBoost || relic.type == RelicType.ActiveState).ToList();

        if (stateRelics.Count <= 0) return;

        foreach (var relic in stateRelics)
        {
            if (curseBlock && relic.grade == 0)
                continue;

            relic.Execute();
        }
    }

    //유산 15 
    public static float ReactiveThornArmor(UnitDataBase unit, List<WarRelic> relics)
    {
        if (relics.FirstOrDefault(relic => relic.id == 15) == null) return 0;

        if (unit.heavyArmor)
        {
            return (unit.armor * 3);
        }
        return 0;
    }

    //유산 18
    public static bool PhalanxTacticsBook(List<WarRelic> relics)
    {
        return relics.FirstOrDefault(relic => relic.id == 18)?.used ?? false;
    }

    //유산 28
    public static bool HeartGemNecklace(float health, List<WarRelic> relics)
    {
        if (health > 0) return false;
        var relic = relics.FirstOrDefault(relic => relic.id == 28);
        if (relic == null || !relic.used) return false;
        relic.used = true;
        return true;
    }

    /* 유산 30
    public static float BrokenStraightSword(float damage, List<WarRelic> relics)
    {
        if (relics.FirstOrDefault(relic => relic.id == 30) != null)
        {
            // 10% 확률 (0~99 중 0~9가 나올 경우)
            if (UnityEngine.Random.Range(0, 100) < 10)
            {
                return damage * 0.9f; // 데미지 10% 감소
            }
        }

        return damage; // 원래 데미지 반환
    }*/

    /* 유산 33
    public static bool SplitShield(List<WarRelic> relics)
    {
        if (relics.FirstOrDefault(relic => relic.id == 33) != null)
        {
            // 10% 확률 (0~99 중 0~9가 나올 경우)
            if (UnityEngine.Random.Range(0, 100) < 10)
            {
                return true;
            }
        }

        return false;
    }*/

    //유산 35
    public static void Relic35(ref List<UnitDataBase> units, List<WarRelic> relics)
    {
        if (relics.FirstOrDefault(relic => relic.id == 35) == null) return;
        int onlyOne = 0;
        int unitIndex = -1;
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i].health > 0)
            {
                onlyOne++;
                unitIndex = i;
                units[i].attackDamage *= 1.3f;
            }
        }
        if (onlyOne == 1)
        {
            units[unitIndex].attackDamage *= 1.3f;
            //units[unitIndex].armor += 9;
            units[unitIndex].mobility += 4;
        }
    }

    //유산 47
    public static bool TechnicalManual(List<WarRelic> relics)
    {
        if (relics.FirstOrDefault(relic => relic.id == 47) == null) return false;
        return true;
    }

    //유산 55
    public static bool Relic55(List<WarRelic> relics)
    {
        if (relics.FirstOrDefault(relic => relic.id == 55) == null) return false;
        return true;
    }
}
