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

    // 사용처: 이벤트 제목/설명을 GameTextDB에서 조회할 때 사용
    public string gameTextKindTitle;
    public string gameTextKindDesc;
    public int gameTextForeignKey;
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

    public List<string> choiceResultText = new();
    public List<string> resultText = new();

    // 사용처: 선택지/결과/성공/실패 텍스트를 GameTextDB에서 조회할 때 사용
    public string gameTextKind;
    public int gameTextForeignKey;
    public int gameTextTitleKey_choiceText;
    public int gameTextTitleKey_positive;
    public int gameTextTitleKey_negative;
    public int gameTextTitleKey_resultDescription;
    public int gameTextTitleKey_resultTextBase;
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
public enum ResultType
{
    None,
    Gold,
    Morale,
    Energy,
    Unit,
    Relic,
    Training,
    Special,
    Change,
    Battle,
    Field,
    Curse
}

[JsonConverter(typeof(StringEnumConverter))]
public enum ResultForm { None, Random, Select, Special, All }
