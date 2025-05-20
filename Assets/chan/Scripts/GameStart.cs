using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStart : MonoBehaviour
{
   public void LoadMainScene()
    {
        // 메인 씬으로 돌아가기 전에 PlayerData 초기화
        PlayerData.Instance.ResetPlayerData();
        SceneManager.LoadScene("RLmap");
    }

    public void LoadUiScene()
    {
        SceneManager.LoadScene("Unit_UI");
    }

    public void LoadDifficultyScene()
    {
        SceneManager.LoadScene("Difficulty");
    }
}
