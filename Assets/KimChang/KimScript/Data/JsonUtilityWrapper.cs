using System;
using System.Collections.Generic;
using UnityEngine;

public static class JsonUtilityWrapper
{
    [Serializable]
    private class Wrapper<T>
    {
        public List<T> items;
    }

    public static List<T> FromJsonItemList<T>(string json)
    {
        string wrapped = "{\"items\":" + json + "}";
        return JsonUtility.FromJson<Wrapper<T>>(wrapped).items;
    }
}
