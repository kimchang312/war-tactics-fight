using System.Collections.Generic;
using UnityEngine;

public class QuestLoader
{
    private static Dictionary<int, QuestClass> questDict;

    public static Dictionary<int, QuestClass> Load()
    {
        // 이미 로드된 경우 캐시 반환
        if (questDict != null)
            return questDict;

        TextAsset json = Resources.Load<TextAsset>("JsonData/QuestDataBase");
        if (json == null)
        {
            Debug.LogError("QuestDataBase.json 파일을 찾을 수 없습니다.");
            return new Dictionary<int, QuestClass>();
        }

        List<QuestClass> questList = JsonUtilityWrapper.FromJsonItemList<QuestClass>(json.text);

        // List → Dictionary로 변환
        questDict = new Dictionary<int, QuestClass>();
        foreach (QuestClass quest in questList)
        {
            if (!questDict.ContainsKey(quest.id))
            {
                questDict.Add(quest.id, quest);
            }
            else
            {
                Debug.LogWarning($"중복된 퀘스트 ID 발견: {quest.id}. 기존 값을 덮어쓰지 않습니다.");
            }
        }

        return questDict;
    }
}
