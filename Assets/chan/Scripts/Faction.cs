using UnityEngine;
using UnityEngine.SceneManagement;

public class Faction : MonoBehaviour
{
    // Emfire ���� ����
    public void SelectEmfire()
    {
        SetFactionAndLoadScene("����");
    }

    // Heptachy ���� ����
    public void SelectHeptachy()
    {
        SetFactionAndLoadScene("ĥ������");
    }

    // Divinitas ���� ����
    public void SelectDivinitas()
    {
        SetFactionAndLoadScene("�ż���");
    }

    // ������ �����ϰ� ���� ���� �ε��ϴ� ���� �Լ�
    private void SetFactionAndLoadScene(string factionName)
    {
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.faction = factionName;
        }
        else
        {
            Debug.LogWarning("PlayerData �ν��Ͻ��� ã�� �� �����ϴ�.");
        }

        // Unit_UI ������ ��ȯ
        SceneManager.LoadScene("Unit_UI");
    }
}
