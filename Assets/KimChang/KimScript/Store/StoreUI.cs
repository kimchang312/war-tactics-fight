using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreUI : MonoBehaviour
{
    [SerializeField] private Button leaveBtn;
    [SerializeField] private Transform unitParent;
    [SerializeField] private Transform relicParent;
    [SerializeField] private Transform itemParent;
    [SerializeField] private Transform rerollObject;
    [SerializeField] private UnitSelectUI unitSelectUI;


    private void OnEnable()
    {
        RestUI();
        ShowUnitUI();
        ShowRelicUI();
        ShowItemUI();
        ShowRerollUI();
    }

    private void ShowUnitUI()
    {
        var items = StoreManager.GetRandomUnitItems();
        for (int i = 0; i < items.Count; i++)
        {
            var child = unitParent.GetChild(i);
            var units = FilterAndSelectUnits(items[i]);
            var price = CalculateUnitPackagePrice(units, items[i]);
            SetUnitPackageUI(child, items[i], units, price);
        }
    }

    private void ShowRelicUI()
    {
        var items = StoreManager.GetRandomRelicItems();
        HashSet<int> usedIds = new();
        Debug.Log(items.Count);
        for (int i = 0; i < items.Count; i++)
        {
            int grade = int.Parse(items[i].value);
            var candidates = RelicManager.GetAvailableRelicIds(grade, RelicManager.RelicAction.Acquire)
                                         .Where(id => !usedIds.Contains(id)).ToList();

            if (candidates.Count == 0) continue;

            int relicId = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            usedIds.Add(relicId);
            int cost = (int)(items[i].price * StoreManager.GetRandomBetweenValue(items[i].priceRateMin, items[i].priceRateMax));
            SetRelicUI(relicParent.GetChild(i), items[i], relicId, cost);
        }
    }

    private void ShowItemUI()
    {
        var items = StoreManager.GetRandomEnergyMoraleItems();
        for (int i = 0; i < items.Count; i++)
        {
            int cost = (int)(items[i].price * StoreManager.GetRandomBetweenValue(items[i].priceRateMin, items[i].priceRateMax));
            string path = $"ItemImages/Item{items[i].itemId}";
            SetStoreSlotUI(itemParent.GetChild(i), items[i], cost, path, () => PurChaseItem(itemParent.GetChild(i).GetComponent<Button>(), items[i], cost));
        }
    }

    private void ShowRerollUI()
    {
        var item = StoreManager.GetRandomDiceItem()[0];
        int cost = (int)(item.price * StoreManager.GetRandomBetweenValue(item.priceRateMin, item.priceRateMax));
        string path = "ItemImages/Item60";
        SetStoreSlotUI(rerollObject, item, cost, path, null, int.Parse(item.value));
    }

    private void SetUnitPackageUI(Transform child, StoreItemData item, List<RogueUnitDataBase> units, int price)
    {
        string imgChannel = $"UnitImages/{units[0].unitImg}";
        SetImageAndPrice(child, imgChannel, price);
        SetItemInformation(child, item, price, units);

        var packageCost = child.GetChild(0);
        var packageCount = packageCost.GetChild(0);
        var packageName = packageCount.GetChild(0);
        packageCount.GetComponent<TextMeshProUGUI>().text = $" X{item.count}";
        packageName.GetComponent<TextMeshProUGUI>().text = item.itemName;

        var btn = child.GetComponent<Button>();
        if (!SetButtonState(btn, price)) return;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => PurchaseUnitPackage(btn, units, price));
    }

    private void SetRelicUI(Transform child, StoreItemData item, int relicId, int price)
    {
        string imgChannel = $"KIcon/WarRelic/{relicId}";
        SetImageAndPrice(child, imgChannel, price);
        SetItemInformation(child, item, price, null, relicId);

        var btn = child.GetComponent<Button>();
        if (!SetButtonState(btn, price)) return;

        child.name = relicId.ToString();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => PurchaseRelic(btn, relicId, price));
    }

    private void SetStoreSlotUI(Transform child, StoreItemData item, int cost, string spritePath, Action onClick, int rerollCount = 0)
    {
        SetImageAndPrice(child, spritePath, cost);
        SetItemInformation(child, item, cost, null, -1, rerollCount);

        var btn = child.GetComponent<Button>();
        if (!SetButtonState(btn, cost)) return;

        child.name = item.itemId.ToString();
        btn.onClick.RemoveAllListeners();
        if (onClick != null) btn.onClick.AddListener(() => onClick());
    }

    private List<RogueUnitDataBase> FilterAndSelectUnits(StoreItemData item)
    {
        var allUnits = GoogleSheetLoader.Instance.GetAllUnitsAsObject();
        List<RogueUnitDataBase> filtered = item.form switch
        {
            "Rarity" => EventManager.ParseRange(item.value) is var (min, max) ? allUnits.Where(u => u.rarity >= min && u.rarity <= max).ToList() : new(),
            "Branch" => int.TryParse(item.value, out var b) ? allUnits.Where(u => u.branchIdx == b).ToList() : new(),
            "Tag" => int.TryParse(item.value, out var t) ? allUnits.Where(u => u.tagIdx == t).ToList() : new(),
            _ => new()
        };

        List<RogueUnitDataBase> result = new();
        for (int i = 0; i < item.count; i++)
        {
            int idx = UnityEngine.Random.Range(0, filtered.Count);
            var unit = RogueUnitDataBase.ConvertToUnitDataBase(GoogleSheetLoader.Instance.GetRowUnitData(idx));
            unit.energy = Math.Max(1, (int)((unit.energy * item.price) * 0.01f));
            result.Add(unit);
        }
        return result;
    }

    private int CalculateUnitPackagePrice(List<RogueUnitDataBase> units, StoreItemData item)
    {
        int total = units.Sum(u => u.unitPrice);
        return (int)(total * StoreManager.GetRandomBetweenValue(item.priceRateMin, item.priceRateMax));
    }

    private void RestUI()
    {
        leaveBtn.onClick.AddListener(CloseStore);
        RogueLikeData.Instance.SetSelectedUnits(new List<RogueUnitDataBase>());
        unitSelectUI.gameObject.SetActive(false);
    }

    private void CloseStore()
    {
        gameObject.SetActive(false);
    }

    private bool SetButtonState(Button btn, int price)
    {
        int gold = RogueLikeData.Instance.GetCurrentGold();
        btn.interactable = gold >= price;
        return btn.interactable;
    }

    private bool SpendGold(int cost)
    {
        int gold = RogueLikeData.Instance.GetCurrentGold();
        if (gold < cost) return false;
        RogueLikeData.Instance.SetCurrentGold(gold - cost);
        return true;
    }

    private void SetImageAndPrice(Transform child, string spritePath, int price)
    {
        child.GetComponent<Image>().sprite = SpriteCacheManager.GetSprite(spritePath);
        child.GetChild(0).GetComponent<TextMeshProUGUI>().text = price.ToString();
    }

    private void SetItemInformation(Transform child, StoreItemData storeItemData, int price, List<RogueUnitDataBase> units = null, int relicId = -1, int rerollCount = 0)
    {
        ItemInformation itemInformation = child.GetComponent<ItemInformation>();
        itemInformation.item = storeItemData;
        itemInformation.price = price;
        if (units != null) itemInformation.units = units;
        else if (relicId != -1) itemInformation.relicId = relicId;
        else if (rerollCount != 0) itemInformation.rerollCount = rerollCount;
    }

    private void PurchaseUnitPackage(Button btn, List<RogueUnitDataBase> units, int price)
    {
        if (!SpendGold(price)) return;
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        myUnits.AddRange(units);
        RogueLikeData.Instance.SetAllMyUnits(myUnits);
        btn.transform.GetChild(2).gameObject.SetActive(true);
        btn.interactable = false;
    }

    private void PurchaseRelic(Button btn, int relicId, int price)
    {
        if (!SpendGold(price)) return;
        RogueLikeData.Instance.AcquireRelic(relicId);
        btn.transform.GetChild(2).gameObject.SetActive(true);
        btn.interactable = false;
    }

    private void PurChaseItem(Button btn, StoreItemData item, int price)
    {
        if (!SpendGold(price)) return;

        switch (item.type)
        {
            case "Energy":
                ApplyEnergyItem(item, btn);
                break;
            case "Morale":
                int morale = RogueLikeData.Instance.GetMorale();
                RogueLikeData.Instance.SetMorale(Math.Min(100, morale + int.Parse(item.value)));
                break;
        }

        btn.transform.GetChild(2).gameObject.SetActive(true);
        btn.interactable = false;
    }

    private void ApplyEnergyItem(StoreItemData item, Button btn)
    {
        if (item.form == "Select")
        {
            var selected = RogueLikeData.Instance.GetSelectedUnits();
            if (selected == null || selected.Count < item.count)
            {
                unitSelectUI.gameObject.SetActive(true);
                unitSelectUI.OpenSelectUnitWindow(() => PurChaseItem(btn, item, int.Parse(item.price.ToString())));
                return;
            }
            foreach (var unit in selected)
                unit.energy = Math.Min(unit.maxEnergy, unit.energy + int.Parse(item.value));
        }
        else if (item.form == "Random")
        {
            int amount = int.Parse(item.value);
            var units = RogueLikeData.Instance.GetMyUnits().Where(u => u.energy < u.maxEnergy).ToList();
            if (units.Count == 0) return;

            for (int i = 0; i < units.Count; i++)
            {
                int r = UnityEngine.Random.Range(i, units.Count);
                (units[i], units[r]) = (units[r], units[i]);
            }

            for (int i = 0; i < Mathf.Min(item.count, units.Count); i++)
            {
                units[i].energy = Math.Min(units[i].maxEnergy, units[i].energy + amount);
            }
        }
    }
}