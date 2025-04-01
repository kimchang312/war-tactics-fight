using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.UI;
using static EventManager;

public class EventManager : MonoBehaviour
{
    [SerializeField] private Image eventImage;
    [SerializeField] private TextMeshProUGUI eventTitle;
    [SerializeField] private TextMeshProUGUI eventDescription;
    [SerializeField] private TextMeshProUGUI selectTitle;
    [SerializeField] private Button arrowLine;
    [SerializeField] private GameObject choiceBtns;
    [SerializeField] private GameObject selectUnitWindow;
    [SerializeField] private GameObject selectUnitParent;
    [SerializeField] private ObjectPool objectPool;

    private const int MaxEventCount = 50; // 이벤트의 최대 개수

    private Dictionary<int, IEventRewardHandler> rewardHandlers = new();

    public interface IEventRewardHandler
    {
        string GetReward(int choice, RogueUnitDataBase unit);
    }
    // 씬 로드 시 실행되는 함수
    void Start()
    {
        //임시 로드
        SaveData saveData = new SaveData();
        saveData.LoadData();

        ResetUI();
        InitRewardHandlers();

        Dictionary<int, int> eventList = RogueLikeData.Instance.GetEncounteredEvent();
        int randomEventId = GenerateUniqueEventId(eventList);

        ShowEvent(0);
        if (randomEventId != -1)
        {
            Debug.Log($"선택된 이벤트 ID: {randomEventId}");
        }
    }
    private void InitRewardHandlers()
    {
        rewardHandlers.Add(0, new TravelersShelterReward());
        // 추가 이벤트 보상 클래스 등록
    }
    //유닛 선택창 생성
    private void CreateSelectUnitsWindow(GameEvent gameEvent)
    {
        List<GameObject> list = new List<GameObject>();
        List<RogueUnitDataBase> myUnits= RogueLikeData.Instance.GetMyUnits();

        selectTitle.text = gameEvent.selectTitle;
        for (int i = 0; i < myUnits.Count; i++)
        {
            GameObject unitObj = objectPool.GetSelectUnit(); // 유닛 UI 오브젝트 가져오기
            if (unitObj == null) continue;
            list.Add(unitObj);
            // 부모 설정
            unitObj.transform.SetParent(selectUnitParent.transform, false);

            RogueUnitDataBase unitData = myUnits[i];

            // 텍스트 갱신
            TextMeshProUGUI energyText = unitObj.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
            energyText.text = $"{unitData.energy}/{unitData.maxEnergy}";
            // 에너지 게이지 fillAmount 조절
            Image energyBar = unitObj.transform.Find("Energy")?.GetComponent<Image>();
            energyBar.fillAmount = (float)unitData.energy / unitData.maxEnergy;

            // 버튼 클릭 시 유닛 기력 회복 + 창 닫기
            Button button = unitObj.GetComponent<Button>();
            RogueUnitDataBase capturedUnit = unitData; // 클로저 문제 방지용 캡처
            button.onClick.RemoveAllListeners(); // 기존 리스너 제거
            button.onClick.AddListener(() => OnSelectUnit(capturedUnit, gameEvent.id));
        }
        selectUnitWindow.SetActive(true);
    }
    // 유닛을 선택했을 때 실행되는 함수
    private void OnSelectUnit(RogueUnitDataBase unitData,int eventId)
    {
        string rewardText = GetReward(eventId, GetChoiceByEventId(eventId),unitData);
        objectPool.ReturnSelectUnit(selectUnitParent);
        eventDescription.text = rewardText;
        selectUnitWindow.SetActive(false);
    }
    //전체 초기화
    private void ResetUI()
    {
        ResetButtonUI();
        arrowLine.gameObject.SetActive(false);
        selectUnitWindow.SetActive(false);
    }

    //버튼UI초기화
    private void ResetButtonUI()
    {
        foreach (Transform child in choiceBtns.transform)
        {
            child.gameObject.SetActive(false);
        }
    }
    // EncounteredEvent에 없는 이벤트 ID를 랜덤으로 생성하는 함수
    private int GenerateUniqueEventId(Dictionary<int, int> encounteredEvents)
    {
        List<int> availableEvents = new List<int>();

        // EncounteredEvent에 없는 이벤트 ID를 모두 리스트에 추가
        for (int i = 0; i < MaxEventCount; i++)
        {
            if (!encounteredEvents.ContainsKey(i))
            {
                availableEvents.Add(i);
            }
        }

        if (availableEvents.Count == 0) return -1; // 모든 이벤트가 이미 EncounteredEvent에 있을 경우

        // 사용 가능한 이벤트 중에서 랜덤으로 하나 선택
        int randomIndex = UnityEngine.Random.Range(0, availableEvents.Count);
        return availableEvents[randomIndex];
    }
    // 선택지가 클릭되었을 때 호출되는 함수
    private void OnChoiceSelected(int eventId, int choiceIndex)
    {
        string text = "";
        switch (choiceIndex)
        {
            case 0:
                if (eventId == 0) 
                {
                    CreateSelectUnitsWindow(EventDataBase.GetEventById(eventId));
                }
                else
                {
                    text = GetReward(eventId, choiceIndex);
                }

                break;
            case 1:
                text = GetReward(eventId, choiceIndex);
                break;
            case 2:
                text = GetReward(eventId, choiceIndex);
                // 아무 효과 없이 지나가기
                break;
            case 3:
                text = GetReward(eventId, choiceIndex);
                break;
            default:
                break;
        }
        eventDescription.text = text;
        ResetButtonUI();
        arrowLine.gameObject.SetActive(true);
    }
    //이벤트 0 여행자의 쉼터
    private void ShowEvent(int eventId)
    {
        GameEvent gameEvent = EventDataBase.GetEventById(eventId);
        string[] choiceTexts = gameEvent.choices;
        int choices = choiceTexts.Length;
        eventTitle.text = gameEvent.title;
        eventDescription.text = gameEvent.description;
        for (int i = 0; i < choices; i++)
        {
            // 자식 오브젝트를 Button으로 가져오기
            Button button = choiceBtns.transform.GetChild(i).GetComponent<Button>();

            // 버튼 활성화
            button.gameObject.SetActive(true);

            // 버튼의 텍스트 설정
            TextMeshProUGUI buttonText = button.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            buttonText.text = choiceTexts[i];

            // 버튼 클릭 이벤트 등록
            button.onClick.RemoveAllListeners(); // 기존에 등록된 이벤트 제거
            int index = i; // 클로저 문제 방지를 위해 따로 변수 선언

            button.onClick.AddListener(() => OnChoiceSelected(eventId, index)); // 버튼 클릭 시 호출할 함수 등록
        }

        eventImage.sprite = Resources.Load<Sprite>($"KImage/Event{eventId}");
    }
    private string GetReward(int eventId, int choice, RogueUnitDataBase unit = null)
    {
        if (rewardHandlers.TryGetValue(eventId, out var handler))
        {
            return handler.GetReward(choice, unit);
        }
        return "잘못된 접근입니다.";
    }
    private int GetChoiceByEventId(int eventId)
    {
        int choice = 0;
        if (eventId == 0) choice= 0;
        return choice;
    }
}
