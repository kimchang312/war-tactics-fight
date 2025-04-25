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

    public void OpenSelectUnitWindow(Action func,List<RogueUnitDataBase> selectUnits=null)
    {
        gameObject.SetActive(true);
        selectUnits ??= RogueLikeData.Instance.GetMyUnits();
        Debug.Log(selectUnits.Count);
        foreach (var unit in selectUnits)
        {
            GameObject selectedUnit = objectPool.GetSelectUnit();
            Image img = selectedUnit.GetComponent<Image>();
            Sprite sprite = Resources.Load<Sprite>($"UnitImages/{unit.unitImg}");
            img.sprite = sprite;
            Image energy = selectedUnit.GetComponentInChildren<Image>();
            energy.fillAmount = unit.energy / unit.maxEnergy;
            TextMeshProUGUI textMeshProUGUI = selectedUnit.GetComponentInChildren<TextMeshProUGUI>();
            textMeshProUGUI.text = $"{unit.energy}/{unit.maxEnergy}";
            selectedUnit.transform.SetParent(selectUnitParent.transform, false);
            Button btn = selectedUnit.GetComponent<Button>();
            btn.onClick.AddListener(() => AddSelectedUnits(func,unit));
        }
    }

    private void AddSelectedUnits(Action func,RogueUnitDataBase unit)
    {
        RogueLikeData.Instance.AddSelectedUnits(unit);
        gameObject.SetActive(false);
        func();
    }

}
