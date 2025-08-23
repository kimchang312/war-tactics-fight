using UnityEngine;
using UnityEngine.SceneManagement;

public class ContinueButton : MonoBehaviour
{
    public void OnContinue()
    {
        var save = SaveSystem.LoadFull();
        if (save != null)
        {
            SceneManager.LoadScene("RLmap");
        }
        else
        {
            Debug.Log("저장된 맵이 없습니다!");
        }
    }
}