using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public static class RewardManager
{
    private const int LastChapter = 3;
    private const int ContinueRun = 0;
    private const int DefeatRun = 1;
    private const int VictoryRun = 2;

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
        if (RelicManager.CheckRelicById(7))
        {
            RogueLikeData.Instance.AddReroll(3);
        }
        return (gold, relicGrade);
    }

    //현재 스테이지와 챕터에 따른 전투 보상 
    // 사용처: 전투 종료 직후 보상 계산 및 실제 전투 결과 기준 게임오버 판정
    public static int AddBattleRewardByStage(
        int battleResult,
        List<RogueUnitDataBase> battleUnits,
        List<RogueUnitDataBase> deadUnits,
        List<RogueUnitDataBase> deadEnemyUnits)
    {
        int battleChapter = RogueLikeData.Instance.GetChapter();
        BattleRewardData reward = RogueLikeData.Instance.GetBattleReward();
        var type = RogueLikeData.Instance.GetCurrentStageType();

        reward.battleResult = battleResult;

        // 사용처: 마지막 챕터 보스 승리만 승리 종료로 처리한다.
        if (battleResult == 0 && type == StageType.Boss && battleChapter == LastChapter)
        {
            return VictoryRun;
        }

        int morale = EndBattleMorale(battleResult, deadUnits, deadEnemyUnits, type);
        int currentMorale = RogueLikeData.Instance.GetMorale();

        if (currentMorale + morale <= 0)
        {
            return DefeatRun;
        }

        // 사용처: 현재 myTeam 원본이 아니라 전투 결과 반영 후 기준으로 게임오버 판정
        if (CheckGameOverAfterBattle(battleUnits, deadUnits))
        {
            return DefeatRun;
        }

        reward.morale += morale;

        int baseGold = stageTypeGold.TryGetValue(type, out var value) ? value : 0;
        int gold = RogueLikeData.Instance.GetGoldByChapter(baseGold, battleResult);

        // 유산
        if (reward.battleResult == 0)
        {
            WarRelic relic86 = RelicManager.GetRelicById(86);
            var vals = relic86?.GetAllValuesAsFloatListOrNull();
            if (vals != null)
            {
                int battleUnitCount = RogueLikeData.Instance.GetBattleUnitCount();
                int maxUnit = RogueLikeData.Instance.GetMaxUnits();
                if (battleUnitCount <= maxUnit - vals[0])
                    gold *= 1 + (int)vals[1];
            }

            if (RelicManager.CheckRelicById(6))
            {
                int roll = RogueLikeData.Instance.GetRandomInt(0, 3);

                if (roll == 0)
                {
                    int relicGold = RogueLikeData.Instance.GetRandomInt(1, 201);
                    gold += relicGold;
                }
                else if (roll == 1)
                {
                    reward.relicGrade.Add(1);
                }
            }

            if (type == StageType.Elite)
            {
                if (RelicManager.CheckRelicById(7))
                {
                    RogueLikeData.Instance.AddReroll(1);
                }

                if (RelicManager.CheckRelicById(35))
                {
                    reward.relicGrade.Add(5);
                }

                if (RelicManager.CheckRelicById(106))
                {
                    WarRelic relic = RelicManager.GetRelicById(106);
                    var relicValue = relic.GetAllValuesAsFloatListOrNull();
                    if (relicValue != null)
                    {
                        relicValue[1] += relicValue[0];
                        string[] updated = relicValue.Select(v => v.ToString(CultureInfo.InvariantCulture)).ToArray();
                        relic.SetValues(updated);
                    }
                }

                if (RelicManager.CheckRelicById(120))
                {
                    WarRelic relic = RelicManager.GetRelicById(120);
                    var relicValue = relic.GetAllValuesAsFloatListOrNull();
                    if (relicValue != null)
                    {
                        relicValue[0] += relicValue[1];
                        string[] updated = relicValue.Select(v => v.ToString(CultureInfo.InvariantCulture)).ToArray();
                        relic.SetValues(updated);
                    }
                }

                // 사용처: 엘리트 전투 승리 시 순금 나팔(50)의 보유 금화 증가 효과를 즉시 적용
                int goldenHornGold = WarRelicDatabase.ApplyGoldenHornEliteGoldReward();
                if (goldenHornGold > 0)
                {
                    Debug.Log($"[RewardManager] 순금 나팔(50) 효과로 금화 {goldenHornGold} 획득");
                }
            }

            if (RelicManager.CheckRelicById(122))
            {
                WarRelic relic = RelicManager.GetRelicById(122);
                var relicValue = relic.GetAllValuesAsFloatListOrNull();
                if (relicValue != null && relicValue[3] < relicValue[1])
                {
                    relicValue[3]++;
                    string[] updated = relicValue.Select(v => v.ToString(CultureInfo.InvariantCulture)).ToArray();
                    relic.SetValues(updated);
                }
            }
        }

        // 유산 94
        gold += RelicManager.RunCastIronHelmet();
        RelicManager.RunTrainingSandbagsOfWar();

        reward.gold += gold;

        int grade = stageTypeGrade.TryGetValue(type, out var val) ? val : 0;
        reward.unitGrade.Add(grade);

        if (type == StageType.Elite || type == StageType.Boss)
        {
            reward.relicGrade.Add(grade);
        }

        // 사용처: 1·2챕터 보스 승리 후 생존이 확정된 런만 다음 챕터 맵으로 전환한다.
        if (battleResult == 0 && type == StageType.Boss && battleChapter < LastChapter)
        {
            RogueLikeData.Instance.SetChapter(battleChapter + 1);
            RogueLikeData.Instance.SetClearChapter(true);
        }

        return ContinueRun;
    }

    //전투 종료 시 사기 계산
    private static int EndBattleMorale(int result, List<RogueUnitDataBase> deadUnits, List<RogueUnitDataBase> deadEnemyUnits, StageType type)
    {
        int morale = 0;
        int addMorale = 0;
        int reduceMorale = 0;
        if (deadUnits != null)
        {
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
        }
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
            else if (result == 1)
                reduceMorale -= 150;
        }
        else if (type == StageType.Boss)
        {
            if (result == 0)
            {
                addMorale += 35;

                //유산59
                if (RelicManager.CheckRelicById(59))
                {
                    List<RogueUnitDataBase> myUnits = RogueLikeData.Instance.GetMyTeam();
                    foreach (var unit in myUnits)
                    {
                        unit.Energy = unit.MaxEnergy;
                    }
                }

            }
            else if (result == 1)
                reduceMorale -= 150;
        }

        if (RelicManager.CheckRelicById(56)) addMorale += deadEnemyUnits.Count;

        morale += addMorale + reduceMorale;

        return morale;
    }

    // 등급에 따라 유닛 3명을 반환하는 함수
    public static List<RogueUnitDataBase> GetRandomUnitsByGrade(int grade)
    {
        int rarity1Rate = 0;
        WarRelic relic = RelicManager.GetRelicById(85);
        var vals = relic?.GetAllValuesAsFloatListOrNull();
        if (vals != null)
        {
            rarity1Rate = Mathf.RoundToInt(vals[0] * 100f);
        }

        var allUnits = UnitLoader.Instance.GetAllCachedUnits();

        Dictionary<int, int> rarityWeights = grade switch
        {
            1 => new() { { 1, 60 + rarity1Rate }, { 2, 37 }, { 3, 3 }, { 4, 0 } },
            5 => new() { { 1, 40 + rarity1Rate }, { 2, 35 }, { 3, 15 }, { 4, 10 } },
            10 => new() { { 1, 0 }, { 2, 40 }, { 3, 40 }, { 4, 20 } },
            _ => new() { { 1, 100 } }
        };
        ApplyOrbOfContemptUnitFilter(rarityWeights);
        ApplyBrokenMirrorHeroPenalty(rarityWeights);

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
            var selected = unitPool[RogueLikeData.Instance.GetRandomInt(0, unitPool.Count)];
            selectedUnits.Add(selected);
        }

        return selectedUnits;
    }

    private static void ApplyOrbOfContemptUnitFilter(Dictionary<int, int> rarityWeights)
    {
        if (!RelicManager.CheckRelicById(111))
            return;

        if (!rarityWeights.TryGetValue(1, out int commonWeight) || commonWeight <= 0)
            return;

        rarityWeights[1] = 0;

        int targetRarity = rarityWeights
            .Where(kvp => kvp.Key != 1 && kvp.Value > 0)
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => kvp.Key)
            .FirstOrDefault();

        if (targetRarity == 0)
            targetRarity = 2;

        if (!rarityWeights.ContainsKey(targetRarity))
            rarityWeights[targetRarity] = 0;

        rarityWeights[targetRarity] += commonWeight;
    }

    private static void ApplyBrokenMirrorHeroPenalty(Dictionary<int, int> rarityWeights)
    {
        WarRelic relic = RelicManager.GetRelicById(90);
        var vals = relic?.GetAllValuesAsFloatListOrNull();
        if (vals == null || vals.Count == 0)
            return;

        if (!rarityWeights.TryGetValue(4, out int heroWeight) || heroWeight <= 0)
            return;

        int adjustedHeroWeight = Mathf.Max(0, Mathf.RoundToInt(heroWeight * (1f + vals[0])));
        int shiftedWeight = heroWeight - adjustedHeroWeight;
        if (shiftedWeight <= 0)
            return;

        rarityWeights[4] = adjustedHeroWeight;

        int targetRarity = 1;
        if (!rarityWeights.ContainsKey(targetRarity) || rarityWeights[targetRarity] <= 0)
        {
            targetRarity = rarityWeights
                .Where(kvp => kvp.Key != 4 && kvp.Value > 0)
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => kvp.Key)
                .FirstOrDefault();
        }

        if (targetRarity == 0)
            targetRarity = 1;

        if (!rarityWeights.ContainsKey(targetRarity))
            rarityWeights[targetRarity] = 0;

        rarityWeights[targetRarity] += shiftedWeight;
    }

    private static int GetRandomRarityByWeight(Dictionary<int, int> weights)
    {
        int total = weights.Values.Sum();
        int rand = RogueLikeData.Instance.GetRandomInt(0, total);
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
        List<RogueUnitDataBase> myUnits = RogueLikeData.Instance.GetMyTeam();
        foreach (var unit in myUnits)
        {
            if (unit.Energy > 0) return false;
        }
        return true;
    }
    // 사용처: 전투 종료 직후 실제 보유 유닛에 전투 결과를 가상 반영해서 게임오버 판정
    private static bool CheckGameOverAfterBattle(
        List<RogueUnitDataBase> battleUnits,
        List<RogueUnitDataBase> deadUnits)
    {
        List<RogueUnitDataBase> simulatedTeam = RogueLikeData.Instance.GetMyTeam()
            .Select(unit => unit.Clone())
            .ToList();

        ApplyBattleResultToTeam(simulatedTeam, battleUnits);
        ApplyBattleResultToTeam(simulatedTeam, deadUnits);

        for (int i = 0; i < simulatedTeam.Count; i++)
        {
            if (simulatedTeam[i].Energy > 0)
                return false;
        }

        return true;
    }

    // 사용처: 전투 참가 유닛의 최종 기력 상태를 실제 보유 유닛 복사본에 반영
    private static void ApplyBattleResultToTeam(
        List<RogueUnitDataBase> team,
        List<RogueUnitDataBase> changedUnits)
    {
        if (changedUnits == null)
            return;

        for (int i = 0; i < changedUnits.Count; i++)
        {
            RogueUnitDataBase changedUnit = changedUnits[i];
            int teamIndex = team.FindIndex(unit => unit.UniqueId == changedUnit.UniqueId);

            if (teamIndex < 0)
                continue;

            if (changedUnit.Energy < 1)
            {
                team.RemoveAt(teamIndex);
            }
            else
            {
                team[teamIndex].Energy = changedUnit.Energy;
            }
        }
    }
}
