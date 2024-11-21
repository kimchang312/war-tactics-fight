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

    
    public void Start()
    {
        GenerateEnemyLineup(); // 적 유닛 생성
        //DisplayEnemyLineupUI(); // 생성된 유닛을 UI에 표시
    }

    public void GenerateEnemyLineup()
    {
        Debug.Log("GenerateEnemyLineup 호출됨");
        //ExcludeRandomBranches(); // 무작위 병종 제외
        Debug.Log("무작위 병종 제외 완료");
        //PurchaseUnits(); // 적 유닛 구매 로직 호출
        Debug.Log("적 유닛 구매 완료");
        List<UnitDataBase> purchasedUnits = UnitDataManager.Instance.GetAllUnits();

        if (purchasedUnits == null || purchasedUnits.Count == 0)
        {
            Debug.LogError("유닛 데이터가 없습니다.");
            return;
        }

        List<UnitDataBase> enemyUnits = PlaceUnits(purchasedUnits);

        // 적 유닛 리스트를 PlayerData에 저장
        PlayerData.Instance.SetEnemyUnits(enemyUnits);

        // 생성된 적 유닛을 디버그로 확인
        Debug.Log("적 유닛 라인업:");
        foreach (var unit in enemyUnits)
        {
            Debug.Log($"유닛: {unit.unitName}");
        }
    }

    private List<string> ExcludeRandomBranches(List<string> branches)
    {
        // Bowman은 항상 포함
        branches.Remove("Bowman");

        // 무작위로 2개의 병종 제외
        for (int i = 0; i < 2; i++)
        {
            if (branches.Count == 0) break;

            int randomIndex = Random.Range(0, branches.Count);
            Debug.Log($"제외된 병종: {branches[randomIndex]}");
            branches.RemoveAt(randomIndex);
        }

        // Bowman 다시 추가
        branches.Add("Bowman");
        return branches;
    }

    private List<UnitDataBase> PurchaseUnits(List<UnitDataBase> allUnits, List<string> branches)
    {
        List<UnitDataBase> purchasedUnits = new List<UnitDataBase>();
        int fundsPerUnit = enemyFunds / maxUnits;

        // 두 가지 구매 전략 중 무작위 선택
        bool prioritizeHighTier = Random.value > 0.5f;
        Debug.Log(prioritizeHighTier ? "구매 전략: 고급 유닛 우선형" : "구매 전략: 균형형");

        foreach (string branch in branches)
        {
            List<UnitDataBase> branchUnits = allUnits.Where(u => u.unitBranch == branch).ToList();
            int remainingFunds = enemyFunds / branches.Count;

            while (remainingFunds > 0 && purchasedUnits.Count < maxUnits)
            {
                // 유닛 필터링 (고급/저급 유닛 우선 순위)
                List<UnitDataBase> availableUnits = prioritizeHighTier
                    ? branchUnits.OrderByDescending(u => u.unitPrice).ToList()
                    : branchUnits.OrderBy(u => u.unitPrice).ToList();

                // 구매 가능한 유닛 선택
                UnitDataBase unit = availableUnits.FirstOrDefault(u => u.unitPrice <= remainingFunds);
                if (unit == null) break;

                purchasedUnits.Add(unit);
                remainingFunds -= unit.unitPrice;
            }
        }

        return purchasedUnits;
    }

    private List<UnitDataBase> PlaceUnits(List<UnitDataBase> purchasedUnits)
    {
        List<UnitDataBase> finalLineup = new List<UnitDataBase>();

        // 방어형 유닛 (전열)
        List<UnitDataBase> defensiveUnits = purchasedUnits
            .Where(u => u.unitBranch == "Heavy_Infantry" || u.unitBranch == "Heavy_Cavalry")
            .ToList();
        finalLineup.AddRange(defensiveUnits.Take(5)); // 최대 5명까지 전열에 배치

        // 근접형 유닛 (전열/중간열)
        List<UnitDataBase> meleeUnits = purchasedUnits
            .Where(u => u.unitBranch == "Spearman" || u.unitBranch == "Warrior")
            .ToList();
        finalLineup.AddRange(meleeUnits.Take(5)); // 추가 5명까지 배치


        // 후열 유닛 (사거리 2 이상)
        List<UnitDataBase> rangedUnits = purchasedUnits
            .Where(u => u.range >= 2)
            .ToList();
        finalLineup.AddRange(rangedUnits.Take(3)); // 최대 3명까지 후열에 배치

        // 나머지 유닛 무작위 배치
        foreach (var unit in purchasedUnits)
        {
            if (!finalLineup.Contains(unit))
            {
                finalLineup.Add(unit);
            }

            if (finalLineup.Count >= maxUnits) break;
        }

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
        foreach (UnitDataBase unit in enemyLineup)
        {
            GameObject unitUI = Instantiate(enemyUnitPrefab, enemyListParent);
            Image unitImage = unitUI.transform.Find("UnitImage").GetComponent<Image>();
            TextMeshProUGUI unitName = unitUI.transform.Find("UnitName").GetComponent<TextMeshProUGUI>();

            unitImage.sprite = Resources.Load<Sprite>($"UnitImages/{unit.unitImg}");
            unitName.text = unit.unitName;
        }
    }
}