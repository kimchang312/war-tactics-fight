using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class TestModeUI : MonoBehaviour
{
    [SerializeField] private ObjectPool objectPool;

    [SerializeField] private Button toggleTestUnitBtn;
    [SerializeField] private Button toggleTestRelicBtn;
    [SerializeField] private Button closeTestUnitBtn;
    [SerializeField] private Button closeTestRelicBtn;
    [SerializeField] private Button openMyUnitWindowBtn;
    [SerializeField] private Button openEnemyUnitWindowBtn;
    [SerializeField] private Button clearWarRelicBtn;

    [SerializeField] private TMP_InputField myUnitIdInput;
    [SerializeField] private TMP_InputField enemyUnitIdInput;
    [SerializeField] private TMP_InputField warRelicIdInput;

    [SerializeField] private GameObject testUnitWindow;
    [SerializeField] private GameObject testRelicWindow;
    [SerializeField] private GameObject myUnitWindow;
    [SerializeField] private GameObject enemyUnitWindow;

    [SerializeField] private FightStartBtn fightStartBtn;

    [SerializeField] private TextMeshProUGUI myUnitIdListText;
    [SerializeField] private TextMeshProUGUI enemyUnitIdListText;
    [SerializeField] private TextMeshProUGUI warRelicIdListText;

    [SerializeField] private TextMeshProUGUI myAllPriceText;
    [SerializeField] private TextMeshProUGUI enemyAllPriceText;

    [SerializeField] private GameObject myCorps;
    [SerializeField] private GameObject enemyCorps;

    UnitPriceDatabase priceDatabase = new UnitPriceDatabase();

    private void Start()
    {
        if (fightStartBtn == null)
        {
            fightStartBtn = GetComponent<FightStartBtn>();
        }

        // 윈도우 토글 버튼 이벤트 등록
        toggleTestUnitBtn.onClick.AddListener(ToggleTestUnitWindow);
        toggleTestRelicBtn.onClick.AddListener(ToggleTestRelicWindow);

        // 윈도우 닫기 버튼 이벤트 등록
        closeTestUnitBtn.onClick.AddListener(CloseTestUnitWindow);
        closeTestRelicBtn.onClick.AddListener(CloseTestRelicWindow);

        openMyUnitWindowBtn.onClick.AddListener(OpenMyUnitWindow);
        openEnemyUnitWindowBtn.onClick.AddListener (OpenEnemyUnitWindow);

        clearWarRelicBtn.onClick.AddListener(OnClickClearBtn);

        myUnitIdInput.onSubmit.AddListener(OnEndEditMyUnit);
        enemyUnitIdInput.onSubmit.AddListener (OnEndEditEnemyUnit);
        warRelicIdInput.onSubmit.AddListener(AddWarRelic);

        SetStringMyIds();
        SetStringEnemyIds();
    }

    // 테스트 유닛 윈도우 토글
    private void ToggleTestUnitWindow()
    {
        testUnitWindow.SetActive(!testUnitWindow.activeSelf);
    }

    // 테스트 렐릭 윈도우 토글
    private void ToggleTestRelicWindow()
    {
        testRelicWindow.SetActive(!testRelicWindow.activeSelf);
    }

    // 테스트 유닛 윈도우 닫기
    private void CloseTestUnitWindow()
    {
        testUnitWindow.SetActive(false);
    }

    // 테스트 렐릭 윈도우 닫기
    private void CloseTestRelicWindow()
    {
        testRelicWindow.SetActive(false);
    }

    //내 유닛 창 오픈 적 오프
    private void OpenMyUnitWindow()
    {
        myUnitWindow.SetActive(true);
        enemyUnitWindow.SetActive(false);
    }
    private void OpenEnemyUnitWindow()
    {
        enemyUnitWindow.SetActive (true);
        myUnitWindow.SetActive (false);
    }

    //내 유닛 입력
    private void OnEndEditMyUnit(string input)
    {
        // 결과를 저장할 리스트 생성
        List<int> myUnitIds = new List<int>();

        // 정규식으로 숫자(하나 이상의 연속된 숫자)를 추출합니다.
        MatchCollection matches = Regex.Matches(input, @"\d+");

        // 추출된 각 숫자 문자열을 int로 파싱 후 리스트에 추가합니다.
        foreach (Match match in matches)
        {
            if (int.TryParse(match.Value, out int number))
            {
                if(number >=0 && number <= 66)
                {
                    myUnitIds.Add(number);
                }
                else
                {
                    Debug.Log("입력 범위를 벗어났습니다. 0~25");
                }

            }
        }

        fightStartBtn.SetMyFightUnits(myUnitIds);
        SetStringMyIds();

        myUnitIdInput.text = "";

        ClearUnitCorps(true);
        StartCreatingUnits(myUnitIds,true);
    }

    //적 유닛 입력
    private void OnEndEditEnemyUnit(string input)
    {
        // 결과를 저장할 리스트 생성
        List<int> enemyUnitIds = new List<int>();

        // 정규식으로 숫자(하나 이상의 연속된 숫자)를 추출합니다.
        MatchCollection matches = Regex.Matches(input, @"\d+");

        // 추출된 각 숫자 문자열을 int로 파싱 후 리스트에 추가합니다.
        foreach (Match match in matches)
        {
            if (int.TryParse(match.Value, out int number))
            {
                if (number >= 0 && number <= 66)
                {
                    enemyUnitIds.Add(number);
                }
                else
                {
                    Debug.Log("입력 범위를 벗어났습니다. 0~25");
                }
            }
        }

        fightStartBtn.SetEnemyFightUnits(enemyUnitIds);
        SetStringEnemyIds();

        enemyUnitIdInput.text = "";

        ClearUnitCorps(false);
        StartCreatingUnits(enemyUnitIds,false);
    }

    //
    private void SetStringMyIds()
    {
         List<int> ids = fightStartBtn.GetMyFightUnits();
         myUnitIdListText.text = $"MyUnitList: {string.Join(",", ids)}";
         myAllPriceText.text= $"가격: {priceDatabase.GetTotalPrice(ids)}";
    }
    private void SetStringEnemyIds()
    {
        List<int> ids = fightStartBtn.GetEnemyFightUnits();
        enemyUnitIdListText.text = $"EnemyUnitIdList: {string.Join(",", fightStartBtn.GetEnemyFightUnits())}";
        enemyAllPriceText.text = $"가격: {priceDatabase.GetTotalPrice(ids)}";
    }

    //유산 추가
    private void AddWarRelic(string input)
    {
        // 결과를 저장할 리스트 생성
        List<int> warRelicIds = new List<int>();

        // 정규식으로 숫자(하나 이상의 연속된 숫자)를 추출합니다.
        MatchCollection matches = Regex.Matches(input, @"\d+");

        // 추출된 각 숫자 문자열을 int로 파싱 후 리스트에 추가합니다.
        foreach (Match match in matches)
        {
            if (int.TryParse(match.Value, out int number))
            {
                if (number >= 1 && number <= 61)
                {
                    warRelicIds.Add(number);
                    RogueLikeData.Instance.AcquireRelic(number);
                }
                else
                {
                    Debug.Log("입력 범위를 벗어났습니다. 0~25");
                }
            }
        }

        warRelicIdInput.text = "";
        SetStringWarRelic();
    }

    //유산 Text 수정
    private void SetStringWarRelic()
    {
        warRelicIdListText.text = $"WarRelicIdList: {string.Join(",", RogueLikeData.Instance.GetAllOwnedRelicIds())}";
    }

    //유산 clear버튼
    private void OnClickClearBtn()
    {
        RogueLikeData.Instance.ResetOwnedRelics();
        SetStringWarRelic();
    }

    // 유닛 무리 생성 (Coroutine)
    private IEnumerator CreatUnitCorpsCoroutine(List<int> unitIds, bool isTeam)
    {
        Vector2 startPosition = isTeam ? new Vector2(-125, -100) : new Vector2(125, -100);
        int xOffset = isTeam ? -100 : 100;
        int yOffset = -80;
        int columns = 3;

        GameObject parent = isTeam ? myCorps : enemyCorps;
        int rows = Mathf.CeilToInt((float)unitIds.Count / columns);

        for (int i = 0; i < unitIds.Count; i++)
        {
            GameObject unit = objectPool.GetOnlyUnit();

            // Set parent first
            RectTransform rectTransform = unit.GetComponent<RectTransform>();
            rectTransform.SetParent(parent.transform, false); // Canvas의 자식으로 설정

            // unitId가 12보다 크면 -1로 설정
            int unitId = unitIds[i] > 12 ? -1 : unitIds[i];

            // 이미지 설정
            Sprite sprite = Resources.Load<Sprite>($"KIcon/OnlyUnitImages/{unitId}");
            Image imageComponent = unit.GetComponent<Image>();
            if (imageComponent != null)
            {
                imageComponent.sprite = sprite;
            }

            // 위치 설정 (초기 위치)
            int row = i / columns;
            int col = i % columns;

            float xPos = startPosition.x + (row * xOffset);
            float yPos = startPosition.y + (col * yOffset);

            rectTransform.anchoredPosition = new Vector2(xPos, yPos);

            // DoTween으로 y-100 이동
            rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y - 100, 0.5f)
                .SetEase(Ease.OutQuad);

            // false일 때 좌우 반전
            Vector3 scale = rectTransform.localScale;
            scale.x = isTeam ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            rectTransform.localScale = scale;

            // 0.2초 대기 후 다음 유닛 생성
            yield return new WaitForSeconds(0.1f);
        }
    }

    // 유닛 생성 함수 호출 (코루틴)
    public void StartCreatingUnits(List<int> unitIds, bool isTeam)
    {
        StartCoroutine(CreatUnitCorpsCoroutine(unitIds, isTeam));
    }

    // 유닛 무리 삭제
    private void ClearUnitCorps(bool isTeam)
    {
        GameObject parent = isTeam ? myCorps : enemyCorps;

        foreach (Transform child in parent.transform)
        {
            objectPool.ReturnOnlyUnit(child.gameObject);
        }
    }

}
