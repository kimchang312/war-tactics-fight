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
                NormalizeEventData(data);
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
                NormalizeChoiceData(choice);
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

        if (token is not JArray array)
            return list;

        foreach (var item in array)
        {
            string raw = item?.ToString();

            if (string.IsNullOrWhiteSpace(raw))
                continue;

            if (TryParseEnumToken(raw, out T value))
            {
                list.Add(value);
            }
            else
            {
                Debug.LogError($"EventDataLoader: enum 파싱 실패. Type={typeof(T).Name}, Value={raw}");
            }
        }

        return list;
    }

    private static bool TryParseEnumToken<T>(string raw, out T value) where T : struct
    {
        if (typeof(T) == typeof(ResultType) && raw == "Curse")
        {
            object boxed = ResultType.Curse;
            value = (T)boxed;
            return true;
        }

        return Enum.TryParse(raw, true, out value);
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
    private static void PadList<T>(List<T> list, int targetCount, T defaultValue)
    {
        if (list == null)
            return;

        while (list.Count < targetCount)
            list.Add(defaultValue);
    }

    private static void NormalizeEventData(EventData data)
    {
        if (data.requireThing == null) data.requireThing = new List<RequireThing>();
        if (data.requireForm == null) data.requireForm = new List<RequireForm>();
        if (data.requireValue == null) data.requireValue = new List<string>();
        if (data.requireCount == null) data.requireCount = new List<string>();

        int requireCount = data.requireThing.Count;

        PadList(data.requireForm, requireCount, RequireForm.None);
        PadList(data.requireValue, requireCount, string.Empty);
        PadList(data.requireCount, requireCount, string.Empty);

        if (data.gameTextForeignKey == 0 && data.eventId != 0)
            data.gameTextForeignKey = data.eventId;
    }

    private static void NormalizeChoiceData(EventChoiceData choice)
    {
        if (choice.requireThing == null) choice.requireThing = new List<RequireThing>();
        if (choice.requireForm == null) choice.requireForm = new List<RequireForm>();
        if (choice.requireValue == null) choice.requireValue = new List<string>();
        if (choice.requireCount == null) choice.requireCount = new List<string>();

        if (choice.resultType == null) choice.resultType = new List<ResultType>();
        if (choice.resultForm == null) choice.resultForm = new List<ResultForm>();
        if (choice.resultValue == null) choice.resultValue = new List<string>();
        if (choice.resultCount == null) choice.resultCount = new List<string>();

        if (choice.choiceResultText == null) choice.choiceResultText = new List<string>();
        if (choice.resultText == null) choice.resultText = new List<string>();

        int requireCount = choice.requireThing.Count;
        PadList(choice.requireForm, requireCount, RequireForm.None);
        PadList(choice.requireValue, requireCount, string.Empty);
        PadList(choice.requireCount, requireCount, string.Empty);

        int resultCount = choice.resultType.Count;
        PadList(choice.resultForm, resultCount, ResultForm.None);
        PadList(choice.resultValue, resultCount, string.Empty);
        PadList(choice.resultCount, resultCount, string.Empty);

        if (choice.gameTextForeignKey == 0 && choice.choiceId != 0)
            choice.gameTextForeignKey = choice.choiceId;
    }
}
