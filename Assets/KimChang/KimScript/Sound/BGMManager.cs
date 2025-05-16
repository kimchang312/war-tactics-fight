using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class BGMEntry
{
    public string key;         // 예: "MainMenu", "BattleBoss" 등
    public AudioClip clip;     // 실제 오디오 클립
}

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    public AudioSource audioSource;        // Inspector에서 AudioSource 연결
    public List<BGMEntry> bgmList;         // Inspector에서 곡 정보 등록

    private Dictionary<string, AudioClip> bgmClips = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var entry in bgmList)
        {
            if (!bgmClips.ContainsKey(entry.key) && entry.clip != null)
            {
                bgmClips[entry.key] = entry.clip;
            }
        }
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
        // 게임 시작 시 예외 상황 대비: 현재 씬 기준으로 재생
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "MainMenuScene":
                PlayBGM("MainMenu");
                break;
            case "RLmap":
                PlayBGM("MapExplore");
                break;
            case "AutoBattleScene":
                {
                    if (RogueLikeData.Instance != null)
                    {
                        var stageType = RogueLikeData.Instance.GetCurrentStageType();
                        if (stageType == StageType.Boss)
                            PlayBGM("BattleBoss");
                        else
                            PlayBGM("BattleNormal");
                    }
                    else
                    {
                        Debug.LogWarning("[BGMManager] RogueLikeData.Instance가 null입니다. 기본 BGM 재생");
                        PlayBGM("BattleNormal");
                    }
                    break;
                }
            default:
                Debug.Log($"[BGMManager] No BGM assigned for scene: {scene.name}");
                break;
        }
    }

    public void PlayBGM(string key)
    {
        if (bgmClips.TryGetValue(key, out var clip))
        {
            if (audioSource.clip == clip)
                return; // 이미 재생 중인 곡이면 무시

            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning($"[BGMManager] No BGM found for key: {key}");
        }
    }
}
