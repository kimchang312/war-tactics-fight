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

    [SerializeField] private UnitListUI unitListUI;

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

        eventNameText.text = GetEventTitle(eventData);
        eventDescriptionText.text = GetEventDescription(eventData);

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
                string choiceText = GetChoiceText(choiceData);
                resultPart = BuildChoiceResultText(choiceData);
                child.GetComponentInChildren<TextMeshProUGUI>().text = choiceText + resultPart;

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
        if (!EventManager.CheckChoiceRequireCondition(choiceData))
            return;

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

        // 선택창에서 오래 머무는 동안 자원이 바뀐 경우를 한번 더 방어
        if (!EventManager.CheckChoiceRequireCondition(choiceData))
        {
            RogueLikeData.Instance.SetSelectedUnits(new List<RogueUnitDataBase>());
            return;
        }

        EventManager.ReduceRequire(choiceData);
        (string, bool) resultText = EventManager.ApplyChoiceResult(choiceData, selectedUnits);
        eventDescriptionText.text = resultText.Item1;
        //만약 56~57 이라면
        if ((choiceData.choiceId >= 56 && choiceData.choiceId <= 57))
        {
            int morale = RogueLikeData.Instance.GetMorale();
            if (morale < 10)
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
        if (resultText.Item2) gameObject.SetActive(false);
        leaveBtn.gameObject.SetActive(true);
        RogueLikeData.Instance.SetSelectedUnits(new List<RogueUnitDataBase>());
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
                    // 기존 조건 검사와 동일하게 value보다 기력이 높은 유닛만 선택 가능
                    selectUnits = myUnits.FindAll(u => u.Energy > energyVal);
                }
            }
        }

        if (selectUnits.Count == 0)
            selectUnits = myUnits;

        RogueLikeData.Instance.SetSelectedUnits(new List<RogueUnitDataBase>());
        unitListUI.Show(requiredCount, selectUnits, () => HandleChoice(choiceData));
    }

    //전체 초기화
    private void ResetUI()
    {
        ResetButtonUI();
        unitListUI.gameObject.SetActive(false);
        leaveBtn.onClick.RemoveListener(ClickLeaveBtn);
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

    // 사용처: GameTextDB에 텍스트가 있으면 우선 사용하고, 없으면 JSON 원문을 사용
    private static string GetTextOrFallback(TextKind kind, int titleKey, int foreignKey, string fallback)
    {
        string text = GameTextDB.Get(kind, titleKey, foreignKey);
        return string.IsNullOrEmpty(text) ? fallback : text;
    }

    // 사용처: 이벤트 제목을 현재 언어 기준으로 가져옴
    private static string GetEventTitle(EventData eventData)
    {
        return GetTextOrFallback(TextKind.EventTitle, -1, eventData.eventId, eventData.eventName);
    }

    // 사용처: 이벤트 설명을 현재 언어 기준으로 가져옴
    private static string GetEventDescription(EventData eventData)
    {
        return GetTextOrFallback(TextKind.EventDesc, -1, eventData.eventId, eventData.description);
    }

    // 사용처: 선택지 버튼의 기본 문장을 현재 언어 기준으로 가져옴
    private static string GetChoiceText(EventChoiceData choiceData)
    {
        return GetTextOrFallback(TextKind.EventDesc, 100, choiceData.choiceId, choiceData.choiceText);
    }

    // 사용처: 선택지 버튼에 성공/실패 결과 요약을 붙임
    private static string BuildChoiceResultText(EventChoiceData choiceData)
    {
        string positive = GameTextDB.Get(TextKind.EventDesc, 110, choiceData.choiceId);
        string negative = GameTextDB.Get(TextKind.EventDesc, 111, choiceData.choiceId);

        if (string.IsNullOrEmpty(positive) && choiceData.choiceResultText != null && choiceData.choiceResultText.Count > 0)
            positive = choiceData.choiceResultText[0];

        if (string.IsNullOrEmpty(negative) && choiceData.choiceResultText != null && choiceData.choiceResultText.Count > 1)
            negative = choiceData.choiceResultText[1];

        if (string.IsNullOrEmpty(positive) && string.IsNullOrEmpty(negative))
        {
            string resultDescription = GameTextDB.Get(TextKind.EventDesc, 120, choiceData.choiceId);
            if (string.IsNullOrEmpty(resultDescription))
                resultDescription = choiceData.resultDescription;

            return string.IsNullOrEmpty(resultDescription)
                ? string.Empty
                : $" <color=green>{resultDescription}</color>";
        }

        if (!string.IsNullOrEmpty(positive) && !string.IsNullOrEmpty(negative))
            return $" <color=green>{positive}</color> <color=red>{negative}</color>";

        if (!string.IsNullOrEmpty(positive))
            return $" <color=green>{positive}</color>";

        return $" <color=red>{negative}</color>";
    }



}
