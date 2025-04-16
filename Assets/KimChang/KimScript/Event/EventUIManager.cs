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

    [SerializeField] private GameObject selectUnitWindow;       //유닛 선택 창
    [SerializeField] private GameObject selectUnitParent;       //유닛이 배치되는 곳
    [SerializeField] private TextMeshProUGUI selectTitle;       //선택창 글자
    [SerializeField] private ObjectPool objectPool;

    [SerializeField] private List<RogueUnitDataBase> selectedUnits;

    private void Awake()
    {
        ResetUI();
        EventManager.LoadEventData();
    }
    private void OnEnable()
    {
        ResetUI();
        selectedUnits.Clear();
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
                if (EventManager.CheckChoiceRequireCondition(eventChoiceDatas[i])){
                    Debug.Log("활성화");
                    btn.interactable = true; }
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
        //만약 유닛 선택이 있다면 유닛 선택 창 띄우기
        if (choiceData.requireForm.Contains(RequireForm.Select))
        {
            int index = choiceData.requireForm.IndexOf(RequireForm.Select);
            int count = int.TryParse(choiceData.requireCount[index], out var parsed) ? parsed : 0;
            if (selectedUnits.Count < count) 
            {
                OpenSelectdUnit(choiceData);
                return;
            }
        }
        string resultText = EventManager.ApplyChoiceResult(choiceData,selectedUnits);

        ResetButtonUI();
        leaveBtn.gameObject.SetActive(true);
    }
    private void OpenSelectdUnit(EventChoiceData choiceData)
    {
        selectUnitWindow.SetActive(true);
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

        foreach(var unit in selectUnits)
        {
            GameObject selectedUnit = objectPool.GetSelectUnit();
            Image img = selectedUnit.GetComponent<Image>();
            Sprite sprite = Resources.Load<Sprite>($"UnitImages/{unit.unitImg}");
            img.sprite = sprite;
            Image energy = selectedUnit.GetComponentInChildren<Image>();
            energy.fillAmount = unit.energy/unit.maxEnergy;
            TextMeshProUGUI textMeshProUGUI = selectedUnit.GetComponentInChildren<TextMeshProUGUI>();
            textMeshProUGUI.text = $"{unit.energy}/{unit.maxEnergy}";
            selectedUnit.transform.SetParent(selectUnitParent.transform, false);
            Button btn  = selectedUnit.GetComponent<Button>();
            btn.onClick.AddListener(()=>AddSelectedUnits(unit,choiceData));
        }
    }

    private void AddSelectedUnits(RogueUnitDataBase unit,EventChoiceData choiceData)
    {
        selectedUnits.Add(unit);
        selectUnitWindow.SetActive(false);
        HandleChoice(choiceData);
    }

    //전체 초기화
    private void ResetUI()
    {
        ResetButtonUI();
        selectUnitWindow.SetActive(false);
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
