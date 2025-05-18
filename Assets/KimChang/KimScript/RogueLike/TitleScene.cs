using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScene : MonoBehaviour
{
    [SerializeField] private Button newGameBtn;
    [SerializeField] private Button loadBtn;
    [SerializeField] private Button exitBtn;
    
    private void Start()
    {
        UnitLoader.Instance.LoadUnitsFromJson();
        EventManager.LoadEventData();
        StoreManager.LoadStoreData();
        string filePath = Application.persistentDataPath + "/PlayerData.json";
        if (File.Exists(filePath))
        {
            loadBtn.interactable = true;
            loadBtn.onClick.AddListener(LoadRogueLike);
        }
        else
        {
            loadBtn.interactable = false;
        }

        newGameBtn.onClick.AddListener(GoRogueLike);
        exitBtn.onClick.AddListener(QuitGame);
    }

    private void GoRogueLike()
    {
        SaveData saveData = new SaveData();
        saveData.DeleteSaveFile();
        List<RogueUnitDataBase> units= RogueUnitDataBase.GetBaseUnits();
        RogueLikeData.Instance.SetMyTeam(units);
        RogueLikeData.Instance.SetAllMyUnits(units);
        saveData.SaveDataFile();
        SceneManager.LoadScene("RLmap");
    }
    private void LoadRogueLike()
    {
        SaveData saveData = new SaveData();
        saveData.LoadData();
        SceneManager.LoadScene("RLmap");
    }
    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;  // 에디터에서 중지
#else
    Application.Quit();  // 빌드된 게임 종료
#endif
    }


}
