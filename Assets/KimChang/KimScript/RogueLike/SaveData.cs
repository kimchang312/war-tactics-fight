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
    public List<int> relicIds= new();
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
    public SavePlayerData(int id ,List<RogueUnitDataBase> myUnits,List<int> relicIds,List<int> eventIds,
        int currentGold,int spentGold,int playerMorale,int currentStageX,int currentStageY,int chapter,
        StageType currentStageType, UnitUpgrade[] unitUpgrades,int sariStack,BattleRewardData battleReward,int nextUniqueId)
    {
        this.id = id;
        this.myUnits = myUnits;
        this.relicIds = relicIds;
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
       // Debug.Log(savePlayerData.myUnits[0].unitName);
        File.WriteAllText(_filePath, _jsonData);
    }
    
    public void SaveDataBattaleEnd(List<RogueUnitDataBase> units, List<RogueUnitDataBase> deadUnits)
    {
        SavePlayerData savePlayerData = RogueLikeData.Instance.GetBattleEndRogueLikeData(units, deadUnits);
        // 데이터를 JSON 문자열로 직렬화
        _jsonData = JsonUtility.ToJson(savePlayerData);
        // Debug.Log(savePlayerData.myUnits[0].unitName);
        File.WriteAllText(_filePath, _jsonData);
    }
    public void LoadData()
    {
        try
        {
            // JSON 문자열을 객체로 역직렬화
            string jsonData = File.ReadAllText(_filePath); // 파일에서 읽기
            SavePlayerData savePlayerData = JsonUtility.FromJson<SavePlayerData>(jsonData);

            // 1. 내 유닛 전부 수정하기
            List<RogueUnitDataBase> myUnits = new(savePlayerData.myUnits);
            RogueLikeData.Instance.SetMyTeam(myUnits);
            foreach (var unit in myUnits)
            {
                unit.effectDictionary = new Dictionary<int, BuffDebuffData>();
            }
            // 2. 유물 정보 업데이트하기 (relicIds를 Dictionary로 변환)
            foreach (var id in savePlayerData.relicIds)
            {
                RogueLikeData.Instance.AcquireRelic(id);
            }

            RogueLikeData.Instance.SetLoadData(savePlayerData.eventIds,savePlayerData.currentGold, savePlayerData.spentGold,
                savePlayerData.playerMorale, savePlayerData.currentStageX, savePlayerData.currentStageY, savePlayerData.chapter,
                savePlayerData.currentStageType, savePlayerData.sariStack, savePlayerData.battleReward);

            Debug.Log("데이터 로드 성공!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"데이터 로드 실패: {ex.Message}");
        }
    }

    public void DeleteSaveFile()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
                Debug.Log("저장 파일 삭제 성공!");
            }
            else
            {
                Debug.LogWarning("삭제할 저장 파일이 없습니다.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"저장 파일 삭제 실패: {ex.Message}");
        }
    }
    private void LoadRogueLike()
    {
        string filePath = Application.dataPath + "/KimChang/Json/PlayerData.json";

        if (File.Exists(filePath))
        {
            LoadData();
            SceneManager.LoadScene("RLmap");
        }
        else
        {
            Debug.LogWarning("로드할 데이터가 없습니다.");
            // 필요하면 사용자 알림 UI 추가
            // e.g., ShowPopup("저장된 데이터가 없습니다.");
        }
    }
}
