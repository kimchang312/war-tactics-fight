using System;
using System.Collections.Generic;

[Serializable]
public class StageSaveData
{
    public int chapter;
    public int level;
    public int row;
    public string stageType;
    public List<StageConnectionData> connections = new();
}

[Serializable]
public class StageConnectionData
{
    public int level;
    public int row;
}