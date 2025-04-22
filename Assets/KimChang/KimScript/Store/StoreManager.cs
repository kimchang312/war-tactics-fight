using System.Collections.Generic;
using UnityEngine;

public class StoreManager
{
    private static List<StoreItemData> storeItems => StoreItemDataLoader.Load();

    public static void LoadStoreData()
    {
        StoreItemDataLoader.Load();
    }

    public static List<StoreItemData> GetRandomEnergyMoraleItems()
    {
        var candidates = storeItems.FindAll(item =>
            (item.itemId >= 0 && item.itemId <= 9) || (item.itemId >= 30 && item.itemId <= 33)
        );

        return GetWeightedRandomItemsByRarity(candidates, 3);
    }

    // 유산 (80~81) 3개
    public static List<StoreItemData> GetRandomRelicItems()
    {
        var candidates = storeItems.FindAll(item => item.itemId >= 80 && item.itemId <= 81);
        return GetWeightedRandomItemsByRarity(candidates, 3);
    }

    // 유닛 (100~119) 중 5개 반환, 단 119번은 condition == "1"일 경우만 등장
    public static List<StoreItemData> GetRandomUnitItems()
    {
        var candidates = storeItems.FindAll(item =>
            (item.itemId >= 100 && item.itemId < 119) ||
            (item.itemId == 119 && item.condition != null && item.condition == "1")
        );

        return GetWeightedRandomItemsByRarity(candidates, 5);
    }
    // 주사위 (60~62) 중 1개, 조건 검사 포함
    public static StoreItemData GetRandomDiceItem()
    {
        var candidates = storeItems.FindAll(item =>
            (item.itemId == 60) ||
            ((item.itemId == 61 || item.itemId == 62) && item.condition != null && item.condition == "1")
        );

        var list = GetWeightedRandomItemsByRarity(candidates, 1);
        return list.Count > 0 ? list[0] : null;
    }

    // 내부 유틸: 중복 없이 랜덤 선택
    private static List<StoreItemData> GetRandomItems(List<StoreItemData> source, int count)
    {
        List<StoreItemData> result = new List<StoreItemData>();
        List<StoreItemData> pool = new List<StoreItemData>(source);

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }
    //등장 확률
    private static List<StoreItemData> GetWeightedRandomItemsByRarity(List<StoreItemData> candidates, int count)
    {
        // 등장 가능한 희귀도 종류 추출
        var groupedByRarity = new Dictionary<int, List<StoreItemData>>();
        foreach (var item in candidates)
        {
            if (!groupedByRarity.ContainsKey(item.rarity))
                groupedByRarity[item.rarity] = new List<StoreItemData>();

            groupedByRarity[item.rarity].Add(item);
        }

        // 희귀도 확률 정의
        Dictionary<int, float> baseRates = new()
    {
        {5, 5f}, {4, 10f}, {3, 15f}, {2, 20f}
    };

        // 등장 희귀도 기준으로 확률 재조정
        float totalRate = 0;
        Dictionary<int, float> finalRates = new();
        foreach (var rarity in groupedByRarity.Keys)
        {
            if (baseRates.ContainsKey(rarity))
            {
                finalRates[rarity] = baseRates[rarity];
                totalRate += baseRates[rarity];
            }
        }

        // 나머지 확률은 rarity 1에 부여
        if (groupedByRarity.ContainsKey(1))
        {
            finalRates[1] = 100f - totalRate;
        }

        // 무작위 추출
        List<StoreItemData> result = new();
        for (int i = 0; i < count; i++)
        {
            float rand = Random.Range(0f, 100f);
            float acc = 0f;

            foreach (var kv in finalRates)
            {
                acc += kv.Value;
                if (rand <= acc)
                {
                    var pool = groupedByRarity[kv.Key];
                    if (pool.Count == 0) break;

                    int idx = Random.Range(0, pool.Count);
                    result.Add(pool[idx]);
                    pool.RemoveAt(idx);
                    break;
                }
            }
        }

        return result;
    }

}
