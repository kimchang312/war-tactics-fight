using Google.GData.Extensions;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UnitUpgradeUI : MonoBehaviour
{


    [SerializeField] private Button toggleUpgradeBtn;
    [SerializeField] private Button closeAllUpgradeBtn;
    [SerializeField] private Button closeOneUpgradeBtn;

    [SerializeField] private Button goMainBtn;

    [SerializeField] private GameObject allUpgradeWindow;
    [SerializeField] private GameObject oneUpgradeWindow;

    [SerializeField] private GameObject unitPrefab;                 // 유닛 프리팹
    [SerializeField] private Transform unitParent;                  // 생성된 유닛의 부모 오브젝트

    [SerializeField] private Image oneUpgradeImg;           //강화 창 유닛 이미지

    [SerializeField] private GameObject currentState;      //강화 수치 모음집

    /*
    [SerializeField] private GameObject healthBoost;       //강화 체력 수치
    [SerializeField] private GameObject armorBoosst;       //강화 장갑 수치
    [SerializeField] private GameObject attackBoost;       //강화 공격력 수치
    [SerializeField] private GameObject mobilityBoost;     //강화 기동력 수치
    [SerializeField] private GameObject rangeBoost;        //강화 사거리 수치
    [SerializeField] private GameObject antiCavalryBoost;  //강화 대기병 수치
    */

    [SerializeField] private TextMeshProUGUI beforeStateText;      //현재 강화 수치
    [SerializeField] private TextMeshProUGUI afterStateText;      //강화 시 변화 수치
    [SerializeField] private TextMeshProUGUI selectUpgradeText;     //선택한 강화

    [SerializeField] private Button upgradeBtn;                     //강화 버튼


    private enum UpgradeState { 
        Health,
        Armor,
        AttackDamage,
        Mobility,
        Range,
        AntiCavalry
    }

    //강화 할 능력치
    private UpgradeState selectState=UpgradeState.Health;

    //강화 할 유닛
    private int selectUnitIndex = 0;

    private bool isAllUpgradeWindowActive = false;

    private void Start()
    {
        //시작 시 UI창 끄기
        //CloseAllUpgradeWindows();

        //UpgradeManager.Instance.Upgrade(0, 0, 0);
        //var unit= UpgradeManager.Instance.GetUpgradeValues(0);
        //Debug.Log(unit.healthBoost);

        CreateUnit();
    }

    private void Awake()
    {
        // 버튼 클릭 이벤트 연결
        toggleUpgradeBtn.onClick.AddListener(ToggleAllUpgradeWindow);
        closeAllUpgradeBtn.onClick.AddListener(CloseAllUpgradeWindows);
        closeOneUpgradeBtn.onClick.AddListener(CloseOneUpgradeWindow);
        upgradeBtn.onClick.AddListener(ClickUpgradeBtn);

        goMainBtn.onClick.AddListener(GoMain);
    }

    // Toggle 버튼 클릭 시 호출 (allUpgradeWindow만 제어)
    private void ToggleAllUpgradeWindow()
    {
        isAllUpgradeWindowActive = !isAllUpgradeWindowActive;
        allUpgradeWindow.SetActive(isAllUpgradeWindowActive);

    }

    // Close All 버튼 클릭 시 호출
    private void CloseAllUpgradeWindows()
    {
        // 모든 창 비활성화
        allUpgradeWindow.SetActive(false);
        oneUpgradeWindow.SetActive(false);

        // 상태 초기화
        isAllUpgradeWindowActive = false;
    }

    // Close One 버튼 클릭 시 호출
    private void CloseOneUpgradeWindow()
    {
        // oneUpgradeWindow만 비활성화
        oneUpgradeWindow.SetActive(false);
    }

    // OneUpgradeWindow 활성화
    private void OpenOneUpgradeWindow()
    {
        oneUpgradeWindow.SetActive(true);
    }

    //유닛 생성
    public void CreateUnit()
    {
        if (unitPrefab == null || unitParent == null)
        {
            Debug.LogWarning("UnitPrefab or UnitParent is not assigned!");
            return;
        }

        int branchCount = 7; // 생성할 유닛 수
        float xDistance = 375; // X축 간격
        float yDistance = 300; // Y축 간격
        Vector3 originPos = new (-640, 110, 0); // 시작 위치

        for (int i = 0; i < branchCount; i++)
        {
            // 유닛 생성
            GameObject unit = Instantiate(unitPrefab, unitParent);

            // RectTransform 위치 설정
            RectTransform rectTransform = unit.GetComponent<RectTransform>();

            // X, Y 좌표 계산
            float xPos = originPos.x + (i < 4 ? i : i - 4) * xDistance;
            float yPos = originPos.y + (i > 3 ? -yDistance : 0);

            // 위치 설정
            rectTransform.anchoredPosition = new Vector2(xPos, yPos);

            //유닛 이름 변경
            unit.name = $"Unit{i}";

            // 유닛 이미지 변경
            ChangeUnitImage(unit, i);

            // 강화 수치를 바탕으로 자식 UI 크기 조정
            AdjustUnitChildSize(unit, i);

            // 유닛 클릭 이벤트 추가
            AddClickEventToUnit(unit);
        }
    }

    // 유닛 이미지 변경 메서드
    private void ChangeUnitImage(GameObject unit, int index)
    {
        //유닛 이미지 자식 위치
        int childNum = 6;

        // i 값에 따른 이미지 이름
        string imageName = index switch
        {
            0 => "Unit_Img_0",
            1 => "Unit_Img_2",
            2 => "Unit_Img_3",
            3 => "Unit_Img_5",
            4 => "Unit_Img_7",
            5 => "Unit_Img_8",
            6 => "Unit_Img_10",
            _ => null
        };

        // Resources에서 이미지 로드
        Sprite newSprite = Resources.Load<Sprite>($"UnitImages/{imageName}");

        // 유닛의 첫 번째 자식 Image 컴포넌트 찾기
        Image unitImage = unit.transform.GetChild(childNum).GetComponent<Image>();
        unitImage.sprite = newSprite;

    }

    //강화 수치를 보여주기
    private void AdjustUnitChildSize(GameObject unit, int branchIdx)
    {
        // 강화 수치 가져오기
        var unitUpgradeValue = UpgradeManager.Instance.GetUpgradeValues(branchIdx);

        //강화 기본 길이
        float originWidth=30;

        // 1번부터 6번 자식의 크기 조정
        for (int i = 0; i <= 5; i++) 
        {
            Transform child = unit.transform.GetChild(i);
            RectTransform childRect = child.GetComponent<RectTransform>();

            // 강화 수치를 바탕으로 Width 설정
            float newWidth = GetBoostValueByIndex(unitUpgradeValue, i) + originWidth;
            childRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);

        }
    }

    // 강화 수치를 i 값에 따라 반환하는 함수
    private float GetBoostValueByIndex(UpgradeManager.UpgradeValues unitUpgradeValue, int index)
    {
        return index switch
        {
            0 => unitUpgradeValue.healthBoost,      // i == 0일 때 healthBoost
            1 => unitUpgradeValue.armorBoost,       // i == 1일 때 armorBoost
            2 => unitUpgradeValue.attackDamageBoost, // i == 2일 때 attackDamageBoost
            3 => unitUpgradeValue.mobilityBoost,    // i == 3일 때 mobilityBoost
            4 => unitUpgradeValue.rangeBoost,       // i == 4일 때 rangeBoost
            5 => unitUpgradeValue.antiCavalryBoost, // i == 5일 때 antiCavalryBoost
            _ => 0 // 잘못된 index 값일 경우 기본값 0 반환
        };
    }

    //유닛 클릭 시 이벤트 추가
    private void AddClickEventToUnit(GameObject unit)
    {
        // EventTrigger 컴포넌트 추가
        EventTrigger eventTrigger = unit.AddComponent<EventTrigger>();

        // 클릭 이벤트 설정
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };

        // 클릭 시 호출할 메서드 추가
        entry.callback.AddListener((eventData) => OnUnitClicked(unit.name));
        eventTrigger.triggers.Add(entry);
    }

    //클릭 시 업글창 띄우기
    private void OnUnitClicked(string name)
    {
        OpenOneUpgradeWindow();

        if (name.StartsWith("Unit"))
        {
            // Unit 뒤의 숫자 부분 파싱
            if (int.TryParse(name.Substring(4), out int unitIndex))
            {
                selectUnitIndex = unitIndex;

                //selectState = (UpgradeState)unitIndex;
            }
        }

        ViewUpgradeState();
    }

    //강화 수치 보여주기
    private void ViewUpgradeState()
    {
        // 강화 수치 가져오기
        var unitUpgradeValue = UpgradeManager.Instance.GetUpgradeValues(selectUnitIndex);

        // currentState의 자식 객체들을 순회
        for (int i = 0; i < currentState.transform.childCount; i++)
        {
            Transform child = currentState.transform.GetChild(i);

            // 강화 수치를 i에 따라 가져오기
            float value = GetBoostValueByIndex(unitUpgradeValue, i);

            //자식 객체에 클릭 이벤트 추가
            AddClickEventToState(child.gameObject);

            // 자식 객체의 상태 업데이트
            ChangeStateValue(child.gameObject, value);

            //
            ChangeCurrentState($"{(int)selectState}");
        }
    }

    //현재 강화시 변화 수치 보여주기, 클릭 시 나오는 메서드
    private void ChangeCurrentState(string name)
    {
        selectState = (UpgradeState)int.Parse(name);

        // 강화 수치 가져오기
        var unitUpgradeValue = UpgradeManager.Instance.GetUpgradeValues(selectUnitIndex);

        float value = GetBoostValueByIndex(unitUpgradeValue, int.Parse(name));

        switch (int.Parse(name))
        {
            case 0:
                name = "체력";
                break;
            case 1:
                name = "장갑";
                break;
            case 2:
                name = "공격력";
                break;
            case 3:
                name = "기동력";
                break;
            case 4:
                name = "사거리";
                break;
            case 5:
                name = "대기병";
                break;

        }

        selectUpgradeText.text = name;

        beforeStateText.text = $"{value}";
        afterStateText.text = $"{value+1}";
    }

    //gameObject의 자식의 textmeshprougui를 접근해 값을 바꾸는 함수
    private void ChangeStateValue(GameObject parent, float value)
    {
        Transform firstChild = parent.transform.GetChild(0);

        // TextMeshProUGUI 컴포넌트 가져오기
        TextMeshProUGUI textMeshPro = firstChild.GetComponent<TextMeshProUGUI>();

        string name = "";

        switch (int.Parse(parent.name)) 
        { 
            case 0:
                name = "체력";
                break;
            case 1:
                name = "장갑";
                break;
            case 2:
                name = "공격력";
                break;
            case 3:
                name = "기동력";
                break;
            case 4:
                name = "사거리";
                break;
            case 5:
                name = "대기병";
                break;

        }

        // 텍스트 값 변경
        textMeshPro.text = $"{name}: {value}";
    }

    //유닛 클릭 시 이벤트 추가
    private void AddClickEventToState(GameObject state)
    {
        // EventTrigger 컴포넌트 추가
        EventTrigger eventTrigger = state.AddComponent<EventTrigger>();

        // 클릭 이벤트 설정
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };

        // 클릭 시 호출할 메서드 추가
        entry.callback.AddListener((eventData) => ChangeCurrentState(state.name));
        eventTrigger.triggers.Add(entry);
    }

    //강화 버튼 클릭 시 이벤트
    private void ClickUpgradeBtn()
    {
        UpgradeManager.Instance.Upgrade(selectUnitIndex, (int)selectState);

        ViewUpgradeState();
    }

    //메인화면으로
    private void GoMain()
    {
        SceneManager.LoadScene("Main");
    }

    
}
