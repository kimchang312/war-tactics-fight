using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[Serializable]
public class EventData
{
    public int eventId;
    public string eventName;
    public List<int> eventChapter;
    public string description;
    public List<int> choiceIds;
    public string requireCondition;

    public List<RequireThing> requireThing;

    public List<RequireForm> requireForm;

    public List<string> requireValue;
    public List<string> requireCount;
}

[Serializable]
public class EventChoiceData
{
    public int choiceId;
    public int eventId;
    public string choiceText;
    public string resultDescription; // 기존 호환용

    public List<RequireThing> requireThing;
    public List<RequireForm> requireForm;
    public List<string> requireValue;
    public List<string> requireCount;

    public List<ResultType> resultType;
    public List<ResultForm> resultForm;
    public List<string> resultValue;
    public List<string> resultCount;

    // 새 필드
    public List<string> choiceResultText = new();
    public List<string> resultText = new();
}


[JsonConverter(typeof(StringEnumConverter))]
public enum RequireThing
{
    None,
    Gold,
    Morale,
    Unit,
    Relic,
    Energy,
    Stage,
    Special,
    AttackDamage
}

[JsonConverter(typeof(StringEnumConverter))]
public enum RequireForm { None, Select, Special, Random }

[JsonConverter(typeof(StringEnumConverter))]
public enum ResultType { None, Gold, Morale, Energy, Unit, Relic, Training, Special, Change, Battle, Field }

[JsonConverter(typeof(StringEnumConverter))]
public enum ResultForm { None, Random, Select, Special, All }
