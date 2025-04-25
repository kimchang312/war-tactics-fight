using System.Collections.Generic;
using UnityEngine;

public class StoreManager
{
    private static List<StoreItemData> storeItems => StoreItemDataLoader.Load();

    public static void LoadStoreData()
    {
        StoreItemDataLoader.Load();
    }

    //기력 사기 아이템 3개 반환
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
        var groupedByRarity = new Dictionary<int, List<StoreItemData>>();
        foreach (var item in candidates)
        {
            if (!groupedByRarity.ContainsKey(item.rarity))
                groupedByRarity[item.rarity] = new List<StoreItemData>();

            groupedByRarity[item.rarity].Add(item);
        }

        Dictionary<int, float> baseRates = new()
    {
        {5, 5f}, {4, 10f}, {3, 15f}, {2, 20f}
    };

        List<StoreItemData> result = new();

        for (int i = 0; i < count; i++)
        {
            float totalRate = 0f;
            Dictionary<int, float> currentRates = new();

            // 현재 남아있는 rarity 기준으로 확률 계산
            foreach (var kv in groupedByRarity)
            {
                if (kv.Value.Count > 0 && baseRates.ContainsKey(kv.Key))
                {
                    currentRates[kv.Key] = baseRates[kv.Key];
                    totalRate += baseRates[kv.Key];
                }
            }

            if (groupedByRarity.ContainsKey(1) && groupedByRarity[1].Count > 0)
            {
                currentRates[1] = 100f - totalRate;
            }

            float rand = Random.Range(0f, 100f);
            float acc = 0f;

            foreach (var kv in currentRates)
            {
                acc += kv.Value;
                if (rand <= acc)
                {
                    var pool = groupedByRarity[kv.Key];
                    if (pool.Count == 0) break;

                    int idx = Random.Range(0, pool.Count);
                    result.Add(pool[idx]);
                    break; // 중요: 선택 후 루프 탈출
                }
            }
        }

        return result;
    }


    // 두 값 사이 무작위 값 반환
    public static float GetRandomBetweenValue(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }


}
