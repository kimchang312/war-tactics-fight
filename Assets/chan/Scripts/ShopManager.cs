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
    public GameObject MyUnitPrefab;         // MyUnit 프리팹
    public Transform myUnitUIcontent;       // MyUnit UI 위치
    public TextMeshProUGUI currencyText;    // 현재 자금을 표시할 Text
    public TextMeshProUGUI factionText;     // 플레이어의 진영을 표시할 Text
    public PlayerData playerData;           // PlayerData를 통해 자금 및 구매한 유닛 확인
    public UnitDataManager unitDataManager; // UnitDataManager를 통해 유닛데이터 로드하기 위함
    public Button placeButton; // 배치버튼
    public GameObject FundsWarning; // 자금 부족 경고

    [SerializeField] private Transform unitPlacementArea;  // 유닛 배치할 UI 영역
    [SerializeField] private GameObject placeunitPrefab;       // 배치할 유닛 프리팹
    private bool isPlacingUnits = false;
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
    public void BuyUnit(UnitDataBase unit)
    {
        // 자금이 부족해도 유닛 구매 가능
            PlayerData.currency -= unit.unitPrice; //자금 차감 (음수로 내려감)
            PlayerData.Instance.AddPurchasedUnit(unit);

            UpdateCurrencyDisplay();
            AddOrUpdateUnitInMyUnitUI(unit);

        // 자금이 양수로 돌아오면 경고 메시지 숨기고 배치 버튼 활성화
        UpdateUIState();
        // 자금 부족 시 경고 표시
        if (PlayerData.currency < 0)
        {
            ShowFundsWarning(true);    // 자금 부족 경고 표시
            DisablePlaceButton(true);  // 배치 버튼 비활성화
        }
       
    }

    // 자금 업데이트 UI 표시
    public void UpdateCurrencyDisplay()
        {
        currencyText.text =  PlayerData.currency.ToString()+"G";
        }
    private void FactionDisplay()
        { 
           factionText.text = "진영 : "+ playerData.faction.ToString();
        }

    // MyUnit UI에 유닛 추가 또는 소지 개수 증가
    private void AddOrUpdateUnitInMyUnitUI(UnitDataBase unit)
    {
        bool unitExists = false;

        // 이미 MyUnitUI에 해당 유닛이 있는지 확인
        foreach (Transform child in myUnitUIcontent)
        {
            MyUnitUI myUnitUI = child.GetComponent<MyUnitUI>();
            if (myUnitUI != null && myUnitUI.UnitData == unit)
            {
                // 소지 개수 업데이트
                myUnitUI.UpdateUnitCount();
                unitExists = true;
                break;
            }
        }

        // MyUnitUI에 해당 유닛이 없다면 새로 생성
        if (!unitExists)
        {
            GameObject unitObj = Instantiate(MyUnitPrefab, myUnitUIcontent);
            MyUnitUI myUnitUI = unitObj.GetComponent<MyUnitUI>();
            if (myUnitUI != null)
            {
                myUnitUI.Setup(unit);
            }
        }
    }
    // 자금 부족 경고 메시지 표시/숨기기
    public void ShowFundsWarning(bool show)
    {
        if (FundsWarning != null)
        {
            FundsWarning.SetActive(show);
        }
    }
    // 배치 버튼을 비활성화하는 메서드
    public void DisablePlaceButton(bool disable)
    {
        // 배치 버튼 비활성화
        if (placeButton != null)
        {
            placeButton.interactable = !disable;
        }
    }
    // 자금 상태에 따라 UI 업데이트 (경고 메시지와 배치 버튼 상태)
    public void UpdateUIState()
    {
        if (PlayerData.currency < 0)
        {
            ShowFundsWarning(true);
            DisablePlaceButton(true);
        }
        else
        {
            ShowFundsWarning(false);
            DisablePlaceButton(false);
        }
    }

    
    // 유닛 클릭시 호출되는 메서드
    public void OnUnitClicked(UnitDataBase unit)
    {
        
        if (isPlacingUnits)
        {
            PlaceUnit(unit);  // 배치 모드일 때 유닛 배치
        }
        else
        {
            PlayerData.Instance.SellUnit(unit); // 배치 모드가 아니면 다른 처리 (예: 판매 등)
        }
    }
    // 선택된 유닛을 배치하는 메서드
    private void PlaceUnit(UnitDataBase unit)
    {
        // 유닛의 이미지와 이름을 가지고 새로운 UI 프리팹을 생성
        GameObject unitObject = Instantiate(placeunitPrefab, unitPlacementArea);
        UnitUI unitUI = unitObject.GetComponent<UnitUI>();

        // 유닛 데이터 설정
        unitUI.Setup(unit);  // UnitUI의 Setup 메서드에서 유닛 데이터와 UI 업데이트

        // 배치 후 해당 유닛의 개수를 차감
        PlayerData.Instance.SellUnit(unit);
        Debug.Log($"배치된 유닛: {unit.unitName}");
    }
    // 배치 모드 활성화 / 비활성화 토글
    public void TogglePlacingUnits()
    {
        isPlacingUnits = !isPlacingUnits;
        placeButton.interactable = isPlacingUnits;  // 배치 버튼 활성화 / 비활성화
    }
}