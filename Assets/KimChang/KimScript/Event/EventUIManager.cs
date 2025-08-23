using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventUIManager : MonoBehaviour
{
    [SerializeField] private Image eventImage;
    [SerializeField] private TextMeshProUGUI eventNameText;
    [SerializeField] private TextMeshProUGUI eventDescriptionText;
    [SerializeField] private Transform choiceBtns;
    [SerializeField] private Button leaveBtn;

    [SerializeField] private UnitSelectUI unitSelectUI;

    [SerializeField] private ObjectPool objectPool;

    private void Awake()
    {
        ResetUI();
    }
    private void OnEnable()
    {
        ResetUI();
        RogueLikeData.Instance.SetSelectedUnits(new List<RogueUnitDataBase>());

        EventData eventData = EventManager.GetRandomEvent();

        List<EventChoiceData> eventChoiceDatas = new();
        
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

                var choiceData = eventChoiceDatas[i];
                string resultPart = "";
                // choiceResultText 처리
                if (choiceData.choiceResultText != null && choiceData.choiceResultText.Count > 0)
                {
                    
                    if (choiceData.choiceResultText.Count == 1)
                    {
                        resultPart = $" <color=green>{choiceData.choiceResultText[0]}</color>";
                        
                    }
                    else if (choiceData.choiceResultText.Count >= 2)
                    {
                        resultPart = $" <color=green>{choiceData.choiceResultText[0]}</color> <color=red>{choiceData.choiceResultText[1]}</color>";
                    }
                }

                // 버튼 텍스트 = choiceText + resultPart
                child.GetComponentInChildren<TextMeshProUGUI>().text = choiceData.choiceText + resultPart;

                Button btn = child.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();

                btn.interactable = EventManager.CheckChoiceRequireCondition(choiceData);

                int index = i;
                btn.onClick.AddListener(() => HandleChoice(choiceData));
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
            if (selectedUnits == null || selectedUnits.Count < count)
            {
                OpenSelectdUnit(choiceData);
                return;
            }
        }
        EventManager.ReduceRequire(choiceData);
        (string, bool) resultText = EventManager.ApplyChoiceResult(choiceData, selectedUnits);
        eventDescriptionText.text = resultText.Item1;
        //만약 56~57 이라면
        if ((choiceData.choiceId >= 56 && choiceData.choiceId <= 57))
        {
            int morale = RogueLikeData.Instance.GetMorale();
            if(morale <10)
            {
                choiceBtns.GetChild(0).gameObject.GetComponent<Button>().interactable = false;
                choiceBtns.GetChild(1).gameObject.GetComponent<Button>().interactable = false;
            }
            RogueLikeData.Instance.SetSelectedUnits(new List<RogueUnitDataBase>());
            return;
        } 

        ResetButtonUI();
        RogueLikeData.Instance.AddEncounteredEvent(choiceData.eventId);
        SaveData saveData = new();
        saveData.SaveDataFile();
        if(resultText.Item2) gameObject.SetActive(false);
        leaveBtn.gameObject.SetActive(true);
    }
    private void OpenSelectdUnit(EventChoiceData choiceData)
    {
        List<RogueUnitDataBase> myUnits = RogueLikeData.Instance.GetMyTeam();
        List<RogueUnitDataBase> selectUnits = new();

        // Select 요구조건(첫 번째 것) 기준으로 정확히 필요한 수 계산
        int selectIdx = choiceData.requireForm.FindIndex(f => f == RequireForm.Select);
        int requiredCount = 1;
        if (selectIdx >= 0 && int.TryParse(choiceData.requireCount[selectIdx], out var need))
            requiredCount = Mathf.Max(1, need);

        for (int i = 0; i < choiceData.requireForm.Count; i++)
        {
            if (choiceData.requireForm[i] != RequireForm.Select) continue;

            var thing = choiceData.requireThing[i];
            var val = choiceData.requireValue[i];

            if (thing == RequireThing.Unit)
            {
                if (string.IsNullOrEmpty(val)) { selectUnits = myUnits; }
                else if (val.Contains("~"))
                {
                    var (min, max) = EventManager.ParseRange(val);
                    selectUnits = myUnits.FindAll(u => u.rarity >= min && u.rarity <= max);
                }
                else if (int.TryParse(val, out var rarityCeil))
                {
                    selectUnits = myUnits.FindAll(u => u.rarity <= rarityCeil);
                }
            }
            else if (thing == RequireThing.Energy)
            {
                if (string.IsNullOrEmpty(val)) { selectUnits = myUnits; }
                else if (int.TryParse(val, out var energyVal))
                {
                    // 요구조건 검사와 일치하도록: '에너지 > value' 만 선택
                    selectUnits = myUnits.FindAll(u => u.energy > energyVal);
                }
            }
        }

        unitSelectUI.gameObject.SetActive(true);
        unitSelectUI.OpenSelectUnitWindow(() => HandleChoice(choiceData), selectUnits, requiredCount);
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
    private void OnDisable()
    {
        GameManager.Instance.UpdateAllUI();
    }
}
