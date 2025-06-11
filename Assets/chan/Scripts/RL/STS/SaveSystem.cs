using UnityEngine;
using System.IO;

public static class SaveSystem
{
    private static readonly string path = Application.persistentDataPath + "/stage_save.json";

    public static void SaveStage(StageNode current)
    {
        StageSaveData data = new()
        {
            chapter = RogueLikeData.Instance.GetChapter(),
            level = current.level,
            row = current.row,
            stageType = current.stageType.ToString(),
            connections = new()
        };

        foreach (var next in current.connectedNodes)
        {
            data.connections.Add(new StageConnectionData
            {
                level = next.level,
                row = next.row
            });
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"📁 스테이지 저장 완료: {path}");
    }

    public static StageSaveData LoadStage()
    {
        if (!File.Exists(path))
        {
            Debug.Log("⛔ 저장된 스테이지 정보 없음");
            return null;
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<StageSaveData>(json);
    }

    public static void Clear()
    {
        if (File.Exists(path)) File.Delete(path);
    }
}