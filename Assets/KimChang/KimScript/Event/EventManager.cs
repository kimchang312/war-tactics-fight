using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static RelicManager;


public class EventManager
{
    private static readonly List<string> lastRequireTokens = new List<string>(4);

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

        if (candidates.Count == 0)
        {
            Debug.LogWarning("등장 가능한 이벤트가 없습니다.");
            return null;
        }

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
                if (!CheckRequireCondition(thing, form, value, count, false))
                    return false;
            }
        }
        return true;
    }


    // 사용처: 사기 소모 조건 표시/선택 가능 여부 검사 시 실제 사기 소모 배율 계산
    private static float GetMoraleCostMultiplier()
    {
        float multiplier = 1f;

        if (RelicManager.CheckRelicById(33))
        {
            WarRelic relic = RelicManager.GetRelicById(33);
            var vals = relic?.GetAllValuesAsFloatListOrNull();

            if (vals != null && vals.Count > 0)
                multiplier += vals[0];
            else
                multiplier += 0.2f;
        }

        return multiplier;
    }

    //해당 이벤트가 실행할때 필요한 자원이 있는지 채크
    private static bool CheckRequireCondition(RequireThing thing, RequireForm form, string value, string count, bool applyMoraleCostMultiplier = true)
    {
        switch (thing)
        {
            case RequireThing.Gold:
                if (form == RequireForm.None)
                {
                    int gold = RogueLikeData.Instance.GetCurrentGold();
                    return InRange(gold, count);
                }
                break;

            case RequireThing.Morale:
                {
                    int morale = RogueLikeData.Instance.GetMorale();
                    float reduceMorale = applyMoraleCostMultiplier ? GetMoraleCostMultiplier() : 1f;
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
                        return myUnits.Count(u => u.Energy > energyValue) >= requireCount;
                    }
                    break;
                }
            case RequireThing.AttackDamage:
                {
                    int threshold = SafeParseInt(value);
                    int requireCount = SafeParseInt(count);
                    if (requireCount <= 0) requireCount = 1;

                    return RogueLikeData.Instance.GetMyTeam()
                        .Count(u => u.attackDamage >= threshold) >= requireCount;
                }
            case RequireThing.Stage:
                if (form == RequireForm.None)
                {
                    return InRange(RogueLikeData.Instance.GetCurrentStageX(), count);
                }
                break;

            case RequireThing.Special:
                return CheckSpecialRequire(null);
        }

        return false;
    }

    private static int SafeParseInt(string str)
    {
        if (int.TryParse(str, out var result)) return result;
        return 0;
    }

    private static bool ShouldConsumeSelectedUnits(EventChoiceData choiceData)
    {
        if (choiceData == null)
            return true;

        bool hasBattleResult = false;
        bool hasChangeSelectResult = false;
        bool hasUnitSelectResult = false;

        int resultCount = Mathf.Min(choiceData.resultType.Count, choiceData.resultForm.Count);
        for (int i = 0; i < resultCount; i++)
        {
            if (choiceData.resultType[i] == ResultType.Battle)
                hasBattleResult = true;
            if (choiceData.resultType[i] == ResultType.Change && choiceData.resultForm[i] == ResultForm.Select)
                hasChangeSelectResult = true;
            if (choiceData.resultType[i] == ResultType.Unit && choiceData.resultForm[i] == ResultForm.Select)
                hasUnitSelectResult = true;
        }

        if (hasUnitSelectResult)
            return false;

        if (hasChangeSelectResult && !hasBattleResult)
            return true;

        switch (choiceData.choiceId)
        {
            case 25:   // 유랑하는 상인 - 병사를 보내고 대가를 받음
            case 75:   // 검은 기사 결투 - 패배 시 사망
            case 86:   // 한명의 영웅 - 일반 유닛을 영웅으로 변경
            case 95:   // 금지된 의식 - 성공/실패와 무관하게 선택 유닛 소모
                return true;
        }

        return false;
    }

    private static bool CheckSpecialRequire(EventData eventData)
    {
        if (eventData == null)
            return false;

        int gold = RogueLikeData.Instance.GetCurrentGold();
        int morale = RogueLikeData.Instance.GetMorale();
        if (eventData.eventId == 5)
        {
            if (gold >= 100 || morale >= 31) return true;
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
            if (gold >= 100 || morale >= 6) return true;
        }
        return false;
    }

    public static bool CheckChoiceRequireCondition(EventChoiceData eventChoiceData)
    {
        bool hasRequire = false;
        for (int i = 0; i < eventChoiceData.requireThing.Count; i++)
        {
            if (eventChoiceData.requireThing[i] != RequireThing.None)
            {
                hasRequire = true;
                break;
            }
        }

        if (!hasRequire)
            return true;

        for (int i = 0; i < eventChoiceData.requireThing.Count; i++)
        {
            var thing = eventChoiceData.requireThing[i];
            var form = eventChoiceData.requireForm[i];
            var value = eventChoiceData.requireValue[i];
            var count = eventChoiceData.requireCount[i];

            if (thing == RequireThing.None)
                continue;

            if (thing == RequireThing.Special || form == RequireForm.Special)
            {
                if (!CheckSpecialChoiceRequire(eventChoiceData))
                    return false;
            }
            else
            {
                if (!CheckRequireCondition(thing, form, value, count, true))
                    return false;
            }
        }

        return true;
    }

    private static bool CheckSpecialChoiceRequire(EventChoiceData eventChoiceData)
    {
        if (eventChoiceData.choiceId == 82)
        {
            List<int> warRelicIds = RogueLikeData.Instance.GetAllOwnedRelicIds();

            int count = 0;
            if (warRelicIds.Contains(23)) count++;
            if (warRelicIds.Contains(24)) count++;
            if (warRelicIds.Contains(25)) count++;

            return count == 2 && RogueLikeData.Instance.GetMyTeam().Count > 0;
        }

        return false;
    }
    //보상 받기전 요구값 감소
    public static void ReduceRequire(EventChoiceData choiceData)
    {
        lastRequireTokens.Clear();
        string requireLog = "";
        for (int i = 0; i < choiceData.requireThing.Count; i++)
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
                    if (form == RequireForm.Select)
                    {
                        List<RogueUnitDataBase> selectedUnits = RogueLikeData.Instance.GetSelectedUnits();
                        foreach (var unit in selectedUnits)
                        {
                            if (string.IsNullOrEmpty(value))
                            {
                                requireLog += $"{unit.unitName}이(가) 선택 되었습니다.";
                                PushRequireToken(unit.unitName);
                            }
                            else if (value == "-1")
                            {
                                unit.Energy = Mathf.Max(0, unit.Energy - 1);
                                requireLog += $"{unit.unitName}이(가) 선택 되었습니다.";
                                PushRequireToken(unit.unitName);
                            }
                            else
                            {
                                // 양수 값은 선택 조건으로만 사용한다. 여기서 기력을 강제로 변경하지 않는다.
                                requireLog += $"{unit.unitName}이(가) 선택 되었습니다.";
                                PushRequireToken(unit.unitName);
                            }
                        }
                    }
                    else if (form == RequireForm.Random)
                    {
                        List<RogueUnitDataBase> myUnits = RogueLikeData.Instance.GetMyTeam();
                        int energy = int.Parse(value);
                        int unitCount = int.Parse(count);

                        List<RogueUnitDataBase> filteredUnits = myUnits.FindAll(unit => unit.Energy > energy);

                        System.Random random = RogueLikeData.Instance.GetRandomBySeed();
                        for (int k = filteredUnits.Count - 1; k > 0; k--)
                        {
                            int j = random.Next(k + 1);
                            (filteredUnits[k], filteredUnits[j]) = (filteredUnits[j], filteredUnits[k]);
                        }

                        int countToSelect = Mathf.Min(unitCount, filteredUnits.Count);
                        List<RogueUnitDataBase> selectUnits = filteredUnits.GetRange(0, countToSelect);

                        foreach (var unit in selectUnits)
                        {
                            unit.Energy = energy;
                            requireLog += $"{unit.unitName}이(가) 선택되었습니다.";
                            PushRequireToken(unit.unitName);
                        }
                    }

                    break;
                case RequireThing.Unit:
                    if (form == RequireForm.Select)
                    {
                        List<RogueUnitDataBase> myUnits = RogueLikeData.Instance.GetMyTeam();
                        List<RogueUnitDataBase> selectedUnits = RogueLikeData.Instance.GetSelectedUnits();

                        if (ShouldConsumeSelectedUnits(choiceData))
                        {
                            myUnits.RemoveAll(unit => selectedUnits.Any(selectedUnit => selectedUnit.UniqueId == unit.UniqueId));
                            RogueLikeData.Instance.SetMyTeam(myUnits);
                        }

                        foreach (var unit in selectedUnits)
                        {
                            requireLog += $"{unit.unitName}이(가) 선택되었습니다.";
                            PushRequireToken(unit.unitName);
                        }
                    }
                    else if (form == RequireForm.Random)
                    {
                        var myUnits = RogueLikeData.Instance.GetMyTeam();
                        int unitCount = int.Parse(count);

                        int minRarity, maxRarity;

                        if (string.IsNullOrEmpty(value))
                        {
                            minRarity = int.MinValue;
                            maxRarity = int.MaxValue;
                        }
                        else if (value.Contains("~"))
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
                            requireLog += $"{unit.unitName}이(가) 선택되었습니다.";
                            PushRequireToken(unit.unitName);
                        }
                    }
                    else if (form == RequireForm.Special)
                    {
                        if (choiceData.choiceId == 82)
                        {
                            List<RogueUnitDataBase> myTeam = RogueLikeData.Instance.GetMyTeam();
                            RogueUnitDataBase sacrificeUnit = myTeam
                                .OrderByDescending(unit => unit.unitPrice)
                                .ThenByDescending(unit => unit.Energy)
                                .FirstOrDefault();

                            if (sacrificeUnit != null)
                            {
                                myTeam.RemoveAll(unit => unit.UniqueId == sacrificeUnit.UniqueId);
                                RogueLikeData.Instance.SetMyTeam(myTeam);
                                RogueLikeData.Instance.AddSelectedUnits(sacrificeUnit);
                                requireLog += $"{sacrificeUnit.unitName}이 선택되었습니다.";
                                PushRequireToken(sacrificeUnit.unitName);
                            }
                        }
                    }
                    else if (form == RequireForm.None)
                    {
                        List<RogueUnitDataBase> myUnits = RogueLikeData.Instance.GetMyTeam();
                        int rarity = int.Parse(value);
                        var candidates = myUnits.Where(unit => unit.rarity == rarity);
                        myUnits.RemoveAll(unit => candidates.Any(candidate => candidate.UniqueId == unit.UniqueId));
                        foreach (var unit in candidates)
                        {
                            RogueLikeData.Instance.AddSelectedUnits(unit);
                            requireLog += $"{unit.unitName}이(가) 선택되었습니다.";
                            PushRequireToken(unit.unitName);
                        }
                    }

                    break;
                case RequireThing.Relic:
                    if (form == RequireForm.Random)
                    {
                        int grade = int.Parse(value);
                        int relicCount = int.Parse(count);
                        for (int k = 0; k < relicCount; k++)
                        {
                            var relic = RelicManager.HandleRandomRelic(grade, RelicAction.Remove);
                            if (relic != null)
                            {
                                string relicName = GameTextDB.GetByForeignKey(TextKind.RelicName, relic.id);
                                if (string.IsNullOrEmpty(relicName)) relicName = relic.name;
                                requireLog += $"{relicName}이(가) 제거되었습니다.";
                                PushRequireToken(relicName);
                            }
                        }
                    }

                    break;
                case RequireThing.Gold:
                    if (choiceData.choiceId == 27)
                    {
                        RogueLikeData.Instance.EarnGold(-RogueLikeData.Instance.GetCurrentGold());
                        requireLog = "모든 금화를 잃었습니다.";
                    }
                    else if (form == RequireForm.None)
                    {
                        int goldCount = int.Parse(count);
                        RogueLikeData.Instance.ReduceGold(goldCount);
                        requireLog += $"{goldCount}금화를 지불하였습니다.";
                    }
                    break;
                case RequireThing.Morale:
                    if (form == RequireForm.None)
                    {
                        int moraleCount = int.Parse(count);
                        moraleCount = RogueLikeData.Instance.ChangeMorale(-moraleCount);
                        requireLog += $"사기가 {-moraleCount}만큼 감소했습니다.";
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
        int resultTextIndex = 0;

        //유산 85
        float rarity1Rate = 0f;
        WarRelic relic85 = RelicManager.GetRelicById(85);
        var vals = relic85?.GetAllValuesAsFloatListOrNull();
        if (vals != null)
        {
            rarity1Rate += Mathf.Clamp01(Mathf.Abs(vals[0]));
        }

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
                        if (form == ResultForm.Random)
                        {
                            int probability = string.IsNullOrEmpty(value) ? 100 : int.Parse(value);
                            bool success = RogueLikeData.Instance.GetRandomFloat() < probability * 0.01f;
                            resultTextIndex = success ? 0 : 1;

                            if (!success)
                            {
                                resultLog += "- 아무것도 얻지 못했습니다.\n";
                                break;
                            }
                        }

                        int gold = SafeParseInt(count);
                        if (isBattle) RogueLikeData.Instance.AddGoldReward(gold);
                        else { gold = RogueLikeData.Instance.AddGoldByEventChapter(gold); resultLog += $"- 금화 {gold} 획득\n"; }
                        break;
                    }

                case ResultType.Morale:
                    {
                        int morale = SafeParseInt(count);
                        if (isBattle) RogueLikeData.Instance.AddMoraleReward(morale);
                        else { morale = RogueLikeData.Instance.ChangeMorale(morale); resultLog += $"- 사기 {morale} 회복\n"; }
                        break;
                    }

                case ResultType.Energy:
                    {
                        int energy = int.Parse(value);
                        if (form == ResultForm.Select)
                        {
                            foreach (var unit in selectedUnits)
                            {
                                unit.Energy = Math.Min(unit.MaxEnergy, unit.Energy + energy);
                                resultLog += $"-기력 회복 {unit.unitName}\n";
                            }
                        }
                        else if (form == ResultForm.All)
                        {
                            var myUnits = RogueLikeData.Instance.GetMyTeam();
                            foreach (var unit in myUnits) unit.Energy = unit.MaxEnergy;
                            resultLog += "모든 유닛의 기력이 회복 되었습니다.";
                        }
                        break;
                    }

                case ResultType.Relic:
                    {
                        if (form == ResultForm.Random)
                        {
                            int grade = int.Parse(value);
                            int relicCount = int.Parse(count);

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
                                    if (relic == null) continue;

                                    string name = GameTextDB.GetByForeignKey(TextKind.RelicName, relic.id);
                                    resultLog += $"- 전쟁 유산 획득: {name}\n";
                                    PushResultToken(resultTokens, name);
                                }
                            }
                        }
                        else if (form == ResultForm.Select || form == ResultForm.None)
                        {
                            int relicId = int.Parse(value);
                            if (RelicManager.AcquireRelic(relicId))
                            {
                                string name = GameTextDB.GetByForeignKey(TextKind.RelicName, relicId);
                                resultLog += $"- 전쟁 유산 획득: {name}\n";
                                PushResultToken(resultTokens, name);
                            }

                            if (choiceData.choiceId == 113)
                            {
                                var resume = RogueLikeData.Instance.GetSelectedUnits();
                                if (resume != null && resume.Count > 0)
                                {
                                    RogueUnitDataBase unit = resume[0];
                                    unit.endless = true;
                                    unit.SetEnergyDirect(1);
                                }
                            }
                        }
                        else if (form == ResultForm.Special)
                        {
                            if (choiceData.choiceId == 59)
                            {
                                bool hasCurse = RogueLikeData.Instance.GetRandomFloat() < 0.5f;
                                resultTextIndex = hasCurse ? 1 : 0;

                                if (hasCurse)
                                {
                                    var relic = RelicManager.HandleRandomRelic(0, RelicAction.Acquire);
                                    if (relic != null)
                                    {
                                        string name = GameTextDB.GetByForeignKey(TextKind.RelicName, relic.id);
                                        resultLog += $"- 전쟁 유산 획득: {name}\n";
                                        PushResultToken(resultTokens, name);
                                    }
                                }
                            }
                            else if (choiceData.choiceId == 73)
                            {
                                bool success = IsUnitVictory(selectedUnits[0]);
                                resultTextIndex = success ? 0 : 1;

                                if (success)
                                {
                                    var relic = RelicManager.HandleRandomRelic(10, RelicAction.Acquire);
                                    if (relic != null)
                                    {
                                        string name = GameTextDB.GetByForeignKey(TextKind.RelicName, relic.id);
                                        resultLog += $"- 전쟁 유산 획득: {name}\n";
                                        PushResultToken(resultTokens, name);
                                    }
                                }
                                else
                                {
                                    resultLog += $"- 경기에 패배해 아무것도 얻지 못했습니다.\n";
                                }
                            }
                            else if (choiceData.choiceId == 75)
                            {
                                bool success = IsUnitVictoryByRarity(selectedUnits[0]);
                                resultTextIndex = success ? 0 : 1;

                                if (success)
                                {
                                    RogueLikeData.Instance.AddMyTeam(selectedUnits[0]);
                                    var relic = RelicManager.HandleRandomRelic(10, RelicAction.Acquire);
                                    if (relic != null)
                                    {
                                        string name = GameTextDB.GetByForeignKey(TextKind.RelicName, relic.id);
                                        resultLog += $"- 결투 승리: 전쟁유산 {name} 획득\n";
                                        PushResultToken(resultTokens, name);
                                    }
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

                                if (RelicManager.AcquireRelic(rewardRelicId))
                                {
                                    string name = GameTextDB.GetByForeignKey(TextKind.RelicName, rewardRelicId);
                                    resultLog += $"- 보석 건틀릿의 마지막 유산을 획득했습니다.\n";
                                    PushResultToken(resultTokens, name);
                                }
                            }
                            else if (choiceData.choiceId == 132)
                            {
                                bool success = RogueLikeData.Instance.GetRandomFloat() < 0.5f;
                                resultTextIndex = success ? 0 : 1;

                                int rewardGrade = success ? 10 : 0;
                                var relic = RelicManager.HandleRandomRelic(rewardGrade, RelicAction.Acquire);
                                if (relic != null)
                                {
                                    string name = GameTextDB.GetByForeignKey(TextKind.RelicName, relic.id);
                                    resultLog += $"- 전쟁 유산 획득: {name}\n";
                                    PushResultToken(resultTokens, name);
                                }
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
                                RogueLikeData.Instance.AddMyTeam(unit);
                                string name = GameTextDB.GetByForeignKey(TextKind.Unit, unit.idx);
                                resultLog += $"- {name} 추가\n";
                                PushResultToken(resultTokens, name);
                            }
                        }
                        else if (form == ResultForm.Random)
                        {
                            if (value.Contains("~"))
                            {
                                int unitCount = int.Parse(count);
                                var (min, max) = ParseRange(value);
                                var all = UnitLoader.Instance.GetAllCachedUnits().Where(u => u.rarity >= min && u.rarity <= max).ToList();
                                RemoveCommonUnitChoicesIfNeeded(all);

                                for (int k = 0; k < unitCount && all.Count > 0; k++)
                                {
                                    int ri = (rarity1Rate > 0f)
                                        ? PickIndexWithRarity1Penalty(all, rarity1Rate)
                                        : RogueLikeData.Instance.GetRandomInt(0, all.Count);

                                    var pick = all[ri];
                                    all.RemoveAt(ri);

                                    RogueUnitDataBase newUnit = UnitLoader.Instance.GetCloneUnitById(pick.idx);
                                    if (isBattle) RogueLikeData.Instance.AddUnitReward(newUnit);
                                    else { RogueLikeData.Instance.AddMyTeam(newUnit); resultLog += $"- {newUnit.unitName} 추가\n"; }
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
                                        string name = GameTextDB.GetByForeignKey(TextKind.RelicName, relic.id);
                                        resultLog += $"모든 영웅유닛 보유. 전쟁유산 {name} 획득\n";
                                        PushResultToken(resultTokens, name);
                                        break;
                                    }
                                }
                                else valid = all.Where(u => u.rarity == unitRarity).ToList();
                                RemoveCommonUnitChoicesIfNeeded(valid);

                                for (int k = 0; k < unitCount && valid.Count > 0; k++)
                                {
                                    int ri = RogueLikeData.Instance.GetRandomInt(0, valid.Count);
                                    var pick = valid[ri]; valid.RemoveAt(ri);
                                    RogueUnitDataBase newUnit = UnitLoader.Instance.GetCloneUnitById(pick.idx);
                                    string name = GameTextDB.GetByForeignKey(TextKind.Unit, newUnit.idx);

                                    if (isBattle) RogueLikeData.Instance.AddUnitReward(newUnit);
                                    else { RogueLikeData.Instance.AddMyTeam(newUnit); resultLog += $"- {newUnit.unitName} 추가\n"; }
                                    PushResultToken(resultTokens, name);
                                }
                            }
                        }
                        else if (form == ResultForm.Select)
                        {
                            var origin = selectedUnits[0];
                            RogueUnitDataBase clone = UnitLoader.Instance.GetCloneUnitById(origin.idx);
                            clone.SetEnergyDirect(origin.Energy);
                            RogueLikeData.Instance.AddMyTeam(clone);
                            string name = GameTextDB.GetByForeignKey(TextKind.Unit, clone.idx);
                            resultLog += $"- {name} 추가\n";
                            PushResultToken(resultTokens, name);
                        }
                        else if (form == ResultForm.Special)
                        {
                            var origin = selectedUnits[0];
                            float chance = origin.rarity switch { 1 => 0.3f, 2 => 0.6f, 3 => 1.0f, _ => 0f };
                            bool success = RogueLikeData.Instance.GetRandomFloat() < chance;
                            if (choiceData.choiceId == 95)
                                resultTextIndex = success ? 0 : 1;

                            if (success)
                            {
                                var myIdx = new HashSet<int>(RogueLikeData.Instance.GetMyTeam().Select(u => u.idx));
                                var valid = UnitLoader.Instance.GetAllCachedUnits().Where(u => u.rarity == 4 && !myIdx.Contains(u.idx)).ToList();
                                if (valid.Count == 0)
                                {
                                    if (choiceData.choiceId == 95)
                                        resultTextIndex = 1;

                                    resultLog += "모든 영웅 유닛 보유\n";
                                    break;
                                }

                                int ri = RogueLikeData.Instance.GetRandomInt(0, valid.Count);
                                var pick = valid[ri];
                                var newUnit = UnitLoader.Instance.GetCloneUnitById(pick.idx);
                                RogueLikeData.Instance.AddMyTeam(newUnit);
                                string name = GameTextDB.GetByForeignKey(TextKind.Unit, newUnit.idx);
                                resultLog += $"'{origin.unitName}' 희생 → '{name}' 획득\n";
                                PushResultToken(resultTokens, name);
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
                        if (choiceData.choiceId == 101)
                        {
                            List<RogueUnitDataBase> myTeam = RogueLikeData.Instance.GetMyTeam();
                            int changedCount = 0;

                            for (int k = 0; k < myTeam.Count; k++)
                            {
                                RogueUnitDataBase origin = myTeam[k];
                                if (origin == null || origin.rarity != 1)
                                    continue;

                                RogueUnitDataBase newUnit = RogueUnitDataBase.RandomUnitReForm(origin);
                                if (newUnit == null)
                                    continue;

                                myTeam[k] = newUnit;
                                changedCount++;
                                resultLog += $"- {origin.unitName} → {newUnit.unitName}\n";
                                PushRequireToken(origin.unitName);
                                PushResultToken(resultTokens, newUnit.unitName);
                            }

                            RogueLikeData.Instance.SetMyTeam(myTeam);
                            if (changedCount == 0)
                                resultLog += "- 전직 가능한 희귀도 1 유닛이 없습니다.\n";

                            break;
                        }

                        int changeCount = SafeParseInt(count);
                        if (changeCount <= 0) changeCount = 1;

                        if (form == ResultForm.Select)
                        {
                            if (selectedUnits == null || selectedUnits.Count == 0)
                                selectedUnits = RogueLikeData.Instance.GetSelectedUnits();

                            if (selectedUnits == null || selectedUnits.Count == 0)
                            {
                                resultLog += "- 전직할 유닛을 찾지 못했습니다.\n";
                                break;
                            }

                            if (isBattle)
                            {
                                foreach (var unit in selectedUnits)
                                {
                                    RogueUnitDataBase newUnit = unit;
                                    for (int k = 0; k < changeCount && newUnit != null; k++)
                                        newUnit = RogueUnitDataBase.RandomUnitReForm(newUnit);

                                    if (newUnit == null) continue;
                                    RogueLikeData.Instance.AddChangeReward(newUnit);
                                    PushResultToken(resultTokens, newUnit.unitName);
                                }
                            }
                            else
                            {
                                foreach (var unit in selectedUnits)
                                {
                                    RogueUnitDataBase newUnit = unit;
                                    for (int k = 0; k < changeCount && newUnit != null; k++)
                                        newUnit = RogueUnitDataBase.RandomUnitReForm(newUnit);

                                    if (newUnit == null) continue;
                                    RogueLikeData.Instance.AddMyTeam(newUnit);
                                    resultLog += $"- {unit.unitName} → {newUnit.unitName}\n";
                                    PushResultToken(resultTokens, newUnit.unitName);
                                }
                            }
                        }
                        else if (form == ResultForm.Random)
                        {
                            int unitCount = SafeParseInt(count);
                            if (unitCount <= 0) unitCount = 1;

                            List<RogueUnitDataBase> targets = selectedUnits;
                            if (targets == null || targets.Count == 0)
                                targets = RogueLikeData.Instance.GetSelectedUnits();

                            bool alreadyConsumedByRequire = targets != null && targets.Count > 0;
                            if (!alreadyConsumedByRequire)
                            {
                                targets = RogueLikeData.Instance.GetMyTeam()
                                    .Where(u => u != null && u.rarity < 4)
                                    .OrderBy(_ => RogueLikeData.Instance.GetRandomFloat())
                                    .Take(unitCount)
                                    .ToList();

                                List<RogueUnitDataBase> currentTeam = RogueLikeData.Instance.GetMyTeam();
                                currentTeam.RemoveAll(unit => targets.Any(target => target.UniqueId == unit.UniqueId));
                                RogueLikeData.Instance.SetMyTeam(currentTeam);
                            }

                            if (targets == null || targets.Count == 0)
                            {
                                resultLog += "- 전직 가능한 유닛이 없습니다.\n";
                                break;
                            }

                            foreach (var unit in targets)
                            {
                                RogueUnitDataBase newUnit = RogueUnitDataBase.RandomUnitReForm(unit);
                                if (newUnit == null) continue;

                                RogueLikeData.Instance.AddMyTeam(newUnit);
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
                            bool positive = RogueLikeData.Instance.GetRandomFloat() < 0.5f;
                            int[] effects = positive
                                ? new[] { 0, 2, 4, 5, 6, 8 }
                                : new[] { 1, 3, 7 };
                            int randEffect = effects[RogueLikeData.Instance.GetRandomInt(0, effects.Length)];

                            switch (randEffect)
                            {
                                case 0:
                                    {
                                        var r = RelicManager.HandleRandomRelic(1, RelicManager.RelicAction.Acquire);
                                        string name = r != null ? GameTextDB.GetByForeignKey(TextKind.RelicName, r.id) : string.Empty;
                                        if (string.IsNullOrEmpty(name) && r != null) name = r.name;
                                        resultLog += $"- 일반 유산 '{name}'\n";
                                        PushResultToken(resultTokens, name);
                                        break;
                                    }
                                case 1:
                                    {
                                        var r = RelicManager.HandleRandomRelic(0, RelicManager.RelicAction.Acquire);
                                        string name = r != null ? GameTextDB.GetByForeignKey(TextKind.RelicName, r.id) : string.Empty;
                                        if (string.IsNullOrEmpty(name) && r != null) name = r.name;
                                        resultLog += $"- 저주 유산 '{name}'\n";
                                        PushResultToken(resultTokens, name);
                                        break;
                                    }
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
                                                string name = GameTextDB.GetByForeignKey(TextKind.Unit, unit.idx);
                                                string pName = GameTextDB.GetByForeignKey(TextKind.Unit, promoted.idx);
                                                resultLog += $"- '{name}' → '{pName}'\n";
                                                PushRequireToken(name);
                                                PushResultToken(resultTokens, pName);
                                            }
                                        }
                                        break;
                                    }
                                case 3:
                                    {
                                        var my = RogueLikeData.Instance.GetMyTeam().Where(u => u.Energy > 1).ToList();
                                        if (my.Count > 0)
                                        {
                                            var target = my[RogueLikeData.Instance.GetRandomInt(0, my.Count)];
                                            target.SetEnergyDirect(1);
                                            string name = GameTextDB.GetByForeignKey(TextKind.Unit, target.idx);
                                            resultLog += $"- '{name}' 기력 1\n";
                                            PushResultToken(resultTokens, name);
                                        }
                                        break;
                                    }
                                case 4:
                                    {
                                        var (min, max) = ParseRange("1~3");
                                        var all = UnitLoader.Instance.GetAllCachedUnits();
                                        var myIdx = System.Linq.Enumerable.ToHashSet(RogueLikeData.Instance.GetMyTeam().Select(u => u.idx));
                                        var cands = all.Where(u => u.rarity >= min && u.rarity <= max && !myIdx.Contains(u.idx)).ToList();
                                        RemoveCommonUnitChoicesIfNeeded(cands);
                                        if (cands.Count > 0)
                                        {
                                            int selIdx = (rarity1Rate > 0f)
                                                ? PickIndexWithRarity1Penalty(cands, rarity1Rate)
                                                : RogueLikeData.Instance.GetRandomInt(0, cands.Count);
                                            var sel = cands[selIdx];
                                            RogueLikeData.Instance.AddMyTeam(sel);
                                            string name = GameTextDB.GetByForeignKey(TextKind.Unit, sel.idx);
                                            resultLog += $"- 유닛 '{name}' 획득\n";
                                            PushResultToken(resultTokens, name);
                                        }
                                        break;
                                    }
                                case 5:
                                    RogueLikeData.Instance.IncreaseRandomUpgrade(false);
                                    resultLog += "- 무작위 병종 강화\n";
                                    PushResultToken(resultTokens, "전술 개량");
                                    break;
                                case 6:
                                    RogueLikeData.Instance.SetMorale(100);
                                    resultLog += "- 사기 최대\n";
                                    PushResultToken(resultTokens, "사기 최대");
                                    break;
                                case 7:
                                    {
                                        int m = RogueLikeData.Instance.ChangeMorale(-25);
                                        resultLog += $"- 사기 {-m} 감소\n";
                                        PushResultToken(resultTokens, $"사기 {-m} 감소");
                                        break;
                                    }
                                case 8:
                                    {
                                        int g = RogueLikeData.Instance.AddGoldByEventChapter(150);
                                        resultLog += $"- 금화 {g} 획득\n";
                                        PushResultToken(resultTokens, $"금화 {g}");
                                        break;
                                    }
                            }
                        }
                        else if (choiceData.choiceId == 25)
                        {
                            var unit = selectedUnits[0];
                            int rarity = unit.rarity;
                            int getGold = (rarity == 1) ? RogueLikeData.Instance.AddGoldByEventChapter(50)
                                       : RogueLikeData.Instance.AddGoldByEventChapter(150);
                            string name = GameTextDB.GetByForeignKey(TextKind.Unit, unit.idx);
                            resultLog += $"'{name}' 희생 → 금화 {getGold}\n";
                            PushResultToken(resultTokens, $"금화 {getGold}");
                            if (rarity == 3)
                            {
                                var r = RelicManager.HandleRandomRelic(1, RelicAction.Acquire);
                                string rName = GameTextDB.GetByForeignKey(TextKind.RelicName, r.id);
                                resultLog += $"+ 전쟁 유산 '{rName}'\n";
                                PushResultToken(resultTokens, rName ?? "");
                            }
                        }
                        else if (choiceData.choiceId == 37)
                        {
                            bool success = UnityEngine.Random.value < 0.5f;
                            resultTextIndex = success ? 0 : 1;

                            if (success)
                            {
                                WarRelic r = RelicManager.HandleRandomRelic(5, RelicAction.Acquire);
                                string rName = GameTextDB.GetByForeignKey(TextKind.RelicName, r.id);
                                resultLog += $"'{rName}' 획득\n";
                                PushResultToken(resultTokens, rName);
                            }
                            else
                            {
                                battleGrade = 5;
                                isBattle = true;
                            }
                        }
                        break;
                    }

                case ResultType.Curse:
                    {
                        int relicCount = SafeParseInt(count);
                        if (relicCount <= 0) relicCount = 1;

                        for (int k = 0; k < relicCount; k++)
                        {
                            var relic = RelicManager.HandleRandomRelic(0, RelicAction.Acquire);
                            if (relic == null) continue;

                            string name = GameTextDB.GetByForeignKey(TextKind.RelicName, relic.id);
                            if (string.IsNullOrEmpty(name)) name = relic.name;
                            resultLog += $"- 저주 유산 획득: {name}\n";
                            PushResultToken(resultTokens, name);
                        }
                        break;
                    }

                case ResultType.None:
                    resultLog += "- 아무 일도 일어나지 않았다\n";
                    break;

                case ResultType.Battle:
                    {
                        battleGrade = SafeParseInt(value);
                        if (form == ResultForm.Special)
                        {
                            if (choiceData.choiceId == 16)
                            {
                                bool noBattle = UnityEngine.Random.value < 0.5f;
                                resultTextIndex = noBattle ? 0 : 1;

                                if (noBattle)
                                {
                                    resultLog += "- 아무 일도 일어나지 않았다\n";
                                    break;
                                }
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
                        int appliedTrainingCount = 0;

                        for (int k = 0; k < choiceData.resultValue.Count; k++)
                        {
                            string valueStr = choiceData.resultValue[k];
                            bool useRandom = string.IsNullOrEmpty(valueStr) || valueStr == "-1";
                            int upCnt = (k < choiceData.resultCount.Count && int.TryParse(choiceData.resultCount[k], out var c)) ? c : 1;

                            for (int j = 0; j < upCnt; j++)
                            {
                                if (useRandom)
                                {
                                    RogueLikeData.Instance.IncreaseRandomUpgrade(false);
                                }
                                else
                                {
                                    var parts = valueStr.Split(',');
                                    var unitTypes = parts.Select(s => int.TryParse(s, out var v) ? v : -1).Where(v => v >= 0 && v < 8).ToList();
                                    if (unitTypes.Count == 0) continue;
                                    int randomType = unitTypes[RogueLikeData.Instance.GetRandomInt(0, unitTypes.Count)];
                                    bool isAttack = UnityEngine.Random.value < 0.5f;
                                    RogueLikeData.Instance.IncreaseUpgrade(randomType, isAttack, false);
                                }

                                appliedTrainingCount++;
                                PushResultToken(resultTokens, "전술 개량");
                            }
                        }

                        resultLog += appliedTrainingCount > 0
                            ? $"랜덤 병종 강화 {appliedTrainingCount}회 적용\n"
                            : "랜덤 병종 강화 적용\n";
                        break;
                    }

                case ResultType.Field:
                    {

                        int fieldId = SafeParseInt(value);
                        Debug.Log(fieldId + "," + value);
                        RogueLikeData.Instance.SetFieldId(fieldId);
                        resultLog += "다음 전장이 변경되었습니다.\n";
                        break;
                    }

                default:
                    resultLog += "- 알 수 없는 보상\n";
                    break;
            } // switch
        } // for

        string narration = ComposeResultNarration(choiceData, selectedUnits, resultTokens, resultTextIndex);
        if (!string.IsNullOrEmpty(narration))
            resultLog = narration;

        if (isBattle)
        {
            SetPresetIdByGrade(battleGrade);
            GameManager.Instance.OpenBattlePanel();
        }
        return (resultLog, isBattle);
    }


    private static bool IsUnitVictory(RogueUnitDataBase unit)
    {
        if (unit.Mobility >= 12f)
            return true;

        float winChance = unit.Mobility * 0.09f;
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

    private static bool InRange(int actual, string countStr, bool useMinBound = true, float addtion = 1)
    {
        if (string.IsNullOrWhiteSpace(countStr))
            return false;

        if (countStr.Contains('~'))
        {
            var (min, max) = ParseRange(countStr);
            return actual >= min && actual <= max * addtion;
        }

        if (!int.TryParse(countStr, out int baseValue))
            return false;

        int v = (int)(baseValue * addtion);
        return useMinBound ? actual >= v : actual <= v;
    }
    // 사용처: 저장된 이벤트 복원. 이미 열린 이벤트이므로 등장 조건은 다시 검사하지 않는다.
    public static EventData GetEventByIdRaw(int eventId)
    {
        EventDataLoader.LoadData();

        if (EventDataLoader.EventDataDict.TryGetValue(eventId, out var eventData))
            return eventData;

        Debug.LogWarning($"저장된 이벤트 {eventId}를 찾지 못했습니다.");
        return null;
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
                    RogueLikeData.Instance.SetCurrentStage(15, 1, StageType.Boss);
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
        StagePreset stage = filtered[RogueLikeData.Instance.GetRandomInt(0, filtered.Count)];
        RogueLikeData.Instance.SetPresetID(stage.PresetID);
    }

    // 이 함수는 선택지 결과 문장을 만든다.
    // EventManager.cs

    private static string ComposeResultNarration(
        EventChoiceData choiceData,
        List<RogueUnitDataBase> selectedUnits,
        List<string> resultTokens,
        int resultTextIndex)
    {
        int foreignKey = choiceData.gameTextForeignKey != 0 ? choiceData.gameTextForeignKey : choiceData.choiceId;
        int baseTitleKey = choiceData.gameTextTitleKey_resultTextBase != 0 ? choiceData.gameTextTitleKey_resultTextBase : 130;
        int requestedTitleKey = baseTitleKey + Mathf.Max(0, resultTextIndex);

        string line = GameTextDB.GetExact(TextKind.EventDesc, requestedTitleKey, foreignKey);

        if (string.IsNullOrEmpty(line) && choiceData.resultText != null && choiceData.resultText.Count > 0)
        {
            int safeIndex = Mathf.Clamp(resultTextIndex, 0, choiceData.resultText.Count - 1);
            line = choiceData.resultText[safeIndex];
        }

        if (string.IsNullOrEmpty(line))
            return string.Empty;

        if (selectedUnits == null || selectedUnits.Count == 0)
            selectedUnits = RogueLikeData.Instance.GetSelectedUnits();

        List<string> requireNames = null;
        if (lastRequireTokens.Count > 0)
        {
            requireNames = new List<string>(lastRequireTokens);
        }
        else if (selectedUnits != null && selectedUnits.Count > 0)
        {
            requireNames = new List<string>(selectedUnits.Count);
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                var u = selectedUnits[i];
                requireNames.Add(u != null ? u.unitName : string.Empty);
            }
        }

        List<string> textTokens = null;
        if (choiceData.resultValue != null && choiceData.resultValue.Count > 0)
        {
            textTokens = new List<string>(choiceData.resultValue.Count);
            for (int i = 0; i < choiceData.resultValue.Count; i++)
                textTokens.Add(choiceData.resultValue[i] ?? string.Empty);
        }

        if ((resultTokens == null || resultTokens.Count == 0) &&
            textTokens != null && textTokens.Count > 0)
        {
            resultTokens = textTokens;
            textTokens = null;
        }

        return GameTextDB.ComposeLines(
            new List<string>(1) { line },
            requireNames,
            resultTokens,
            textTokens);
    }

    // 요구 조건으로 선택/제거된 대상 이름을 결과 문장 치환에 사용한다.
    private static void PushRequireToken(string value)
    {
        if (!string.IsNullOrEmpty(value)) lastRequireTokens.Add(value);
    }

    // 결과 토큰을 추가한다.
    private static void PushResultToken(List<string> tokens, string value)
    {
        if (!string.IsNullOrEmpty(value)) tokens.Add(value);
    }

    private static void RemoveCommonUnitChoicesIfNeeded(List<RogueUnitDataBase> units)
    {
        if (units == null || units.Count == 0)
            return;

        if (RelicManager.CheckRelicById(111))
            units.RemoveAll(unit => unit != null && unit.rarity == 1);
    }

    // 사용처: 유산85 보유 시 rarity==1 유닛의 선택 확률을 vals[0]만큼 낮춘 가중 랜덤 인덱스 선택
    private static int PickIndexWithRarity1Penalty(List<RogueUnitDataBase> list, float reduce)
    {
        // rarity1의 최종 가중치 = 1 - reduce (예: reduce=0.15 => 가중치 0.85)
        float r1w = Mathf.Max(0f, 1f - reduce);

        // 총 가중치 합
        float total = 0f;
        for (int i = 0; i < list.Count; i++)
            total += (list[i].rarity == 1) ? r1w : 1f;

        // 전부 rarity1인데 reduce=1로 가중치가 0이 되는 예외 대비
        if (total <= 0f)
            return RogueLikeData.Instance.GetRandomInt(0, list.Count);

        // 룰렛휠
        float roll = RogueLikeData.Instance.GetRandomFloat() * total;
        for (int i = 0; i < list.Count; i++)
        {
            roll -= (list[i].rarity == 1) ? r1w : 1f;
            if (roll <= 0f)
                return i;
        }
        return list.Count - 1;
    }

}
