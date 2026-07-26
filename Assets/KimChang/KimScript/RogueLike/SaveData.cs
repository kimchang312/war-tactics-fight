using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[System.Serializable]
public enum SaveProgressState
{
    StageSelect,
    EventOpen,
    EventResult,
    StoreOpen,
    BattlePending,
    BattlePlacement,
    RestOpen,
    TreasureOpen,
    RewardOpen,
    BattleInProgress
}

[System.Serializable]
public class EventSnapshot
{
    public int eventId = -1;
    public int selectedChoiceId = -1;
    public bool resultApplied;
    public string resultText;
    public bool closeEventAfterResult;
}

[System.Serializable]
public class BattleResumeSnapshot
{
    public bool active;
    public bool fromEvent;
    public int presetID = -1;
    public StageType stageType = StageType.Combat;
    public int fieldId;
    public int stageX = -1;
    public int stageY = -1;
    public List<int> placedUniqueIds = new();
}

[System.Serializable]
public class ChapterCounterSaveEntry
{
    public int chapter;
    public int count;

    public ChapterCounterSaveEntry() { }

    public ChapterCounterSaveEntry(int chapter, int count)
    {
        this.chapter = chapter;
        this.count = count;
    }
}

[System.Serializable]
public class SavePlayerData
{
    public int saveVersion;
    public int id;
    public List<RogueUnitDataBase> myUnits;
    public List<RogueUnitDataBase> enemyUnits = new();
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
    public EventSnapshot currentEvent;
    public BattleResumeSnapshot battleResume;
    public SaveProgressState progressState = SaveProgressState.StageSelect;

    // 추가: 누락 필드
    public int language;     // RogueLikeData.language (0=kr,1=en,2=jp)
    public int fieldId;      // RogueLikeData.fieldId
    public int presetID;     // RogueLikeData.presetID
    public int rerollChance; // RogueLikeData.rerollChance
    public int unitOrder;    // RogueLikeData.unitOrder
    /// <summary>47번 보물지도: 다음 이벤트를 보물로 바꿀 예정인지.</summary>
    public bool nextEventToTreasure;
    public int randomSeed;
    public int stageCallCount;
    public List<ChapterCounterSaveEntry> rainbowKeyUses = new();

    public SavePlayerData(
        int id, List<RogueUnitDataBase> myUnits, List<RogueUnitDataBase> enemyUnits, List<WarRelic> warRelics, List<int> eventIds,
        int currentGold, int spentGold, int playerMorale, int currentStageX, int currentStageY, int chapter,
        StageType currentStageType, UnitUpgrade[] unitUpgrades, int sariStack, BattleRewardData battleReward, int nextUniqueId, int score,
        // 추가 파라미터
        int language, int fieldId, int presetID, int rerollChance, int unitOrder, bool nextEventToTreasure)
    {
        saveVersion = 3;
        this.id = id;
        this.myUnits = myUnits;
        this.enemyUnits = enemyUnits ?? new List<RogueUnitDataBase>();
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
        this.nextEventToTreasure = nextEventToTreasure;
    }
}

public class SaveData
{
    private static bool continueLoadRequested;
    private string _filePath;
    private string _jsonData;

    // 사용처: Title 씬 계속하기 버튼에서 호출. 씬 로드는 호출한 쪽에서 처리하고, 여기서는 요청 플래그만 저장한다.
    public bool RequestContinueLoadFromTitle()
    {
        if (continueLoadRequested)
            return false;

        if (!CanContinueRun())
        {
            continueLoadRequested = false;
            Debug.LogWarning("불러올 저장 데이터가 없습니다. PlayerData.json과 stage_save.json이 모두 필요합니다.");
            return false;
        }

        continueLoadRequested = true;
        return true;
    }

    // 사용처: RLmap의 GameManager/UIGenerator가 계속하기 요청 여부를 확인한다.
    public static bool HasContinueLoadRequest()
    {
        return continueLoadRequested;
    }

    // 사용처: RLmap 복원 처리가 끝난 뒤 계속하기 요청을 제거한다.
    public static void ClearContinueLoadRequest()
    {
        continueLoadRequested = false;
    }

    // 사용처: Title 계속하기 버튼 표시/로드 가능 여부 확인
    public static bool HasPlayerSaveFile()
    {
        string path = Application.persistentDataPath + "/PlayerData.json";
        return File.Exists(path);
    }

    public static bool CanContinueRun()
    {
        return HasPlayerSaveFile() && SaveSystem.HasSave();
    }

    public bool SaveGame()
    {
        return SaveGame(null);
    }

    public bool SaveGame(MapGenerator mapGenerator)
    {
        try
        {
            CaptureTransientSaveState();
            SaveDataFile();
            if (TrySaveMapData(mapGenerator, true))
                return true;

            MapGenerator resolvedMapGenerator = ResolveMapGenerator(mapGenerator);
            if (resolvedMapGenerator == null ||
                resolvedMapGenerator.NodeDictionary == null ||
                resolvedMapGenerator.NodeDictionary.Count == 0)
            {
                Debug.LogWarning("맵 저장에 사용할 MapGenerator를 찾지 못했습니다.");
                return false;
            }

            return SaveSystem.SaveStageFull(resolvedMapGenerator.NodeDictionary);
        }
        catch (Exception ex)
        {
            Debug.LogError($"게임 저장 실패: {ex.Message}");
            return false;
        }
    }

    private static void CaptureTransientSaveState()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.TryCaptureBattlePlacementSnapshot();
    }

    public bool LoadGame()
    {
        return LoadGame(out _);
    }

    public bool LoadGame(out StageFullSaveData stageSaveData)
    {
        stageSaveData = null;

        if (!CanContinueRun())
        {
            Debug.LogWarning("불러올 저장 데이터가 없습니다.");
            return false;
        }

        LoadData();
        if (!RogueLikeData.Instance.HasLoadedSaveData())
        {
            Debug.LogWarning("플레이어 저장 데이터 로드에 실패했습니다.");
            return false;
        }

        stageSaveData = SaveSystem.LoadFull();
        if (stageSaveData == null)
            return false;

        ApplyStageSaveToRunData(stageSaveData);
        RogueLikeData.Instance.RestoreBattleResumeContextIfNeeded();
        ClearContinueLoadRequest();
        return true;
    }

    private static MapGenerator ResolveMapGenerator(MapGenerator preferred)
    {
        if (HasUsableMap(preferred))
            return preferred;

        if (GameManager.Instance != null && GameManager.Instance.uIGenerator != null)
        {
            GameManager.Instance.uIGenerator.RehydrateMapFromExistingUIIfNeeded();
            if (HasUsableMap(GameManager.Instance.uIGenerator.mapGenerator))
                return GameManager.Instance.uIGenerator.mapGenerator;
        }

        foreach (UIGenerator uiGenerator in UnityEngine.Object.FindObjectsOfType<UIGenerator>(true))
        {
            if (uiGenerator == null)
                continue;

            uiGenerator.RehydrateMapFromExistingUIIfNeeded();
            if (HasUsableMap(uiGenerator.mapGenerator))
                return uiGenerator.mapGenerator;
        }

        if (preferred != null &&
            preferred.NodeDictionary != null &&
            preferred.NodeDictionary.Count > 0)
        {
            return preferred;
        }

        foreach (MapGenerator candidate in UnityEngine.Object.FindObjectsOfType<MapGenerator>(true))
        {
            if (candidate != null &&
                candidate.NodeDictionary != null &&
                candidate.NodeDictionary.Count > 0)
            {
                return candidate;
            }
        }

        return preferred;
    }

    private static bool HasUsableMap(MapGenerator mapGenerator)
    {
        return mapGenerator != null &&
               mapGenerator.NodeDictionary != null &&
               mapGenerator.NodeDictionary.Count > 0;
    }

    private static bool TrySaveMapData(MapGenerator mapGenerator, bool allowExistingMapSave)
    {
        MapGenerator resolvedMapGenerator = ResolveMapGenerator(mapGenerator);
        if (HasUsableMap(resolvedMapGenerator) &&
            SaveSystem.SaveStageFull(resolvedMapGenerator.NodeDictionary))
        {
            return true;
        }

        Dictionary<string, StageNode> nodesFromUI = BuildNodeDictionaryFromStageUI();
        if (nodesFromUI.Count > 0 && SaveSystem.SaveStageFull(nodesFromUI))
        {
            return true;
        }

        if (allowExistingMapSave && SaveSystem.HasSave())
        {
            Debug.LogWarning("MapGenerator was not available; keeping the existing stage_save.json.");
            return true;
        }

        Debug.LogWarning("MapGenerator was not available for saving.");
        return false;
    }

    private static Dictionary<string, StageNode> BuildNodeDictionaryFromStageUI()
    {
        StageNodeUI[] stageUIs = UnityEngine.Object.FindObjectsOfType<StageNodeUI>(true);
        var dict = new Dictionary<string, StageNode>();

        if (stageUIs == null || stageUIs.Length == 0)
            return dict;

        foreach (StageNodeUI ui in stageUIs)
        {
            if (ui == null)
                continue;

            string key = $"{ui.level}_{ui.row}";
            StageNode node = new(ui.level, ui.row, ui.stageType)
            {
                presetID = ui.PresetID,
                battlefieldEffect = ui.battlefieldEffect
            };
            dict[key] = node;
        }

        foreach (StageNodeUI ui in stageUIs)
        {
            if (ui == null)
                continue;

            string key = $"{ui.level}_{ui.row}";
            if (!dict.TryGetValue(key, out StageNode node))
                continue;

            foreach (StageNodeUI nextUI in ui.connectedStages)
            {
                if (nextUI == null)
                    continue;

                string nextKey = $"{nextUI.level}_{nextUI.row}";
                if (dict.TryGetValue(nextKey, out StageNode nextNode))
                    node.connectedNodes.Add(nextNode);
            }
        }

        return dict;
    }

    private static void ApplyStageSaveToRunData(StageFullSaveData stageSaveData)
    {
        if (stageSaveData == null)
            return;

        if (stageSaveData.currentLevel < 0 || stageSaveData.currentRow < 0)
        {
            RogueLikeData.Instance.SetCurrentStage(-1, -1, StageType.Unknown);
            return;
        }

        StageNodeSaveEntry currentEntry = null;
        if (stageSaveData.allNodes != null)
        {
            foreach (StageNodeSaveEntry entry in stageSaveData.allNodes)
            {
                if (entry.level == stageSaveData.currentLevel &&
                    entry.row == stageSaveData.currentRow)
                {
                    currentEntry = entry;
                    break;
                }
            }
        }

        StageType stageType = stageSaveData.currentStageType;
        if (stageType == StageType.Unknown && currentEntry != null)
            stageType = currentEntry.stageType;

        RogueLikeData.Instance.SetCurrentStage(stageSaveData.currentLevel, stageSaveData.currentRow, stageType);

        if (currentEntry != null)
            RogueLikeData.Instance.SetPresetID(currentEntry.presetID);
    }


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
        TrySaveMapData(null, true);
    }
    public void LoadData()
    {
        if (RogueLikeData.Instance.GetTestMode()) return;
        RogueLikeData.Instance.SetHasLoadedSaveData(false);
        RogueLikeData.Instance.SetIsDataLoading(true);

        try
        {
            _filePath = Application.persistentDataPath + "/PlayerData.json";
            if (!File.Exists(_filePath))
                return;

            string jsonData = File.ReadAllText(_filePath);
            SavePlayerData savePlayerData = JsonUtility.FromJson<SavePlayerData>(jsonData);
            if (savePlayerData == null)
            {
                Debug.LogWarning("플레이어 저장 데이터가 비어 있습니다.");
                return;
            }

            // 유닛/유물/기본 스냅샷 복원
            List<RogueUnitDataBase> myTeam = new(savePlayerData.myUnits ?? new List<RogueUnitDataBase>());
            RogueLikeData.Instance.SetMyTeam(myTeam);
            foreach (var unit in myTeam)
            {
                if (unit != null)
                    unit.effectDictionary = new Dictionary<int, BuffDebuffData>();
            }

            List<RogueUnitDataBase> enemyUnits = new(savePlayerData.enemyUnits ?? new List<RogueUnitDataBase>());
            foreach (var unit in enemyUnits)
            {
                if (unit != null)
                    unit.effectDictionary = new Dictionary<int, BuffDebuffData>();
            }
            RogueLikeData.Instance.SetAllEnemyUnits(enemyUnits);

            RogueLikeData.Instance.SetRelicBySaveData(new List<WarRelic>(savePlayerData.warRelics ?? new List<WarRelic>()));

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
            RogueLikeData.Instance.SetNextEventToTreasure(savePlayerData.nextEventToTreasure);
            RogueLikeData.Instance.SetRainbowKeyUsesFromSave(savePlayerData.rainbowKeyUses);

            if (savePlayerData.saveVersion >= 2)
            {
                RogueLikeData.Instance.SetRandomSeed(savePlayerData.randomSeed);
                RogueLikeData.Instance.SetStageCallCount(savePlayerData.stageCallCount);
            }

            RogueLikeData.Instance.SetCurrentStoreSnapshot(savePlayerData.currentStore);
            RogueLikeData.Instance.SetCurrentEventSnapshot(savePlayerData.currentEvent);
            RogueLikeData.Instance.SetBattleResumeSnapshot(savePlayerData.battleResume);
            RogueLikeData.Instance.SetProgressState(savePlayerData.progressState);
            RogueLikeData.Instance.SetUpgradeValues(savePlayerData.unitUpgrades);
            RogueLikeData.Instance.SetHasLoadedSaveData(true);

            // 언어 반영: 텍스트 DB 재로딩
            GameTextDB.LoadFromRogueLike();
        }
        catch (Exception ex)
        {
            Debug.LogError($"데이터 로드 실패: {ex.Message}");
        }
        finally
        {
            RogueLikeData.Instance.SetIsDataLoading(false);
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
        ClearContinueLoadRequest();
        DeleteSaveFile();
        SaveSystem.Clear();
        RogueLikeData.Instance.ResetToDefault();
        SaveDataFile();
        LoadData();
    }
}
