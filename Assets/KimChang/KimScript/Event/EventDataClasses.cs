
using System;
using System.Collections.Generic;

[Serializable]
public class EventData
{
    public int eventId;
    public string eventName;
    public List<int> eventChapter;
    public string description;
    public List<int> choiceIds;

    public string requireCondition; // 조건 설명
    public RequireThing requireThing;
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
    public List<ResultType> resultType;
    public List<ResultForm> resultForm;
    public List<string> resultValue;
    public List<string> resultCount;
}

public enum RequireThing
{
    None,
    Gold,
    Morale,
    Unit,
    Relic,
    Energy,   // 기력 조건용
    Stage,    // 지역 조건용
    Special,   // 복합조건, 특수조건
    AttackDamage
}

public enum RequireForm { None, Count, Select, Special,Random }
public enum ResultType { None, Gold, Morale, Energy, Unit, Relic,Training, Special,Change,Battle,Field }
public enum ResultForm { None, Random, Select,Special,All }
