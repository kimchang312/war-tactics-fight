using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SavePlayerData
{
    public int id;
    public List<RogueUnitDataBase> myUnits;
    public List<WarRelic> warRelics = new();
    public List<int> eventIds = new();
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
    public StoreSnapshot currentStore;

    // 추가: 누락 필드
    public int language;     // RogueLikeData.language (0=kr,1=en,2=jp)
    public int fieldId;      // RogueLikeData.fieldId
    public int presetID;     // RogueLikeData.presetID
    public int rerollChance; // RogueLikeData.rerollChance
    public int unitOrder;    // RogueLikeData.unitOrder

    public SavePlayerData(
        int id, List<RogueUnitDataBase> myUnits, List<WarRelic> warRelics, List<int> eventIds,
        int currentGold, int spentGold, int playerMorale, int currentStageX, int currentStageY, int chapter,
        StageType currentStageType, UnitUpgrade[] unitUpgrades, int sariStack, BattleRewardData battleReward, int nextUniqueId, int score,
        // 추가 파라미터
        int language, int fieldId, int presetID, int rerollChance, int unitOrder)
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

        // 추가
        this.language = language;
        this.fieldId = fieldId;
        this.presetID = presetID;
        this.rerollChance = rerollChance;
        this.unitOrder = unitOrder;
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
        RogueLikeData.Instance.SetIsDataLoading(true);

        _filePath = Application.persistentDataPath + "/PlayerData.json";
        try
        {
            string jsonData = File.ReadAllText(_filePath);
            SavePlayerData savePlayerData = JsonUtility.FromJson<SavePlayerData>(jsonData);

            // 유닛/유물/기본 스냅샷 복원
            List<RogueUnitDataBase> myTeam = new(savePlayerData.myUnits);
            RogueLikeData.Instance.SetMyTeam(myTeam);
            foreach (var unit in myTeam)
                unit.effectDictionary = new Dictionary<int, BuffDebuffData>();
            RogueLikeData.Instance.SetRelicBySaveData(new List<WarRelic>(savePlayerData.warRelics));

            RogueLikeData.Instance.SetLoadData(
                savePlayerData.eventIds,
                savePlayerData.currentGold, savePlayerData.spentGold, savePlayerData.playerMorale,
                savePlayerData.currentStageX, savePlayerData.currentStageY, savePlayerData.chapter,
                savePlayerData.currentStageType, savePlayerData.sariStack, savePlayerData.battleReward,
                savePlayerData.nextUniqueId, savePlayerData.score
            );

            // 추가 필드 복원 (기본값 0이어도 안전)
            RogueLikeData.Instance.SetLanguage(savePlayerData.language);
            RogueLikeData.Instance.SetFieldId(savePlayerData.fieldId);
            RogueLikeData.Instance.SetPresetID(savePlayerData.presetID);
            RogueLikeData.Instance.SetRerollChance(savePlayerData.rerollChance);
            RogueLikeData.Instance.SetUnitOrder(savePlayerData.unitOrder);

            RogueLikeData.Instance.SetCurrentStoreSnapshot(savePlayerData.currentStore);

            // 언어 반영: 텍스트 DB 재로딩
            GameTextDB.LoadFromRogueLike();
        }
        catch (Exception ex)
        {
            Debug.LogError($"데이터 로드 실패: {ex.Message}");
        }

        RogueLikeData.Instance.SetIsDataLoading(false);
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
