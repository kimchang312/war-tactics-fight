using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RewardManager
{
    // 랜덤으로 1개의 보유하지 않은 유산을 반환하는 함수
    public static int GetRandomWarRelicId(int stage = 0)
    {
        // 현재 보유 중인 유산 ID를 가져옴
        List<int> ownedRelicIds = RogueLikeData.Instance.GetAllOwnedRelicIds();

        // 보유하지 않은 유산 중 grade가 1~10인 것만 필터링
        List<WarRelic> availableRelics = WarRelicDatabase.relics
            .Where(relic => !ownedRelicIds.Contains(relic.id) && relic.grade >= 1 && relic.grade <= 10)
            .ToList();

        // 보유하지 않은 유산이 없다면 -1 반환
        if (availableRelics.Count == 0)
        {
            Debug.LogWarning("보유하지 않은 유산이 없습니다.");
            return -1;
        }

        // 랜덤으로 1개의 유산 선택
        System.Random random = new System.Random();
        int index = random.Next(availableRelics.Count);

        return availableRelics[index].id;
    }

    //보상 획득 
    public static void GetRewardByStage()
    {
        BattleRewardData battleRewardData = RogueLikeData.Instance.GetBattleReward();
        var (x,y,stage) = RogueLikeData.Instance.GetCurrentStage();
        int gold=0;
        int morale=0;
        int reroll=0;
        List<int> relicIds =new();
        List<RogueUnitDataBase> units=new();

        var strageGlass = RogueLikeData.Instance.GetOwnedRelicById(7);
        if(stage == StageType.Combat)
        {
            gold += RogueLikeData.Instance.AddGoldByEventChapter(50);
        }
        else if(stage == StageType.Elite)
        {
            if(strageGlass != null)
            {
                reroll += 1;
            }
            
        }
        else if(stage == StageType.Treasure)
        {
            if(strageGlass != null)
            {
                reroll += 3;
            }
            
        }



    }

    public static RogueUnitDataBase GetUnitRewardByStage()
    {
        var (x, y, stage) = RogueLikeData.Instance.GetCurrentStage();
        int selectedRarity;

        switch (stage)
        {
            case StageType.Combat:
                {
                    selectedRarity = RollRarity(new Dictionary<int, int>
            {
                { 1, 60 },
                { 2, 37 },
                { 3, 3 }

            });
                    return RogueUnitDataBase.GetRandomUnitByRarity(selectedRarity);
                }
            case StageType.Elite:
                {
                    selectedRarity = RollRarity(new Dictionary<int, int>
            {
                { 1, 50 },
                { 2, 40 },
                { 3, 10 }
            });

                    return RogueUnitDataBase.GetRandomUnitByRarity(selectedRarity);
                }
            case StageType.Boss:
                {
                    selectedRarity = RollRarity(new Dictionary<int, int>
            {
                { 2, 50 },
                { 3, 45 },
                { 4, 5 } // 4를 영웅 유닛 희귀도로 가정
            });
                    return RogueUnitDataBase.GetRandomUnitByRarity(selectedRarity);
                }
                default:
                return null;
        }
    }

    // 주어진 확률 사전 기반 무작위 희귀도 선택
    private static int RollRarity(Dictionary<int, int> weights)
    {
        int total = weights.Values.Sum();
        int rand = UnityEngine.Random.Range(0, total);
        int cumulative = 0;

        foreach (var kvp in weights)
        {
            cumulative += kvp.Value;
            if (rand < cumulative)
                return kvp.Key;
        }

        return weights.Keys.First(); // fallback
    }
   




}
