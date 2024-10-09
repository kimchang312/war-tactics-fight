using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FightStartBtn : MonoBehaviour
{
    public AutoBattleManager battleManager;  // Inspector���� ���� ������ AutoBattleManager

    public Button fightButton;

    void Start()
    {
        fightButton.onClick.AddListener(OnFightButtonClick);
    }

    void OnFightButtonClick()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("AutoBattleScene");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "AutoBattleScene")
        {
            // Inspector���� ����� battleManager ���
            if (battleManager != null)
            {
                battleManager.StartBattle();
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
