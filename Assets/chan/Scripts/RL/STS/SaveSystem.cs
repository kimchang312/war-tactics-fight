using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

public static class SaveSystem
{
    private static readonly string path = Application.persistentDataPath + "/stage_save.json";

    public static void SaveStageFull(Dictionary<string, StageNode> allNodes)
    {
        StageFullSaveData data = new()
        {
            chapter = RogueLikeData.Instance.GetChapter(),
            allNodes = new List<StageNodeSaveEntry>()
        };

        foreach (var node in allNodes.Values)
        {
            StageNodeSaveEntry entry = new()
            {
                level = node.level,
                row = node.row,
                stageType = node.stageType.ToString(),
                presetID = node.presetID,
                connections = node.connectedNodes
                    .Select(n => new StageConnectionData { level = n.level, row = n.row })
                    .ToList()
            };
            data.allNodes.Add(entry);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"📁 전체 맵 저장 완료: {path}");
    }

    public static StageFullSaveData LoadFull()
    {
        if (!File.Exists(path))
        {
            Debug.Log("⛔ 저장된 전체 맵 정보 없음");
            return null;
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<StageFullSaveData>(json);
    }

    public static void Clear()
    {
        if (File.Exists(path))
            File.Delete(path);
    }
}

[Serializable]
public class StageFullSaveData
{
    public int chapter;
    public List<StageNodeSaveEntry> allNodes;
}

[Serializable]
public class StageNodeSaveEntry
{
    public int level;
    public int row;
    public string stageType;
    public int presetID;
    public List<StageConnectionData> connections;
}