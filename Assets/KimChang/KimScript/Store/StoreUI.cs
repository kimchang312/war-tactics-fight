using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreUI : MonoBehaviour
{
    [SerializeField] private Button leaveBtn;

    private void Awake()
    {
        StoreManager.LoadStoreData();
    }

    private void OnEnable()
    {
        
    }

    private void RestUI()
    {
        leaveBtn.onClick.AddListener(CloseStore);
    }

    private void CloseStore()
    {
        gameObject.SetActive(false);
    }

}
