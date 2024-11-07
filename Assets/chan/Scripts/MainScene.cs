using UnityEngine;
using System.Threading.Tasks;

public class MainScene : MonoBehaviour
{
    private void Awake()
    {
        // �� ������Ʈ�� ���� PlayerData�� UnitDataManager�� �̱��� �ν��Ͻ��� �ʱ�ȭ
        if (PlayerData.Instance == null)
        {
            new GameObject("PlayerData").AddComponent<PlayerData>();
        }

        if (UnitDataManager.Instance == null)
        {
            new GameObject("UnitDataManager").AddComponent<UnitDataManager>();
        }
    }

    /*private async void Start()
    {
        // PlayerData �ν��Ͻ��� null�� ��� �ʱ�ȭ Ȯ��
        if (PlayerData.Instance == null)
        {
            Debug.LogError("PlayerData �ν��Ͻ��� ã�� �� �����ϴ�.");
            return;
        }

        // UnitDataManager �ν��Ͻ��� null�� ��� �ʱ�ȭ Ȯ��
        if (UnitDataManager.Instance == null)
        {
            Debug.LogError("UnitDataManager �ν��Ͻ��� ã�� �� �����ϴ�.");
            return;
        }

        // ���������� �ʱ�ȭ�� ��� ������ �ε� ����
        Debug.Log("PlayerData �� UnitDataManager �ν��Ͻ��� ���������� �����մϴ�.");
        await UnitDataManager.Instance.LoadUnitDataAsync();
    }*/
}