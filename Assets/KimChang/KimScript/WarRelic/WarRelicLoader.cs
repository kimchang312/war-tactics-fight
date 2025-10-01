using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

//public enum RelicType { SpecialEffect, StateBoost, BattleActive, GetEffect }

[Serializable]
public sealed class WarRelicRecord
{
    public int id;
    public string name;
    public string description;
    public string type;   // JSON에서 문자열로 들어오므로 파싱
    public int grade;
    public string[] value;
}

public static class WarRelicLoader
{
    // 사용처: 게임 시작 시 1회 로드
    public static Dictionary<int, WarRelicRecord> LoadFromTextAsset(TextAsset json)
    {
        // JsonUtility는 배열 루트 파싱이 약해서 래퍼 사용
        var wrapper = JsonUtility.FromJson<WarRelicArrayWrapper>(json.text);
        var dict = new Dictionary<int, WarRelicRecord>(wrapper.items.Length);
        for (int i = 0; i < wrapper.items.Length; i++)
        {
            var r = wrapper.items[i];
            dict[r.id] = r;
        }
        return dict;
    }

    [Serializable]
    private sealed class WarRelicArrayWrapper
    {
        public WarRelicRecord[] items;
    }
}
