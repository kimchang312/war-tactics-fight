using UnityEngine;
using UnityEngine.SceneManagement;

public class Faction : MonoBehaviour
{
    // Emfire 진영 선택
    public void SelectEmfire()
    {
        SetFactionAndLoadScene("제국", 1);
    }

    // Heptachy 진영 선택
    public void SelectHeptachy()
    {
        SetFactionAndLoadScene("칠성연합", 3);
    }

    // Divinitas 진영 선택
    public void SelectDivinitas()
    {
        SetFactionAndLoadScene("신성국", 2);
    }

    // 진영을 설정하고 다음 씬을 로드하는 공통 함수
    private void SetFactionAndLoadScene(string factionName, int factionidx)
    {
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.faction = factionName;
            PlayerData.Instance.factionidx = factionidx;
        }
        else
        {
            Debug.LogWarning("PlayerData 인스턴스를 찾을 수 없습니다.");
        }

        // Unit_UI 씬으로 전환
        SceneManager.LoadScene("Unit_UI");
    }
}
