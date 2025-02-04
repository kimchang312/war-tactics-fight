using System;
using System.Collections.Generic;

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

    private List<UnitDataBase> myUnits = new List<UnitDataBase>();
    private List<UnitDataBase> enemyUnits = new List<UnitDataBase>();
    private Dictionary<RelicType, List<WarRelic>> relicsByType;

    private int currentStageX = 0;
    private int currentStageY = 0;
    private StageType currentStageType = StageType.Battle;

    private int maxGold=0;
    private int currentGold = 0;
    private int earnedGold = 0;
    private int spentGold = 0;

    private float myFinalDamage = 1;
    private float enemyFinalDamage = 1;

    private RogueLikeData()
    {
        relicsByType = new Dictionary<RelicType, List<WarRelic>>();
        foreach (RelicType type in Enum.GetValues(typeof(RelicType)))
        {
            relicsByType[type] = new List<WarRelic>();
        }
    }

    //내 유닛 전부 수정하기
    public void AllMyUnits(List<UnitDataBase> units)
    {
        myUnits = new List<UnitDataBase>(units);
    }

    //상대 유닛 전부 수정하기
    public void AllEnemyUnits(List<UnitDataBase> units)
    {
        enemyUnits = new List<UnitDataBase>(units);
    }

    //내 유닛 가져오기
    public List<UnitDataBase> GetMyUnits()
    {
        return new List<UnitDataBase>(myUnits);
    }

    //상대 유닛 가져오기
    public List<UnitDataBase> GetEnemyUnits()
    {
        return new List<UnitDataBase>(enemyUnits);
    }

    //내 유닛 하나 수정하기
    public void UpdateMyUnit(int uniqueId, UnitDataBase updatedUnit)
    {
        for (int i = 0; i < myUnits.Count; i++)
        {
            if (myUnits[i].UniqueId == uniqueId)
            {
                myUnits[i] = updatedUnit;
                return;
            }
        }
    }

    //상대 유닛 하나 수정하기
    public void UpdateEnemyUnit(int uniqueId, UnitDataBase updatedUnit)
    {
        for (int i = 0; i < enemyUnits.Count; i++)
        {
            if (enemyUnits[i].UniqueId == uniqueId)
            {
                enemyUnits[i] = updatedUnit;
                return;
            }
        }
    }

    //유물 추가하기
    public void AcquireRelic(int relicId)
    {
        WarRelic relic = WarRelicDatabase.GetRelicById(relicId);
        if (relic != null && relicsByType.ContainsKey(relic.type))
        {
            relicsByType[relic.type].Add(relic);
        }
    }

    //특정 타입 유물만 가져오기
    public List<WarRelic> GetRelicsByType(RelicType type)
    {
        List<WarRelic> result = new List<WarRelic>();
        if (relicsByType.ContainsKey(type))
        {
            result.AddRange(relicsByType[type]);
        }
        if (type == RelicType.StateBoost || type == RelicType.BattleActive)
        {
            if (relicsByType.ContainsKey(RelicType.ActiveState))
            {
                result.AddRange(relicsByType[RelicType.ActiveState]);
            }
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

    //현재 골드 수정
    public void SetCurrentGold(int gold)
    {
        currentGold = gold;
    }

    //현재 골드 가져오기
    public int GetCurrentGold()
    {
        return currentGold;
    }
    //사용한 골드 가져오기
    public int GetReduceGold()
    {
        return spentGold;
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

    public void Clear()
    {
        myUnits.Clear();
        enemyUnits.Clear();
        foreach (var type in relicsByType.Keys)
        {
            relicsByType[type].Clear();
        }
    }
}
