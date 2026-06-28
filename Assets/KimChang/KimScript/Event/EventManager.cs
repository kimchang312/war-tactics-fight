using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static RelicManager;


public class EventManager
{
    private static readonly List<string> lastRequireTokens = new List<string>(4);

    // 사용처: 길이가 불완전한 이벤트 JSON을 읽을 때 누락된 값을 기본값으로 처리한다.
    private static T GetListValue<T>(IList<T> values, int index, T fallback = default)
    {
        if (values == null || index < 0 || index >= values.Count)
            return fallback;

        return values[index];
    }

    // 사용처: 이벤트 JSON 문자열 배열의 누락 항목을 빈 문자열로 처리한다.
    private static string GetStringValue(IList<string> values, int index)
    {
        string value = GetListValue(values, index, string.Empty);
        return value ?? string.Empty;
    }

    // 사용처: Gold, Morale, Stage 조건이 count 대신 value에 저장된 구형 데이터도 처리한다.
    private static string GetRequireAmount(RequireThing thing, string value, string count)
    {
        if ((thing == RequireThing.Gold || thing == RequireThing.Morale || thing == RequireThing.Stage) &&
            string.IsNullOrWhiteSpace(count) &&
            !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return count;
    }

    // 사용처: JSON에 두 결과의 count가 하나로 합쳐진 전투 선택지를 원래 설계값으로 보정한다.
    private static string GetResultCount(EventChoiceData choiceData, int resultIndex)
    {
        if (choiceData != null && choiceData.choiceId == 28)
            return resultIndex == 0 ? "1" : "150";

        return GetStringValue(choiceData?.resultCount, resultIndex);
    }

    // 사용처: 선택지 UI가 실제 요구 조건과 동일한 비용 수치를 표시할 때 사용한다.
    public static string GetChoiceRequireCountForPreview(EventChoiceData choiceData, int requireIndex)
    {
        string count = GetChoiceRequireCount(choiceData, requireIndex);
        if (choiceData == null ||
            choiceData.requireThing == null ||
            requireIndex < 0 ||
            requireIndex >= choiceData.requireThing.Count)
        {
            return count;
        }

        if (choiceData.requireThing[requireIndex] != RequireThing.Morale ||
            !int.TryParse(count, out int moraleCost) ||
            moraleCost <= 0)
        {
            return count;
        }

        return Mathf.CeilToInt(moraleCost * GetMoraleCostMultiplier()).ToString();
    }

    // 사용처: 선택지 UI가 실제 결과 처리와 동일한 수치를 표시할 때 사용한다.
    public static string GetResultCountForPreview(EventChoiceData choiceData, int resultIndex)
    {
        return GetResultCount(choiceData, resultIndex);
    }

    // 사용처: Training 결과 하나에 여러 병종이 저장된 경우 병종 목록으로 합친다.
    private static string GetTrainingValue(EventChoiceData choiceData, int resultIndex, string fallbackValue)
    {
        if (choiceData == null || choiceData.resultType == null || choiceData.resultValue == null)
            return fallbackValue;

        if (choiceData.resultType.Count == 1 &&
            choiceData.resultType[0] == ResultType.Training &&
            choiceData.resultValue.Count > 1)
        {
            return string.Join(",", choiceData.resultValue.Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        return fallbackValue;
    }

    // 사용처: 유산 획득 및 제거 결과의 표시 이름을 안전하게 가져온다.
    private static string GetRelicDisplayName(WarRelic relic)
    {
        if (relic == null)
            return string.Empty;

        string name = GameTextDB.GetByForeignKey(TextKind.RelicName, relic.id);
        return string.IsNullOrEmpty(name) ? relic.name : name;
    }

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
        if (eventData == null || RogueLikeData.Instance == null)
            return false;

        int currentChapter = RogueLikeData.Instance.GetChapter();
        if (eventData.eventChapter == null || !eventData.eventChapter.Contains(currentChapter))
            return false;

        // 이벤트 데이터는 선택지 조건과 등장 조건의 의미가 일부 다르다.
        // 기존 기획에 맞는 특수 등장 조건은 여기에서 분리해 처리한다.
        switch (eventData.eventId)
        {
            case 5:
                return RogueLikeData.Instance.GetCurrentGold() >= 100 ||
                       RogueLikeData.Instance.GetMorale() >= 31;

            case 7:
                return RogueLikeData.Instance.GetMyTeam().Count(unit => unit != null && unit.Energy == 1) >= 2;

            case 11:
                {
                    int morale = RogueLikeData.Instance.GetMorale();
                    return morale >= 35 && morale <= 50;
                }

            case 17:
                return RogueLikeData.Instance.GetMyTeam().Any(unit => unit != null && unit.Energy >= 2);

            case 46:
                return RogueLikeData.Instance.GetMorale() > 50;
        }

        int requireCount = eventData.requireThing?.Count ?? 0;
        if (requireCount == 0)
            return true;

        bool hasCondition = false;
        for (int i = 0; i < requireCount; i++)
        {
            if (eventData.requireThing[i] != RequireThing.None)
            {
                hasCondition = true;
                break;
            }
        }

        if (!hasCondition)
            return true;

        for (int i = 0; i < requireCount; i++)
        {
            RequireThing thing = eventData.requireThing[i];
            if (thing == RequireThing.None)
                continue;

            RequireForm form = GetListValue(eventData.requireForm, i, RequireForm.None);
            string value = GetStringValue(eventData.requireValue, i);
            string count = GetStringValue(eventData.requireCount, i);

            if (thing == RequireThing.Special || form == RequireForm.Special)
            {
                if (!CheckSpecialRequire(eventData))
                    return false;

                continue;
            }

            if (!CheckRequireCondition(thing, form, value, count, false))
                return false;
        }

        return true;
    }


    // 사용처: 사기 소모 조건 표시와 실제 사기 차감에 동일한 유산 배율을 적용한다.
    private static float GetMoraleCostMultiplier()
    {
        if (!RelicManager.CheckRelicById(33))
            return 1f;

        WarRelic relic = RelicManager.GetRelicById(33);
        var values = relic?.GetAllValuesAsFloatListOrNull();
        if (values == null || values.Count == 0)
            return 1.2f;

        float increase = Mathf.Abs(values[0]);
        if (increase > 1f)
            increase *= 0.01f;

        return 1f + Mathf.Clamp(increase, 0f, 1f);
    }

    // 사용처: 이벤트 등장과 선택지 활성화 전에 현재 자원이 요구 조건을 만족하는지 검사한다.
    private static bool CheckRequireCondition(
        RequireThing thing,
        RequireForm form,
        string value,
        string count,
        bool applyMoraleCostMultiplier = true)
    {
        value ??= string.Empty;
        count ??= string.Empty;

        switch (thing)
        {
            case RequireThing.Gold:
                if (form == RequireForm.None)
                {
                    string amount = GetRequireAmount(thing, value, count);
                    return InRange(RogueLikeData.Instance.GetCurrentGold(), amount);
                }
                break;

            case RequireThing.Morale:
                {
                    string amount = GetRequireAmount(thing, value, count);
                    float multiplier = applyMoraleCostMultiplier ? GetMoraleCostMultiplier() : 1f;
                    return InRange(RogueLikeData.Instance.GetMorale(), amount, true, multiplier);
                }

            case RequireThing.Unit:
                {
                    int requiredCount = SafeParseInt(count);
                    List<RogueUnitDataBase> myUnits = RogueLikeData.Instance.GetMyTeam();

                    if (form != RequireForm.Select && form != RequireForm.Random && form != RequireForm.Special)
                        break;

                    if (string.IsNullOrWhiteSpace(value))
                        return myUnits.Count >= requiredCount;

                    if (value.Contains("~"))
                    {
                        var (min, max) = ParseRange(value);
                        return myUnits.Count(unit => unit != null && unit.rarity >= min && unit.rarity <= max) >= requiredCount;
                    }

                    int rarity = SafeParseInt(value);
                    return myUnits.Count(unit => unit != null && unit.rarity <= rarity) >= requiredCount;
                }

            case RequireThing.Relic:
                if (form == RequireForm.Random)
                {
                    int grade = SafeParseInt(value);
                    int requiredCount = SafeParseInt(count);

                    return RogueLikeData.Instance.GetAllOwnedRelics()
                        .Count(relic => relic != null && relic.grade == grade) >= requiredCount;
                }

                if (form == RequireForm.None)
                {
                    int relicId = SafeParseInt(string.IsNullOrWhiteSpace(count) ? value : count);
                    return !RelicManager.CheckRelicById(relicId);
                }
                break;

            case RequireThing.Energy:
                if (form == RequireForm.Random || form == RequireForm.Select)
                {
                    int requiredCount = SafeParseInt(count);
                    List<RogueUnitDataBase> myTeam = RogueLikeData.Instance.GetMyTeam();

                    // 빈 값은 기력 수치 조건이 아니라 유닛 선택만 요구하는 데이터다.
                    if (string.IsNullOrWhiteSpace(value))
                        return myTeam.Count(unit => unit != null) >= requiredCount;

                    int energyValue = SafeParseInt(value);
                    if (energyValue < 0)
                    {
                        int requiredEnergy = Mathf.Abs(energyValue);
                        return myTeam.Count(unit => unit != null && unit.Energy >= requiredEnergy) >= requiredCount;
                    }

                    return myTeam.Count(unit => unit != null && unit.Energy > energyValue) >= requiredCount;
                }
                break;

            case RequireThing.AttackDamage:
                {
                    int threshold = SafeParseInt(value);
                    int requiredCount = Mathf.Max(1, SafeParseInt(count));

                    return RogueLikeData.Instance.GetMyTeam()
                        .Count(unit => unit != null && unit.attackDamage >= threshold) >= requiredCount;
                }

            case RequireThing.Stage:
                if (form == RequireForm.None)
                {
                    string amount = GetRequireAmount(thing, value, count);
                    return InRange(RogueLikeData.Instance.GetCurrentStageX(), amount);
                }
                break;
        }

        return false;
    }

    private static int SafeParseInt(string value)
    {
        return int.TryParse(value, out int result) ? result : 0;
    }

    // 사용처: 선택형 결과에서 전달된 목록이 비어 있으면 전역 선택 목록의 첫 유닛을 사용한다.
    private static RogueUnitDataBase GetSelectedUnit(List<RogueUnitDataBase> selectedUnits)
    {
        if (selectedUnits != null && selectedUnits.Count > 0)
            return selectedUnits[0];

        List<RogueUnitDataBase> savedSelection = RogueLikeData.Instance.GetSelectedUnits();
        return savedSelection != null && savedSelection.Count > 0 ? savedSelection[0] : null;
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
        if (eventChoiceData == null || eventChoiceData.requireThing == null)
            return false;

        int requireCount = eventChoiceData.requireThing.Count;
        for (int i = 0; i < requireCount; i++)
        {
            RequireThing thing = eventChoiceData.requireThing[i];
            if (thing == RequireThing.None)
                continue;

            RequireForm form = GetListValue(eventChoiceData.requireForm, i, RequireForm.None);
            string value = GetStringValue(eventChoiceData.requireValue, i);
            string count = GetChoiceRequireCount(eventChoiceData, i);

            if (thing == RequireThing.Special || form == RequireForm.Special)
            {
                if (!CheckSpecialChoiceRequire(eventChoiceData))
                    return false;

                continue;
            }

            if (!CheckRequireCondition(thing, form, value, count, true))
                return false;
        }

        return true;
    }

    // 사용처: 선택지 81의 합쳐진 비용 문자열을 원래 사기 20, 금화 100으로 분리한다.
    private static string GetChoiceRequireCount(EventChoiceData choiceData, int requireIndex)
    {
        if (choiceData != null && choiceData.choiceId == 81)
            return requireIndex == 0 ? "20" : requireIndex == 1 ? "100" : string.Empty;

        return GetStringValue(choiceData?.requireCount, requireIndex);
    }

    private static bool CheckSpecialChoiceRequire(EventChoiceData eventChoiceData)
    {
        if (eventChoiceData == null)
            return false;

        if (eventChoiceData.choiceId == 82)
        {
            List<int> relicIds = RogueLikeData.Instance.GetAllOwnedRelicIds();
            int count = 0;

            if (relicIds.Contains(23)) count++;
            if (relicIds.Contains(24)) count++;
            if (relicIds.Contains(25)) count++;

            return count == 2 && RogueLikeData.Instance.GetMyTeam().Count > 0;
        }

        return false;
    }

    // 사용처: 선택지 효과를 적용하기 전에 금화, 사기, 유닛, 유산 같은 요구 자원을 차감한다.
    public static void ReduceRequire(EventChoiceData choiceData)
    {
        if (choiceData == null || choiceData.requireThing == null)
            return;

        lastRequireTokens.Clear();

        bool hasSelectRequirement = choiceData.requireForm != null &&
                                    choiceData.requireForm.Contains(RequireForm.Select);
        if (!hasSelectRequirement)
            RogueLikeData.Instance.SetSelectedUnits(new List<RogueUnitDataBase>());

        int requireCount = choiceData.requireThing.Count;
        for (int i = 0; i < requireCount; i++)
        {
            RequireThing thing = choiceData.requireThing[i];
            RequireForm form = GetListValue(choiceData.requireForm, i, RequireForm.None);
            string value = GetStringValue(choiceData.requireValue, i);
            string count = GetChoiceRequireCount(choiceData, i);

            switch (thing)
            {
                case RequireThing.None:
                    break;

                case RequireThing.Energy:
                    ApplyEnergyRequirement(form, value, count);
                    break;

                case RequireThing.Unit:
                    ApplyUnitRequirement(choiceData, form, value, count);
                    break;

                case RequireThing.Relic:
                    if (form == RequireForm.Random)
                    {
                        int grade = SafeParseInt(value);
                        int relicCount = SafeParseInt(count);

                        for (int k = 0; k < relicCount; k++)
                        {
                            WarRelic relic = RelicManager.HandleRandomRelic(grade, RelicAction.Remove);
                            string relicName = GetRelicDisplayName(relic);
                            if (!string.IsNullOrEmpty(relicName))
                                PushRequireToken(relicName);
                        }
                    }
                    break;

                case RequireThing.Gold:
                    if (choiceData.choiceId == 27)
                    {
                        RogueLikeData.Instance.EarnGold(-RogueLikeData.Instance.GetCurrentGold());
                    }
                    else if (form == RequireForm.None)
                    {
                        int goldCount = SafeParseInt(GetRequireAmount(thing, value, count));
                        if (goldCount > 0)
                            RogueLikeData.Instance.ReduceGold(goldCount);
                    }
                    break;

                case RequireThing.Morale:
                    if (form == RequireForm.None)
                    {
                        int baseCost = SafeParseInt(GetRequireAmount(thing, value, count));
                        if (baseCost <= 0)
                            break;

                        int actualCost = Mathf.CeilToInt(baseCost * GetMoraleCostMultiplier());
                        RogueLikeData.Instance.ChangeMorale(-actualCost);
                    }
                    break;
            }
        }
    }

    // 사용처: 선택지에서 지정한 유닛의 기력 조건을 처리하고 선택된 유닛 이름을 결과 텍스트에 전달한다.
    private static void ApplyEnergyRequirement(RequireForm form, string value, string count)
    {
        if (form == RequireForm.Select)
        {
            List<RogueUnitDataBase> selectedUnits = RogueLikeData.Instance.GetSelectedUnits();
            if (selectedUnits == null)
                return;

            foreach (RogueUnitDataBase unit in selectedUnits)
            {
                if (unit == null)
                    continue;

                if (value == "-1")
                    unit.SetEnergyDirect(Mathf.Max(0, unit.Energy - 1));

                PushRequireToken(unit.unitName);
            }

            return;
        }

        if (form != RequireForm.Random)
            return;

        int targetEnergy = SafeParseInt(value);
        int unitCount = SafeParseInt(count);

        List<RogueUnitDataBase> candidates = RogueLikeData.Instance.GetMyTeam()
            .Where(unit => unit != null && unit.Energy > targetEnergy)
            .ToList();

        for (int i = 0; i < unitCount && candidates.Count > 0; i++)
        {
            int randomIndex = RogueLikeData.Instance.GetRandomInt(0, candidates.Count);
            RogueUnitDataBase unit = candidates[randomIndex];
            candidates.RemoveAt(randomIndex);

            unit.SetEnergyDirect(targetEnergy);
            PushRequireToken(unit.unitName);
        }
    }

    // 사용처: 선택지에서 요구하는 유닛 희생 또는 선택을 처리하고 결과 텍스트에 유닛 이름을 전달한다.
    private static void ApplyUnitRequirement(EventChoiceData choiceData, RequireForm form, string value, string count)
    {
        List<RogueUnitDataBase> myUnits = RogueLikeData.Instance.GetMyTeam();

        if (form == RequireForm.Select)
        {
            List<RogueUnitDataBase> selectedUnits = RogueLikeData.Instance.GetSelectedUnits() ?? new List<RogueUnitDataBase>();

            if (ShouldConsumeSelectedUnits(choiceData))
            {
                myUnits.RemoveAll(unit => unit != null &&
                                          selectedUnits.Any(selected => selected != null && selected.UniqueId == unit.UniqueId));
                RogueLikeData.Instance.SetMyTeam(myUnits);
            }

            foreach (RogueUnitDataBase unit in selectedUnits)
            {
                if (unit != null)
                    PushRequireToken(unit.unitName);
            }

            return;
        }

        if (form == RequireForm.Random)
        {
            int unitCount = SafeParseInt(count);
            int minRarity = int.MinValue;
            int maxRarity = int.MaxValue;

            if (!string.IsNullOrWhiteSpace(value))
            {
                if (value.Contains("~"))
                {
                    (minRarity, maxRarity) = ParseRange(value);
                }
                else
                {
                    minRarity = 1;
                    maxRarity = SafeParseInt(value);
                }
            }

            List<RogueUnitDataBase> candidates = myUnits
                .Where(unit => unit != null && unit.rarity >= minRarity && unit.rarity <= maxRarity)
                .ToList();

            List<RogueUnitDataBase> selected = new List<RogueUnitDataBase>(unitCount);
            for (int i = 0; i < unitCount && candidates.Count > 0; i++)
            {
                int randomIndex = RogueLikeData.Instance.GetRandomInt(0, candidates.Count);
                selected.Add(candidates[randomIndex]);
                candidates.RemoveAt(randomIndex);
            }

            myUnits.RemoveAll(unit => selected.Any(candidate => candidate.UniqueId == unit.UniqueId));
            RogueLikeData.Instance.SetMyTeam(myUnits);

            foreach (RogueUnitDataBase unit in selected)
            {
                RogueLikeData.Instance.AddSelectedUnits(unit);
                PushRequireToken(unit.unitName);
            }

            return;
        }

        if (form == RequireForm.Special && choiceData.choiceId == 82)
        {
            RogueUnitDataBase sacrificeUnit = myUnits
                .OrderByDescending(unit => unit.unitPrice)
                .ThenByDescending(unit => unit.Energy)
                .FirstOrDefault();

            if (sacrificeUnit == null)
                return;

            myUnits.RemoveAll(unit => unit.UniqueId == sacrificeUnit.UniqueId);
            RogueLikeData.Instance.SetMyTeam(myUnits);
            RogueLikeData.Instance.AddSelectedUnits(sacrificeUnit);
            PushRequireToken(sacrificeUnit.unitName);
        }
    }

    //선택시 보상 획득
    public static (string, bool) ApplyChoiceResult(EventChoiceData choiceData, List<RogueUnitDataBase> selectedUnits)
    {
        if (choiceData == null || choiceData.resultType == null)
            return (string.Empty, false);

        string resultLog = string.Empty;
        bool isBattle = false;
        int battleGrade = 0;

        List<string> resultTokens = new List<string>(4);
        int resultTextIndex = 0;

        float rarity1Rate = 0f;
        WarRelic relic85 = RelicManager.GetRelicById(85);
        var values = relic85?.GetAllValuesAsFloatListOrNull();
        if (values != null && values.Count > 0)
        {
            rarity1Rate = Mathf.Abs(values[0]);
            if (rarity1Rate > 1f)
                rarity1Rate *= 0.01f;

            rarity1Rate = Mathf.Clamp01(rarity1Rate);
        }

        for (int i = 0; i < choiceData.resultType.Count; i++)
        {
            ResultType type = choiceData.resultType[i];
            ResultForm form = GetListValue(choiceData.resultForm, i, ResultForm.None);
            string value = GetStringValue(choiceData.resultValue, i);
            string count = GetResultCount(choiceData, i);

            switch (type)
            {
                case ResultType.Gold:
                    {
                        if (form == ResultForm.Random)
                        {
                            int probability = string.IsNullOrWhiteSpace(value) ? 100 : SafeParseInt(value);
                            bool success = RogueLikeData.Instance.GetRandomFloat() < probability * 0.01f;
                            resultTextIndex = success ? 0 : 1;

                            if (!success)
                            {
                                resultLog += "- 아무것도 얻지 못했습니다.\n";
                                break;
                            }
                        }

                        int gold = SafeParseInt(count);
                        if (isBattle)
                        {
                            RogueLikeData.Instance.AddGoldReward(gold);
                        }
                        else
                        {
                            gold = RogueLikeData.Instance.AddGoldByEventChapter(gold);
                            resultLog += $"- 금화 {gold} 획득\n";
                            PushResultToken(resultTokens, $"금화 {gold}");
                        }
                        break;
                    }

                case ResultType.Morale:
                    {
                        int morale = SafeParseInt(count);
                        if (isBattle)
                        {
                            RogueLikeData.Instance.AddMoraleReward(morale);
                        }
                        else
                        {
                            morale = RogueLikeData.Instance.ChangeMorale(morale);
                            resultLog += $"- 사기 {morale} 회복\n";
                            PushResultToken(resultTokens, $"사기 {morale}");
                        }
                        break;
                    }

                case ResultType.Energy:
                    {
                        int energy = SafeParseInt(value);
                        List<RogueUnitDataBase> targets = null;

                        if (form == ResultForm.Select)
                            targets = selectedUnits == null || selectedUnits.Count == 0
                                ? RogueLikeData.Instance.GetSelectedUnits()
                                : selectedUnits;
                        else if (form == ResultForm.All)
                            targets = RogueLikeData.Instance.GetMyTeam();

                        if (targets == null)
                            break;

                        foreach (RogueUnitDataBase unit in targets)
                        {
                            if (unit == null)
                                continue;

                            int nextEnergy = Mathf.Clamp(unit.Energy + energy, 1, unit.MaxEnergy);
                            unit.SetEnergyDirect(nextEnergy);
                        }

                        if (form == ResultForm.All)
                        {
                            resultLog += energy >= 0
                                ? "모든 유닛의 기력이 회복되었습니다.\n"
                                : "모든 유닛의 기력이 감소했습니다.\n";
                        }
                        else if (form == ResultForm.Select)
                        {
                            foreach (RogueUnitDataBase unit in targets)
                            {
                                if (unit != null)
                                {
                                    resultLog += $"- 기력 회복 {unit.unitName}\n";
                                    PushResultToken(resultTokens, unit.unitName);
                                }
                            }
                        }

                        break;
                    }

                case ResultType.Relic:
                    {
                        if (form == ResultForm.Random)
                        {
                            int grade = SafeParseInt(value);
                            int relicCount = SafeParseInt(count);

                            if (isBattle)
                            {
                                for (int k = 0; k < relicCount; k++)
                                {
                                    int relicId = RelicManager.GetRandomRelicId(grade, RelicAction.Acquire);
                                    if (relicId >= 0)
                                        RogueLikeData.Instance.AddRelicReward(relicId);
                                }
                            }
                            else
                            {
                                for (int k = 0; k < relicCount; k++)
                                {
                                    WarRelic relic = RelicManager.HandleRandomRelic(grade, RelicAction.Acquire);
                                    string name = GetRelicDisplayName(relic);
                                    if (string.IsNullOrEmpty(name))
                                        continue;

                                    resultLog += $"- 전쟁 유산 획득: {name}\n";
                                    PushResultToken(resultTokens, name);
                                }
                            }

                            break;
                        }

                        if (form == ResultForm.Select || form == ResultForm.None)
                        {
                            int relicId = SafeParseInt(value);
                            if (RelicManager.TryAcquireRelic(relicId, out WarRelic acquiredRelic))
                            {
                                string name = GetRelicDisplayName(acquiredRelic);
                                resultLog += $"- 전쟁 유산 획득: {name}\n";
                                PushResultToken(resultTokens, name);

                                if (choiceData.choiceId == 113)
                                {
                                    List<RogueUnitDataBase> selected = RogueLikeData.Instance.GetSelectedUnits();
                                    if (selected != null && selected.Count > 0 && selected[0] != null)
                                    {
                                        selected[0].endless = true;
                                        selected[0].SetEnergyDirect(1);
                                    }
                                }
                            }
                            else
                            {
                                resultLog += "- 전쟁 유산을 획득하지 못했습니다.\n";
                            }

                            break;
                        }

                        if (form != ResultForm.Special)
                            break;

                        if (choiceData.choiceId == 59)
                        {
                            bool hasCurse = RogueLikeData.Instance.GetRandomFloat() < 0.5f;
                            resultTextIndex = hasCurse ? 1 : 0;

                            if (hasCurse)
                            {
                                WarRelic relic = RelicManager.HandleRandomRelic(0, RelicAction.Acquire);
                                string name = GetRelicDisplayName(relic);
                                if (!string.IsNullOrEmpty(name))
                                {
                                    resultLog += $"- 전쟁 유산 획득: {name}\n";
                                    PushResultToken(resultTokens, name);
                                }
                            }
                        }
                        else if (choiceData.choiceId == 73)
                        {
                            RogueUnitDataBase unit = GetSelectedUnit(selectedUnits);
                            if (unit == null)
                            {
                                resultLog += "- 경기에 참가할 유닛을 찾지 못했습니다.\n";
                                break;
                            }

                            bool success = IsUnitVictory(unit);
                            resultTextIndex = success ? 0 : 1;

                            if (success)
                            {
                                WarRelic relic = RelicManager.HandleRandomRelic(10, RelicAction.Acquire);
                                string name = GetRelicDisplayName(relic);
                                if (!string.IsNullOrEmpty(name))
                                {
                                    resultLog += $"- 전쟁 유산 획득: {name}\n";
                                    PushResultToken(resultTokens, name);
                                }
                            }
                            else
                            {
                                resultLog += "- 경기에 패배해 아무것도 얻지 못했습니다.\n";
                            }
                        }
                        else if (choiceData.choiceId == 75)
                        {
                            RogueUnitDataBase unit = GetSelectedUnit(selectedUnits);
                            if (unit == null)
                            {
                                resultLog += "- 결투할 유닛을 찾지 못했습니다.\n";
                                break;
                            }

                            bool success = IsUnitVictoryByRarity(unit);
                            resultTextIndex = success ? 0 : 1;

                            if (success)
                            {
                                RogueLikeData.Instance.AddMyTeam(unit);
                                WarRelic relic = RelicManager.HandleRandomRelic(10, RelicAction.Acquire);
                                string name = GetRelicDisplayName(relic);
                                if (!string.IsNullOrEmpty(name))
                                {
                                    resultLog += $"- 결투 승리: 전쟁 유산 {name} 획득\n";
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
                            List<int> relicIds = RogueLikeData.Instance.GetAllOwnedRelicIds();
                            int rewardRelicId = !relicIds.Contains(23) ? 23 : (!relicIds.Contains(24) ? 24 : 25);

                            if (RelicManager.AcquireRelic(rewardRelicId))
                            {
                                string name = GameTextDB.GetByForeignKey(TextKind.RelicName, rewardRelicId);
                                resultLog += "- 보석 건틀릿의 마지막 유산을 획득했습니다.\n";
                                PushResultToken(resultTokens, name);
                            }
                        }
                        else if (choiceData.choiceId == 132)
                        {
                            bool success = RogueLikeData.Instance.GetRandomFloat() < 0.5f;
                            resultTextIndex = success ? 0 : 1;

                            int rewardGrade = success ? 10 : 0;
                            WarRelic relic = RelicManager.HandleRandomRelic(rewardGrade, RelicAction.Acquire);
                            string name = GetRelicDisplayName(relic);
                            if (!string.IsNullOrEmpty(name))
                            {
                                resultLog += $"- 전쟁 유산 획득: {name}\n";
                                PushResultToken(resultTokens, name);
                            }
                        }

                        break;
                    }

                case ResultType.Unit:
                    {
                        if (form == ResultForm.None)
                        {
                            int unitId = SafeParseInt(value);
                            int unitCount = SafeParseInt(count);

                            for (int k = 0; k < unitCount; k++)
                            {
                                RogueUnitDataBase unit = UnitLoader.Instance.GetCloneUnitById(unitId);
                                if (unit == null)
                                    continue;

                                RogueLikeData.Instance.AddMyTeam(unit);
                                string name = GameTextDB.GetByForeignKey(TextKind.Unit, unit.idx);
                                if (string.IsNullOrEmpty(name))
                                    name = unit.unitName;

                                resultLog += $"- {name} 추가\n";
                                PushResultToken(resultTokens, name);
                            }
                            break;
                        }

                        if (form == ResultForm.Random)
                        {
                            int unitCount = SafeParseInt(count);

                            if (value.Contains("~"))
                            {
                                var (min, max) = ParseRange(value);
                                List<RogueUnitDataBase> candidates = UnitLoader.Instance.GetAllCachedUnits()
                                    .Where(unit => unit != null && unit.rarity >= min && unit.rarity <= max)
                                    .ToList();
                                RemoveCommonUnitChoicesIfNeeded(candidates);

                                for (int k = 0; k < unitCount && candidates.Count > 0; k++)
                                {
                                    int randomIndex = rarity1Rate > 0f
                                        ? PickIndexWithRarity1Penalty(candidates, rarity1Rate)
                                        : RogueLikeData.Instance.GetRandomInt(0, candidates.Count);

                                    RogueUnitDataBase pick = candidates[randomIndex];
                                    candidates.RemoveAt(randomIndex);

                                    RogueUnitDataBase newUnit = UnitLoader.Instance.GetCloneUnitById(pick.idx);
                                    if (newUnit == null)
                                        continue;

                                    if (isBattle)
                                        RogueLikeData.Instance.AddUnitReward(newUnit);
                                    else
                                        RogueLikeData.Instance.AddMyTeam(newUnit);

                                    resultLog += isBattle ? string.Empty : $"- {newUnit.unitName} 추가\n";
                                    PushResultToken(resultTokens, newUnit.unitName);
                                }

                                break;
                            }

                            int unitRarity = SafeParseInt(value);
                            List<RogueUnitDataBase> allUnits = UnitLoader.Instance.GetAllCachedUnits();
                            List<RogueUnitDataBase> validUnits;

                            if (unitRarity == 4)
                            {
                                HashSet<int> ownedUnitIds = new HashSet<int>(
                                    RogueLikeData.Instance.GetMyTeam()
                                        .Where(unit => unit != null)
                                        .Select(unit => unit.idx));

                                validUnits = allUnits
                                    .Where(unit => unit != null && unit.rarity == 4 && !ownedUnitIds.Contains(unit.idx))
                                    .ToList();

                                if (validUnits.Count == 0)
                                {
                                    WarRelic relic = RelicManager.HandleRandomRelic(10, RelicAction.Acquire);
                                    string name = GetRelicDisplayName(relic);
                                    if (!string.IsNullOrEmpty(name))
                                    {
                                        resultLog += $"모든 영웅 유닛 보유. 전쟁 유산 {name} 획득\n";
                                        PushResultToken(resultTokens, name);
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                validUnits = allUnits
                                    .Where(unit => unit != null && unit.rarity == unitRarity)
                                    .ToList();
                            }

                            RemoveCommonUnitChoicesIfNeeded(validUnits);

                            for (int k = 0; k < unitCount && validUnits.Count > 0; k++)
                            {
                                int randomIndex = rarity1Rate > 0f
                                    ? PickIndexWithRarity1Penalty(validUnits, rarity1Rate)
                                    : RogueLikeData.Instance.GetRandomInt(0, validUnits.Count);

                                RogueUnitDataBase pick = validUnits[randomIndex];
                                validUnits.RemoveAt(randomIndex);

                                RogueUnitDataBase newUnit = UnitLoader.Instance.GetCloneUnitById(pick.idx);
                                if (newUnit == null)
                                    continue;

                                if (isBattle)
                                    RogueLikeData.Instance.AddUnitReward(newUnit);
                                else
                                    RogueLikeData.Instance.AddMyTeam(newUnit);

                                string name = GameTextDB.GetByForeignKey(TextKind.Unit, newUnit.idx);
                                if (string.IsNullOrEmpty(name))
                                    name = newUnit.unitName;

                                resultLog += isBattle ? string.Empty : $"- {name} 추가\n";
                                PushResultToken(resultTokens, name);
                            }

                            break;
                        }

                        if (form == ResultForm.Select)
                        {
                            RogueUnitDataBase origin = GetSelectedUnit(selectedUnits);
                            if (origin == null)
                                break;

                            RogueUnitDataBase clone = UnitLoader.Instance.GetCloneUnitById(origin.idx);
                            if (clone == null)
                                break;

                            clone.SetEnergyDirect(origin.Energy);
                            RogueLikeData.Instance.AddMyTeam(clone);
                            string name = GameTextDB.GetByForeignKey(TextKind.Unit, clone.idx);
                            if (string.IsNullOrEmpty(name))
                                name = clone.unitName;

                            resultLog += $"- {name} 추가\n";
                            PushResultToken(resultTokens, name);
                            break;
                        }

                        if (form == ResultForm.Special)
                        {
                            RogueUnitDataBase origin = GetSelectedUnit(selectedUnits);
                            if (origin == null)
                                break;

                            float chance = origin.rarity switch
                            {
                                1 => 0.3f,
                                2 => 0.6f,
                                3 => 1f,
                                _ => 0f
                            };

                            bool success = RogueLikeData.Instance.GetRandomFloat() < chance;
                            if (choiceData.choiceId == 95)
                                resultTextIndex = success ? 0 : 1;

                            if (!success)
                            {
                                resultLog += $"'{origin.unitName}' 희생했지만 변화 없음\n";
                                break;
                            }

                            HashSet<int> ownedHeroIds = new HashSet<int>(
                                RogueLikeData.Instance.GetMyTeam()
                                    .Where(unit => unit != null)
                                    .Select(unit => unit.idx));

                            List<RogueUnitDataBase> heroes = UnitLoader.Instance.GetAllCachedUnits()
                                .Where(unit => unit != null && unit.rarity == 4 && !ownedHeroIds.Contains(unit.idx))
                                .ToList();

                            if (heroes.Count == 0)
                            {
                                if (choiceData.choiceId == 95)
                                    resultTextIndex = 1;

                                resultLog += "모든 영웅 유닛 보유\n";
                                break;
                            }

                            RogueUnitDataBase pick = heroes[RogueLikeData.Instance.GetRandomInt(0, heroes.Count)];
                            RogueUnitDataBase newUnit = UnitLoader.Instance.GetCloneUnitById(pick.idx);
                            if (newUnit == null)
                                break;

                            RogueLikeData.Instance.AddMyTeam(newUnit);
                            string heroName = GameTextDB.GetByForeignKey(TextKind.Unit, newUnit.idx);
                            if (string.IsNullOrEmpty(heroName))
                                heroName = newUnit.unitName;

                            resultLog += $"'{origin.unitName}' 희생 → '{heroName}' 획득\n";
                            PushResultToken(resultTokens, heroName);
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
                                        WarRelic relic = RelicManager.HandleRandomRelic(1, RelicAction.Acquire);
                                        string name = GetRelicDisplayName(relic);
                                        if (!string.IsNullOrEmpty(name))
                                        {
                                            resultLog += $"- 일반 유산 '{name}'\n";
                                            PushResultToken(resultTokens, name);
                                        }
                                        break;
                                    }

                                case 1:
                                    {
                                        WarRelic relic = RelicManager.HandleRandomRelic(0, RelicAction.Acquire);
                                        string name = GetRelicDisplayName(relic);
                                        if (!string.IsNullOrEmpty(name))
                                        {
                                            resultLog += $"- 저주 유산 '{name}'\n";
                                            PushResultToken(resultTokens, name);
                                        }
                                        break;
                                    }

                                case 2:
                                    {
                                        List<RogueUnitDataBase> myTeam = RogueLikeData.Instance.GetMyTeam();
                                        if (myTeam.Count == 0)
                                            break;

                                        RogueUnitDataBase unit = myTeam[RogueLikeData.Instance.GetRandomInt(0, myTeam.Count)];
                                        RogueUnitDataBase promoted = RogueUnitDataBase.RandomUnitReForm(unit);
                                        if (promoted == null)
                                            break;

                                        int unitIndex = myTeam.IndexOf(unit);
                                        if (unitIndex >= 0)
                                            myTeam[unitIndex] = promoted;

                                        RogueLikeData.Instance.SetMyTeam(myTeam);
                                        string originName = GameTextDB.GetByForeignKey(TextKind.Unit, unit.idx);
                                        string promotedName = GameTextDB.GetByForeignKey(TextKind.Unit, promoted.idx);
                                        resultLog += $"- '{originName}' → '{promotedName}'\n";
                                        PushRequireToken(originName);
                                        PushResultToken(resultTokens, promotedName);
                                        break;
                                    }

                                case 3:
                                    {
                                        List<RogueUnitDataBase> candidates = RogueLikeData.Instance.GetMyTeam()
                                            .Where(unit => unit != null && unit.Energy > 1)
                                            .ToList();

                                        if (candidates.Count == 0)
                                            break;

                                        RogueUnitDataBase target = candidates[RogueLikeData.Instance.GetRandomInt(0, candidates.Count)];
                                        target.SetEnergyDirect(1);
                                        string name = GameTextDB.GetByForeignKey(TextKind.Unit, target.idx);
                                        resultLog += $"- '{name}' 기력 1\n";
                                        PushResultToken(resultTokens, name);
                                        break;
                                    }

                                case 4:
                                    {
                                        List<RogueUnitDataBase> candidates = UnitLoader.Instance.GetAllCachedUnits()
                                            .Where(unit => unit != null && unit.rarity >= 1 && unit.rarity <= 3)
                                            .ToList();
                                        RemoveCommonUnitChoicesIfNeeded(candidates);

                                        if (candidates.Count == 0)
                                            break;

                                        int randomIndex = rarity1Rate > 0f
                                            ? PickIndexWithRarity1Penalty(candidates, rarity1Rate)
                                            : RogueLikeData.Instance.GetRandomInt(0, candidates.Count);

                                        RogueUnitDataBase pick = candidates[randomIndex];
                                        RogueUnitDataBase newUnit = UnitLoader.Instance.GetCloneUnitById(pick.idx);
                                        if (newUnit == null)
                                            break;

                                        RogueLikeData.Instance.AddMyTeam(newUnit);
                                        string name = GameTextDB.GetByForeignKey(TextKind.Unit, newUnit.idx);
                                        if (string.IsNullOrEmpty(name))
                                            name = newUnit.unitName;

                                        resultLog += $"- 유닛 '{name}' 획득\n";
                                        PushResultToken(resultTokens, name);
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
                                        int changedMorale = RogueLikeData.Instance.ChangeMorale(-25);
                                        resultLog += $"- 사기 {Mathf.Abs(changedMorale)} 감소\n";
                                        PushResultToken(resultTokens, $"사기 {Mathf.Abs(changedMorale)} 감소");
                                        break;
                                    }

                                case 8:
                                    {
                                        int gold = RogueLikeData.Instance.AddGoldByEventChapter(150);
                                        resultLog += $"- 금화 {gold} 획득\n";
                                        PushResultToken(resultTokens, $"금화 {gold}");
                                        break;
                                    }
                            }

                            break;
                        }

                        if (choiceData.choiceId == 25)
                        {
                            RogueUnitDataBase unit = GetSelectedUnit(selectedUnits);
                            if (unit == null)
                                break;

                            int gold = unit.rarity == 1
                                ? RogueLikeData.Instance.AddGoldByEventChapter(50)
                                : RogueLikeData.Instance.AddGoldByEventChapter(150);

                            string unitName = GameTextDB.GetByForeignKey(TextKind.Unit, unit.idx);
                            if (string.IsNullOrEmpty(unitName))
                                unitName = unit.unitName;

                            resultLog += $"'{unitName}' 희생 → 금화 {gold}\n";
                            PushResultToken(resultTokens, $"금화 {gold}");

                            if (unit.rarity == 3)
                            {
                                WarRelic relic = RelicManager.HandleRandomRelic(1, RelicAction.Acquire);
                                string relicName = GetRelicDisplayName(relic);
                                if (!string.IsNullOrEmpty(relicName))
                                {
                                    resultLog += $"+ 전쟁 유산 '{relicName}'\n";
                                    PushResultToken(resultTokens, relicName);
                                }
                            }

                            break;
                        }

                        if (choiceData.choiceId == 37)
                        {
                            bool success = RogueLikeData.Instance.GetRandomFloat() < 0.5f;
                            resultTextIndex = success ? 0 : 1;

                            if (success)
                            {
                                WarRelic relic = RelicManager.HandleRandomRelic(5, RelicAction.Acquire);
                                string relicName = GetRelicDisplayName(relic);
                                if (!string.IsNullOrEmpty(relicName))
                                {
                                    resultLog += $"'{relicName}' 획득\n";
                                    PushResultToken(resultTokens, relicName);
                                }
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
                        int relicCount = Mathf.Max(1, SafeParseInt(count));

                        for (int k = 0; k < relicCount; k++)
                        {
                            WarRelic relic = RelicManager.HandleRandomRelic(0, RelicAction.Acquire);
                            string name = GetRelicDisplayName(relic);
                            if (string.IsNullOrEmpty(name))
                                continue;

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
                                bool noBattle = RogueLikeData.Instance.GetRandomFloat() < 0.5f;
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
                        string trainingValue = GetTrainingValue(choiceData, i, value);
                        int trainingCount = Mathf.Max(1, SafeParseInt(count));
                        bool useRandom = string.IsNullOrWhiteSpace(trainingValue) ||
                                         trainingValue == "-1" ||
                                         (form == ResultForm.Random && trainingValue == "0");

                        List<int> targetBranches = null;
                        if (!useRandom)
                        {
                            targetBranches = trainingValue
                                .Split(',')
                                .Select(part => int.TryParse(part, out int branch) ? branch : -1)
                                .Where(branch => branch >= 0 && branch < 8)
                                .ToList();

                            if (targetBranches.Count == 0)
                                useRandom = true;
                        }

                        int appliedTrainingCount = 0;
                        for (int k = 0; k < trainingCount; k++)
                        {
                            if (useRandom)
                            {
                                RogueLikeData.Instance.IncreaseRandomUpgrade(false);
                            }
                            else
                            {
                                int branch = targetBranches[RogueLikeData.Instance.GetRandomInt(0, targetBranches.Count)];
                                bool isAttack = RogueLikeData.Instance.GetRandomFloat() < 0.5f;
                                RogueLikeData.Instance.IncreaseUpgrade(branch, isAttack, false);
                            }

                            appliedTrainingCount++;
                            PushResultToken(resultTokens, "전술 개량");
                        }

                        resultLog += appliedTrainingCount > 0
                            ? $"병종 강화 {appliedTrainingCount}회 적용\n"
                            : "병종 강화 적용\n";
                        break;
                    }

                case ResultType.Field:
                    {
                        int fieldId = SafeParseInt(value);
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

            if (GameManager.Instance != null)
                GameManager.Instance.OpenBattlePanel();
        }
        return (resultLog, isBattle);
    }


    private static bool IsUnitVictory(RogueUnitDataBase unit)
    {
        if (unit.Mobility >= 12f)
            return true;

        float winChance = unit.Mobility * 0.09f;
        return RogueLikeData.Instance.GetRandomFloat() < winChance;
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

        return RogueLikeData.Instance.GetRandomFloat() < winChance;
    }
    public static (int min, int max) ParseRange(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (int.MinValue, int.MaxValue);

        string[] parts = value.Split('~');
        if (parts.Length == 2 &&
            int.TryParse(parts[0], out int min) &&
            int.TryParse(parts[1], out int max))
        {
            return min <= max ? (min, max) : (max, min);
        }

        return int.TryParse(value, out int minimum)
            ? (minimum, int.MaxValue)
            : (int.MinValue, int.MaxValue);
    }

    private static bool InRange(int actual, string value, bool useMinBound = true, float multiplier = 1f)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (value.Contains("~"))
        {
            var (min, max) = ParseRange(value);
            return actual >= min && actual <= max;
        }

        if (!int.TryParse(value, out int baseValue))
            return false;

        int threshold = Mathf.CeilToInt(baseValue * multiplier);
        return useMinBound ? actual >= threshold : actual <= threshold;
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
        if (RogueLikeData.Instance == null || StagePresetLoader.I == null || StagePresetLoader.I.presets == null)
            return;

        int chapter = RogueLikeData.Instance.GetChapter();
        int level = RogueLikeData.Instance.GetCurrentStageX();
        string targetStageType;

        switch (grade)
        {
            case 1:
                RogueLikeData.Instance.SetStageType(StageType.Combat);
                targetStageType = "normal";
                break;

            case 5:
                RogueLikeData.Instance.SetStageType(StageType.Elite);
                targetStageType = "elite";
                break;

            case 20:
                RogueLikeData.Instance.SetCurrentStage(15, 1, StageType.Boss);
                targetStageType = "boss";
                break;

            default:
                Debug.LogWarning($"지원하지 않는 이벤트 전투 등급입니다. grade={grade}");
                return;
        }

        List<StagePreset> candidates = StagePresetLoader.I.presets
            .Where(p => p.Chapter == chapter && p.StageType == targetStageType)
            .ToList();

        if (grade == 1)
            candidates = candidates.Where(p => p.Level == level).ToList();

        if (candidates.Count == 0)
        {
            Debug.LogWarning($"이벤트 전투 프리셋을 찾지 못했습니다. chapter={chapter}, type={targetStageType}, level={level}");
            return;
        }

        StagePreset preset = candidates[RogueLikeData.Instance.GetRandomInt(0, candidates.Count)];
        RogueLikeData.Instance.SetPresetID(preset.PresetID);
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

        string line = NormalizeEventNarrationText(
            GameTextDB.GetExact(TextKind.EventDesc, requestedTitleKey, foreignKey));

        if (string.IsNullOrEmpty(line) && choiceData.resultText != null && choiceData.resultText.Count > 0)
        {
            int safeIndex = Mathf.Clamp(resultTextIndex, 0, choiceData.resultText.Count - 1);
            line = NormalizeEventNarrationText(choiceData.resultText[safeIndex]);
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

        string narration = GameTextDB.ComposeLines(
            new List<string>(1) { line },
            requireNames,
            resultTokens,
            textTokens);

        return NormalizeEventNarrationText(narration);
    }

    [Serializable]
    private sealed class EventNarrationArrayWrapper
    {
        public string[] values;
    }

    // 사용처: GameTextDB에서 ["문장"] 형태로 저장된 이벤트 결과 문장을 실제 출력 문자열로 복원한다.
    private static string NormalizeEventNarrationText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        string normalized = text.Trim();
        for (int i = 0; i < 2; i++)
            normalized = normalized.Replace("\\\"", "\"");

        while (HasOuterNarrationQuotes(normalized))
            normalized = normalized.Substring(1, normalized.Length - 2).Trim();

        if (normalized.Length < 2 || normalized[0] != '[' || normalized[normalized.Length - 1] != ']')
            return normalized;

        try
        {
            EventNarrationArrayWrapper wrapper = JsonUtility.FromJson<EventNarrationArrayWrapper>(
                "{\"values\":" + normalized + "}");

            if (wrapper == null || wrapper.values == null || wrapper.values.Length == 0)
                return normalized;

            return string.Join("\n", wrapper.values.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()));
        }
        catch
        {
            return normalized;
        }
    }

    // 사용처: 이벤트 결과 문자열의 저장용 외곽 큰따옴표만 제거한다.
    private static bool HasOuterNarrationQuotes(string text)
    {
        return !string.IsNullOrEmpty(text) &&
               text.Length >= 2 &&
               text[0] == '\"' &&
               text[text.Length - 1] == '\"';
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
