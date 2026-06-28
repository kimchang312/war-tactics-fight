using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionBtn : MonoBehaviour
{
    [SerializeField] private Button optionBtn;
    [SerializeField] private Button resumeGame;
    [SerializeField] private Button goTitle;
    [SerializeField] private GameObject optionWindow;
    [SerializeField] private Toggle gameSpeedToggle;
    [SerializeField] private Button goTest;

    private bool isPaused;
    private float previousTimeScale = 1f;
    private AutoBattleManager pausedBattleManager;

    private const float NormalSpeed = 1f;
    private const float FastSpeed = 0.5f;

    private void Start()
    {
        if (optionWindow != null)
            optionWindow.SetActive(false);

        if (optionBtn != null)
            optionBtn.onClick.AddListener(ToggleOptionWindow);

        if (resumeGame != null)
            resumeGame.onClick.AddListener(ResumeGame);

        if (goTitle != null)
            goTitle.onClick.AddListener(Movetitle);

        if (goTest != null)
            goTest.onClick.AddListener(GoTestMode);


#if !UNITY_EDITOR
    gameSpeedToggle.gameObject.SetActive(false);
#else
        gameSpeedToggle.gameObject.SetActive(true);
#endif

        if (gameSpeedToggle != null)
        {
            gameSpeedToggle.onValueChanged.RemoveListener(OnToggleChanged);
            gameSpeedToggle.onValueChanged.AddListener(OnToggleChanged);

            OnToggleChanged(gameSpeedToggle.isOn);
        }
    }

    private void OnDestroy()
    {
        if (optionBtn != null)
            optionBtn.onClick.RemoveListener(ToggleOptionWindow);

        if (resumeGame != null)
            resumeGame.onClick.RemoveListener(ResumeGame);

        if (goTitle != null)
            goTitle.onClick.RemoveListener(Movetitle);

        if (goTest != null)
            goTest.onClick.RemoveListener(GoTestMode);

        if (gameSpeedToggle != null)
            gameSpeedToggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            ToggleOptionWindow();
    }

    private void ToggleOptionWindow()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    // 사용처: AutoBattleScene 안의 옵션창을 열 때 전투 진행을 멈춘다.
    private void PauseGame()
    {
        if (optionWindow != null)
        {
            optionWindow.transform.SetAsLastSibling();
            optionWindow.SetActive(true);
        }

        previousTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0f;

        pausedBattleManager = FindObjectOfType<AutoBattleManager>();
        if (pausedBattleManager != null)
            pausedBattleManager.SetBattlePaused(true);

        isPaused = true;
    }

    // 사용처: AutoBattleScene 안의 옵션창을 닫을 때 전투 진행을 재개한다.
    private void ResumeGame()
    {
        if (optionWindow != null)
            optionWindow.SetActive(false);

        if (pausedBattleManager != null)
            pausedBattleManager.SetBattlePaused(false);

        Time.timeScale = previousTimeScale > 0f ? previousTimeScale : 1f;

        pausedBattleManager = null;
        isPaused = false;
    }

    // 사용처: 테스트 씬 이동 버튼 클릭 시 테스트 씬으로 이동한다.
    private void GoTestMode()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Test");
    }

    // 사용처: 전투 포기 후 타이틀로 이동한다.
    private void Movetitle()
    {
        Time.timeScale = 1f;

        SaveData saveData = new SaveData();
        saveData.ResetGameData();

        GameManager.Instance.SetCurrentStageNull();
        RogueLikeData.Instance.SetResetMap(true);

        SceneManager.LoadScene("Title");
    }

    // 사용처: AutoBattleScene의 배속 토글 값을 전역 배속 상태에 저장한다.
    private void OnToggleChanged(bool isOn)
    {
        GameSpeedManager.Instance.GameSpeed = isOn ? FastSpeed : NormalSpeed;
    }
}