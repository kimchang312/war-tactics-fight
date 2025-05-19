using System;
using System.Collections.Generic;
using System.Linq;
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
    
    //실제 보유한 유닛
    private List<RogueUnitDataBase> myTeam = new();
    //전투에 사용되는 유닛
    private List<RogueUnitDataBase> myUnits = new();
    private List<RogueUnitDataBase> enemyUnits = new();
    private List<RogueUnitDataBase> savedMyUnits = new();
    private List<RogueUnitDataBase> selectedUnits = new(); //유닛 선택 화면에서 선택된 유닛들

    private Dictionary<RelicType, List<WarRelic>> relicsByType;
    private Dictionary<RelicType, HashSet<int>> relicIdsByType = new(); // 추가된 중복 체크용 HashSet
    private Dictionary<int, int> encounteredEvent = new();
    private int currentStageX = 1;
    private int currentStageY = 0;
    private int chapter = 1;
    private int presetID = -1;

    private StageType currentStageType = StageType.Combat;

    private int currentGold = 0;
    private int playerMorale = 50;
    private int spentGold = 0;
    private float myFinalDamage = 1;
    private float enemyFinalDamage = 1;

    private UnitUpgrade[] upgradeValues = new UnitUpgrade[8];

    private int sariStack = 0;

    private int[] costTable = { 100, 150, 200, 250, 300 };

    //현재 필드 id 전투 종료 후 0으로 1~5
    private int fieldId = 0;

    //전투 추가 보상
    private BattleRewardData battleReward = new();

    private int rerollChance = 0;

    private bool isFreeUpgrade =false;

    //실제 전투에 배치된 유닛 수
    private int battleUnitCount = 0;

    private int score = 0;

    private bool clearChapter=false;
    // 생성자에서 초기화
    private RogueLikeData()
    {
        relicsByType = new Dictionary<RelicType, List<WarRelic>>();
        relicIdsByType = new Dictionary<RelicType, HashSet<int>>();

        foreach (RelicType type in Enum.GetValues(typeof(RelicType)))
        {
            relicsByType[type] = new List<WarRelic>();
            relicIdsByType[type] = new HashSet<int>(); // 중복 검사용 HashSet 초기화
        }
        for (int i = 0; i < upgradeValues.Length; i++)
            upgradeValues[i] = new UnitUpgrade();
    }
    //내 데이터 전부 반환
    public SavePlayerData GetRogueLikeData()
    {
        SavePlayerData data = new(0, myUnits, relicIdsByType.Values
                                    .SelectMany(hashSet => hashSet)
                                    .ToList(),encounteredEvent.Values.ToList(),
                                    currentGold,spentGold,playerMorale,currentStageX,currentStageY,chapter,currentStageType,
                                    upgradeValues, sariStack,battleReward, nextUnitUniqueId,score);
        return data;
    }
    // 보유한 유닛 기력만 재설정 해서 반환
    public SavePlayerData GetBattleEndRogueLikeData(List<RogueUnitDataBase> units, List<RogueUnitDataBase> deadUnits)
    {
        // 저장용 복사 리스트 생성 (깊은 복사하지 않고 원본 savedMyUnits를 수정)
        List<RogueUnitDataBase> savedCopy = new(savedMyUnits);
        // 전투에 참여한 유닛 전부 순회 (생존 + 사망)
        foreach (var unit in units.Concat(deadUnits))
        {
            var savedUnit = savedCopy.Find(u => u.UniqueId == unit.UniqueId);
            if (savedUnit == null) continue;

            if (unit.energy < 1)
            {
                savedCopy.Remove(savedUnit); // 기력이 0이면 제거
            }
            else
            {
                savedUnit.energy = unit.energy; // 기력 갱신
            }
        }

        SavePlayerData data = new(
            0,
            savedCopy,
            relicIdsByType.Values.SelectMany(hashSet => hashSet).ToList(),
            encounteredEvent.Values.ToList(),
            currentGold,
            spentGold,
            playerMorale,
            currentStageX,
            currentStageY,
            chapter,
            currentStageType,
            upgradeValues,
            sariStack,
            battleReward,
            nextUnitUniqueId,
            score
        );
        myTeam = savedCopy;
        return data;
    }

    //내 유닛 전부 수정하기
    public void SetAllMyUnits(List<RogueUnitDataBase> units)
    {
        myUnits = new List<RogueUnitDataBase>(units);
    }
    //내 유닛 하나 추가
    public void AddMyUnis(RogueUnitDataBase unit)
    {
        
        int heroCount = 0;
        int maxHeroCount = GetMaxHero();
        foreach(var one in myTeam)
        {
            if(one.branchIdx==8) heroCount++;
        }
        if(heroCount >= maxHeroCount)
        {
            RelicManager.HandleRandomRelic(10, RelicManager.RelicAction.Acquire);
        }
        else
        {
            myTeam.Add(unit);
        }
        
    }

    //상대 유닛 전부 수정하기
    public void SetAllEnemyUnits(List<RogueUnitDataBase> units)
    {
        enemyUnits = new List<RogueUnitDataBase>(units);
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
    // 중복 방지 유물 추가 함수
    public void AcquireRelic(int relicId)
    {
        WarRelic relic = WarRelicDatabase.GetRelicById(relicId);
        if (relic != null && relicsByType.ContainsKey(relic.type))
        {
            if (!relicIdsByType[relic.type].Contains(relicId))
            {
                if (relicId == 53 && UnityEngine.Random.value <= 0.75f)
                {
                    RelicManager.HandleRandomRelic(10, RelicManager.RelicAction.Acquire);
                }

                relicsByType[relic.type].Add(relic);
                relicIdsByType[relic.type].Add(relicId); // 중복 관리 HashSet에도 추가

                //획득 시 발동
                if (relic.type == RelicType.GetEffect)
                {
                    relic.executeAction();
                }

            }
        }
    }
    //특정 타입 유물만 가져오기
    public List<WarRelic> GetRelicsByType(RelicType type)
    {
        HashSet<int> uniqueIds = new();
        List<WarRelic> result = new();

        void AddUniqueRelics(RelicType relicType)
        {
            if (!relicsByType.ContainsKey(relicType)) return;
            foreach (var relic in relicsByType[relicType])
            {
                if (uniqueIds.Add(relic.id)) // 중복된 ID 방지
                {
                    result.Add(relic);
                }
            }
        }

        AddUniqueRelics(type);

        if (type == RelicType.StateBoost || type == RelicType.BattleActive)
        {
            AddUniqueRelics(RelicType.ActiveState);
        }

        return result;
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
                    return;
                }
            }
        }
    }

    // 특정 ID
    public WarRelic GetOwnedRelicById(int relicId)
    {
        foreach (var relicList in relicsByType.Values)
        {
            foreach (var relic in relicList)
            {
                if (relic.id == relicId)
                {
                    return relic;
                }
            }
        }
        return null;
    }

    // 보유한 모든 유물을 반환하는 함수
    public List<WarRelic> GetAllOwnedRelics()
    {
        List<WarRelic> allRelics = new();

        foreach (var relicList in relicsByType.Values)
        {
            allRelics.AddRange(relicList);
        }

        return allRelics;
    }

    // 보유한 모든 유물의 ID만 반환하는 함수
    public List<int> GetAllOwnedRelicIds()
    {
        List<int> relicIds = new();

        foreach (var relicList in relicsByType.Values)
        {
            foreach (var relic in relicList)
            {
                relicIds.Add(relic.id);
            }
        }

        return relicIds;
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
    }
    //현재 스테이지 수정
    public void SetCurrentStage(int x, int y, StageType type)
    {
        currentStageX = x;
        currentStageY = y;
        currentStageType = type;
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
    //골드 획득
    public void EarnGold(int gold)
    {
        float addGold = GetOwnedRelicById(5) == null ? 0 : 0.15f;
        gold = (int)((addGold+1)*gold);
        currentGold += gold;
    }
    //골드 감소
    public void ReduceGold(int gold)
    {
        bool hasLoanRelic = RogueLikeData.Instance.GetOwnedRelicById(49) != null;
        int minGold = hasLoanRelic ? -500 : 0;

        int availableGold = currentGold - gold;

        if (availableGold < minGold)
        {
            // 초과 사용한 만큼 차감 불가능
            int allowedUsage = currentGold - minGold;
            spentGold += allowedUsage;
            currentGold = minGold;
        }
        else
        {
            spentGold += gold;
            currentGold = availableGold;
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
        int morale;
        if (value >= 0)
        {
            if (RelicManager.CheckRelicById(116)) return 0;
            // 증가: 최대 100 제한
            morale= Math.Min(100, playerMorale + value);
            playerMorale = morale;
        }
        else
        {
            // 감소: 유산 33 보유 시 20% 감소량 완화
            float reductionModifier = GetOwnedRelicById(33) == null ? 1f : 0.8f;
            int reduced = (int)(-value * reductionModifier);
            morale = Math.Max(0, playerMorale - reduced);
            playerMorale = morale;
        }
        return morale;
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
    //사리 스택 변경
    public void SetSariStack(int stack)
    {
        sariStack =stack;
    }
    /*
    public void Clear()
    {
        myUnits.Clear();
        enemyUnits.Clear();
        ResetOwnedRelics();
    }
    */
    //만난 이벤트 반환
    public Dictionary<int,int> GetEncounteredEvent()
    {
        return encounteredEvent;
    }
    //만난 이벤트 추가
    public void AddEncounteredEvent(int id)
    {
        encounteredEvent.Add(id, id);
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
        savedMyUnits = myTeam;
    }

    public int GetChapter()
    {
        return chapter;
    }
    public void SetChapter(int chapter)
    {
        this.chapter = chapter;
    }
    //챕터에 따른 이벤트 골드
    public int GetGoldByChapter(int gold)
    {
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
        foreach(var unit in myUnits)
        {
            unit.effectDictionary.Clear();
        }
    }
    public int GetCostTable(int level)
    {
        return costTable[level];
    }

    public int GetRerollChance()
    {
        return rerollChance;
    }
    public void SetRerollChance(int reroll)
    {
        rerollChance = reroll;
    }

    public void SetLoadData(List<int> eventId,int gold,int sentGold,int morale,
        int stageX,int stageY,int chapter, StageType stageType,int sariSatck,BattleRewardData battleReward,int nextUniqueId,int score)
    {
        foreach(int id in eventId)
        {
            encounteredEvent[id] = id;
        }
        this.currentGold = gold;
        this.spentGold = sentGold;
        this.playerMorale = morale;
        this.currentStageX = stageX;
        this.currentStageY = stageY;
        this.chapter = chapter;
        this.currentStageType = stageType;
        this.sariStack = sariSatck;
        this.battleReward = battleReward;
        this.nextUnitUniqueId = nextUniqueId;
        this.score = score;  
    }
    //강화 반환
    public UnitUpgrade[] GetUpgradeValue()
    {
        return upgradeValues;
    }

    // 강화 수치 반환
    public int GetUpgrade(int unitTypeIndex, bool isAttack)
    {
        return isAttack ? upgradeValues[unitTypeIndex].attackLevel : upgradeValues[unitTypeIndex].defenseLevel;
    }

    // 강화 수치 증가 (강화 비용 차감 포함)
    public void IncreaseUpgrade(int unitTypeIndex, bool isAttack,bool isPurchase=true)
    {
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
                // 단계별 비용 테이블
                float isSale = RelicManager.CheckRelicById(1) ? 0.2f : 0;
                isSale += RelicManager.CheckRelicById(58) ? -0.2f : 0;

                int cost = costTable[currentLevel];

                if (currentGold < cost)
                    return; // 금화 부족

                // 금화 차감
                currentGold -= cost;
            }
            isFreeUpgrade = false;
        }

        // 강화 수치 증가
        if (isAttack)
            upgradeValues[unitTypeIndex].attackLevel++;
        else
            upgradeValues[unitTypeIndex].defenseLevel++;
    }

    public (int unitType, bool isAttack) GetRandomUpgradeTarget()
    {
        // 총 16개 항목 중 하나 선택
        int randomIndex = UnityEngine.Random.Range(0, 16);

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
        if (RelicManager.CheckRelicById(66)) addMax += 1;
        if (RelicManager.CheckRelicById(67)) addMax += 3;
        if (RelicManager.CheckRelicById(89)) addMax -= 2;
        maxCount = maxCount +addMax;
        if (RelicManager.CheckRelicById(107)) maxCount /= 2;

        return maxCount;
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
        this.presetID=presetID;
    }

    public int GetMaxHero()
    {
        int addHero = 0;
        if (RelicManager.CheckRelicById(57))
        {
            addHero += 1;
        }
        if (RelicManager.CheckRelicById(110))
        {
            addHero += 2;
        }
        
        return maxHero;
    }
    public void SetMaxHero(int maxHero)
    {
        this.maxHero = maxHero;
    }

    public List<RogueUnitDataBase> GetMyTeam()
    {
        return myTeam;
    }
    public void AddMyTeam(RogueUnitDataBase unit)
    {
        myTeam.Add(unit);
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
        // 유닛 초기화: 기본 유닛 로드
        var baseUnits = RogueUnitDataBase.GetBaseUnits();
        SetMyTeam(baseUnits);
        SetAllMyUnits(baseUnits);

        // 유물 초기화
        ResetOwnedRelics();

        // 스테이지 위치 초기화
        currentStageX = 1;
        currentStageY = 0;
        currentStageType = StageType.Combat;

        // 챕터 및 전투 수치 초기화
        chapter = 1;
        currentGold = 0;
        spentGold = 0;
        playerMorale = 50;
        sariStack = 0;
        nextUnitUniqueId = 0;

        // 이벤트 초기화
        encounteredEvent.Clear();

        // 데미지 배율 초기화
        ResetFinalDamage();
    }

}
