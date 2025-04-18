using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static RelicManager;
using static UnityEngine.UI.CanvasScaler;

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

        int randomIdx = Random.Range(0, availableEventIds.Count);
        int eventId = availableEventIds[randomIdx];
        return EventDataLoader.EventDataDict[eventId];
    }
    private static bool CanAppear(EventData eventData)
    {
        int currentChapter = RogueLikeData.Instance.GetChapter();

        // 등장 챕터 체크
        if (!eventData.eventChapter.Contains(currentChapter))
            return false;

        // 등장 조건 없으면 무조건 등장
        if (eventData.requireThing ==RequireThing.None)
        {
            return true;
        }

        // Special 조건 처리
        if (eventData.requireThing == RequireThing.Special)
        {
            return CheckSpecialRequire(eventData);
        }

        // 일반 조건 처리
        return CheckRequireCondition(eventData.requireThing, eventData.requireForm,"", eventData.requireValue);
    }
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
                    return RogueLikeData.Instance.GetMorale() > int.Parse(count);

                if (form == RequireForm.Special)
                {
                    // 예: 35,-50  → Morale >= 35 && <= 50
                    if (count.Contains(","))
                    {
                        string[] parts = count.Split(',');
                        int min = int.Parse(parts[0]);
                        int max = int.Parse(parts[1].Replace("-", "")); // -붙여서 넣었으니까 제거

                        int morale = RogueLikeData.Instance.GetMorale();
                        return morale >= min && morale <= max;
                    }
                }
                break;
            case RequireThing.Unit:
                if (form == RequireForm.None)
                {
                    if (!value.Contains(""))
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
                if (form == RequireForm.Count || form == RequireForm.Select)
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
                    else if (!value.Contains(""))
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
                    else if (!value.Contains(""))
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
                    // 예: 1~2,5 → 희귀도 1~2 유닛이 5개 이상
                    if (count.Contains("~") && count.Contains(","))
                    {
                        string[] parts = count.Split(',');
                        string rarityRange = parts[0];  // 1~2
                        int requiredCount = int.Parse(parts[1]);

                        int minRarity = int.Parse(rarityRange.Split('~')[0]);
                        int maxRarity = int.Parse(rarityRange.Split('~')[1]);

                        int unitCount = 0;
                        foreach (var unit in RogueLikeData.Instance.GetMyUnits())
                        {
                            if (unit.rarity >= minRarity && unit.rarity <= maxRarity)
                            {
                                unitCount++;
                            }
                        }
                        return unitCount >= requiredCount;
                    }
                }
                break;

            case RequireThing.Relic:
                if (form == RequireForm.Count || form == RequireForm.None) // 특정 등급 유산 존재 여부 검사
                {
                    if (!value.Contains(""))
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
                else if (form == RequireForm.Special) // 특정 id 유산이 없어야 true
                {
                    int relicId = int.Parse(count);
                    return !RogueLikeData.Instance.GetAllOwnedRelics().Exists(relic => relic.id == relicId);
                }
                break;
            case RequireThing.Energy:
                if (form == RequireForm.None)
                    return RogueLikeData.Instance.GetMyUnits().Exists(u => u.energy > int.Parse(count));
                if (form == RequireForm.Count)
                {
                    string[] split = count.Split(',');
                    int targetEnergy = int.Parse(split[0]);
                    int targetCount = int.Parse(split[1]);

                    int unitCount = 0;
                    foreach (var unit in RogueLikeData.Instance.GetMyUnits())
                    {
                        if (unit.energy == targetEnergy)
                            unitCount++;
                    }
                    return unitCount >= targetCount;
                }
                if(form == RequireForm.Random)
                {
                    if (!value.Contains(""))
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
                if(form == RequireForm.Select) return true;
                break;
            case RequireThing.AttackDamage:
                if (form == RequireForm.None)
                {
                    int threshold = int.Parse(count);
                    var myUnits = RogueLikeData.Instance.GetMyUnits();
                    foreach (var unit in myUnits)
                    {
                        if (unit.attackDamage >= threshold)
                            return true;
                    }
                    return false;
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
            Debug.Log("52 현재 미구현");
            return true;
        }
        return false;
    }
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
                            if (value == "") 
                            { 
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
                    if(form == RequireForm.None)
                    {

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
                    if (choiceData.resultForm.Contains(ResultForm.Select))
                    {

                    }
                    else if(choiceData.resultForm.Contains(ResultForm.All))
                    {

                    }
                    // 적용 방식 지정 필요 (현재 예시로 1명만 회복 가정)
                    if (RogueLikeData.Instance.GetMyUnits().Count > 0)
                    {
                        RogueLikeData.Instance.GetMyUnits()[0].energy += energy;
                        resultLog += $"- 유닛 기력 {energy} 회복\n";
                    }
                    break;

                case ResultType.Relic:
                    int grade = int.Parse(value);
                    var relic = RelicManager.HandleRandomRelic(grade, RelicAction.Acquire);
                    resultLog += $"- 전쟁 유산 획득: {relic.name}\n";
                    break;

                case ResultType.Unit:
                    if (form == ResultForm.Random)
                    {
                       
                    }
                    else if (form == ResultForm.Special)
                    {
                       
                    }
                    break;

                case ResultType.Special:
                    resultLog += $"- 특별한 결과 처리 필요 (choiceId: {choiceData.choiceId})\n";
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


}
