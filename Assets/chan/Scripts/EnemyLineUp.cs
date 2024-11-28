using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyLineUp : MonoBehaviour
{
    [Header("적 리스트 표시")]
    [SerializeField] private Transform enemyListParent; // 적 리스트를 표시할 부모 오브젝트
    [SerializeField] private GameObject enemyUnitPrefab; // 유닛 정보를 표시할 프리팹
    [SerializeField] private Sprite hiddenSprite;        // 숨김 처리된 유닛의 스프라이트

    [Header("설정")]
    [SerializeField] private int maxUnits = 20; // 최대 유닛 수
    private int enemyFunds; // 적 군비 (PlayerData에서 가져옴)


    private void Start()
    {
        Debug.Log("EnemyLineupManager의 Start 함수가 호출되었습니다."); // 디버그 확인
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

        PlayerData.Instance.SetRandomEnemyFaction();
        Debug.Log("적 유닛 구매 완료");
        // 모든 유닛 데이터 가져오기
        List<UnitDataBase> allUnits = UnitDataManager.Instance.GetAllUnits();

        if (allUnits == null || allUnits.Count == 0)
        {
            Debug.LogError("유닛 데이터가 없습니다.");
            return;
        }

        // 플레이어 진영의 인덱스 가져오기
        int playerFactionIdx = PlayerData.Instance.factionidx;
        int enemyFactionIdx = PlayerData.Instance.enemyFactionidx;
        Debug.Log($"플레이어가 선택한 진영 인덱스: {playerFactionIdx}");

        // 적의 진영 유닛과 공통 유닛(진영 인덱스가 0인 유닛)만 필터링
        List<UnitDataBase> availableUnits = allUnits
            .Where(unit => unit.factionIdx == 0 || unit.factionIdx == enemyFactionIdx) // 공통 유닛(진영 인덱스 0) 포함, 나머지는 플레이어 진영 제외
            .ToList();
        Debug.Log($"필터링된 유닛 수: {availableUnits.Count}");

        // 필터링 결과 디버그 로그
        foreach (var unit in availableUnits)
        {
            Debug.Log($"적이 사용 가능한 유닛 이름: {unit.unitName}, 진영 인덱스: {unit.factionIdx}");
        }

        if (availableUnits.Count == 0)
        {
            Debug.LogError("플레이어 진영을 제외한 유닛이 없습니다.");
            return;
        }
        // 사용 가능한 병종 가져오기
        List<string> branches = availableUnits.Select(u => u.unitBranch).Distinct().ToList();
        branches = ExcludeRandomBranches(branches);

        // 자금을 기준으로 적 라인업 생성
        List<UnitDataBase> selectedUnits = GenerateUnitsByBudget(availableUnits, branches);

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
    private void DisplayAndHideEnemyUnits(List<UnitDataBase> enemyLineup)
    {
        // 기존에 생성된 리스트 제거
        foreach (Transform child in enemyListParent)
        {
            Destroy(child.gameObject);
        }

        // 숨김 대상 유닛 인덱스 계산
        List<int> hiddenIndexes = GetHiddenIndexes(enemyLineup.Count);

        // 적 리스트 생성
        for (int i = 0; i < enemyLineup.Count; i++)
        {
            // 프리팹 생성
            GameObject enemyUnitUI = Instantiate(enemyUnitPrefab, enemyListParent);

            // EnemyUnitUI 스크립트를 가져와 유닛 데이터와 인덱스 설정
            EnemyUnitUI enemyUIComponent = enemyUnitUI.GetComponent<EnemyUnitUI>();
            if (enemyUIComponent != null)
            {
                if (hiddenIndexes.Contains(i))
                {
                    // 숨김 처리
                    enemyUIComponent.SetHidden(hiddenSprite);
                }
                else
                {
                    // 정상 유닛 표시
                    enemyUIComponent.SetUnitData(enemyLineup[i]);
                }
                enemyUIComponent.SetUnitIndex(i); // 인덱스 설정
            }
            else
            {
                Debug.LogError("EnemyUnitUI 스크립트를 프리팹에서 찾을 수 없습니다.");
            }
        }
    }
    private List<int> GetHiddenIndexes(int totalUnits)
    {
        // 숨김 유닛 수 계산
        int hiddenCount = Mathf.FloorToInt(totalUnits * 0.4f);

        // 1부터 N까지의 유닛 번호 생성
        List<int> allIndexes = Enumerable.Range(0, totalUnits).ToList();

        // 숨김 유닛 번호 무작위 선택
        List<int> hiddenIndexes = new List<int>();
        for (int i = 0; i < hiddenCount; i++)
        {
            int randomIndex = Random.Range(0, allIndexes.Count);
            hiddenIndexes.Add(allIndexes[randomIndex]);
            allIndexes.RemoveAt(randomIndex); // 선택된 번호 제거
        }

        Debug.Log($"숨김 유닛 인덱스: {string.Join(", ", hiddenIndexes)}");
        return hiddenIndexes;
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
        int remainingFunds = enemyFunds;
        Debug.Log($"초기 적 자금: {enemyFunds}, 사용 가능한 병종 수: {branches.Count}");

        bool prioritizeHighTier = Random.value > 0.5f;  // 50% 확률로 고급 유닛 우선 전략 선택
        Debug.Log(prioritizeHighTier ? "구매 전략: 고급 유닛 우선" : "구매 전략: 균형형");

        // 병종별로 순환하면서 구매
        int maxIterations = 100; // 루프 안전장치
        while (remainingFunds > 0 && selectedUnits.Count < maxUnits && maxIterations-- > 0)
        {
            bool unitPurchasedInThisIteration = false;

            // 고급 유닛 우선 전략일 경우 진영 유닛을 우선적으로 구매
            if (prioritizeHighTier)
            {
                List<UnitDataBase> factionUnits = allUnits.Where(u => u.factionIdx == PlayerData.Instance.enemyFactionidx).ToList();
                factionUnits = factionUnits.OrderByDescending(u => u.unitPrice).ToList();

                foreach (var unit in factionUnits)
                {
                    if (unit.unitPrice <= remainingFunds)
                    {
                        selectedUnits.Add(unit);
                        remainingFunds -= unit.unitPrice;
                        unitPurchasedInThisIteration = true;
                        Debug.Log($"유닛 선택: {unit.unitName}, 병종: {unit.unitBranch}, 남은 자금: {remainingFunds}");
                        if (selectedUnits.Count >= maxUnits) break;
                    }
                }
            }

            // 병종별 예산 배분 후, 예산에 맞는 유닛 구매
            foreach (string branch in branches)
            {
                if (remainingFunds <= 0 || selectedUnits.Count >= maxUnits) break;

                List<UnitDataBase> branchUnits = allUnits.Where(u => u.unitBranch == branch).ToList();
                if (branchUnits.Count == 0) continue;

                List<UnitDataBase> availableUnits = prioritizeHighTier
                    ? branchUnits.OrderByDescending(u => u.unitPrice).ToList()
                    : branchUnits.OrderBy(u => u.unitPrice).ToList();

                // 구매 가능한 유닛 중 하나 선택
                UnitDataBase unit = availableUnits.FirstOrDefault(u => u.unitPrice <= remainingFunds);
                if (unit != null)
                {
                    selectedUnits.Add(unit);
                    remainingFunds -= unit.unitPrice;
                    unitPurchasedInThisIteration = true;
                    Debug.Log($"유닛 선택: {unit.unitName}, 병종: {branch}, 남은 자금: {remainingFunds}");
                }
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
    private List<UnitDataBase> GenerateBalancedUnitsByBudget(List<UnitDataBase> allUnits, List<string> branches)
    {
        List<UnitDataBase> selectedUnits = new List<UnitDataBase>();
        int remainingFunds = enemyFunds;
        Debug.Log($"초기 적 자금: {enemyFunds}, 사용 가능한 병종 수: {branches.Count}");

        // 병종별 예산 배분
        int fundsPerBranch = remainingFunds / branches.Count;
        Debug.Log($"각 병종에 할당된 예산: {fundsPerBranch}");

        // 각 병종에 대해 무작위로 유닛 구매
        foreach (string branch in branches)
        {
            List<UnitDataBase> branchUnits = allUnits.Where(u => u.unitBranch == branch).ToList();
            if (branchUnits.Count == 0) continue;

            List<UnitDataBase> availableUnits = branchUnits.OrderBy(u => u.unitPrice).ToList();
            int branchFunds = fundsPerBranch;

            while (branchFunds > 0)
            {
                UnitDataBase unit = availableUnits.FirstOrDefault(u => u.unitPrice <= branchFunds);
                if (unit == null) break; // 더 이상 구매할 유닛이 없으면 종료

                selectedUnits.Add(unit);
                branchFunds -= unit.unitPrice;
                Debug.Log($"병종: {branch}, 유닛 선택: {unit.unitName}, 남은 예산: {branchFunds}");
            }
        }

        // 남은 예산으로 비싼 유닛 추가 구매
        remainingFunds -= selectedUnits.Sum(u => u.unitPrice);
        Debug.Log($"남은 예산: {remainingFunds}");

        while (remainingFunds > 0 && selectedUnits.Count < maxUnits)
        {
            List<UnitDataBase> expensiveUnits = allUnits.OrderByDescending(u => u.unitPrice).ToList();
            foreach (var unit in expensiveUnits)
            {
                if (unit.unitPrice <= remainingFunds)
                {
                    selectedUnits.Add(unit);
                    remainingFunds -= unit.unitPrice;
                    Debug.Log($"유닛 선택: {unit.unitName}, 남은 자금: {remainingFunds}");
                    if (selectedUnits.Count >= maxUnits) break;
                }
            }
        }

        Debug.Log($"최종 선택된 유닛 수: {selectedUnits.Count}, 남은 자금: {remainingFunds}");
        return selectedUnits;
    }
    private void FinalizeUnitSelection(List<UnitDataBase> selectedUnits)
    {
        // 총 유닛 수가 20명을 초과하면 더 이상 유닛을 구매하지 않음
        if (selectedUnits.Count > maxUnits)
        {
            selectedUnits = selectedUnits.Take(maxUnits).ToList();
            Debug.LogWarning("유닛 수가 20명을 초과하여, 최대 20명으로 제한되었습니다.");
        }

        // 예산 부족으로 더 이상 유닛을 구매할 수 없는 경우
        int remainingFunds = enemyFunds - selectedUnits.Sum(u => u.unitPrice);
        if (remainingFunds <= 0)
        {
            Debug.LogWarning("남은 군비로 추가 유닛을 구매할 수 없습니다.");
        }
    }
    public List<UnitDataBase> PlaceUnits(List<UnitDataBase> purchasedUnits)
    {
        List<UnitDataBase> finalLineup = new List<UnitDataBase>();

        // 구입한 유닛 수만큼 배치 공간 설정
        int remainingSpaces = purchasedUnits.Count;
        Debug.Log($"배치 가능한 유닛 수: {remainingSpaces}");

        // 2. 전열 유닛 배치
        List<UnitDataBase> frontlineUnits = new List<UnitDataBase>();

        // 방어형 유닛 (중갑병, 중기병)
        List<UnitDataBase> defensiveUnits = purchasedUnits
            .Where(u => u.unitBranch == "Branch_Heavy Infantry" || u.unitBranch == "Branch_Heavy Cavalry")
            .OrderByDescending(u => u.health)
            .ToList();

        if (defensiveUnits.Count > 0 && remainingSpaces > 0)
        {
            frontlineUnits.Add(defensiveUnits[Random.Range(0, defensiveUnits.Count)]);
            remainingSpaces--;
            Debug.Log($"전열에 방어형 유닛 배치: {frontlineUnits.Last().unitName}");
        }
        else if (remainingSpaces > 0)
        {
            // 방어형 유닛이 없으면 근접 공격형 유닛 배치 (창병, 전사)
            List<UnitDataBase> meleeUnits = purchasedUnits
                .Where(u => u.unitBranch == "Branch_Spearman" || u.unitBranch == "Branch_Warrior")
                .OrderByDescending(u => u.health)
                .ToList();

            if (meleeUnits.Count > 0)
            {
                frontlineUnits.Add(meleeUnits[Random.Range(0, meleeUnits.Count)]);
                remainingSpaces--;
                Debug.Log($"전열에 근접 공격형 유닛 배치: {frontlineUnits.Last().unitName}");
            }
        }

        // 방어형 유닛 또는 근접 유닛이 없으면, 체력이 가장 높은 유닛을 배치
        if (frontlineUnits.Count == 0 && remainingSpaces > 0)
        {
            List<UnitDataBase> remainingUnits = purchasedUnits
                .OrderByDescending(u => u.health)
                .ToList();
            frontlineUnits.Add(remainingUnits[0]);
            remainingSpaces--;
            Debug.Log($"전열에 체력이 가장 높은 유닛 배치: {frontlineUnits.Last().unitName}");
        }

        finalLineup.AddRange(frontlineUnits);
        Debug.Log($"전열 유닛 배치 완료. 남은 배치 공간: {remainingSpaces}");

        // 3. 후열 유닛 배치 (궁병 그룹)
        List<UnitDataBase> rangedUnits = purchasedUnits
            .Where(u => u.unitBranch == "Branch_Bowman")
            .OrderBy(u => u.unitPrice)  // 가격 순서대로 배치
            .ToList();

        List<List<UnitDataBase>> archerGroups = new List<List<UnitDataBase>>();
        while (rangedUnits.Count >= 2 && remainingSpaces > 0)
        {
            // 2명 또는 3명씩 그룹화
            int groupSize = (remainingSpaces > 1) ? Random.Range(2, 4) : 2;
            List<UnitDataBase> group = rangedUnits.Take(groupSize).ToList();
            archerGroups.Add(group);
            rangedUnits.RemoveRange(0, groupSize);
            remainingSpaces -= groupSize;
            Debug.Log($"후열에 궁병 그룹 배치: {group.Count}명. 남은 배치 공간: {remainingSpaces}");
        }

        // 각 궁병 그룹 사이에 간격을 둡니다.
        foreach (var group in archerGroups)
        {
            finalLineup.AddRange(group);
        }

        Debug.Log($"후열 유닛 배치 완료. 남은 배치 공간: {remainingSpaces}");

        // 4. 나머지 유닛 무작위 배치
        List<UnitDataBase> remainingUnitsForRandomPlacement = purchasedUnits.Except(finalLineup).ToList();
        while (remainingSpaces > 0 && remainingUnitsForRandomPlacement.Count > 0)
        {
            UnitDataBase unitToPlace = remainingUnitsForRandomPlacement[Random.Range(0, remainingUnitsForRandomPlacement.Count)];
            finalLineup.Add(unitToPlace);
            remainingUnitsForRandomPlacement.Remove(unitToPlace);
            remainingSpaces--;
            Debug.Log($"무작위 배치: {unitToPlace.unitName}. 남은 배치 공간: {remainingSpaces}");
        }

        Debug.Log($"최종 배치 유닛 수: {finalLineup.Count}");

        return finalLineup;  // 유닛 배치 결과를 반환
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