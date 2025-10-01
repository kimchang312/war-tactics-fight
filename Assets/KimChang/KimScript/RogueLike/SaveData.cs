using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using static RogueLikeData;

[System.Serializable]
public class SavePlayerData 
{
    public int id;
    public List<RogueUnitDataBase> myUnits;
    public List<WarRelic> warRelics= new();
    public List<int> eventIds= new();
    public int currentGold;
    public int spentGold = 0;
    public int playerMorale;
    public int currentStageX;
    public int currentStageY;
    public int chapter;
    public StageType currentStageType;
    public UnitUpgrade[] unitUpgrades; 
    public int sariStack;
    public BattleRewardData battleReward;
    public int nextUniqueId;
    public int score;

    public SavePlayerData(int id ,List<RogueUnitDataBase> myUnits,List<WarRelic> warRelics, List<int> eventIds,
        int currentGold,int spentGold,int playerMorale,int currentStageX,int currentStageY,int chapter,
        StageType currentStageType, UnitUpgrade[] unitUpgrades,int sariStack,BattleRewardData battleReward,int nextUniqueId,int score)
    {
        this.id = id;
        this.myUnits = myUnits;
        this.warRelics = warRelics;
        this.eventIds = eventIds;
        this.currentGold = currentGold;
        this.spentGold = spentGold;
        this.playerMorale = playerMorale;
        this.currentStageX = currentStageX;
        this.currentStageY = currentStageY;
        this.chapter = chapter;
        this.currentStageType = currentStageType;
        this.unitUpgrades = unitUpgrades;
        this.sariStack = sariStack;
        this.battleReward = battleReward;
        this.nextUniqueId = nextUniqueId;
        this.score = score;
    }
}

public class SaveData
{
    private string _filePath;
    private string _jsonData;

    public void SaveDataFile()
    {
        _filePath = Application.persistentDataPath + "/PlayerData.json";

        SavePlayerData savePlayerData = RogueLikeData.Instance.GetRogueLikeData();
        _jsonData = JsonUtility.ToJson(savePlayerData);
        File.WriteAllText(_filePath, _jsonData);
    }
    
    public void SaveDataBattaleEnd(List<RogueUnitDataBase> units, List<RogueUnitDataBase> deadUnits)
    {
        _filePath = Application.persistentDataPath + "/PlayerData.json";

        SavePlayerData savePlayerData = RogueLikeData.Instance.GetBattleEndRogueLikeData(units, deadUnits);
        _jsonData = JsonUtility.ToJson(savePlayerData);
        File.WriteAllText(_filePath, _jsonData);
    }
    public void LoadData()
    {
        if (RogueLikeData.Instance.GetTestMode()) return;
        _filePath = Application.persistentDataPath + "/PlayerData.json";
        try
        {
            string jsonData = File.ReadAllText(_filePath);
            SavePlayerData savePlayerData = JsonUtility.FromJson<SavePlayerData>(jsonData);

            List<RogueUnitDataBase> myUnits = new(savePlayerData.myUnits);
            RogueLikeData.Instance.SetMyTeam(myUnits);
            foreach (var unit in myUnits)
            {
                unit.effectDictionary = new Dictionary<int, BuffDebuffData>();
            }
            List<WarRelic> warRelics = new(savePlayerData.warRelics);
            RogueLikeData.Instance.SetRelicBySaveData(warRelics);

            RogueLikeData.Instance.SetLoadData(savePlayerData.eventIds,savePlayerData.currentGold, savePlayerData.spentGold,
                savePlayerData.playerMorale, savePlayerData.currentStageX, savePlayerData.currentStageY, savePlayerData.chapter,
                savePlayerData.currentStageType, savePlayerData.sariStack, savePlayerData.battleReward,savePlayerData.nextUniqueId,savePlayerData.score);
        }
        catch (Exception ex)
        {
            Debug.LogError($"데이터 로드 실패: {ex.Message}");
        }
    }

    public void DeleteSaveFile()
    {
        _filePath = Application.persistentDataPath + "/PlayerData.json";
        try
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }

        }
        catch (Exception ex)
        {
            Debug.LogError($"저장 파일 삭제 실패: {ex.Message}");
        }
    }
    //데이터 삭제하고 다시 로드
    public void ResetGameData()
    {
        DeleteSaveFile();
        RogueLikeData.Instance.ResetToDefault();
        SaveDataFile();
        LoadData();
    }
}
