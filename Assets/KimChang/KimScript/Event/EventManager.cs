using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static RelicManager;


public class EventManager
{
    public static void LoadEventData()
    {
        EventDataLoader.LoadData();
    }
    //
    public static EventData GetRandomEvent()
    {
        var encountered = RogueLikeData.Instance.GetEncounteredEvent().Keys;
        var candidates = EventDataLoader.EventDataDict
            .Where(kv => !encountered.Contains(kv.Key) && CanAppear(kv.Value))
            .Select(kv => kv.Value)
            .ToList();

        int idx = UnityEngine.Random.Range(0, candidates.Count);
        return candidates[idx];
    }

    public static bool CanAppear(EventData eventData)
    {
        int currentChapter = RogueLikeData.Instance.GetChapter();

        if (!eventData.eventChapter.Contains(currentChapter))
            return false;

        bool allNone = true;
        for (int i = 0; i < eventData.requireThing.Count; i++)
        {
            if (eventData.requireThing[i] != RequireThing.None)
            {
                allNone = false;
                break;
            }
        }
        if (allNone) return true;

        for (int i = 0; i < eventData.requireThing.Count; i++)
        {
            var thing = eventData.requireThing[i];
            var form = eventData.requireForm[i];
            var value = eventData.requireValue[i];
            var count = eventData.requireCount[i];

            if (thing == RequireThing.Special || form == RequireForm.Special)
            {
                if (!CheckSpecialRequire(eventData))
                    return false;
            }
            else
            {
                if (!CheckRequireCondition(thing, form, value, count))
                    return false;
            }
        }
        return true;
    }

    private static bool CheckRequireCondition(RequireThing thing, RequireForm form, string value,string count)
    {
        switch (thing)
        {
            case RequireThing.Gold:
                if (form == RequireForm.None)
                {
                    int gold = RogueLikeData.Instance.GetCurrentGold();
                    if(RogueLikeData.Instance.GetOwnedRelicById(49) != null)
                    {
                        gold += 500;
                    }
                    return InRange(gold, count);
                }
                break;
            case RequireThing.Morale:
                {
                    int morale = RogueLikeData.Instance.GetMorale();
                    float reduceMorale = 1;
                    if(RogueLikeData.Instance.GetOwnedRelicById(33) == null)
                    {
                        reduceMorale += 0.2f;
                    }
                    return InRange(morale, count,true, reduceMorale);
                }
            case RequireThing.Unit:
                {
                    int requireCount = int.Parse(count);
                    if (form == RequireForm.Select)
                    {
                        if (value.Contains("~"))
                        {
                            var myUnits = RogueLikeData.Instance.GetMyTeam();

                            var (min, max) = ParseRange(value);
                            int unitCount = 0;
                            foreach (var unit in myUnits)
                            {
                                if (unit.rarity >= min && unit.rarity <= max)
                                {
                                    unitCount++;
                                    if (unitCount >= requireCount) return true;
                                }
                            }
                            return false;
                        }
                        else if (!string.IsNullOrEmpty(value))
                        {
                            var myUnits = RogueLikeData.Instance.GetMyTeam();
                            int rarity = int.Parse(value);
                            int unitCount = 0;
                            foreach (var unit in myUnits)
                            {
                                if (unit.rarity <= rarity)
                                {
                                    unitCount++;
                                    if (unitCount >= requireCount) return true;
                                }
                            }
                            return false;
                        }
                        return RogueLikeData.Instance.GetMyTeam().Count >= requireCount;
                    }
                    else if (form == RequireForm.Random)
                    {

                        if (value.Contains("~"))
                        {
                            var myUnits = RogueLikeData.Instance.GetMyTeam();

                            var (min, max) = ParseRange(value);
                            int unitCount = 0;
                            foreach (var unit in myUnits)
                            {
                                if (unit.rarity >= min && unit.rarity <= max)
                                {
                                    unitCount++;
                                    if (unitCount >= requireCount) return true;
                                }
                            }
                            return false;
                        }
                        else if (!string.IsNullOrEmpty(value))
                        {
                            var myUnits = RogueLikeData.Instance.GetMyTeam();
                            int rarity = int.Parse(value);
                            int unitCount = 0;

                            foreach (var unit in myUnits)
                            {
                                if (unit.rarity <= rarity)
                                {
                                    unitCount++;
                                    if (unitCount >= requireCount) return true;
                                }
                            }
                            return false;
                        }
                        return RogueLikeData.Instance.GetMyTeam().Count >= requireCount;
                    }
                    else if (form == RequireForm.Special)
                    {
                        return RogueLikeData.Instance.GetMyTeam().Count >= requireCount;
                    }
                    break;
                }
            case RequireThing.Relic:
                if (form == RequireForm.Random)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        var relics = RogueLikeData.Instance.GetAllOwnedRelics();
                        int grade = int.Parse(value);
                        int relicCount = 0;
                        int requireCount = int.Parse(count);
                        foreach (var relic in relics)
                        {
                            if(relic.grade == grade)
                            {
                                relicCount++;
                                if (relicCount >= requireCount) return true;
                            }
                        }
                        return false;
                    }
                    int relicGrade = int.Parse(count);
                    return RogueLikeData.Instance.GetAllOwnedRelics().Exists(relic => relic.grade == relicGrade);
                }
                else if (form == RequireForm.None) // 특정 id 유산이 없어야 true
                {
                    int relicId = int.Parse(count);
                    return !RelicManager.CheckRelicById(relicId);
                }
                break;
            case RequireThing.Energy:
                if(form == RequireForm.Random)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        var myUnits = RogueLikeData.Instance.GetMyTeam();
                        int energyValue = int.Parse(value);
                        int unitCount= 0;
                        int requireCount = int.Parse(count);
                        foreach (var unit in myUnits)
                        {
                            if(unit.energy > energyValue)
                            {
                                unitCount++;
                                if(unitCount >= requireCount) return true;
                            }
                        }
                        return false;
                    }
                }
                if(form == RequireForm.Select)
                {
                    if(string.IsNullOrEmpty(value)) return true;

                    var myUnits = RogueLikeData.Instance.GetMyTeam();
                    int energyValue = int.Parse(value);
                    int unitCount = 0;
                    int requireCount = int.Parse(count);
                    foreach (var unit in myUnits)
                    {
                        if (unit.energy > energyValue)
                        {
                            unitCount++;
                            if (unitCount >= requireCount) return true;
                        }
                    }
                }
                break;
            case RequireThing.AttackDamage:
                {
                    int threshold = int.Parse(count);
                    var myUnits = RogueLikeData.Instance.GetMyTeam();
                    foreach (var unit in myUnits)
                    {
                        if (unit.attackDamage >= threshold)
                            return true;
                    }
                    break;
                }
            case RequireThing.Stage:
                if (form == RequireForm.None)
                {
                    return InRange(RogueLikeData.Instance.GetCurrentStageX(), count);
                }
                break;

        }
        return false;
    }

    private static bool CheckSpecialRequire(EventData eventData)
    {
        int gold = RogueLikeData.Instance.GetCurrentGold();
        if (RogueLikeData.Instance.GetOwnedRelicById(49) != null)
        {
            gold += 500;
        }
        int morale = RogueLikeData.Instance.GetMorale();
        if (eventData.eventId == 5)
        {
            if(gold >=100 || morale >=31) return true;
        }
        else if (eventData.eventId == 30)
        {
            List<int> warRelicIds = RogueLikeData.Instance.GetAllOwnedRelicIds();

            int count = 0;

            if (warRelicIds.Contains(23)) count++;
            if (warRelicIds.Contains(24)) count++;
            if (warRelicIds.Contains(25)) count++;

            return count >= 2;
        }
        else if (eventData.eventId == 45)
        {
            if(gold >=100 || morale >=6) return true;
        }
        return false;
    }

    public static bool CheckChoiceRequireCondition(EventChoiceData eventChoiceData)
    {
        // 조건이 모두 None이면 true
        if (eventChoiceData.requireThing.Contains(RequireThing.None))
        {
            return true;
        }
        // 조건 전부 순회하며 검사 (모든 조건을 만족해야 true)
        for (int i = 0; i < eventChoiceData.requireThing.Count; i++)
        {
            var thing = eventChoiceData.requireThing[i];
            var form = eventChoiceData.requireForm[i];
            var value = eventChoiceData.requireValue[i];
            var count = eventChoiceData.requireCount[i];

            if (thing == RequireThing.Special || form == RequireForm.Special)
            {
                if (!CheckSpecialChoiceRequire(eventChoiceData))
                    return false;
            }
            else
            {
                if (!CheckRequireCondition(thing, form, value, count))
                    return false;
            }
        }

        return true;
    }

    private static bool CheckSpecialChoiceRequire(EventChoiceData eventChoiceData)
    {
        //예외 처리
        if(eventChoiceData.eventId == 82)
        {
            Debug.Log("82 현재 미구현");
            return true;
        }
        return false;
    }
    //보상 받기전 요구값 감소
    public static void ReduceRequire(EventChoiceData choiceData)
    {
        string requireLog = "";
        for(int i = 0;i < choiceData.requireThing.Count;i++)
        {
            RequireThing thig = choiceData.requireThing[i];
            RequireForm form = choiceData.requireForm[i];
            string value = choiceData.requireValue[i];
            string count = choiceData.requireCount[i];
            switch (thig)
            {
                case RequireThing.None:
                    break;
                case RequireThing.Energy:
                    if(form == RequireForm.Select)
                    {
                        var selectedUnits = RogueLikeData.Instance.GetSelectedUnits();
                        foreach(var unit  in selectedUnits)
                        {
                            if (string.IsNullOrEmpty(value)) 
                            { 
                                requireLog += $"{unit.unitName}이(가) 선택 되었습니다."; 
                            }
                            else if(value == "-1")
                            {
                                unit.energy = Math.Max(1,unit.energy-1);
                                requireLog += $"{unit.unitName}이(가) 선택 되었습니다.";
                            }
                            else
                            {
                                int energy = int.Parse(value);
                                unit.energy = energy;
                                requireLog += $"{unit.unitName}이(가) 선택 되었습니다.";
                            }
                        }
                    }
                    else if (form == RequireForm.Random)
                    {
                        var myUnits = RogueLikeData.Instance.GetMyTeam();
                        int energy = int.Parse(value);
                        int unitCount = int.Parse(value);

                        // energy 보다 높은 에너지를 가진 유닛 필터링
                        List<RogueUnitDataBase> filteredUnits = myUnits.FindAll(unit => unit.energy > energy);

                        // 랜덤 셔플을 위해 리스트 섞기
                        System.Random random = new();
                        for (int k = filteredUnits.Count - 1; k > 0; k--)
                        {
                            int j = random.Next(k + 1);
                            (filteredUnits[k], filteredUnits[j]) = (filteredUnits[j], filteredUnits[k]);
                        }

                        // 최대 unitCount 개수만큼 선택
                        int countToSelect = Mathf.Min(unitCount, filteredUnits.Count);
                        List<RogueUnitDataBase> selectUnits = filteredUnits.GetRange(0, countToSelect);

                        foreach (var unit in selectUnits)
                        {
                            unit.energy = energy;
                            requireLog += $"{unit.unitName}이(가) 선택되었습니다.\n";
                        }
                    }

                    break;
                case RequireThing.Unit:
                    if(form == RequireForm.Select)
                    {
                        var myUnits = RogueLikeData.Instance.GetMyTeam();
                        var selectedUnits = RogueLikeData.Instance.GetSelectedUnits();
                        myUnits.RemoveAll(selectedUnits.Contains);
                        RogueLikeData.Instance.SetMyTeam(myUnits);
                        foreach(var unit in selectedUnits)
                        {
                            requireLog += $"{unit.unitName}이(가) 선택되었습니다.\n";
                        }
                    }
                    else if (form == RequireForm.Random)
                    {
                        var myUnits = RogueLikeData.Instance.GetMyTeam();
                        int maxRarity = int.Parse(value);
                        int unitCount = int.Parse(count);

                        var candidates = myUnits
                            .Where(unit => unit.rarity <= maxRarity)
                            .OrderBy(_ => UnityEngine.Random.value)
                            .Take(unitCount)
                            .ToList();

                        var currentUnits = RogueLikeData.Instance.GetMyTeam();
                        currentUnits.RemoveAll(unit => candidates.Contains(unit));
                        RogueLikeData.Instance.SetMyTeam(currentUnits);
                        foreach (var unit in candidates)
                        {
                            RogueLikeData.Instance.AddSelectedUnits(unit);
                            requireLog += $"{unit.unitName}이(가) 선택되었습니다.\n";
                        }
                    }
                    else if(form == RequireForm.Special)
                    {
                        if (choiceData.choiceId == 82)
                        {
                            List<RogueUnitDataBase> myTeam = RogueLikeData.Instance.GetMyTeam();
                            RogueUnitDataBase expensiveUnit = RogueLikeData.Instance
    .GetMyTeam()
    .OrderByDescending(unit => unit.unitPrice)
    .First();
                            myTeam.Remove(expensiveUnit);
                            RogueLikeData.Instance.SetMyTeam(myTeam);
                            requireLog += $"{expensiveUnit.unitName}이 선택되었습니다.";
                        }
                    }
                    else if(form == RequireForm.None)
                    {
                        var myUnits = RogueLikeData.Instance.GetMyTeam();
                        int rarity = int.Parse(value);
                        var candidates = myUnits.Where(unit=>unit.rarity==rarity);
                        myUnits.RemoveAll(candidates.Contains);
                        foreach(var unit in candidates)
                        {
                            RogueLikeData.Instance.AddSelectedUnits(unit);
                            requireLog += $"{unit.unitName}이(가) 선택되었습니다.\n";
                        }
                    }

                    break;
                case RequireThing.Relic:
                    if(form == RequireForm.Random)
                    {
                        int grade = int.Parse(value);
                        int relicCount = int.Parse(count);
                        for(int k = 0; k < relicCount; k++)
                        {
                            var relic = RelicManager.HandleRandomRelic(grade, RelicAction.Acquire);
                            requireLog += $"{relic.name}이(가) 제거되었습니다.";
                        }
                    }

                    break;
                case RequireThing.Gold:
                    if(choiceData.choiceId == 27)
                    {
                        RogueLikeData.Instance.SetCurrentGold(0);
                        requireLog = "모든 금화를 잃었습니다.";
                    }
                    else if(form == RequireForm.None)
                    {
                        int goldCount = int.Parse(count);
                        RogueLikeData.Instance.ReduceGold(goldCount);
                        requireLog += $"{goldCount}금화를 지불하였습니다.\n";
                    }
                    break;
                case RequireThing.Morale:
                    if(form == RequireForm.None)
                    {
                        float reduceMorale = 1;
                        if (RogueLikeData.Instance.GetOwnedRelicById(33) == null)
                        {
                            reduceMorale += 0.2f;
                        }
                        int moraleCount = (int)(int.Parse(count)*reduceMorale);
                        moraleCount = RogueLikeData.Instance.ChangeMorale(moraleCount);
                        requireLog += $"사기가 {moraleCount}만큼 감소했습니다.\n";
                    }
                    break;
            }
        }
    }
    //선택시 보상 획득
    public static (string,bool) ApplyChoiceResult(EventChoiceData choiceData,List<RogueUnitDataBase> selectedUnits)
    {
        string resultLog = "";
        bool isBattle= false;
        int battleGrade = 0;
        for (int i = 0; i < choiceData.resultType.Count; i++)
        {
            ResultType type = choiceData.resultType[i];
            ResultForm form = choiceData.resultForm[i];
            string value = choiceData.resultValue[i];
            string count = choiceData.resultCount[i];

            switch (type)
            {
                case ResultType.Gold:
                    {
                        int gold = int.Parse(count);
                        if (isBattle)
                        {
                            RogueLikeData.Instance.AddGoldReward(gold);
                        }
                        else
                        {
                            gold = RogueLikeData.Instance.AddGoldByEventChapter(gold);
                            resultLog += $"- 금화 {gold} 획득\n";
                        }
                        break;
                    }

                case ResultType.Morale:
                    {
                        int morale = int.Parse(count);
                        if (isBattle)
                        {
                            RogueLikeData.Instance.AddMoraleReward(morale);
                        }
                        else
                        {
                            morale = RogueLikeData.Instance.ChangeMorale(morale);
                            resultLog += $"- 사기 {morale} 회복\n";
                        }
                        break;
                    }

                case ResultType.Energy:
                    int energy = int.Parse(value);
                    if (form == ResultForm.Select)
                    {
                        foreach(var unit in selectedUnits)
                        {
                            unit.energy = Math.Max(unit.maxEnergy, unit.energy+energy);
                            resultLog += $"-기력 회복 {unit.unitName}\n";
                        }
                    }
                    else if(form == ResultForm.All)
                    {
                        var myUnits = RogueLikeData.Instance.GetMyTeam();
                        foreach(var unit in myUnits)
                        {
                            unit.energy = unit.maxEnergy;
                        }
                        resultLog += "모든 유닛의 기력이 회복 되었습니다.";
                    }
                    break;

                case ResultType.Relic:
                    int grade = int.Parse(value);
                    int relicCount = int.Parse(count);
                    if (form == ResultForm.Random)
                    {
                        if (isBattle)
                        {
                            for(int k=0; k<relicCount; k++)
                            {
                                RogueLikeData.Instance.AddRelicReward(RelicManager.GetRandomRelicId(grade, RelicAction.Acquire));
                            }
                        }
                        else
                        {
                            for (int k = 0; k < relicCount; k++)
                            {
                                var relic = RelicManager.HandleRandomRelic(grade, RelicAction.Acquire);
                                resultLog += $"- 전쟁 유산 획득: {relic.name}\n";
                            }
                        }
                    }
                    else if(form == ResultForm.Special)
                    {
                        if (choiceData.choiceId == 59 && UnityEngine.Random.value < 0.5f)
                        {
                            var relic = RelicManager.HandleRandomRelic(0, RelicAction.Acquire);
                            resultLog += $"- 전쟁 유산 획득: {relic.name}\n";
                        }
                        else if(choiceData.choiceId == 73)
                        {
                            if (IsUnitVictory(selectedUnits[0]))
                            {
                                var relic = RelicManager.HandleRandomRelic(10, RelicAction.Acquire);
                                resultLog += $"- 전쟁 유산 획득: {relic.name}\n";
                            }
                            else
                            {
                                resultLog += $"- 경기에 패배해 아무것도 얻지 못했습니다.\n";
                            }
                        }
                        else if(choiceData.choiceId == 75)
                        {
                            if (IsUnitVictoryByRarity(selectedUnits[0]))
                            {
                                RogueLikeData.Instance.AddMyUnis(selectedUnits[0]);
                                var relic =RelicManager.HandleRandomRelic(10,RelicAction.Acquire);
                                resultLog += $"- 결투에 승리해 전쟁유산 {relic.name} 획득";
                            }
                            else
                            {
                                resultLog += $"- 결투에 패배해 아무것도 얻지 못했습니다.\n";
                            }
                        }
                        else if(choiceData.choiceId == 82)
                        {
                            var relicIds = RogueLikeData.Instance.GetAllOwnedRelicIds();
                            int rewardRelicId = -1;
                            if(!relicIds.Contains(23)) rewardRelicId = 23;
                            else if(!relicIds.Contains(24)) rewardRelicId = 24;
                            else rewardRelicId = 25;
                            RogueLikeData.Instance.AcquireRelic(rewardRelicId);
                            resultLog += $"- 보석 건틀릿의 마지막 유산을 획득했습니다.";
                        }
                    }
                    break;
                case ResultType.Unit:
                    if(form == ResultForm.None)
                    {
                        int unitId = int.Parse(value);
                        int unitCount = int.Parse(count);
                        for (int k = 0; k < unitCount; k++)
                        {
                            RogueUnitDataBase unit = UnitLoader.Instance.GetCloneUnitById(unitId);
                            RogueLikeData.Instance.AddMyUnis(unit);
                            resultLog += $"- {unit.unitName}이(가) 추가되었습니다.";
                        }
                    }
                    else if (form == ResultForm.Random)
                    {
                        if (value.Contains("~"))
                        {
                            int unitCount = int.Parse(count);
                            var (min, max) = ParseRange(value);

                            var allUnits = UnitLoader.Instance.GetAllCachedUnits();

                            var validUnits = allUnits.Where(u => u.rarity >= min && u.rarity <= max).ToList();

                            for (int k = 0; k < unitCount && validUnits.Count > 0; k++)
                            {
                                int randIndex = UnityEngine.Random.Range(0, validUnits.Count);
                                validUnits.RemoveAt(randIndex);
                                RogueUnitDataBase newUnit = UnitLoader.Instance.GetCloneUnitById(validUnits[randIndex].idx);
                                if (isBattle)
                                {
                                    RogueLikeData.Instance.AddUnitReward(newUnit);
                                }
                                else
                                {
                                    RogueLikeData.Instance.AddMyUnis(newUnit);
                                    resultLog += $"- {newUnit.unitName}이(가) 추가되었습니다.";
                                }
                            }
                        }
                        else
                        {
                            int unitRarity = int.Parse(value);
                            int unitCount = int.Parse(count);

                            var allUnits = UnitLoader.Instance.GetAllCachedUnits();
                            List<RogueUnitDataBase> validUnits;
                            if (unitRarity == 4)
                            {
                                var myUnits = RogueLikeData.Instance.GetMyTeam();
                                var myUnitIdxSet = System.Linq.Enumerable.ToHashSet(myUnits.Select(u => u.idx)); // 중복 탐색 최적화

                                validUnits = allUnits
                                    .Where(u => u.rarity == unitRarity && !myUnitIdxSet.Contains(u.idx))
                                    .ToList();
                                if (validUnits.Count == 0)
                                {
                                    WarRelic relic = RelicManager.HandleRandomRelic(10, RelicAction.Acquire);
                                    resultLog += $"모든 영웅유닛을 보유하고 있습니다. 전쟁유산 {relic.name} 획득";
                                    break;
                                }
                            }
                            else
                            {
                                validUnits = allUnits.Where(u => u.rarity == unitRarity).ToList();
                            }

                            for(int k = 0; k < unitCount; k++)
                            {
                                int randIndex = UnityEngine.Random.Range(0, validUnits.Count);
                                validUnits.RemoveAt(randIndex);
                                RogueUnitDataBase newUnit = UnitLoader.Instance.GetCloneUnitById(validUnits[randIndex].idx);
                                if (isBattle)
                                {
                                    RogueLikeData.Instance.AddUnitReward(newUnit);
                                }
                                else
                                {
                                    RogueLikeData.Instance.AddMyUnis(newUnit);
                                    resultLog += $"- {newUnit.unitName}이(가) 추가되었습니다.";
                                }
                            }
                        }
                    }
                    else if(form == ResultForm.Select)
                    {
                        var originUnit = selectedUnits[0];
                        RogueUnitDataBase clone= UnitLoader.Instance.GetCloneUnitById(originUnit.idx);
                        clone.energy=originUnit.energy;
                        RogueLikeData.Instance.AddMyUnis(clone);
                        resultLog += $"- {clone.unitName}이(가) 추가되었습니다.";
                    }
                    else if (form == ResultForm.Special)
                    {
                        int unitRarity = int.Parse(value);
                        int unitCount = int.Parse(count);
                        var allUnits = UnitLoader.Instance.GetAllCachedUnits();
                        List<RogueUnitDataBase> validUnits;
                        var originUnit = selectedUnits[0];
                        float chance = originUnit.rarity switch
                        {
                            1 => 0.3f,
                            2 => 0.6f,
                            3 => 1.0f,
                            _ => 0f
                        };
                        if (UnityEngine.Random.value < chance)
                        {
                            var myUnits = RogueLikeData.Instance.GetMyTeam();
                            var myUnitIdxSet = new HashSet<int>(myUnits.Select(u => u.idx));

                            validUnits = allUnits
                                .Where(u => u.rarity == 4 && !myUnitIdxSet.Contains(u.idx))
                                .ToList();

                            if (validUnits.Count == 0)
                            {
                                resultLog += $"모든 영웅 유닛을 보유하고 있습니다.";
                                break;
                            }
                            for (int k = 0; k < unitCount && validUnits.Count > 0; k++)
                            {
                                int randIndex = UnityEngine.Random.Range(0, validUnits.Count);
                                var newUnit = validUnits[randIndex];
                                validUnits.RemoveAt(randIndex);

                                RogueLikeData.Instance.AddMyUnis(newUnit);
                                resultLog += $"'{originUnit.unitName}'을(를) 희생하고 '{newUnit.unitName}'을(를) 얻었습니다.\n";
                            }
                        }
                        else
                        {
                            resultLog += $"'{originUnit.unitName}'을(를) 희생했지만 아무 일도 일어나지 않았습니다.\n";
                        }

                    }
                    break;
                case ResultType.Change:
                    int changeCount = int.Parse(count);
                    if (form == ResultForm.Select)
                    {
                        if (isBattle)
                        {
                            foreach(var unit in selectedUnits)
                            {
                                RogueUnitDataBase newUnit = RogueUnitDataBase.RandomUnitReForm(unit);
                                for(int k = 1;k< changeCount; k++)
                                {
                                    newUnit = RogueUnitDataBase.RandomUnitReForm(unit);
                                }
                                RogueLikeData.Instance.AddChangeReward(newUnit);
                            }
                        }
                        else
                        {
                            foreach (var unit in selectedUnits)
                            {
                                RogueUnitDataBase newUnit = RogueUnitDataBase.RandomUnitReForm(unit);
                                for (int k = 1; k < changeCount; k++)
                                {
                                    newUnit = RogueUnitDataBase.RandomUnitReForm(unit);
                                }
                                RogueLikeData.Instance.AddMyUnis(newUnit);
                                resultLog += $"- {unit.unitName}이 전직해 {newUnit.unitName}이(가) 되었습니다.\n";
                            }
                        }
                    }
                    else if(form == ResultForm.Random)
                    {
                        int unitCount = int.Parse(count);
                        var myUnits = RogueLikeData.Instance.GetMyTeam();

                        var candidates = myUnits.Where(u => u.rarity < 4).ToList();

                        var changeUnits = candidates
                            .OrderBy(_ => UnityEngine.Random.value)
                            .Take(changeCount)
                            .ToList();
                        foreach(var unit in changeUnits)
                        {
                            var newUnit = RogueUnitDataBase.RandomUnitReForm(unit);
                            RogueLikeData.Instance.AddMyUnis(newUnit);
                            resultLog += $"- {unit.unitName}이 전직해 {newUnit.unitName}이(가) 되었습니다.";
                        }

                    }
                    break;
                case ResultType.Special:
                    if (choiceData.choiceId == 1)
                    {
                        int randEffect = UnityEngine.Random.Range(0, 9);

                        switch (randEffect)
                        {
                            case 0:
                                {
                                    var relic = RelicManager.HandleRandomRelic(grade: 1, RelicManager.RelicAction.Acquire);
                                    resultLog += $"- 일반 등급 전쟁 유산 '{relic?.name}'을 획득했습니다.\n";
                                    break;
                                }
                            case 1:
                                {
                                    var relic = RelicManager.HandleRandomRelic(grade: 0, RelicManager.RelicAction.Acquire);
                                    resultLog += $"- 저주 등급 전쟁 유산 '{relic?.name}'을 획득했습니다.\n";
                                    break;
                                }
                            case 2:
                                {
                                    var myUnits = RogueLikeData.Instance.GetMyTeam();
                                    if (myUnits.Count > 0)
                                    {
                                        var unit = myUnits[UnityEngine.Random.Range(0, myUnits.Count)];
                                        var promoted = RogueUnitDataBase.RandomUnitReForm(unit);
                                        if (promoted != null)
                                        {
                                            myUnits[myUnits.IndexOf(unit)] = promoted;
                                            RogueLikeData.Instance.SetMyTeam(myUnits);
                                            resultLog += $"- '{unit.unitName}'이(가) 전직하여 '{promoted.unitName}'이(가) 되었습니다.\n";
                                        }
                                    }
                                    break;
                                }
                            case 3:
                                {
                                    var myUnits = RogueLikeData.Instance.GetMyTeam().Where(u => u.energy > 1).ToList();
                                    if (myUnits.Count > 0)
                                    {
                                        var target = myUnits[UnityEngine.Random.Range(0, myUnits.Count)];
                                        target.energy = 1;
                                        resultLog += $"- '{target.unitName}'의 기력이 1이 되었습니다.\n";
                                    }
                                    break;
                                }
                            case 4:
                                {
                                    var (min, max) = ParseRange("1~3");

                                    var allUnits = UnitLoader.Instance.GetAllCachedUnits();
                                    var myIdxSet = Enumerable.ToHashSet(RogueLikeData.Instance.GetMyTeam().Select(u => u.idx));
                                    var candidates = allUnits.Where(u => u.rarity >= min && u.rarity <= max && !myIdxSet.Contains(u.idx)).ToList();

                                    if (candidates.Count > 0)
                                    {
                                        var selected = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                                        RogueLikeData.Instance.AddMyUnis(selected);
                                        resultLog += $"- 무작위 유닛 '{selected.unitName}'을(를) 획득했습니다.\n";
                                    }
                                    break;
                                }
                            case 5:
                                {
                                    RogueLikeData.Instance.IncreaseRandomUpgrade(false);
                                    resultLog += "- 무작위 병종이 강화되었습니다.\n";
                                    break;
                                }
                            case 6:
                                {
                                    RogueLikeData.Instance.SetMorale(100);
                                    resultLog += "- 사기가 최대치로 회복되었습니다.\n";
                                    break;
                                }
                            case 7:
                                {
                                    int morale = RogueLikeData.Instance.ChangeMorale(25);
                                    resultLog += $" 사기가 {morale} 감소했습니다.\n";
                                    break;
                                }
                            case 8:
                                {
                                    int getGold = RogueLikeData.Instance.AddGoldByEventChapter(150);
                                    resultLog += $"- 금화 {getGold}를 획득했습니다.\n";
                                    break;
                                }
                        }
                    }
                    else if (choiceData.choiceId == 25)
                    {
                        var unit = selectedUnits[0];
                        int rarity = unit.rarity;

                        var myUnits = RogueLikeData.Instance.GetMyTeam();
                        int index = myUnits.IndexOf(unit);

                        switch (rarity)
                        {
                            case 1:
                                {
                                    int getGold = RogueLikeData.Instance.AddGoldByEventChapter(50);
                                    resultLog += $"'{unit.unitName}'을(를) 희생하여 금화 {getGold}를 획득했습니다.\n";
                                    break;
                                }
                            case 2:
                                {
                                    int getGold = RogueLikeData.Instance.AddGoldByEventChapter(150);
                                    resultLog += $"'{unit.unitName}'을(를) 희생하여 금화 {getGold}를 획득했습니다.\n";
                                    break;
                                }
                            case 3:
                                {
                                    int getGold = RogueLikeData.Instance.AddGoldByEventChapter(150);
                                    var relic = RelicManager.HandleRandomRelic(grade: 1, RelicManager.RelicAction.Acquire);
                                    resultLog += $"'{unit.unitName}'을(를) 희생하여 금화 {getGold}와 전쟁 유산 '{relic?.name}'을 획득했습니다.\n";
                                    break;
                                }
                            default:
                                {
                                    resultLog += $"'{unit.unitName}'의 희귀도({rarity})는 처리되지 않았습니다. 희생만 처리되었습니다.\n";
                                    break;
                                }
                        }

                    }
                    else if(choiceData.choiceId == 37)
                    {
                        if(UnityEngine.Random.value < 0.5f)
                        {
                            WarRelic relic = RelicManager.HandleRandomRelic(5, RelicManager.RelicAction.Acquire);
                            resultLog += $"'{relic.name}을 획득했습니다.\n";
                        }
                        else
                        {
                            battleGrade = 5;
                            isBattle = true;
                        }
                    }
                    break;

                case ResultType.None:
                    resultLog += "- 아무 일도 일어나지 않았다\n";
                    break;
                case ResultType.Battle:
                    {
                        battleGrade = int.Parse(value);
                        if(form == ResultForm.Special)
                        {
                            if (choiceData.choiceId == 16 && UnityEngine.Random.value < 0.5f)
                            {
                                resultLog += "- 아무 일도 일어나지 않았다\n";
                                break;
                            }
                            else if (choiceData.choiceId == 108)
                            {
                                RogueLikeData.Instance.SetCurrentStage(15, 1, StageType.Boss);
                                battleGrade = 20;
                            }
                        }

                        isBattle = true;
                        break;
                    }
                case ResultType.Training:
                    {
                        for (int k = 0; k < choiceData.resultValue.Count; k++)
                        {
                            string valueStr = choiceData.resultValue[k];
                            bool useRandom = string.IsNullOrEmpty(valueStr) || valueStr == "-1";

                            int upgradeCount = (k < choiceData.resultCount.Count && int.TryParse(choiceData.resultCount[k], out var c)) ? c : 1;

                            for (int j = 0; j < upgradeCount; j++)
                            {
                                if (useRandom)
                                {
                                    RogueLikeData.Instance.IncreaseRandomUpgrade(false);
                                }
                                else
                                {
                                    var unitTypeParts = valueStr.Split(',');
                                    List<int> unitTypes = unitTypeParts
                                        .Select(s => int.TryParse(s, out var v) ? v : -1)
                                        .Where(v => v >= 0 && v < 8)
                                        .ToList();

                                    if (unitTypes.Count == 0) continue;

                                    int randomType = unitTypes[UnityEngine.Random.Range(0, unitTypes.Count)];
                                    bool isAttack = UnityEngine.Random.value < 0.5f;

                                    RogueLikeData.Instance.IncreaseUpgrade(randomType, isAttack, false);
                                }
                            }
                            resultLog += $"랜덤 병종 {upgradeCount}회 강화함\n";
                        }
                        break;
                    }
                case ResultType.Field:
                    {
                        int fieldId = int.Parse(value);
                        RogueLikeData.Instance.SetFieldId(fieldId);
                        resultLog += "다음 전장이 변경되었습니다.";
                    }
                    break;
                default:
                    resultLog += "- 알 수 없는 보상\n";
                    break;
            }
        }
        if (isBattle)
        {
            SetPresetIdByGrade(battleGrade);
            GameManager.Instance.OpenBattlePanel();
        }
        return (resultLog,isBattle);
    }

    private static bool IsUnitVictory(RogueUnitDataBase unit)
    {
        if (unit.mobility >= 12f)
            return true;

        float winChance = unit.mobility * 0.09f;
        return UnityEngine.Random.value < winChance;
    }
    public static bool IsUnitVictoryByRarity(RogueUnitDataBase unit)
    {
        float winChance = unit.rarity switch
        {
            1 => 0.25f, 
            2 => 0.5f,  
            3 => 0.75f, 
            4 => 0.9f,  
            _ => 0f     
        };

        return UnityEngine.Random.value < winChance;
    }
    public static (int min, int max) ParseRange(string countStr)
    {
        if (string.IsNullOrEmpty(countStr))
            return (int.MinValue, int.MaxValue);

        if (countStr.Contains('~'))
        {
            var parts = countStr.Split('~');
            int min = int.Parse(parts[0]);
            int max = int.Parse(parts[1]);
            return (min, max);
        }

        int v = int.Parse(countStr);
        return (v, int.MaxValue); 
    }

    private static bool InRange(int actual, string countStr, bool useMinBound = true,float addtion=1)
    {
        if (countStr.Contains('~'))
        {
            var (min, max) = ParseRange(countStr);
            return actual >= min && actual <= max*addtion;
        }

        int v = (int)(int.Parse(countStr)*addtion);
        return useMinBound ? actual >= v : actual <= v;
    }
    public static EventData GetEventById(int eventId)
{
    if (EventDataLoader.EventDataDict.TryGetValue(eventId, out var eventData))
    {
        if (CanAppear(eventData))
        {
            return eventData;
        }
        else
        {
            Debug.LogWarning($"이벤트 {eventId}는 현재 등장 조건을 만족하지 않음.");
            return null;
        }
    }
    return null;
}

    private static void SetPresetIdByGrade(int grade)
    {
        int chapter = RogueLikeData.Instance.GetChapter();
        int level = RogueLikeData.Instance.GetCurrentStageX();

        string targetStageType = "normal";
        switch (grade) 
        {
            case 1:
                {
                    RogueLikeData.Instance.SetStageType(StageType.Combat);
                    targetStageType = "normal";
                }
                break;
            case 5:
                {
                    RogueLikeData.Instance.SetStageType(StageType.Elite);
                    targetStageType = "elite";
                }
                break;
            case 20:
                {
                    RogueLikeData.Instance.SetCurrentStage(15,1,StageType.Boss);
                    targetStageType = "boss";
                }
                break;
        }

        if (targetStageType == null) return;

        List<StagePreset> filtered = StagePresetLoader.I.presets
            .Where(p => p.Chapter == chapter && p.StageType == targetStageType)
            .ToList();

        if (grade == 1)
        {
            filtered = filtered.Where(p => p.Level == level).ToList();
        }

        if (filtered.Count == 0) return;
        StagePreset stage =  filtered[UnityEngine.Random.Range(0, filtered.Count)];
        RogueLikeData.Instance.SetPresetID(stage.PresetID);
    }


}
