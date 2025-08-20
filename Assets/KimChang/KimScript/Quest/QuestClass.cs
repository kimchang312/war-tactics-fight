using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[Serializable]
public class QuestClass
{
    public int id;
    public QuestDataType questDataType;
    public List<RequireThing> requireThing;
    public List<RequireType> requireType;
    public List<RequireForm> requireForm;
    public List<string> requireValue;
    public List<int> requireCount;

    public List<ResultType> resultType;
    public List<ResultForm> resultForm;
    public List<string> resultVaule;
    public List<int> resultCount;
    public bool questClear;
    public QuestClass Clone()
    {
        return new QuestClass
        {
            id = this.id,
            questDataType = this.questDataType,
            requireThing = this.requireThing,
            requireForm = this.requireForm,
            requireValue = this.requireValue,
            requireCount = this.requireCount,
            resultType = this.resultType,
            resultForm = this.resultForm,
            resultVaule = this.resultVaule,
            resultCount = this.resultCount,
            questClear = this.questClear
        };
    }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum QuestDataType
{
    None,
    AllData,
    SpotData,
}
[JsonConverter(typeof(StringEnumConverter))]
public enum RequireType
{
    None,
    Over,
    Equal,
    Under,
}