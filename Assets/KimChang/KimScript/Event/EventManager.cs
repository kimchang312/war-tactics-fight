using System.Collections.Generic;
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
        if (eventData.requireThing == RequireThing.Special || eventData.requireForm == RequireForm.Special)
        {
            return CheckSpecialRequire(eventData);
        }

        // 일반 조건 처리
        return CheckRequireCondition(eventData.requireThing, eventData.requireForm, eventData.requireValue);
    }
    private static bool CheckRequireCondition(RequireThing thing, RequireForm form, string value)
    {
        switch (thing)
        {
            case RequireThing.Gold:
                return RogueLikeData.Instance.GetCurrentGold() >= int.Parse(value);
            case RequireThing.Morale:
                return RogueLikeData.Instance.GetMorale() >= int.Parse(value);
            case RequireThing.Unit:
                return RogueLikeData.Instance.GetMyUnits().Count >= int.Parse(value);
            case RequireThing.Relic:
                return RogueLikeData.Instance.GetAllOwnedRelics().Count >= int.Parse(value);
        }
        return true;
    }

    private static bool CheckSpecialRequire(EventData eventData)
    {
        // 예시) 사기 21~50 사이
        if (eventData.requireValue.Contains("~"))
        {
            string[] split = eventData.requireValue.Split('~');
            int minValue = int.Parse(split[0]);
            int maxValue = int.Parse(split[1]);

            if (eventData.requireThing == RequireThing.Morale)
            {
                int morale = RogueLikeData.Instance.GetMorale();
                return morale >= minValue && morale <= maxValue;
            }
            if (eventData.requireThing == RequireThing.Gold)
            {
                int gold = RogueLikeData.Instance.GetCurrentGold();
                return gold >= minValue && gold <= maxValue;
            }
        }

        // 그 외 조건 생기면 추가
        return false;
    }

}
