using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreUI : MonoBehaviour
{
    [SerializeField] private Button purchaseBtn;
    [SerializeField] private Button leaveBtn;
    [SerializeField] private Transform unitParent, relicParent, itemParent, rerollObject;
    [SerializeField] private UnitSelectUI unitSelectUI;
    [SerializeField] private Transform unitPackage;

    [SerializeField] private GameObject packagePanel;

    [SerializeField] private Button purchasePackageBtn;
    [SerializeField] private Button leavePackageBtn;
    [SerializeField] private TextMeshProUGUI packageGoldText;
    [SerializeField] private Transform relicField;
    [SerializeField] private Transform itemField;


    private List<StoreItemData> cachedUnitItems;
    private List<List<RogueUnitDataBase>> cachedUnitPackages;
    private List<StoreItemData> cachedRelicItems;
    private List<int> cachedRelicIds;
    private List<StoreItemData> cachedItemItems;
    private StoreItemData cachedRerollItem;
    private Button checkedBtn;
    
    private const float aniTime = 0.5f;

    private void OnEnable()
    {
        RogueLikeData.Instance.SetCurrentGold(3000);
        RestUI();
        ShowUnitUI();
        ShowRelicUI();
        ShowItemUI();
        ShowRerollUI();
    }
    private void OnDisable()
    {
        GameManager.Instance.UpdateAllUI();
    }
    private void RestUI()
    {
        leaveBtn.onClick.RemoveAllListeners();
        leaveBtn.onClick.AddListener(CloseStore);
        
        leavePackageBtn.onClick.RemoveAllListeners();

        purchaseBtn.onClick.RemoveAllListeners();
        purchaseBtn.onClick.AddListener(ClickPurchaseItemBtn);


        RogueLikeData.Instance.SetSelectedUnits(new List<RogueUnitDataBase>());
        unitSelectUI.gameObject.SetActive(false);
        ClosePackageBack();
        UnCheckAllItem();
        AddClickEventItemToCheck();
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

            cachedUnitPackages.Add(units);
            int price = CalculateUnitPackagePrice(units, item);
            /*
             *             Transform child = unitParent.GetChild(i);
            SetUnitPackageUI(child, item, units, price);
            */
            UnitPackageUI child = unitPackage.GetChild(i).GetComponent<UnitPackageUI>();

            child.SetUnitPackage(units,item, price);
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
        btn.onClick.AddListener(() => ClickItemAndCheck(btn));

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
        btn.onClick.AddListener(() => ClickItemAndCheck(btn));

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
    
    private void PurchaseUnitPackage(GameObject obj,List<RogueUnitDataBase> units, int price)
    {
        if (!SpendGold(price)) return;
        var myUnits = RogueLikeData.Instance.GetMyTeam();
        myUnits.AddRange(units);
        RogueLikeData.Instance.SetMyTeam(myUnits);
        //btn.transform.GetChild(2).gameObject.SetActive(true);
        //btn.interactable = false;
        obj.SetActive(false);
        ClosePackageBack();
    }

    private void PurchaseRelic(Button btn, int relicId, int price)
    {
        if (!SpendGold(price)) return;

        RogueLikeData.Instance.AcquireRelic(relicId);

        //RefreshStorePrices();
        SoldOutItemBtn(btn);
    }

    //구매 버튼 클릭 시
    private void ClickPurchaseItemBtn()
    {
        if (checkedBtn == null) return;

        ItemInformation info = checkedBtn.GetComponent<ItemInformation>();
        if (info == null) return;

        // 가격 검사 및 소모
        if (!SpendGold(info.price)) return;

        // Relic인 경우
        if (info.relicId != -1)
        {
            RogueLikeData.Instance.AcquireRelic(info.relicId);
        }
        // 일반 아이템 처리
        else if (info.item != null)
        {
            switch (info.item.type)
            {
                case "Energy":
                    ApplyEnergyItem(info.item, checkedBtn);
                    break;
                case "Morale":
                    RogueLikeData.Instance.ChangeMorale(int.Parse(info.item.value));
                    break;
                case "Reroll":
                    RogueLikeData.Instance.AddReroll(info.item.count);
                    UIManager.Instance.UpdateReroll();
                    break;
            }
        }

        // 구매 UI 처리
        checkedBtn.transform.GetChild(2).gameObject.SetActive(true);  // SOLD OUT
        checkedBtn.transform.GetChild(3).gameObject.SetActive(false); // 체크 해제
        checkedBtn.interactable = false;
        checkedBtn.onClick.RemoveAllListeners();
        checkedBtn = null;
    }


    //아이템 클릭 시 
    private void ClickItemAndCheck(Button btn)
    {
        if (!btn.interactable) return;

        UnCheckAllItem();

        btn.transform.GetChild(3).gameObject.SetActive(true); // 체크 표시
        checkedBtn = btn;
    }



    //채크 풀기
    private void UnCheckAllItem()
    {
        foreach (Transform item in relicField)
        {
            item.GetChild(3).gameObject.SetActive(false);
        }
        foreach (Transform item in itemField)
        {
            item.GetChild(3).gameObject.SetActive(false);
        }
        checkedBtn = null;
    }

    //버튼들에 채크 이벤트 추가
    private void AddClickEventItemToCheck()
    {
        foreach (Transform t in relicField)
        {
            Button btn = t.GetComponent<Button>();
            if (btn == null) continue;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => ClickItemAndCheck(btn));
        }

        foreach (Transform t in itemField)
        {
            Button btn = t.GetComponent<Button>();
            if (btn == null) continue;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => ClickItemAndCheck(btn));
        }
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
                UIManager.Instance.UpdateReroll();
                break;
        }

        SoldOutItemBtn(btn);
    }

    private void ApplyEnergyItem(StoreItemData item, Button btn)
    {
        if (item.form == "Select")
        {
            List<RogueUnitDataBase> selected = RogueLikeData.Instance.GetSelectedUnits();
            if (selected == null || selected.Count < item.count)
            {
                List<RogueUnitDataBase> canSelectUnits = RogueLikeData.Instance.GetMyTeam()
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
            List<RogueUnitDataBase> units = RogueLikeData.Instance.GetMyTeam().Where(u => u.energy < u.maxEnergy).ToList();
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

    //유닛 구매
    public void ClickUnitPackage(UnitPackageUI unitPackageUI,List<RogueUnitDataBase> units,int price)
    {
        //패키지 누르면 구매하기 버튼 (돈이 안되면 구매 버튼 상호작용 불가,눌르면 해당 패키지 유닛들 비활성화, 구매)
        //나가기 버튼 패키지 되돌리기 함수 + 배경 비활성화
        //금화 가격으로 세팅
        leavePackageBtn.onClick.RemoveAllListeners();
        leavePackageBtn.onClick.AddListener(() => ClickLeavePackageBtn(unitPackageUI));

        packageGoldText.text = $"{price}";

        int gold = RogueLikeData.Instance.GetCurrentGold();
        purchasePackageBtn.onClick.RemoveAllListeners();
        if (gold < price)
        {
            purchasePackageBtn.interactable = false; 
        }
        else purchasePackageBtn.interactable=true;

        purchasePackageBtn.onClick.AddListener(() => PurchaseUnitPackage(unitPackageUI.gameObject, units, price));

    }
    private void ClickLeavePackageBtn(UnitPackageUI unitPackageUI)
    {
        unitPackageUI.ReturnUnitPackage();
        AnimatePackageBackFalse();
        foreach (Transform i in unitPackage)
        {
            UnitPackageUI ui = i.GetComponent<UnitPackageUI>();
            if (ui != null)
            {
                ui.UpdateUnitPackage();
            }
        }

    }

    public void ClosePackageBack()
    {
        packagePanel.SetActive(false);
    }



    //페키지 배경 설정
    public void AnimatePackageBackTrue()
    {
        PackagePanelChildDisActive();
        packagePanel.SetActive(true);
        HidePurchaseLeaveBtn();
        packagePanel.transform.SetAsLastSibling();
        Image backImg = packagePanel.transform.Find("Backgrond")?.GetComponent<Image>();
        UnityEngine.Color startColor = backImg.color;
        startColor.a = 0f;
        backImg.color = startColor;
        backImg.gameObject.SetActive(true);
        // 0.5초 동안 알파값을 1로 변경 (불투명하게)
        backImg.DOFade(0.95f, 0.5f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            PackagePanelChildActive();
        });

    }
    public void AnimatePackageBackFalse()
    {
        PackagePanelChildDisActive();
        packagePanel.SetActive(true);
        Image backImg = packagePanel.transform.Find("Backgrond")?.GetComponent<Image>();
        UnityEngine.Color startColor = backImg.color;
        startColor.a = 1f;
        backImg.color = startColor;
        backImg.gameObject.SetActive(true);
        // 0.5초 동안 알파값을 1로 변경 (불투명하게)
        backImg.DOFade(0f, 0.5f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            backImg.gameObject.SetActive(false);
        });

    }
    private void PackagePanelChildDisActive()
    {
        foreach(Transform child in packagePanel.transform)
        {
            child.gameObject.SetActive(false);  
        }

    }
    private void PackagePanelChildActive()
    {
        foreach (Transform child in packagePanel.transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    private void SoldOutItemBtn(Button btn)
    {
        btn.transform.GetChild(2).gameObject.SetActive(true);
        btn.interactable = false;
        btn.onClick.RemoveAllListeners();
    }

    public void HidePurchaseLeaveBtn()
    {
        purchaseBtn.gameObject.SetActive(false);
        leaveBtn.gameObject.SetActive(false);
    }
    public void VisiblePurchaseLeaveBtn()
    {
        purchaseBtn.gameObject.SetActive(true);
        leaveBtn.gameObject.SetActive(false);
    }

}