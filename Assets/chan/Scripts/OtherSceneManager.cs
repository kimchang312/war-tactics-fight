using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherSceneManager : MonoBehaviour
{
    private async void Start()
    {
        await UnitDataManager.Instance.LoadUnitDataAsync();
        Debug.Log("���� ������ �ε� �Ϸ�");
    }
}