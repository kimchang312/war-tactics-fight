using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    private const int JapaneseLanguageIndex = 2;
    private const string KoreanLanguageText = "\uD55C\uAD6D\uC5B4";
    private const string EnglishLanguageText = "English";
    private const string JapaneseLanguageText = "\u65E5\u672C\u8A9E";

    [Header("Audio Settings")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Language Settings")]
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private Sprite languageOptionBackground;
    [SerializeField] private TMP_FontAsset languageDropdownBaseFont;
    [SerializeField] private TMP_FontAsset languageDropdownJapaneseFont;
    [SerializeField] private float languageDropdownOptionHeight;

    [Header("Ingame Buttons Group")]
    [SerializeField] private GameObject ingameButtonsArea;

    [Header("Confirmation Popup")]
    [SerializeField] private GameObject confirmGiveUpPopup;

    [Header("Tutorial Settings")]
    [SerializeField] private Button tutorialResetButton;

    private bool pausedBattleByThisUI;
    private float previousTimeScale = 1f;
    private AutoBattleManager pausedBattleManager;
    private Coroutine dropdownFontRefreshCoroutine;
    private bool languageDropdownOpenEventsBound;

    private void Awake()
    {
        NormalizeLanguageDropdownOptions();
        ConfigureLanguageDropdownVisuals();
        BindUIEvents();
    }

    private void NormalizeLanguageDropdownOptions()
    {
        if (languageDropdown == null)
            return;

        languageDropdown.options.Clear();
        languageDropdown.options.Add(CreateLanguageOption(KoreanLanguageText));
        languageDropdown.options.Add(CreateLanguageOption(EnglishLanguageText));
        languageDropdown.options.Add(CreateLanguageOption(JapaneseLanguageText));
        languageDropdown.RefreshShownValue();
    }

    private TMP_Dropdown.OptionData CreateLanguageOption(string text)
    {
        return new TMP_Dropdown.OptionData(text, languageOptionBackground);
    }

    private void ConfigureLanguageDropdownVisuals()
    {
        if (languageDropdown == null)
            return;

        EnsureJapaneseDropdownFallback();
        ApplyDropdownSprite(languageDropdown.targetGraphic as Image, languageOptionBackground);
        ApplyTextFont(languageDropdown.captionText, GetCaptionFont(languageDropdown.value));
        ApplyTextFont(languageDropdown.itemText, GetBaseDropdownFont());

        if (languageDropdown.template == null)
            return;

        bool useCustomLanguageBackground = languageOptionBackground != null;
        if (useCustomLanguageBackground || languageDropdownOptionHeight > 0f)
            ConfigureLanguageDropdownLayout();

        Image templateImage = languageDropdown.template.GetComponent<Image>();
        if (templateImage != null && useCustomLanguageBackground)
        {
            Color color = templateImage.color;
            color.a = 0f;
            templateImage.color = color;
        }

        Toggle itemToggle = languageDropdown.template.GetComponentInChildren<Toggle>(true);
        if (itemToggle != null && useCustomLanguageBackground)
            ApplyDropdownSprite(itemToggle.targetGraphic as Image, languageOptionBackground);
    }

    private void ConfigureLanguageDropdownLayout()
    {
        RectTransform template = languageDropdown.template;
        RectTransform dropdownRect = languageDropdown.GetComponent<RectTransform>();
        float optionHeight = GetDropdownHeight(dropdownRect);
        int optionCount = Mathf.Max(1, languageDropdown.options.Count);
        float templateHeight = optionHeight * optionCount;

        template.anchorMin = new Vector2(0f, 0f);
        template.anchorMax = new Vector2(1f, 0f);
        template.pivot = new Vector2(0.5f, 1f);
        template.anchoredPosition = Vector2.zero;
        template.sizeDelta = new Vector2(0f, templateHeight);

        ScrollRect scrollRect = template.GetComponent<ScrollRect>();
        if (scrollRect != null)
        {
            if (scrollRect.viewport != null)
            {
                scrollRect.viewport.anchorMin = Vector2.zero;
                scrollRect.viewport.anchorMax = Vector2.one;
                scrollRect.viewport.anchoredPosition = Vector2.zero;
                scrollRect.viewport.sizeDelta = Vector2.zero;
            }

            if (scrollRect.content != null)
            {
                scrollRect.content.anchorMin = new Vector2(0f, 1f);
                scrollRect.content.anchorMax = new Vector2(1f, 1f);
                scrollRect.content.pivot = new Vector2(0.5f, 1f);
                scrollRect.content.anchoredPosition = Vector2.zero;
                scrollRect.content.sizeDelta = new Vector2(0f, templateHeight);
            }
        }

        Toggle itemToggle = template.GetComponentInChildren<Toggle>(true);
        if (itemToggle != null)
        {
            RectTransform itemRect = itemToggle.GetComponent<RectTransform>();
            if (itemRect != null)
            {
                itemRect.anchorMin = new Vector2(0f, 1f);
                itemRect.anchorMax = new Vector2(1f, 1f);
                itemRect.pivot = new Vector2(0.5f, 1f);
                itemRect.anchoredPosition = Vector2.zero;
                itemRect.sizeDelta = new Vector2(0f, optionHeight);
            }
        }
    }

    private float GetDropdownHeight(RectTransform dropdownRect)
    {
        if (dropdownRect != null)
        {
            float rectHeight = dropdownRect.rect.height;
            if (rectHeight > 0f)
                return rectHeight;

            float sizeDeltaHeight = Mathf.Abs(dropdownRect.sizeDelta.y);
            if (sizeDeltaHeight > 0f)
                return sizeDeltaHeight;
        }

        return Mathf.Max(1f, languageDropdownOptionHeight);
    }

    private void EnsureJapaneseDropdownFallback()
    {
        TMP_FontAsset baseFont = GetBaseDropdownFont();
        TMP_FontAsset japaneseFont = GetJapaneseDropdownFont();
        if (baseFont == null || japaneseFont == null)
            return;

        if (baseFont.fallbackFontAssetTable == null)
            baseFont.fallbackFontAssetTable = new List<TMP_FontAsset>();

        if (!baseFont.fallbackFontAssetTable.Contains(japaneseFont))
            baseFont.fallbackFontAssetTable.Add(japaneseFont);

        if (japaneseFont.fallbackFontAssetTable == null)
            japaneseFont.fallbackFontAssetTable = new List<TMP_FontAsset>();

        if (!japaneseFont.fallbackFontAssetTable.Contains(baseFont))
            japaneseFont.fallbackFontAssetTable.Add(baseFont);
    }

    private TMP_FontAsset GetCaptionFont(int languageIndex)
    {
        return languageIndex == JapaneseLanguageIndex
            ? GetJapaneseDropdownFont()
            : GetBaseDropdownFont();
    }

    private TMP_FontAsset GetOptionFont(int optionIndex)
    {
        return optionIndex == JapaneseLanguageIndex
            ? GetJapaneseDropdownFont()
            : GetBaseDropdownFont();
    }

    private TMP_FontAsset GetBaseDropdownFont()
    {
        if (languageDropdownBaseFont != null)
            return languageDropdownBaseFont;

        return FontManager.Instance != null
            ? FontManager.Instance.GetFont(0)
            : null;
    }

    private TMP_FontAsset GetJapaneseDropdownFont()
    {
        if (languageDropdownJapaneseFont != null)
            return languageDropdownJapaneseFont;

        return FontManager.Instance != null
            ? FontManager.Instance.GetFont(JapaneseLanguageIndex)
            : null;
    }

    private static void ApplyTextFont(TMP_Text text, TMP_FontAsset font)
    {
        if (text == null || font == null || text.font == font)
            return;

        text.font = font;
    }

    private void RefreshLanguageDropdownFont(int languageIndex)
    {
        if (languageDropdown == null)
            return;

        ApplyTextFont(languageDropdown.captionText, GetCaptionFont(languageIndex));
        ApplyTextFont(languageDropdown.itemText, GetBaseDropdownFont());
        languageDropdown.RefreshShownValue();
        RefreshOpenDropdownItemFonts();
    }

    private static void ApplyDropdownSprite(Image image, Sprite sprite)
    {
        if (image == null || sprite == null)
            return;

        image.sprite = sprite;
        image.color = Color.white;
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
    }

    private void BindLanguageDropdownOpenEvents()
    {
        if (languageDropdownOpenEventsBound || languageDropdown == null)
            return;

        EventTrigger trigger = languageDropdown.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = languageDropdown.gameObject.AddComponent<EventTrigger>();

        AddLanguageDropdownRefreshEvent(trigger, EventTriggerType.PointerClick);
        AddLanguageDropdownRefreshEvent(trigger, EventTriggerType.Submit);
        languageDropdownOpenEventsBound = true;
    }

    private void AddLanguageDropdownRefreshEvent(EventTrigger trigger, EventTriggerType eventType)
    {
        if (trigger.triggers == null)
            trigger.triggers = new List<EventTrigger.Entry>();

        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = eventType
        };

        entry.callback.AddListener(_ => ScheduleOpenDropdownFontRefresh());
        trigger.triggers.Add(entry);
    }

    private void ScheduleOpenDropdownFontRefresh()
    {
        if (!isActiveAndEnabled)
            return;

        if (dropdownFontRefreshCoroutine != null)
            StopCoroutine(dropdownFontRefreshCoroutine);

        dropdownFontRefreshCoroutine = StartCoroutine(RefreshOpenDropdownItemFontsNextFrame());
    }

    private IEnumerator RefreshOpenDropdownItemFontsNextFrame()
    {
        yield return null;

        RefreshOpenDropdownItemFonts();
        dropdownFontRefreshCoroutine = null;
    }

    private void RefreshOpenDropdownItemFonts()
    {
        Transform dropdownList = FindOpenLanguageDropdownList();
        if (dropdownList == null)
            return;

        TMP_Text[] itemTexts = dropdownList.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < itemTexts.Length; i++)
        {
            int optionIndex = GetLanguageOptionIndex(itemTexts[i].text);
            if (optionIndex < 0)
                continue;

            ApplyTextFont(itemTexts[i], GetOptionFont(optionIndex));
        }
    }

    private Transform FindOpenLanguageDropdownList()
    {
        if (languageDropdown == null || languageDropdown.template == null)
            return null;

        Transform parent = languageDropdown.template.parent;
        if (parent == null)
            return null;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == "Dropdown List" && child.gameObject.activeInHierarchy)
                return child;
        }

        return null;
    }

    private int GetLanguageOptionIndex(string optionText)
    {
        switch (optionText)
        {
            case KoreanLanguageText:
                return 0;
            case EnglishLanguageText:
                return 1;
            case JapaneseLanguageText:
                return JapaneseLanguageIndex;
            default:
                return -1;
        }
    }

    private void OnEnable()
    {
        if (BGMManager.Instance != null)
            BGMManager.Instance.RegisterButtonSEPlayersInScene();

        LoadCurrentSettingValues();
        RefreshIngameButtonState();
        ResetConfirmPopup();
        PauseBattleIfNeeded();
    }

    private void OnDisable()
    {
        if (dropdownFontRefreshCoroutine != null)
        {
            StopCoroutine(dropdownFontRefreshCoroutine);
            dropdownFontRefreshCoroutine = null;
        }

        SaveAudioSettings();
        ResumeBattleIfNeeded();
    }

    private void OnDestroy()
    {
        UnbindUIEvents();
    }

    // 사용처: 설정 UI의 슬라이더/드롭다운 이벤트를 코드에서 보장한다.
    private void BindUIEvents()
    {
        if (masterSlider != null)
            masterSlider.onValueChanged.AddListener(UpdateMasterVolume);

        if (bgmSlider != null)
            bgmSlider.onValueChanged.AddListener(UpdateBgmVolume);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(UpdateSfxVolume);

        if (languageDropdown != null)
        {
            languageDropdown.onValueChanged.AddListener(UpdateLanguage);
            BindLanguageDropdownOpenEvents();
        }

        EnsureTutorialResetButton();
        if (tutorialResetButton != null)
        {
            tutorialResetButton.onClick.RemoveListener(OnClickResetTutorialProgress);
            tutorialResetButton.onClick.AddListener(OnClickResetTutorialProgress);
        }
    }

    // 사용처: 설정 UI가 제거될 때 동적으로 등록한 이벤트를 해제한다.
    private void UnbindUIEvents()
    {
        if (masterSlider != null)
            masterSlider.onValueChanged.RemoveListener(UpdateMasterVolume);

        if (bgmSlider != null)
            bgmSlider.onValueChanged.RemoveListener(UpdateBgmVolume);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(UpdateSfxVolume);

        if (languageDropdown != null)
            languageDropdown.onValueChanged.RemoveListener(UpdateLanguage);

        if (tutorialResetButton != null)
            tutorialResetButton.onClick.RemoveListener(OnClickResetTutorialProgress);
    }

    // 사용처: 설정창이 열릴 때 저장된 사운드/언어 값을 UI에 반영한다.
    private void LoadCurrentSettingValues()
    {
        BGMManager bgmManager = BGMManager.Instance;
        RogueLikeData data = RogueLikeData.Instance;

        float master = bgmManager != null ? bgmManager.MasterVolume : 1f;
        float bgm = bgmManager != null ? bgmManager.BgmVolume : 1f;
        float sfx = bgmManager != null ? bgmManager.SfxVolume : 1f;

        if (masterSlider != null)
            masterSlider.SetValueWithoutNotify(master);

        if (bgmSlider != null)
            bgmSlider.SetValueWithoutNotify(bgm);

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(sfx);

        if (languageDropdown != null && data != null)
        {
            languageDropdown.SetValueWithoutNotify(data.GetLanguage());
            RefreshLanguageDropdownFont(data.GetLanguage());
        }

        SyncAudioData(master, bgm, sfx);
    }

    // 사용처: 현재 사운드 설정을 저장한다.
    private void SaveAudioSettings()
    {
        if (BGMManager.Instance != null)
            BGMManager.Instance.SaveVolumeSettings();
    }

    // 사용처: BGMManager의 실제 사운드 설정과 RogueLikeData의 표시용 값을 맞춘다.
    private void SyncAudioData(float master, float bgm, float sfx)
    {
        RogueLikeData data = RogueLikeData.Instance;
        if (data == null)
            return;

        data.MasterVolume = Mathf.Clamp01(master);
        data.BgmVolume = Mathf.Clamp01(bgm);
        data.SfxVolume = Mathf.Clamp01(sfx);
    }

    // 사용처: 타이틀이 아닌 씬에서만 인게임 버튼 영역을 표시한다.
    private void RefreshIngameButtonState()
    {
        if (ingameButtonsArea == null)
            return;

        string sceneName = SceneManager.GetActiveScene().name;
        ingameButtonsArea.SetActive(sceneName != "Title");
    }

    // 사용처: 설정창을 다시 열 때 전투 포기 확인 팝업을 초기 상태로 닫는다.
    private void ResetConfirmPopup()
    {
        if (confirmGiveUpPopup != null)
            confirmGiveUpPopup.SetActive(false);
    }

    // 사용처: 설정창이 열릴 때 현재 로드된 전투 매니저가 있으면 전투를 일시정지한다.
    private void PauseBattleIfNeeded()
    {
        if (pausedBattleByThisUI)
            return;

        AutoBattleManager manager = FindObjectOfType<AutoBattleManager>();
        if (manager == null)
            return;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        manager.SetBattlePaused(true);

        pausedBattleManager = manager;
        pausedBattleByThisUI = true;
    }

    // 사용처: 설정창이 닫힐 때 이 설정창이 멈춘 전투만 다시 진행시킨다.
    private void ResumeBattleIfNeeded()
    {
        if (!pausedBattleByThisUI)
            return;

        if (pausedBattleManager != null)
        {
            pausedBattleManager.SetBattlePaused(false);
        }
        else
        {
            AutoBattleManager manager = FindObjectOfType<AutoBattleManager>();
            if (manager != null)
                manager.SetBattlePaused(false);
        }

        Time.timeScale = previousTimeScale;

        pausedBattleManager = null;
        pausedBattleByThisUI = false;
    }

    public void UpdateMasterVolume(float value)
    {
        float v = Mathf.Clamp01(value);

        if (BGMManager.Instance != null)
            BGMManager.Instance.SetMasterVolume(v);

        if (RogueLikeData.Instance != null)
            RogueLikeData.Instance.MasterVolume = v;
    }

    public void UpdateBgmVolume(float value)
    {
        float v = Mathf.Clamp01(value);

        if (BGMManager.Instance != null)
            BGMManager.Instance.SetBGMVolume(v);

        if (RogueLikeData.Instance != null)
            RogueLikeData.Instance.BgmVolume = v;
    }

    public void UpdateSfxVolume(float value)
    {
        float v = Mathf.Clamp01(value);

        if (BGMManager.Instance != null)
            BGMManager.Instance.SetSEVolume(v);

        if (RogueLikeData.Instance != null)
            RogueLikeData.Instance.SfxVolume = v;
    }

    public void UpdateLanguage(int index)
    {
        int selectedIndex = languageDropdown != null
            ? languageDropdown.value
            : Mathf.Clamp(index, 0, 2);

        Debug.Log(selectedIndex);

        RogueLikeData data = RogueLikeData.Instance;
        if (data != null && data.GetLanguage() == selectedIndex)
        {
            if (FontManager.Instance != null)
                FontManager.Instance.ApplyLanguageFont(selectedIndex);

            RefreshLanguageDropdownFont(selectedIndex);
            return;
        }

        if (data != null)
            data.SetLanguage(selectedIndex);

        if (FontManager.Instance != null)
            FontManager.Instance.ApplyLanguageFont(selectedIndex);

        RefreshLanguageDropdownFont(selectedIndex);
    }

    public void OnClickClose()
    {
        SaveAudioSettings();
        gameObject.SetActive(false);
    }

    public void OnClickOpenTutorialLibrary()
    {
        TutorialService service = GetTutorialService();
        if (service == null)
            return;

        service.OpenManualLibrary();
    }

    public void OnClickResetTutorialProgress()
    {
        TutorialService service = GetTutorialService();
        if (service == null)
            return;

        service.ResetTutorialProgressForDebug();
        Debug.Log("[SettingsUI] Tutorial progress has been reset.");
    }

    public void OnTutorialAutoPopupChanged(bool enabled)
    {
        TutorialService service = GetTutorialService();
        if (service == null)
            return;

        service.SetAutoPopupEnabled(enabled);
    }

    public void OnClickGiveUpRequest()
    {
        if (confirmGiveUpPopup != null)
            confirmGiveUpPopup.SetActive(true);
    }

    public void OnClickCancelGiveUp()
    {
        if (confirmGiveUpPopup != null)
            confirmGiveUpPopup.SetActive(false);
    }

    private TutorialService GetTutorialService()
    {
        TutorialService service = TutorialService.Instance;
        if (service != null)
            return service;

        service = FindObjectOfType<TutorialService>(true);
        if (service == null)
            Debug.LogWarning("[SettingsUI] TutorialService를 찾을 수 없습니다.");

        return service;
    }

    private void EnsureTutorialResetButton()
    {
        if (tutorialResetButton != null)
            return;

        Transform resetTransform = FindDeepChild(transform, "TutorialReset");
        if (resetTransform == null)
            return;

        tutorialResetButton = resetTransform.GetComponent<Button>()
            ?? resetTransform.GetComponentInChildren<Button>(true);
    }

    private static Transform FindDeepChild(Transform root, string targetName)
    {
        if (root == null)
            return null;

        if (root.name == targetName)
            return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindDeepChild(root.GetChild(i), targetName);
            if (found != null)
                return found;
        }

        return null;
    }
}
