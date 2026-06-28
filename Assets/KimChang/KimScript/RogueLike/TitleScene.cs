using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScene : MonoBehaviour
{
    [SerializeField] private Button newGameBtn;
    [SerializeField] private Button loadBtn;
    [SerializeField] private Button exitBtn;
    [SerializeField] private Button testOptionBtn;

    [SerializeField] private TextMeshProUGUI newText;
    [SerializeField] private TextMeshProUGUI loadText;
    [SerializeField] private TextMeshProUGUI endText;


    private void Start()
    {
        UnitLoader.Instance.LoadUnitsFromJson();
        EventManager.LoadEventData();
        StoreManager.LoadStoreData();
        QuestManager.LoadQuestData();
        GameTextDB.Boot();
        if (SaveData.CanContinueRun())
        {
            loadBtn.interactable = true;
            loadBtn.onClick.AddListener(LoadRogueLike);
        }
        else
        {
            loadBtn.interactable = false;
        }
        newText.text = GameTextDB.Get(2);
        //loadText.text = GameTextDB.Get(2);
        endText.text = GameTextDB.Get(4);
        RogueLikeData.Instance.ResetToDefault();
        newGameBtn.onClick.AddListener(GoRogueLike);
        exitBtn.onClick.AddListener(QuitGame);

    }

    private void GoRogueLike()
    {
        SaveData saveData = new();

        saveData.ResetGameData();

        RogueLikeData.Instance.SetResetMap(true);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentStageNull();
        }
        SceneManager.LoadScene("RLmap");
    }
    private void LoadRogueLike()
    {
        SaveData saveData = new SaveData();

        if (saveData.RequestContinueLoadFromTitle())
        {
            SceneManager.LoadScene("RLmap");
        }
    }
    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }


}
