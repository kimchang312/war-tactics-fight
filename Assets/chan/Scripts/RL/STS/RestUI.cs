using UnityEngine;
using UnityEngine.UI;

public class RestUI : MonoBehaviour
{
    [Header("Rest Panel References")]
    public GameObject panel;        // Canvas 안의 Rest 패널 오브젝트
    public Button UpgradeButton;    // 유닛 강화 버튼
    public Button buffButton;       // 기력,사기 회복 선택 버튼. 버튼명 임시
    public Button closeButton;      // 패널 닫기 버튼

    private void Awake()
    {
        panel.SetActive(false);

        UpgradeButton.onClick.AddListener(OnUpgrade);
        buffButton.onClick.AddListener(OnBuff);
        closeButton.onClick.AddListener(Hide);
    }

    public void Show()
    {
        panel.SetActive(true);
        // 여기에 애니메이션이나 사운드를 추가해도 좋습니다.
    }

    public void Hide()
    {
        panel.SetActive(false);
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
