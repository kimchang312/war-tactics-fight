using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

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

    }

    private void Update()
    {
        WarRelicBoxUI.SetRelicBox(this.gameObject, itemToolTip, objectPool);
    }
}
