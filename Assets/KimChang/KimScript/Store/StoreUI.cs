using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreUI : MonoBehaviour
{
    [SerializeField] private Button leaveBtn;
    [SerializeField] private Transform unitParent, relicParent, itemParent, rerollObject;
    [SerializeField] private UnitSelectUI unitSelectUI;

    private List<StoreItemData> cachedUnitItems;
    private List<List<RogueUnitDataBase>> cachedUnitPackages;
    private List<StoreItemData> cachedRelicItems;
    private List<int> cachedRelicIds;
    private List<StoreItemData> cachedItemItems;
    private StoreItemData cachedRerollItem;

    private void OnEnable()
    {
        RestUI();
        ShowUnitUI();
        ShowRelicUI();
        ShowItemUI();
        ShowRerollUI();
    }

    private void RestUI()
    {
        leaveBtn.onClick.AddListener(CloseStore);
        RogueLikeData.Instance.SetSelectedUnits(new List<RogueUnitDataBase>());
        unitSelectUI.gameObject.SetActive(false);
    }

    private void CloseStore() => gameObject.SetActive(false);

    private float GetSaleRatio() => RogueLikeData.Instance.GetOwnedRelicById(0) != null ? 0.8f : 1f;

    private int CalculateDiscountedPrice(StoreItemData item)
    {
        int cost =  (int)(item.price * StoreManager.GetRandomBetweenValue(item.priceRateMin, item.priceRateMax));
        float sale = 1;
        sale += RelicManager.CheckRelicById(0) ? 0.2f : 0;
        sale += RelicManager.CheckRelicById(58) ? -0.2f : 0;
        cost = (int)(cost * sale);
        return cost;
    }

    private void ShowUnitUI()
    {
        cachedUnitItems = StoreManager.GetRandomUnitItems();
        cachedUnitPackages = new();

        for (int i = 0; i < cachedUnitItems.Count; i++)
        {
            var item = cachedUnitItems[i];
            var units = FilterAndSelectUnits(item);
            Transform child = unitParent.GetChild(i);
            cachedUnitPackages.Add(units);
            int price = CalculateUnitPackagePrice(units, item);
            SetUnitPackageUI(child, item, units, price);
        }
    }

    private void ShowRelicUI()
    {
        cachedRelicItems = StoreManager.GetRandomRelicItems();
        cachedRelicIds = new();

        for (int i = 0; i < cachedRelicItems.Count; i++)
        {
            StoreItemData item = cachedRelicItems[i];
            Transform child = relicParent.GetChild(i);
            int grade = int.Parse(item.value);
            var candidates = RelicManager.GetAvailableRelicIds(grade, RelicManager.RelicAction.Acquire)
                                         .Where(id => !cachedRelicIds.Contains(id)).ToList();
            if (candidates.Count == 0) continue;

            int relicId = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            cachedRelicIds.Add(relicId);
            int cost = CalculateDiscountedPrice(item);
            SetRelicUI(child, item, relicId, cost);
        }
    }

    private void ShowItemUI()
    {
        cachedItemItems = StoreManager.GetRandomEnergyMoraleItems();

        for (int i = 0; i < cachedItemItems.Count; i++)
        {
            StoreItemData item = cachedItemItems[i];
            Transform child = itemParent.GetChild(i);
            Button btn = child.GetComponent<Button>();
            int cost = CalculateDiscountedPrice(item);
            string path = $"ItemImages/Item{item.itemId}";
            SetStoreSlotUI(child, item, cost, path, () => PurChaseItem(btn, item, cost));
        }
    }

    private void ShowRerollUI()
    {
        cachedRerollItem = StoreManager.GetRandomDiceItem()[0];
        int cost = CalculateDiscountedPrice(cachedRerollItem);
        //SetStoreSlotUI(rerollObject, cachedRerollItem, cost, "ItemImages/Item60", null, int.Parse(cachedRerollItem.value));
        SetStoreSlotUI(
        rerollObject,
        cachedRerollItem,
        cost,
        "ItemImages/Item60",
        () => PurChaseItem(rerollObject.GetComponent<Button>(), cachedRerollItem, cost),
        int.Parse(cachedRerollItem.value)
    );
    }

    private void RefreshStorePrices()
    {
        for (int i = 0; i < cachedUnitItems.Count; i++)
        {
            StoreItemData item = cachedUnitItems[i];
            List<RogueUnitDataBase> units = cachedUnitPackages[i];
            int price = CalculateUnitPackagePrice(units, item);
            price = (int)(price * GetSaleRatio());

            var slot = unitParent.GetChild(i);
            SetImageAndPrice(slot, $"UnitImages/{units[0].unitImg}", price);
            SetItemInformation(slot, item, price, units);

            SetButtonState(slot.GetComponent<Button>(), price);
        }

        for (int i = 0; i < cachedRelicItems.Count; i++)
        {
            StoreItemData item = cachedRelicItems[i];
            int relicId = cachedRelicIds[i];
            int price = CalculateDiscountedPrice(item);

            var slot = relicParent.GetChild(i);
            SetImageAndPrice(slot, $"KIcon/WarRelic/{relicId}", price);
            SetItemInformation(slot, item, price, null, relicId);

            SetButtonState(slot.GetComponent<Button>(), price);
        }

        for (int i = 0; i < cachedItemItems.Count; i++)
        {
            StoreItemData item = cachedItemItems[i];
            int cost = CalculateDiscountedPrice(item);

            var slot = itemParent.GetChild(i);
            SetImageAndPrice(slot, $"ItemImages/Item{item.itemId}", cost);
            SetItemInformation(slot, item, cost);

            SetButtonState(slot.GetComponent<Button>(), cost);
        }

        int rerollCost = CalculateDiscountedPrice(cachedRerollItem);
        SetImageAndPrice(rerollObject, "ItemImages/Item60", rerollCost);
        SetItemInformation(rerollObject, cachedRerollItem, rerollCost, null, -1, int.Parse(cachedRerollItem.value));

        SetButtonState(rerollObject.GetComponent<Button>(), rerollCost);
    }

    private void SetUnitPackageUI(Transform child, StoreItemData item, List<RogueUnitDataBase> units, int price)
    {
        string imgChannel = $"UnitImages/{units[0].unitImg}";
        SetImageAndPrice(child, imgChannel, price);
        SetItemInformation(child, item, price, units);

        var packageCost = child.GetChild(0);
        var packageCount = packageCost.GetChild(0);
        var packageName = packageCount.GetChild(0);
        child.GetChild(2).gameObject.SetActive(false);
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

        child.GetChild(2).gameObject.SetActive(false);
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

        child.GetChild(2).gameObject.SetActive(false);
        var btn = child.GetComponent<Button>();
        if (!SetButtonState(btn, cost)) return;

        child.name = item.itemId.ToString();
        btn.onClick.RemoveAllListeners();
        if (onClick != null) btn.onClick.AddListener(() => onClick());
    }

    private List<RogueUnitDataBase> FilterAndSelectUnits(StoreItemData item)
    {
        var allUnits = UnitLoader.Instance.GetAllCachedUnits();

        List<RogueUnitDataBase> filtered = item.form switch
        {
            "Rarity" => item.value.Contains("~")
                ? EventManager.ParseRange(item.value) is var (min, max)
                    ? allUnits.Where(u => u.rarity >= min && u.rarity <= max).ToList()
                    : new()
                : int.TryParse(item.value, out var exact)
                    ? allUnits.Where(u => u.rarity == exact).ToList()
                    : new(),

            "Branch" => int.TryParse(item.value, out var b)
                ? allUnits.Where(u => u.branchIdx == b).ToList()
                : new(),

            "Tag" => int.TryParse(item.value, out var t)
                ? allUnits.Where(u => u.tagIdx == t).ToList()
                : new(),

            _ => new()
        };
        
        List<RogueUnitDataBase> result = new();
        for (int i = 0; i < item.count; i++)
        {
            if (filtered.Count == 0) break;

            int rand = UnityEngine.Random.Range(0, filtered.Count);
            RogueUnitDataBase baseUnit = filtered[rand];
            RogueUnitDataBase unit = UnitLoader.Instance.GetCloneUnitById(baseUnit.idx);

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

    private bool SetButtonState(Button btn, int price)
    {
        int gold = RogueLikeData.Instance.GetCurrentGold();
        if (RogueLikeData.Instance.GetOwnedRelicById(49) != null)
        {
            gold += 500;
        }
        btn.interactable = gold >= price;
        return btn.interactable;
    }

    private bool SpendGold(int cost)
    {
        int gold = RogueLikeData.Instance.GetCurrentGold();
        int lental = RelicManager.CheckRelicById(49)?500:0;
        if (gold+ lental < cost) return false;
        RogueLikeData.Instance.ReduceGold(cost);
        SaveData saveData = new();
        saveData.SaveDataFile();
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
        itemInformation.isItem = true;
        itemInformation.item = storeItemData;
        itemInformation.price = price;
        if (units != null) itemInformation.units = units;
        else if (relicId != -1) itemInformation.relicId = relicId;
        else if (rerollCount != 0) itemInformation.rerollCount = rerollCount;
    }

    private void PurchaseUnitPackage(Button btn, List<RogueUnitDataBase> units, int price)
    {
        if (!SpendGold(price)) return;
        var myUnits = RogueLikeData.Instance.GetMyTeam();
        myUnits.AddRange(units);
        RogueLikeData.Instance.SetMyTeam(myUnits);
        btn.transform.GetChild(2).gameObject.SetActive(true);
        btn.interactable = false;
    }

    private void PurchaseRelic(Button btn, int relicId, int price)
    {
        if (!SpendGold(price)) return;

        RogueLikeData.Instance.AcquireRelic(relicId);
        btn.transform.GetChild(2).gameObject.SetActive(true);
        btn.interactable = false;

        RefreshStorePrices();
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
                RogueLikeData.Instance.ChangeMorale(int.Parse(item.value));
                break;
            case "Reroll":
                RogueLikeData.Instance.AddReroll(item.count);
                Debug.Log($"🟢 리롤 획득 → 현재 리롤 수: {RogueLikeData.Instance.GetRerollChance()}");
                UIManager.Instance.UpdateReroll();
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
                var canSelectUnits = RogueLikeData.Instance.GetMyUnits()
                .Where(u => u.energy < u.maxEnergy)
                .ToList();

            if (canSelectUnits.Count < item.count)
                return;

                unitSelectUI.gameObject.SetActive(true);
                unitSelectUI.OpenSelectUnitWindow(() => PurChaseItem(btn, item, int.Parse(item.price.ToString())),null,item.count);
                return;
            }
            foreach (var unit in selected)
            {
                unit.energy = Math.Min(unit.maxEnergy, unit.energy + int.Parse(item.value));
            }

        }
        else if (item.form == "Random")
        {
            int amount = int.Parse(item.value);
            var units = RogueLikeData.Instance.GetMyTeam().Where(u => u.energy < u.maxEnergy).ToList();
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
    private void OnDisable()
    {
        GameManager.Instance.UpdateAllUI();
    }
}