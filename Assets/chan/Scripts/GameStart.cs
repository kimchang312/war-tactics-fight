using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStart : MonoBehaviour
{
   public void LoadMainScene()
    {
        SceneManager.LoadScene("Main");
    }

    public void LoadUiScene()
    {
        SceneManager.LoadScene("Unit_UI");
    }
}
