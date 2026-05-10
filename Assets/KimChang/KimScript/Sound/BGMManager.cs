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

    private const string TreasureButtonObjectName = "TeasureBox";

    public static BGMManager Instance;

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private List<BGMEntry> bgmList = new();

    [Header("SE Audio Sources")]
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource systemSource;
    [SerializeField] private AudioSource battleSource;
    [SerializeField] private AudioSource worldSource;

    [Header("SE List")]
    [SerializeField] private List<SEEntry> seList = new();

    [Header("Volume")]
    [Range(0f, 1f)][SerializeField] private float bgmVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float seVolume = 1f;

    private readonly Dictionary<string, AudioClip> bgmClips = new();
    private readonly Dictionary<string, SEEntry> seClips = new();

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

        uiSource.loop = false;
        systemSource.loop = false;
        battleSource.loop = false;
        worldSource.loop = false;

        uiSource.playOnAwake = false;
        systemSource.playOnAwake = false;
        battleSource.playOnAwake = false;
        worldSource.playOnAwake = false;
        bgmSource.playOnAwake = false;
    }

    // 사용처: Inspector에 등록된 BGM 목록을 빠르게 찾을 수 있도록 Dictionary로 캐싱
    private void InitializeBGMClips()
    {
        bgmClips.Clear();

        foreach (var entry in bgmList)
        {
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

        foreach (var entry in seList)
        {
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

    // 사용처: 씬 진입 시 현재 씬에 맞는 BGM 재생 및 버튼 클릭음 자동 등록
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "Title":
                PlayBGM("MainMenu");
                break;

            case "RLmap":
                PlayBGM("MapExplore");
                break;

            case "AutoBattleScene":
                {
                    if (RogueLikeData.Instance != null)
                    {
                        StageType stageType = RogueLikeData.Instance.GetCurrentStageType();

                        if (stageType == StageType.Boss)
                            PlayBGM("BattleBoss");
                        else
                            PlayBGM("BattleNormal");
                    }
                    else
                    {
                        Debug.LogWarning("[BGMManager] RogueLikeData.Instance가 null입니다. 기본 전투 BGM 재생");
                        PlayBGM("BattleNormal");
                    }

                    break;
                }

            default:
                Debug.Log($"[BGMManager] No BGM assigned for scene: {scene.name}");
                break;
        }

        StartCoroutine(RegisterButtonSEPlayersNextFrame());
    }

    // 사용처: key에 해당하는 BGM을 재생
    public void PlayBGM(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        if (!bgmClips.TryGetValue(key, out AudioClip clip))
        {
            Debug.LogWarning($"[BGMManager] No BGM found for key: {key}");
            return;
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        bgmSource.clip = clip;
        bgmSource.volume = bgmVolume;
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

        source.PlayOneShot(entry.clip, entry.volume * seVolume);
    }

    // 사용처: 전투 타격음처럼 같은 효과음을 다른 볼륨으로 재생
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

        source.PlayOneShot(entry.clip, entry.volume * seVolume * volumeScale);
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

    // 사용처: BGM 전체 볼륨을 변경
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);

        if (bgmSource != null)
            bgmSource.volume = bgmVolume;
    }

    // 사용처: 효과음 전체 볼륨을 변경
    public void SetSEVolume(float volume)
    {
        seVolume = Mathf.Clamp01(volume);
    }

    // 사용처: 현재 설정된 BGM/SE 볼륨을 AudioSource에 적용
    private void ApplyVolume()
    {
        if (bgmSource != null)
            bgmSource.volume = bgmVolume;
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

        int addedCount = 0;

        foreach (Button button in buttons)
        {
            if (button == null)
                continue;

            ButtonSEPlayer sePlayer = button.GetComponent<ButtonSEPlayer>();

            if (sePlayer == null)
            {
                sePlayer = button.gameObject.AddComponent<ButtonSEPlayer>();
                addedCount++;
            }

            sePlayer.SetSEKey(GetButtonSEKey(button));
        }
    }

    // 사용처: 버튼 오브젝트 이름에 따라 기본 클릭음 또는 보물상자 클릭음을 선택
    private string GetButtonSEKey(Button button)
    {
        if (button != null && button.gameObject.name == TreasureButtonObjectName)
            return SeTreasure;

        return SeButtonClick;
    }


}