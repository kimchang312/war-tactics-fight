using UnityEngine;
using UnityEngine.UI;

public class RestUI : MonoBehaviour
{
    [Header("Rest Panel References")]
    public GameObject panel;        // Canvas 안의 Rest 패널 오브젝트
    public Button UpgradeButton;    // 유닛 강화 버튼
    public Button buffButton;       // 기력,사기 회복 선택 버튼. 버튼명 임시
    public Button closeButton;      // 패널 닫기 버튼

    private CanvasGroup panelCG;

    private void Awake()
    {
        // CanvasGroup 세팅 (없으면 추가)
        panelCG = panel.GetComponent<CanvasGroup>();
        if (panelCG == null) panelCG = panel.AddComponent<CanvasGroup>();

        // 처음엔 숨기고, 인터랙션 차단
        panel.SetActive(false);
        panelCG.interactable = false;
        panelCG.blocksRaycasts = false;

        UpgradeButton.onClick.AddListener(OnUpgrade);
        buffButton.onClick.AddListener(OnBuff);
        closeButton.onClick.AddListener(Hide);
    }

    public void Show()
    {
        panel.SetActive(true);
        // 여기에 애니메이션이나 사운드를 추가해도 좋습니다.
        panel.SetActive(true);
        panelCG.interactable = true;
        panelCG.blocksRaycasts = true;

        Debug.Log("[RestUI] Show() 호출됨");

    }

    public void Hide()
    {
        panelCG.interactable = false;
        panelCG.blocksRaycasts = false;
        panel.SetActive(false);
        Debug.Log("[RestUI] Hide() 호출됨");
    }

    private void OnUpgrade()
    {
        Debug.Log("선택적으로 유닛 강화 가능.");
        // TODO: 실제 강화 로직 호출
        Hide();
    }

    private void OnBuff()
    {
        Debug.Log("기력 및 사기 회복!");
        // TODO: 기력 및 사기 회복 UI 열기
        Hide();
    }
}
