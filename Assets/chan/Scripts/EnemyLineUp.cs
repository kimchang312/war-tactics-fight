using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class EnemyLineUp : MonoBehaviour
{

    [Header("적 리스트 표시")]
    [SerializeField] private Transform enemyListParent; // 적 리스트를 표시할 부모 오브젝트
    [SerializeField] private Transform enemyListParent2; //적 상세 1번줄
    [SerializeField] private Transform enemyListParent3; //적 상세 1번줄
    [SerializeField] private Transform enemyListParent4; //적 상세 1번줄
    [SerializeField] private GameObject enemyUnitPrefab; // 유닛 정보를 표시할 프리팹
    [SerializeField] private Sprite hiddenSprite;        // 숨김 처리된 유닛의 스프라이트

    [Header("적 라인업 구성 설정")]
    [SerializeField] private int maxUnits = 20; // 최대 유닛 수
    [SerializeField] private int initialFunds; // 초기 군비

    private int remainingFunds; // 남은 군비
    private List<UnitDataBase> selectedUnits = new List<UnitDataBase>(); // 최종 선택된 유닛 리스트
    private List<string> availableBranches;
    private bool branchesInitialized = false;

    // 씬 로드 시 유닛을 초기화
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬이 로드될 때 호출되어 유닛을 초기화
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeEnemyUnits();
    }



    private static readonly List<string> allBranches = new List<string>
    {
        "Branch_Spearman",
        "Branch_Warrior",
        "Branch_Bowman",
        "Branch_Heavy_Infantry",
        "Branch_Assassin",
        "Branch_Light_Calvary",
        "Branch_Heavy_Calvary"
    };



    private void Start()
    {
        // 병종 초기화
        InitializeBranches();

        // 초기 군비 설정
        initialFunds = PlayerData.Instance.enemyFunds;
        remainingFunds = initialFunds;

        // 적 라인업 구성
        GenerateEnemyLineup();

        // 라인업 출력
        Debug.Log("=== 적 라인업 ===");
        foreach (var unit in selectedUnits)
        {
            Debug.Log($"유닛 이름: {unit.unitName}, 가격: {unit.unitPrice}, 병종: {unit.unitBranch}");
        }

    }

    // 병종 초기화 및 제외 로직
    private void InitializeBranches()
    {
        availableBranches = new List<string>(allBranches);

        // 병종 제외 처리 (궁병은 항상 포함)
        List<string> branchesToExclude = new List<string>(availableBranches);
        branchesToExclude.Remove("Branch_Bowman"); // 궁병 제거해서 제외 후보로 설정

        for (int i = 0; i < 2; i++)
        {
            if (branchesToExclude.Count > 0)
            {
                int randomIndex = Random.Range(0, branchesToExclude.Count);
                string excludedBranch = branchesToExclude[randomIndex];
                availableBranches.Remove(excludedBranch); // 실제 사용 가능한 병종에서 제거
                branchesToExclude.RemoveAt(randomIndex); // 제외 후보에서도 제거
                Debug.Log($"제외된 병종: {excludedBranch}");
            }
        }

        Debug.Log($"최종 병종 목록: {string.Join(", ", availableBranches)}");
    }
    // 고급 유닛 우선형 전략
    private List<UnitDataBase> GenerateHighTierLineup(List<UnitDataBase> allUnits)
    {
        List<UnitDataBase> lineup = new List<UnitDataBase>();

        List<UnitDataBase> factionUnits = allUnits
            .Where(u => u.factionIdx == PlayerData.Instance.enemyFactionidx && availableBranches.Contains(u.unitBranch))
            .OrderByDescending(u => u.unitPrice)
            .ToList();

        while (remainingFunds > 0 && lineup.Count < maxUnits)
        {
            bool unitAdded = false;

            foreach (var unit in factionUnits)
            {
                if (remainingFunds >= unit.unitPrice)
                {
                    lineup.Add(unit);
                    remainingFunds -= unit.unitPrice;
                    Debug.Log($"[고급 전략] 유닛 추가: {unit.unitName}, 남은 군비: {remainingFunds}");
                    unitAdded = true;

                    if (lineup.Count >= maxUnits)
                        break;
                }
            }

            if (!unitAdded)
                break;
        }

        return lineup;
    }

    // 균형형 전략
    private List<UnitDataBase> GenerateBalancedLineup(List<UnitDataBase> allUnits)
    {
        List<UnitDataBase> lineup = new List<UnitDataBase>();

        List<string> branches = availableBranches;
        int fundsPerBranch = remainingFunds / branches.Count;
        Debug.Log($"병종별 예산: {fundsPerBranch}");

        foreach (var branch in branches)
        {
            List<UnitDataBase> branchUnits = allUnits
                .Where(u => u.unitBranch == branch && u.unitPrice <= fundsPerBranch)
                .OrderBy(u => u.unitPrice)
                .ToList();

            while (branchUnits.Count > 0 && remainingFunds > 0 && lineup.Count < maxUnits)
            {
                UnitDataBase unit = branchUnits[0];
                if (remainingFunds >= unit.unitPrice)
                {
                    lineup.Add(unit);
                    remainingFunds -= unit.unitPrice;
                    Debug.Log($"[균형 전략] 유닛 추가: {unit.unitName}, 남은 군비: {remainingFunds}");
                    branchUnits.RemoveAt(0);
                }
                else
                {
                    break;
                }
            }
        }

        List<UnitDataBase> remainingUnits = allUnits
            .Where(u => availableBranches.Contains(u.unitBranch))
            .OrderByDescending(u => u.unitPrice)
            .ToList();

        while (remainingFunds > 0 && lineup.Count < maxUnits)
        {
            bool unitAdded = false;

            foreach (var unit in remainingUnits)
            {
                if (remainingFunds >= unit.unitPrice)
                {
                    lineup.Add(unit);
                    remainingFunds -= unit.unitPrice;
                    Debug.Log($"[남은 예산] 추가: {unit.unitName}, 남은 군비: {remainingFunds}");
                    unitAdded = true;

                    if (lineup.Count >= maxUnits)
                        break;
                }
            }

            if (!unitAdded)
                break;
        }

        return lineup;
    }




    public void GenerateEnemyLineup()
    {
        PlayerData.Instance.SetRandomEnemyFaction();
        Debug.Log("적 유닛 구매 시작");

        // 모든 유닛 데이터 가져오기
        List<UnitDataBase> allUnits = UnitDataManager.Instance.GetAllUnits();

        if (allUnits == null || allUnits.Count == 0)
        {
            Debug.LogError("유닛 데이터가 없습니다.");
            return;
        }

        int playerFactionIdx = PlayerData.Instance.factionidx;
        int enemyFactionIdx = PlayerData.Instance.enemyFactionidx;

        Debug.Log($"플레이어 진영: {playerFactionIdx}, 적 진영: {enemyFactionIdx}");

        // 필터링 적용
        List<UnitDataBase> availableUnits = allUnits
            .Where(unit => unit.factionIdx == 0 || unit.factionIdx == enemyFactionIdx)
            .Where(unit => availableBranches.Contains(unit.unitBranch))
            .ToList();

        Debug.Log($"총 유닛 수: {allUnits.Count}, 필터링된 유닛 수: {availableUnits.Count}");

        foreach (var unit in availableUnits)
        {
            Debug.Log($"필터링된 유닛: {unit.unitName}, 병종: {unit.unitBranch}, 진영: {unit.factionIdx}");
        }

        if (availableUnits.Count == 0)
        {
            Debug.LogWarning("필터링된 유닛이 없습니다. 공통 유닛만 사용합니다.");

            availableUnits = allUnits
                .Where(unit => unit.factionIdx == 0)
                .Where(unit => availableBranches.Contains(unit.unitBranch))
                .ToList();

            if (availableUnits.Count == 0)
            {
                Debug.LogError("사용 가능한 유닛이 전혀 없습니다.");
                return;
            }
        }

        // 구매 전략 선택
        bool prioritizeHighTier = Random.value > 0.5f;
        Debug.Log(prioritizeHighTier ? "고급 유닛 우선 전략 선택" : "균형형 전략 선택");

        selectedUnits = prioritizeHighTier
            ? GenerateHighTierLineup(availableUnits)
            : GenerateBalancedLineup(availableUnits);

        // 디버그로 Branch_Assassin 확인
        foreach (var unit in selectedUnits)
        {
            Debug.Log($"선택된 유닛: {unit.unitName}, 병종: {unit.unitBranch}");
        }

        List<UnitDataBase> enemyLineup = PlaceUnits(selectedUnits);

        PlayerData.Instance.SetEnemyUnits(enemyLineup);

        Debug.Log("=== 최종 적 유닛 라인업 ===");
        foreach (var unit in enemyLineup)
        {
            Debug.Log($"배치된 유닛: {unit.unitName}, 병종: {unit.unitBranch}");
        }

        DisplayAndHideEnemyUnits(enemyLineup);
    }

    public void DisplayAndHideEnemyUnits(List<UnitDataBase> enemyLineup)
    {
        Debug.Log("여기서 생성");
        // 기존에 생성된 리스트 제거
        foreach (Transform child in enemyListParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in enemyListParent2)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in enemyListParent3)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in enemyListParent4)
        {
            Destroy(child.gameObject);
        }

        // 숨김 대상 유닛 인덱스 계산 (배치 상태가 false인 경우에만)
        List<int> hiddenIndexes = !ShopManager.Instance.isPlacingUnits
            ? GetHiddenIndexes(enemyLineup.Count)
            : new List<int>();

        // 적 리스트 생성
        for (int i = 0; i < enemyLineup.Count; i++)
        {
            // 프리팹 생성
            GameObject enemyUnitUI = Instantiate(enemyUnitPrefab);
            GameObject enemyDetailUI = Instantiate(enemyUnitPrefab);

            // EnemyUnitUI 스크립트를 가져와 유닛 데이터와 인덱스 설정
            EnemyUnitUI enemyUIComponent = enemyUnitUI.GetComponent<EnemyUnitUI>();
            EnemyUnitUI enemyUIComponent2 = enemyDetailUI.GetComponent<EnemyUnitUI>();
            if (enemyUIComponent != null)
            {
                if (!ShopManager.Instance.isPlacingUnits && hiddenIndexes.Contains(i))
                {
                    // 배치 상태가 아닌 경우에만 숨김 처리
                    enemyUIComponent.SetHidden("hiddenSprite");
                    enemyUIComponent2.SetHidden("hiddenSprite");
                }
                else
                {
                    // 정상 유닛 표시
                    enemyUIComponent.SetUnitData(enemyLineup[i]);
                    enemyUIComponent2.SetUnitData(enemyLineup[i]);
                }
                enemyUIComponent.SetUnitIndex(i); // 인덱스 설정
                enemyUIComponent2.SetUnitIndex(i); // 인덱스 설정
            }
            enemyUnitUI.transform.SetParent(enemyListParent);
            // 인덱스에 따라 해당 부모 오브젝트를 설정
            if (i >= 0 && i <= 6)
            {
                enemyDetailUI.transform.SetParent(enemyListParent2, false);  // 적절한 부모 설정
                
            }
            else if (i >= 7 && i <= 13)
            {
                enemyDetailUI.transform.SetParent(enemyListParent3, false);  // 적절한 부모 설정
                
            }
            else if (i >= 14 && i <= 20)
            {
                enemyDetailUI.transform.SetParent(enemyListParent4, false);  // 적절한 부모 설정
               
            }
            else
            {
                Debug.LogError("EnemyUnitUI 스크립트를 프리팹에서 찾을 수 없습니다.");
            }
            
        }
        // 레이아웃 갱신 (배치 상태가 변경되었을 때 UI 업데이트)
        LayoutRebuilder.ForceRebuildLayoutImmediate(enemyListParent.GetComponent<RectTransform>());
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



    public List<UnitDataBase> PlaceUnits(List<UnitDataBase> purchasedUnits)
    {
        List<UnitDataBase> finalLineup = new List<UnitDataBase>();
        int remainingSpaces = maxUnits; // 최대 배치 가능한 공간

        // 전열 및 궁병 필터링
        List<UnitDataBase> frontlineUnits = purchasedUnits
            .Where(u => u.unitBranch == "Branch_Heavy_Infantry" || u.unitBranch == "Branch_Heavy_Calvary")
            .ToList();

        List<UnitDataBase> bowmanUnits = purchasedUnits
            .Where(u => u.unitBranch == "Branch_Bowman")
            .ToList();

        // 전열 및 궁병을 제외한 나머지 병종
        List<UnitDataBase> remainingUnits = purchasedUnits
            .Where(u => u.unitBranch == "Branch_Warrior" || u.unitBranch == "Branch_Light_Calvary" || u.unitBranch == "Branch_Assasin" || u.unitBranch == "Branch_Spearman")
            .ToList();

        Debug.Log($"전열 유닛 수: {frontlineUnits.Count}, 궁병 유닛 수: {bowmanUnits.Count}, 기타 유닛 수: {remainingUnits.Count}");

        // 1. 전열 + 궁병 순환 배치
        while (remainingSpaces > 0 && (frontlineUnits.Count > 0 || bowmanUnits.Count > 0))
        {
            if (frontlineUnits.Count > 0)
            {
                finalLineup.Add(frontlineUnits[0]);
                frontlineUnits.RemoveAt(0);
                remainingSpaces--;
            }

            if (bowmanUnits.Count > 0 && remainingSpaces > 0)
            {
                finalLineup.Add(bowmanUnits[0]);
                bowmanUnits.RemoveAt(0);
                remainingSpaces--;
            }
        }

        // 2. 기타 병종 배치
        foreach (var unit in remainingUnits)
        {
            if (remainingSpaces <= 0)
                break;

            finalLineup.Add(unit);
            remainingSpaces--;
        }

        // 3. 누락된 유닛 강제 배치
        if (finalLineup.Count != purchasedUnits.Count)
        {
            Debug.LogWarning("누락된 유닛 발견! 남은 공간을 활용하여 강제 배치합니다.");
            var missingUnits = purchasedUnits.Except(finalLineup).ToList();

            foreach (var unit in missingUnits)
            {
                if (remainingSpaces <= 0)
                    break;

                finalLineup.Add(unit);
                remainingSpaces--;
                Debug.Log($"강제 배치된 유닛: {unit.unitName}, 병종: {unit.unitBranch}");
            }
        }

        // 최종 디버그 로그
        Debug.Log($"구매한 유닛 수: {purchasedUnits.Count}, 배치된 유닛 수: {finalLineup.Count}");

        if (finalLineup.Count != purchasedUnits.Count)
        {
            var missingUnits = purchasedUnits.Except(finalLineup).ToList();
            Debug.LogWarning("여전히 배치되지 않은 유닛 목록:");
            foreach (var unit in missingUnits)
            {
                Debug.LogWarning($"- {unit.unitName}, 병종: {unit.unitBranch}, 가격: {unit.unitPrice}");
            }
        }

        return finalLineup;
    }


    // 특정 병종 필터링 함수
    private List<UnitDataBase> FilterUnitsByBranch(List<UnitDataBase> units, string[] branches)
    {
        return units.Where(u => branches.Contains(u.unitBranch)).ToList();
    }


    
    public void DisplayEnemyLineupUI(List<UnitDataBase> enemyLineup)
    {
        Debug.Log("여기서 생성2");
        // 기존에 생성된 리스트 제거
        foreach (Transform child in enemyListParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in enemyListParent2)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in enemyListParent3)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in enemyListParent4)
        {
            Destroy(child.gameObject);
        }

        // 숨김 대상 유닛 인덱스 계산 (배치 상태가 false인 경우에만)
        List<int> hiddenIndexes = !ShopManager.Instance.isPlacingUnits
            ? GetHiddenIndexes(enemyLineup.Count)
            : new List<int>();

        // 적 리스트 생성
        for (int i = 0; i < enemyLineup.Count; i++)
        {
            // 프리팹 생성
            GameObject enemyUnitUI = Instantiate(enemyUnitPrefab, enemyListParent);
            GameObject enemyDetailUI = Instantiate(enemyUnitPrefab);
            
            // EnemyUnitUI 스크립트를 가져와 유닛 데이터와 인덱스 설정
            EnemyUnitUI enemyUIComponent = enemyUnitUI.GetComponent<EnemyUnitUI>();
            if (enemyUIComponent != null)
            {
                if (!ShopManager.Instance.isPlacingUnits && hiddenIndexes.Contains(i))
                {
                    // 배치 상태가 아닌 경우에만 숨김 처리
                    enemyUIComponent.SetHidden("hiddenSprite");
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
            // 인덱스에 따라 해당 부모 오브젝트를 설정
            if (i >= 0 && i <= 6)
            {
                enemyUnitUI.transform.SetParent(enemyListParent2, false);
            }
            else if (i >= 7 && i <= 13)
            {
                enemyUnitUI.transform.SetParent(enemyListParent3, false);
            }
            else if (i >= 14 && i <= 20)
            {
                enemyUnitUI.transform.SetParent(enemyListParent4, false);
            }
            
        }
        
    }
    
    
    // 유닛 초기화 (씬 로드 시 호출)
    private void InitializeEnemyUnits()
    {
        // 기존에 생성된 리스트 제거
        foreach (Transform child in enemyListParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in enemyListParent2)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in enemyListParent3)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in enemyListParent4)
        {
            Destroy(child.gameObject);
        }
    }
}