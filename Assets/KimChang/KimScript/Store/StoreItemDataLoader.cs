using System.Collections.Generic;
using UnityEngine;

public static class StoreItemDataLoader
{
    private static List<StoreItemData> cachedItems;

    public static List<StoreItemData> Load()
    {
        // 이미 로드된 경우 캐시 반환
        if (cachedItems != null)
            return cachedItems;

        TextAsset json = Resources.Load<TextAsset>("JsonData/StoreItemData"); // Resources/JsonData/StoreItemData.json
        if (json == null)
        {
            Debug.LogError("StoreItemData.json 파일을 찾을 수 없습니다.");
            return new List<StoreItemData>();
        }

        cachedItems = JsonUtilityWrapper.FromJsonItemList<StoreItemData>(json.text);
        return cachedItems;
    }

    // 필요 시 캐시 초기화
    public static void ClearCache()
    {
        cachedItems = null;
    }
}
