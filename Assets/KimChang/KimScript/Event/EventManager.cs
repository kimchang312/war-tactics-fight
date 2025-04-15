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
        return CheckRequireCondition(eventData.requireThing, eventData.requireForm,"", eventData.requireValue);
    }
    private static bool CheckRequireCondition(RequireThing thing, RequireForm form, string value,string count)
    {
        switch (thing)
        {
            case RequireThing.Gold:
                if (form == RequireForm.None)
                    return RogueLikeData.Instance.GetCurrentGold() >= int.Parse(count);
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

}
