using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FightResult : MonoBehaviour
{
    [SerializeField] private GameObject winImg;
    [SerializeField] private GameObject loseImg;
    [SerializeField] private GameObject drawImg;
    [SerializeField] private Button goTitleBtn;

    // Start is called before the first frame update
    void Start()
    {
        goTitleBtn.onClick.AddListener(OnGoTitleButtonClick);
        /*
        this.gameObject.SetActive(false);
        winImg.SetActive(false);
        loseImg.SetActive(false);
        drawImg.SetActive(false);
        */
    }

    public void EndGame(int result)
    {
        int siblingCount = this.transform.parent.childCount; // 형제 개수 확인
        this.transform.SetSiblingIndex(siblingCount - 1);

        this.gameObject.SetActive(true);

        if (result == 0)
        {
            WinGame();
        }
        else if (result == 1)
        {
            LoseGame();
        }
        else
        {
            DrawGame(); 
        }
    }

    private void OnGoTitleButtonClick()
    {
        SceneManager.LoadScene("Main");
    }

    private void WinGame()
    {
        winImg.SetActive(true);
    }

    private void LoseGame()
    {
        loseImg.SetActive(true);
    }

    private void DrawGame()
    {
        drawImg.SetActive(true);
    }
}
