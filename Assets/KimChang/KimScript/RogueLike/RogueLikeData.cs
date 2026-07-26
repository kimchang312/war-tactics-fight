using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RogueLikeData
{
    private static RogueLikeData instance;

    public static RogueLikeData Instance
    {
        get
        {
            instance ??= new RogueLikeData();

            return instance;
        }
    }

    private int nextUnitUniqueId = 0;

    private int maxUnits = 5;
    private int maxHero = 2;

    private List<RogueUnitDataBase> myTeam = new();
    private List<RogueUnitDataBase> myUnits = new();
    private List<RogueUnitDataBase> enemyUnits = new();
    private List<RogueUnitDataBase> savedMyUnits = new();
    private List<RogueUnitDataBase> selectedUnits = new();

    private Dictionary<RelicType, List<WarRelic>> relicsByType;
    private Dictionary<RelicType, HashSet<int>> relicIdsByType = new();
    private readonly Dictionary<int, WarRelic> ownedRelicsById = new(64);
    private Dictionary<int, int> encounteredEvent = new();
    //퀘스트의 상태 id, 완료 여부 퀘스트 수락시 false 퀘스트 취소되면 배열삭제
    private Dictionary<int, QuestClass> questState = new();
    private int currentStageX = 1;
    private int currentStageY = 0; private int currentStageZ = 0;
    private int chapter = 1;
    private int presetID = -1;

    private StageType currentStageType = StageType.Combat;

    private int currentGold = 150;
    private int playerMorale = 50;
    private int spentGold = 0;
    private float myFinalDamage = 1;
    private float enemyFinalDamage = 1;

    private const int UpgradeSlotCount = 9;
    private UnitUpgrade[] upgradeValues = new UnitUpgrade[UpgradeSlotCount];

    private int sariStack = 0;

    private readonly int[] costTable = { 100, 150, 200, 250, 300 };

    private int fieldId = 0;

    private BattleRewardData battleReward = new();

    private int rerollChance = 2;

    public bool isFreeUpgrade = false;

    private int battleUnitCount = 0;

    private int score = 0;

    private bool clearChapter = false;
    private bool resetMap = false;

    private int randomSeed;
    private System.Random systemRandom;
    private int currentStageSeedBase;
    private int stageCallCount;

    private bool isTestMode = false;

    private int unitOrder = 0;

    // --- 전쟁유산 47/48 상태 ---
    // 47: 보물지도 - 다음 이벤트 지역을 보물로 변환하는 1회성 플래그
    private bool nextEventToTreasure = false;
    // 48: 무지개 열쇠 - 챕터당 2회 사용(챕터별 사용 횟수 기록)
    private Dictionary<int, int> rainbowKeyUsesPerChapter = new Dictionary<int, int>();

    // 사용처: 현재 상점 세션 저장/복원
    private StoreSnapshot currentStore;
    private Dictionary<string, StoreSnapshot> storeSessions;

    // 사용처: 저장/로드 시 현재 진행 중인 UI 상태 복원
    private EventSnapshot currentEvent;
    private BattleResumeSnapshot battleResume;
    private SaveProgressState progressState = SaveProgressState.StageSelect;
    private bool hasLoadedSaveData = false;

    // 사용처: 상점 좌표 → 유니크 키
    private string BuildStoreKey(int chapter, int x, int y) => $"{chapter}:{x}:{y}";

    private bool isStageClear = false;

    private bool isDataLoading = false;

    private int language = 0;

    private const string MasterVolumePrefKey = "RL_MasterVolume";
    private const string BgmVolumePrefKey = "RL_BgmVolume";
    private const string SfxVolumePrefKey = "RL_SfxVolume";
    private const string LanguagePrefKey = "Language";
    private float masterVolume = 1f;
    private float bgmVolume = 1f;
    private float sfxVolume = 1f;

    private RogueLikeData()
    {
        language = Mathf.Clamp(PlayerPrefs.GetInt(LanguagePrefKey, language), 0, 2);

        relicsByType = new Dictionary<RelicType, List<WarRelic>>();
        relicIdsByType = new Dictionary<RelicType, HashSet<int>>();

        foreach (RelicType type in Enum.GetValues(typeof(RelicType)))
        {
            relicsByType[type] = new List<WarRelic>();
            relicIdsByType[type] = new HashSet<int>();
        }
        EnsureUpgradeValues();

        LoadAudioSettings();
    }
    public SavePlayerData GetRogueLikeData()
    {
        SavePlayerData data = new(
            0,
            new List<RogueUnitDataBase>(myTeam),
            new List<RogueUnitDataBase>(enemyUnits),
            ownedRelicsById.Values.ToList(),
            encounteredEvent.Values.ToList(),
            currentGold, spentGold, playerMorale,
            currentStageX, currentStageY, chapter, currentStageType,
            upgradeValues, sariStack, battleReward, nextUnitUniqueId, score,
            // 추가 필드
            language, fieldId, presetID, rerollChance, unitOrder, nextEventToTreasure
        );
        data.currentStore = currentStore;
        data.currentEvent = currentEvent;
        data.battleResume = CopyBattleResumeSnapshot(battleResume);
        data.progressState = progressState;
        data.randomSeed = randomSeed;
        data.stageCallCount = stageCallCount;
        data.rainbowKeyUses = GetRainbowKeyUsesSnapshot();
        return data;
    }

    // 사용처: SaveData.LoadData()에서 로드한 상점 스냅샷을 현재 세션에도 복원
    public void SetCurrentStoreSnapshot(StoreSnapshot snap)
    {
        currentStore = snap;

        if (snap == null)
            return;

        storeSessions ??= new Dictionary<string, StoreSnapshot>();
        string key = BuildStoreKey(snap.chapter, snap.stageX, snap.stageY);
        storeSessions[key] = snap;
    }

    // 사용처: 이벤트 UI가 처음 열릴 때 현재 이벤트를 저장
    public void OpenEventSnapshot(int eventId)
    {
        currentEvent = new EventSnapshot
        {
            eventId = eventId,
            selectedChoiceId = -1,
            resultApplied = false,
            resultText = "",
            closeEventAfterResult = false
        };

        progressState = SaveProgressState.EventOpen;
    }

    // 사용처: 이벤트 선택 결과 적용 후 중복 보상 적용을 막기 위해 결과 상태를 저장
    public void SetEventResultSnapshot(int eventId, int choiceId, string resultText, bool closeAfterResult)
    {
        currentEvent = new EventSnapshot
        {
            eventId = eventId,
            selectedChoiceId = choiceId,
            resultApplied = true,
            resultText = resultText ?? "",
            closeEventAfterResult = closeAfterResult
        };

        progressState = closeAfterResult
            ? SaveProgressState.BattlePending
            : SaveProgressState.EventResult;
    }

    // 사용처: 이벤트 종료 후 스테이지 선택 상태로 되돌림
    public void ClearEventSnapshot()
    {
        currentEvent = null;

        if (progressState == SaveProgressState.EventOpen ||
            progressState == SaveProgressState.EventResult ||
            progressState == SaveProgressState.BattlePending)
        {
            progressState = SaveProgressState.StageSelect;
        }
    }

    // 사용처: SaveData.LoadData()에서 저장된 이벤트 스냅샷을 복원
    public void SetCurrentEventSnapshot(EventSnapshot snapshot)
    {
        currentEvent = snapshot;
    }

    // 사용처: EventUIManager, GameManager에서 저장된 이벤트 상태 확인
    public EventSnapshot GetCurrentEventSnapshot()
    {
        return currentEvent;
    }

    // 사용처: SaveData.LoadData(), StoreUI 종료, 이벤트 종료 등에서 현재 진행 상태 갱신
    private static BattleResumeSnapshot CopyBattleResumeSnapshot(BattleResumeSnapshot snapshot)
    {
        if (snapshot == null)
            return null;

        return new BattleResumeSnapshot
        {
            active = snapshot.active,
            fromEvent = snapshot.fromEvent,
            presetID = snapshot.presetID,
            stageType = snapshot.stageType,
            fieldId = snapshot.fieldId,
            stageX = snapshot.stageX,
            stageY = snapshot.stageY,
            placedUniqueIds = snapshot.placedUniqueIds != null
                ? new List<int>(snapshot.placedUniqueIds)
                : new List<int>()
        };
    }

    private static bool IsBattleResumeState(SaveProgressState state)
    {
        return state == SaveProgressState.BattlePending ||
               state == SaveProgressState.BattlePlacement ||
               state == SaveProgressState.BattleInProgress;
    }

    private void EnsureBattleResumeSnapshot(bool fromEvent)
    {
        if (battleResume == null)
            battleResume = new BattleResumeSnapshot();

        battleResume.active = true;
        battleResume.fromEvent = fromEvent;
        battleResume.presetID = presetID;
        battleResume.stageType = currentStageType;
        battleResume.fieldId = fieldId;
        battleResume.stageX = currentStageX;
        battleResume.stageY = currentStageY;
        battleResume.placedUniqueIds ??= new List<int>();
    }

    public void BeginBattleResumeSnapshot(bool fromEvent)
    {
        EnsureBattleResumeSnapshot(fromEvent);
        progressState = SaveProgressState.BattlePlacement;
    }

    public void UpdateBattleResumePlacement(List<int> placedUniqueIds)
    {
        EnsureBattleResumeSnapshot(battleResume != null && battleResume.fromEvent);
        battleResume.placedUniqueIds = placedUniqueIds != null
            ? new List<int>(placedUniqueIds)
            : new List<int>();
    }

    public void MarkBattleResumeInProgress(List<int> placedUniqueIds)
    {
        UpdateBattleResumePlacement(placedUniqueIds);
        progressState = SaveProgressState.BattleInProgress;
    }

    public void SetBattleResumeSnapshot(BattleResumeSnapshot snapshot)
    {
        battleResume = CopyBattleResumeSnapshot(snapshot);
    }

    public BattleResumeSnapshot GetBattleResumeSnapshot()
    {
        return CopyBattleResumeSnapshot(battleResume);
    }

    public void ClearBattleResumeSnapshot()
    {
        battleResume = null;
    }

    public void RestoreBattleResumeContextIfNeeded()
    {
        if (battleResume == null || !battleResume.active || !IsBattleResumeState(progressState))
            return;

        if (battleResume.stageX >= 0 && battleResume.stageY >= 0)
        {
            currentStageX = battleResume.stageX;
            currentStageY = battleResume.stageY;
        }

        if (battleResume.presetID >= 0)
            presetID = battleResume.presetID;

        currentStageType = battleResume.stageType;
        fieldId = battleResume.fieldId;
        SetStage();
    }

    public List<RogueUnitDataBase> GetBattleResumePlacedUnits()
    {
        var result = new List<RogueUnitDataBase>();
        if (battleResume == null || battleResume.placedUniqueIds == null)
            return result;

        foreach (int uniqueId in battleResume.placedUniqueIds)
        {
            RogueUnitDataBase unit = myTeam.Find(u => u != null && u.UniqueId == uniqueId);
            if (unit != null && !result.Exists(u => u.UniqueId == unit.UniqueId))
                result.Add(unit);
        }

        return result;
    }

    public bool HasBattleResumePlacement()
    {
        return battleResume != null &&
               battleResume.placedUniqueIds != null &&
               battleResume.placedUniqueIds.Count > 0;
    }

    public void SetProgressState(SaveProgressState state)
    {
        progressState = state;
    }

    // 사용처: GameManager가 로드 후 어떤 UI를 복원할지 판단
    public SaveProgressState GetProgressState()
    {
        return progressState;
    }

    // 사용처: 상점 닫기 후 현재 UI 진행 상태만 스테이지 선택으로 되돌림
    public void ClearStoreOpenState()
    {
        if (progressState == SaveProgressState.StoreOpen)
            progressState = SaveProgressState.StageSelect;
    }

    // 사용처: GameManager가 실제 저장 파일 로드 성공 여부에 따라 위치/UI 복원을 제한
    public void SetHasLoadedSaveData(bool value)
    {
        hasLoadedSaveData = value;
    }

    public bool HasLoadedSaveData()
    {
        return hasLoadedSaveData;
    }

    // RogueLikeData.cs
    // 사용처: 상점 구매/이벤트/전투 보상 등 데이터 변경 완료 후 최종 저장
    public void SaveNow()
    {
        new SaveData().SaveGame();
    }
    public SavePlayerData GetBattleEndRogueLikeData(List<RogueUnitDataBase> units, List<RogueUnitDataBase> deadUnits)
    {
        List<RogueUnitDataBase> savedCopy = new(savedMyUnits);
        foreach (var unit in units.Concat(deadUnits))
        {
            var savedUnit = savedCopy.Find(u => u.UniqueId == unit.UniqueId);
            if (savedUnit == null) continue;
            if (unit.Energy < 1) savedCopy.Remove(savedUnit);
            else savedUnit.Energy = unit.Energy;
        }

        SavePlayerData data = new(
            0,
            savedCopy,
            new List<RogueUnitDataBase>(enemyUnits),
            ownedRelicsById.Values.ToList(),
            encounteredEvent.Values.ToList(),
            currentGold, spentGold, playerMorale,
            currentStageX, currentStageY, chapter, currentStageType,
            upgradeValues, sariStack, battleReward, nextUnitUniqueId, score,
            // 추가 필드
            language, fieldId, presetID, rerollChance, unitOrder, nextEventToTreasure
        );
        data.currentStore = currentStore;
        data.currentEvent = null;
        data.battleResume = null;
        data.progressState = SaveProgressState.RewardOpen;
        data.randomSeed = randomSeed;
        data.stageCallCount = stageCallCount;
        data.rainbowKeyUses = GetRainbowKeyUsesSnapshot();

        myTeam = savedCopy;
        currentEvent = null;
        battleResume = null;
        progressState = SaveProgressState.RewardOpen;
        savedMyUnits.Clear();
        return data;
    }
    //내 유닛 전부 수정하기
    public void SetAllMyUnits(List<RogueUnitDataBase> units)
    {
        myUnits = new List<RogueUnitDataBase>(units);
    }
    //내 유닛 하나 추가
    public void AddMyTeam(RogueUnitDataBase unit)
    {
        int heroCount = 0;
        int maxHeroCount = GetMaxHero();
        if (unit.branchIdx == 8)
        {
            for (int i = 0; i < myTeam.Count; i++)
            {
                if (myTeam[i].branchIdx == 8)
                {
                    heroCount++;
                    if (heroCount >= maxHeroCount)
                    {
                        RelicManager.HandleRandomRelic(10, RelicManager.RelicAction.Acquire);
                        return;
                    }
                }
            }
        }

        myTeam.Add(unit);
    }

    //상대 유닛 전부 수정하기
    public void SetAllEnemyUnits(List<RogueUnitDataBase> units)
    {
        enemyUnits = units != null ? new List<RogueUnitDataBase>(units) : new List<RogueUnitDataBase>();
    }

    //내 유닛 가져오기
    public List<RogueUnitDataBase> GetMyUnits()
    {
        return new List<RogueUnitDataBase>(myUnits);
    }

    //상대 유닛 가져오기
    public List<RogueUnitDataBase> GetEnemyUnits()
    {
        return new List<RogueUnitDataBase>(enemyUnits);
    }
    // 사용처: 외부 코드 호환용 래퍼, 실제 유산 획득 로직은 RelicManager에서 처리
    public void AcquireRelic(int relicId)
    {
        RelicManager.AcquireRelic(relicId);
    }


    // 사용처: RelicManager가 검증을 끝낸 유물을 실제 보유 목록에 반영
    public bool TryAddOwnedRelic(WarRelic relic)
    {
        if (relic == null) return false;
        if (ownedRelicsById.ContainsKey(relic.id)) return false;

        relicsByType[relic.type].Add(relic);
        relicIdsByType[relic.type].Add(relic.id);
        ownedRelicsById.Add(relic.id, relic);
        return true;
    }

    // 특정 등급의 유물 가져오기
    public List<WarRelic> GetRelicsByGrade(int grade)
    {
        List<WarRelic> result = new();

        foreach (var relicList in relicsByType.Values)
        {
            result.AddRange(relicList.FindAll(relic => relic.grade == grade));
        }

        return result;
    }

    // 특정 유물 정보 수정
    public void UpdateRelic(int relicId, WarRelic updatedRelic)
    {
        foreach (var relicList in relicsByType.Values)
        {
            for (int i = 0; i < relicList.Count; i++)
            {
                if (relicList[i].id == relicId)
                {
                    relicList[i] = updatedRelic;
                    ownedRelicsById[relicId] = updatedRelic;
                    return;
                }
            }
        }
    }

    // 특정 ID
    public WarRelic GetOwnedRelicById(int relicId)
    {
        return ownedRelicsById.TryGetValue(relicId, out var relic) ? relic : null;
    }

    // 보유한 모든 유물을 반환하는 함수
    public List<WarRelic> GetAllOwnedRelics()
    {
        return ownedRelicsById.Values.ToList();
    }

    // 보유한 모든 유물의 ID만 반환하는 함수
    public List<int> GetAllOwnedRelicIds()
    {
        return ownedRelicsById.Keys.ToList();
    }

    // 사용처: 보유 유산 존재 여부(빠른 조회)
    public bool HasOwnedRelic(int relicId)
    {
        return ownedRelicsById.ContainsKey(relicId);
    }

    // 사용처: 전투/이벤트에서 보유 유산 전체 순회(할당 없이)
    public IReadOnlyDictionary<int, WarRelic> GetOwnedRelicMap()
    {
        return ownedRelicsById;
    }

    //보유 유산 전부 삭제
    public void ResetOwnedRelics()
    {
        // 각 유물 타입별로 저장된 유물 리스트를 비웁니다.
        foreach (RelicType type in relicsByType.Keys)
        {
            relicsByType[type].Clear();
        }

        // 중복 체크용 HashSet도 모두 비웁니다.
        foreach (RelicType type in relicIdsByType.Keys)
        {
            relicIdsByType[type].Clear();
        }

        ownedRelicsById.Clear();
    }
    //현재 스테이지 수정
    public void SetCurrentStage(int x, int y, StageType type)
    {
        currentStageX = x;
        currentStageY = y;
        currentStageType = type;
        SetStage();
    }
    public int GetCurrentStageX()
    {
        return currentStageX;
    }
    //현재 스테이지 가져오기
    public (int x, int y, StageType type) GetCurrentStage()
    {
        return (currentStageX, currentStageY, currentStageType);
    }
    //현재 스테이지 종류
    public StageType GetCurrentStageType()
    {
        return currentStageType;
    }
    public void SetStageType(StageType type)
    {
        currentStageType = type;
    }
    //현재 골드 가져오기
    public int GetCurrentGold()
    {
        return currentGold;
    }
    // 사용처: 금화 사용 가능 여부 계산. 현재 유산 49는 차용 한도 효과가 아니므로 음수 금화를 허용하지 않는다.
    private int GetMinGoldLimit()
    {
        return 0;
    }

    // 사용처: 상점/이벤트/강화 등 금화 사용 가능 여부 확인
    public bool CanSpendGold(int reduceGold)
    {
        if (reduceGold <= 0)
            return true;

        return currentGold - reduceGold >= GetMinGoldLimit();
    }

    // 사용처: 상점/이벤트/강화 등 금화 차감
    public bool ReduceGold(int gold)
    {
        if (gold <= 0)
            return true;

        if (!CanSpendGold(gold))
            return false;

        int baseGold = currentGold;

        spentGold += gold;
        currentGold -= gold;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.AnimateGoldChange(baseGold, -gold);
        }
        return true;
    }

    //골드 획득
    public void EarnGold(int gold)
    {
        int baseGold = currentGold;
        float addGold = 1;
        if (RelicManager.CheckRelicById(5))
        {
            WarRelic relic = RelicManager.GetRelicById(5);
            var vals = relic.GetAllValuesAsFloatListOrNull();
            if (vals != null)
            {
                addGold += vals[0];
            }
        }
        gold = (int)(addGold * gold);
        currentGold += gold;

        if (UIManager.Instance != null)
        {
            //골드 애니메이션
            UIManager.Instance.AnimateGoldChange(baseGold, gold);

        }


    }


    //현재 골드 수정
    public void SetCurrentGold(int gold)
    {
        currentGold = gold;
    }
    //사용한 골드 가져오기
    public int GetSpentGold()
    {
        return spentGold;
    }
    public void SetSpentGold(int gold)
    {
        spentGold = gold;
    }
    public int GetMorale()
    {
        return playerMorale;
    }

    // 사기 증감 통합 함수
    public int ChangeMorale(int value)
    {
        int baseMorale = playerMorale;
        int actualChange;

        if (value >= 0)
        {
            if (RelicManager.CheckRelicById(116)) return 0;

            actualChange = Mathf.Min(value, 100 - playerMorale);
            playerMorale += actualChange;
        }
        else
        {
            float reductionModifier = 1f;
            if (RelicManager.CheckRelicById(33))
            {
                WarRelic relic = RelicManager.GetRelicById(33);
                var vals = relic.GetAllValuesAsFloatListOrNull();
                if (vals != null)
                {
                    reductionModifier += vals[0];
                }
            }

            int reduced = Mathf.RoundToInt(value * reductionModifier); // value < 0

            actualChange = Mathf.Max(reduced, -playerMorale); // 최소 0 유지
            playerMorale += actualChange;
        }
        UnitStateChange.ChangeStateMyUnits();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.AnimateMoraleChange(baseMorale, actualChange);
        }
        return actualChange;
    }



    public void SetMorale(int value)
    {
        playerMorale = value;
    }
    //데미지 배율 초기화
    public void ResetFinalDamage()
    {
        myFinalDamage = 1;
        enemyFinalDamage = 1;
    }
    //내 데미지 배율 수정
    public void SetMyMultipleDamage(float multiple)
    {
        myFinalDamage = multiple;
    }
    //상대 데미지 배율 수정
    public void SetEnemyMultipleDamage(float multiple)
    {
        enemyFinalDamage = multiple;
    }
    //내 데미지 배율 추가
    public void AddMyMultipleDamage(float multiple)
    {
        myFinalDamage += multiple;
    }
    //상대 데미지 배율 수정
    public void AddEnemyMultipleDamage(float multiple)
    {
        enemyFinalDamage += multiple;
    }
    //내 데미지 배율 가져오기
    public float GetMyMultipleDamage()
    {
        return myFinalDamage;
    }
    //상대 데미지 배율 가져오기
    public float GetEnemyMultipleDamage()
    {
        return enemyFinalDamage;
    }
    //내 사리 스택 가져오기
    public int GetSariStack()
    {
        return sariStack;
    }
    public void AddSariStack(int add)
    {
        sariStack += add;
    }
    //사리 스택 변경
    public void SetSariStack(int stack)
    {
        sariStack = stack;
    }
    //만난 이벤트 반환
    public Dictionary<int, int> GetEncounteredEvent()
    {
        return encounteredEvent;
    }
    //만난 이벤트 추가
    public void AddEncounteredEvent(int id)
    {
        encounteredEvent[id] = id;
    }
    //아군 데이터 저장
    public void AddSavedMyUnits(RogueUnitDataBase unit)
    {
        savedMyUnits.Add(unit);
    }
    //저장된 데이터 초기화
    public void ClearSavedMyUnits()
    {
        savedMyUnits.Clear();
        foreach (var unit in myTeam)
        {
            savedMyUnits.Add(unit.Clone());
        }
    }

    public int GetChapter()
    {
        return chapter;
    }
    public void SetChapter(int chapter)
    {
        // 챕터가 변경되면 무지개 열쇠 사용 횟수 초기화
        if (this.chapter != chapter)
        {
            if (!rainbowKeyUsesPerChapter.ContainsKey(chapter))
            {
                rainbowKeyUsesPerChapter[chapter] = 0;
            }
        }
        this.chapter = chapter;
    }
    //챕터에 따른 이벤트 골드
    public int GetGoldByChapter(int gold, int battleResult = 0)
    {
        //승리일때만 골드
        if (battleResult != 0) return 0;
        float value = chapter == 1 ? 1 : (chapter == 2 ? 1.5f : 2);
        return ((int)(gold * value));
    }

    //챕터에 따른 이벤트 골드 획득
    public int AddGoldByEventChapter(int gold)
    {
        int getGold = GetGoldByChapter(gold);
        EarnGold(getGold);
        return getGold;
    }
    //랜덤유산 제거
    public void RemoveRelicById(int relicId)
    {
        foreach (var kvp in relicsByType)
        {
            var list = kvp.Value;
            list.RemoveAll(r => r.id == relicId);
        }

        foreach (var kvp in relicIdsByType)
        {
            kvp.Value.Remove(relicId);
        }

        ownedRelicsById.Remove(relicId);
        UnitStateChange.ChangeStateMyUnits();
    }


    public int GetNextUnitUniqueId()
    {
        return nextUnitUniqueId++;
    }

    public List<RogueUnitDataBase> GetSelectedUnits()
    {
        return selectedUnits;
    }
    public void AddSelectedUnits(RogueUnitDataBase unit)
    {
        selectedUnits.Add(unit);
    }
    public void SetSelectedUnits(List<RogueUnitDataBase> units)
    {
        selectedUnits = units;
    }
    public void ClearSelectedUnis()
    {
        selectedUnits.Clear();
    }
    public int GetFieldId()
    {
        return fieldId;
    }
    public void SetFieldId(int Id)
    {
        fieldId = Id;
    }
    public BattleRewardData GetBattleReward()
    {
        return battleReward;
    }
    public void AddGoldReward(int setGold)
    {
        battleReward.gold += setGold;
    }
    public void AddMoraleReward(int setMoraleReward)
    {
        battleReward.morale += setMoraleReward;
    }
    public void AddRerollChange(int addReroll)
    {
        battleReward.rerollChance += addReroll;
    }
    //리롤의 추가 및 사용
    public void AddReroll(int addReroll)
    {
        rerollChance += addReroll;
        if (addReroll < 0)
        {
            if (RelicManager.CheckRelicById(60))
            {
                WarRelic relic = RelicManager.GetRelicById(60);
                var vals = relic.GetAllValuesAsFloatListOrNull();
                if (vals != null && vals.Count > 0)
                {
                    var unit = RogueUnitDataBase.GetRandomUnitByRarity((int)vals[0]);
                    if (unit != null)
                    {
                        // 에너지 값이 없으면 기본 0으로 처리해서 인덱스 예외를 방지한다.
                        int energy = (vals.Count > 1) ? (int)vals[1] : 0;
                        unit.SetEnergyDirect(energy);
                        AddMyTeam(unit);
                    }
                }
            }
        }

    }
    public void AddRelicReward(int setRelicId)
    {
        battleReward.relicIds.Add(setRelicId);
    }
    public void AddUnitReward(RogueUnitDataBase setUnits)
    {
        battleReward.newUnits.Add(setUnits);
    }
    public void AddChangeReward(RogueUnitDataBase setChanges)
    {
        battleReward.changedUnits.Add(setChanges);
    }
    public void SetBattleReward(BattleRewardData battleReward)
    {
        this.battleReward = battleReward;
    }
    public void ClearBattleReward()
    {
        battleReward = new();
    }
    //버프 디버프 초기화
    public void ClearBuffDeBuff()
    {
        foreach (var unit in myUnits)
        {
            unit.effectDictionary.Clear();
        }
    }
    public int GetCostTable(int level)
    {
        float sale = 1;
        if (RelicManager.CheckRelicById(1)) sale -= 0.2f;

        return costTable[level];
    }

    public (int, bool) GetRerollChance()
    {
        bool canReroll = true;
        if (RelicManager.CheckRelicById(64))
        {
            canReroll = false;
        }

        return (rerollChance, canReroll);
    }
    public void SetRerollChance(int reroll)
    {
        rerollChance = reroll;
    }

    public void SetLoadData(List<int> eventId, int gold, int sentGold, int morale,
        int stageX, int stageY, int chapter, StageType stageType, int sariSatck, BattleRewardData battleReward, int nextUniqueId, int score)
    {
        encounteredEvent.Clear();

        if (eventId != null)
        {
            foreach (int id in eventId)
            {
                encounteredEvent[id] = id;
            }
        }

        this.currentGold = gold;
        this.spentGold = sentGold;
        this.playerMorale = morale;
        this.currentStageX = stageX;
        this.currentStageY = stageY;
        this.chapter = chapter;
        this.currentStageType = stageType;
        this.sariStack = sariSatck;
        this.battleReward = battleReward ?? new BattleRewardData();
        this.nextUnitUniqueId = nextUniqueId;
        this.score = score;
        SetStage();
    }

    private void EnsureUpgradeValues()
    {
        if (upgradeValues == null || upgradeValues.Length != UpgradeSlotCount)
        {
            UnitUpgrade[] normalized = new UnitUpgrade[UpgradeSlotCount];
            if (upgradeValues != null)
            {
                int copyCount = Mathf.Min(upgradeValues.Length, normalized.Length);
                Array.Copy(upgradeValues, normalized, copyCount);
            }

            upgradeValues = normalized;
        }

        for (int i = 0; i < upgradeValues.Length; i++)
        {
            if (upgradeValues[i] == null)
                upgradeValues[i] = new UnitUpgrade();
        }
    }

    // 사용처: 저장 데이터 로드 시 전술 개량 수치를 복원
    public void SetUpgradeValues(UnitUpgrade[] values)
    {
        upgradeValues = values;
        EnsureUpgradeValues();
    }

    //강화 반환
    public UnitUpgrade[] GetUpgradeValue()
    {
        EnsureUpgradeValues();
        return upgradeValues;
    }

    // 강화 수치 반환
    public int GetUpgrade(int unitTypeIndex, bool isAttack)
    {
        EnsureUpgradeValues();
        if (unitTypeIndex < 0 || unitTypeIndex >= upgradeValues.Length)
            return 0;

        return isAttack ? upgradeValues[unitTypeIndex].attackLevel : upgradeValues[unitTypeIndex].defenseLevel;
    }

    // 강화 수치 증가 (강화 비용 차감 포함)
    public void IncreaseUpgrade(int unitTypeIndex, bool isAttack, bool isPurchase = true)
    {
        EnsureUpgradeValues();
        if (unitTypeIndex < 0 || unitTypeIndex >= upgradeValues.Length)
            return;

        int currentLevel = isAttack
            ? upgradeValues[unitTypeIndex].attackLevel
            : upgradeValues[unitTypeIndex].defenseLevel;

        if (currentLevel >= 5)
            return;

        if (isPurchase)
        {
            if (!isFreeUpgrade)
            {
                float isSale = 1f;

                if (RelicManager.CheckRelicById(58))
                {
                    WarRelic relic58 = RelicManager.GetRelicById(58);
                    var vals = relic58.GetAllValuesAsFloatListOrNull();
                    if (vals != null)
                        isSale += vals[0];
                }

                int cost = Mathf.RoundToInt(costTable[currentLevel] * isSale);
                currentGold -= cost; // 실제 비용 차감
            }

            isFreeUpgrade = false;
        }

        if (isAttack)
            upgradeValues[unitTypeIndex].attackLevel++;
        else
            upgradeValues[unitTypeIndex].defenseLevel++;

        //유산 51
        if (RelicManager.CheckRelicById(51))
        {
            WarRelic relic = RelicManager.GetRelicById(51);
            var vals = relic.GetAllValuesAsFloatListOrNull();
            if (vals != null)
            {
                if (GetRandomFloat() < vals[0])
                {
                    RogueUnitDataBase addUnit = RogueUnitDataBase.GetRandomUnitByBranchAndRarity(unitTypeIndex, (int)vals[1]);
                    AddMyTeam(addUnit);
                }
            }

        }

        TryTriggerRelic3Reward(unitTypeIndex, isAttack);

        // 전술 개량 직후 유닛 스탯(StatBlock + 표시용 필드)에 즉시 반영
        UnitStateChange.ChangeStateMyUnits();
        foreach (var u in GetMyTeam())
        {
            u.ApplyModifiers();
        }
    }

    private void TryTriggerRelic3Reward(int unitTypeIndex, bool isAttack)
    {
        // 유산 3번 없으면 즉시 종료
        WarRelic relic = RelicManager.GetRelicById(3);
        if (relic == null || relic.used)
            return;

        // 현재 강화 상태 확인
        int atkLv = upgradeValues[unitTypeIndex].attackLevel;
        int defLv = upgradeValues[unitTypeIndex].defenseLevel;

        // 최초로 레벨 5 도달한 경우만
        if (atkLv == 5 || defLv == 5)
        {
            var vals = relic.GetAllValuesAsFloatListOrNull();
            if (vals != null && vals.Count > 1)
            {
                int gold = Mathf.RoundToInt(vals[1]);
                if (gold > 0)
                {
                    RogueLikeData.Instance.EarnGold(gold);
                }
            }

            relic.used = true; // 유산 사용 처리 (1회만)
        }
    }
    public (int unitType, bool isAttack) GetRandomUpgradeTarget()
    {
        // 총 16개 항목 중 하나 선택
        var random = GetRandomBySeed();
        int randomIndex = random.Next(0, 16);

        int unitType = randomIndex / 2;           // 0~7
        bool isAttack = (randomIndex % 2 == 0);   // 짝수면 공격, 홀수면 방어

        return (unitType, isAttack);
    }

    // 랜덤 병종 강화 (기존 함수 재사용)
    public void IncreaseRandomUpgrade(bool isPurchase = true)
    {
        var (unitType, isAttack) = GetRandomUpgradeTarget();
        IncreaseUpgrade(unitType, isAttack, isPurchase);
    }

    public void SetIsFreeUpgrade(bool isFree = true)
    {
        isFreeUpgrade = isFree;
    }

    public int GetMaxUnits()
    {
        int maxCount = maxUnits;
        int addMax = 0;
        float multy = 1;
        addMax += RelicManager.RunPantheonModel();              //유산 12
        addMax += RelicManager.RunBlindWarriorsEyepatch();      //유산 36
        addMax += RelicManager.RunMistakenOrderReceipt();       //유산 44
        addMax += RelicManager.RunExpandedFormationDiagram();   //유산 66
        addMax += RelicManager.RunWarlordsInsignia();           //유산 67
        addMax += RelicManager.RunTornList();                   //유산 89
        if (GetMyTeam().Find((e) => e.idx == 56) != null) addMax += 3;
        addMax += 5 * (chapter - 1);
        maxCount = maxCount + addMax;
        multy += RelicManager.RunPileOfMedals();
        maxCount = (int)(maxCount * multy);

        return maxCount;
    }
    public int GetCurrentEliteCommanderId()
    {
        if (currentStageType != StageType.Elite || StagePresetLoader.I == null)
            return 0;

        StagePreset preset = StagePresetLoader.I.GetByID(presetID);
        int commanderId = preset?.CommanderNumericId ?? 0;
        return commanderId >= 1 && commanderId <= 20 ? commanderId : 0;
    }

    public int GetEffectiveMaxUnitsForCurrentBattle(int enemyUnitCount = -1)
    {
        int maxCount = GetMaxUnits();

        if (GetCurrentEliteCommanderId() != 1)
            return maxCount;

        if (enemyUnitCount < 0)
            enemyUnitCount = enemyUnits?.Count ?? 0;

        return enemyUnitCount > 0 ? Mathf.Min(maxCount, enemyUnitCount) : maxCount;
    }

    public void SetMaxUnits(int maxUnits)
    {
        this.maxUnits = maxUnits;
    }
    public int GetPresetID()
    {
        return presetID;
    }
    public void SetPresetID(int presetID)
    {
        this.presetID = presetID;
    }

    public int GetMaxHero()
    {
        int addHero = 0;
        if (RelicManager.CheckRelicById(57))
        {
            WarRelic relic57 = RelicManager.GetRelicById(57);
            var vals = relic57.GetAllValuesAsFloatListOrNull();
            if (vals != null)
            {
                addHero += (int)vals[0];
            }
        }
        WarRelic deliciousSpecialMeal = RelicManager.GetRelicById(93);
        if (deliciousSpecialMeal != null)
        {
            var vals = deliciousSpecialMeal.GetAllValuesAsFloatListOrNull();
            if (vals != null)
            {
                if (deliciousSpecialMeal.used)
                {
                    addHero += (int)vals[2];
                }
                else
                {
                    int haveHero = 0;
                    var myTeamUnits = GetMyTeam();
                    foreach (var unit in myTeamUnits)
                    {
                        if (haveHero >= 2)
                        {
                            addHero += (int)vals[2];
                            break;
                        }
                        if (unit.rarity == 4)
                        {
                            haveHero++;
                        }
                    }
                }
            }
        }

        addHero += RelicManager.RunEpic();

        return maxHero + addHero;
    }
    public void SetMaxHero(int maxHero)
    {
        this.maxHero = maxHero;
    }

    public List<RogueUnitDataBase> GetMyTeam()
    {
        return myTeam;
    }
    public void SetMyTeam(List<RogueUnitDataBase> units)
    {
        this.myTeam = units;
    }

    public int GetBattleUnitCount()
    {
        return battleUnitCount;
    }
    public void SetBattleUnitCount(int battleCount)
    {
        this.battleUnitCount = battleCount;
    }

    public int GetScore()
    {
        return score;
    }
    public void AddScore(int score)
    {
        this.score += score;
    }
    public void ClearScore()
    {
        this.score = 0;
    }
    public bool GetClearChpater()
    {
        return clearChapter;
    }
    public void SetClearChapter(bool clearChapter)
    {
        this.clearChapter = clearChapter;
    }
    public void ResetToDefault()
    {
        ResetOwnedRelics();

        currentStageX = 1;
        currentStageY = 0;
        currentStageType = StageType.Combat;

        chapter = 1;
        currentGold = 150;
        spentGold = 0;
        playerMorale = 50;
        sariStack = 0;
        nextUnitUniqueId = 0;
        rerollChance = 2;
        score = 0;
        upgradeValues = new UnitUpgrade[UpgradeSlotCount];
        EnsureUpgradeValues();
        var baseUnits = RogueUnitDataBase.GetBaseUnits();
        currentStageSeedBase = 0;
        stageCallCount = 0;
        unitOrder = 0;

        nextEventToTreasure = false;
        clearChapter = false;
        resetMap = false;
        rainbowKeyUsesPerChapter?.Clear();

        SetMyTeam(baseUnits);
        SetAllMyUnits(baseUnits);

        encounteredEvent.Clear();
        SetRandomRandomSeed();
        ResetFinalDamage();

        isTestMode = false;
        currentStore = null;
        storeSessions = null;
        currentEvent = null;
        battleResume = null;
        progressState = SaveProgressState.StageSelect;
        hasLoadedSaveData = false;

    }

    public bool GetResetMap()
    {
        return resetMap;
    }
    public void SetResetMap(bool resetMap)
    {
        this.resetMap = resetMap;
    }

    //퀘스트 추가
    public Dictionary<int, QuestClass> GetQuestList()
    {
        return questState;
    }
    public void AddQuest(QuestClass q)
    {
        questState[q.id] = q;
    }

    //유산 저장 데이터로 초기화
    public void SetRelicBySaveData(List<WarRelic> warRelics)
    {
        foreach (RelicType type in Enum.GetValues(typeof(RelicType)))
        {
            relicsByType[type] = new List<WarRelic>();
            relicIdsByType[type] = new HashSet<int>();
        }
        ownedRelicsById.Clear();
        // warRelics를 타입별로 분류해서 추가
        foreach (WarRelic relic in warRelics)
        {
            WarRelicDatabase.RebindRuntime(relic);

            relicsByType[relic.type].Add(relic);
            relicIdsByType[relic.type].Add(relic.id);
            ownedRelicsById[relic.id] = relic;
        }
    }
    //랜덤 시드 고정
    public void SetRandomSeed(int seed)
    {
        randomSeed = seed;
    }
    public void SetStageCallCount(int count)
    {
        stageCallCount = Mathf.Max(0, count);
    }

    //랜덤 시드 무작위로 설정
    public void SetRandomRandomSeed()
    {
        int seed = Environment.TickCount ^ Guid.NewGuid().GetHashCode();
        randomSeed = seed;
        //randomSeed = 0;
    }
    //랜덤 시드 반환
    public int GetRandomSeed()
    {
        return randomSeed;
    }
    // 특정 스테이지 전용 랜덤 생성기 반환
    public System.Random GetRandomBySeed()
    {
        stageCallCount++;

        int stageSeed = currentStageSeedBase + stageCallCount;
        int finalSeed = unchecked((randomSeed * 397) ^ stageSeed);
        return new System.Random(finalSeed);
    }
    // 스테이지 진입 시 고유 번호 설정 + 카운터 초기화
    public void SetStage()
    {
        currentStageSeedBase =
           chapter * 1_000_0000
           + currentStageX * 100_000
           + currentStageY * 10_000
           + (int)currentStageType * 1_000;
        stageCallCount = 0;
    }
    //스테이지 위치로 랜덤 시드 생성
    public System.Random GetRandomStage(int currentX, int currentY)
    {
        int currentStageSeed = chapter * 1_000_0000 + currentX * 100_000 + currentY * 10_000;
        int finalSeed = HashCode.Combine(randomSeed, currentStageSeed);
        return new System.Random(finalSeed);
    }
    // 0~1 사이 랜덤값
    public float GetRandomFloat()
    {
        return (float)GetRandomBySeed().NextDouble();
    }
    // 정수 범위 랜덤값 (스테이지 기반)
    public int GetRandomInt(int min, int max)
    {
        return GetRandomBySeed().Next(min, max);
    }
    public bool GetTestMode()
    {
        return isTestMode;
    }
    public void SetTestMode(bool _isTestMode)
    {
        isTestMode = _isTestMode;
    }

    public void SetUnitOrder(int order)
    {
        unitOrder = order;
    }
    public int GetUnitOrder()
    {
        return unitOrder;
    }

    // 시드 + 챕터 값으로 랜덤 시드 반환 (스테이지 제작 시 필요)
    private int GetRandomBySeedChapter()
    {
        return chapter + randomSeed;
    }

    #region 47번 보물지도 관련 메서드

    public bool GetNextEventToTreasure()
    {
        return nextEventToTreasure;
    }

    public void SetNextEventToTreasure(bool value)
    {
        nextEventToTreasure = value;
    }

    #endregion

    #region 48번 무지개 열쇠 관련 메서드

    public int GetRainbowKeyUses(int chapter)
    {
        if (!rainbowKeyUsesPerChapter.ContainsKey(chapter))
        {
            rainbowKeyUsesPerChapter[chapter] = 0;
        }
        return rainbowKeyUsesPerChapter[chapter];
    }

    public void UseRainbowKey(int chapter)
    {
        if (!rainbowKeyUsesPerChapter.ContainsKey(chapter))
        {
            rainbowKeyUsesPerChapter[chapter] = 0;
        }
        rainbowKeyUsesPerChapter[chapter]++;
        Debug.Log($"[무지개 열쇠] 챕터 {chapter}에서 사용 횟수: {rainbowKeyUsesPerChapter[chapter]}/2");
    }

    public bool CanUseRainbowKey(int chapter)
    {
        return GetRainbowKeyUses(chapter) < 2;
    }

    public List<ChapterCounterSaveEntry> GetRainbowKeyUsesSnapshot()
    {
        return rainbowKeyUsesPerChapter
            .Select(kvp => new ChapterCounterSaveEntry(kvp.Key, kvp.Value))
            .ToList();
    }

    public void SetRainbowKeyUsesFromSave(List<ChapterCounterSaveEntry> entries)
    {
        rainbowKeyUsesPerChapter.Clear();

        if (entries == null)
            return;

        foreach (var entry in entries)
        {
            if (entry == null)
                continue;

            rainbowKeyUsesPerChapter[entry.chapter] = Mathf.Max(0, entry.count);
        }
    }

    #endregion

    #region 상점 스냅샷

    // 사용처: 상점 진입 시 최초 1회 라인업을 고정하거나, 기존 스냅샷을 되살림
    public StoreSnapshot OpenShopAndFreezeIfNeeded(
        int chapter, int x, int y,
        Func<List<UnitPackageOffer>> rollUnitPacks,
        Func<List<SimpleOffer>> rollRelics,
        Func<List<SimpleOffer>> rollItems,
        Func<SimpleOffer> rollReroll)
    {
        storeSessions ??= new Dictionary<string, StoreSnapshot>();
        string key = BuildStoreKey(chapter, x, y);

        if (!storeSessions.TryGetValue(key, out var snap))
        {
            snap = new StoreSnapshot
            {
                chapter = chapter,
                stageX = x,
                stageY = y,
                stageType = StageType.Shop
            };
            snap.unitPacks = rollUnitPacks?.Invoke() ?? new List<UnitPackageOffer>();
            snap.relics = rollRelics?.Invoke() ?? new List<SimpleOffer>();
            snap.items = rollItems?.Invoke() ?? new List<SimpleOffer>();
            snap.reroll = rollReroll?.Invoke();

            storeSessions[key] = snap;
        }
        currentStore = snap;
        progressState = SaveProgressState.StoreOpen;

        SetCurrentStage(x, y, StageType.Shop);

        SaveNow();
        return snap;
    }

    // 사용처: 현재 상점 스냅샷 읽기
    public StoreSnapshot GetCurrentStoreSnapshot() => currentStore;

    // 사용처: 구매 시 슬롯을 판매 상태로 잠금(이중 클릭 방지)

    public bool TryMarkSold(StoreSlotType type, int indexOrId)
    {
        if (currentStore == null) return false;

        switch (type)
        {
            case StoreSlotType.UnitPackage:
                if ((uint)indexOrId >= (uint)currentStore.unitPacks.Count) return false;
                if (currentStore.unitPacks[indexOrId].sold) return false;
                currentStore.unitPacks[indexOrId].sold = true;
                break;

            case StoreSlotType.Relic:
                if ((uint)indexOrId >= (uint)currentStore.relics.Count) return false;
                if (currentStore.relics[indexOrId].sold) return false;
                currentStore.relics[indexOrId].sold = true;
                break;

            case StoreSlotType.Item:
                if ((uint)indexOrId >= (uint)currentStore.items.Count) return false;
                if (currentStore.items[indexOrId].sold) return false;
                currentStore.items[indexOrId].sold = true;
                break;

            case StoreSlotType.Reroll:
                if (currentStore.reroll == null || currentStore.reroll.sold) return false;
                currentStore.reroll.sold = true;
                break;

            default:
                return false;
        }

        return true;
    }

    // 사용처: 결제 실패 등 예외 시 롤백 필요할 때
    public void UnmarkSold(StoreSlotType type, int indexOrId)
    {
        if (currentStore == null) return;

        switch (type)
        {
            case StoreSlotType.UnitPackage:
                if ((uint)indexOrId < (uint)currentStore.unitPacks.Count)
                    currentStore.unitPacks[indexOrId].sold = false;
                break;
            case StoreSlotType.Relic:
                if ((uint)indexOrId < (uint)currentStore.relics.Count)
                    currentStore.relics[indexOrId].sold = false;
                break;
            case StoreSlotType.Item:
                if ((uint)indexOrId < (uint)currentStore.items.Count)
                    currentStore.items[indexOrId].sold = false;
                break;
            case StoreSlotType.Reroll:
                if (currentStore.reroll != null)
                    currentStore.reroll.sold = false;
                break;
        }
    }

    #endregion

    public void SetIsDataLoading(bool isLoad)
    {
        isDataLoading = isLoad;
    }

    public void SetLanguage(int _language)
    {
        language = Mathf.Clamp(_language, 0, 2);
        PlayerPrefs.SetInt(LanguagePrefKey, language);
        PlayerPrefs.Save();
        GameTextDB.LoadFromRogueLike();
    }
    public int GetLanguage()
    {
        return language;
    }

    public float MasterVolume
    {
        get => masterVolume;
        set
        {
            float v = Mathf.Clamp01(value);
            if (Mathf.Approximately(masterVolume, v)) return;

            masterVolume = v;
            ApplyAudioVolumeToManager();
        }
    }

    public float BgmVolume
    {
        get => bgmVolume;
        set
        {
            float v = Mathf.Clamp01(value);
            if (Mathf.Approximately(bgmVolume, v)) return;

            bgmVolume = v;
            ApplyAudioVolumeToManager();
        }
    }

    public float SfxVolume
    {
        get => sfxVolume;
        set
        {
            float v = Mathf.Clamp01(value);
            if (Mathf.Approximately(sfxVolume, v)) return;

            sfxVolume = v;
            ApplyAudioVolumeToManager();
        }
    }

    // 사용처: 게임 시작 시 저장된 사운드 설정을 불러온다.
    public void LoadAudioSettings()
    {
        masterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumePrefKey, 1f));
        bgmVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(BgmVolumePrefKey, 1f));
        sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumePrefKey, 1f));

        ApplyAudioVolumeToManager();
    }

    // 사용처: 설정창을 닫을 때 현재 사운드 설정을 저장한다.
    public void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat(MasterVolumePrefKey, masterVolume);
        PlayerPrefs.SetFloat(BgmVolumePrefKey, bgmVolume);
        PlayerPrefs.SetFloat(SfxVolumePrefKey, sfxVolume);
        PlayerPrefs.Save();
    }

    // 사용처: RogueLikeData의 사운드 값을 실제 BGMManager에 반영한다.
    public void ApplyAudioVolumeToManager()
    {
        if (BGMManager.Instance == null)
            return;

        BGMManager.Instance.SetVolumes(masterVolume, bgmVolume, sfxVolume);
    }



}
