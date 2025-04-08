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
    public int[] chapters; // 등장 가능한 챕터 목록

    public GameEvent(int id, string title, string description, EventType type, string[] choices, int[] resultIds, int[] chapters)
    {
        this.id = id;
        this.title = title;
        this.description = description;
        this.type = type;
        this.choices = choices;
        this.resultIds = resultIds;
        this.chapters = chapters;
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