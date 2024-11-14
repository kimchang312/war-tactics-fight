using UnityEngine;
using System.Threading.Tasks;

public class MainScene : MonoBehaviour
{
    private void Awake()
    {
        // 빈 오브젝트를 통해 PlayerData와 UnitDataManager의 싱글톤 인스턴스를 초기화
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