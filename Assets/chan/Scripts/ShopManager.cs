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
    public GameObject linePrefab;           // 점칸이미지 Prefab
    public Transform content;               // 유닛이 표시될 위치 (ScrollView의 Content)
    public GameObject MyUnitPrefab;         // MyUnit 프리팹
    public Transform myUnitUIcontent;       // MyUnit UI 위치
    public TextMeshProUGUI currencyText;    // 현재 자금을 표시할 Text
    public TextMeshProUGUI factionText;     // 플레이어의 진영을 표시할 Text
    public TextMeshProUGUI MustPlaceText;   // 배치 필요 텍스트
    public PlayerData playerData;           // PlayerData를 통해 자금 및 구매한 유닛 확인
    public UnitDataManager unitDataManager; // UnitDataManager를 통해 유닛데이터 로드하기 위함
    public Button placeButton; // 배치버튼 = 배치BTN
    public Button startButton; // 전투시작 버튼
    public GameObject FundsWarning; // 자금 부족 경고

    //유닛 상세에 필요한 연결
    public UnitDetailUI unitDetailUI; // Inspector에서 연결


    private List<MyUnitUI> myUnitUIList = new List<MyUnitUI>(); // MyUnitUI 리스트

    [SerializeField] private Transform unitPlacementArea;  // 유닛 배치할 UI 영역
    [SerializeField] private GameObject placeunitPrefab;   // 배치할 유닛 프리팹
    [SerializeField] private Image currencyTextImg;
    public bool isPlacingUnits = false;                   // 배치 모드 확인
    private GameObject lineObject; // 점칸을 한 번만 생성할 객체
    public bool IsPlacingUnits => isPlacingUnits;
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
        playerData = PlayerData.Instance;
        

        
        // UnitDataManager 인스턴스 연결
        while (UnitDataManager.Instance == null)
        {
            await Task.Yield();  // 비동기 대기
        }
        unitDataManager = UnitDataManager.Instance;
        

        await unitDataManager.LoadUnitDataAsync();

        if (unitDataManager.unitDataList.Count > 0)
        {
            DisplayUnits();
        }
        else
        {
            
        }
        // 로딩이 완료된 후 바로 상점 화면을 표시

        UpdateCurrencyDisplay(); // 자금 UI 업데이트
        FactionDisplay();// 진영 UI 업데이트
        placeButton.onClick.AddListener(TogglePlacingUnits);
        placeButton.onClick.AddListener(OnPlaceButtonClicked); // 배치 버튼 클릭 리스너 연결

        UpdatePlacementUIState(); // 초기 상태 업데이트

        // 배치 UI에 점칸을 최초 1번만 생성
        lineObject = Instantiate(linePrefab, unitPlacementArea);

        DisablePlaceButton(true);
        //디버그 체크
        placeButton.onClick.AddListener(() =>
        {
            Debug.Log("배치 버튼 클릭됨");
            OnPlaceButtonClicked();
        });
        DebugCheck();
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
                    // UnitDetailUI 전달 (URC와 연동)
                    URC urc = unitObject.GetComponent<URC>();
                    if (urc != null)
                    {
                        urc.SetUnitData(unit);               // 유닛 데이터 설정
                        urc.UnitDetailUI = unitDetailUI;    // UnitDetailUI 참조 전달
                    }
                }
                
            }
            // 유닛 추가 후 빈 유닛 5개 추가
            AddEmptyUnits();
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
    {// 총 유닛 수가 20을 초과하는지 확인
        int totalUnitCount = PlayerData.Instance.GetTotalUnitCount();

        if (totalUnitCount >= 20)
        {
            Debug.LogWarning("유닛 수가 20명을 초과할 수 없습니다.");
            return; // 유닛 추가를 막음
        }
        else {
        // 자금이 부족해도 유닛 구매 가능
        PlayerData.currency -= unit.unitPrice; //자금 차감 (음수로 내려감)
        PlayerData.Instance.AddPurchasedUnit(unit);

            UpdateCurrencyDisplay();
            AddOrUpdateUnitInMyUnitUI(unit);

        // 자금이 양수로 돌아오면 경고 메시지 숨기고 배치 버튼 활성화
        UpdateUIState();
        UpdatePlacementUIState(); // 구매 후 상태 업데이트
        }
        // 디버그 로그 추가
        Debug.Log($"{unit.unitName}을(를) 구매했습니다. 현재 총 유닛 수: {totalUnitCount + 1}");
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
                    // UnitDetailUI 전달 (URC와 연동)
                    URC urc = unitObj.GetComponent<URC>();
                    if (urc != null)
                    {
                        urc.SetUnitData(unit);               // 유닛 데이터 설정
                        urc.UnitDetailUI = unitDetailUI;    // UnitDetailUI 참조 전달
                    }
                
                
            }
                myUnitUI.Setup(unit);
                myUnitUIList.Add(myUnitUI);                    
        }
    }
    // 특정 유닛에 해당하는 MyUnitUI 찾기
    private MyUnitUI FindMyUnitUI(UnitDataBase unit)
    {
        return myUnitUIList.Find(ui => ui.UnitData == unit);
    }

    // 특정 유닛의 UI 업데이트
    public void UpdateUnitCountForUnit(UnitDataBase unit)
    {
        MyUnitUI myUnitUI = FindMyUnitUI(unit);
        if (myUnitUI != null)
        {
            myUnitUI.UpdateUnitCount();
        }
        
    }

    // 특정 유닛의 UI 삭제
    public void RemoveMyUnitUI(UnitDataBase unit)
    {
        MyUnitUI myUnitUI = FindMyUnitUI(unit);
        if (myUnitUI != null)
        {
            myUnitUIList.Remove(myUnitUI);
            Destroy(myUnitUI.gameObject); // UI 삭제
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
        // 구매한 유닛이 있는지 먼저 확인
        if (PlayerData.Instance.GetTotalUnitCount() == 0)
        {
            DisablePlaceButton(true); // 구매한 유닛이 없으면 배치 버튼 비활성화
            ShowFundsWarning(false); // 경고는 표시하지 않음
            ChangeBackgroundColor("#ffffff"); // 기본 배경색으로 설정
            return; // 더 이상 조건 확인하지 않음
        }

        if (PlayerData.currency < 0)
        {
            ShowFundsWarning(true);
            DisablePlaceButton(true);
            ChangeBackgroundColor("#f4cccc"); //헥스코드로 플레이어 골드 배경색 변경

        }
        else
        {
            ShowFundsWarning(false);
            DisablePlaceButton(false);
            ChangeBackgroundColor("#ffffFF"); //헥스코드로 플레이어 골드 배경색 변경

        }
    }


    // 유닛 클릭시 호출되는 메서드
    public void OnUnitClicked(UnitDataBase unit)
    {
        Debug.Log($"OnUnitClicked 호출됨: {unit.unitName}");
        PlaceUnit(unit);  // 유닛 배치
    }
    // 선택된 유닛을 배치하는 메서드
    private void PlaceUnit(UnitDataBase unit)
    {
        // 현재 배치된 유닛 리스트의 길이를 가져와 인덱스 설정
        int unitIndex = PlayerData.Instance.ShowPlacedUnitList().Count; // 현재 배치된 유닛의 개수로 인덱스를 추적

        // 유닛의 이미지와 이름을 가지고 새로운 UI 프리팹을 생성
        GameObject placeunitObject = Instantiate(placeunitPrefab, unitPlacementArea);
        

        // PlacedUnit 스크립트 컴포넌트를 가져옴
        PlacedUnit placedUnit = placeunitObject.GetComponent<PlacedUnit>();        
        // UnitDetailUI 전달 (URC와 연동)
        URC urc = placedUnit.GetComponent<URC>();
        if (urc != null)
        {
            urc.SetUnitData(unit);               // 유닛 데이터 설정
            urc.UnitDetailUI = unitDetailUI;    // UnitDetailUI 참조 전달
        }
        
        
    

        // 유닛 데이터 설정
        placedUnit.SetUnitData(unit,unitIndex);  // PlacedUnit의 SetUnitData 메서드에서 유닛 데이터와 UI 업데이트

        // 배치 후 해당 유닛을 PlayerData의 배치된 유닛 리스트에 추가
        PlayerData.Instance.AddPlacedUnit(unit);  // 유닛을 배치된 유닛 목록에 추가

        // 배치 후 점칸을 마지막으로 이동
        MoveLineToLast();
        

        
        // 배치 후 해당 유닛의 개수를 차감
        PlayerData.Instance.SellUnit(unit);

        // 배치 상태 업데이트
        UpdatePlacementUIState(); // 배치가 발생할 때마다 호출
    }

    
    // 점칸을 마지막으로 이동하는 메서드
    private void MoveLineToLast()
    {
        // lineObject가 마지막 자식이 되도록 강제로 설정
        int lastIndex = unitPlacementArea.childCount;  // 현재 자식의 개수를 가져옴
        lineObject.transform.SetSiblingIndex(lastIndex);  // 마지막 인덱스 위치로 설정

        // SetAsLastSibling 호출 후 상태 확인
        Debug.Log($"[After SetAsLastSibling] lineObject의 인덱스: {lineObject.transform.GetSiblingIndex()}, 부모 자식 수: {unitPlacementArea.childCount}");
        
    }
        // 배치 모드 활성화 / 비활성화 토글
        public void TogglePlacingUnits()
    {
        isPlacingUnits = !isPlacingUnits;
        Debug.Log($"배치 모드 활성화 상태: {isPlacingUnits}");
        placeButton.interactable = isPlacingUnits;  // 배치 버튼 활성화 / 비활성화
    }
    public void ReturnUnit(UnitDataBase unit)
    {
        PlayerData.Instance.AddPurchasedUnit(unit);
        AddOrUpdateUnitInMyUnitUI(unit);
        // 배치 상태 업데이트
        UpdatePlacementUIState(); // 반환 시에는 UI 상태 업데이트
    }

    // 헥스코드로 배경을 변경하는 메서드
    public void ChangeBackgroundColor(string hexColor)
    {
        if (currencyTextImg != null)
        {
            Color newColor;
            if (ColorUtility.TryParseHtmlString(hexColor, out newColor))
            {
                currencyTextImg.color = newColor;
            }
            
        }
        
    }
    // 배치 상태를 업데이트하는 메서드
    public void UpdatePlacementUIState()
    {
        // PlayerData에서 남아있는 구매한 유닛 개수를 확인
        int remainingUnits = PlayerData.Instance.GetTotalUnitCount();
        Debug.Log(remainingUnits);

        if (remainingUnits > 0)
        {
            // 배치해야 할 유닛이 남아있는 경우
            startButton.interactable = false; // Start 버튼 비활성화
            MustPlaceText.text = "배치 필요한 유닛 존재"; // 알림 메시지 설정
            MustPlaceText.color = new Color(0.596f, 0f, 0f); // #980000 색상 설정
        }
        else
        {
            // 모든 유닛이 배치된 경우
            startButton.interactable = true; // Start 버튼 활성화
            MustPlaceText.text = "배치 완료"; // 완료 메시지 설정
            MustPlaceText.color = new Color(0.290f, 0.525f, 0.910f); // #4a86e8 색상 설정
        }
    }
    // 배치 버튼 클릭 시 호출되는 메서드
    private void OnPlaceButtonClicked()
    {
        // 배치 모드를 토글
        TogglePlacingUnits();

        // 배치 모드가 활성화되었을 경우 상태 업데이트
        if (isPlacingUnits)
        {
            UpdatePlacementUIState();
        }
    }
    private void DebugCheck()
    {
        if (content == null) Debug.LogError("Content가 연결되지 않았습니다!");
        if (myUnitUIcontent == null) Debug.LogError("MyUnitUIContent가 연결되지 않았습니다!");
        if (currencyText == null) Debug.LogError("CurrencyText가 연결되지 않았습니다!");
    }
}