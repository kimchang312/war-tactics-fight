using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

public static class SaveSystem
{
    private static string GetPath(string fileName = "stage_save.json")
        => Path.Combine(Application.persistentDataPath, fileName);

    public static void SaveStageFull(Dictionary<string, StageNode> allNodes, string fileName = "stage_save.json")
    {
        try
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
                stageType = node.stageType, // enum 그대로 저장
                presetID = node.presetID,
                connections = node.connectedNodes
                    .ConvertAll(n => new StageConnectionData { level = n.level, row = n.row })
            };
            data.allNodes.Add(entry);
        }

        // RogueLikeData에서 현재 플레이어 위치 가져와서 저장
        var currentStage = RogueLikeData.Instance.GetCurrentStage();
        data.currentLevel = currentStage.x;
        data.currentRow = currentStage.y;
        data.currentStageType = currentStage.type;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(fileName), json);
        Debug.Log($"📁 전체 맵 저장 완료: {GetPath(fileName)}");
        Debug.Log($"📍 플레이어 위치 저장: Level {currentStage.x}, Row {currentStage.y}, Type {currentStage.type}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"맵 저장 실패: {ex.Message}");
        }
    }

    public static StageFullSaveData LoadFull(string fileName = "stage_save.json")
    {
        try
        {
            string path = GetPath(fileName);
            if (!File.Exists(path))
            {
                Debug.Log("⛔ 저장된 전체 맵 정보 없음");
                return null;
            }
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<StageFullSaveData>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"맵 불러오기 실패: {ex.Message}");
            return null;
        }
    }

    public static void Clear(string fileName = "stage_save.json")
    {
        try
        {
            string path = GetPath(fileName);
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex)
        {
            Debug.LogError($"맵 삭제 실패: {ex.Message}");
        }
    }
}