using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TestLoad : MonoBehaviour
{
    [SerializeField] private Button button;

    private void Start()
    {
        button.onClick.AddListener(ClickBtn);
    }

    private void ClickBtn()
    {
        SaveData saveData = new SaveData();
        saveData.LoadData();
        SceneManager.LoadScene("Event");
    }
}
