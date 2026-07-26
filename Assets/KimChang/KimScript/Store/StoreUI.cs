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
    [SerializeField] private UnitListUI unitListUI;
    [SerializeField] private Transform unitPackage;

    [SerializeField] private GameObject packagePanel;

    [SerializeField] private Button purchasePackageBtn;
    [SerializeField] private Button leavePackageBtn;
    [SerializeField] private TextMeshProUGUI packageGoldText;
    [SerializeField] private Transform relicField;
    [SerializeField] private Transform itemField;

    [SerializeField] private LineUpBar lineUpBar;

    private List<StoreItemData> cachedUnitItems;
    private List<List<RogueUnitDataBase>> cachedUnitPackages;
    private List<StoreItemData> cachedRelicItems;
    private List<int> cachedRelicIds;
    private List<StoreItemData> cachedItemItems;
    private StoreItemData cachedRerollItem;
    private Button checkedBtn;

    private const float aniTime = 0.5f;

    private void Awake()
    {
        EnsureUnitListUI();
    }

    private void OnEnable()
    {
        ResetUI();

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
    private void ResetUI()
    {
        leaveBtn.onClick.RemoveAllListeners();
        leaveBtn.onClick.AddListener(CloseStore);

        leavePackageBtn.onClick.RemoveAllListeners();

        purchaseBtn.onClick.RemoveAllListeners();
        purchaseBtn.onClick.AddListener(ClickPurchaseItemBtn);


        RogueLikeData.Instance.SetSelectedUnits(new List<RogueUnitDataBase>());
        if (EnsureUnitListUI())
            unitListUI.gameObject.SetActive(false);
        ClosePackageBack();
        UnCheckAllItem();
        AddClickEventItemToCheck();
        SetStoreMainButtonsInteractable(true);
    }

    private bool EnsureUnitListUI()
    {
        if (unitListUI != null)
            return true;

        if (GameManager.Instance != null && GameManager.Instance.unitListUI != null)
        {
            unitListUI = GameManager.Instance.unitListUI;
            return true;
        }

        unitListUI = FindObjectOfType<UnitListUI>(true);
        if (unitListUI != null)
            return true;

        Debug.LogError("[StoreUI] UnitListUI 참조를 찾을 수 없습니다.");
        return false;
    }

    private void CloseStore()
    {
        RogueLikeData.Instance.ClearStoreOpenState();
        RogueLikeData.Instance.SaveNow();
        gameObject.SetActive(false);
        GameManager.Instance.RefreshNodeInfoButtonVisibility();
    }

    private float GetSaleRatio() => RogueLikeData.Instance.GetOwnedRelicById(0) != null ? 0.8f : 1f;

    private int CalculateDiscountedPrice(StoreItemData item)
    {
        int cost = (int)(item.price * StoreManager.GetRandomBetweenValue(item.priceRateMin, item.priceRateMax));
        float sale = 1;
        if (RelicManager.CheckRelicById(0))
        {
            WarRelic discountCoupon = RelicManager.GetRelicById(0);
            var vals = discountCoupon.GetAllValuesAsFloatListOrNull();
            if (vals != null)
            {
                sale += vals[0];
            }
        }
        if (RelicManager.CheckRelicById(58))
        {
            WarRelic evidenceOfEmbezzlement = RelicManager.GetRelicById(58);
            var vals = evidenceOfEmbezzlement.GetAllValuesAsFloatListOrNull();
            if (vals != null)
            {
                sale += vals[0];
            }
        }
        cost = (int)(cost * sale);
        return cost;
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

        if (onClick != null)
            btn.onClick.AddListener(() => onClick());
        else
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

        if (RelicManager.CheckRelicById(111))
            filtered.RemoveAll(unit => unit != null && unit.rarity == 1);

        List<RogueUnitDataBase> result = new();
        for (int i = 0; i < item.count; i++)
        {
            if (filtered.Count == 0) break;

            int rand = RogueLikeData.Instance.GetRandomInt(0, filtered.Count);
            RogueUnitDataBase baseUnit = filtered[rand];
            RogueUnitDataBase unit = UnitLoader.Instance.GetCloneUnitById(baseUnit.idx);

            unit.Energy = Math.Max(1, (int)((unit.Energy * item.price) * 0.01f));
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
        bool canSpend = RogueLikeData.Instance.CanSpendGold(price);
        if (btn.interactable != canSpend)
        {
            btn.interactable = canSpend;
        }
        return canSpend;
    }
    // 사용처: 상점 결제 시 금화만 차감
    private bool SpendGold(int cost)
    {
        return RogueLikeData.Instance.ReduceGold(cost);
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
        itemInformation.data.isRelic = false;
        itemInformation.data.units = null;
        itemInformation.data.rerollCount = 0;

        if (units != null)
        {
            itemInformation.data.units = units;
        }
        else if (relicId != -1)
        {
            itemInformation.data.relicId = relicId;
            itemInformation.data.isRelic = true;
        }
        else if (rerollCount != 0)
        {
            itemInformation.data.rerollCount = rerollCount;
        }

    }
    // 사용처: 패키지 구매 최종 처리
    private void PurchaseUnitPackage(int slotIndex, UnitPackageUI unitPackageUI, List<RogueUnitDataBase> units, int price)
    {
        if (!RogueLikeData.Instance.CanSpendGold(price))
            return;

        if (!RogueLikeData.Instance.TryMarkSold(StoreSlotType.UnitPackage, slotIndex))
            return;

        if (!SpendGold(price))
        {
            RogueLikeData.Instance.UnmarkSold(StoreSlotType.UnitPackage, slotIndex);
            return;
        }

        for (int i = 0; i < units.Count; i++)
        {
            RogueLikeData.Instance.AddMyTeam(units[i]);
        }

        RogueLikeData.Instance.SaveNow();

        unitPackageUI.gameObject.SetActive(false);
        if (lineUpBar != null)
        {
            lineUpBar.RefreshUnitList();
        }
        ClosePackageBack();
    }

    // 사용처: 체크된 슬롯을 구매 버튼으로 결제
    private void ClickPurchaseItemBtn()
    {
        if (checkedBtn == null) return;

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

        if (type == StoreSlotType.Item &&
            info.data.item != null &&
            info.data.item.type == "Energy" &&
            info.data.item.form == "Select")
        {
            OpenEnergySelectPurchase(type, slotIndex, checkedBtn, info.data.item, price);
            return;
        }

        if (info.data.isRelic && info.data.relicId == 79)
        {
            OpenStrangePiecePurchase(type, slotIndex, checkedBtn, price);
            return;
        }

        if (!RogueLikeData.Instance.TryMarkSold(type, slotIndex))
            return;

        if (info.data.isRelic)
        {
            if (!RelicManager.AcquireRelic(info.data.relicId))
            {
                RogueLikeData.Instance.UnmarkSold(type, slotIndex);
                Debug.LogError($"[StoreUI] 유물 획득 실패 relicId={info.data.relicId}");
                return;
            }

            if (!SpendGold(price))
            {
                RogueLikeData.Instance.RemoveRelicById(info.data.relicId);
                RogueLikeData.Instance.UnmarkSold(type, slotIndex);
                return;
            }
        }
        else
        {
            if (!SpendGold(price))
            {
                RogueLikeData.Instance.UnmarkSold(type, slotIndex);
                return;
            }

            if (info.data.item != null)
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
        }

        RogueLikeData.Instance.SaveNow();

        checkedBtn.transform.GetChild(2).gameObject.SetActive(true);
        checkedBtn.transform.GetChild(3).gameObject.SetActive(false);
        checkedBtn.interactable = false;
        checkedBtn.onClick.RemoveAllListeners();
        checkedBtn = null;
    }


    //아이템 클릭 시 
    private void OpenStrangePiecePurchase(StoreSlotType type, int slotIndex, Button btn, int price)
    {
        if (!RogueLikeData.Instance.CanSpendGold(price))
            return;

        if (!EnsureUnitListUI())
            return;

        List<RogueUnitDataBase> candidates = RogueLikeData.Instance.GetMyTeam()?.FindAll(unit => unit != null) ?? new List<RogueUnitDataBase>();
        if (candidates == null || candidates.Count == 0)
            return;

        SetStoreMainButtonsInteractable(false);
        RogueLikeData.Instance.SetSelectedUnits(new List<RogueUnitDataBase>());

        unitListUI.Show(
            1,
            candidates,
            () => CompleteStrangePiecePurchase(type, slotIndex, btn, price),
            () => SetStoreMainButtonsInteractable(true)
        );
    }

    private void CompleteStrangePiecePurchase(StoreSlotType type, int slotIndex, Button btn, int price)
    {
        if (!RogueLikeData.Instance.TryMarkSold(type, slotIndex))
        {
            SetStoreMainButtonsInteractable(true);
            return;
        }

        if (!RelicManager.AcquireRelic(79))
        {
            RogueLikeData.Instance.UnmarkSold(type, slotIndex);
            SetStoreMainButtonsInteractable(true);
            Debug.LogError("[StoreUI] Failed to acquire relicId=79");
            return;
        }

        if (!SpendGold(price))
        {
            RogueLikeData.Instance.RemoveRelicById(79);
            RogueLikeData.Instance.UnmarkSold(type, slotIndex);
            SetStoreMainButtonsInteractable(true);
            return;
        }

        List<RogueUnitDataBase> selectedUnits = RogueLikeData.Instance.GetSelectedUnits() ?? new List<RogueUnitDataBase>();
        for (int i = 0; i < selectedUnits.Count; i++)
        {
            if (selectedUnits[i] != null)
                selectedUnits[i].endless = true;
        }
        RogueLikeData.Instance.ClearSelectedUnis();

        RogueLikeData.Instance.SaveNow();

        if (btn != null)
        {
            SoldOutItemBtn(btn);
            if (btn.transform.childCount > 3)
                btn.transform.GetChild(3).gameObject.SetActive(false);
        }

        if (checkedBtn == btn)
            checkedBtn = null;

        SetStoreMainButtonsInteractable(true);

        if (lineUpBar != null)
            lineUpBar.RefreshUnitList();
    }

    private void ClickItemAndCheck(Button btn)
    {
        if (!btn.interactable) return;

        UnCheckAllItem();

        ItemInformation info = btn.GetComponent<ItemInformation>();
        if (info != null && info.data.isItem)
        {
            btn.transform.GetChild(3).gameObject.SetActive(true); // 체크 표시

            checkedBtn = btn;
        }
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


    // 사용처: 선택형 기력 아이템은 유닛 선택이 끝난 뒤에만 결제/판매/저장 처리
    private void OpenEnergySelectPurchase(StoreSlotType type, int slotIndex, Button btn, StoreItemData item, int price)
    {
        if (!EnsureUnitListUI())
            return;

        List<RogueUnitDataBase> canSelect = RogueLikeData.Instance.GetMyTeam() ?? new List<RogueUnitDataBase>();
        List<RogueUnitDataBase> filtered = new List<RogueUnitDataBase>(canSelect.Count);

        for (int i = 0; i < canSelect.Count; i++)
        {
            RogueUnitDataBase unit = canSelect[i];
            if (unit != null && unit.Energy < unit.MaxEnergy)
                filtered.Add(unit);
        }

        if (filtered.Count < item.count)
        {
            Debug.LogWarning($"[StoreUI] 기력 회복 아이템을 적용할 유닛이 부족합니다. itemId={item.itemId}, required={item.count}, candidates={filtered.Count}");
            return;
        }

        SetStoreMainButtonsInteractable(false);
        RogueLikeData.Instance.SetSelectedUnits(new List<RogueUnitDataBase>());

        unitListUI.Show(
            item.count,
            filtered,
            () => CompleteEnergySelectPurchase(type, slotIndex, btn, item, price),
            () => SetStoreMainButtonsInteractable(true)
        );
    }

    // 사용처: 선택형 기력 아이템의 선택 완료 후 실제 구매를 확정
    private void CompleteEnergySelectPurchase(StoreSlotType type, int slotIndex, Button btn, StoreItemData item, int price)
    {
        if (!RogueLikeData.Instance.TryMarkSold(type, slotIndex))
        {
            SetStoreMainButtonsInteractable(true);
            return;
        }

        if (!SpendGold(price))
        {
            RogueLikeData.Instance.UnmarkSold(type, slotIndex);
            SetStoreMainButtonsInteractable(true);
            return;
        }

        ApplyEnergyToSelectedUnits(item);
        SoldOutItemBtn(btn);

        if (btn != null && btn.transform.childCount > 3)
            btn.transform.GetChild(3).gameObject.SetActive(false);

        if (checkedBtn == btn)
            checkedBtn = null;

        RogueLikeData.Instance.SaveNow();
        SetStoreMainButtonsInteractable(true);

        if (lineUpBar != null)
            lineUpBar.RefreshUnitList();
    }

    // 사용처: 선택형 기력 아이템 구매 확정 후 선택된 유닛에게 회복 적용
    private void ApplyEnergyToSelectedUnits(StoreItemData item)
    {
        int amount = int.TryParse(item.value, out var parsed) ? parsed : 0;
        List<RogueUnitDataBase> selectedUnits = RogueLikeData.Instance.GetSelectedUnits() ?? new List<RogueUnitDataBase>();

        for (int i = 0; i < selectedUnits.Count; i++)
        {
            RogueUnitDataBase unit = selectedUnits[i];
            if (unit == null) continue;
            unit.Energy = Math.Min(unit.MaxEnergy, unit.Energy + amount);
        }

        RogueLikeData.Instance.SetSelectedUnits(new List<RogueUnitDataBase>());
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
        // 사용처: 선택형 기력 아이템에서 유닛 선택 UI 오픈
        if (item.form == "Select")
        {
            if (!EnsureUnitListUI())
                return;

            List<RogueUnitDataBase> canSelect = RogueLikeData.Instance.GetMyTeam() ?? new List<RogueUnitDataBase>();
            var filtered = new List<RogueUnitDataBase>(canSelect.Count);

            for (int i = 0; i < canSelect.Count; i++)
            {
                var u = canSelect[i];
                if (u != null && u.Energy < u.MaxEnergy)
                    filtered.Add(u);
            }

            if (filtered.Count < item.count)
            {
                Debug.LogWarning($"[StoreUI] 선택형 기력 아이템을 적용할 유닛이 부족합니다. itemId={item.itemId}, required={item.count}, candidates={filtered.Count}");
                return;
            }

            SetStoreMainButtonsInteractable(false);

            unitListUI.Show(
                item.count,
                filtered,
                () =>
                {
                    ApplyEnergyToSelectedUnits(item);
                    RogueLikeData.Instance.SaveNow();
                    SetStoreMainButtonsInteractable(true);

                    if (lineUpBar != null)
                        lineUpBar.RefreshUnitList();
                },
                () => SetStoreMainButtonsInteractable(true)
            );

            return;
        }
        else if (item.form == "Random")
        {
            int amount = int.Parse(item.value);
            var units = (RogueLikeData.Instance.GetMyTeam() ?? new List<RogueUnitDataBase>())
                .Where(u => u != null && u.Energy < u.MaxEnergy)
                .ToList();
            if (units.Count == 0) return;

            for (int i = 0; i < units.Count; i++)
            {
                int r = RogueLikeData.Instance.GetRandomInt(i, units.Count);
                (units[i], units[r]) = (units[r], units[i]);
            }
            for (int i = 0; i < Mathf.Min(item.count, units.Count); i++)
            {
                var u = units[i];
                u.Energy = Math.Min(u.MaxEnergy, u.Energy + amount);
            }
        }
    }

    // 사용처: 패키지 클릭 시 구매 버튼에 정확한 스냅샷 인덱스 연결
    public void ClickUnitPackage(UnitPackageUI unitPackageUI, List<RogueUnitDataBase> units, int price)
    {
        leavePackageBtn.onClick.RemoveAllListeners();
        leavePackageBtn.onClick.AddListener(() => ClickLeavePackageBtn(unitPackageUI));

        packageGoldText.text = $"{price}";
        purchasePackageBtn.onClick.RemoveAllListeners();

        bool canSpend = RogueLikeData.Instance.CanSpendGold(price);
        purchasePackageBtn.interactable = canSpend;

        int slotIndex = GetUnitPackageSlotIndex(unitPackageUI);
        if (slotIndex < 0)
        {
            purchasePackageBtn.interactable = false;
            Debug.LogError("[StoreUI] 패키지 슬롯 인덱스를 찾지 못함");
            return;
        }

        purchasePackageBtn.onClick.AddListener(() => PurchaseUnitPackage(slotIndex, unitPackageUI, units, price));
    }

    // 사용처: 패키지 UI를 스냅샷 unitPacks 인덱스로 변환
    private int GetUnitPackageSlotIndex(UnitPackageUI target)
    {
        int slotIndex = 0;

        for (int i = 0; i < unitPackage.childCount; i++)
        {
            UnitPackageUI ui = unitPackage.GetChild(i).GetComponent<UnitPackageUI>();
            if (ui == null)
                continue;

            if (ui == target)
                return slotIndex;

            slotIndex++;
        }

        return -1;
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
        VisiblePurchaseLeaveBtn();
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
        foreach (Transform child in packagePanel.transform)
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
        if (snap == null || snap.unitPacks == null) return;

        // UI 슬롯 수집
        int childCount = unitPackage.childCount;
        var slots = new List<UnitPackageUI>(childCount);
        for (int i = 0; i < childCount; i++)
        {
            var comp = unitPackage.GetChild(i).GetComponent<UnitPackageUI>();
            if (comp != null) slots.Add(comp);
        }

        int slotCount = slots.Count;
        int packCount = snap.unitPacks.Count;
        int n = slotCount < packCount ? slotCount : packCount;

        // 사용처: UI 텍스트 TitleKey (전역 X, 이 메서드 한정)
        // TK_PACK_NAME  : "{0} 외 {1}종" 같은 포맷 문자열
        // TK_PACK_EMPTY : "빈 패키지"
        const int TK_PACK_NAME = 1001;
        const int TK_PACK_EMPTY = 1002;

        for (int i = 0; i < slotCount; i++)
        {
            var slot = slots[i];

            if (i >= n)
            {
                if (slot.gameObject.activeSelf) slot.gameObject.SetActive(false);
                continue;
            }

            var offer = snap.unitPacks[i];

            // 사용처: idx 목록으로 유닛 복원
            var ids = offer.unitIdxs;
            int idLen = (ids != null) ? ids.Length : 0;
            var units = new List<RogueUnitDataBase>(idLen);
            for (int k = 0; k < idLen; k++)
            {
                var u = UnitLoader.Instance.GetCloneUnitById(ids[k]);
                if (u != null) units.Add(u);
            }

            // 사용처: 패키지명 구성
            string packName;
            int count = units.Count;

            if (count <= 0)
            {
                // 빈 패키지: UI/TK_PACK_EMPTY 조회 → 없으면 기본값
                string s = GameTextDB.GetByTitleKey(TextKind.UI, TK_PACK_EMPTY);
                packName = string.IsNullOrEmpty(s) ? "빈 패키지" : s;
            }
            else if (count == 1)
            {
                // 유닛 1개: 이름만
                packName = units[0].unitName;
            }
            else
            {
                // 유닛 2개+: 포맷 조회 → 없으면 기본 규칙
                string fmt = GameTextDB.GetByTitleKey(TextKind.UI, TK_PACK_NAME);
                int etc = count - 1;
                packName = string.IsNullOrEmpty(fmt)
                    ? (units[0].unitName + " 외 " + etc + "종")
                    : string.Format(fmt, units[0].unitName, etc);
            }

            // 사용처: 슬롯에 바인딩할 최소 아이템 정보(Null 방지)
            var fakeItem = new StoreItemData
            {
                itemId = -1,
                itemName = packName,
                price = offer.price,
                priceRateMin = 1f,
                priceRateMax = 1f,
                rarity = 0,
                type = "Unit",
                form = "Fixed",
                value = "",
                count = count,
                condition = "",
                description = packName
            };

            slot.SetUnitPackage(units, fakeItem, offer.price);

            bool active = !offer.sold;
            if (slot.gameObject.activeSelf != active)
                slot.gameObject.SetActive(active);
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

    // 사용처: 유닛 선택 UI가 열려 있는 동안 상점 구매/떠나기 버튼 잠금
    private void SetStoreMainButtonsInteractable(bool interactable)
    {
        if (purchaseBtn != null)
            purchaseBtn.interactable = interactable;

        if (leaveBtn != null)
            leaveBtn.interactable = interactable;
    }

}
