using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; } // 싱글톤 인스턴스

    private Dictionary<UnitDataBase, int> purchasedUnits = new Dictionary<UnitDataBase, int>();

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

    private int CalculateEnemyFunds(string difficulty)
    {
        // 난이도에 따른 자금 계산
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
        if (purchasedUnits.ContainsKey(unit))
        {
            purchasedUnits[unit]++;
        }
        else
        {
            purchasedUnits[unit] = 1;
        }
    }
    // 특정 유닛을 판매하여 자금 환불 및 수량 감소
    public void SellUnit(UnitDataBase unit)
    {
        if (purchasedUnits.ContainsKey(unit) && purchasedUnits[unit] > 0)
        {
            currency += unit.unitPrice;
            purchasedUnits[unit]--;

            if (purchasedUnits[unit] == 0)
            {
                purchasedUnits.Remove(unit);
            }
        }
        else
        {
            Debug.LogWarning("판매할 유닛이 없습니다.");
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
}