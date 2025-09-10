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

    private Action onSelectAction;
    private int requireCount = 1;
    private List<RogueUnitDataBase> availableUnits;

    private void Start()
    {
        gameObject.SetActive(false);
        if (objectPool == null)
        {
            objectPool = GameManager.Instance.objectPool;
        }
    }

    public void OpenSelectUnitWindow(Action func, List<RogueUnitDataBase> selectUnits = null, int requiredCount = 1)
    {
        onSelectAction = func;
        requireCount = requiredCount;

        objectPool.ReturnSelectUnit(selectUnitParent);

        availableUnits = selectUnits ?? RogueLikeData.Instance.GetMyTeam();

        var selected = RogueLikeData.Instance.GetSelectedUnits();
        if (selected != null && selected.Count >= requireCount)
        {
            onSelectAction?.Invoke();
            gameObject.SetActive(false);
            return;
        }

        foreach (var unit in availableUnits)
        {
            GameObject selectedUnit = objectPool.GetSelectUnit();
            UIMaker.CreateSelectUnitEnergy(unit, selectedUnit);
            selectedUnit.transform.SetParent(selectUnitParent.transform, false);

            RogueUnitDataBase copy = unit;
            Button btn = selectedUnit.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => AddSelectedUnits(copy));
        }

        gameObject.SetActive(true);
    }

    private void AddSelectedUnits(RogueUnitDataBase unit)
    {
        if (RogueLikeData.Instance.GetSelectedUnits().Contains(unit))
            return;

        RogueLikeData.Instance.AddSelectedUnits(unit);

        if (RogueLikeData.Instance.GetSelectedUnits().Count >= requireCount)
        {
            gameObject.SetActive(false);
            onSelectAction?.Invoke();
        }
    }
}
