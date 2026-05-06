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
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Awake()
    {
        optionPanel?.SetActive(false);
        // 초기에는 다 꺼두고
        ownedRelicsPanel?.SetActive(false);
        upgradePanel?.SetActive(false);
        upgradeStatusPanel?.SetActive(false);
        academyPanel?.SetActive(false);

        // TopBar 크기 변경/마스크 영향으로 패널 상단이 잘리는 문제 방지:
        // 토글 패널들은 Canvas 루트(TopBar의 부모)로 올려서 TopBar 레이아웃에 종속되지 않게 한다.
        var canvasRoot = transform.parent;
        MovePanelToRootIfNeeded(ownedRelicsPanel, canvasRoot);
        MovePanelToRootIfNeeded(upgradeStatusPanel, canvasRoot);
        MovePanelToRootIfNeeded(academyPanel, canvasRoot);
        EnsureChildPanel(upgradeStatusPanel, upgradePanel);

        relicsToggleButton?.onClick.AddListener(() => ToggleOnly(ownedRelicsPanel));
        upgradeToggleButton?.onClick.AddListener(() => {
            ToggleUpgradePanelsTogether();
            TryRefreshUpgradeChoices();
        });
        upgradeStatusButton?.onClick.AddListener(() =>
        {
            ToggleUpgradePanelsTogether();
            TryRefreshUpgradeChoices();
        });
        academyToggleButton?.onClick.AddListener(() => ToggleOnly(academyPanel));
        // 닫기 버튼에도 같은 토글 메서드 연결
        closeRelicsButton.onClick.AddListener(() => ToggleOnly(ownedRelicsPanel));
        closeupgradeStatusButton.onClick.AddListener(CloseUpgradePanelsTogether);

        // 옵션 관련 버튼 연결
        optionButton?.onClick.AddListener(() => ToggleOptionPanel(true));
        continueButton?.onClick.AddListener(() => ToggleOptionPanel(false));
        saveAndGoTitleButton?.onClick.AddListener(SaveAndGoTitle);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // RLmap 외 씬에서는 상단바 관련 토글 패널이 남지 않도록 정리
        if (scene.name != "RLmap")
        {
            CloseAllTopPanels();
        }
    }

    private static void MovePanelToRootIfNeeded(GameObject panel, Transform root)
    {
        if (panel == null || root == null) return;
        var t = panel.transform;
        if (t.parent == root) return;

        // 원래 위치 유지 (UI라서 local 기준 유지가 더 안전)
        Vector3 localPos = t.localPosition;
        Quaternion localRot = t.localRotation;
        Vector3 localScale = t.localScale;

        t.SetParent(root, worldPositionStays: false);
        t.localPosition = localPos;
        t.localRotation = localRot;
        t.localScale = localScale;
    }

    private static void EnsureChildPanel(GameObject parentPanel, GameObject childPanel)
    {
        if (parentPanel == null || childPanel == null) return;
        if (childPanel.transform.parent == parentPanel.transform) return;

        childPanel.transform.SetParent(parentPanel.transform, worldPositionStays: false);
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

        // 켜는 경우 항상 최상단으로
        if (panel.activeSelf)
            panel.transform.SetAsLastSibling();
    }

    private void ToggleUpgradePanelsTogether()
    {
        bool wereBothActive = (upgradeStatusPanel != null && upgradeStatusPanel.activeSelf)
                             && (upgradePanel != null && upgradePanel.activeSelf);

        // 먼저 모든 패널을 닫고
        ownedRelicsPanel?.SetActive(false);
        upgradePanel?.SetActive(false);
        upgradeStatusPanel?.SetActive(false);
        academyPanel?.SetActive(false);

        // 둘 다 열려 있었다면 닫기, 아니면 둘 다 열기
        if (wereBothActive) return;

        upgradeStatusPanel?.SetActive(true);
        upgradePanel?.SetActive(true);
        upgradeStatusPanel?.transform.SetAsLastSibling();
        upgradePanel?.transform.SetAsLastSibling();
    }

    private void CloseUpgradePanelsTogether()
    {
        upgradeStatusPanel?.SetActive(false);
        upgradePanel?.SetActive(false);
    }
    private void ToggleOptionPanel(bool show)
    {
        optionPanel?.SetActive(show);
    }

    private void CloseAllTopPanels()
    {
        optionPanel?.SetActive(false);
        ownedRelicsPanel?.SetActive(false);
        upgradePanel?.SetActive(false);
        upgradeStatusPanel?.SetActive(false);
        academyPanel?.SetActive(false);
    }

    private void TryRefreshUpgradeChoices()
    {
        if (upgradePanel == null || GameManager.Instance == null || !GameManager.Instance.shouldRefreshUpgradeUI)
            return;

        var upgradeUI = upgradePanel.GetComponent<UpgradeUI>();
        if (upgradeUI == null) return;

        upgradeUI.ShowRandomChoices();
        GameManager.Instance.shouldRefreshUpgradeUI = false;
    }

    private void SaveAndGoTitle()
    {
   
        Debug.Log("💾 게임 저장 중...");
        //저장하는 함수
        // ✅ 타이틀 이동 전에 상단바 관련 패널 모두 정리
        CloseAllTopPanels();
        Debug.Log("🏁 타이틀 씬으로 이동 중...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title"); // 씬 이름이 정확해야 함
    }
}
