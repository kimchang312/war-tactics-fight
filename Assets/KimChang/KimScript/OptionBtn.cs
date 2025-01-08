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

    public PlayerData playerData;

    private bool isPaused=false;

    private float animationSpeed = 1f;

    void Start()
    {
        optionWindow.SetActive(false);
        optionBtn.onClick.AddListener(ToggleOptionWindow);
        resumeGame.onClick.AddListener(ResumeGame);
        goTitle.onClick.AddListener(Movetitle);

        gameSpeedToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnDestroy()
    {
        // 이벤트 등록 해제 (메모리 누수 방지)
        gameSpeedToggle.onValueChanged.RemoveListener(OnToggleChanged);


        // PlayerData 싱글톤 인스턴스를 연결
        playerData = PlayerData.Instance;

    }


    // 옵션 창을 토글하는 함수
    private void ToggleOptionWindow()
    {
        if (isPaused)
        {
            ResumeGame(); // 게임 재개
        }
        else
        {
            PauseGame(); // 게임 멈춤
        }
    }


    // 게임을 멈추는 함수
    private void PauseGame()
    {
        Debug.Log("멈춰");

        int siblingCount = optionWindow.transform.parent.childCount; // 형제 개수 확인
        optionWindow.transform.SetSiblingIndex(siblingCount - 1);

        optionWindow.SetActive(true); // 옵션 창 활성화
        Time.timeScale = 0f;         // 게임 멈춤
        isPaused = true;             // 멈춤 상태 설정
    }


    // 게임을 재개하는 함수
    private void ResumeGame()
    {
        Debug.Log("재시작");

        optionWindow.SetActive(false); // 옵션 창 비활성화
        Time.timeScale = 1f;          // 게임 재개
        isPaused = false;             // 멈춤 상태 해제
    }

    //타이틀로 가는 함수
    private void Movetitle()
    {
        if (isPaused)
        {

            Time.timeScale = 1f; // 게임 속도 초기화
            playerData.ResetPlayerData();
            SceneManager.LoadScene("Main");
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
            animationSpeed = 1f;

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
