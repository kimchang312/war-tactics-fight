using UnityEngine;
using System.Collections.Generic;

public static class ListExtensions
{
    public static T Random<T>(this List<T> list)
    {
        if (list == null || list.Count == 0)
            throw new System.InvalidOperationException("Empty list");
        return list[UnityEngine.Random.Range(0, list.Count)];
    }
}
