using System;
using System.Collections.Generic;

[Serializable]
public class StageFullSaveData
{
    public int chapter;
    public List<StageNodeSaveEntry> allNodes = new();
    public List<SpecialPresetSaveEntry> specialPresetUnits = new();
    
    // 플레이어의 현재 위치 (불러오기 시 복원용)
    public int currentLevel = -1;
    public int currentRow = -1;
    public StageType currentStageType = StageType.Unknown;

    public Dictionary<int, List<int>> GetSpecialPresetData()
    {
        var data = new Dictionary<int, List<int>>();
        if (specialPresetUnits == null)
            return data;

        foreach (SpecialPresetSaveEntry entry in specialPresetUnits)
        {
            if (entry == null)
                continue;

            data[entry.presetID] = entry.unitIds != null
                ? new List<int>(entry.unitIds)
                : new List<int>();
        }

        return data;
    }

    public void SetSpecialPresetData(Dictionary<int, List<int>> data)
    {
        specialPresetUnits = new List<SpecialPresetSaveEntry>();
        if (data == null)
            return;

        foreach (var pair in data)
        {
            specialPresetUnits.Add(new SpecialPresetSaveEntry
            {
                presetID = pair.Key,
                unitIds = pair.Value != null ? new List<int>(pair.Value) : new List<int>()
            });
        }
    }
}

[Serializable]
public class SpecialPresetSaveEntry
{
    public int presetID;
    public List<int> unitIds = new();
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
