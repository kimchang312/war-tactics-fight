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
        // 1. 기존 오브젝트 정리
        objectPool.ReturnSelectUnit(selectUnitParent);

        // 2. 유닛 리스트 가져오기
        selectUnits ??= RogueLikeData.Instance.GetMyUnits();

        // 3. 유닛 UI 생성
        foreach (var unit in selectUnits)
        {
            GameObject selectedUnit = objectPool.GetSelectUnit();
            UIMaker.CreateSelectUnitEnergy(unit, selectedUnit);
            selectedUnit.transform.SetParent(selectUnitParent.transform, false);
            Button btn = selectedUnit.GetComponent<Button>();
            btn.onClick.AddListener(() => AddSelectedUnits(func, unit));
        }
    }


    private void AddSelectedUnits(Action func,RogueUnitDataBase unit)
    {
        RogueLikeData.Instance.AddSelectedUnits(unit);
        gameObject.SetActive(false);
        func();
    }

}
