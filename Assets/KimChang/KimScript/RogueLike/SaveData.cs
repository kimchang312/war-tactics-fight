using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static RogueLikeData;

[System.Serializable]
public class SavePlayerData 
{
    public int id;
    public RogueUnitDataBase[] myUnits;
    public Dictionary<RelicType, HashSet<int>> relicIdsByType = new();
    public int currentGold;
    public int earnedGold = 0;
    public int spentGold = 0;
    public int playerMorale;
    public int currentStageX;
    public int currentStageY;
    public StageType currentStageType;
    
    public SavePlayerData(int id ,List<RogueUnitDataBase> myUnits, int currentGold,int earnedGold,int spentGold,int playerMorale,int currentStageX,int currentStageY,StageType currentStageType)
    {
        this.id = id;
        this.myUnits = myUnits.ToArray();
        this.currentGold = currentGold;
        this.earnedGold=earnedGold;
        this.spentGold = spentGold;
        this.playerMorale = playerMorale;
        this.currentStageX = currentStageX;
        this.currentStageY = currentStageY;
        this.currentStageType = currentStageType;
    }
}

public class SaveData
{
    private string _filePath= Application.dataPath + "/KimChang/Json/PlayerData.json";
    private string _jsonData;

    public void SaveDataFile()
    {
        SavePlayerData savePlayerData = RogueLikeData.Instance.GetRogueLikeData();

        // 데이터를 JSON 문자열로 직렬화
        _jsonData = JsonUtility.ToJson(savePlayerData);
        Debug.Log(savePlayerData.myUnits[0].unitName);
        File.WriteAllText(_filePath, _jsonData);
    }

    public SavePlayerData LoadData()
    {
        // JSON 문자열을 객체로 역직렬화
        string jsonData = File.ReadAllText(_filePath); // 파일에서 읽기
        SavePlayerData savePlayerData = JsonUtility.FromJson<SavePlayerData>(jsonData);//역직렬화
        return savePlayerData;
    }
}
