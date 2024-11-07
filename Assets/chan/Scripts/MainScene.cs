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

    /*private async void Start()
    {
        // PlayerData 인스턴스가 null인 경우 초기화 확인
        if (PlayerData.Instance == null)
        {
            Debug.LogError("PlayerData 인스턴스를 찾을 수 없습니다.");
            return;
        }

        // UnitDataManager 인스턴스가 null인 경우 초기화 확인
        if (UnitDataManager.Instance == null)
        {
            Debug.LogError("UnitDataManager 인스턴스를 찾을 수 없습니다.");
            return;
        }

        // 정상적으로 초기화된 경우 데이터 로드 시작
        Debug.Log("PlayerData 및 UnitDataManager 인스턴스가 정상적으로 존재합니다.");
        await UnitDataManager.Instance.LoadUnitDataAsync();
    }*/
}