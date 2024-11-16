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
    public PlayerData playerData;           // PlayerData를 통해 자금 및 구매한 유닛 확인
    public UnitDataManager unitDataManager; // UnitDataManager를 통해 유닛데이터 로드하기 위함
    public Button placeButton; // 배치버튼 = 배치BTN
    public GameObject FundsWarning; // 자금 부족 경고

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
        placeButton.onClick.AddListener(TogglePlacingUnits);

        // 배치 UI에 점칸을 최초 1번만 생성
        lineObject = Instantiate(linePrefab, unitPlacementArea);
        Debug.Log("lineObject parent: " + lineObject.transform.parent.name);


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
                myUnitUIList.Add(myUnitUI);
            }
            
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
        else
        {
            Debug.LogWarning($"[UpdateUnitCountForUnit] {unit.unitName}에 해당하는 UI를 찾을 수 없습니다.");
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
        PlaceUnit(unit);  // 유닛 배치
    }
    // 선택된 유닛을 배치하는 메서드
    private void PlaceUnit(UnitDataBase unit)
    {
        // 유닛의 이미지와 이름을 가지고 새로운 UI 프리팹을 생성
        GameObject placeunitObject = Instantiate(placeunitPrefab, unitPlacementArea);
        
        // PlacedUnit 스크립트 컴포넌트를 가져옴
        PlacedUnit placedUnit = placeunitObject.GetComponent<PlacedUnit>();

        
        // 유닛 데이터 설정
        placedUnit.SetUnitData(unit);  // PlacedUnit의 SetUnitData 메서드에서 유닛 데이터와 UI 업데이트

        // 배치 후 해당 유닛을 PlayerData의 배치된 유닛 리스트에 추가
        PlayerData.Instance.AddPlacedUnit(unit);  // 유닛을 배치된 유닛 목록에 추가

        // 배치 후 점칸을 마지막으로 이동
        MoveLineToLast();
        Debug.Log(lineObject.transform.GetSiblingIndex());

        Debug.Log($"[PlaceUnit] 유닛 {unit.unitName} 배치 완료. 점칸 위치 업데이트.");
        Debug.Log($"[PlaceUnit] 배치할 유닛: {unit.unitName}");
        Debug.Log($"[PlaceUnit] 배치 전 유닛 개수: {PlayerData.Instance.GetUnitCount(unit)}");
        // 배치 후 해당 유닛의 개수를 차감
        PlayerData.Instance.SellUnit(unit);
        Debug.Log($"[PlaceUnit] 배치 후 유닛 개수: {PlayerData.Instance.GetUnitCount(unit)}");
        

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
        placeButton.interactable = isPlacingUnits;  // 배치 버튼 활성화 / 비활성화
    }
    public void ReturnUnit(UnitDataBase unit)
    {
        PlayerData.Instance.AddPurchasedUnit(unit);
        AddOrUpdateUnitInMyUnitUI(unit);
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
            else
            {
                Debug.LogWarning("유효하지 않은 헥스코드입니다: " + hexColor);
            }
        }
        else
        {
            Debug.LogWarning("backgroundImage가 설정되지 않았습니다.");
        }
    }
    
}