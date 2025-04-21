using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static RelicManager;

public class EventManager
{
    public static void LoadEventData()
    {
        EventDataLoader.LoadData();
    }
    public static EventData GetRandomEvent()
    {
        Dictionary<int, int> encounterEvent = RogueLikeData.Instance.GetEncounteredEvent();
        List<int> availableEventIds = new();

        foreach (var id in EventDataLoader.EventDataDict.Keys)
        {
            if (encounterEvent.ContainsKey(id))
                continue;

            var eventData = EventDataLoader.EventDataDict[id];
            
            if (CanAppear(eventData))
                availableEventIds.Add(id);
        }
        
        if (availableEventIds.Count == 0)
        {
            Debug.Log("모든 이벤트를 만났거나 등장조건 불일치");
            return null;
        }

        int randomIdx = UnityEngine.Random.Range(0, availableEventIds.Count);
        int eventId = availableEventIds[randomIdx];
        return EventDataLoader.EventDataDict[eventId];
    }
    private static bool CanAppear(EventData eventData)
    {
        int currentChapter = RogueLikeData.Instance.GetChapter();

        // 등장 챕터 조건 확인
        if (eventData.eventChapter.Count > 0 && !eventData.eventChapter.Contains(currentChapter))
            return false;

        // 모든 조건을 확인하여 하나라도 실패하면 false
        for (int i = 0; i < eventData.requireThing.Count; i++)
        {
            var thing = eventData.requireThing[i];
            var form = eventData.requireForm[i];
            var value = eventData.requireValue[i];
            var count = eventData.requireCount[i];

            if (thing == RequireThing.None)
                continue;

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

        // 모든 조건 통과 시 등장 가능
        return true;
    }

    //등장 조건 확인
    private static bool CheckRequireCondition(RequireThing thing, RequireForm form, string value,string count)
    {
        switch (thing)
        {
            case RequireThing.Gold:
                if (form == RequireForm.None)
                {
                    int gold = RogueLikeData.Instance.GetCurrentGold();
                    return RogueLikeData.Instance.GetCurrentGold() >= int.Parse(count);
                }
                break;
            case RequireThing.Morale:
                if (form == RequireForm.None)
                {
                    if (count.Contains("~"))
                    {
                        string[] parts = count.Split('~');
                        int min = int.Parse(parts[0]);
                        int max = int.Parse(parts[1]); // -붙여서 넣었으니까 제거

                        int morale = RogueLikeData.Instance.GetMorale();
                        return morale >= min && morale <= max;
                    }
                    else
                    {
                        return RogueLikeData.Instance.GetMorale() >= int.Parse(count);
                    }
                }
                break;
            case RequireThing.Unit:
                if (form == RequireForm.Select)
                {
                    if (value.Contains("~"))
                    {
                        var myUnits =RogueLikeData.Instance.GetMyUnits();

                        string[] parts = count.Split('~');
                        int min = int.Parse(parts[0]);
                        int max = int.Parse(parts[1]); // -붙여서 넣었으니까 제거
                        int unitCount = 0;
                        foreach(var unit in myUnits)
                        {
                            if(unit.rarity>=min && unit.rarity <= max)
                            {
                                unitCount++;
                                if(unitCount>=int.Parse(count)) return true;
                            }
                        }
                        return false;
                    }
                    else if (!string.IsNullOrEmpty(value))
                    {
                        var myUnits = RogueLikeData.Instance.GetMyUnits();
                        int rarity = int.Parse(value);
                        int unitCount = 0;
                        foreach (var unit in myUnits)
                        {
                            if (unit.rarity <= rarity)
                            {
                                unitCount++;
                                if (unitCount >= int.Parse(count)) return true;
                            }
                        }
                        return false;
                    }
                    return RogueLikeData.Instance.GetMyUnits().Count >= int.Parse(count);
                }
                else if(form == RequireForm.Random)
                {
                    if (value.Contains("~"))
                    {
                        var myUnits = RogueLikeData.Instance.GetMyUnits();

                        string[] parts = count.Split('~');
                        int min = int.Parse(parts[0]);
                        int max = int.Parse(parts[1]); // -붙여서 넣었으니까 제거
                        int unitCount = 0;
                        foreach (var unit in myUnits)
                        {
                            if (unit.rarity >= min && unit.rarity <= max)
                            {
                                unitCount++;
                                if (unitCount >= int.Parse(count)) return true;
                            }
                        }
                        return false;
                    }
                    else if (!string.IsNullOrEmpty(value))
                    {
                        var myUnits = RogueLikeData.Instance.GetMyUnits();
                        int rarity = int.Parse(value);
                        int unitCount = 0;
                        foreach (var unit in myUnits)
                        {
                            if (unit.rarity <= rarity)
                            {
                                unitCount++;
                                if (unitCount >= int.Parse(count)) return true;
                            }
                        }
                        return false;
                    }
                    return RogueLikeData.Instance.GetMyUnits().Count >= int.Parse(count);
                }
                else if (form == RequireForm.Special)
                {
                    return RogueLikeData.Instance.GetMyUnits().Count >= int.Parse(count);
                }
                break;

            case RequireThing.Relic:
                if (form == RequireForm.Random) // 특정 등급 유산 존재 여부 검사
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        var relics = RogueLikeData.Instance.GetAllOwnedRelics();
                        int grade = int.Parse(value);
                        int relicCount = 0;
                        foreach (var relic in relics)
                        {
                            if(relic.grade == grade)
                            {
                                relicCount++;
                                if (relicCount >= int.Parse(count)) return true;
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
                    return !RogueLikeData.Instance.GetAllOwnedRelics().Exists(relic => relic.id == relicId);
                }
                break;
            case RequireThing.Energy:
                if(form == RequireForm.Random)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        var myUnits = RogueLikeData.Instance.GetMyUnits();
                        int energyValue = int.Parse(value);
                        int unitCount= 0;   
                        foreach (var unit in myUnits)
                        {
                            if(unit.energy > energyValue)
                            {
                                unitCount++;
                                if(unitCount >= int.Parse(count)) return true;
                            }
                        }
                        return false;
                    }
                }
                if(form == RequireForm.Select)
                {
                    if(string.IsNullOrEmpty(value)) return true;

                    var myUnits = RogueLikeData.Instance.GetMyUnits();
                    int energyValue = int.Parse(value);
                    int unitCount = 0;
                    foreach (var unit in myUnits)
                    {
                        if (unit.energy > energyValue)
                        {
                            unitCount++;
                            if (unitCount >= int.Parse(count)) return true;
                        }
                    }
                }
                break;
            case RequireThing.AttackDamage:
                {
                    int threshold = int.Parse(count);
                    var myUnits = RogueLikeData.Instance.GetMyUnits();
                    foreach (var unit in myUnits)
                    {
                        if (unit.attackDamage >= threshold)
                            return true;
                    }
                }
                break;
        }
        return false;
    }

    private static bool CheckSpecialRequire(EventData eventData)
    {
        int gold = RogueLikeData.Instance.GetCurrentGold();
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
        // 그 외 조건 생기면 추가
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
            var count = eventChoiceData.resultCount[i];

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
                        var myUnits = RogueLikeData.Instance.GetMyUnits();
                        int energy = int.Parse(value);
                        int unitCount = int.Parse(value);

                        // energy 보다 높은 에너지를 가진 유닛 필터링
                        List<RogueUnitDataBase> filteredUnits = myUnits.FindAll(unit => unit.energy > energy);

                        // 랜덤 셔플을 위해 리스트 섞기
                        System.Random random = new System.Random();
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
                        var myUnits = RogueLikeData.Instance.GetMyUnits();
                        var selectedUnits = RogueLikeData.Instance.GetSelectedUnits();
                        myUnits.RemoveAll(selectedUnits.Contains);
                        RogueLikeData.Instance.SetAllMyUnits(myUnits);
                        foreach(var unit in selectedUnits)
                        {
                            requireLog += $"{unit.unitName}이(가) 선택되었습니다.\n";
                        }
                    }
                    else if (form == RequireForm.Random)
                    {
                        var myUnits = RogueLikeData.Instance.GetMyUnits();
                        int maxRarity = int.Parse(value);
                        int unitCount = int.Parse(count);

                        // 조건에 맞는 유닛 필터링 후 랜덤 정렬, 원하는 수만큼 선택
                        var candidates = myUnits
                            .Where(unit => unit.rarity <= maxRarity)
                            .OrderBy(_ => UnityEngine.Random.value)
                            .Take(unitCount)
                            .ToList();

                        // 선택된 유닛 제거
                        var currentUnits = RogueLikeData.Instance.GetMyUnits(); // 새로 가져오기
                        currentUnits.RemoveAll(unit => candidates.Contains(unit));
                        RogueLikeData.Instance.SetAllMyUnits(currentUnits);
                        foreach (var unit in candidates)
                        {
                            RogueLikeData.Instance.AddSelectedUnits(unit);
                            requireLog += $"{unit.unitName}이(가) 선택되었습니다.\n";
                        }
                    }
                    else if(form == RequireForm.Special)
                    {
                        //82번
                        if (choiceData.choiceId == 82)
                        {
                            Debug.Log("미구현");
                            requireLog += "미구현 입니다";
                        }
                    }
                    else if(form == RequireForm.None)
                    {
                        var myUnits = RogueLikeData.Instance.GetMyUnits();
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
                        int gold = RogueLikeData.Instance.GetCurrentGold();
                        RogueLikeData.Instance.SetCurrentGold(gold-goldCount);
                        requireLog += $"{goldCount}금화를 지불하였습니다.\n";
                    }
                    break;
                case RequireThing.Morale:
                    if(form == RequireForm.None)
                    {
                        int moraleCount = int.Parse(count);
                        int morale =RogueLikeData.Instance.GetMorale();
                        RogueLikeData.Instance.SetMorale(morale-moraleCount);
                        requireLog += $"사기가 {moraleCount}만큼 감소했습니다.\n";
                    }
                    break;
            }

        }
    }
    //선택시 보상 획득
    public static string ApplyChoiceResult(EventChoiceData choiceData,List<RogueUnitDataBase> selectedUnits)
    {
        string resultLog = "";
        bool isBattle= false;

        for (int i = 0; i < choiceData.resultType.Count; i++)
        {
            ResultType type = choiceData.resultType[i];
            ResultForm form = choiceData.resultForm[i];
            string value = choiceData.resultValue[i];
            string count = choiceData.resultCount[i];

            switch (type)
            {
                case ResultType.Gold:
                    int gold = int.Parse(value);
                    gold= RogueLikeData.Instance.AddGoldByEventChapter(gold);
                    resultLog += $"- 금화 {gold} 획득\n";
                    break;

                case ResultType.Morale:
                    int morale = int.Parse(value);
                    RogueLikeData.Instance.SetMorale(
                        Mathf.Min(100, RogueLikeData.Instance.GetMorale() + morale));
                    resultLog += $"- 사기 {morale} 회복\n";
                    break;

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
                        var myUnits = RogueLikeData.Instance.GetMyUnits();
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
                        for(int k = 0; k < relicCount; k++)
                        {
                            var relic = RelicManager.HandleRandomRelic(grade, RelicAction.Acquire);
                            resultLog += $"- 전쟁 유산 획득: {relic.name}\n";
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
                                resultLog += $"- 결투에 승리해 전쟁유사 {relic.name} 획득";
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
                        var row = GoogleSheetLoader.Instance.GetRowUnitData(unitId);
                        for (int k = 0; k < unitCount; k++)
                        {
                            RogueUnitDataBase unit = RogueUnitDataBase.ConvertToUnitDataBase(row);
                            RogueLikeData.Instance.AddMyUnis(unit);
                            resultLog += $"- {row[1]}이(가) 추가되었습니다.";
                        }
                    }
                    else if (form == ResultForm.Random)
                    {
                        if (value.Contains("~"))
                        {
                            int unitCount = int.Parse(count);
                            string[] parts = value.Split('~');
                            int min = int.Parse(parts[0]);
                            int max = int.Parse(parts[1]);

                            // 전체 유닛 불러오기
                            var allUnits = GoogleSheetLoader.Instance.GetAllUnitsAsObject();

                            // 희귀도 범위 조건에 맞는 유닛 필터링
                            var validUnits = allUnits.Where(u => u.rarity >= min && u.rarity <= max).ToList();

                            for (int k = 0; k < unitCount && validUnits.Count > 0; k++)
                            {
                                int randIndex = UnityEngine.Random.Range(0, validUnits.Count);
                                RogueLikeData.Instance.AddMyUnis(validUnits[randIndex]);
                                validUnits.RemoveAt(randIndex); // 중복 방지
                                resultLog += $"- {validUnits[randIndex].unitName}이(가) 추가되었습니다.";
                            }

                        }
                        else
                        {
                            int unitRarity = int.Parse(value);
                            int unitCount = int.Parse(count);
                            // 전체 유닛 불러오기
                            var allUnits = GoogleSheetLoader.Instance.GetAllUnitsAsObject();
                            List<RogueUnitDataBase> validUnits;
                            if (unitRarity == 4)
                            {
                                var myUnits = RogueLikeData.Instance.GetMyUnits();
                                var myUnitIdxSet = System.Linq.Enumerable.ToHashSet(myUnits.Select(u => u.idx)); // 중복 탐색 최적화

                                validUnits = allUnits
                                    .Where(u => u.rarity == unitRarity && !myUnitIdxSet.Contains(u.idx))
                                    .ToList();
                                if (validUnits.Count == 0)
                                {
                                    resultLog += $"모든 영웅유닛을 보유하고 있습니다.";
                                    break;
                                }
                            }
                            else
                            {
                                validUnits = allUnits.Where(u => u.rarity == unitRarity).ToList();
                            }

                            // 희귀도 범위 조건에 맞는 유닛 필터링
                              
                            for(int k = 0; k < unitCount; k++)
                            {
                                int randIndex = UnityEngine.Random.Range(0, validUnits.Count);
                                RogueLikeData.Instance.AddMyUnis(validUnits[randIndex]);
                                validUnits.RemoveAt(randIndex); // 중복 방지
                                resultLog += $"- {validUnits[randIndex].unitName}이(가) 추가되었습니다.";
                            }
                        }
                    }
                    else if(form == ResultForm.Select)
                    {
                        var originUnit = selectedUnits[0];
                        var row = GoogleSheetLoader.Instance.GetRowUnitData(originUnit.idx);
                        RogueUnitDataBase clone= RogueUnitDataBase.ConvertToUnitDataBase(row);
                        clone.energy=originUnit.energy;
                        RogueLikeData.Instance.AddMyUnis(clone);
                        resultLog += $"- {clone.unitName}이(가) 추가되었습니다.";
                    }
                    else if (form == ResultForm.Special)
                    {
                        int unitRarity = int.Parse(value);
                        int unitCount = int.Parse(count);
                        // 전체 유닛 불러오기
                        var allUnits = GoogleSheetLoader.Instance.GetAllUnitsAsObject();
                        List<RogueUnitDataBase> validUnits;
                        var originUnit = selectedUnits[0]; // 희생할 유닛
                        float chance = originUnit.rarity switch
                        {
                            1 => 0.3f,
                            2 => 0.6f,
                            3 => 1.0f,
                            _ => 0f // 희귀도 4 이상은 없음
                        };
                        if (UnityEngine.Random.value < chance)
                        {
                            // 중복되지 않은 영웅 유닛만 필터링
                            var myUnits = RogueLikeData.Instance.GetMyUnits();
                            var myUnitIdxSet = new HashSet<int>(myUnits.Select(u => u.idx));

                            validUnits = allUnits
                                .Where(u => u.rarity == 4 && !myUnitIdxSet.Contains(u.idx))
                                .ToList();

                            if (validUnits.Count == 0)
                            {
                                resultLog += $"모든 영웅 유닛을 보유하고 있습니다.";
                                break;
                            }

                            // 랜덤하게 unitCount개 획득
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
                    if(form == ResultForm.Select)
                    {
                        foreach(var unit in selectedUnits)
                        {
                            var newUnit = RogueUnitDataBase.RandomUnitReForm(unit);
                            RogueLikeData.Instance.AddMyUnis(newUnit);
                            resultLog += $"- {unit.unitName}이 전직해 {newUnit.unitName}이(가) 되었습니다.";
                        }
                    }
                    else if(form == ResultForm.Random)
                    {
                        int unitCount = int.Parse(count);
                        var myUnits = RogueLikeData.Instance.GetMyUnits();

                        // rarity < 4 조건을 만족하는 유닛 필터링
                        var candidates = myUnits.Where(u => u.rarity < 4).ToList();

                        // 무작위로 unitCount개 뽑기 (중복 없이)
                        var changeUnits = candidates
                            .OrderBy(_ => UnityEngine.Random.value)
                            .Take(unitCount)
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
                            case 0: // 일반 등급 전쟁 유산
                                {
                                    var relic = RelicManager.HandleRandomRelic(grade: 1, RelicManager.RelicAction.Acquire);
                                    resultLog += $"- 일반 등급 전쟁 유산 '{relic?.name}'을 획득했습니다.\n";
                                    break;
                                }
                            case 1: // 저주 등급 전쟁 유산
                                {
                                    var relic = RelicManager.HandleRandomRelic(grade: 0, RelicManager.RelicAction.Acquire);
                                    resultLog += $"- 저주 등급 전쟁 유산 '{relic?.name}'을 획득했습니다.\n";
                                    break;
                                }
                            case 2: // 무작위 유닛 전직
                                {
                                    var myUnits = RogueLikeData.Instance.GetMyUnits();
                                    if (myUnits.Count > 0)
                                    {
                                        var unit = myUnits[UnityEngine.Random.Range(0, myUnits.Count)];
                                        var promoted = RogueUnitDataBase.RandomUnitReForm(unit);
                                        if (promoted != null)
                                        {
                                            myUnits[myUnits.IndexOf(unit)] = promoted;
                                            RogueLikeData.Instance.SetAllMyUnits(myUnits);
                                            resultLog += $"- '{unit.unitName}'이(가) 전직하여 '{promoted.unitName}'이(가) 되었습니다.\n";
                                        }
                                    }
                                    break;
                                }
                            case 3: // 무작위 유닛 기력을 1로
                                {
                                    var myUnits = RogueLikeData.Instance.GetMyUnits().Where(u => u.energy > 1).ToList();
                                    if (myUnits.Count > 0)
                                    {
                                        var target = myUnits[UnityEngine.Random.Range(0, myUnits.Count)];
                                        target.energy = 1;
                                        resultLog += $"- '{target.unitName}'의 기력이 1이 되었습니다.\n";
                                    }
                                    break;
                                }
                            case 4: // 무작위 희귀도 1~3 유닛 획득
                                {
                                    string[] parts = "1~3".Split('~');
                                    int min = int.Parse(parts[0]);
                                    int max = int.Parse(parts[1]);

                                    var allUnits = GoogleSheetLoader.Instance.GetAllUnitsAsObject();
                                    var myIdxSet = Enumerable.ToHashSet(RogueLikeData.Instance.GetMyUnits().Select(u => u.idx));
                                    var candidates = allUnits.Where(u => u.rarity >= min && u.rarity <= max && !myIdxSet.Contains(u.idx)).ToList();

                                    if (candidates.Count > 0)
                                    {
                                        var selected = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                                        RogueLikeData.Instance.AddMyUnis(selected);
                                        resultLog += $"- 무작위 유닛 '{selected.unitName}'을(를) 획득했습니다.\n";
                                    }
                                    break;
                                }
                            case 5: // 무작위 병종 무작위 강화 미구현
                                {
                                    //resultLog += RogueUnitTraining.ApplyRandomTraining();
                                    break;
                                }
                            case 6: // 사기 회복(최대)
                                {
                                    RogueLikeData.Instance.SetMorale(100);
                                    resultLog += "- 사기가 최대치로 회복되었습니다.\n";
                                    break;
                                }
                            case 7: // 사기 -25
                                {
                                    int current = RogueLikeData.Instance.GetMorale();
                                    RogueLikeData.Instance.SetMorale(Math.Max(0, current - 25));
                                    resultLog += " 사기가 25 감소했습니다.\n";
                                    break;
                                }
                            case 8: // 금화(중) 획득
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

                        // 희생 유닛 제거
                        var myUnits = RogueLikeData.Instance.GetMyUnits();
                        int index = myUnits.IndexOf(unit);

                        switch (rarity)
                        {
                            case 1:
                                {
                                    int getGold = RogueLikeData.Instance.AddGoldByEventChapter(50); // 소
                                    resultLog += $"'{unit.unitName}'을(를) 희생하여 금화 {getGold}를 획득했습니다.\n";
                                    break;
                                }
                            case 2:
                                {
                                    int getGold = RogueLikeData.Instance.AddGoldByEventChapter(150); // 중
                                    resultLog += $"'{unit.unitName}'을(를) 희생하여 금화 {getGold}를 획득했습니다.\n";
                                    break;
                                }
                            case 3:
                                {
                                    int getGold = RogueLikeData.Instance.AddGoldByEventChapter(150); // 중
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
                        Debug.Log("미구현");
                    }
                    break;

                case ResultType.None:
                    resultLog += "- 아무 일도 일어나지 않았다\n";
                    break;
                case ResultType.Battle:
                    isBattle = true;
                    break;
                case ResultType.Training:
                    resultLog += "병종강화 미구현";
                    break;
                default:
                    resultLog += "- 알 수 없는 보상\n";
                    break;
            }

            if(isBattle) break;
        }
        if(isBattle) return "전투다! 미구현";
        return resultLog;
    }
    //우승 유무
    private static bool IsUnitVictory(RogueUnitDataBase unit)
    {
        // 기동력 12 이상이면 무조건 승리
        if (unit.mobility >= 12f)
            return true;

        // 기동력 × 9% 확률
        float winChance = unit.mobility * 0.09f;
        return UnityEngine.Random.value < winChance;
    }
    public static bool IsUnitVictoryByRarity(RogueUnitDataBase unit)
    {
        float winChance = unit.rarity switch
        {
            1 => 0.25f,  // 희귀도 1 → 25%
            2 => 0.5f,   // 희귀도 2 → 50%
            3 => 0.75f,  // 희귀도 3 → 75%
            4 => 0.9f,   // 희귀도 4 → 90%
            _ => 0f      // 그 외 → 승리 불가
        };

        return UnityEngine.Random.value < winChance;
    }

}
