using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public static class RewardManager
{
    private static readonly Dictionary<StageType, int> stageTypeGold = new()
{
    { StageType.Combat, 50 },
    { StageType.Elite, 150 },
    { StageType.Treasure, 150 },
    { StageType.Boss, 250 }
};
    private static readonly Dictionary<StageType, int> stageTypeGrade = new()
{
    { StageType.Combat, 1 },
    { StageType.Elite, 5 },
    { StageType.Boss, 10 }
};

    //보물 스테이지 일때 보상
    public static (int, int) GetRewardTeasure()
    {
        var type = RogueLikeData.Instance.GetCurrentStageType();
        int baseGold = stageTypeGold.TryGetValue(type, out var value) ? value : 0;
        int gold = RogueLikeData.Instance.GetGoldByChapter(baseGold);
        int relicGrade = 7;
        return (gold,relicGrade);
    }

    //현재 스테이지와 챕터에 따른 전투 보상 
    public static void AddBattleRewardByStage(int battleResult,List<RogueUnitDataBase> deadUnits, List<RogueUnitDataBase> deadEnemyUnits)
    {
        int chapter = RogueLikeData.Instance.GetChapter();
        BattleRewardData reward = RogueLikeData.Instance.GetBattleReward();
        reward.battleResult = battleResult;
        var type = RogueLikeData.Instance.GetCurrentStageType();
        if (battleResult == 0 && type ==StageType.Boss && chapter==1)
        {
            RogueLikeData.Instance.SetChapter(2);
            RogueLikeData.Instance.SetClearChapter(true);
        }
        else if (battleResult == 0 && type == StageType.Boss && chapter == 2)
        {
            RogueLikeData.Instance.SetChapter(3);
            RogueLikeData.Instance.SetClearChapter(true);
        }
        int morale = EndBattleMorale(battleResult, deadUnits, deadEnemyUnits, type);
        RogueLikeData.Instance.ChangeMorale(morale);
        reward.morale += morale;
        int baseGold = stageTypeGold.TryGetValue(type, out var value) ? value : 0;
        int gold = RogueLikeData.Instance.GetGoldByChapter(baseGold);
        //유산 
        if (RelicManager.CheckRelicById(86) && reward.battleResult ==0)
        {
            int battleUnitCount = RogueLikeData.Instance.GetBattleUnitCount();
            int maxUnit = RogueLikeData.Instance.GetMaxUnits();
            if (battleUnitCount <= maxUnit - 3) gold *= 2;
        }
        reward.gold += gold;
        int grade = stageTypeGrade.TryGetValue(type,out var val)? val: 0;
        reward.unitGrade.Add(grade);
        if(type == StageType.Elite || type == StageType.Boss)
        {
            reward.relicGrade.Add(grade);
        }

        RelicManager.ConquerorSeal(ref reward,type,grade);
    }
    //전투 종료 시 사기 계산
    private static int EndBattleMorale(int result, List<RogueUnitDataBase> deadUnits, List<RogueUnitDataBase> deadEnemyUnits,StageType type)
    {
        int morale = 0;
        int addMorale = 0;
        int reduceMorale = 0;
        foreach (var unit in deadUnits)
        {
            switch (unit.rarity)
            {
                case 1:
                    reduceMorale -= 1;
                    break;
                case 2:
                    reduceMorale -= 2;
                    break;
                case 3:
                    reduceMorale -= 2;
                    break;
                case 4:
                    reduceMorale -= 5;
                    break;
            }
        }
        if (deadUnits.Count == 0) addMorale += 10;
        if (type == StageType.Combat)
        {
            if (result == 0)
                addMorale += 15;
            else if (result == 1)
                reduceMorale -= 55;
        }
        else if (type == StageType.Elite)
        {
            if (result == 0)
            addMorale += 20;
            else if (result ==1)
                reduceMorale -= 150;
        }
        else if (type == StageType.Boss)
        {
            if (result == 0)
                addMorale += 35;
            else if (result == 1)
                reduceMorale -= 150;
        }//유산 33
        if (RelicManager.CheckRelicById(33)) addMorale = (int)(reduceMorale * 1.2);
        //유산 56
        if (RelicManager.CheckRelicById(56)) addMorale += deadEnemyUnits.Count;
        if (RelicManager.CheckRelicById(59) && type ==StageType.Boss) 
        {
            var myUnits =RogueLikeData.Instance.GetMyUnits();
            foreach (var unit in myUnits)
            {
                unit.energy = unit.maxEnergy;
            }
        }
        morale += addMorale + reduceMorale;

        return morale;
    }

    // 등급에 따라 유닛 3명을 반환하는 함수
    public static List<RogueUnitDataBase> GetRandomUnitsByGrade(int grade)
    {
        var allUnits = UnitLoader.Instance.GetAllCachedUnits();

        Dictionary<int, int> rarityWeights = grade switch
        {
            1 => new() { { 1, 60 }, { 2, 37 }, { 3, 3 }, { 4, 0 } },
            5 => new() { { 1, 50 }, { 2, 40 }, { 3, 10 }, { 4, 0 } },
            10 => new() { { 1, 0 }, { 2, 50 }, { 3, 45 }, { 4, 5 } },
            _ => new() { { 1, 100 } }
        };

        Dictionary<int, List<RogueUnitDataBase>> unitsByRarity = new();
        foreach (var unit in allUnits)
        {
            if (!unitsByRarity.ContainsKey(unit.rarity))
                unitsByRarity[unit.rarity] = new List<RogueUnitDataBase>();
            unitsByRarity[unit.rarity].Add(unit);
        }

        List<RogueUnitDataBase> selectedUnits = new();
        for (int i = 0; i < 3; i++)
        {
            int selectedRarity = GetRandomRarityByWeight(rarityWeights);
            if (!unitsByRarity.ContainsKey(selectedRarity) || unitsByRarity[selectedRarity].Count == 0)
            {
                i--;
                continue;
            }

            var unitPool = unitsByRarity[selectedRarity];
            var selected = unitPool[UnityEngine.Random.Range(0, unitPool.Count)];
            selectedUnits.Add(selected);
        }

        return selectedUnits;
    }

    private static int GetRandomRarityByWeight(Dictionary<int, int> weights)
    {
        int total = weights.Values.Sum();
        int rand = UnityEngine.Random.Range(0, total);
        int sum = 0;
        foreach (var kvp in weights)
        {
            sum += kvp.Value;
            if (rand < sum)
                return kvp.Key;
        }
        return weights.Keys.First();
    }

    public static bool CheckGameOver()
    {
        int morale = RogueLikeData.Instance.GetMorale();
        if (morale < 1) return true;
        List<RogueUnitDataBase> myUnits = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in myUnits)
        {
            if (unit.energy > 0) return false; 
        }
        return true;
    }


}
