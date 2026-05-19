using System.Collections.Generic;
using System.Text;
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

    private float choiceButtonWidth = 580f;
    private float choiceButtonMinHeight = 80f;
    private float choiceButtonTextLeft = 32f;
    private float choiceButtonTextRight = 32f;
    private float choiceButtonTextTop = 10f;
    private float choiceButtonTextBottom = 10f;

    private bool useChoiceIcon = false;
    private float choiceIconSize = 56f;
    private float choiceIconLeft = 24f;
    private float choiceIconTextGap = 16f;

    [SerializeField] private Sprite defaultChoiceIcon;
    [SerializeField] private Sprite battleIcon;
    [SerializeField] private Sprite goldIcon;
    [SerializeField] private Sprite moraleIcon;
    [SerializeField] private Sprite energyIcon;
    [SerializeField] private Sprite relicIcon;
    [SerializeField] private Sprite unitIcon;
    [SerializeField] private Sprite disabledIcon;

    private void Awake()
    {
        ConfigureChoiceButtonParentLayout();
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

                EventChoiceData choiceData = eventChoiceDatas[i];
                string choiceText = GetChoiceText(choiceData);
                bool canSelect = EventManager.CheckChoiceRequireCondition(choiceData);

                ApplyChoiceButtonView(child, choiceData, choiceText, canSelect);

                Button btn = child.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.interactable = canSelect;
                btn.onClick.AddListener(() => HandleChoice(choiceData));
            }
            else
            {
                child.SetActive(false);
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(choiceBtns as RectTransform);
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
        GameManager.Instance.RefreshNodeInfoButtonVisibility();
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
        int titleKey = choiceData.gameTextTitleKey_choiceText != 0 ? choiceData.gameTextTitleKey_choiceText : 100;
        int foreignKey = choiceData.gameTextForeignKey != 0 ? choiceData.gameTextForeignKey : choiceData.choiceId;
        string text = GameTextDB.GetExact(TextKind.EventDesc, titleKey, foreignKey);
        return string.IsNullOrEmpty(text) ? choiceData.choiceText : text;
    }

    // 사용처: 선택지 버튼 텍스트를 기본 흰색, 위험/소모 붉은색, 보상/이득 초록색으로 조합
    private static string BuildChoiceButtonRichText(EventChoiceData choiceData, string choiceText, bool canSelect)
    {
        if (!canSelect)
        {
            return
                $"<color=#9A9A9A>{choiceText}</color>\n" +
                "<color=#B8A98E>조건 미충족</color>";
        }

        int foreignKey = choiceData.gameTextForeignKey != 0 ? choiceData.gameTextForeignKey : choiceData.choiceId;
        int positiveKey = choiceData.gameTextTitleKey_positive != 0 ? choiceData.gameTextTitleKey_positive : 110;
        int negativeKey = choiceData.gameTextTitleKey_negative != 0 ? choiceData.gameTextTitleKey_negative : 111;

        string positive = GameTextDB.GetExact(TextKind.EventDesc, positiveKey, foreignKey);
        string negative = GameTextDB.GetExact(TextKind.EventDesc, negativeKey, foreignKey);

        if (string.IsNullOrEmpty(positive) && choiceData.choiceResultText != null && choiceData.choiceResultText.Count > 0)
            positive = choiceData.choiceResultText[0];

        if (string.IsNullOrEmpty(negative) && choiceData.choiceResultText != null && choiceData.choiceResultText.Count > 1)
            negative = choiceData.choiceResultText[1];

        if (string.IsNullOrEmpty(positive) && string.IsNullOrEmpty(negative))
        {
            int resultDescriptionKey = choiceData.gameTextTitleKey_resultDescription != 0 ? choiceData.gameTextTitleKey_resultDescription : 120;
            string resultDescription = GameTextDB.GetExact(TextKind.EventDesc, resultDescriptionKey, foreignKey);
            if (string.IsNullOrEmpty(resultDescription))
                resultDescription = choiceData.resultDescription;

            positive = resultDescription;
        }

        StringBuilder sb = new StringBuilder(128);

        sb.Append("<color=#FFFFFF>");
        sb.Append(choiceText);
        sb.Append("</color>");

        if (!string.IsNullOrEmpty(negative))
        {
            sb.Append('\n');
            sb.Append("<color=#FF5A3C>");
            sb.Append(negative);
            sb.Append("</color>");
        }

        if (!string.IsNullOrEmpty(positive))
        {
            sb.Append('\n');
            sb.Append("<color=#8DFF4A>");
            sb.Append(positive);
            sb.Append("</color>");
        }

        return sb.ToString();
    }

    // 사용처: 선택지 버튼 부모와 버튼 루트의 레이아웃 충돌을 방지하고 고정 폭, 유동 높이 구조로 설정
    private void ConfigureChoiceButtonParentLayout()
    {
        if (choiceBtns == null)
            return;

        VerticalLayoutGroup layoutGroup = choiceBtns.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
        }

        for (int i = 0; i < choiceBtns.childCount; i++)
        {
            GameObject buttonObject = choiceBtns.GetChild(i).gameObject;

            ContentSizeFitter fitter = buttonObject.GetComponent<ContentSizeFitter>();
            if (fitter != null)
                fitter.enabled = false;

            HorizontalLayoutGroup horizontalLayout = buttonObject.GetComponent<HorizontalLayoutGroup>();
            if (horizontalLayout != null)
                horizontalLayout.enabled = false;

            LayoutElement buttonLayout = buttonObject.GetComponent<LayoutElement>();
            if (buttonLayout == null)
                buttonLayout = buttonObject.AddComponent<LayoutElement>();

            buttonLayout.minWidth = choiceButtonWidth;
            buttonLayout.preferredWidth = choiceButtonWidth;
            buttonLayout.flexibleWidth = 0f;
        }
    }


    // 사용처: 선택지 버튼의 텍스트 색상, 비활성 상태, 아이콘 예약 영역, 유동 높이를 적용
    private void ApplyChoiceButtonView(GameObject buttonObject, EventChoiceData choiceData, string choiceText, bool canSelect)
    {
        if (buttonObject == null)
            return;

        bool hasIcon = ApplyReservedChoiceIcon(buttonObject, choiceData, canSelect);

        TextMeshProUGUI text = buttonObject.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text == null)
            return;

        string displayText = BuildChoiceButtonRichText(choiceData, choiceText, canSelect);

        text.richText = true;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Overflow;
        text.color = Color.white;
        text.text = displayText;

        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.pivot = new Vector2(0.5f, 0.5f);

        float left = hasIcon
            ? choiceIconLeft + choiceIconSize + choiceIconTextGap
            : choiceButtonTextLeft;

        textRect.offsetMin = new Vector2(left, choiceButtonTextBottom);
        textRect.offsetMax = new Vector2(-choiceButtonTextRight, -choiceButtonTextTop);

        float textWidth = choiceButtonWidth - left - choiceButtonTextRight;
        textWidth = Mathf.Max(1f, textWidth);

        Vector2 preferredTextSize = text.GetPreferredValues(displayText, textWidth, Mathf.Infinity);

        float buttonHeight = preferredTextSize.y + choiceButtonTextTop + choiceButtonTextBottom;
        buttonHeight = Mathf.Max(choiceButtonMinHeight, Mathf.Ceil(buttonHeight));

        LayoutElement buttonLayout = buttonObject.GetComponent<LayoutElement>();
        if (buttonLayout == null)
            buttonLayout = buttonObject.AddComponent<LayoutElement>();

        buttonLayout.minWidth = choiceButtonWidth;
        buttonLayout.preferredWidth = choiceButtonWidth;
        buttonLayout.flexibleWidth = 0f;

        buttonLayout.minHeight = buttonHeight;
        buttonLayout.preferredHeight = buttonHeight;
        buttonLayout.flexibleHeight = 0f;
    }

    // 사용처: 추후 선택지 아이콘이 추가되면 자동 사용하고, 현재 아이콘이 없으면 텍스트 전용 버튼으로 처리
    private bool ApplyReservedChoiceIcon(GameObject buttonObject, EventChoiceData choiceData, bool canSelect)
    {
        Transform iconTransform = buttonObject.transform.Find("Icon");
        if (iconTransform == null)
            return false;

        Image iconImage = iconTransform.GetComponent<Image>();
        if (iconImage == null)
        {
            iconTransform.gameObject.SetActive(false);
            return false;
        }

        Sprite iconSprite = useChoiceIcon ? GetReservedChoiceIcon(choiceData, canSelect) : null;

        if (iconSprite == null)
        {
            iconImage.enabled = false;
            iconTransform.gameObject.SetActive(false);
            return false;
        }

        iconTransform.gameObject.SetActive(true);
        iconImage.enabled = true;
        iconImage.sprite = iconSprite;
        iconImage.preserveAspect = true;

        RectTransform iconRect = iconTransform as RectTransform;
        if (iconRect != null)
        {
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(choiceIconLeft + choiceIconSize * 0.5f, 0f);
            iconRect.sizeDelta = new Vector2(choiceIconSize, choiceIconSize);
        }

        return true;
    }

    // 사용처: 선택지 결과 타입을 기준으로 추후 추가될 아이콘 Sprite를 반환
    private Sprite GetReservedChoiceIcon(EventChoiceData choiceData, bool canSelect)
    {
        if (!canSelect && disabledIcon != null)
            return disabledIcon;

        if (choiceData == null || choiceData.resultType == null || choiceData.resultType.Count == 0)
            return defaultChoiceIcon;

        for (int i = 0; i < choiceData.resultType.Count; i++)
        {
            string resultTypeName = choiceData.resultType[i].ToString();

            switch (resultTypeName)
            {
                case "Battle":
                    return battleIcon != null ? battleIcon : defaultChoiceIcon;

                case "Gold":
                    return goldIcon != null ? goldIcon : defaultChoiceIcon;

                case "Morale":
                    return moraleIcon != null ? moraleIcon : defaultChoiceIcon;

                case "Energy":
                    return energyIcon != null ? energyIcon : defaultChoiceIcon;

                case "Relic":
                case "Curse":
                    return relicIcon != null ? relicIcon : defaultChoiceIcon;

                case "Unit":
                case "Change":
                    return unitIcon != null ? unitIcon : defaultChoiceIcon;
            }
        }

        return defaultChoiceIcon;
    }
}
