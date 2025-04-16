using System;
using System.Collections.Generic;
using System.Linq;

public class RogueLikeData
{
    private static RogueLikeData instance;

    public static RogueLikeData Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new RogueLikeData();
            }
            return instance;
        }
    }
    
    private int nextUnitUniqueId = 0;

    private List<RogueUnitDataBase> myUnits = new List<RogueUnitDataBase>();
    private List<RogueUnitDataBase> enemyUnits = new List<RogueUnitDataBase>();
    private List<RogueUnitDataBase> savedMyUnits = new List<RogueUnitDataBase>();

    private Dictionary<RelicType, List<WarRelic>> relicsByType;
    private Dictionary<RelicType, HashSet<int>> relicIdsByType = new(); // 추가된 중복 체크용 HashSet
    private Dictionary<int, int> encounteredEvent = new();
    private int currentStageX = 1;
    private int currentStageY = 0;
    private StageType currentStageType = StageType.Combat;

    private int chapter = 1;
    private int currentGold = 0;
    private int playerMorale = 50;
    private int spentGold = 0;
    private float myFinalDamage = 1;
    private float enemyFinalDamage = 1;

    private int sariStack = 0;
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
    }
    //내 데이터 전부 반환
    public SavePlayerData GetRogueLikeData()
    {
        SavePlayerData data = new SavePlayerData(0, savedMyUnits, relicIdsByType.Values
                                    .SelectMany(hashSet => hashSet)
                                    .ToList(),encounteredEvent.Values.ToList(),
                                    currentGold,spentGold,playerMorale,currentStageX,currentStageY,currentStageType,sariStack);
        return data;
    }
    // 보유한 유닛 기력만 재설정 해서 반환
    public SavePlayerData GetBattleEndRogueLikeData(List<RogueUnitDataBase> units, List<RogueUnitDataBase> deadUnits)
    {
        // 저장용 복사 리스트 생성 (깊은 복사하지 않고 원본 savedMyUnits를 수정)
        List<RogueUnitDataBase> savedCopy = new List<RogueUnitDataBase>(savedMyUnits);

        // 전투에 참여한 유닛 전부 순회 (생존 + 사망)
        foreach (var unit in units.Concat(deadUnits))
        {
            //UnityEngine.Debug.Log(savedCopy[0].UniqueId+""+unit.UniqueId);
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

        SavePlayerData data = new SavePlayerData(
            0,
            savedCopy,
            relicIdsByType.Values.SelectMany(hashSet => hashSet).ToList(),
            encounteredEvent.Values.ToList(),
            currentGold,
            spentGold,
            playerMorale,
            currentStageX,
            currentStageY,
            currentStageType,
            sariStack
        );

        return data;
    }

    //내 유닛 전부 수정하기
    public void SetAllMyUnits(List<RogueUnitDataBase> units)
    {
        myUnits = new List<RogueUnitDataBase>(units);
    }
    //내 유닛 하나 추가
    public void SetAddMyUnis(RogueUnitDataBase unit)
    {
        myUnits.Add(unit);
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
            // HashSet을 사용한 빠른 중복 확인
            if (!relicIdsByType[relic.type].Contains(relicId))
            {
                relicsByType[relic.type].Add(relic);
                relicIdsByType[relic.type].Add(relicId); // 중복 관리 HashSet에도 추가
            }
        }
    }
    //특정 타입 유물만 가져오기
    public List<WarRelic> GetRelicsByType(RelicType type)
    {
        HashSet<int> uniqueIds = new HashSet<int>();
        List<WarRelic> result = new List<WarRelic>();

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
        List<WarRelic> result = new List<WarRelic>();

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

    // 특정 ID의 유물을 가져오는 함수
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
        return null; // 보유한 유물 중 해당 ID의 유물이 없음
    }

    // 보유한 모든 유물을 반환하는 함수
    public List<WarRelic> GetAllOwnedRelics()
    {
        List<WarRelic> allRelics = new List<WarRelic>();

        foreach (var relicList in relicsByType.Values)
        {
            allRelics.AddRange(relicList);
        }

        return allRelics;
    }

    // 보유한 모든 유물의 ID만 반환하는 함수
    public List<int> GetAllOwnedRelicIds()
    {
        List<int> relicIds = new List<int>();

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
    //현재 골드 가져오기
    public int GetCurrentGold()
    {
        return currentGold;
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
    public void SetEncounteredEvent(int id)
    {
        encounteredEvent.Add(id, id);
    }
    //아군 데이터 저장
    public void SetSavedMyUnits(RogueUnitDataBase unit)
    {
        savedMyUnits.Add(unit);
    }
    public int GetChapter()
    {
        return chapter;
    }
    public void SetChapter(int chapter)
    {
        this.chapter = chapter;
    }
    //챕터에 따른 이벤트 골드 획득
    public int AddGoldByEventChapter(int gold)
    {
        float value = chapter == 1 ? 1 : (chapter == 2 ? 1.5f : 2);
        int getGold = ((int)(gold * value));
        currentGold += getGold;
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

}
