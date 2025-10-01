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

    [SerializeField] private AutoBattleManager autoBattleManager;
    [SerializeField] private AutoBattleUI autoBattleUI;
    [SerializeField] private MoveDamageUI moveDamageUI;
    [SerializeField] private MoveAbilityUI moveAbilityUI;
    [SerializeField] private Button goTest;
    private bool isPaused=false;

    private float animationSpeed = 1f;

    void Start()
    {
        optionWindow.SetActive(false);
        optionBtn.onClick.AddListener(ToggleOptionWindow);
        resumeGame.onClick.AddListener(ResumeGame);
        goTitle.onClick.AddListener(Movetitle);

        gameSpeedToggle.onValueChanged.AddListener(OnToggleChanged);
        goTest.onClick.AddListener(GoTestMode);
    }

    private void OnDestroy()
    {
        gameSpeedToggle.onValueChanged.RemoveListener(OnToggleChanged);
    }
    private void GoTestMode()
    {
        SceneManager.LoadScene("Test");
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleOptionWindow();
        }
    }
    private void ToggleOptionWindow()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }


    // 게임을 멈추는 함수
    private void PauseGame()
    {
        optionWindow.transform.SetAsLastSibling();

        optionWindow.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }


    // 게임을 재개하는 함수
    private void ResumeGame()
    {
        optionWindow.SetActive(false); // 옵션 창 비활성화
        Time.timeScale = 1f;          // 게임 재개
        isPaused = false;             // 멈춤 상태 해제
    }

    //타이틀로 가는 함수
    private void Movetitle()
    {
        if (isPaused)
        {
            SaveData saveData = new SaveData();
            Time.timeScale = 1f; // 게임 속도 초기화
            saveData.ResetGameData();
            GameManager.Instance.SetCurrentStageNull();
            RogueLikeData.Instance.SetResetMap(true);
            SceneManager.LoadScene("Title");
        }
    }

    // 토글 상태 변경 시 호출될 메서드
    private void OnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            animationSpeed = 0.5f;

            ManageTimeSpeed();
        }
        else
        {
            animationSpeed = 2f;
            ManageTimeSpeed();
        }
    }

    private void ManageTimeSpeed()
    {
        autoBattleManager.ChangeWaittingTime(animationSpeed);
        autoBattleUI.ChangeWaittingTime(animationSpeed);
        moveDamageUI.ChangeWaittingTime(animationSpeed);
        moveAbilityUI.ChangeWaittingTime(animationSpeed);
    }
}
