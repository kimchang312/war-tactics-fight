using UnityEngine;
using UnityEngine.SceneManagement;

public class ContinueButton : MonoBehaviour
{
    public void OnContinue()
    {
        if (SaveData.HasContinueLoadRequest())
            return;

        SaveData saveData = new();
        if (saveData.RequestContinueLoadFromTitle())
        {
            SceneManager.LoadScene("RLmap");
        }
        else
        {
            Debug.Log("불러올 저장 데이터가 없습니다!");
        }
    }
}
