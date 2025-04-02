
using System;

[Serializable]
public class GameEvent
{
    public int id;
    public string title;
    public string description;
    public EventType type;
    public string[] choices;
    public int[] resultIds;

    public GameEvent(int id, string title, string description, EventType type, string[] choices, int[] resultIds)
    {
        this.id = id;
        this.title = title;
        this.description = description;
        this.type = type;
        this.choices = choices;
        this.resultIds = resultIds;
    }
}

public enum EventType
{
    Normal,
    Battle,
    Reward,
    Shop,
    Special
}
