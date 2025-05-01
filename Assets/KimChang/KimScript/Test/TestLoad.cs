using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLoad : MonoBehaviour
{
    private async void Awake()
    {
        await GoogleSheetLoader.Instance.LoadUnitSheetData();
        SaveData save = new();
        save.LoadData();
        EventManager.LoadEventData();
        StoreManager.LoadStoreData();

    }
}
