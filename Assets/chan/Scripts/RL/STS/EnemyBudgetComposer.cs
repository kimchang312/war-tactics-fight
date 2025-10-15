using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 뭉치 프리셋 데이터
/// </summary>
public class BundlePreset
{
    public int presetId;
    public List<int> unitIds;
    public int totalPrice;
    public List<RogueUnitDataBase> units;
    
    public BundlePreset(int id, List<int> ids)
    {
        presetId = id;
        unitIds = ids;
        units = new List<RogueUnitDataBase>();
        totalPrice = 0;
    }
}

/// <summary>
/// 적 부대 구성 결과
/// </summary>
public class EnemyCompositionResult
{
    public List<BundlePreset> purchasedBundles = new();
    public List<RogueUnitDataBase> expansionUnits = new();
    public List<RogueUnitDataBase> finalComposition = new();
    
    public int totalBudget;
    public int spentOnBundles;
    public int spentOnExpansion;
    public int remainingBudget;
    
    public bool isPurchasedPreset;
}

/// <summary>
/// 적 부대 예산 기반 구성 시스템
/// </summary>
public class EnemyBudgetComposer : MonoBehaviour
{
    public static EnemyBudgetComposer Instance { get; private set; }
    
    // 뭉치 프리셋 ID 범위
    private const int BUNDLE_PRESET_MIN = 37;
    private const int BUNDLE_PRESET_MAX = 129;
    
    // 확장 유닛 제외 리스트
    private static readonly int[] EXCLUDED_UNIT_IDS = { 13, 21, 24, 26, 28, 30, 36 };
    private static readonly string[] EXCLUDED_UNIT_NAMES = 
    { 
        "군의관", "월아 승병", "약탈자", "곤봉 승병", 
        "몰아치는 파도", "방랑자", "진홍 사제", "황금 사제" 
    };
    
    // 확장 유닛 최소 가격 (민병대 궁병)
    private const int MIN_EXPANSION_PRICE = 185;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 예산 기반으로 적 부대를 구성합니다.
    /// </summary>
    /// <param name="budget">전투 예산</param>
    /// <returns>구성 결과</returns>
    public EnemyCompositionResult ComposeEnemyArmy(int budget)
    {
        var result = new EnemyCompositionResult
        {
            totalBudget = budget,
            remainingBudget = budget,
            isPurchasedPreset = false
        };
        
        Debug.Log($"=== 적 부대 구성 시작 ===");
        Debug.Log($"📊 전투 예산: {budget}");
        
        // 1단계: 뭉치 프리셋 구매
        PurchaseBundlePresets(result);
        
        // 2단계: 확장 유닛 구매
        PurchaseExpansionUnits(result);
        
        // 3단계: 최종 배치
        ArrangeFinalComposition(result);
        
        // 결과 로그
        LogCompositionResult(result);
        
        return result;
    }
    
    /// <summary>
    /// 1단계: 뭉치 프리셋 구매
    /// </summary>
    private void PurchaseBundlePresets(EnemyCompositionResult result)
    {
        Debug.Log($"\n--- 뭉치 프리셋 구매 단계 ---");
        
        var random = RogueLikeData.Instance.GetRandomBySeed();
        
        while (true)
        {
            // 구매 가능한 뭉치 프리셋 찾기
            var availableBundles = GetAvailableBundles(result.remainingBudget);
            
            if (availableBundles.Count == 0)
            {
                Debug.Log($"💰 잔여 예산 {result.remainingBudget}으로 구매 가능한 뭉치 없음");
                break;
            }
            
            // 무작위로 하나 선택
            int randomIndex = random.Next(0, availableBundles.Count);
            var selectedBundle = availableBundles[randomIndex];
            
            // 구매
            result.purchasedBundles.Add(selectedBundle);
            result.remainingBudget -= selectedBundle.totalPrice;
            result.spentOnBundles += selectedBundle.totalPrice;
            result.isPurchasedPreset = true;
            
            Debug.Log($"✅ 뭉치 구매: ID {selectedBundle.presetId}, " +
                     $"가격 {selectedBundle.totalPrice}, " +
                     $"잔여 {result.remainingBudget}");
            Debug.Log($"   구성 유닛: {string.Join(", ", selectedBundle.units.Select(u => u.unitName))}");
        }
        
        Debug.Log($"📦 총 {result.purchasedBundles.Count}개 뭉치 구매 완료");
    }
    
    /// <summary>
    /// 2단계: 확장 유닛 구매
    /// </summary>
    private void PurchaseExpansionUnits(EnemyCompositionResult result)
    {
        Debug.Log($"\n--- 확장 유닛 구매 단계 ---");
        
        // 잔여 예산 150 미만이면 종료
        if (result.remainingBudget < 150)
        {
            Debug.Log($"💰 잔여 예산 {result.remainingBudget} < 150, 확장 유닛 구매 종료");
            return;
        }
        
        // 150 이상이면 100 추가
        result.remainingBudget += 100;
        Debug.Log($"💵 예산 +100 추가 → 잔여 예산: {result.remainingBudget}");
        
        var random = RogueLikeData.Instance.GetRandomBySeed();
        
        while (result.remainingBudget >= MIN_EXPANSION_PRICE)
        {
            RogueUnitDataBase purchasedUnit = null;
            
            // isPurchasedPreset = 1이면 뭉치 내 유닛 우선 구매
            if (result.isPurchasedPreset)
            {
                purchasedUnit = TryPurchaseFromBundleUnits(result, random);
            }
            
            // 뭉치에서 못 샀거나 isPurchasedPreset = 0이면 무작위 확장 유닛
            if (purchasedUnit == null)
            {
                purchasedUnit = PurchaseRandomExpansionUnit(result, random);
            }
            
            if (purchasedUnit == null)
            {
                Debug.Log($"💰 더 이상 구매 가능한 확장 유닛 없음");
                break;
            }
            
            result.expansionUnits.Add(purchasedUnit);
            int price = GetUnitPriceWithoutMorale(purchasedUnit);
            result.remainingBudget -= price;
            result.spentOnExpansion += price;
            
            Debug.Log($"✅ 확장 유닛 구매: {purchasedUnit.unitName}, " +
                     $"가격 {price}, 잔여 {result.remainingBudget}");
        }
        
        Debug.Log($"🎯 총 {result.expansionUnits.Count}개 확장 유닛 구매 완료");
    }
    
    /// <summary>
    /// 3단계: 최종 배치
    /// </summary>
    private void ArrangeFinalComposition(EnemyCompositionResult result)
    {
        Debug.Log($"\n--- 최종 배치 단계 ---");
        
        var random = RogueLikeData.Instance.GetRandomBySeed();
        var composition = new List<object>(); // object: BundlePreset 또는 RogueUnitDataBase
        
        // 1) 뭉치들을 무작위 순서로 배치
        var shuffledBundles = result.purchasedBundles.OrderBy(x => random.Next()).ToList();
        foreach (var bundle in shuffledBundles)
        {
            composition.Add(bundle);
        }
        
        // 2) 확장 유닛들을 배치
        foreach (var unit in result.expansionUnits)
        {
            if (unit.branchIdx == 2 || unit.branchIdx == 7)
            {
                // 맨 앞자리 제외하고 배치
                int position = composition.Count > 0 
                    ? random.Next(1, composition.Count + 1) 
                    : 0;
                composition.Insert(position, unit);
                Debug.Log($"   {unit.unitName} (병종 {unit.branchIdx}) → 위치 {position} (맨 앞 제외)");
            }
            else
            {
                // 무작위 위치 배치 (뭉치 앞 또는 뒤)
                int position = random.Next(0, composition.Count + 1);
                composition.Insert(position, unit);
                Debug.Log($"   {unit.unitName} (병종 {unit.branchIdx}) → 위치 {position}");
            }
        }
        
        // 3) 뭉치 압축 해제하여 최종 배치 확정
        foreach (var item in composition)
        {
            if (item is BundlePreset bundle)
            {
                // 뭉치 내부 유닛들 추가 (순서 유지)
                result.finalComposition.AddRange(bundle.units);
                Debug.Log($"   [뭉치 {bundle.presetId}] 압축 해제: {string.Join(", ", bundle.units.Select(u => u.unitName))}");
            }
            else if (item is RogueUnitDataBase unit)
            {
                result.finalComposition.Add(unit);
            }
        }
        
        Debug.Log($"✅ 최종 배치 완료: 총 {result.finalComposition.Count}개 유닛");
    }
    
    /// <summary>
    /// 구매 가능한 뭉치 프리셋 목록 가져오기
    /// </summary>
    private List<BundlePreset> GetAvailableBundles(int budget)
    {
        var availableBundles = new List<BundlePreset>();
        
        for (int presetId = BUNDLE_PRESET_MIN; presetId <= BUNDLE_PRESET_MAX; presetId++)
        {
            var preset = StagePresetLoader.I?.GetByID(presetId);
            if (preset == null || preset.UnitList == null) continue;
            
            var bundle = new BundlePreset(presetId, preset.UnitList);
            
            // 유닛 데이터 로드 및 가격 계산
            foreach (int unitId in preset.UnitList)
            {
                var unit = UnitLoader.Instance?.GetCloneUnitById(unitId, false);
                if (unit != null)
                {
                    bundle.units.Add(unit);
                    bundle.totalPrice += GetUnitPriceWithoutMorale(unit);
                }
            }
            
            // 예산 이하면 추가
            if (bundle.totalPrice <= budget)
            {
                availableBundles.Add(bundle);
            }
        }
        
        return availableBundles;
    }
    
    /// <summary>
    /// 뭉치 내 유닛에서 구매 시도
    /// </summary>
    private RogueUnitDataBase TryPurchaseFromBundleUnits(EnemyCompositionResult result, System.Random random)
    {
        // 뭉치에 포함된 모든 유닛 타입 수집
        var bundleUnitTypes = new HashSet<int>();
        foreach (var bundle in result.purchasedBundles)
        {
            foreach (var unit in bundle.units)
            {
                bundleUnitTypes.Add(unit.idx);
            }
        }
        
        // 구매 가능한 뭉치 유닛들
        var availableUnits = new List<RogueUnitDataBase>();
        foreach (int unitId in bundleUnitTypes)
        {
            var unit = UnitLoader.Instance?.GetCloneUnitById(unitId, false);
            if (unit != null)
            {
                int price = GetUnitPriceWithoutMorale(unit);
                if (price <= result.remainingBudget)
                {
                    availableUnits.Add(unit);
                }
            }
        }
        
        if (availableUnits.Count == 0)
        {
            // 가장 저렴한 보유 유닛이 예산보다 비싸면 무작위 확장 유닛으로
            return null;
        }
        
        // 무작위 선택
        int index = random.Next(0, availableUnits.Count);
        return availableUnits[index];
    }
    
    /// <summary>
    /// 무작위 확장 유닛 구매
    /// </summary>
    private RogueUnitDataBase PurchaseRandomExpansionUnit(EnemyCompositionResult result, System.Random random)
    {
        var allUnits = UnitLoader.Instance?.GetAllCachedUnits();
        if (allUnits == null) return null;
        
        // 확장 유닛 필터링
        var availableUnits = allUnits
            .Where(u => !IsExcludedUnit(u))
            .Where(u => GetUnitPriceWithoutMorale(u) <= result.remainingBudget)
            .ToList();
        
        if (availableUnits.Count == 0) return null;
        
        int index = random.Next(0, availableUnits.Count);
        return UnitLoader.Instance.GetCloneUnitById(availableUnits[index].idx, false);
    }
    
    /// <summary>
    /// 확장 유닛 제외 대상 확인
    /// </summary>
    private bool IsExcludedUnit(RogueUnitDataBase unit)
    {
        // IDX 체크
        if (EXCLUDED_UNIT_IDS.Contains(unit.idx)) return true;
        if (unit.idx >= 42 && unit.idx <= 58) return true;
        
        // 이름 체크
        if (EXCLUDED_UNIT_NAMES.Contains(unit.unitName)) return true;
        
        // 희귀도 4 체크
        if (unit.rarity == 4) return true;
        
        return false;
    }
    
    /// <summary>
    /// 기력 제외 가격 계산
    /// </summary>
    private int GetUnitPriceWithoutMorale(RogueUnitDataBase unit)
    {
        // 기본 가격 사용 (기력 영향 제외)
        return unit.unitPrice;
    }
    
    /// <summary>
    /// 구성 결과 로그 출력
    /// </summary>
    private void LogCompositionResult(EnemyCompositionResult result)
    {
        Debug.Log($"\n========== 적 부대 구성 완료 ==========");
        Debug.Log($"💰 총 예산: {result.totalBudget}");
        Debug.Log($"📦 뭉치 구매: {result.spentOnBundles} ({result.purchasedBundles.Count}개)");
        Debug.Log($"🎯 확장 유닛: {result.spentOnExpansion} ({result.expansionUnits.Count}개)");
        Debug.Log($"💵 잔여 예산: {result.remainingBudget}");
        Debug.Log($"\n🎖️ 최종 배치 ({result.finalComposition.Count}개 유닛):");
        
        for (int i = 0; i < result.finalComposition.Count; i++)
        {
            var unit = result.finalComposition[i];
            Debug.Log($"   [{i + 1}] {unit.unitName} (병종: {unit.branchIdx}, 가격: {GetUnitPriceWithoutMorale(unit)})");
        }
        
        Debug.Log($"======================================\n");
    }
}

