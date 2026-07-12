using UnityEngine;

[DefaultExecutionOrder(-10000)]
public sealed class GameBoot : MonoBehaviour
{
    void Awake()
    {
        UnitLoader.Instance.LoadUnitsFromJson();
        SaveData save = new();
        save.LoadData();
        EventManager.LoadEventData();
        StoreManager.LoadStoreData();
        GameTextDB.Boot();
        DontDestroyOnLoad(gameObject);
    }
}
