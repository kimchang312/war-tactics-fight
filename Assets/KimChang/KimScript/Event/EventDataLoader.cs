using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

public static class EventDataLoader
{
    public static Dictionary<int, EventData> EventDataDict { get; private set; } = new();
    public static Dictionary<int, EventChoiceData> EventChoiceDataDict { get; private set; } = new();

    public static void LoadData()
    {
        EventDataDict.Clear();
        EventChoiceDataDict.Clear();

        TextAsset eventDataJson = Resources.Load<TextAsset>("JsonData/EventDataBase");
        TextAsset eventChoiceDataJson = Resources.Load<TextAsset>("JsonData/EventChoiceDataBase");

        if (eventDataJson == null || eventChoiceDataJson == null)
        {
            Debug.LogError("EventDataLoader: Resources/JsonData/ 경로에 Json 파일이 없습니다.");
            return;
        }

        try
        {
            var eventList = JsonConvert.DeserializeObject<List<EventData>>(eventDataJson.text);
            foreach (var data in eventList)
            {
                EventDataDict[data.eventId] = data;
            }

            // 직접 파싱: EventChoiceData는 enum List들이 문자열로 되어 있음
            var jArray = JArray.Parse(eventChoiceDataJson.text);
            foreach (var jToken in jArray)
            {
                var obj = (JObject)jToken;

                EventChoiceData choice = new EventChoiceData
                {
                    choiceId = obj["choiceId"]?.ToObject<int>() ?? -1,
                    eventId = obj["eventId"]?.ToObject<int>() ?? -1,
                    choiceText = obj["choiceText"]?.ToString(),
                    resultDescription = obj["resultDescription"]?.ToString(), // 호환 유지용

                    requireThing = ParseEnumList<RequireThing>(obj["requireThing"]),
                    requireForm = ParseEnumList<RequireForm>(obj["requireForm"]),
                    requireValue = ParseStringList(obj["requireValue"]),
                    requireCount = ParseStringList(obj["requireCount"]),

                    resultType = ParseEnumList<ResultType>(obj["resultType"]),
                    resultForm = ParseEnumList<ResultForm>(obj["resultForm"]),
                    resultValue = ParseStringList(obj["resultValue"]),
                    resultCount = ParseStringList(obj["resultCount"]),

                    // 새 필드 추가
                    choiceResultText = ParseStringList(obj["choiceResultText"]),
                    resultText = ParseStringList(obj["resultText"])
                };

                EventChoiceDataDict[choice.choiceId] = choice;

            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"EventDataLoader Error: {e.Message}\n{e.StackTrace}");
        }
    }

    private static List<T> ParseEnumList<T>(JToken token) where T : struct
    {
        var list = new List<T>();

        if (token is JArray array)
        {
            foreach (var item in array)
            {
                if (Enum.TryParse(item.ToString(), out T value))
                {
                    list.Add(value);
                }
            }
        }

        return list;
    }

    private static List<string> ParseStringList(JToken token)
    {
        var list = new List<string>();
        if (token is JArray array)
        {
            foreach (var item in array)
            {
                // null -> "", 문자열은 그대로
                list.Add(item.Type == JTokenType.Null ? "" : item.ToString());
            }
        }
        return list;
    }

}
