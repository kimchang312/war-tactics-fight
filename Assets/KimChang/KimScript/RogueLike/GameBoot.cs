using UnityEngine;

[DefaultExecutionOrder(-10000)]
public sealed class GameBoot : MonoBehaviour
{
    void Awake()
    {
        SaveData save = new();
        save.LoadData();
        EventManager.LoadEventData();
        StoreManager.LoadStoreData();
        UnitLoader.Instance.LoadUnitsFromJson();
        GameTextDB.Boot();
        DontDestroyOnLoad(gameObject);
    }
}