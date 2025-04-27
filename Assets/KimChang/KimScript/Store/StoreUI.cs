using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class StoreUI : MonoBehaviour
{
    [SerializeField] private Button leaveBtn;
    [SerializeField] private Transform unitParent;
    [SerializeField] private Transform relicParent;
    [SerializeField] private Transform itemParent;
    [SerializeField] private Transform rerollObject;

    private void Awake()
    {
        StoreManager.LoadStoreData();
    }

    private void OnEnable()
    {

        ShowRelicUI();
        ShowItemUI();
        

    }

    private void ShowUnitUI()
    {
        var items = StoreManager.GetRandomUnitItems();
        int price = 0;
        for (int i = 0; i < items.Count; i++)
        {
            List<RogueUnitDataBase> units = new();
            List<int> rows = new();
            Transform child = unitParent.GetChild(i);
            if (items[i].form == "Rarity")
            {
                var (min, max) = EventManager.ParseRange(items[i].value);
                var sellUnits = GoogleSheetLoader.Instance.GetAllUnitsAsObject();

                // 조건에 맞는 유닛 필터링
                List<RogueUnitDataBase> filtered = sellUnits
                    .Where(u => u.rarity >= min && u.rarity <= max)
                    .ToList();
    
                int count = items[i].count;
                for (int j = 0; j < count; j++)
                {
                    int idx = UnityEngine.Random.Range(0, filtered.Count);
                    rows.Add(idx); // 중복 허용
                }

            }
            else if (items[i].form == "Branch")
            {
                if (!int.TryParse(items[i].value, out int branch)) return;

                var allUnits = GoogleSheetLoader.Instance.GetAllUnitsAsObject();
                var filtered = allUnits.Where(u => u.branchIdx == branch).ToList();

                for (int j = 0; j < items[i].count; j++)
                {
                    int idx = UnityEngine.Random.Range(0, filtered.Count);
                    rows.Add(idx);
                }
            }
            else if (items[i].form == "Tag")
            {
                var tag = int.Parse(items[i].value);

                var allUnits = GoogleSheetLoader.Instance.GetAllUnitsAsObject();
                var filtered = allUnits.Where(u => u.tagIdx == tag).ToList();

                for (int j = 0; j < items[i].count; j++)
                {
                    int idx = UnityEngine.Random.Range(0, filtered.Count);
                    rows.Add(idx);
                }
            }

            foreach (int row in rows)
            {
                RogueUnitDataBase unit = RogueUnitDataBase.ConvertToUnitDataBase(GoogleSheetLoader.Instance.GetRowUnitData(row));
                unit.energy = (int)((unit.energy * items[i].price) * 0.01f);
                price += unit.unitPrice;

                units.Add(unit);

            }
            price = (int)(price*StoreManager.GetRandomBetweenValue(items[i].priceRateMin, items[i].priceRateMax));
            Button btn = child.GetComponent<Button>();
            BtnIteractable(btn, price);
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => PurchaseUnitPackage(units, price));

        }
    }

    private void ShowRelicUI()
    {
        var items = StoreManager.GetRandomRelicItems();
        List<int> relicIds = new List<int>();

        for (int i = 0; i < items.Count; i++)
        {
            int relicId;

            // 중복되지 않는 relicId가 나올 때까지 반복
            int safetyCount = 0;
            do
            {
                // 등급 0 = 저주 유산 등급 (itemId 80, 81은 등급이 정해진 것이라면 그에 맞게 수정 필요)
                relicId = RelicManager.GetRandomRelicId(0, RelicManager.RelicAction.Acquire);
                safetyCount++;

                if (safetyCount > 100)
                {
                    Debug.LogWarning("Relic 중복 제거 실패: 최대 반복 초과");
                    relicId = -1;
                    break;
                }
            } while (relicIds.Contains(relicId));

            relicIds.Add(relicId);

            // UI에 이미지 표시
            Transform child = relicParent.GetChild(i);
            Image img = child.GetComponent<Image>();
            img.sprite = SpriteCacheManager.GetSprite($"KIcon/WarRelic/{relicId}");

            // 가격 표시
            TextMeshProUGUI costText = child.GetChild(0).GetComponent<TextMeshProUGUI>();
            int cost =(int)(items[i].price * StoreManager.GetRandomBetweenValue(items[i].priceRateMin, items[i].priceRateMax));
            costText.text = $"{cost}";

            Button btn = child.GetComponent<Button>();
            BtnIteractable(btn, cost);

            child.name = $"{relicId}";
            //눌렀을때 유산id값을 바탕으로 구매 & 내 골드 감소
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(()=>PurchaseRelic(relicId,cost));

        }
    }



    private void ShowItemUI()
    {
        var items = StoreManager.GetRandomEnergyMoraleItems();
        for (int i = 0; i < items.Count; i++)
        {
            // 아이템 오브젝트 가져오기
            Transform child = itemParent.GetChild(i);
            ShowStoreImagePrice(items[i], child, 3);
            //눌렀을때 아이템id값을 바탕으로 구매 & 내 골드 감소

        }
    }

    private void RestUI()
    {
        leaveBtn.onClick.AddListener(CloseStore);
    }

    private void CloseStore()
    {
        gameObject.SetActive(false);
    }

    private void ShowStoreImagePrice(StoreItemData item,Transform child,int number)
    {
        string imgChannel = number == 1 ? $"UnitImages/Unit_Img_{item.itemId}" : (number == 2 ? $"KIcon/WarRelic/{item.itemId}" : $"ItemImages/Item{item.itemId}");

        // 아이템 오브젝트 가져오기
        Image img = child.GetComponent<Image>();

        img.sprite = SpriteCacheManager.GetSprite(imgChannel);
        TextMeshProUGUI costText = child.GetChild(0).GetComponent<TextMeshProUGUI>();
        int cost = (int)(item.price * StoreManager.GetRandomBetweenValue(item.priceRateMin, item.priceRateMax));
        costText.text = $"{cost}";

        Button btn = child.GetComponent<Button>();
        BtnIteractable(btn, cost);

        child.name = $"{item.itemId}";
        //눌렀을때 유산id값을 바탕으로 구매 & 내 골드 감소
        btn.onClick.AddListener(()=>PurChaseItem(item,cost));

    }

    private void BtnIteractable(Button btn,int price)
    {
        int gold = RogueLikeData.Instance.GetCurrentGold();
        if(gold < price) btn.interactable = false;
        else btn.interactable = true;
    }
    private void PurchaseUnitPackage(List<RogueUnitDataBase> units, int price)
    {

    }

    private void PurchaseRelic(int relicId, int price)
    {

    }

    private void PurChaseItem(StoreItemData item, int price)
    {



    }
}
