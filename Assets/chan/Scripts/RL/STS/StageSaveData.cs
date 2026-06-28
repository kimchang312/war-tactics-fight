using System;
using System.Collections.Generic;

[Serializable]
public class StageFullSaveData
{
    public int chapter;
    public List<StageNodeSaveEntry> allNodes = new();
    
    // 플레이어의 현재 위치 (불러오기 시 복원용)
    public int currentLevel = -1;
    public int currentRow = -1;
    public StageType currentStageType = StageType.Unknown;
}

[Serializable]
public class StageNodeSaveEntry
{
    public int level;
    public int row;
    public StageType stageType;
    public int presetID;
    public BattlefieldEffect battlefieldEffect;
    public List<StageConnectionData> connections = new();
}

[Serializable]
public class StageConnectionData
{
    public int level;
    public int row;
}
