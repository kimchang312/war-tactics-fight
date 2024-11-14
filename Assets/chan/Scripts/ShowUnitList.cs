using UnityEngine;
using UnityEngine.UI;

public class ShowUnitButton : MonoBehaviour
{
    public Button showUnitListButton; // ���� ����� Ȯ���� ��ư

    private void Start()
    {
        // ��ư Ŭ�� �̺�Ʈ �߰�
        if (showUnitListButton != null)
        {
            showUnitListButton.onClick.AddListener(OnShowUnitListButtonClicked);
        }
    }

    // ��ư Ŭ�� �� ���� ����� Debug.Log�� ���
    private void OnShowUnitListButtonClicked()
    {
        PlayerData.Instance.ShowPlacedUnitList();  // ��ġ�� ���� ��� ���
    }
}