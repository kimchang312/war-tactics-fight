using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FightStartBtn : MonoBehaviour
{
    public AutoBattleManager battleManager;  // Inspector에서 직접 연결할 AutoBattleManager

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
            // Inspector에서 연결된 battleManager 사용
            if (battleManager != null)
            {
                battleManager.StartBattle();
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
