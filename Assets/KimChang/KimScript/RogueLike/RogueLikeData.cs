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
    public enum StageType
    {
        Battle,
        Chest,
        Shop,
        Unknown,
        Rest,
        Elite,
        Boss
    }

    private List<RogueUnitDataBase> myUnits = new List<RogueUnitDataBase>();
    private List<RogueUnitDataBase> enemyUnits = new List<RogueUnitDataBase>();

    private Dictionary<RelicType, List<WarRelic>> relicsByType;
    private Dictionary<RelicType, HashSet<int>> relicIdsByType = new(); // 추가된 중복 체크용 HashSet

    private int currentStageX = 0;
    private int currentStageY = 0;
    private StageType currentStageType = StageType.Battle;

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
        SavePlayerData data = new SavePlayerData(0,myUnits, relicIdsByType.Values
                                    .SelectMany(hashSet => hashSet)
                                    .ToList(),
                                    currentGold,spentGold,playerMorale,currentStageX,currentStageY,currentStageType,sariStack);
        return data;
    }
    //내 유닛 전부 수정하기
    public void AllMyUnits(List<RogueUnitDataBase> units)
    {
        myUnits = new List<RogueUnitDataBase>(units);
    }

    //상대 유닛 전부 수정하기
    public void AllEnemyUnits(List<RogueUnitDataBase> units)
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
    //현재 골드 가져오기
    public int GetCurrentGold()
    {
        return currentGold;
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
    /*
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
    */
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

}
