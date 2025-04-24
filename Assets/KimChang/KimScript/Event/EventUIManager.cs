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
    [SerializeField] private Button leaveBtn;                   //떠나기

    [SerializeField] private UnitSelectUI unitSelectUI;       //유닛 선택 창

    [SerializeField] private TextMeshProUGUI selectTitle;       //선택창 글자
    [SerializeField] private ObjectPool objectPool;

    private void Awake()
    {
        SaveData save = new();
        save.LoadData();

        ResetUI();
        EventManager.LoadEventData();
        gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        ResetUI();
        RogueLikeData.Instance.SetSelectedUnits(null);

        EventData eventData = EventManager.GetRandomEvent();
        List<EventChoiceData> eventChoiceDatas = new List<EventChoiceData>();

        foreach (int choiceId in eventData.choiceIds)
        {
            if (EventDataLoader.EventChoiceDataDict.TryGetValue(choiceId, out var choiceData))
            {
                eventChoiceDatas.Add(choiceData);
            }
        }

        // 이벤트 이미지 캐싱 로드
        eventImage.sprite = SpriteCacheManager.GetSprite($"EventImages/Event{eventData.eventId}");

        eventNameText.text = eventData.eventName;
        eventDescriptionText.text = eventData.description;

        for (int i = 0; i < choiceBtns.childCount; i++)
        {
            GameObject child = choiceBtns.transform.GetChild(i).gameObject;

            if (i < eventChoiceDatas.Count)
            {
                child.SetActive(true);
                child.GetComponentInChildren<TextMeshProUGUI>().text = eventChoiceDatas[i].choiceText;

                Button btn = child.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();

                btn.interactable = EventManager.CheckChoiceRequireCondition(eventChoiceDatas[i]);

                int index = i;
                btn.onClick.AddListener(() => HandleChoice(eventChoiceDatas[index]));
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
        List<RogueUnitDataBase> selectedUnits = new();
        //만약 유닛 선택이 있다면 유닛 선택 창 띄우기
        if (choiceData.requireForm.Contains(RequireForm.Select))
        {
            int index = choiceData.requireForm.IndexOf(RequireForm.Select);
            int count = int.TryParse(choiceData.requireCount[index], out var parsed) ? parsed : 0;
            selectedUnits = RogueLikeData.Instance.GetSelectedUnits();
            if (selectedUnits==null || selectedUnits.Count < count)
            {
                OpenSelectdUnit(choiceData);
                return;
            }

        }
        EventManager.ReduceRequire(choiceData);
        string resultText = EventManager.ApplyChoiceResult(choiceData, selectedUnits);
        eventDescriptionText.text = resultText;
        ResetButtonUI();
        //이벤트 추가
        RogueLikeData.Instance.AddEncounteredEvent(choiceData.eventId);
        leaveBtn.gameObject.SetActive(true);
    }
    private void OpenSelectdUnit(EventChoiceData choiceData)
    {
        List<RogueUnitDataBase> myUnits =RogueLikeData.Instance.GetMyUnits();
        List<RogueUnitDataBase> selectUnits = new();
        for(int i =0; i< choiceData.requireForm.Count; i++)
        {
            if (choiceData.requireForm[i] != RequireForm.Select) continue;
            switch (choiceData.requireThing[i])
            {
                case RequireThing.Unit:
                    if (choiceData.requireValue[i].Contains("")) selectUnits = myUnits;
                    else
                    {
                        if (int.TryParse(choiceData.requireValue[i], out int value))
                        {
                            selectUnits = myUnits.FindAll(unit => unit.rarity <= value);
                        }
                    }
                    break;
                case RequireThing.Energy:
                    if (choiceData.requireValue[i].Contains("")) selectUnits = myUnits;
                    else
                    {
                        if (int.TryParse(choiceData.requireValue[i], out int value))
                        {
                            selectUnits = myUnits.FindAll(unit => unit.energy <= value);
                        }
                    }
                    break;
            }
        }
        unitSelectUI.gameObject.SetActive(true);
        unitSelectUI.OpenSelectUnitWindow(()=>HandleChoice(choiceData),selectUnits);
    }

    //전체 초기화
    private void ResetUI()
    {
        ResetButtonUI();
        unitSelectUI.gameObject.SetActive(false);
        leaveBtn.onClick.AddListener(ClickLeaveBtn);
        leaveBtn.gameObject.SetActive(false);
    }
    //버튼 전부 비활성화 
    private void ResetButtonUI()
    {
        foreach (Transform child in choiceBtns.transform)
        {
            child.gameObject.SetActive(false);
        }
    }
    //
    private void ClickLeaveBtn()
    {
        gameObject.SetActive(false);
    }

}
