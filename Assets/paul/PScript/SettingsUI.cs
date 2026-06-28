using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SettingsUI : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Language Settings")]
    [SerializeField] private TMP_Dropdown languageDropdown;

    [Header("Ingame Buttons Group")]
    [SerializeField] private GameObject ingameButtonsArea;

    [Header("Confirmation Popup")]
    [SerializeField] private GameObject confirmGiveUpPopup;

    private bool pausedBattleByThisUI;
    private float previousTimeScale = 1f;
    private AutoBattleManager pausedBattleManager;

    private void Awake()
    {
        NormalizeLanguageDropdownOptions();
        BindUIEvents();
    }

    private void NormalizeLanguageDropdownOptions()
    {
        if (languageDropdown == null)
            return;

        languageDropdown.options.Clear();
        languageDropdown.options.Add(new TMP_Dropdown.OptionData("한국어"));
        languageDropdown.options.Add(new TMP_Dropdown.OptionData("English"));
        languageDropdown.options.Add(new TMP_Dropdown.OptionData("日本語"));
        languageDropdown.RefreshShownValue();
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
            languageDropdown.onValueChanged.AddListener(UpdateLanguage);
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
            languageDropdown.SetValueWithoutNotify(data.GetLanguage());

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
            return;

        if (data != null)
            data.SetLanguage(selectedIndex);

        if (FontManager.Instance != null)
            FontManager.Instance.ApplyLanguageFont(selectedIndex);
    }

    public void OnClickClose()
    {
        SaveAudioSettings();
        gameObject.SetActive(false);
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
}
