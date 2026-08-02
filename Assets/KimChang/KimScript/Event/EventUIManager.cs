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

    [SerializeField] private LineUpBar lineUpBar;

    private readonly List<EventChoiceData> currentChoiceDatas = new List<EventChoiceData>();

    private float leaveButtonWidth = 580f;
    private float leaveButtonHeight = 80f;

    private void Awake()
    {
        EnsureUnitListUI();
        ConfigureChoiceButtonParentLayout();
        ResetUI();
    }

    private void OnEnable()
    {
        ResetUI();
        RogueLikeData.Instance.SetSelectedUnits(new List<RogueUnitDataBase>());

        EventSnapshot snapshot = RogueLikeData.Instance.GetCurrentEventSnapshot();
        EventData eventData = null;

        if (snapshot != null && snapshot.eventId >= 0)
            eventData = EventManager.GetEventByIdRaw(snapshot.eventId);

        if (eventData == null)
        {
            eventData = EventManager.GetRandomEvent();
            if (eventData == null)
            {
                gameObject.SetActive(false);
                return;
            }

            RogueLikeData.Instance.OpenEventSnapshot(eventData.eventId);
            RogueLikeData.Instance.SaveNow();
            snapshot = RogueLikeData.Instance.GetCurrentEventSnapshot();
        }

        currentChoiceDatas.Clear();
        if (eventData.choiceIds != null)
        {
            for (int i = 0; i < eventData.choiceIds.Count; i++)
            {
                int choiceId = eventData.choiceIds[i];
                if (EventDataLoader.EventChoiceDataDict.TryGetValue(choiceId, out EventChoiceData choiceData))
                    currentChoiceDatas.Add(choiceData);
            }
        }

        eventImage.sprite = SpriteCacheManager.GetSprite($"EventImages/Event{eventData.eventId}");
        eventNameText.text = NormalizeChoiceDisplayText(GetEventTitle(eventData));
        eventDescriptionText.text = NormalizeChoiceDisplayText(snapshot != null && snapshot.resultApplied
            ? snapshot.resultText
            : GetEventDescription(eventData));

        if (snapshot != null && snapshot.resultApplied)
        {
            ResetButtonUI();

            // 구버전 저장에서 전투 시작 이벤트 스냅샷이 남아 있으면 선택지 없는 화면에 고정되지 않도록 정리한다.
            if (snapshot.closeEventAfterResult)
            {
                RogueLikeData.Instance.ClearEventSnapshot();
                RogueLikeData.Instance.SaveNow();
                gameObject.SetActive(false);
                return;
            }

            leaveBtn.gameObject.SetActive(true);
            return;
        }

        RefreshChoiceButtonViews();
        TutorialHook.EnqueueAndNotifyCurrentStage(TutorialId.STG_04_EVENT, "Event");
    }

    // 사용처: 이벤트 진입 후와 반복 선택지 처리 후 선택 버튼의 문구와 활성 상태를 다시 맞춘다.
    private void RefreshChoiceButtonViews()
    {
        if (choiceBtns == null)
            return;

        for (int i = 0; i < choiceBtns.childCount; i++)
        {
            GameObject child = choiceBtns.GetChild(i).gameObject;

            if (i >= currentChoiceDatas.Count)
            {
                child.SetActive(false);
                continue;
            }

            EventChoiceData choiceData = currentChoiceDatas[i];
            bool canSelect = EventManager.CheckChoiceRequireCondition(choiceData);
            string choiceText = GetChoiceText(choiceData);

            child.SetActive(true);
            ApplyChoiceButtonView(child, choiceData, choiceText, canSelect);

            Button button = child.GetComponent<Button>();
            if (button == null)
                continue;

            button.onClick.RemoveAllListeners();
            button.interactable = canSelect;
            button.onClick.AddListener(() => HandleChoice(choiceData));
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(choiceBtns as RectTransform);
    }

    // 사용처: 이벤트 선택지를 실행하고 결과 상태를 저장한 뒤 일반 이벤트, 반복 이벤트, 전투 이벤트를 분기한다.
    private void HandleChoice(EventChoiceData choiceData)
    {
        if (choiceData == null || !EventManager.CheckChoiceRequireCondition(choiceData))
            return;

        List<RogueUnitDataBase> selectedUnits = new List<RogueUnitDataBase>();
        if (choiceData.requireForm != null && choiceData.requireForm.Contains(RequireForm.Select))
        {
            int selectIndex = choiceData.requireForm.IndexOf(RequireForm.Select);
            int requiredCount = 1;

            if (choiceData.requireCount != null &&
                selectIndex >= 0 &&
                selectIndex < choiceData.requireCount.Count &&
                int.TryParse(choiceData.requireCount[selectIndex], out int parsedCount))
            {
                requiredCount = Mathf.Max(1, parsedCount);
            }

            List<RogueUnitDataBase> currentSelection = RogueLikeData.Instance.GetSelectedUnits();
            if (currentSelection == null || currentSelection.Count < requiredCount)
            {
                OpenSelectdUnit(choiceData);
                return;
            }

            selectedUnits = new List<RogueUnitDataBase>(currentSelection);
        }

        // 유닛 선택 창을 연 뒤 자원이나 유닛 상태가 변경됐을 때도 잘못된 차감을 막는다.
        if (!EventManager.CheckChoiceRequireCondition(choiceData))
        {
            RogueLikeData.Instance.SetSelectedUnits(new List<RogueUnitDataBase>());
            RefreshChoiceButtonViews();
            return;
        }

        TutorialHook.CancelCurrentStageContext("Event");
        EventManager.ReduceRequire(choiceData);
        (string resultText, bool startsBattle) = EventManager.ApplyChoiceResult(choiceData, selectedUnits);
        eventDescriptionText.text = NormalizeChoiceDisplayText(resultText);

        bool repeatsCurrentEvent = choiceData.choiceId == 56 || choiceData.choiceId == 57;
        if (repeatsCurrentEvent)
        {
            // 반복 이벤트는 아직 종료되지 않았다. 결과 스냅샷을 저장하면 이어하기 후 선택지가 사라진다.
            RogueLikeData.Instance.SetSelectedUnits(new List<RogueUnitDataBase>());
            RogueLikeData.Instance.SaveNow();
            RefreshChoiceButtonViews();

            if (lineUpBar != null)
                lineUpBar.RefreshUnitList();

            return;
        }

        RogueLikeData.Instance.SetEventResultSnapshot(
            choiceData.eventId,
            choiceData.choiceId,
            resultText,
            startsBattle);

        ResetButtonUI();
        RogueLikeData.Instance.AddEncounteredEvent(choiceData.eventId);
        RogueLikeData.Instance.SetSelectedUnits(new List<RogueUnitDataBase>());

        if (lineUpBar != null)
            lineUpBar.RefreshUnitList();

        if (startsBattle)
        {
            // 전투 진입 정보는 전장 프리셋과 보상 큐에 저장된다.
            // 종료된 이벤트 스냅샷을 남기면 전투 후 선택지 없는 이벤트 화면으로 복원될 수 있다.
            RogueLikeData.Instance.ClearEventSnapshot();
            RogueLikeData.Instance.BeginBattleResumeSnapshot(true);
            RogueLikeData.Instance.SaveNow();
            gameObject.SetActive(false);
            return;
        }

        RogueLikeData.Instance.SaveNow();
        leaveBtn.gameObject.SetActive(true);
    }

    // 사용처: Select 요구조건에 맞는 유닛만 UnitListUI에 전달한다.
    private void OpenSelectdUnit(EventChoiceData choiceData)
    {
        if (choiceData == null || choiceData.requireForm == null || choiceData.requireThing == null)
            return;

        if (!EnsureUnitListUI())
            return;

        List<RogueUnitDataBase> myUnits = RogueLikeData.Instance.GetMyTeam() ?? new List<RogueUnitDataBase>();
        int selectIndex = choiceData.requireForm.FindIndex(form => form == RequireForm.Select);
        if (selectIndex < 0)
            return;

        int requiredCount = 1;
        if (choiceData.requireCount != null &&
            selectIndex < choiceData.requireCount.Count &&
            int.TryParse(choiceData.requireCount[selectIndex], out int parsedCount))
        {
            requiredCount = Mathf.Max(1, parsedCount);
        }

        List<RogueUnitDataBase> candidates = myUnits.FindAll(unit => unit != null);
        for (int i = 0; i < choiceData.requireForm.Count; i++)
        {
            if (choiceData.requireForm[i] != RequireForm.Select)
                continue;

            RequireThing thing = i < choiceData.requireThing.Count
                ? choiceData.requireThing[i]
                : RequireThing.None;
            string value = choiceData.requireValue != null && i < choiceData.requireValue.Count
                ? choiceData.requireValue[i] ?? string.Empty
                : string.Empty;

            if (thing == RequireThing.Unit)
            {
                if (value.Contains("~"))
                {
                    (int min, int max) = EventManager.ParseRange(value);
                    candidates = candidates.FindAll(unit => unit != null && unit.rarity >= min && unit.rarity <= max);
                }
                else if (int.TryParse(value, out int rarityCeil))
                {
                    candidates = candidates.FindAll(unit => unit != null && unit.rarity <= rarityCeil);
                }
            }
            else if (thing == RequireThing.Energy && int.TryParse(value, out int energyValue))
            {
                candidates = energyValue < 0
                    ? candidates.FindAll(unit => unit != null && !unit.IsEnergyLockedByRarity && unit.Energy >= Mathf.Abs(energyValue))
                    : candidates.FindAll(unit => unit != null && !unit.IsEnergyLockedByRarity && unit.Energy > energyValue);
            }
        }

        if (candidates.Count < requiredCount)
        {
            Debug.LogWarning($"[EventUIManager] 선택 가능한 유닛이 부족합니다. choiceId={choiceData.choiceId}, required={requiredCount}, candidates={candidates.Count}");
            RefreshChoiceButtonViews();
            return;
        }

        RogueLikeData.Instance.SetSelectedUnits(new List<RogueUnitDataBase>());
        unitListUI.Show(requiredCount, candidates, () => HandleChoice(choiceData));
    }

    //전체 초기화
    private void ResetUI()
    {
        ResetButtonUI();
        if (EnsureUnitListUI())
            unitListUI.gameObject.SetActive(false);
        leaveBtn.onClick.RemoveListener(ClickLeaveBtn);
        leaveBtn.onClick.AddListener(ClickLeaveBtn);
        leaveBtn.gameObject.SetActive(false);
    }

    private bool EnsureUnitListUI()
    {
        if (unitListUI != null)
            return true;

        if (GameManager.Instance != null && GameManager.Instance.unitListUI != null)
        {
            unitListUI = GameManager.Instance.unitListUI;
            return true;
        }

        unitListUI = FindObjectOfType<UnitListUI>(true);
        if (unitListUI != null)
            return true;

        Debug.LogError("[EventUIManager] UnitListUI 참조를 찾을 수 없습니다.");
        return false;
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
        TutorialHook.CancelCurrentStageContext("Event");
        RogueLikeData.Instance.ClearEventSnapshot();
        RogueLikeData.Instance.SaveNow();
        gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        if (GameManager.Instance == null)
            return;

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

    // 사용처: 이벤트 데이터에 저장된 선택지 문장을 우선 표시하고, 누락된 구형 데이터만 GameTextDB에서 보완한다.
    private static string GetChoiceText(EventChoiceData choiceData)
    {
        if (choiceData == null)
            return string.Empty;

        string choiceText = NormalizeChoiceDisplayText(choiceData.choiceText);
        if (!string.IsNullOrEmpty(choiceText))
            return choiceText;

        int titleKey = choiceData.gameTextTitleKey_choiceText != 0 ? choiceData.gameTextTitleKey_choiceText : 100;
        int foreignKey = choiceData.gameTextForeignKey != 0 ? choiceData.gameTextForeignKey : choiceData.choiceId;
        return NormalizeChoiceDisplayText(GameTextDB.GetExact(TextKind.EventDesc, titleKey, foreignKey));
    }

    [System.Serializable]
    private sealed class JsonStringArrayWrapper
    {
        public string[] values;
    }

    // 사용처: GameTextDB에 문자열 배열이 직렬화된 상태로 저장된 경우 UI에 배열 기호와 큰따옴표가 노출되지 않게 한 줄 문자열로 복원한다.
    private static string NormalizeChoiceDisplayText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        string normalized = text.Trim();
        for (int i = 0; i < 2; i++)
            normalized = normalized.Replace("\\\"", "\"");

        normalized = RemoveOuterDoubleQuotes(normalized);

        if (TryUnwrapJsonStringArray(normalized, out string arrayText))
            normalized = RemoveOuterDoubleQuotes(arrayText);

        return normalized.Trim();
    }

    // 사용처: 문자열 전체를 감싼 저장용 큰따옴표만 제거하고, 문장 내부의 정상적인 인용 부호는 유지한다.
    private static string RemoveOuterDoubleQuotes(string text)
    {
        string normalized = text ?? string.Empty;
        while (HasOuterDoubleQuotes(normalized))
            normalized = normalized.Substring(1, normalized.Length - 2).Trim();

        return normalized;
    }

    // 사용처: GameTextDB가 반환한 ["문장"] 또는 ["문장1", "문장2"] 형식을 일반 텍스트로 변환한다.
    private static bool TryUnwrapJsonStringArray(string text, out string result)
    {
        result = string.Empty;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        string source = text.Trim();
        if (source.Length < 2 || source[0] != '[' || source[source.Length - 1] != ']')
            return false;

        try
        {
            JsonStringArrayWrapper wrapper = JsonUtility.FromJson<JsonStringArrayWrapper>("{\"values\":" + source + "}");
            if (wrapper == null || wrapper.values == null || wrapper.values.Length == 0)
                return false;

            StringBuilder builder = new StringBuilder(source.Length);
            for (int i = 0; i < wrapper.values.Length; i++)
            {
                string line = wrapper.values[i];
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (builder.Length > 0)
                    builder.Append('\n');

                builder.Append(line.Trim());
            }

            result = builder.ToString();
            return !string.IsNullOrEmpty(result);
        }
        catch
        {
            return false;
        }
    }

    // 사용처: 선택지 문자열이 전체 큰따옴표로 감싸졌는지 판단한다.
    private static bool HasOuterDoubleQuotes(string text)
    {
        return !string.IsNullOrEmpty(text) &&
               text.Length >= 2 &&
               text[0] == '"' &&
               text[text.Length - 1] == '"';
    }

    // 사용처: 선택지용 긍정 또는 부정 결과 문구를 배열에서 안전하게 가져온다.
    private static string GetChoiceResultText(IList<string> resultTexts, int index)
    {
        if (resultTexts == null || index < 0 || index >= resultTexts.Count)
            return string.Empty;

        return NormalizeChoiceDisplayText(resultTexts[index]);
    }

    // 사용처: 선택지의 resultText 규칙을 그대로 사용해 첫 번째 문구는 긍정, 두 번째 문구는 부정으로 읽는다.
    private static string GetChoiceEffectText(EventChoiceData choiceData, int resultIndex)
    {
        if (choiceData == null)
            return string.Empty;

        string text = GetChoiceResultText(choiceData.choiceResultText, resultIndex);
        if (!string.IsNullOrEmpty(text))
            return text;

        return GetChoiceResultText(choiceData.resultText, resultIndex);
    }

    // 사용처: 선택 전에는 실제 대상이 정해지지 않은 결과 문장의 {require[n]}, {result[n]} 토큰을 읽을 수 있는 일반 표현으로 치환한다.
    private static string BuildPreviewText(string text, EventChoiceData choiceData)
    {
        string preview = NormalizeChoiceDisplayText(text);
        if (string.IsNullOrEmpty(preview) || choiceData == null)
            return preview;

        const int tokenLimit = 8;
        for (int i = 0; i < tokenLimit; i++)
        {
            preview = preview.Replace("{require[" + i + "]}", GetRequirementPreviewToken(choiceData, i));
            preview = preview.Replace("{result[" + i + "]}", GetResultPreviewToken(choiceData, i));
            preview = preview.Replace("{text[" + i + "]}", GetResultPreviewToken(choiceData, i));
        }

        return preview;
    }

    // 사용처: 선택지 미리보기에서 요구 대상이 아직 선택되지 않았을 때 대상 종류를 안내한다.
    private static string GetRequirementPreviewToken(EventChoiceData choiceData, int index)
    {
        if (choiceData.requireThing == null || index < 0 || index >= choiceData.requireThing.Count)
            return "대상";

        RequireThing thing = choiceData.requireThing[index];
        RequireForm form = choiceData.requireForm != null && index < choiceData.requireForm.Count
            ? choiceData.requireForm[index]
            : RequireForm.None;

        switch (thing)
        {
            case RequireThing.Unit:
            case RequireThing.Energy:
                return form == RequireForm.Random ? "무작위 유닛" : "선택한 유닛";

            case RequireThing.Relic:
                return form == RequireForm.Random ? "무작위 전쟁 유산" : "전쟁 유산";

            case RequireThing.Gold:
                return "금화";

            case RequireThing.Morale:
                return "사기";

            default:
                return "대상";
        }
    }

    // 사용처: 선택지 미리보기에서 아직 지급되지 않은 결과 대상의 종류를 안내한다.
    private static string GetResultPreviewToken(EventChoiceData choiceData, int index)
    {
        if (choiceData.resultType == null || index < 0 || index >= choiceData.resultType.Count)
            return "효과";

        switch (choiceData.resultType[index])
        {
            case ResultType.Relic:
            case ResultType.Curse:
                return "전쟁 유산";

            case ResultType.Unit:
                return "유닛";

            case ResultType.Change:
                return "전직 유닛";

            case ResultType.Gold:
                return "금화";

            case ResultType.Morale:
                return "사기";

            case ResultType.Energy:
                return "유닛";

            case ResultType.Training:
                return "전술 개량";

            default:
                return "효과";
        }
    }

    // 사용처: resultText 데이터가 없는 기존 이벤트를 위해 실제 요구 조건과 결과 타입을 기준으로 초록/빨강 미리보기 문구를 만든다.
    private static void BuildFallbackChoiceEffects(EventChoiceData choiceData, out string positive, out string negative)
    {
        StringBuilder positiveBuilder = new StringBuilder(64);
        StringBuilder negativeBuilder = new StringBuilder(64);

        if (choiceData == null)
        {
            positive = string.Empty;
            negative = string.Empty;
            return;
        }

        AppendRequirementCosts(choiceData, negativeBuilder);
        AppendResultBenefits(choiceData, positiveBuilder, negativeBuilder);

        if (choiceData.choiceId == 113)
            AppendPreviewLine(negativeBuilder, "선택한 유닛의 기력이 1이 됩니다.");

        positive = positiveBuilder.ToString();
        negative = negativeBuilder.ToString();
    }

    // 사용처: 금화, 사기, 유닛, 유산처럼 선택 즉시 소모되는 요구 조건을 빨간색 미리보기 대상으로 만든다.
    private static void AppendRequirementCosts(EventChoiceData choiceData, StringBuilder negativeBuilder)
    {
        if (choiceData.requireThing == null)
            return;

        for (int i = 0; i < choiceData.requireThing.Count; i++)
        {
            RequireThing thing = choiceData.requireThing[i];
            RequireForm form = choiceData.requireForm != null && i < choiceData.requireForm.Count
                ? choiceData.requireForm[i]
                : RequireForm.None;
            string value = choiceData.requireValue != null && i < choiceData.requireValue.Count
                ? choiceData.requireValue[i] ?? string.Empty
                : string.Empty;
            string count = EventManager.GetChoiceRequireCountForPreview(choiceData, i);

            switch (thing)
            {
                case RequireThing.Gold:
                    if (form == RequireForm.None && TryGetPositiveInt(count, out int gold))
                        AppendPreviewLine(negativeBuilder, "금화 " + gold + " 소모");
                    break;

                case RequireThing.Morale:
                    if (form == RequireForm.None && TryGetPositiveInt(count, out int morale))
                        AppendPreviewLine(negativeBuilder, "사기 " + morale + " 감소");
                    break;

                case RequireThing.Relic:
                    if (form == RequireForm.Random && TryGetPositiveInt(count, out int relicCount))
                        AppendPreviewLine(negativeBuilder, "전쟁 유산 " + relicCount + "개 소모");
                    break;

                case RequireThing.Unit:
                    if (form == RequireForm.Random && TryGetPositiveInt(count, out int unitCount))
                        AppendPreviewLine(negativeBuilder, "무작위 유닛 " + unitCount + "명 희생");
                    break;

                case RequireThing.Energy:
                    if (form == RequireForm.Select && int.TryParse(value, out int energyDelta) && energyDelta < 0)
                        AppendPreviewLine(negativeBuilder, "선택한 유닛 기력 " + Mathf.Abs(energyDelta) + " 감소");
                    break;
            }
        }
    }

    // 사용처: 이벤트 JSON에 resultText가 없는 경우 resultType, resultForm, resultValue를 읽어 초록색 보상과 빨간색 위험을 표시한다.
    private static void AppendResultBenefits(EventChoiceData choiceData, StringBuilder positiveBuilder, StringBuilder negativeBuilder)
    {
        if (choiceData.resultType == null)
            return;

        for (int i = 0; i < choiceData.resultType.Count; i++)
        {
            ResultType type = choiceData.resultType[i];
            ResultForm form = choiceData.resultForm != null && i < choiceData.resultForm.Count
                ? choiceData.resultForm[i]
                : ResultForm.None;
            string value = choiceData.resultValue != null && i < choiceData.resultValue.Count
                ? choiceData.resultValue[i] ?? string.Empty
                : string.Empty;
            string count = EventManager.GetResultCountForPreview(choiceData, i);

            switch (type)
            {
                case ResultType.Gold:
                    AppendPreviewLine(positiveBuilder, "금화 " + GetPreviewAmount(count, "획득"));
                    break;

                case ResultType.Morale:
                    AppendPreviewLine(positiveBuilder, "사기 " + GetPreviewAmount(count, "회복"));
                    break;

                case ResultType.Energy:
                    if (int.TryParse(value, out int energy) && energy < 0)
                        AppendPreviewLine(negativeBuilder, "유닛 기력 " + Mathf.Abs(energy) + " 감소");
                    else
                        AppendPreviewLine(positiveBuilder, form == ResultForm.All ? "전체 유닛 기력 회복" : "선택한 유닛 기력 회복");
                    break;

                case ResultType.Relic:
                    if (form == ResultForm.Random && value == "0")
                        AppendPreviewLine(negativeBuilder, "저주 전쟁 유산 획득");
                    else
                        AppendPreviewLine(positiveBuilder, "전쟁 유산 획득");
                    break;

                case ResultType.Curse:
                    AppendPreviewLine(negativeBuilder, "저주 전쟁 유산 획득");
                    break;

                case ResultType.Unit:
                    AppendPreviewLine(positiveBuilder, "유닛 획득");
                    break;

                case ResultType.Change:
                    AppendPreviewLine(positiveBuilder, "유닛 전직");
                    break;

                case ResultType.Training:
                    AppendPreviewLine(positiveBuilder, "병종 강화");
                    break;

                case ResultType.Battle:
                    AppendPreviewLine(negativeBuilder, "전투 발생");
                    break;

                case ResultType.None:
                    if (positiveBuilder.Length == 0 && negativeBuilder.Length == 0)
                        AppendPreviewLine(positiveBuilder, "아무 일도 없음");
                    break;
            }
        }
    }

    // 사용처: 색상별 미리보기 문구를 줄 단위로 추가한다.
    private static void AppendPreviewLine(StringBuilder builder, string line)
    {
        if (builder == null || string.IsNullOrWhiteSpace(line))
            return;

        if (builder.Length > 0)
            builder.Append('\n');

        builder.Append(line);
    }

    // 사용처: 숫자형 결과 값이 없는 JSON에서도 미리보기 문장을 유지한다.
    private static string GetPreviewAmount(string value, string suffix)
    {
        return int.TryParse(value, out int amount) && amount > 0
            ? amount + " " + suffix
            : suffix;
    }

    // 사용처: 비용 조건이 실제로 양수인지 검사한다.
    private static bool TryGetPositiveInt(string value, out int result)
    {
        return int.TryParse(value, out result) && result > 0;
    }

    // 사용처: 선택지 버튼 텍스트를 기본 흰색, resultText[0] 긍정 초록색, resultText[1] 부정 빨간색 순서로 조합한다.
    private static string BuildChoiceButtonRichText(EventChoiceData choiceData, string choiceText, bool canSelect)
    {
        string safeChoiceText = BuildPreviewText(choiceText, choiceData);

        if (!canSelect)
        {
            return
                "<color=#9A9A9A>" + safeChoiceText + "</color>\n" +
                "<color=#B8A98E>조건 미충족</color>";
        }

        if (choiceData == null)
            return "<color=#FFFFFF>" + safeChoiceText + "</color>";

        string positive = BuildPreviewText(GetChoiceEffectText(choiceData, 0), choiceData);
        string negative = BuildPreviewText(GetChoiceEffectText(choiceData, 1), choiceData);

        BuildFallbackChoiceEffects(choiceData, out string fallbackPositive, out string fallbackNegative);

        if (string.IsNullOrEmpty(positive))
            positive = fallbackPositive;

        if (string.IsNullOrEmpty(negative))
            negative = fallbackNegative;

        StringBuilder builder = new StringBuilder(160);
        builder.Append("<color=#FFFFFF>");
        builder.Append(safeChoiceText);
        builder.Append("</color>");

        if (!string.IsNullOrEmpty(positive))
        {
            builder.Append('\n');
            builder.Append("<color=#8DFF4A>");
            builder.Append(positive);
            builder.Append("</color>");
        }

        if (!string.IsNullOrEmpty(negative))
        {
            builder.Append('\n');
            builder.Append("<color=#FF5A3C>");
            builder.Append(negative);
            builder.Append("</color>");
        }

        return builder.ToString();
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
}
