using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json; // WarRelic.cs에서 이미 사용 중

[Serializable]
public sealed class WarRelicRecord
{
    public int id;
    public string name;
    public string description; // JSON의 description을 그대로 사용
    public string[] type;      // 다중 타입 지원
    public int grade;
    public string[] value;
}

public static class WarRelicLoader
{
    // 사용처: 게임 시작 시 1회 호출해 캐시에 보관
    public static bool TryLoadFromResources(string resourcePath, out Dictionary<int, WarRelicRecord> dict)
    {
        // resourcePath 예시: "JsonDat/WarRelicsList"  // 확장자 붙이지 않음
        TextAsset ta = Resources.Load<TextAsset>(resourcePath);
        if (ta == null)
        {
            dict = null;
            Debug.LogError($"WarRelicLoader: TextAsset not found at Resources/{resourcePath}");
            return false;
        }
        return TryParse(ta.text, out dict);
    }

    // 사용처: 단위 테스트나 에디터 툴에서 문자열 직접 파싱
    public static bool TryParse(string json, out Dictionary<int, WarRelicRecord> dict)
    {
        dict = null;
        if (string.IsNullOrEmpty(json)) return false;

        // 선행 공백 스킵
        int i = 0;
        int n = json.Length;
        while (i < n && char.IsWhiteSpace(json[i])) i++;
        if (i >= n) return false;

        try
        {
            char c = json[i];
            if (c == '{')
            {
                // 구(舊) 방식: { "items": [...] } 래퍼
                var wrapper = JsonUtility.FromJson<WarRelicArrayWrapper>(json);
                var items = wrapper?.items;
                if (items == null)
                {
                    dict = new Dictionary<int, WarRelicRecord>(0);
                    return true;
                }
                int len = items.Length;
                dict = new Dictionary<int, WarRelicRecord>(len);
                for (int k = 0; k < len; k++)
                {
                    var r = items[k];
                    dict[r.id] = r;
                }
                return true;
            }
            else if (c == '[')
            {
                // 신(新) 방식: 배열 루트
                var list = JsonConvert.DeserializeObject<List<WarRelicRecord>>(json);
                if (list == null)
                {
                    dict = new Dictionary<int, WarRelicRecord>(0);
                    return true;
                }
                int len = list.Count;
                dict = new Dictionary<int, WarRelicRecord>(len);
                for (int k = 0; k < len; k++)
                {
                    var r = list[k];
                    dict[r.id] = r;
                }
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"WarRelicLoader parse error: {e.Message}");
        }
        return false;
    }

    [Serializable]
    private sealed class WarRelicArrayWrapper
    {
        public WarRelicRecord[] items;
    }
}
