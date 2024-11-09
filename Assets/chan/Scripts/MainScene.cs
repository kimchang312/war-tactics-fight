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

    
}