using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class RelicManager
{
    // 보유 유물 (Dictionary 형태로 변경)
    private static Dictionary<int, WarRelic> ownedRelics = new Dictionary<int, WarRelic>();
    private static bool curseBlock;

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
    //랜덤 일반등급 획득 및 반환
    public static WarRelic AcquireRandomNormalRelic()
    {
        var allNormal = WarRelicDatabase.relics.Where(r => r.grade == 1).ToList();
        var ownedIds = RogueLikeData.Instance.GetAllOwnedRelicIds().ToHashSet();

        var notOwned = allNormal.Where(r => !ownedIds.Contains(r.id)).ToList();
        if (notOwned.Count == 0) return null;

        var selected = notOwned[UnityEngine.Random.Range(0, notOwned.Count)];
        RogueLikeData.Instance.AcquireRelic(selected.id);
        return selected;
    }
    //랜덤 저주등급 획득 및 반환
    public static WarRelic AcquireRandomCursedRelic()
    {
        var allCursed = WarRelicDatabase.relics.Where(r => r.grade == 0).ToList();
        var ownedIds = RogueLikeData.Instance.GetAllOwnedRelicIds().ToHashSet();

        var notOwned = allCursed.Where(r => !ownedIds.Contains(r.id)).ToList();
        if (notOwned.Count == 0) return null;

        var selected = notOwned[UnityEngine.Random.Range(0, notOwned.Count)];
        RogueLikeData.Instance.AcquireRelic(selected.id);
        return selected;
    }
    //랜덤 저주 유산 제거
    public static string RemoveRandomCursedRelic()
    {
        // 현재 보유 중인 저주 등급 유산 목록
        var cursedRelics = RogueLikeData.Instance
            .GetAllOwnedRelics()
            .Where(r => r.grade == 0)
            .ToList();

        if (cursedRelics.Count == 0)
            return null;

        // 무작위로 하나 선택
        WarRelic toRemove = cursedRelics[UnityEngine.Random.Range(0, cursedRelics.Count)];

        // 데이터에서 제거
        RogueLikeData.Instance.RemoveRelicById(toRemove.id);

        // RelicManager 내부 캐시에도 반영
        ownedRelics.Remove(toRemove.id);

        return toRemove.name;
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
