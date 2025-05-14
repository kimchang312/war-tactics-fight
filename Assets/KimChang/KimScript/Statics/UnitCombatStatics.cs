using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

public class UnitCombatStatics
{
    public int UnitID { get; private set; } // 유닛 고유 ID
    public string UnitName { get; private set; } // 유닛 이름

    // 통계 데이터
    // 피해량
    public float TotalDamageDealt { get; private set; }
    // 받은 피해량
    public float TotalDamageTaken { get; private set; }
    // 장갑으로 인한 피해 감소량
    public float ArmorDamageReduced { get; private set; }
    // 암살로 인한 처치 수
    public int UnitsAssassinated { get; private set; }
    // 처치 수
    public int UnitsKilled { get; private set; }
    // 회피 수
    public int TimesDodged { get; private set; }
    // 회피로 인한 피해 감소량(장갑 제외)
    public float DamageDodged { get; private set; }
    // 전투 충돌 횟수
    public int CombatEncounters { get; private set; }
    //원거리 피해
    public float RangedDamageDealt { get; private set; }
    //관통으로 인한 추가 피해
    public float PierceDamageGained { get; private set; }

    //특성 피해량
    public Dictionary<string, float> AdditionalDamageByTraits = new Dictionary<string, float>();
    //스킬 피해량
    public Dictionary<string, float> DamageBySkills = new Dictionary<string, float>();
    //스킬 사용 횟수
    public Dictionary<string, int> SkillUsageCount = new Dictionary<string, int>();
    //특성 사용 횟수
    public Dictionary<string, int> TraitUsageCount=new Dictionary<string, int>();
    //특성 피해 감소량
    public Dictionary<string, float> ReducedDamageByTraits = new Dictionary<string, float>();
    //스킬 피해 감소량
    public Dictionary<string, float> ReducedDamageBySkills = new Dictionary<string, float>();


    public UnitCombatStatics(int unitID, string unitName)
    {
        UnitID = unitID;
        UnitName = unitName;

        TotalDamageDealt = 0;
        TotalDamageTaken = 0;
        ArmorDamageReduced = 0;
        UnitsAssassinated = 0;
        UnitsKilled = 0;
        TimesDodged = 0;
        DamageDodged = 0;
        CombatEncounters = 0;
    }

    // 통계 업데이트 메서드
    public void AddDamageDealt(float damage) => TotalDamageDealt += damage;
    public void AddDamageTaken(float damage) => TotalDamageTaken += damage;
    public void AddArmorDamageReduced(float damage) => ArmorDamageReduced += damage;
    public void IncrementAssassinations() => UnitsAssassinated++;
    public void IncrementKills() => UnitsKilled++;
    public void IncrementDodges() => TimesDodged++;
    public void AddDodgedDamage(float damage) => DamageDodged += damage;
    public void IncrementCombatEncounters() => CombatEncounters++;
    public void AddRangedDamageDealt(float damage) => RangedDamageDealt += damage;
    public void AddPierceDamageGained(float damage) => PierceDamageGained += damage;

    //특성 데미지
    public void AddAdditionalDamageByTrait(string trait, float damage)
    {
        if (AdditionalDamageByTraits.ContainsKey(trait))
            AdditionalDamageByTraits[trait] += damage;
        else
            AdditionalDamageByTraits[trait] = damage;
    }
    //기술 데미지
    public void AddDamageBySkill(string skill, float damage)
    {
        if (DamageBySkills.ContainsKey(skill))
            DamageBySkills[skill] += damage;
        else
            DamageBySkills[skill] = damage;
    }
    //기술 발동
    public void IncrementSkillUsage(string skill)
    {
        if (SkillUsageCount.ContainsKey(skill))
            SkillUsageCount[skill]++;
        else
            SkillUsageCount[skill] = 1;
    }
    //특성 발동
    public void IncrementTraitUsage(string trait)
    {
        if (TraitUsageCount.ContainsKey(trait))
            TraitUsageCount[trait]++;
        else
            TraitUsageCount[trait] = 1;
    }

    // 팀 정보 추출 메서드
    public bool IsAlly()
    {
        return ((UnitID >> 31) & 1) == 0; // UnitID의 31번째 비트를 확인하여 팀 반환
    }

    //가장 높은 피해량 아군 
    public static UnitCombatStatics GetAllyWithHighestDamageDealt(List<UnitCombatStatics> unitStats)
    {
        return unitStats
            .Where(unit => unit.IsAlly()) // 아군만 필터링
            .OrderByDescending(unit => unit.TotalDamageDealt) // TotalDamageDealt 기준 내림차순 정렬
            .FirstOrDefault(); // 가장 높은 값 반환
    }
    //가장 많은 피해 받은 아군 
    public static UnitCombatStatics GetAllyWithHighestDamageTaken(List<UnitCombatStatics> unitStats)
    {
        return unitStats
            .Where(unit => unit.IsAlly()) // 아군만 필터링
            .OrderByDescending(unit => unit.TotalDamageTaken) // TotalDamageTaken 기준 내림차순 정렬
            .FirstOrDefault(); // 가장 높은 값 반환
    }
    //가장 높은 피해량 적군 
    public static UnitCombatStatics GetEnemyWithHighestDamageDealt(List<UnitCombatStatics> unitStats)
    {
        return unitStats
            .Where(unit => !unit.IsAlly()) // 적군만 필터링
            .OrderByDescending(unit => unit.TotalDamageDealt) // TotalDamageDealt 기준 내림차순 정렬
            .FirstOrDefault(); // 가장 높은 값 반환
    }
    //가장 많은 피해 받은 적군 
    public static UnitCombatStatics GetEnemyWithHighestDamageTaken(List<UnitCombatStatics> unitStats)
    {
        return unitStats
            .Where(unit => !unit.IsAlly()) // 적군만 필터링
            .OrderByDescending(unit => unit.TotalDamageTaken) // TotalDamageTaken 기준 내림차순 정렬
            .FirstOrDefault(); // 가장 높은 값 반환
    }

    // 피해량 골라내기
    public static string FilterHighDamage(UnitCombatStatics unitStats)
    {
        string result = "";

        // 유닛 ID 및 이름
        result += $"유닛 ID: {unitStats.UnitID}\n";
        result += $"유닛 이름: {unitStats.UnitName}\n";

        // 피해량
        result += $"피해량: {unitStats.TotalDamageDealt}\n";

        // 암살로 처치한 수 (조건부 추가)
        if (unitStats.UnitsAssassinated > 0)
        {
            result += $"암살로 처치한 수: {unitStats.UnitsAssassinated}\n";
        }

        // 처치 수 (조건부 추가)
        if (unitStats.UnitsKilled > 0)
        {
            result += $"처치 수: {unitStats.UnitsKilled}\n";
        }

        // 전투 충돌 횟수 (조건부 추가)
        if (unitStats.CombatEncounters > 0)
        {
            result += $"전투 충돌 횟수: {unitStats.CombatEncounters}\n";
        }

        // 관통 추가 피해 (조건부 추가)
        if (unitStats.PierceDamageGained > 0)
        {
            result += $"관통 추가 피해: {unitStats.PierceDamageGained}\n";
        }

        // 원거리 피해 (조건부 추가)
        if (unitStats.RangedDamageDealt > 0)
        {
            result += $"원거리 피해: {unitStats.RangedDamageDealt}\n";
        }

        // 특성 데미지 (조건부 추가)
        if (unitStats.AdditionalDamageByTraits.Count > 0)
        {
            result += "특성 데미지:\n";
            foreach (var trait in unitStats.AdditionalDamageByTraits)
            {
                result += $"  {trait.Key}: {trait.Value}\n";
            }
        }

        // 기술 데미지 (조건부 추가)
        if (unitStats.DamageBySkills.Count > 0)
        {
            result += "기술 데미지:\n";
            foreach (var skill in unitStats.DamageBySkills)
            {
                result += $"  {skill.Key}: {skill.Value}\n";
            }
        }

        // 특성 횟수 (조건부 추가)
        if (unitStats.TraitUsageCount.Count > 0)
        {
            result += "특성 횟수:\n";
            foreach (var trait in unitStats.TraitUsageCount)
            {
                result += $"  {trait.Key}: {trait.Value}\n";
            }
        }

        // 기술 횟수 (조건부 추가)
        if (unitStats.SkillUsageCount.Count > 0)
        {
            result += "기술 횟수:\n";
            foreach (var skill in unitStats.SkillUsageCount)
            {
                result += $"  {skill.Key}: {skill.Value}\n";
            }
        }

        return result;
    }

    // 받은 피해량 골라내기
    public static string FilterHighTaken(UnitCombatStatics unitStats)
    {
        string result = "";

        // 받은 피해량
        result += $"받은 피해량: {unitStats.TotalDamageTaken}\n";

        // 장갑 감소량 (조건부 추가)
        if (unitStats.ArmorDamageReduced > 0)
        {
            result += $"장갑 감소량: {unitStats.ArmorDamageReduced}\n";
        }

        // 회피 수 (조건부 추가)
        if (unitStats.TimesDodged > 0)
        {
            result += $"회피 수: {unitStats.TimesDodged}\n";
        }

        // 회피 피해 감소량 (조건부 추가)
        if (unitStats.DamageDodged > 0)
        {
            result += $"회피 피해 감소량: {unitStats.DamageDodged}\n";
        }

        // 특성 횟수 (조건부 추가)
        if (unitStats.TraitUsageCount.Count > 0)
        {
            result += "특성 횟수:\n";
            foreach (var trait in unitStats.TraitUsageCount)
            {
                result += $"  {trait.Key}: {trait.Value}\n";
            }
        }

        // 기술 횟수 (조건부 추가)
        if (unitStats.SkillUsageCount.Count > 0)
        {
            result += "기술 횟수:\n";
            foreach (var skill in unitStats.SkillUsageCount)
            {
                result += $"  {skill.Key}: {skill.Value}\n";
            }
        }

        return result;
    }

    // 통계 리스트에서 피해량 합 계산
    public static float GetTotalDamageDealt(List<UnitCombatStatics> unitStats, bool isTeam)
    {
        return unitStats.Where(unit => unit.IsAlly() == isTeam).Sum(unit => unit.TotalDamageDealt);
    }

    // 받은 피해량 합 계산
    public static float GetTotalDamageTaken(List<UnitCombatStatics> unitStats, bool isTeam)
    {
        return unitStats.Where(unit => unit.IsAlly() == isTeam).Sum(unit => unit.TotalDamageTaken);
    }


    // 전투 통계 저장
    private static void SaveCombatStatistics(Dictionary<int, UnitCombatStatics> unitStats)
    {
        // 현재 시간 기반 파일 이름 생성
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"); // 형식: 2023-12-31_23-59-59
        // 지정된 경로 설정
        string directoryPath = @"C:\Users\admin\Desktop\BattleStatics";
        string filePath = Path.Combine(directoryPath, $"BattleStatics.json_{timestamp}");

        // 폴더가 없으면 저장 작업 중단
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        // 통계 저장
        CombatStatisticsManager statsManager = new CombatStatisticsManager(unitStats);
        statsManager.SaveStatisticsToFile(filePath);


    }

}
