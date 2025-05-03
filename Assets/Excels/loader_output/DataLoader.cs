// ************************************************************************
// Auto generated class from Excel file
// Do not modify this file directly.
// ************************************************************************
using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public abstract class BaseJsonData
{
    public int key;
}

public abstract class JsonLoader
{
    public List<BaseJsonData> ItemsList { get; protected set; }
    public Dictionary<int, BaseJsonData> ItemsDict { get; protected set; }

    protected void AddDatas<T>(List<T> items) where T : BaseJsonData
    {
        ItemsList = items.ConvertAll(x => x as BaseJsonData);
        ItemsDict = new Dictionary<int, BaseJsonData>();
        foreach (var item in ItemsList)
        {
            ItemsDict.Add(item.key, item);
        }
    }

    public T GetByKey<T>(int key) where T : BaseJsonData
    {
        if (ItemsDict.ContainsKey(key))
        {
            return ItemsDict[key] as T;
        }
        return null;
    }

    public T GetByIndex<T>(int index) where T : BaseJsonData
    {
        if (index >= 0 && index < ItemsList.Count)
        {
            return ItemsList[index] as T;
        }
        return null;
    }

    public List<T> GetAll<T>() where T : BaseJsonData
    {
        return ItemsList.ConvertAll(x => x as T);
    }

    public int Count()
    {
        return ItemsList.Count;
    }
}
public class DataLoader
{
    private Dictionary<string, JsonLoader> _loaders = new Dictionary<string, JsonLoader>();

    // loadFunc is json file loader function
    // ex) Func<string, TextAsset> loadFunc = () => Resources.Load<TextAsset>();
    public DataLoader(Func<string, TextAsset> loadFunc)
    {
        _loaders.Add(nameof(스테이지프리셋), new 스테이지프리셋Loader(loadFunc));
    }

    public T GetByKey<T>(int key) where T : BaseJsonData
    {
        if (_loaders.ContainsKey(typeof(T).Name))
        {
            return _loaders[typeof(T).Name].GetByKey<T>(key);
        }
        return null;
    }
    public T GetByIndex<T>(int index) where T : BaseJsonData
    {
        if (_loaders.ContainsKey(typeof(T).Name))
        {
            return _loaders[typeof(T).Name].GetByIndex<T>(index);
        }
        return null;
    }
    public int Count<T>() where T : BaseJsonData
    {
        if (_loaders.ContainsKey(typeof(T).Name))
        {
            return _loaders[typeof(T).Name].ItemsList.Count;
        }
        return 0;
    }
    public List<T> GetAll<T>() where T : BaseJsonData
    {
        if (_loaders.ContainsKey(typeof(T).Name))
        {
            return _loaders[typeof(T).Name].GetAll<T>();
        }
        return null;
    }
}
