using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.UI;

public class EventManager : MonoBehaviour
{
    [SerializeField] private Image eventImage;
    [SerializeField] private TextMeshProUGUI eventTitle;
    [SerializeField] private TextMeshProUGUI eventDescription;
    [SerializeField] private GameObject choiceBtns;

    private const int MaxEventCount = 50; // 이벤트의 최대 개수
    private Dictionary<int, System.Action<int>> eventDictionary;
    // 씬 로드 시 실행되는 함수
    void Start()
    {
        UIReset();
        InitializeEvents();

        Dictionary<int, int> eventList = RogueLikeData.Instance.GetEncounteredEvent();
        int randomEventId = GenerateUniqueEventId(eventList);

        ExecuteEvent(0);
        if (randomEventId != -1)
        {
            Debug.Log($"선택된 이벤트 ID: {randomEventId}");
            // 여기에 선택된 이벤트 ID를 기반으로 이벤트를 실행하는 코드를 추가하세요.
        }
        else
        {
            Debug.Log("모든 이벤트가 이미 진행되었습니다.");
        }
    }
    //UI초기화
    private void UIReset()
    {
        foreach (Transform child in choiceBtns.transform)
        {
            child.gameObject.SetActive(false);
        }
    }
    // 이벤트 딕셔너리 초기화
    private void InitializeEvents()
    {
        eventDictionary = new Dictionary<int, System.Action<int>>()
    {
        { 0, TravelersShelter },
        { 12, SpringSpirit }
        // 여기서 다른 이벤트들을 추가
    };
    }
    // 선택된 이벤트 실행
    private void ExecuteEvent(int eventId)
    {
        if (eventDictionary.TryGetValue(eventId, out var eventAction))
        {
            eventAction(eventId);
        }
        else
        {
            Debug.Log($"이벤트 {eventId}는 아직 구현되지 않았습니다.");
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
        int randomIndex = Random.Range(0, availableEvents.Count);
        return availableEvents[randomIndex];
    }
    // 선택지가 클릭되었을 때 호출되는 함수
    private void OnChoiceSelected(int choiceIndex)
    {
        switch (choiceIndex)
        {
            case 0:
                Debug.Log("선택지 1: 한 병사를 침대에 눕혔습니다. 단일 기력 회복(대)");
                // 단일 기력 회복(대) 기능 구현 (예: 특정 유닛의 기력 회복)
                break;
            case 1:
                Debug.Log("선택지 2: 모두가 바닥에서 쉬었습니다. 사기 회복(소)");
                RogueLikeData.Instance.SetMorale(RogueLikeData.Instance.GetMorale()+10);
                // 사기 회복(소) 기능 구현 (예: RogueLikeData.Instance.SetMorale() 호출)
                break;
            case 2:
                Debug.Log("선택지 3: 길을 서둘렀습니다.");
                // 아무 효과 없이 지나가기
                break;
        }
    }
    //이벤트 0 여행자의 쉼터
    private void TravelersShelter(int eventId)
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

            button.onClick.AddListener(() => OnChoiceSelected(index)); // 버튼 클릭 시 호출할 함수 등록
        }

        eventImage.sprite = Resources.Load<Sprite>($"KImage/Event{eventId}");
    }

    //이벤트 12 샘의 정령
    private void SpringSpirit(int eventId)
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

            button.onClick.AddListener(() => OnChoiceSelected(index)); // 버튼 클릭 시 호출할 함수 등록
        }

        eventImage.sprite = Resources.Load<Sprite>($"KImage/Event{eventId}");
    }


}
