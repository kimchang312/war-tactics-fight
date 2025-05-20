using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadRLMap()
    {
        SceneManager.LoadScene("RLmap");
    }
}