using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    public GameObject emptyUnitPrefab;      // 레이아웃 자리차지를 위한 빈 프리팹
    public GameObject unitPrefab;           // 유닛 Prefab
    public Transform content;               // 유닛이 표시될 위치 (ScrollView의 Content)
    public TextMeshProUGUI currencyText;    // 현재 자금을 표시할 Text
    public TextMeshProUGUI factionText;     // 플레이어의 진영을 표시할 Text
    public PlayerData playerData;           // PlayerData를 통해 자금 및 구매한 유닛 확인
    public UnitDataManager unitDataManager; // UnitDataManager를 통해 유닛데이터 로드하기 위함



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

    // 유닛 데이터를 로드하는 시점에 맞춰 DisplayUnits 호출
    // 게임 시작 시 데이터를 미리 로드하고 상점 화면을 준비
    private async void Start()
    {

        // PlayerData 싱글톤 인스턴스를 연결
        if (PlayerData.Instance != null)
        {
            playerData = PlayerData.Instance;
            Debug.Log("PlayerData 연결 완료: " + playerData.faction);
        }
        else
        {
            Debug.LogError("PlayerData 싱글톤 인스턴스가 존재하지 않습니다.");
        }
        Debug.Log("ShopManager Start()");

        // 유닛 데이터를 미리 로드하여 텀을 줄임
        /*if (unitDataManager != null/* && unitDataManager.unitDataList.Count == 0)
        {
            unitDataManager = UnitDataManager.Instance;
            await unitDataManager.LoadUnitDataAsync(); // 비동기적으로 데이터를 미리 로드
            if (unitDataManager.unitDataList.Count > 0)
            {
                DisplayUnits(); // 유닛 데이터가 로드된 후 유닛을 표시
            }*/
        // UnitDataManager 인스턴스 연결
        while (UnitDataManager.Instance == null)
        {
            await Task.Yield();  // 비동기 대기
        }
        unitDataManager = UnitDataManager.Instance;
        Debug.Log("UnitDataManager 연결 완료");

        await unitDataManager.LoadUnitDataAsync();

        if (unitDataManager.unitDataList.Count > 0)
        {
            DisplayUnits();
        }
        else
        {
            Debug.LogWarning("유닛 데이터가 비어 있습니다.");
        }
// 로딩이 완료된 후 바로 상점 화면을 표시

    UpdateCurrencyDisplay(); // 자금 UI 업데이트
    FactionDisplay();// 진영 UI 업데이트
    }

    

    /* 데이터가 로드되었는지 확인하고, 로드된 후 DisplayUnits 호출
    if (UnitDataManager.Instance != null && UnitDataManager.Instance.unitDataList.Count == 0)
    {
        UnitDataManager = UnitDataManager.Instance;

        await UnitDataManager.Instance.LoadUnitDataAsync(); // 비동기적으로 데이터를 로드

        DisplayUnits(); // 데이터를 로드한 후 유닛을 표시
    }

    else
    {
        Debug.LogWarning("UnitDataManager가 존재하지 않습니다.");
    }
    //자금 및 진영 UI 업데이트
    FactionDisplay();
    UpdateCurrencyDisplay();*/
    
    // 유닛 데이터를 UI에 표시
    public void DisplayUnits()
    {
        var units = UnitDataManager.Instance.GetAllUnits(); // UnitDataManager에서 유닛 리스트 가져오기
        if (units != null && units.Count > 0)
        {
            foreach (var unit in units)
            {
                GameObject unitObject = Instantiate(unitPrefab, content);
                UnitUI unitUI = unitObject.GetComponent<UnitUI>(); // 유닛 정보를 표시할 UI 컴포넌트

                if (unitUI != null)
                {
                    unitUI.SetUnitData(unit); // 유닛 정보를 UI에 세팅
                    
                }
                else
                {
                    Debug.LogWarning("UnitUI 컴포넌트를 찾을 수 없습니다.");
                }
            }
            // 유닛 추가 후 빈 유닛 5개 추가
            AddEmptyUnits();
        }
        else
        {
            Debug.LogWarning("유닛 데이터가 비어 있습니다.");
        }
    }

    // 빈 유닛 5개 추가 (레이아웃 자리용)
    private void AddEmptyUnits()
    {
        for (int i = 0; i < 5; i++)
        {
            // 빈 유닛 프리팹을 Content에 추가
            Instantiate(emptyUnitPrefab, content);
        }
    }
    // 유닛 구매 버튼 클릭
    public void BuyUnit(UnitDataBase unit)
        {
        // 자금이 충분한지 확인
        if (PlayerData.currency >= unit.unitPrice) // PlayerData.Instance로 자금 확인
        {
            // 자금 차감
            PlayerData.currency -= unit.unitPrice;

            // 유닛 구매 목록에 추가
            PlayerData.Instance.AddPurchasedUnit(unit); // PlayerData.Instance로 구매 내역 추가

            // UI 업데이트
            UpdateCurrencyDisplay();
            
        }
        else
        {
            Debug.Log("자금이 부족합니다.");
        }
        }

        // 자금 업데이트 UI 표시
        private void UpdateCurrencyDisplay()
        {
        currencyText.text =  PlayerData.currency.ToString()+"G";
        }
        private void FactionDisplay()
        { 
           factionText.text = "진영 : "+ playerData.faction.ToString();
        }
    
}