using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
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
    public StageType currentStageType;

    public int sariStack;
    
    public SavePlayerData(int id ,List<RogueUnitDataBase> myUnits,List<int> relicIds,List<int> eventIds, int currentGold,int spentGold,int playerMorale,int currentStageX,int currentStageY,StageType currentStageType,int sariStack)
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
        this.currentStageType = currentStageType;
        this.sariStack = sariStack;
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
            List<RogueUnitDataBase> myUnits = new List<RogueUnitDataBase>(savePlayerData.myUnits);
            RogueLikeData.Instance.AllMyUnits(myUnits);

            // 2. 유물 정보 업데이트하기 (relicIds를 Dictionary로 변환)
            foreach (var id in savePlayerData.relicIds)
            {
                RogueLikeData.Instance.AcquireRelic(id);
            }

            // 3. 현재 스테이지 설정
            RogueLikeData.Instance.SetCurrentStage(
                savePlayerData.currentStageX,
                savePlayerData.currentStageY,
                savePlayerData.currentStageType
            );

            // 4. 사리 스택 설정
            RogueLikeData.Instance.SetSariStack(savePlayerData.sariStack);

            // 5. 사기 설정
            RogueLikeData.Instance.SetMorale(savePlayerData.playerMorale);

            // 6. 골드 업데이트
            RogueLikeData.Instance.SetCurrentGold(savePlayerData.currentGold);

            Debug.Log("데이터 로드 성공!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"데이터 로드 실패: {ex.Message}");
        }
    }

}
