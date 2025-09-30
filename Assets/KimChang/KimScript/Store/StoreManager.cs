using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class StoreManager
{
    private static List<StoreItemData> storeItems => StoreItemDataLoader.Load();

    private static readonly Dictionary<int, int> defaultWeights = new()
    {
        { 1, 60 },
        { 2, 25 },
        { 3, 10 },
        { 4, 4 },
        { 5, 1 },
    };

    public static void LoadStoreData()
    {
        StoreItemDataLoader.Load();
    }

    public static int CalculatePrice(StoreItemData item)
    {
        float rate = RogueLikeData.Instance.GetRandomInt((int)item.priceRateMin, (int)item.priceRateMax);
        return Mathf.RoundToInt(item.price * rate);
    }

    public static List<StoreItemData> GetFilteredItems(Func<StoreItemData, bool> predicate, int count, bool allowDuplicate = false, Dictionary<int, int> rarityWeights = null)
    {
        var candidates = storeItems.Where(predicate).ToList();
        return GetWeightedRandomItemsByRarity(candidates, count, allowDuplicate, rarityWeights);
    }


    private static List<StoreItemData> GetWeightedRandomItemsByRarity(List<StoreItemData> items, int count, bool allowDuplicate = false, Dictionary<int, int> rarityWeights = null)
    {
        var result = new List<StoreItemData>();
        rarityWeights ??= defaultWeights;

        var weightedItems = new List<StoreItemData>();
        foreach (var item in items)
        {
            if (!rarityWeights.TryGetValue(item.rarity, out int weight)) continue;
            for (int i = 0; i < weight; i++)
                weightedItems.Add(item);
        }

        int safeGuard = 1000; // 무한 루프 방지
        while (result.Count < count && weightedItems.Count > 0 && safeGuard-- > 0)
        {
            var selected = weightedItems[RogueLikeData.Instance.GetRandomInt(0, weightedItems.Count)];
            if (allowDuplicate || !result.Contains(selected))
                result.Add(selected);
        }

        return result;
    }

    public static List<StoreItemData> GetRandomEnergyMoraleItems()
    {
        return GetFilteredItems(
        item => (item.itemId >= 0 && item.itemId <= 9) || (item.itemId >= 30 && item.itemId <= 33),
        3
    );
    }

    public static List<StoreItemData> GetRandomDiceItem()
    {
        return GetFilteredItems(item => item.itemId >= 60 && item.itemId <= 62, 1);
    }
    public static List<StoreItemData> GetRandomRelicItems()
    {
        return GetFilteredItems(item => item.itemId >= 80 && item.itemId <= 81, 3,true);
    }

    public static List<StoreItemData> GetRandomUnitItems()
    {
        return GetFilteredItems(item => item.itemId >= 100 && item.itemId <= 119, 5);
    }

    // 두 값 사이 무작위 값 반환
    public static float GetRandomBetweenValue(float min, float max)
    {
        return min + (max - min) * RogueLikeData.Instance.GetRandomFloat();
    }

    // 사용처: 스냅샷의 itemId → StoreItemData 복구
    public static StoreItemData GetItemDataById(int id)
    {
        // 단순 순회가 가장 빠름(소수 개, 캐시된 리스트)
        var list = storeItems;
        for (int i = 0, n = list.Count; i < n; i++)
            if (list[i].itemId == id) return list[i];
        return null;
    }

}
