using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; } // 싱글톤 인스턴스

    public string faction;                // 플레이어가 선택한 진영
    public string difficulty;             // 플레이어가 선택한 난이도
    public int enemyFunds;                // 난이도에 따른 적의 자금
    public List<UnitDataBase> purchasedUnits; // 상점에서 구매한 유닛 리스트
    public static int currency = 3000;    // 플레이어 자금 (static으로 관리)

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 유지
            purchasedUnits = new List<UnitDataBase>(); // 구매한 유닛 리스트 초기화
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

    // 상점에서 유닛 구매 후 구매 내역을 리스트에 추가
    public void AddPurchasedUnit(UnitDataBase unit)
    {
        purchasedUnits.Add(unit);
    }

}