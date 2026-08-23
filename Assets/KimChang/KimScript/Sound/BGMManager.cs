using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class BGMEntry
{
    public string key;
    public AudioClip clip;
}

public enum SEGroup
{
    UI,
    System,
    Battle,
    Map,
    Event,
    Store,
    Treasure,
    Rest
}

[System.Serializable]
public class SEEntry
{
    public string key;
    public SEGroup group;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
}

public class BGMManager : MonoBehaviour
{
    public const string SeButtonClick = "se_btn_click";
    public const string SeTreasure = "se_Treasure";

    private const string MasterVolumePrefKey = "RL_MasterVolume";
    private const string BgmVolumePrefKey = "RL_BgmVolume";
    private const string SfxVolumePrefKey = "RL_SfxVolume";

    private const string TreasureButtonObjectName = "TreasureBox";
    private const string LegacyTreasureButtonObjectName = "TeasureBox";

    public static BGMManager Instance;

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private List<BGMEntry> bgmList = new List<BGMEntry>();

    [Header("SE Audio Sources")]
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource systemSource;
    [SerializeField] private AudioSource battleSource;
    [SerializeField] private AudioSource worldSource;

    [Header("SE List")]
    [SerializeField] private List<SEEntry> seList = new List<SEEntry>();

    [Header("Volume")]
    [Range(0f, 1f)][SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float bgmVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;

    private readonly Dictionary<string, AudioClip> bgmClips = new Dictionary<string, AudioClip>(16);
    private readonly Dictionary<string, SEEntry> seClips = new Dictionary<string, SEEntry>(64);

    public float MasterVolume => masterVolume;
    public float BgmVolume => bgmVolume;
    public float SfxVolume => sfxVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAudioSources();
        InitializeBGMClips();
        InitializeSEClips();
        LoadVolumeSettings();
        ApplyVolume();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    // 사용처: Inspector에 AudioSource가 누락됐을 때 현재 오브젝트 기준으로 기본 AudioSource를 보정
    private void InitializeAudioSources()
    {
        if (bgmSource == null)
            bgmSource = gameObject.AddComponent<AudioSource>();

        if (uiSource == null)
            uiSource = gameObject.AddComponent<AudioSource>();

        if (systemSource == null)
            systemSource = gameObject.AddComponent<AudioSource>();

        if (battleSource == null)
            battleSource = gameObject.AddComponent<AudioSource>();

        if (worldSource == null)
            worldSource = gameObject.AddComponent<AudioSource>();

        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

        uiSource.loop = false;
        systemSource.loop = false;
        battleSource.loop = false;
        worldSource.loop = false;

        uiSource.playOnAwake = false;
        systemSource.playOnAwake = false;
        battleSource.playOnAwake = false;
        worldSource.playOnAwake = false;
    }

    // 사용처: Inspector에 등록된 BGM 목록을 빠르게 찾을 수 있도록 Dictionary로 캐싱
    private void InitializeBGMClips()
    {
        bgmClips.Clear();

        for (int i = 0; i < bgmList.Count; i++)
        {
            BGMEntry entry = bgmList[i];
            if (entry == null)
                continue;

            if (string.IsNullOrWhiteSpace(entry.key))
                continue;

            if (entry.clip == null)
                continue;

            if (bgmClips.ContainsKey(entry.key))
            {
                Debug.LogWarning($"[BGMManager] 중복 BGM key: {entry.key}");
                continue;
            }

            bgmClips.Add(entry.key, entry.clip);
        }
    }

    // 사용처: Inspector에 등록된 효과음 목록을 빠르게 찾을 수 있도록 Dictionary로 캐싱
    private void InitializeSEClips()
    {
        seClips.Clear();

        for (int i = 0; i < seList.Count; i++)
        {
            SEEntry entry = seList[i];
            if (entry == null)
                continue;

            if (string.IsNullOrWhiteSpace(entry.key))
                continue;

            if (entry.clip == null)
                continue;

            if (seClips.ContainsKey(entry.key))
            {
                Debug.LogWarning($"[BGMManager] 중복 SE key: {entry.key}");
                continue;
            }

            seClips.Add(entry.key, entry);
        }
    }

    // 사용처: 저장된 볼륨 설정을 불러와 AudioSource 적용 전 내부 값에 반영
    private void LoadVolumeSettings()
    {
        masterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumePrefKey, masterVolume));
        bgmVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(BgmVolumePrefKey, bgmVolume));
        sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumePrefKey, sfxVolume));
    }

    // 사용처: 설정 UI를 닫을 때 현재 사운드 설정을 저장
    public void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat(MasterVolumePrefKey, masterVolume);
        PlayerPrefs.SetFloat(BgmVolumePrefKey, bgmVolume);
        PlayerPrefs.SetFloat(SfxVolumePrefKey, sfxVolume);
        PlayerPrefs.Save();
    }

    // 사용처: 씬 진입 시 현재 씬에 맞는 BGM 재생 및 버튼 클릭음 자동 등록
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 전투 씬을 벗어날 때 PlayOneShot으로 남아 있는 타격/승패 효과음을 정리한다.
        if (scene.name != "AutoBattleScene" && battleSource != null)
            battleSource.Stop();

        switch (scene.name)
        {
            case "Title":
                PlayBGM("MainMenu");
                break;

            case "RLmap":
                PlayBGM("MapExplore");
                break;

            case "AutoBattleScene":
                PlayBattleSceneBGM();
                break;

            default:
                Debug.Log($"[BGMManager] No BGM assigned for scene: {scene.name}");
                break;
        }

        StartCoroutine(RegisterButtonSEPlayersNextFrame());
    }

    // 사용처: 전투 씬 진입 시 현재 스테이지 타입에 맞는 BGM을 선택
    private void PlayBattleSceneBGM()
    {
        if (RogueLikeData.Instance == null)
        {
            Debug.LogWarning("[BGMManager] RogueLikeData.Instance가 null입니다. 기본 전투 BGM 재생");
            PlayBGM("BattleNormal");
            return;
        }

        StageType stageType = RogueLikeData.Instance.GetCurrentStageType();
        PlayBGM(stageType == StageType.Boss ? "BattleBoss" : "BattleNormal");
    }

    // 사용처: key에 해당하는 BGM을 재생
    public void PlayBGM(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        if (bgmSource == null)
            return;

        if (!bgmClips.TryGetValue(key, out AudioClip clip))
        {
            Debug.LogWarning($"[BGMManager] No BGM found for key: {key}");
            return;
        }

        float effectiveVolume = GetEffectiveBgmVolume();

        if (bgmSource.clip == clip)
        {
            bgmSource.volume = effectiveVolume;

            if (!bgmSource.isPlaying)
                bgmSource.Play();

            return;
        }

        bgmSource.clip = clip;
        bgmSource.volume = effectiveVolume;
        bgmSource.Play();
    }

    // 사용처: 현재 재생 중인 BGM을 정지
    public void StopBGM()
    {
        if (bgmSource == null)
            return;

        bgmSource.Stop();
        bgmSource.clip = null;
    }

    // 사용처: 공용 버튼 클릭음을 재생
    public void PlayButtonClick()
    {
        PlaySE(SeButtonClick);
    }

    // 사용처: 보물상자 버튼 클릭음을 재생
    public void PlayTreasureClick()
    {
        PlaySE(SeTreasure);
    }

    // 사용처: key에 해당하는 효과음을 분류별 AudioSource로 1회 재생
    public void PlaySE(string key)
    {
        PlaySE(key, 1f);
    }

    // 사용처: 전투 타격음처럼 같은 효과음을 다른 볼륨 배율로 재생
    public void PlaySE(string key, float volumeScale)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        if (!seClips.TryGetValue(key, out SEEntry entry))
        {
            Debug.LogWarning($"[BGMManager] No SE found for key: {key}");
            return;
        }

        AudioSource source = GetSESource(entry.group);
        if (source == null || entry.clip == null)
            return;

        source.volume = GetEffectiveSfxVolume();
        source.PlayOneShot(entry.clip, Mathf.Clamp01(entry.volume * volumeScale));
    }

    // 사용처: 효과음 분류에 맞는 AudioSource를 반환
    private AudioSource GetSESource(SEGroup group)
    {
        switch (group)
        {
            case SEGroup.UI:
                return uiSource;

            case SEGroup.System:
                return systemSource;

            case SEGroup.Battle:
                return battleSource;

            case SEGroup.Map:
            case SEGroup.Event:
            case SEGroup.Store:
            case SEGroup.Treasure:
            case SEGroup.Rest:
                return worldSource;

            default:
                return uiSource;
        }
    }

    // 사용처: 설정 UI에서 마스터/BGM/효과음 볼륨을 한 번에 반영
    public void SetVolumes(float master, float bgm, float sfx)
    {
        masterVolume = Mathf.Clamp01(master);
        bgmVolume = Mathf.Clamp01(bgm);
        sfxVolume = Mathf.Clamp01(sfx);

        ApplyVolume();
    }

    // 사용처: 마스터 볼륨만 변경
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyVolume();
    }

    // 사용처: BGM 전체 볼륨만 변경
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        ApplyVolume();
    }

    // 사용처: 효과음 전체 볼륨만 변경
    public void SetSEVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplyVolume();
    }

    // 사용처: 기존 코드에서 SFX 명칭으로 호출할 경우를 위한 호환용 함수
    public void SetSFXVolume(float volume)
    {
        SetSEVolume(volume);
    }

    // 사용처: 마스터 볼륨과 BGM 볼륨을 합산한 실제 BGM 볼륨을 반환
    private float GetEffectiveBgmVolume()
    {
        return masterVolume * bgmVolume;
    }

    // 사용처: 마스터 볼륨과 효과음 볼륨을 합산한 실제 효과음 볼륨을 반환
    private float GetEffectiveSfxVolume()
    {
        return masterVolume * sfxVolume;
    }

    // 사용처: 현재 설정된 마스터/BGM/효과음 볼륨을 실제 AudioSource에 적용
    private void ApplyVolume()
    {
        if (bgmSource != null)
            bgmSource.volume = GetEffectiveBgmVolume();

        float effectiveSfxVolume = GetEffectiveSfxVolume();

        if (uiSource != null)
            uiSource.volume = effectiveSfxVolume;

        if (systemSource != null)
            systemSource.volume = effectiveSfxVolume;

        if (battleSource != null)
            battleSource.volume = effectiveSfxVolume;

        if (worldSource != null)
            worldSource.volume = effectiveSfxVolume;
    }

    // 사용처: 씬 로드 후 현재 씬의 버튼에 클릭 효과음을 자동 등록
    private IEnumerator RegisterButtonSEPlayersNextFrame()
    {
        yield return null;
        RegisterButtonSEPlayersInScene();
    }

    // 사용처: 현재 씬에 존재하는 모든 Button에 ButtonSEPlayer를 자동 부착
    public void RegisterButtonSEPlayersInScene()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null)
                continue;

            ButtonSEPlayer sePlayer = button.GetComponent<ButtonSEPlayer>();
            if (sePlayer == null)
                sePlayer = button.gameObject.AddComponent<ButtonSEPlayer>();

            sePlayer.SetSEKey(GetButtonSEKey(button));
        }
    }

    // 사용처: 버튼 오브젝트 이름에 따라 기본 클릭음 또는 보물상자 클릭음을 선택
    private string GetButtonSEKey(Button button)
    {
        if (button == null)
            return SeButtonClick;

        string objectName = button.gameObject.name;
        if (objectName == TreasureButtonObjectName || objectName == LegacyTreasureButtonObjectName)
            return SeTreasure;

        return SeButtonClick;
    }
}
