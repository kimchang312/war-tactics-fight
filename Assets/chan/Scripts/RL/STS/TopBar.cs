using UnityEngine.UI;
using UnityEngine;

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
    private void Awake()
    {
        // 초기에는 다 꺼두고
        ownedRelicsPanel?.SetActive(false);
        upgradePanel?.SetActive(false);
        upgradeStatusPanel?.SetActive(false);
        academyPanel?.SetActive(false);

        relicsToggleButton?.onClick.AddListener(() => ToggleOnly(ownedRelicsPanel));
        upgradeToggleButton?.onClick.AddListener(() => ToggleOnly(upgradePanel));
        upgradeStatusButton?.onClick.AddListener(() => ToggleOnly(upgradeStatusPanel));
        academyToggleButton?.onClick.AddListener(() => ToggleOnly(academyPanel));
        // 닫기 버튼에도 같은 토글 메서드 연결
        closeRelicsButton.onClick.AddListener(() => ToggleOnly(ownedRelicsPanel));
        closeupgradeStatusButton.onClick.AddListener(() => ToggleOnly(upgradeStatusPanel));
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
    private void closePanel()
    {
        
    }
}
