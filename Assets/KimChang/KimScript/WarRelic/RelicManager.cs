using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
    // 특정 등급에서 중복 여부를 고려하여 랜덤 유산 ID 1개를 선택
    public static int GetRandomRelicId(int grade, RelicAction action)
    {
        if (grade == 5)
        {
            grade = UnityEngine.Random.value < 0.2f ? 10 : 1;
        }

        var relics = WarRelicDatabase.relics.Where(r => r.grade == grade).ToList();
        var ownedIds = RogueLikeData.Instance.GetAllOwnedRelicIds().ToHashSet();

        var available = (action == RelicAction.Acquire)
            ? relics.Where(r => !ownedIds.Contains(r.id)).ToList()
            : relics.Where(r => ownedIds.Contains(r.id)).ToList();

        if (available.Count == 0) return -1;

        return available[UnityEngine.Random.Range(0, available.Count)].id;
    }
    // 유산 ID 기반으로 획득 또는 제거 처리
    public static void ApplyRelicAction(int relicId, RelicAction action)
    {
        if (relicId < 0) return;

        if (action == RelicAction.Acquire)
        {
            RogueLikeData.Instance.AcquireRelic(relicId);
            var relic = WarRelicDatabase.GetRelicById(relicId);
            if (relic != null)
                ownedRelics.TryAdd(relicId, relic);
        }
        else if (action == RelicAction.Remove)
        {
            RogueLikeData.Instance.RemoveRelicById(relicId);
            ownedRelics.Remove(relicId);
        }
    }

    //무작위 유산 추가 || 제거
    public static WarRelic HandleRandomRelic(int grade, RelicAction action)
    {
        if (grade == 5)
        {
            grade = UnityEngine.Random.value < 0.2f ? 10 : 1;
        }
        // 후보 유산 리스트
        var relics = WarRelicDatabase.relics.Where(r => r.grade == grade).ToList();
        var ownedIds = RogueLikeData.Instance.GetAllOwnedRelicIds().ToHashSet();

        // 중복 방지 리스트
        var available = (action == RelicAction.Acquire)
            ? relics.Where(r => !ownedIds.Contains(r.id)).ToList()
            : relics.Where(r => ownedIds.Contains(r.id)).ToList();

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
    // 보유한 유물 중 ID 23, 24, 25을 모두 가지고 있는지 확인하고, 있으면 유물 26 추가
    public static void CheckFusion()
    {
        if (ownedRelics.ContainsKey(26)) return; // 이미 유물 26을 보유 중이면 종료

        int[] requiredRelicIds = { 23, 24, 25 };

        // 필요한 유물들이 전부 포함되어 있는지 확인
        if (requiredRelicIds.All(id => ownedRelics.ContainsKey(id)))
        {
            WarRelic newRelic = WarRelicDatabase.GetRelicById(26);
            if (newRelic != null)
            {
                RogueLikeData.Instance.AcquireRelic(26);
                ownedRelics.TryAdd(26, newRelic); // 유물 26 추가
            }
        }
    }

    // 스탯 유산 발동
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

    // 유산 유무 확인
    public static bool CheckRelicById(int relicId)
    {
        return ownedRelics.ContainsKey(relicId) && !ownedRelics[relicId].used;
    }

    // 유산 34 생존자의 넝마떼기
    public static void SurvivorOfRag(ref List<RogueUnitDataBase> units)
    {
        if (!ownedRelics.ContainsKey(34)) return;

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

        if (onlyOne == 1 && unitIndex != -1)
        {
            units[unitIndex].attackDamage *= 1.3f;
            units[unitIndex].mobility += 4;
        }
    }
}
