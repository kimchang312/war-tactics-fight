
using System.Collections.Generic;
using UnityEngine;

public static class EventDataLoader
{
    public static Dictionary<int, EventData> EventDataDict { get; private set; } = new();
    public static Dictionary<int, EventChoiceData> EventChoiceDataDict { get; private set; } = new();

    public static void LoadData()
    {
        EventDataDict.Clear();
        EventChoiceDataDict.Clear();

        TextAsset eventDataJson = Resources.Load<TextAsset>("EventDataBase/EventDataBase");
        TextAsset eventChoiceDataJson = Resources.Load<TextAsset>("EventDataBase/EventChoiceDataBase");

        if (eventDataJson == null || eventChoiceDataJson == null)
        {
            Debug.LogError("EventDataLoader: Resources/EventDataBase/ 경로에 Json 파일이 없습니다.");
            return;
        }

        var eventList = JsonUtility.FromJson<EventDataList>("{\"data\":" + eventDataJson.text + "}").data;
        var choiceList = JsonUtility.FromJson<EventChoiceDataList>("{\"data\":" + eventChoiceDataJson.text + "}").data;

        foreach (var data in eventList)
            EventDataDict[data.eventId] = data;

        foreach (var data in choiceList)
            EventChoiceDataDict[data.choiceId] = data;
    }
}

[System.Serializable]
public class EventDataList { public List<EventData> data; }
[System.Serializable]
public class EventChoiceDataList { public List<EventChoiceData> data; }
