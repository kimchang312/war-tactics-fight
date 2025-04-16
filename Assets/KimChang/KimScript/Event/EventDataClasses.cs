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

    [JsonConverter(typeof(StringEnumConverter))]
    public RequireThing requireThing;

    [JsonConverter(typeof(StringEnumConverter))]
    public RequireForm requireForm;

    public string requireValue;
}

[Serializable]
public class EventChoiceData
{
    public int choiceId;
    public int eventId;
    public string choiceText;
    public string resultDescription;

    public List<RequireThing> requireThing;

    public List<RequireForm> requireForm;

    public List<string> requireValue;
    public List<string> requireCount;

    [JsonConverter(typeof(StringEnumConverter))]
    public List<ResultType> resultType;

    [JsonConverter(typeof(StringEnumConverter))]
    public List<ResultForm> resultForm;

    public List<string> resultValue;
    public List<string> resultCount;
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
public enum RequireForm { None, Count, Select, Special, Random }

[JsonConverter(typeof(StringEnumConverter))]
public enum ResultType { None, Gold, Morale, Energy, Unit, Relic, Training, Special, Change, Battle, Field }

[JsonConverter(typeof(StringEnumConverter))]
public enum ResultForm { None, Random, Select, Special, All }
