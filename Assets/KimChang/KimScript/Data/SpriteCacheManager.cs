using System.Collections.Generic;
using UnityEngine;

public static class SpriteCacheManager
{
    private static readonly Dictionary<string, Sprite> spriteCache = new();

    public static Sprite GetSprite(string path)
    {
        if (spriteCache.TryGetValue(path, out var cachedSprite))
        {
            return cachedSprite;
        }

        var loadedSprite = Resources.Load<Sprite>(path);
        if (loadedSprite == null)
        {
            Debug.LogWarning($"[SpriteCacheManager] Sprite not found at path: {path}");
            return null;
        }

        spriteCache[path] = loadedSprite;
        return loadedSprite;
    }

    // 필요시 캐시 비우기 (예: 씬 전환 시)
    public static void Clear()
    {
        spriteCache.Clear();
    }
}
