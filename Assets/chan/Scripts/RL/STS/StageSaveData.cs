using System;
using System.Collections.Generic;

[Serializable]
public class StageFullSaveData
{
    public int chapter;
    public List<StageNodeSaveEntry> allNodes = new();
}

[Serializable]
public class StageNodeSaveEntry
{
    public int level;
    public int row;
    public StageType stageType;
    public int presetID;
    public List<StageConnectionData> connections = new();
}

[Serializable]
public class StageConnectionData
{
    public int level;
    public int row;
}