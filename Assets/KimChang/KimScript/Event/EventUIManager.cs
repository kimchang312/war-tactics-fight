using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventUIManager : MonoBehaviour
{
    [SerializeField] private Image eventImage;                  //이벤트 이미지
    [SerializeField] private TextMeshProUGUI eventNameText;        //이벤트 제목
    [SerializeField] private TextMeshProUGUI eventDescriptionText;  //이벤트 설명
    [SerializeField] private Transform choiceBtns;             //선택지 버튼 부모

    [SerializeField] private GameObject selectUnitWindow;       //유닛 선택 창
    [SerializeField] private GameObject selectUnitParent;       //유닛이 배치되는 곳
    [SerializeField] private TextMeshProUGUI selectTitle;       //선택창 글자
    [SerializeField] private ObjectPool objectPool;

    private void Awake()
    {
        ResetUI();
        EventManager.LoadEventData();
    }
    private void OnEnable()
    {
        ResetUI();

        EventData eventData = EventManager.GetRandomEvent();
        List<EventChoiceData> eventChoiceDatas = new List<EventChoiceData>();
        foreach(int choiceId in eventData.choiceIds)
        {
            if (EventDataLoader.EventChoiceDataDict.TryGetValue(choiceId, out var choiceData))
            {
                eventChoiceDatas.Add(choiceData);
            }
        }
        Sprite sprite = Resources.Load<Sprite>($"EventImages/Event{eventData.eventId}");
        eventImage.sprite = sprite;
        eventNameText.text = eventData.eventName;
        eventDescriptionText.text = eventData.description;

        // 버튼 초기화
        for (int i = 0; i < choiceBtns.childCount; i++)
        {
            GameObject child = choiceBtns.transform.GetChild(i).gameObject;

            if (i < eventChoiceDatas.Count)
            {
                child.SetActive(true);
                // 버튼에 선택지 내용 연결
                child.GetComponentInChildren<TextMeshProUGUI>().text = eventChoiceDatas[i].choiceText; 
                Button btn = child.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                if (EventManager.CheckChoiceRequireCondition(eventChoiceDatas[i])) btn.interactable = true;
                else btn.interactable = false;
                btn.onClick.AddListener(() => HandleChoice(eventChoiceDatas[i]));
            }
            else
            {
                child.SetActive(false);
            }
        }

    }
    //선택지 버튼 눌렀을때 실행
    private void HandleChoice(EventChoiceData choiceData)
    {

    }

    //전체 초기화
    private void ResetUI()
    {
        ResetButtonUI();
        selectUnitWindow.SetActive(false);
    }
    //버튼 전부 비활성화 
    private void ResetButtonUI()
    {
        foreach (Transform child in choiceBtns.transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}
