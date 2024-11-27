using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyLineupManager : MonoBehaviour
{
    [Header("적 리스트 표시")]
    [SerializeField] private Transform enemyListParent; // 적 리스트를 표시할 부모 오브젝트
    [SerializeField] private GameObject enemyUnitPrefab; // 유닛 정보를 표시할 프리팹
    

    [Header("설정")]
    [SerializeField] private int maxUnits = 20; // 최대 유닛 수
    private int enemyFunds; // 적 군비 (PlayerData에서 가져옴)


    private void Start()
    {
        // 적 군비를 PlayerData에서 가져오기
        enemyFunds = PlayerData.Instance.enemyFunds;

        if (enemyFunds <= 0)
        {
            Debug.LogError("적 군비가 설정되지 않았습니다.");
            return;
        }

        // 적 라인업 생성
        GenerateEnemyLineup();
    }

    public void GenerateEnemyLineup()
    {


        Debug.Log("적 유닛 구매 완료");
        // 모든 유닛 데이터 가져오기
        List<UnitDataBase> allUnits = UnitDataManager.Instance.GetAllUnits();

        if (allUnits == null || allUnits.Count == 0)
        {
            Debug.LogError("유닛 데이터가 없습니다.");
            return;
        }

        // 사용 가능한 병종 가져오기
        List<string> branches = allUnits.Select(u => u.unitBranch).Distinct().ToList();
        branches = ExcludeRandomBranches(branches);

        // 자금을 기준으로 적 라인업 생성
        List<UnitDataBase> selectedUnits = GenerateUnitsByBudget(allUnits, branches);

        // 적 유닛 배치
        List<UnitDataBase> enemyLineup = PlaceUnits(selectedUnits);

        // PlayerData에 저장
        PlayerData.Instance.SetEnemyUnits(enemyLineup);


        // Debug.Log로 확인
        Debug.Log("=== 적 유닛 라인업 ===");
        foreach (var unit in enemyLineup)
        {
            Debug.Log($"유닛 이름: {unit.unitName}, 병종: {unit.unitBranch}, 가격: {unit.unitPrice}");
        }

        // UI에 표시
        DisplayEnemyLineupUI(enemyLineup);
        DebugEnemyListIDX(enemyLineup);
    }

    private List<string> ExcludeRandomBranches(List<string> branches)
    {
        const string mandatoryBranch = "Branch_Bowman"; // 항상 포함되어야 하는 병종

        // Bowman 병종을 분리
        bool containsBowman = branches.Remove(mandatoryBranch);
        if (!containsBowman)
        {
            Debug.LogError("Bowman 병종이 유닛 데이터에 없습니다.");
        }

        // 나머지 병종 중 무작위로 2개의 병종 제외
        for (int i = 0; i < 2; i++)
        {
            if (branches.Count == 0) break; // 병종이 없으면 중단

            int randomIndex = Random.Range(0, branches.Count);
            Debug.Log($"제외된 병종: {branches[randomIndex]}");
            branches.RemoveAt(randomIndex); // 무작위 병종 제거
        }

        // Bowman 병종 다시 추가
        if (containsBowman)
        {
            branches.Add(mandatoryBranch);
            Debug.Log("Bowman 병종은 항상 포함됩니다.");
        }

        return branches;
    }

    private List<UnitDataBase> GenerateUnitsByBudget(List<UnitDataBase> allUnits, List<string> branches)
    {
        List<UnitDataBase> selectedUnits = new List<UnitDataBase>();
        // int fundsPerUnit = enemyFunds / maxUnits;
        int remainingFunds = enemyFunds; // 적의 총 자금
                                         // 두 가지 전략 중 무작위 선택
        Debug.Log($"초기 적 자금: {enemyFunds}, 사용 가능한 병종 수: {branches.Count}");

        bool prioritizeHighTier = Random.value > 0.5f;
        Debug.Log(prioritizeHighTier ? "구매 전략: 고급 유닛 우선" : "구매 전략: 균형형");

        // 병종별로 순환하면서 구매
        int maxIterations = 100; // 루프 안전장치
        while (remainingFunds > 0 && selectedUnits.Count < maxUnits && maxIterations-- > 0)
        {
            bool unitPurchasedInThisIteration = false; // 루프에서 유닛 구매 여부 확인

            foreach (string branch in branches)
            {
                

                // 해당 병종의 유닛 필터링
                List<UnitDataBase> branchUnits = allUnits.Where(u => u.unitBranch == branch).ToList();
                if (branchUnits.Count == 0) continue;

                // 고급/저급 우선 순위에 따라 유닛 정렬
                List<UnitDataBase> availableUnits = prioritizeHighTier
                    ? branchUnits.OrderByDescending(u => u.unitPrice).ToList()
                    : branchUnits.OrderBy(u => u.unitPrice).ToList();

                // 구매 가능한 유닛 중 하나 선택
                UnitDataBase unit = availableUnits.FirstOrDefault(u => u.unitPrice <= remainingFunds);
                if (unit == null) continue;

                // 유닛 구매
                selectedUnits.Add(unit);
                remainingFunds -= unit.unitPrice;
                unitPurchasedInThisIteration = true;

                Debug.Log($"유닛 선택: {unit.unitName}, 병종: {branch}, 남은 자금: {remainingFunds}");

                // 최대 유닛 수에 도달하면 루프 종료
                if (selectedUnits.Count >= maxUnits) break;
            }
        
            // 모든 병종을 순환했지만 유닛을 구매하지 못했다면 루프 종료
            if (!unitPurchasedInThisIteration)
            {
                Debug.LogWarning("더 이상 구매 가능한 유닛이 없습니다.");
                break;
            }
        }
            if (maxIterations <= 0)
            {
                Debug.LogError("루프가 너무 많이 실행되었습니다. 무한 루프 가능성 있음.");
            }
        

        Debug.Log($"총 선택된 유닛 수: {selectedUnits.Count}, 남은 자금: {remainingFunds}");
        return selectedUnits;
    }

    private List<UnitDataBase> PlaceUnits(List<UnitDataBase> purchasedUnits)
    {
        List<UnitDataBase> finalLineup = new List<UnitDataBase>();

        Debug.Log($"초기 배치 유닛 수: {purchasedUnits.Count}");

        // 방어형 유닛 (전열)
        List<UnitDataBase> defensiveUnits = purchasedUnits
            .Where(u => u.unitBranch == "Branch_Heavy_Infantry" || u.unitBranch == "Branch_Heavy_Cavalry")
            .ToList();
        finalLineup.AddRange(defensiveUnits); // 최대 5명까지 전열에 배치
        Debug.Log($"방어형 유닛 배치: {defensiveUnits.Count}명 중 {finalLineup.Count}명 추가");

        // 근접형 유닛 (전열/중간열)
        List<UnitDataBase> meleeUnits = purchasedUnits
            .Where(u => u.unitBranch == "Branch_Spearman" || u.unitBranch == "Branch_Warrior")
            .ToList();
        finalLineup.AddRange(meleeUnits); // 최대 5명까지 배치
        Debug.Log($"근접형 유닛 배치: {meleeUnits.Count}명 중 {finalLineup.Count}명 추가");

        // 후열 유닛 (사거리 2 이상)
        List<UnitDataBase> rangedUnits = purchasedUnits
            .Where(u => u.range >= 2 && (u.unitBranch == "Branch_Bowman" || u.unitBranch == "Branch_Assasin"))
            .ToList();
        finalLineup.AddRange(rangedUnits); // 최대 3명까지 후열에 배치
        Debug.Log($"후열 유닛 배치: {rangedUnits.Count}명 중 {finalLineup.Count}명 추가");

        // 나머지 유닛 (전열, 중간열, 후열 외에 남은 유닛들)
        List<UnitDataBase> remainingUnits = purchasedUnits.Except(finalLineup).ToList();

        // 남은 유닛을 무작위로 배치 (최대 7명까지)
        finalLineup.AddRange(remainingUnits.Take(maxUnits - finalLineup.Count));
        Debug.Log($"추가 배치: {remainingUnits.Count}명 중 {maxUnits - finalLineup.Count}명 추가");

        Debug.Log($"최종 배치 유닛 수: {finalLineup.Count}");
        return finalLineup;
    
}

    private void DisplayEnemyLineupUI(List<UnitDataBase> enemyLineup)
    {
        // 기존에 생성된 리스트 제거
        foreach (Transform child in enemyListParent)
        {
            Destroy(child.gameObject);
        }

        // 적 리스트 생성
        for (int i = 0; i < enemyLineup.Count; i++)
        {
            // 프리팹 생성
            GameObject enemyUnitUI = Instantiate(enemyUnitPrefab, enemyListParent);

            // EnemyUnitUI 스크립트를 가져와 유닛 데이터와 인덱스 설정
            EnemyUnitUI enemyUIComponent = enemyUnitUI.GetComponent<EnemyUnitUI>();
            if (enemyUIComponent != null)
            {
                enemyUIComponent.SetUnitData(enemyLineup[i]); // 유닛 데이터 설정
                enemyUIComponent.SetUnitIndex(i);            // 인덱스 설정 (0부터 시작)
            }
            else
            {
                Debug.LogError("EnemyUnitUI 스크립트를 프리팹에서 찾을 수 없습니다.");
            }
        }
    }
    public void DebugEnemyListIDX(List<UnitDataBase> enemyLineup)
    {
        List<int> enemyIndexes = PlayerData.Instance.GetEnemyUnitIndexes();
        Debug.Log("적 유닛 인덱스 목록:");
        foreach (var idx in enemyIndexes)
        {
            Debug.Log($"유닛 인덱스: {idx}");
        }
    }
    
}