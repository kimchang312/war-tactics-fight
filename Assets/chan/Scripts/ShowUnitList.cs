using UnityEngine;
using UnityEngine.UI;

public class ShowUnitButton : MonoBehaviour
{
    public Button showUnitListButton; // 유닛 목록을 확인할 버튼

    private void Start()
    {
        // 버튼 클릭 이벤트 추가
        if (showUnitListButton != null)
        {
            showUnitListButton.onClick.AddListener(OnShowUnitListButtonClicked);
        }
    }

    // 버튼 클릭 시 유닛 목록을 Debug.Log로 출력
    private void OnShowUnitListButtonClicked()
    {
        PlayerData.Instance.ShowPlacedUnitList();  // 배치된 유닛 목록 출력
    }
}