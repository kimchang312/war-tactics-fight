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
        RestUI();

        // 현재 위치/챕터 가져오기(네가 쓰는 방식에 맞게)
        int chapter = RogueLikeData.Instance.GetChapter();
        var (x, y, _) = RogueLikeData.Instance.GetCurrentStage();

        // 최초 진입이면 롤 함수로 스냅샷 생성, 아니면 기존 스냅샷 반환
        var snap = RogueLikeData.Instance.OpenShopAndFreezeIfNeeded(
            chapter, x, y,
            RollUnitPacksOnce,
            RollRelicsOnce,
            RollItemsOnce,
            RollRerollOnce
        );

        // 스냅샷을 UI로 바인딩
        BindUnitPackages(snap);
        BindRelics(snap);
        BindItems(snap);
        BindReroll(snap);
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

            int relicId = candidates[RogueLikeData.Instance.GetRandomInt(0, candidates.Count)];
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

            int rand = RogueLikeData.Instance.GetRandomInt(0, filtered.Count);
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
        itemInformation.data.isItem = true;
        itemInformation.data.item = storeItemData;
        itemInformation.data.price = price;
        if (units != null) itemInformation.data.units = units;
        else if (relicId != -1) itemInformation.data.relicId = relicId;
        else if (rerollCount != 0) itemInformation.data.rerollCount = rerollCount;
    }

    // 사용처: 패키지 결제/소유 반영/스냅샷 판매 잠금
    private void PurchaseUnitPackage(GameObject obj, List<RogueUnitDataBase> units, int price)
    {
        // 사전 검증(금화 UI 체크는 되어 있지만 한번 더)
        int gold = RogueLikeData.Instance.GetCurrentGold();
        int lental = RelicManager.CheckRelicById(49) ? 500 : 0;
        if (gold + lental < price) return;

        int slotIndex = obj.transform.GetSiblingIndex();

        // 판매 잠금(중복 방지)
        if (!RogueLikeData.Instance.TryMarkSold(StoreSlotType.UnitPackage, slotIndex))
            return;

        // 금화 차감 실패 시 롤백
        if (!SpendGold(price))
        {
            RogueLikeData.Instance.UnmarkSold(StoreSlotType.UnitPackage, slotIndex);
            return;
        }

        var myUnits = RogueLikeData.Instance.GetMyTeam();
        myUnits.AddRange(units);
        RogueLikeData.Instance.SetMyTeam(myUnits);

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

    // 사용처: 체크된 슬롯을 구매 버튼으로 결제
    private void ClickPurchaseItemBtn()
    {
        if (checkedBtn == null) return;

        // 체크된 버튼이 어느 영역인지 판별
        StoreSlotType type;
        int slotIndex;

        if (checkedBtn.transform.IsChildOf(relicField))
        {
            type = StoreSlotType.Relic;
            slotIndex = checkedBtn.transform.GetSiblingIndex();
        }
        else if (checkedBtn.transform.IsChildOf(itemField))
        {
            type = StoreSlotType.Item;
            slotIndex = checkedBtn.transform.GetSiblingIndex();
        }
        else if (checkedBtn.transform == rerollObject)
        {
            type = StoreSlotType.Reroll;
            slotIndex = 0;
        }
        else
        {
            return;
        }

        ItemInformation info = checkedBtn.GetComponent<ItemInformation>();
        if (info == null) return;

        int price = info.data.price;

        // 판매 잠금 선 수행(중복 클릭 방지)
        if (!RogueLikeData.Instance.TryMarkSold(type, slotIndex))
            return;

        // 결제 실패 시 롤백
        if (!SpendGold(price))
        {
            RogueLikeData.Instance.UnmarkSold(type, slotIndex);
            return;
        }

        // 효과 적용
        if (info.data.relicId != -1)
        {
            RogueLikeData.Instance.AcquireRelic(info.data.relicId);
        }
        else if (info.data.item != null)
        {
            switch (info.data.item.type)
            {
                case "Energy":
                    ApplyEnergyItem(info.data.item, checkedBtn);
                    break;
                case "Morale":
                    RogueLikeData.Instance.ChangeMorale(int.Parse(info.data.item.value));
                    break;
                case "Reroll":
                    RogueLikeData.Instance.AddReroll(info.data.item.count);
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
                int r = RogueLikeData.Instance.GetRandomInt(i, units.Count);
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
        leaveBtn.gameObject.SetActive(true);
    }
    // 사용처: 최초 입장 시 1회 라인업 동결용
    private List<UnitPackageOffer> RollUnitPacksOnce()
    {
        var unitItems = StoreManager.GetRandomUnitItems();
        var result = new List<UnitPackageOffer>(unitItems.Count);

        for (int i = 0; i < unitItems.Count; i++)
        {
            var item = unitItems[i];
            var units = FilterAndSelectUnits(item);
            int price = CalculateUnitPackagePrice(units, item);
            // idx만 저장해 직렬화 비용 절감
            var idxs = new int[units.Count];
            for (int k = 0; k < units.Count; k++) idxs[k] = units[k].idx;

            result.Add(new UnitPackageOffer { unitIdxs = idxs, price = price, sold = false });
        }
        return result;
    }

    private List<SimpleOffer> RollRelicsOnce()
    {
        var items = StoreManager.GetRandomRelicItems();
        var used = new HashSet<int>();
        var result = new List<SimpleOffer>(items.Count);

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            int grade = int.Parse(item.value);
            var candidates = RelicManager.GetAvailableRelicIds(grade, RelicManager.RelicAction.Acquire)
                                         .Where(id => !used.Contains(id)).ToList();
            if (candidates.Count == 0)
            {
                result.Add(new SimpleOffer { id = -1, price = 0, sold = true });
                continue;
            }
            int relicId = candidates[RogueLikeData.Instance.GetRandomInt(0, candidates.Count)];
            used.Add(relicId);
            int price = CalculateDiscountedPrice(item);
            result.Add(new SimpleOffer { id = relicId, price = price, sold = false });
        }
        return result;
    }

    private List<SimpleOffer> RollItemsOnce()
    {
        var items = StoreManager.GetRandomEnergyMoraleItems();
        var result = new List<SimpleOffer>(items.Count);
        for (int i = 0; i < items.Count; i++)
        {
            var it = items[i];
            int price = CalculateDiscountedPrice(it);
            result.Add(new SimpleOffer { id = it.itemId, price = price, sold = false });
        }
        return result;
    }

    private SimpleOffer RollRerollOnce()
    {
        var r = StoreManager.GetRandomDiceItem()[0];
        return new SimpleOffer { id = r.itemId, price = CalculateDiscountedPrice(r), sold = false };
    }
    // 사용처: 유닛 패키지 슬롯 바인딩
    private void BindUnitPackages(StoreSnapshot snap)
    {
        if (snap.unitPacks == null) return;
        int childCount = unitPackage.childCount;
        int n = Mathf.Min(childCount, snap.unitPacks.Count);

        for (int i = 0; i < childCount; i++)
        {
            var slot = unitPackage.GetChild(i).GetComponent<UnitPackageUI>();
            if (i >= n)
            {
                slot.gameObject.SetActive(false);
                continue;
            }

            var offer = snap.unitPacks[i];

            // idx → Unit 복원(캐시 기준 최소 연산)
            var units = new List<RogueUnitDataBase>(offer.unitIdxs.Length);
            for (int k = 0; k < offer.unitIdxs.Length; k++)
            {
                var u = UnitLoader.Instance.GetCloneUnitById(offer.unitIdxs[k]);
                if (u != null) units.Add(u);
            }

            slot.SetUnitPackage(units, null, offer.price);
            slot.gameObject.SetActive(!offer.sold);
        }
    }

    // 사용처: 유물 슬롯 바인딩
    private void BindRelics(StoreSnapshot snap)
    {
        int childCount = relicParent.childCount;
        int n = Mathf.Min(childCount, snap.relics.Count);

        for (int i = 0; i < childCount; i++)
        {
            var t = relicParent.GetChild(i);
            var btn = t.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();

            if (i >= n)
            {
                t.gameObject.SetActive(false);
                continue;
            }

            var offer = snap.relics[i];
            if (offer.id <= 0) { t.gameObject.SetActive(false); continue; }

            // StoreItemData는 없어도 됨(정보 UI만 필요하다면 룩업 가능)
            SetRelicUI(t, null, offer.id, offer.price);

            if (offer.sold) SoldOutItemBtn(btn);
            else
            {
                // 체크 선택 → 구매 버튼으로 구매
                btn.onClick.AddListener(() => ClickItemAndCheck(btn));
            }
        }
    }

    // 사용처: 일반 아이템 슬롯 바인딩
    private void BindItems(StoreSnapshot snap)
    {
        int childCount = itemParent.childCount;
        int n = Mathf.Min(childCount, snap.items.Count);

        for (int i = 0; i < childCount; i++)
        {
            var t = itemParent.GetChild(i);
            var btn = t.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();

            if (i >= n)
            {
                t.gameObject.SetActive(false);
                continue;
            }

            var offer = snap.items[i];
            var data = StoreManager.GetItemDataById(offer.id);
            if (data == null) { t.gameObject.SetActive(false); continue; }

            string path = $"ItemImages/Item{data.itemId}";
            SetStoreSlotUI(t, data, offer.price, path, null);

            if (offer.sold) SoldOutItemBtn(btn);
            else btn.onClick.AddListener(() => ClickItemAndCheck(btn));
        }
    }

    // 사용처: 리롤 슬롯 바인딩
    private void BindReroll(StoreSnapshot snap)
    {
        if (rerollObject == null) return;
        var btn = rerollObject.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();

        var offer = snap.reroll;
        if (offer == null) { rerollObject.gameObject.SetActive(false); return; }

        var data = StoreManager.GetItemDataById(offer.id);
        if (data == null) { rerollObject.gameObject.SetActive(false); return; }

        SetStoreSlotUI(rerollObject, data, offer.price, "ItemImages/Item60", null, int.Parse(data.value));

        if (offer.sold) SoldOutItemBtn(btn);
        else btn.onClick.AddListener(() => ClickItemAndCheck(btn));
    }

}