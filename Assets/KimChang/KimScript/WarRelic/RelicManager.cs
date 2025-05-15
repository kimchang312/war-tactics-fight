using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class RelicManager
{
    // 보유 유물 (Dictionary 형태로 변경)
    private static Dictionary<int, WarRelic> ownedRelics = new Dictionary<int, WarRelic>();
    private static bool curseBlock;
    public enum RelicAction
    {
        Acquire,
        Remove
    }

    // 유산 저장
    public static void GetRelicData()
    {
        ownedRelics.Clear();

        // 중복을 방지하며 유산 추가하는 메서드
        void AddRelics(List<WarRelic> relics)
        {
            foreach (var relic in relics)
            {
                ownedRelics.TryAdd(relic.id, relic);  // 중복되지 않으면 추가
            }
        }

        // 유물 데이터 가져오기
        AddRelics(RogueLikeData.Instance.GetRelicsByType(RelicType.AllEffect));
        AddRelics(RogueLikeData.Instance.GetRelicsByType(RelicType.SpecialEffect));
        AddRelics(RogueLikeData.Instance.GetRelicsByType(RelicType.StateBoost));
        AddRelics(RogueLikeData.Instance.GetRelicsByType(RelicType.BattleActive));
        AddRelics(RogueLikeData.Instance.GetRelicsByType(RelicType.ActiveState));
    }
    // 특정 등급에서 중복 여부를 고려하여 랜덤 유산들 반환
    public static List<WarRelic> GetAvailableRelics(int grade, RelicAction action)
    {
        if (grade == 5)
        {
            grade = UnityEngine.Random.value < 0.2f ? 10 : 1;
        }

        var relics = WarRelicDatabase.relics.Where(r => r.grade == grade).ToList();
        var ownedIds = RogueLikeData.Instance.GetAllOwnedRelicIds().ToHashSet();

        return (action == RelicAction.Acquire)
            ? relics.Where(r => !ownedIds.Contains(r.id)).ToList()
            : relics.Where(r => ownedIds.Contains(r.id)).ToList();
    }
    //랜덤 유산id하나 반환
    public static int GetRandomRelicId(int grade, RelicAction action)
    {
        // Relic 6 효과: 일반 → 전설로 1회 업그레이드
        if (grade == 1)
        {
            var relic = RogueLikeData.Instance.GetOwnedRelicById(6);
            if (relic != null && !relic.used)
            {
                relic.used = true;
                grade = 10;
            }
        }

        var available = GetAvailableRelics(grade, action);
        if (available.Count == 0) return -1;

        return available[UnityEngine.Random.Range(0, available.Count)].id;
    }


    //무작위 유산 추가 || 제거
    public static WarRelic HandleRandomRelic(int grade, RelicAction action)
    {
        // Relic 6 효과 적용
        if (grade == 1)
        {
            var relic = RogueLikeData.Instance.GetOwnedRelicById(6);
            if (relic != null && !relic.used)
            {
                relic.used = true;
                grade = 10;
            }
        }

        var available = GetAvailableRelics(grade, action);
        if (available.Count == 0) return null;

        var selected = available[UnityEngine.Random.Range(0, available.Count)];

        if (action == RelicAction.Acquire)
        {
            RogueLikeData.Instance.AcquireRelic(selected.id);
            ownedRelics.TryAdd(selected.id, selected);
        }
        else if (action == RelicAction.Remove)
        {
            RogueLikeData.Instance.RemoveRelicById(selected.id);
            ownedRelics.Remove(selected.id);
        }

        return selected;
    }


    //
    public static void CheckFusion()
    {
        if (ownedRelics.ContainsKey(26)) return;

        int[] requiredRelicIds = { 23, 24, 25 };

        if (requiredRelicIds.All(id => ownedRelics.ContainsKey(id)))
        {
            foreach (int id in requiredRelicIds)
            {
                ownedRelics[id].used = true;
            }

            WarRelic newRelic = WarRelicDatabase.GetRelicById(26);
            if (newRelic != null)
            {
                RogueLikeData.Instance.AcquireRelic(26);
                ownedRelics.TryAdd(26, newRelic); // 유물 26 추가
            }
        }
    }
    public static int RunStateRelic()
    {
        var stateRelics = ownedRelics.Values
            .Where(relic => relic.type == RelicType.StateBoost || relic.type == RelicType.ActiveState)
            .ToList();

        if (stateRelics.Count <= 0) return stateRelics.Count;
        curseBlock = CheckRelicById(22);
        
        foreach (var relic in stateRelics)
        {
            if (curseBlock && relic.grade == 0)
                continue;

            relic.Execute();
        }
        return 1;
    }

    public static bool CheckRelicById(int relicId)
    {
        return ownedRelics.ContainsKey(relicId) && !ownedRelics[relicId].used;
    }

    // 유산 34 생존자의 넝마떼기
    public static void SurvivorOfRag(List<RogueUnitDataBase> units,bool isTeam)
    {
        if (!isTeam) return;
        if (!ownedRelics.ContainsKey(34)) return;

        int onlyOne = 0;
        int unitIndex = -1;

        for (int i = 0; i < units.Count; i++)
        {
            if (units[i].health > 0)
            {
                onlyOne++;
                unitIndex = i;
                units[i].attackDamage += Mathf.Round(units[i].baseAttackDamage*0.03f);
            }
        }

        if (onlyOne == 1 && unitIndex != -1)
        {
            units[unitIndex].attackDamage += Mathf.Round(units[unitIndex].baseAttackDamage * 0.3f);
            units[unitIndex].mobility += 4;
        }
    }
    //35
    public static void ConquerorSeal(ref BattleRewardData reward,StageType type,int grade)
    {
        if (reward.battleResult != 0) return;
        if (type != StageType.Elite) return;
        reward.relicGrade.Add(grade);
    }

    public static List<int> GetAvailableRelicIds(int grade, RelicAction action)
    {
        var relics = WarRelicDatabase.relics.Where(r => r.grade == grade).Select(r => r.id).ToList();
        var ownedIds = RogueLikeData.Instance.GetAllOwnedRelicIds().ToHashSet();

        if (action == RelicAction.Acquire)
            return relics.Where(id => !ownedIds.Contains(id)).ToList();
        else
            return relics.Where(id => ownedIds.Contains(id)).ToList();
    }
    public static List<WarRelic> GetAvailableRelicsAllGrades(RelicAction action)
    {
        var ownedIds = RogueLikeData.Instance.GetAllOwnedRelicIds().ToHashSet();

        return (action == RelicAction.Acquire)
            ? WarRelicDatabase.relics.Where(r => !ownedIds.Contains(r.id)).ToList()
            : WarRelicDatabase.relics.Where(r => ownedIds.Contains(r.id)).ToList();
    }

    public static WarRelic HandleRandomRelicAllGrades(RelicAction action)
    {
        var available = GetAvailableRelicsAllGrades(action);
        if (available.Count == 0) return null;

        var selected = available[UnityEngine.Random.Range(0, available.Count)];

        if (action == RelicAction.Acquire)
        {
            RogueLikeData.Instance.AcquireRelic(selected.id);
            ownedRelics.TryAdd(selected.id, selected);
        }
        else if (action == RelicAction.Remove)
        {
            RogueLikeData.Instance.RemoveRelicById(selected.id);
            ownedRelics.Remove(selected.id);
        }

        return selected;
    }

}
