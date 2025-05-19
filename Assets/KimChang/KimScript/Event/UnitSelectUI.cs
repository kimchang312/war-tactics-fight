using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitSelectUI : MonoBehaviour
{
    [SerializeField] private GameObject selectUnitParent;
    [SerializeField] private ObjectPool objectPool;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void OpenSelectUnitWindow(Action func, List<RogueUnitDataBase> selectUnits = null)
    {
        objectPool.ReturnSelectUnit(selectUnitParent);

        selectUnits ??= RogueLikeData.Instance.GetMyTeam();

        // 3. 유닛 UI 생성
        foreach (var unit in selectUnits)
        {
            GameObject selectedUnit = objectPool.GetSelectUnit();
            UIMaker.CreateSelectUnitEnergy(unit, selectedUnit);
            selectedUnit.transform.SetParent(selectUnitParent.transform, false);

            RogueUnitDataBase copy = unit;
            Button btn = selectedUnit.GetComponent<Button>();
            btn.onClick.AddListener(() => AddSelectedUnits(func, copy));
        }
    }


    private void AddSelectedUnits(Action func,RogueUnitDataBase unit)
    {
        RogueLikeData.Instance.AddSelectedUnits(unit);
        gameObject.SetActive(false);
        func();
    }

}
