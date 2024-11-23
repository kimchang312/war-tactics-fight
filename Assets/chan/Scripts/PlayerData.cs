using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; } // 싱글톤 인스턴스

    private Dictionary<UnitDataBase, int> purchasedUnits = new Dictionary<UnitDataBase, int>();
    private List<UnitDataBase> placedUnits = new List<UnitDataBase>(); // 배치된 유닛 목록. 이후 전투 씬에 필요한 형태로 전달해야함.
    private List<UnitDataBase> enemyUnits;

    public string faction;                // 플레이어가 선택한 진영
    public string difficulty;             // 플레이어가 선택한 난이도
    public int enemyFunds;                // 난이도에 따른 적의 자금
    
    public static int currency = 3000;    // 플레이어 자금 (static으로 관리)

    

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 유지
            
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 초기화 시 필요한 값을 설정
        faction = "기본 진영";  // 기본 진영 또는 선택된 진영
        difficulty = "기본 난이도"; // 기본 난이도 또는 선택된 난이도
        enemyFunds = CalculateEnemyFunds(difficulty); // 난이도에 따른 자금 설정
    }
    
    // 난이도에 따른 자금 계산
    private int CalculateEnemyFunds(string difficulty)
    {
        
        return difficulty switch
        {
            "쉬움" => 2500,
            "보통" => 3500,
            "어려움" => 5000,
            "도전" => 6000,
            _ => 2500 // 나머지 경우에 대한 기본값  쉬움과 같음
        };
    }
    // 유닛을 구매하고 리스트에 추가
    public void AddPurchasedUnit(UnitDataBase unit)
    {

        // 총 유닛 수가 20을 초과하는지 확인
        int totalUnitCount = GetTotalUnitCount();

        if (totalUnitCount >= 20)
        {
            Debug.LogWarning("유닛 수가 20명을 초과할 수 없습니다.");
            return; // 유닛 추가를 막음
        }

        if (purchasedUnits.ContainsKey(unit))
        {
            purchasedUnits[unit]++;
        }
        else
        {
            purchasedUnits[unit] = 1;
        }
        // 디버그 로그 추가
        Debug.Log($"{unit.unitName}을(를) 구매했습니다. 현재 총 유닛 수: {totalUnitCount + 1}");
    }

    // 모든 유닛의 총 수를 계산하는 메서드
    public int GetTotalUnitCount()
    {
        int totalCount = 0;
        foreach (var unit in purchasedUnits)
        {
            totalCount += unit.Value; // 유닛의 수량을 더함
        }
        return totalCount;
    }
    // 특정 유닛을 판매하여 자금 환불 및 수량 감소
    public void SellUnit(UnitDataBase unit)
    {       
        
        if (purchasedUnits.ContainsKey(unit) && purchasedUnits[unit] > 0)
        {

            currency += unit.unitPrice;
            purchasedUnits[unit]--;
            Debug.Log($"[SellUnit] {unit.unitName} 유닛 개수 감소: {purchasedUnits[unit]}");

            // UI 업데이트
            ShopManager.Instance.UpdateUnitCountForUnit(unit);

            // 유닛 개수가 0이면 UI 삭제
            if (purchasedUnits[unit] == 0)
            {
                purchasedUnits.Remove(unit);
                ShopManager.Instance.RemoveMyUnitUI(unit);
                Debug.Log($"[SellUnit] {unit.unitName} 유닛 제거됨");
            }

            // 자금 상태가 변경되면 ShopManager에서 UI 업데이트 호출
            ShopManager.Instance.UpdateUIState();

            
        }
    }

    // 특정 유닛의 수량을 가져옴
    public int GetUnitCount(UnitDataBase unit)
    {
        return purchasedUnits.ContainsKey(unit) ? purchasedUnits[unit] : 0;
    }

    // 플레이어의 모든 유닛 목록을 반환
    public Dictionary<UnitDataBase, int> GetAllPurchasedUnits()
    {
        return new Dictionary<UnitDataBase, int>(purchasedUnits);
    }
    // 플레이어 데이터 초기화
    public void ResetPlayerData()
    {
        // 유닛 목록 초기화
        purchasedUnits.Clear();

        // 자금 초기화
        currency = 3000;

        // 기타 초기화할 데이터가 있다면 여기에 추가
        faction = "기본 진영";
        difficulty = "기본 난이도";
        enemyFunds = CalculateEnemyFunds(difficulty); // 난이도에 따른 자금 초기화
    }

    
    
    // 배치된 유닛 목록에 유닛을 추가하는 메서드
    public void AddPlacedUnit(UnitDataBase unit)
    {
        placedUnits.Add(unit);
        Debug.Log($"배치된 유닛 추가: {unit.unitName}");
    }
    // 배치된 유닛 목록을 확인
    public List<int> ShowPlacedUnitList()
    {
        List<int> result = new List<int>();
        Debug.Log("배치된 유닛 목록:");
        foreach (var unit in placedUnits)
        {
            result.Add(unit.idx);
            Debug.Log($"유닛 이름: {unit.unitName}");
        }

        return result;
    }
    public void RemovePlacedUnit(UnitDataBase unit)
    {
        if (placedUnits.Contains(unit))
        {
            placedUnits.Remove(unit);
            Debug.Log($"{unit.unitName} 유닛이 배치된 리스트에서 제거되었습니다.");
        }
        else
        {
            Debug.LogWarning($"{unit.unitName} 유닛이 배치된 리스트에 존재하지 않습니다.");
        }
    }
    public void SetEnemyUnits(List<UnitDataBase> enemyUnits)
    {
        this.enemyUnits = enemyUnits;
    }

    public List<UnitDataBase> GetEnemyUnits()
    {
        return enemyUnits;
    }
}