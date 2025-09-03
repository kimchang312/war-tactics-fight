using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
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

        int idx = RogueLikeData.Instance.GetRandomInt(0, candidates.Count);
        return candidates[idx];
    }

    public static bool CanAppear(EventData eventData)
    {
        int currentChapter = RogueLikeData.Instance.GetChapter();

        if (!eventData.eventChapter.Contains(currentChapter))
            return false;

        // 조건이 전부 None이면 바로 등장 가능
        bool hasCondition = false;
        for (int i = 0; i < eventData.requireThing.Count; i++)
        {
            if (eventData.requireThing[i] != RequireThing.None)
            {
                hasCondition = true;
                break;
            }
        }
        if (!hasCondition) return true;

        // 조건 검사
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


    //해당 이벤트가 실행할때 필요한 자원이 있는지 채크
    private static bool CheckRequireCondition(RequireThing thing, RequireForm form, string value, string count)
    {
        switch (thing)
        {
            case RequireThing.Gold:
                if (form == RequireForm.None)
                {
                    int gold = RogueLikeData.Instance.GetCurrentGold();
                    if (RogueLikeData.Instance.GetOwnedRelicById(49) != null) gold += 500;
                    return InRange(gold, count);
                }
                break;

            case RequireThing.Morale:
                {
                    int morale = RogueLikeData.Instance.GetMorale();
                    float reduceMorale = 1f;
                    if (RogueLikeData.Instance.GetOwnedRelicById(33) == null) reduceMorale += 0.2f;
                    return InRange(morale, count, true, reduceMorale);
                }

            case RequireThing.Unit:
                {
                    int requireCount = SafeParseInt(count);
                    var myUnits = RogueLikeData.Instance.GetMyTeam();

                    if (form == RequireForm.Select || form == RequireForm.Random)
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            if (value.Contains("~"))
                            {
                                var (min, max) = ParseRange(value);
                                return myUnits.Count(u => u.rarity >= min && u.rarity <= max) >= requireCount;
                            }
                            else
                            {
                                int rarity = SafeParseInt(value);
                                return myUnits.Count(u => u.rarity <= rarity) >= requireCount;
                            }
                        }
                        return myUnits.Count >= requireCount;
                    }
                    else if (form == RequireForm.Special)
                    {
                        return myUnits.Count >= requireCount;
                    }
                    break;
                }
            case RequireThing.Relic:
                if (form == RequireForm.Random)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        int grade = SafeParseInt(value);
                        int requireCount = SafeParseInt(count);
                        return RogueLikeData.Instance.GetAllOwnedRelics().Count(r => r.grade == grade) >= requireCount;
                    }
                    int relicGrade = SafeParseInt(count);
                    return RogueLikeData.Instance.GetAllOwnedRelics().Exists(r => r.grade == relicGrade);
                }
                else if (form == RequireForm.None)
                {
                    int relicId = SafeParseInt(count);
                    return !RelicManager.CheckRelicById(relicId);
                }
                break;

            case RequireThing.Energy:
                {
                    var myUnits = RogueLikeData.Instance.GetMyTeam();
                    int energyValue = SafeParseInt(value);
                    int requireCount = SafeParseInt(count);

                    if (form == RequireForm.Random || form == RequireForm.Select)
                    {
                        return myUnits.Count(u => u.energy > energyValue) >= requireCount;
                    }
                    break;
                }
            case RequireThing.AttackDamage:
                {
                    int threshold = SafeParseInt(count);
                    return RogueLikeData.Instance.GetMyTeam().Any(u => u.attackDamage >= threshold);
                }

            case RequireThing.Stage:
                if (form == RequireForm.None)
                {
                    return InRange(RogueLikeData.Instance.GetCurrentStageX(), count);
                }
                break;

            case RequireThing.Special:
                // Special 조건은 별도 함수로 검사
                return CheckSpecialRequire(null); // eventData 전달 가능하면 전달

        }

        return false;
    }
    
    private static int SafeParseInt(string str)
    {
        if (int.TryParse(str, out var result)) return result;
        return 0;
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
                        List<RogueUnitDataBase> selectedUnits = RogueLikeData.Instance.GetSelectedUnits();
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
                        List<RogueUnitDataBase> myUnits = RogueLikeData.Instance.GetMyTeam();
                        int energy = int.Parse(value);
                        int unitCount = int.Parse(count);

                        // energy 보다 높은 에너지를 가진 유닛 필터링
                        List<RogueUnitDataBase> filteredUnits = myUnits.FindAll(unit => unit.energy > energy);

                        // 랜덤 셔플을 위해 리스트 섞기
                        System.Random random = RogueLikeData.Instance.GetRandomBySeed();
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
                        List<RogueUnitDataBase> myUnits = RogueLikeData.Instance.GetMyTeam();
                        List<RogueUnitDataBase> selectedUnits = RogueLikeData.Instance.GetSelectedUnits();
                        foreach (var unit in myUnits)
                        {
                            Debug.Log(unit.unitName + unit.UniqueId + selectedUnits[0].unitName + selectedUnits[0].UniqueId);
                        }
                        myUnits.RemoveAll(unit => selectedUnits.Any(selectedUnit => selectedUnit.UniqueId == unit.UniqueId));
                        Debug.Log(myUnits.Count);
                        RogueLikeData.Instance.SetMyTeam(myUnits);
                        foreach(var unit in selectedUnits)
                        {
                            requireLog += $"{unit.unitName}이(가) 선택되었습니다.\n";
                        }
                    }
                    else if (form == RequireForm.Random)
                    {
                        var myUnits = RogueLikeData.Instance.GetMyTeam();
                        int unitCount = int.Parse(count);

                        int minRarity, maxRarity;

                        if (value.Contains("~"))
                        {
                            var split = value.Split('~');
                            minRarity = int.Parse(split[0]);
                            maxRarity = int.Parse(split[1]);
                        }
                        else
                        {
                            minRarity = 1;
                            maxRarity = int.Parse(value);
                        }

                        List<RogueUnitDataBase> candidates = myUnits
                            .Where(unit => unit.rarity >= minRarity && unit.rarity <= maxRarity)
                            .OrderBy(_ => RogueLikeData.Instance.GetRandomFloat())
                            .Take(unitCount)
                            .ToList();

                        List<RogueUnitDataBase> currentUnits = RogueLikeData.Instance.GetMyTeam();
                        currentUnits.RemoveAll(unit => candidates.Any(candidate => candidate.UniqueId == unit.UniqueId));
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
                        List<RogueUnitDataBase> myUnits = RogueLikeData.Instance.GetMyTeam();
                        int rarity = int.Parse(value);
                        var candidates = myUnits.Where(unit=>unit.rarity==rarity);
                        myUnits.RemoveAll(unit=> candidates.Any(candidate => candidate.UniqueId == unit.UniqueId));
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
                        requireLog = "모든 금화를 잃었습니다.\n";
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
                        moraleCount = RogueLikeData.Instance.ChangeMorale(-moraleCount);
                        requireLog += $"사기가 {-moraleCount}만큼 감소했습니다.\n";
                    }
                    break;
            }
        }
    }
    //선택시 보상 획득
    public static (string, bool) ApplyChoiceResult(EventChoiceData choiceData, List<RogueUnitDataBase> selectedUnits)
    {
        string resultLog = "";
        bool isBattle = false;
        int battleGrade = 0;

        // 결과 템플릿 치환용 토큰 버퍼
        List<string> resultTokens = new List<string>(4);

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
                        if (isBattle) RogueLikeData.Instance.AddGoldReward(gold);
                        else { gold = RogueLikeData.Instance.AddGoldByEventChapter(gold); resultLog += $"- 금화 {gold} 획득\n"; }
                        // 필요 시 토큰으로 “금화 xxx”를 템플릿에 쓰고 싶으면 아래 라인 유지
                        // PushResultToken(resultTokens, $"금화 {gold}");
                        break;
                    }

                case ResultType.Morale:
                    {
                        int morale = int.Parse(count);
                        if (isBattle) RogueLikeData.Instance.AddMoraleReward(morale);
                        else { morale = RogueLikeData.Instance.ChangeMorale(morale); resultLog += $"- 사기 {morale} 회복\n"; }
                        // PushResultToken(resultTokens, $"사기 {morale}");
                        break;
                    }

                case ResultType.Energy:
                    {
                        int energy = int.Parse(value);
                        if (form == ResultForm.Select)
                        {
                            foreach (var unit in selectedUnits)
                            {
                                unit.energy = Math.Min(unit.maxEnergy, unit.energy + energy);
                                resultLog += $"-기력 회복 {unit.unitName}\n";
                                // 템플릿에 쓸 일이 거의 없지만 필요하면 토큰 추가 가능
                            }
                        }
                        else if (form == ResultForm.All)
                        {
                            var myUnits = RogueLikeData.Instance.GetMyTeam();
                            foreach (var unit in myUnits) unit.energy = unit.maxEnergy;
                            resultLog += "모든 유닛의 기력이 회복 되었습니다.";
                        }
                        break;
                    }

                case ResultType.Relic:
                    {
                        int grade = int.Parse(value);
                        int relicCount = int.Parse(count);

                        if (form == ResultForm.Random)
                        {
                            if (isBattle)
                            {
                                for (int k = 0; k < relicCount; k++)
                                    RogueLikeData.Instance.AddRelicReward(RelicManager.GetRandomRelicId(grade, RelicAction.Acquire));
                            }
                            else
                            {
                                for (int k = 0; k < relicCount; k++)
                                {
                                    var relic = RelicManager.HandleRandomRelic(grade, RelicAction.Acquire);
                                    resultLog += $"- 전쟁 유산 획득: {relic.name}\n";
                                    PushResultToken(resultTokens, relic.name);
                                }
                            }
                        }
                        else if (form == ResultForm.Special)
                        {
                            if (choiceData.choiceId == 59 && RogueLikeData.Instance.GetRandomFloat() < 0.5f)
                            {
                                var relic = RelicManager.HandleRandomRelic(0, RelicAction.Acquire);
                                resultLog += $"- 전쟁 유산 획득: {relic.name}\n";
                                PushResultToken(resultTokens, relic.name);
                            }
                            else if (choiceData.choiceId == 73)
                            {
                                if (IsUnitVictory(selectedUnits[0]))
                                {
                                    var relic = RelicManager.HandleRandomRelic(10, RelicAction.Acquire);
                                    resultLog += $"- 전쟁 유산 획득: {relic.name}\n";
                                    PushResultToken(resultTokens, relic.name);
                                }
                                else
                                {
                                    resultLog += $"- 경기에 패배해 아무것도 얻지 못했습니다.\n";
                                }
                            }
                            else if (choiceData.choiceId == 75)
                            {
                                if (IsUnitVictoryByRarity(selectedUnits[0]))
                                {
                                    RogueLikeData.Instance.AddMyUnis(selectedUnits[0]);
                                    var relic = RelicManager.HandleRandomRelic(10, RelicAction.Acquire);
                                    resultLog += $"- 결투 승리: 전쟁유산 {relic.name} 획득\n";
                                    PushResultToken(resultTokens, relic.name);
                                }
                                else
                                {
                                    resultLog += "- 결투 패배\n";
                                }
                            }
                            else if (choiceData.choiceId == 82)
                            {
                                var relicIds = RogueLikeData.Instance.GetAllOwnedRelicIds();
                                int rewardRelicId = !relicIds.Contains(23) ? 23 : (!relicIds.Contains(24) ? 24 : 25);
                                RogueLikeData.Instance.AcquireRelic(rewardRelicId);
                                var relic = RelicManager.GetRelicById(rewardRelicId);
                                resultLog += $"- 보석 건틀릿의 마지막 유산을 획득했습니다.\n";
                                PushResultToken(resultTokens, relic?.name ?? "");
                            }
                        }
                        break;
                    }

                case ResultType.Unit:
                    {
                        if (form == ResultForm.None)
                        {
                            int unitId = int.Parse(value);
                            int unitCount = int.Parse(count);
                            for (int k = 0; k < unitCount; k++)
                            {
                                RogueUnitDataBase unit = UnitLoader.Instance.GetCloneUnitById(unitId);
                                RogueLikeData.Instance.AddMyUnis(unit);
                                resultLog += $"- {unit.unitName} 추가\n";
                                PushResultToken(resultTokens, unit.unitName);
                            }
                        }
                        else if (form == ResultForm.Random)
                        {
                            if (value.Contains("~"))
                            {
                                int unitCount = int.Parse(count);
                                var (min, max) = ParseRange(value);
                                var all = UnitLoader.Instance.GetAllCachedUnits().Where(u => u.rarity >= min && u.rarity <= max).ToList();

                                for (int k = 0; k < unitCount && all.Count > 0; k++)
                                {
                                    int ri = RogueLikeData.Instance.GetRandomInt(0, all.Count);
                                    var pick = all[ri]; all.RemoveAt(ri);
                                    RogueUnitDataBase newUnit = UnitLoader.Instance.GetCloneUnitById(pick.idx);

                                    if (isBattle) RogueLikeData.Instance.AddUnitReward(newUnit);
                                    else { RogueLikeData.Instance.AddMyUnis(newUnit); resultLog += $"- {newUnit.unitName} 추가\n"; }
                                    PushResultToken(resultTokens, newUnit.unitName);
                                }
                            }
                            else
                            {
                                int unitRarity = int.Parse(value);
                                int unitCount = int.Parse(count);
                                var all = UnitLoader.Instance.GetAllCachedUnits();

                                List<RogueUnitDataBase> valid;
                                if (unitRarity == 4)
                                {
                                    var myIdx = System.Linq.Enumerable.ToHashSet(RogueLikeData.Instance.GetMyTeam().Select(u => u.idx));
                                    valid = all.Where(u => u.rarity == unitRarity && !myIdx.Contains(u.idx)).ToList();
                                    if (valid.Count == 0)
                                    {
                                        WarRelic relic = RelicManager.HandleRandomRelic(10, RelicAction.Acquire);
                                        resultLog += $"모든 영웅유닛 보유. 전쟁유산 {relic.name} 획득\n";
                                        PushResultToken(resultTokens, relic.name);
                                        break;
                                    }
                                }
                                else valid = all.Where(u => u.rarity == unitRarity).ToList();

                                for (int k = 0; k < unitCount && valid.Count > 0; k++)
                                {
                                    int ri = RogueLikeData.Instance.GetRandomInt(0, valid.Count);
                                    var pick = valid[ri]; valid.RemoveAt(ri);
                                    RogueUnitDataBase newUnit = UnitLoader.Instance.GetCloneUnitById(pick.idx);

                                    if (isBattle) RogueLikeData.Instance.AddUnitReward(newUnit);
                                    else { RogueLikeData.Instance.AddMyUnis(newUnit); resultLog += $"- {newUnit.unitName} 추가\n"; }
                                    PushResultToken(resultTokens, newUnit.unitName);
                                }
                            }
                        }
                        else if (form == ResultForm.Select)
                        {
                            var origin = selectedUnits[0];
                            RogueUnitDataBase clone = UnitLoader.Instance.GetCloneUnitById(origin.idx);
                            clone.energy = origin.energy;
                            RogueLikeData.Instance.AddMyUnis(clone);
                            resultLog += $"- {clone.unitName} 추가\n";
                            PushResultToken(resultTokens, clone.unitName);
                        }
                        else if (form == ResultForm.Special)
                        {
                            var origin = selectedUnits[0];
                            float chance = origin.rarity switch { 1 => 0.3f, 2 => 0.6f, 3 => 1.0f, _ => 0f };
                            if (RogueLikeData.Instance.GetRandomFloat() < chance)
                            {
                                var myIdx = new HashSet<int>(RogueLikeData.Instance.GetMyTeam().Select(u => u.idx));
                                var valid = UnitLoader.Instance.GetAllCachedUnits().Where(u => u.rarity == 4 && !myIdx.Contains(u.idx)).ToList();
                                if (valid.Count == 0) { resultLog += "모든 영웅 유닛 보유\n"; break; }

                                int ri = RogueLikeData.Instance.GetRandomInt(0, valid.Count);
                                var pick = valid[ri];
                                var newUnit = UnitLoader.Instance.GetCloneUnitById(pick.idx);
                                RogueLikeData.Instance.AddMyUnis(newUnit);
                                resultLog += $"'{origin.unitName}' 희생 → '{newUnit.unitName}' 획득\n";
                                PushResultToken(resultTokens, newUnit.unitName);
                            }
                            else
                            {
                                resultLog += $"'{origin.unitName}' 희생했지만 변화 없음\n";
                            }
                        }
                        break;
                    }

                case ResultType.Change:
                    {
                        int changeCount = int.Parse(count);
                        if (form == ResultForm.Select)
                        {
                            if (isBattle)
                            {
                                foreach (var unit in selectedUnits)
                                {
                                    RogueUnitDataBase newUnit = unit;
                                    for (int k = 0; k < changeCount; k++) newUnit = RogueUnitDataBase.RandomUnitReForm(newUnit);
                                    RogueLikeData.Instance.AddChangeReward(newUnit);
                                    PushResultToken(resultTokens, newUnit.unitName);
                                }
                            }
                            else
                            {
                                foreach (var unit in selectedUnits)
                                {
                                    RogueUnitDataBase newUnit = unit;
                                    for (int k = 0; k < changeCount; k++) newUnit = RogueUnitDataBase.RandomUnitReForm(newUnit);
                                    RogueLikeData.Instance.AddMyUnis(newUnit);
                                    resultLog += $"- {unit.unitName} → {newUnit.unitName}\n";
                                    PushResultToken(resultTokens, newUnit.unitName);
                                }
                            }
                        }
                        else if (form == ResultForm.Random)
                        {
                            int unitCount = int.Parse(count);
                            var myUnits = RogueLikeData.Instance.GetMyTeam().Where(u => u.rarity < 4).OrderBy(_ => UnityEngine.Random.value).Take(unitCount).ToList();
                            foreach (var unit in myUnits)
                            {
                                var newUnit = RogueUnitDataBase.RandomUnitReForm(unit);
                                RogueLikeData.Instance.AddMyUnis(newUnit);
                                resultLog += $"- {unit.unitName} → {newUnit.unitName}\n";
                                PushResultToken(resultTokens, newUnit.unitName);
                            }
                        }
                        break;
                    }

                case ResultType.Special:
                    {
                        if (choiceData.choiceId == 3)
                        {
                            int randEffect = RogueLikeData.Instance.GetRandomInt(0, 9);
                            // 아래 기존 로직 유지, 획득/변경 시 토큰만 추가
                            switch (randEffect)
                            {
                                case 0: { var r = RelicManager.HandleRandomRelic(1, RelicManager.RelicAction.Acquire); resultLog += $"- 일반 유산 '{r?.name}'\n"; PushResultToken(resultTokens, r?.name ?? ""); break; }
                                case 1: { var r = RelicManager.HandleRandomRelic(0, RelicManager.RelicAction.Acquire); resultLog += $"- 저주 유산 '{r?.name}'\n"; PushResultToken(resultTokens, r?.name ?? ""); break; }
                                case 2:
                                    {
                                        var my = RogueLikeData.Instance.GetMyTeam();
                                        if (my.Count > 0)
                                        {
                                            var unit = my[RogueLikeData.Instance.GetRandomInt(0, my.Count)];
                                            var promoted = RogueUnitDataBase.RandomUnitReForm(unit);
                                            if (promoted != null)
                                            {
                                                my[my.IndexOf(unit)] = promoted;
                                                RogueLikeData.Instance.SetMyTeam(my);
                                                resultLog += $"- '{unit.unitName}' → '{promoted.unitName}'\n";
                                                PushResultToken(resultTokens, promoted.unitName);
                                            }
                                        }
                                        break;
                                    }
                                case 3:
                                    {
                                        var my = RogueLikeData.Instance.GetMyTeam().Where(u => u.energy > 1).ToList();
                                        if (my.Count > 0)
                                        {
                                            var target = my[RogueLikeData.Instance.GetRandomInt(0, my.Count)];
                                            target.energy = 1;
                                            resultLog += $"- '{target.unitName}' 기력 1\n";
                                            PushResultToken(resultTokens, target.unitName);
                                        }
                                        break;
                                    }
                                case 4:
                                    {
                                        var (min, max) = ParseRange("1~3");
                                        var all = UnitLoader.Instance.GetAllCachedUnits();
                                        var myIdx = System.Linq.Enumerable.ToHashSet(RogueLikeData.Instance.GetMyTeam().Select(u => u.idx));
                                        var cands = all.Where(u => u.rarity >= min && u.rarity <= max && !myIdx.Contains(u.idx)).ToList();
                                        if (cands.Count > 0)
                                        {
                                            var sel = cands[RogueLikeData.Instance.GetRandomInt(0, cands.Count)];
                                            RogueLikeData.Instance.AddMyUnis(sel);
                                            resultLog += $"- 유닛 '{sel.unitName}' 획득\n";
                                            PushResultToken(resultTokens, sel.unitName);
                                        }
                                        break;
                                    }
                                case 5: RogueLikeData.Instance.IncreaseRandomUpgrade(false); resultLog += "- 무작위 병종 강화\n"; break;
                                case 6: RogueLikeData.Instance.SetMorale(100); resultLog += "- 사기 최대\n"; break;
                                case 7: { int m = RogueLikeData.Instance.ChangeMorale(25); resultLog += $" 사기 {-m} 감소\n"; break; }
                                case 8: { int g = RogueLikeData.Instance.AddGoldByEventChapter(150); resultLog += $"- 금화 {g} 획득\n"; PushResultToken(resultTokens, $"금화 {g}"); break; }
                            }
                        }
                        else if (choiceData.choiceId == 25)
                        {
                            var unit = selectedUnits[0];
                            int rarity = unit.rarity;
                            int getGold = (rarity == 1) ? RogueLikeData.Instance.AddGoldByEventChapter(50)
                                       : RogueLikeData.Instance.AddGoldByEventChapter(150);
                            resultLog += $"'{unit.unitName}' 희생 → 금화 {getGold}\n";
                            PushResultToken(resultTokens, $"금화 {getGold}");
                            if (rarity == 3)
                            {
                                var r = RelicManager.HandleRandomRelic(1, RelicAction.Acquire);
                                resultLog += $"+ 전쟁 유산 '{r?.name}'\n";
                                PushResultToken(resultTokens, r?.name ?? "");
                            }
                        }
                        else if (choiceData.choiceId == 37)
                        {
                            if (UnityEngine.Random.value < 0.5f)
                            {
                                WarRelic r = RelicManager.HandleRandomRelic(5, RelicAction.Acquire);
                                resultLog += $"'{r.name}' 획득\n";
                                PushResultToken(resultTokens, r.name);
                            }
                            else
                            {
                                battleGrade = 5;
                                isBattle = true;
                            }
                        }
                        break;
                    }

                case ResultType.None:
                    resultLog += "- 아무 일도 일어나지 않았다\n";
                    break;

                case ResultType.Battle:
                    {
                        battleGrade = int.Parse(value);
                        if (form == ResultForm.Special)
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
                            int upCnt = (k < choiceData.resultCount.Count && int.TryParse(choiceData.resultCount[k], out var c)) ? c : 1;

                            for (int j = 0; j < upCnt; j++)
                            {
                                if (useRandom) RogueLikeData.Instance.IncreaseRandomUpgrade(false);
                                else
                                {
                                    var parts = valueStr.Split(',');
                                    var unitTypes = parts.Select(s => int.TryParse(s, out var v) ? v : -1).Where(v => v >= 0 && v < 8).ToList();
                                    if (unitTypes.Count == 0) continue;
                                    int randomType = unitTypes[RogueLikeData.Instance.GetRandomInt(0, unitTypes.Count)];
                                    bool isAttack = UnityEngine.Random.value < 0.5f;
                                    RogueLikeData.Instance.IncreaseUpgrade(randomType, isAttack, false);
                                }
                            }
                        }
                        resultLog += "랜덤 병종 강화 적용\n";
                        break;
                    }

                case ResultType.Field:
                    {
                        int fieldId = int.Parse(value);
                        RogueLikeData.Instance.SetFieldId(fieldId);
                        resultLog += "다음 전장이 변경되었습니다.\n";
                        // PushResultToken(resultTokens, FieldName(fieldId)); // 필드명이 필요하면 추가
                        break;
                    }

                default:
                    resultLog += "- 알 수 없는 보상\n";
                    break;
            } // switch
        } // for

        if (choiceData.resultText != null && choiceData.resultText.Count > 0)
        {
            // 선택 유닛 리스트가 null로 들어오면 RogueLikeData의 선택 목록 사용
            var requires = selectedUnits ?? RogueLikeData.Instance.GetSelectedUnits();
            // JSON 내러티브를 최우선으로 사용
            resultLog = ComposeResultNarration(choiceData, requires, resultTokens);
        }

        if (isBattle)
        {
            SetPresetIdByGrade(battleGrade);
            GameManager.Instance.OpenBattlePanel();
        }
        return (resultLog, isBattle);
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
        StagePreset stage =  filtered[RogueLikeData.Instance.GetRandomInt(0, filtered.Count)];
        RogueLikeData.Instance.SetPresetID(stage.PresetID);
    }

    // 이 함수는 선택지 결과 문장을 만든다.
    // 사용처: ApplyChoiceResult 마지막에서 JSON의 resultText를 기반으로 내러티브 문자열을 조립할 때 사용
    private static string ComposeResultNarration(EventChoiceData choiceData, List<RogueUnitDataBase> selectedUnits, List<string> resultTokens)
    {
        if (choiceData.resultText == null || choiceData.resultText.Count == 0)
            return "";

        // 선택 유닛(Require)의 이름 목록 준비
        var requireNames = new List<string>();
        if (selectedUnits != null)
        {
            for (int i = 0; i < selectedUnits.Count; i++)
                requireNames.Add(selectedUnits[i]?.unitName ?? "");
        }

        // 빠른 치환
        System.Text.StringBuilder sb = new System.Text.StringBuilder(256);
        foreach (var line in choiceData.resultText)
        {
            if (string.IsNullOrEmpty(line)) { sb.AppendLine(); continue; }

            string s = line;

            // {require[i]} 치환
            for (int i = 0; i < requireNames.Count; i++)
                s = s.Replace($"{{require[{i}]}}", requireNames[i]);

            // {result[i]} 치환
            for (int i = 0; i < resultTokens.Count; i++)
                s = s.Replace($"{{result[{i}]}}", resultTokens[i]);

            sb.AppendLine(s);
        }
        return sb.ToString();
    }

    // 결과 토큰을 추가한다.
    // 사용처: ApplyChoiceResult 스위치 안에서 "플레이어가 실제로 얻은 것"의 표시 문자열을 순서대로 넣는다.
    private static void PushResultToken(List<string> tokens, string value)
    {
        if (!string.IsNullOrEmpty(value)) tokens.Add(value);
    }

}
