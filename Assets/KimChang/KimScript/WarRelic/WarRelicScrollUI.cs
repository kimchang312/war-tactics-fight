using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class WarRelicScrollUI : MonoBehaviour
{
    [SerializeField] private GameObject itemToolTip;
    [SerializeField] private ObjectPool objectPool;

    private void Start()
    {
        if (itemToolTip == null)
        {
            itemToolTip = GameManager.Instance.itemToolTip;
        }
        if (objectPool == null)
        {
            objectPool = GameManager.Instance.objectPool;
        }
        CreateRelicList();
    }

    private void CreateRelicList()
    {
        WarRelicBoxUI.SetRelicBox(this.gameObject, itemToolTip, objectPool);
    }
}
