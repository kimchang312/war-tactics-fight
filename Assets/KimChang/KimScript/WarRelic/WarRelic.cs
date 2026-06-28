using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Globalization;


[JsonConverter(typeof(StringEnumConverter))]
public enum RelicType
{
    AllEffect,
    SpecialEffect,
    StateBoost,
    BattleActive,
    ActiveState,         //BattleActive,StateBoost의 효과가 둘다 있는경우
    GetEffect,
    NoneBattle,
    RewardEffect,
    Delete,

}
[System.Serializable]
public class WarRelic
{
    public int id;
    public int grade;
    public bool used;
    public RelicType type;
    public RelicType[] types;
    public string name;
    public string description;

    private Action<WarRelic> executeAction;

    private string[] _values;
    private float[] _cachedValues;
    private bool _cacheValid;
    private bool _cacheOk;

    public void BindMeta(string name, string description)
    {
        this.name = name;
        this.description = description;
    }

    public void BindExecute(Action<WarRelic> action) => executeAction = action;

    public void BindConfig(string[] values) => _values = values;

    public void BindTypes(RelicType[] relicTypes)
    {
        types = relicTypes;
    }

    public bool HasType(RelicType targetType)
    {
        if (type == targetType)
            return true;

        if (type == RelicType.ActiveState &&
            (targetType == RelicType.BattleActive || targetType == RelicType.StateBoost))
            return true;

        if (types == null)
            return false;

        for (int i = 0; i < types.Length; i++)
        {
            RelicType current = types[i];
            if (current == targetType)
                return true;

            if (current == RelicType.ActiveState &&
                (targetType == RelicType.BattleActive || targetType == RelicType.StateBoost))
                return true;
        }

        return false;
    }

    public void Execute() => executeAction?.Invoke(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetVStr(int index, string def = "")
    {
        var a = _values;
        return (a != null && (uint)index < (uint)a.Length) ? a[index] : def;
    }

    /// <summary>
    /// 규칙: '+'는 그대로, '-'는 음수, '%'는 ×0.01 적용. 값이 하나도 없으면 null 반환.
    /// </summary>
    public List<float> GetAllValuesAsFloatListOrNull()
    {
        if (!EnsureParsed())
            return null;

        var src = _cachedValues;
        // List 생성은 불가피(시그니처 유지). 파싱은 캐시 덕분에 1회만 수행됨
        var list = new List<float>(src.Length);
        for (int i = 0; i < src.Length; i++)
            list.Add(src[i]);
        return list;
    }
    public bool TryGetAllValuesNonAlloc(List<float> dst)
    {
        if (!EnsureParsed())
            return false;

        dst.Clear();
        var arr = _cachedValues;
        for (int i = 0; i < arr.Length; i++)
            dst.Add(arr[i]);
        return true;
    }
    public ReadOnlySpan<float> GetAllValuesSpanOrEmpty()
    {
        if (!EnsureParsed())
            return ReadOnlySpan<float>.Empty;
        return new ReadOnlySpan<float>(_cachedValues);
    }
    private bool EnsureParsed()
    {
        if (_cacheValid)
            return _cacheOk;

        _cacheValid = true;

        var src = _values;
        if (src == null || src.Length == 0)
        {
            _cachedValues = Array.Empty<float>();
            _cacheOk = false;
            return false;
        }

        // 캐시 배열을 정확 길이로 할당
        var arr = new float[src.Length];

        for (int i = 0; i < src.Length; i++)
        {
            if (!TryParseValueTokenSpan(src[i].AsSpan(), out float v))
            {
                _cachedValues = Array.Empty<float>();
                _cacheOk = false;
                return false;
            }
            arr[i] = v;
        }

        _cachedValues = arr;
        _cacheOk = true;
        return true;
    }
    // 사용처: "+100" / "-2%" / "25%" 같은 토큰을 float으로 변환
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryParseValueTokenSpan(ReadOnlySpan<char> token, out float value)
    {
        value = 0f;
        if (token.Length == 0)
            return false;

        // 앞뒤 공백 제거(Span 수동 Trim)
        int start = 0;
        int end = token.Length - 1;
        while (start <= end && char.IsWhiteSpace(token[start])) start++;
        while (end >= start && char.IsWhiteSpace(token[end])) end--;
        if (start > end)
            return false;

        bool hasPercent = token[end] == '%';
        if (hasPercent)
            end--;

        if (start > end)
            return false;

        var core = token.Slice(start, end - start + 1);

        if (!float.TryParse(core, NumberStyles.Float | NumberStyles.AllowLeadingSign,
                            CultureInfo.InvariantCulture, out float v))
            return false;

        if (hasPercent)
            v *= 0.01f;

        value = v;
        return true;
    }
    public void SetValues(string[] values)
    {
        _values = values;
        _cacheValid = false;
        _cacheOk = false;
    }
}

