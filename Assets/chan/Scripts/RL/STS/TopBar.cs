using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TopBar : MonoBehaviour
{
    [Header("Relics Panel Toggle")]
    [SerializeField] private GameObject ownedRelicsPanel;
    [SerializeField] private Button relicsToggleButton;
    [SerializeField] private Button closeRelicsButton;

    [Header("Upgrade Panel Toggle")]
    [SerializeField] private Button upgradeToggleButton;
    [SerializeField] private GameObject upgradePanel;

    [Header("Upgrade Status Panel Toggle")]
    [SerializeField] private GameObject upgradeStatusPanel;
    [SerializeField] private Button upgradeStatusButton;
    [SerializeField] private Button closeupgradeStatusButton;

    [Header("Academy Panel Toggle")]
    [SerializeField] private GameObject academyPanel;
    [SerializeField] private Button academyToggleButton;

    [Header("Option Panel Toggle")]
    [SerializeField] private GameObject optionPanel;
    [SerializeField] private Button optionButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button saveAndGoTitleButton;
    private void Awake()
    {
        optionPanel?.SetActive(false);
        // 초기에는 다 꺼두고
        ownedRelicsPanel?.SetActive(false);
        upgradePanel?.SetActive(false);
        upgradeStatusPanel?.SetActive(false);
        academyPanel?.SetActive(false);

        relicsToggleButton?.onClick.AddListener(() => ToggleOnly(ownedRelicsPanel));
        upgradeToggleButton?.onClick.AddListener(() => {
            ToggleOnly(upgradePanel);

            if (upgradePanel.activeSelf && GameManager.Instance.shouldRefreshUpgradeUI)
            {
                var upgradeUI = upgradePanel.GetComponent<UpgradeUI>();
                if (upgradeUI != null)
                {
                    upgradeUI.ShowRandomChoices();
                    GameManager.Instance.shouldRefreshUpgradeUI = false;
                }
            }
        });
        upgradeStatusButton?.onClick.AddListener(() => ToggleOnly(upgradeStatusPanel));
        academyToggleButton?.onClick.AddListener(() => ToggleOnly(academyPanel));
        // 닫기 버튼에도 같은 토글 메서드 연결
        closeRelicsButton.onClick.AddListener(() => ToggleOnly(ownedRelicsPanel));
        closeupgradeStatusButton.onClick.AddListener(() => ToggleOnly(upgradeStatusPanel));

        // 옵션 관련 버튼 연결
        optionButton?.onClick.AddListener(() => ToggleOptionPanel(true));
        continueButton?.onClick.AddListener(() => ToggleOptionPanel(false));
        saveAndGoTitleButton?.onClick.AddListener(SaveAndGoTitle);
    }

    private void Start()
    {
        // Title 씬일 때 TopBarPanel 비활성화
        CheckAndDisableInTitleScene();
        
        // 씬 전환 시에도 확인하도록 이벤트 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // 이벤트 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckAndDisableInTitleScene();
    }

    private void CheckAndDisableInTitleScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == "Title")
        {
            gameObject.SetActive(false);
            Debug.Log("[TopBar] Title 씬에서 TopBarPanel 비활성화");
        }
        else
        {
            // 다른 씬에서는 활성화 (이미 활성화되어 있을 수도 있음)
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }
    }
    
    private void ToggleOnly(GameObject panel)
    {
        bool wasActive = panel.activeSelf;

        // 모든 패널 끄기
        ownedRelicsPanel?.SetActive(false);
        upgradePanel?.SetActive(false);
        upgradeStatusPanel?.SetActive(false);
        academyPanel?.SetActive(false);

        // 클릭 직전 꺼져 있었다면 켜고, 켜져 있었다면 그대로 꺼두기
        panel.SetActive(!wasActive);
    }
    private void ToggleOptionPanel(bool show)
    {
        optionPanel?.SetActive(show);
    }

    private void SaveAndGoTitle()
    {
   
        Debug.Log("💾 게임 저장 중...");
        //저장하는 함수
        // ✅ 옵션 패널 끄기
        optionPanel?.SetActive(false);
        Debug.Log("🏁 타이틀 씬으로 이동 중...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title"); // 씬 이름이 정확해야 함
    }
}
