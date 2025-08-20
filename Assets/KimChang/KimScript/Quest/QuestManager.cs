using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public static class QuestManager
{
    private static Dictionary<int, QuestClass> questList = new();

    public static void LoadQuestData()
    {
        questList = QuestLoader.Load();
    }
    public static QuestClass GetQuest(int id)
    {
        return questList[id];
    }
    public static void AcceptQuest(int id)
    {
        QuestClass quest = questList[id].Clone();
        RogueLikeData.Instance.AddQuest(quest);
    }
    public static void AddQuestRequire(int id, RequireThing requireThing ,int count)
    {
        Dictionary<int, QuestClass> quest = RogueLikeData.Instance.GetQuestList();
        if (quest[id] != null)
        {
            int index = quest[id].requireThing.FindIndex(f => f == requireThing);
            if (index == -1) return;
            quest[id].requireCount[index] += count; 
        }
    }
    public static bool CheckQuestRequire(int id)
    {
        if (questList.ContainsKey(id) && questList[id].questClear) return true;

        int clearCount = questList[id].requireThing.Count;
        for (int i = 0; i < questList[id].requireThing.Count; i++)
        {
            if (questList[id].requireCount[i] == 0)
            {
                clearCount--;
                if (clearCount == 0) questList[id].questClear = true;
            }
        }
        return questList[id].questClear;
    }

}
