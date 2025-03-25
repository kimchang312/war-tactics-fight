using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RelicManager
{
    //보유 유물
    private static List<WarRelic> ownedRelics = new List<WarRelic>();
    private static HashSet<int> ownedRelicIds = new HashSet<int>();
    private static bool curseBlock;

    //유산 저장
    public static List<WarRelic> GetRelicData()
    {
        ownedRelics.Clear();
        ownedRelicIds.Clear();

        // 중복을 방지하며 유산 추가하는 메서드
        void AddRelics(List<WarRelic> relics)
        {
            foreach (var relic in relics)
            {
                if (ownedRelicIds.Add(relic.id)) // HashSet에 추가 성공하면 중복 아님
                {
                    ownedRelics.Add(relic);
                }
            }
        }

        // 유물 데이터 가져오기
        AddRelics(RogueLikeData.Instance.GetRelicsByType(RelicType.AllEffect));
        AddRelics(RogueLikeData.Instance.GetRelicsByType(RelicType.SpecialEffect));
        AddRelics(RogueLikeData.Instance.GetRelicsByType(RelicType.StateBoost));
        AddRelics(RogueLikeData.Instance.GetRelicsByType(RelicType.BattleActive));
        AddRelics(RogueLikeData.Instance.GetRelicsByType(RelicType.ActiveState));

        return ownedRelics;
    }


    // 보유한 유물 중 ID 23, 24, 25을 모두 가지고 있는지 확인하고, 있으면 유물 27 추가
    public static void CheckFusion()
    {
        // 이미 유물 26을 보유하고 있으면 실행할 필요 없음
        if (ownedRelics.Any(relic => relic.id == 26)) return;

        HashSet<int> requiredRelicIds = new HashSet<int> { 23, 24, 25 };
        HashSet<int> ownedRelicIds = new HashSet<int>(ownedRelics.Select(relic => relic.id));

        // 모든 필수 유물(23, 24, 25)을 보유하고 있는지 확인
        if (requiredRelicIds.All(id => ownedRelicIds.Contains(id)))
        {
            // RogueLikeData에 유물 26 추가
            RogueLikeData.Instance.AcquireRelic(26);

            // 유물이 정상적으로 존재하는 경우만 추가
            ownedRelics.Add(WarRelicDatabase.GetRelicById(26));
            return;
        }

        return;
    }

    //스탯 유산 발동
    public static void RunStateRelic()
    {
        List<WarRelic> stateRelics = ownedRelics.Where(relic => relic.type == RelicType.StateBoost || relic.type == RelicType.ActiveState).ToList();

        if (stateRelics.Count <= 0) return;
        curseBlock = CheckRelicById(22);
        foreach (var relic in stateRelics)
        {
            if (curseBlock && relic.grade == 0)
                continue;

            relic.Execute();
        }
    }
    //유산 유무 채크
    public static bool CheckRelicById(int relicId)
    {
        var relic = ownedRelics.FirstOrDefault(relic => relic.id == relicId);
        if (relic == null || !relic.used) return false;
        relic.used = true;
        return true;
    }
    //유산 34 생존자의 넝마떼기
    public static void SurvivorOfRag(ref List<RogueUnitDataBase> units)
    {
        if (ownedRelics.FirstOrDefault(relic => relic.id == 34) == null) return;
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
            units[unitIndex].mobility += 4;
        }
    }

}
