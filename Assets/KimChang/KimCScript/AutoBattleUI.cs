using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AutoBattleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _myUnitCountUI;
    [SerializeField] private TextMeshProUGUI _enemyUnitCountUI;
    [SerializeField] private TextMeshProUGUI _myUnitHPUI;
    [SerializeField] private TextMeshProUGUI _emyUnitHPUI;

    [SerializeField] private TextMeshPro _myUnitName;
    [SerializeField] private TextMeshPro _enemyUnitName;

    public void UpdateUnitCountUI(int myUnitCount,int enemyUnitCount)
    {
        if (_myUnitCountUI != null && _enemyUnitCountUI)
        {
            _myUnitCountUI.text = $"{myUnitCount}";
            _enemyUnitCountUI.text = $"{enemyUnitCount}";
        }
    }
    public void UpateUnitHPUI(float myUnitHP,float enemyUnitHP)
    {
        if (_myUnitHPUI != null && _emyUnitHPUI)
        {
            _myUnitHPUI.text = $"{myUnitHP}";
            _emyUnitHPUI.text = $"{enemyUnitHP}";
        }
        
    }

    public void UpdateName(string myUnitName,string enemyUnitName)
    {
        _myUnitName.text = $"{myUnitName}";
        _enemyUnitName.text=$"{enemyUnitName}";
    }
}
