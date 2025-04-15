using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
        if (eventData.requireThing == RequireThing.None)
            return true;

        // Special 조건 처리
        if (eventData.requireThing == RequireThing.Special)
        {
            return CheckSpecialRequire(eventData);
        }

        // 일반 조건 처리
        return CheckRequireCondition(eventData.requireThing, eventData.requireForm, eventData.requireValue);
    }
    private static bool CheckRequireCondition(RequireThing thing, RequireForm form, string value,int id=-1)
    {
        switch (thing)
        {
            case RequireThing.Gold:
                if (form == RequireForm.None)
                    return RogueLikeData.Instance.GetCurrentGold() >= int.Parse(value);
                break;
            case RequireThing.Morale:
                if (form == RequireForm.None)
                    return RogueLikeData.Instance.GetMorale() >= int.Parse(value);

                if (form == RequireForm.Special)
                {
                    // 예: 35,-50  → Morale >= 35 && <= 50
                    if (value.Contains(","))
                    {
                        string[] parts = value.Split(',');
                        int min = int.Parse(parts[0]);
                        int max = int.Parse(parts[1].Replace("-", "")); // -붙여서 넣었으니까 제거

                        int morale = RogueLikeData.Instance.GetMorale();
                        return morale >= min && morale <= max;
                    }
                }
                break;

            case RequireThing.Unit:
                if (form == RequireForm.Count || form == RequireForm.Select)
                {
                    if(id == 47)
                    {
                        foreach (var unit in RogueLikeData.Instance.GetMyUnits())
                        {
                            if (unit.energy >=2)
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                    else if(id == 52)
                    {
                        foreach (var unit in RogueLikeData.Instance.GetMyUnits())
                        {
                            if (unit.attackDamage >= 90)
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                    return RogueLikeData.Instance.GetMyUnits().Count >= int.Parse(value);
                }
                else if (form == RequireForm.Special)
                {
                    // 예: 1~2,5 → 희귀도 1~2 유닛이 5개 이상
                    if (value.Contains("~") && value.Contains(","))
                    {
                        string[] parts = value.Split(',');
                        string rarityRange = parts[0];  // 1~2
                        int requiredCount = int.Parse(parts[1]);

                        int minRarity = int.Parse(rarityRange.Split('~')[0]);
                        int maxRarity = int.Parse(rarityRange.Split('~')[1]);

                        int count = 0;
                        foreach (var unit in RogueLikeData.Instance.GetMyUnits())
                        {
                            if (unit.rarity >= minRarity && unit.rarity <= maxRarity)
                            {
                                count++;
                            }
                        }

                        return count >= requiredCount;
                    }
                }
                break;

            case RequireThing.Relic:
                if (form == RequireForm.Count) // 특정 등급 유산 존재 여부 검사
                {
                    int relicGrade = int.Parse(value);
                    return RogueLikeData.Instance.GetAllOwnedRelics().Exists(relic => relic.grade == relicGrade);
                }
                else if (form == RequireForm.Special) // 특정 id 유산이 없어야 true
                {
                    int relicId = int.Parse(value);
                    return !RogueLikeData.Instance.GetAllOwnedRelics().Exists(relic => relic.id == relicId);
                }
                break;
            case RequireThing.Energy:
                if (form == RequireForm.None)
                    return RogueLikeData.Instance.GetMyUnits().Exists(u => u.energy >= int.Parse(value));
                if (form == RequireForm.Count)
                {
                    string[] split = value.Split(',');
                    int targetEnergy = int.Parse(split[0]);
                    int targetCount = int.Parse(split[1]);

                    int count = 0;
                    foreach (var unit in RogueLikeData.Instance.GetMyUnits())
                    {
                        if (unit.energy == targetEnergy)
                            count++;
                    }
                    return count >= targetCount;
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
        // 조건 없음 = 무조건 선택 가능
        if (eventChoiceData.requireThing == RequireThing.None)
            return true;

        // 복잡 조건 처리
        if (eventChoiceData.requireThing == RequireThing.Special || eventChoiceData.requireForm == RequireForm.Special)
            return CheckSpecialChoiceRequire(eventChoiceData);

        // 일반 조건 처리
        return CheckRequireCondition(eventChoiceData.requireThing, eventChoiceData.requireForm, eventChoiceData.requireValue);
    }

    private static bool CheckSpecialChoiceRequire(EventChoiceData eventChoiceData)
    {

        return false;
    }

}
