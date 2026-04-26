using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum TextKind
{
    None = 0,
    UI = 1,

    Unit = 2,
    Tag = 3,
    UnitRarity = 4,
    RelicRarity = 5,
    RelicName = 10,
    EventTitle = 20,
    EventDesc = 21,
    ItemName = 30,
    ItemDesc = 31,
    Tutorial = 40,
    Ability = 50,
    BuffDeBuff = 60,
}

public static class GameTextDB
{
    [Serializable]
    class Entry
    {
        public int Idx;
        public int kind;
        public string kindName;
        public int TitleKey;
        public int ForeignKey;
        public string kr;
        public string en;
        public string jp;
    }

    [Serializable]
    class Root { public Entry[] entries; }

    // 데이터에서 '미지정'을 의미하는 값(엑셀 공란 -> -1로 변환된 값)
    const int KeyNone = -1;

    struct Pick2
    {
        public int idx;
        public bool preferred; // 다른 키가 -1이면 true
        public Pick2(int idx, bool preferred) { this.idx = idx; this.preferred = preferred; }
    }

    struct TripleKey : IEquatable<TripleKey>
    {
        public readonly int kind;
        public readonly int title;
        public readonly int foreign;

        public TripleKey(int kind, int title, int foreign)
        {
            this.kind = kind;
            this.title = title;
            this.foreign = foreign;
        }

        public bool Equals(TripleKey other)
            => kind == other.kind && title == other.title && foreign == other.foreign;

        public override bool Equals(object obj)
            => obj is TripleKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                h = h * 31 + kind;
                h = h * 31 + title;
                h = h * 31 + foreign;
                return h;
            }
        }
    }

    static string _resourcePath = "JsonData/GameTextData"; // Resources/GameTextData.json(TextAsset)로 넣는 형태면 확장자 없이

    static string _curLang = "kr";
    static string _fbLang = "en";

    static Dictionary<int, string> _byIdxCur;
    static Dictionary<int, string> _byIdxFb;

    // 2-key 캐시(대표 1개만)
    static Dictionary<long, string> _byKindTitleCur;
    static Dictionary<long, string> _byKindTitleFb;

    static Dictionary<long, string> _byKindForeignCur;
    static Dictionary<long, string> _byKindForeignFb;

    // 3-key 캐시(정확 조회)
    static Dictionary<TripleKey, string> _byKindTitleForeignCur;
    static Dictionary<TripleKey, string> _byKindTitleForeignFb;

    // 역조회: (kind, foreignKey) -> idx (대표 1개만)
    static Dictionary<long, int> _idxByKindForeign;

    static readonly Dictionary<string, TextKind> _kindMap = new Dictionary<string, TextKind>(StringComparer.OrdinalIgnoreCase)
    {
        { "None", TextKind.None },
        { "UI", TextKind.UI },

        { "Unit", TextKind.Unit },
        { "Tag", TextKind.Tag },
        { "UnitRarity", TextKind.UnitRarity },
        { "RelicRarity", TextKind.RelicRarity },
        { "RelicName", TextKind.RelicName },

        { "EventTitle", TextKind.EventTitle },
        { "EventDesc", TextKind.EventDesc },

        { "ItemName", TextKind.ItemName },
        { "ItemDesc", TextKind.ItemDesc },

        { "Tutorial", TextKind.Tutorial },
        { "Ability", TextKind.Ability },
        { "BuffDeBuff", TextKind.BuffDeBuff },
    };

    // 사용처: 게임 시작 시 1회 초기화
    public static void Boot(string defaultLang = "kr", string fallback = "en")
    {
        Load(defaultLang, fallback);
    }

    // 사용처: 언어 변경(옵션 메뉴 등)
    public static void Load(string lang, string fallback = "en")
    {
        _curLang = lang;
        _fbLang = (fallback == lang) ? null : fallback;

        var ta = Resources.Load<TextAsset>(_resourcePath);
        if (ta == null || string.IsNullOrEmpty(ta.text))
        {
#if UNITY_EDITOR
            Debug.LogError("[GameTextDB] Resources에 GameTextData가 없거나 비었습니다.");
#endif
            _byIdxCur = new Dictionary<int, string>(0);
            _byKindTitleCur = new Dictionary<long, string>(0);
            _byKindForeignCur = new Dictionary<long, string>(0);
            _byKindTitleForeignCur = new Dictionary<TripleKey, string>(0);
            _idxByKindForeign = new Dictionary<long, int>(0);

            _byIdxFb = null;
            _byKindTitleFb = null;
            _byKindForeignFb = null;
            _byKindTitleForeignFb = null;

            return;
        }

        var root = JsonUtility.FromJson<Root>(ta.text);
        var arr = root?.entries ?? Array.Empty<Entry>();
        int cap = Mathf.Max(16, arr.Length * 2);

        bool useFb = _fbLang != null;

        // 1-key: Idx
        _byIdxCur = new Dictionary<int, string>(cap);
        _byIdxFb = useFb ? new Dictionary<int, string>(cap) : null;

        // 3-key: (kind, TitleKey, ForeignKey)
        _byKindTitleForeignCur = new Dictionary<TripleKey, string>(cap);
        _byKindTitleForeignFb = useFb ? new Dictionary<TripleKey, string>(cap) : null;

        // 2-key는 데이터상 중복이 매우 흔하다.
        // 따라서 대표 Entry를 1개만 고르는 pick 맵을 만들고, 이후에 최종 캐시로 변환한다.
        // 룰:
        // - (kind,TitleKey) 조회는 ForeignKey == -1 인 Entry를 우선
        // - (kind,ForeignKey) 조회는 TitleKey == -1 인 Entry를 우선
        var pickTitle = new Dictionary<long, Pick2>(cap);
        var pickForeign = new Dictionary<long, Pick2>(cap);

        for (int i = 0; i < arr.Length; i++)
        {
            var e = arr[i];
            if (e == null) continue;

            var cur = PickLang(e, _curLang);
            var fb = useFb ? PickLang(e, _fbLang) : null;

            // Idx
            if (cur != null) _byIdxCur[e.Idx] = cur;
            else if (useFb && fb != null) _byIdxFb[e.Idx] = fb;

            // (kind, TitleKey, ForeignKey) 정확 조회용
            // 사용처: TitleKey/ForeignKey를 동시에 주는 호출(또는 한쪽만 주는 호출)
            // 주의: (TitleKey,ForeignKey) 둘 다 -1(미지정)인 데이터는 키 충돌이 매우 많아서 캐싱 대상에서 제외
            if (e.TitleKey != KeyNone || e.ForeignKey != KeyNone)
            {
                var k3 = new TripleKey(e.kind, e.TitleKey, e.ForeignKey);

                if (cur != null)
                {
                    // 같은 키가 여러 번 나오면 "첫 번째"를 유지한다.
                    if (!_byKindTitleForeignCur.TryGetValue(k3, out _))
                        _byKindTitleForeignCur.Add(k3, cur);
                }
                else if (useFb && fb != null)
                {
                    if (_byKindTitleForeignFb != null && !_byKindTitleForeignFb.TryGetValue(k3, out _))
                        _byKindTitleForeignFb.Add(k3, fb);
                }
            }

            // (kind, TitleKey) 대표 선택
            // 사용처: kind + titleKey만 주는 호출(== foreignKey는 미지정(-1)으로 취급)
            if (e.TitleKey != KeyNone)
            {
                long k2 = Pack2(e.kind, e.TitleKey);
                bool preferred = (e.ForeignKey == KeyNone);

                if (pickTitle.TryGetValue(k2, out var p))
                {
#if UNITY_EDITOR
                    if (p.preferred && preferred)
                        Debug.LogWarning($"[GameTextDB] Duplicated default (kind,TitleKey) pick: ({e.kind},{e.TitleKey})");
#endif
                    if (!p.preferred && preferred)
                        pickTitle[k2] = new Pick2(e.Idx, true); // -1(미지정) 쪽을 우선
                }
                else
                {
                    pickTitle.Add(k2, new Pick2(e.Idx, preferred));
                }
            }

            // (kind, ForeignKey) 대표 선택
            // 사용처: kind + foreignKey만 주는 호출(== titleKey는 미지정(-1)으로 취급)
            if (e.ForeignKey != KeyNone)
            {
                long k2 = Pack2(e.kind, e.ForeignKey);
                bool preferred = (e.TitleKey == KeyNone);

                if (pickForeign.TryGetValue(k2, out var p))
                {
#if UNITY_EDITOR
                    if (p.preferred && preferred)
                        Debug.LogWarning($"[GameTextDB] Duplicated default (kind,ForeignKey) pick: ({e.kind},{e.ForeignKey})");
#endif
                    if (!p.preferred && preferred)
                        pickForeign[k2] = new Pick2(e.Idx, true); // -1(미지정) 쪽을 우선
                }
                else
                {
                    pickForeign.Add(k2, new Pick2(e.Idx, preferred));
                }
            }
        }

        // pick -> 최종 캐시 변환
        _byKindTitleCur = new Dictionary<long, string>(pickTitle.Count);
        _byKindTitleFb = useFb ? new Dictionary<long, string>(pickTitle.Count) : null;

        foreach (var kv in pickTitle)
        {
            int idx = kv.Value.idx;
            if (_byIdxCur.TryGetValue(idx, out var s)) _byKindTitleCur[kv.Key] = s;
            else if (useFb && _byIdxFb != null && _byIdxFb.TryGetValue(idx, out s)) _byKindTitleFb[kv.Key] = s;
        }

        _byKindForeignCur = new Dictionary<long, string>(pickForeign.Count);
        _byKindForeignFb = useFb ? new Dictionary<long, string>(pickForeign.Count) : null;

        // 역조회: (kind, foreignKey) -> Entry.Idx
        // 주의: 미지정(-1)은 역조회 대상에서 제외(충돌 위험 큼)
        _idxByKindForeign = new Dictionary<long, int>(pickForeign.Count);

        foreach (var kv in pickForeign)
        {
            int idx = kv.Value.idx;
            _idxByKindForeign[kv.Key] = idx;

            if (_byIdxCur.TryGetValue(idx, out var s)) _byKindForeignCur[kv.Key] = s;
            else if (useFb && _byIdxFb != null && _byIdxFb.TryGetValue(idx, out s)) _byKindForeignFb[kv.Key] = s;
        }

        Resources.UnloadAsset(ta);
    }

    // 사용처: 리소스 파일 경로 변경(테스트용)
    public static string ResourcePath
    {
        get => _resourcePath;
        set => _resourcePath = value;
    }

    // 사용처: idx 기반 텍스트 조회(가장 빠르고 안전)
    public static string Get(int idx)
    {
        if (_byIdxCur != null && _byIdxCur.TryGetValue(idx, out var v)) return v;
        if (_byIdxFb != null && _byIdxFb.TryGetValue(idx, out v)) return v;

#if UNITY_EDITOR
        Debug.LogWarning($"[GameTextDB] Missing Idx={idx}");
#endif
        return string.Empty;
    }

    // 사용처: (kind,TitleKey) 기반 조회(외래키를 안 쓰는 데이터)
    public static string GetByTitleKey(TextKind kind, int titleKey)
    {
        long k = Pack2((int)kind, titleKey);
        if (_byKindTitleCur != null && _byKindTitleCur.TryGetValue(k, out var v)) return v;
        if (_byKindTitleFb != null && _byKindTitleFb.TryGetValue(k, out v)) return v;

#if UNITY_EDITOR
        Debug.LogWarning($"[GameTextDB] Missing (kind,TitleKey): ({kind},{titleKey})");
#endif
        return string.Empty;
    }

    // 사용처: (kind,ForeignKey) 기반 조회(타이틀키를 안 주는 호출)
    public static string GetByForeignKey(TextKind kind, int foreignKey)
    {
        long k = Pack2((int)kind, foreignKey);
        if (_byKindForeignCur != null && _byKindForeignCur.TryGetValue(k, out var v)) return v;
        if (_byKindForeignFb != null && _byKindForeignFb.TryGetValue(k, out v)) return v;

#if UNITY_EDITOR
        Debug.LogWarning($"[GameTextDB] Missing (kind,ForeignKey): ({kind},{foreignKey})");
#endif
        return string.Empty;
    }

    // 사용처: titleKey / foreignKey 둘 다(또는 한쪽만) 받을 수 있는 통합 조회
    // -1(미지정)을 넣으면 자동으로 대표 값을 찾아준다.
    public static string Get(TextKind kind, int titleKey, int foreignKey)
    {
        // 1) 정확히 일치하는 3-key 조회
        var k3 = new TripleKey((int)kind, titleKey, foreignKey);
        if (_byKindTitleForeignCur != null && _byKindTitleForeignCur.TryGetValue(k3, out var v)) return v;
        if (_byKindTitleForeignFb != null && _byKindTitleForeignFb.TryGetValue(k3, out v)) return v;

        // 2) 한쪽 키만 주어진 경우: 다른 키는 -1(미지정)인 대표 값을 반환
        if (titleKey == KeyNone && foreignKey != KeyNone)
        {
            long k2 = Pack2((int)kind, foreignKey);
            if (_byKindForeignCur != null && _byKindForeignCur.TryGetValue(k2, out v)) return v;
            if (_byKindForeignFb != null && _byKindForeignFb.TryGetValue(k2, out v)) return v;
        }
        else if (titleKey != KeyNone && foreignKey == KeyNone)
        {
            long k2 = Pack2((int)kind, titleKey);
            if (_byKindTitleCur != null && _byKindTitleCur.TryGetValue(k2, out v)) return v;
            if (_byKindTitleFb != null && _byKindTitleFb.TryGetValue(k2, out v)) return v;
        }
        else
        {
            // 3) 둘 다 주어진 경우: "다른 키가 -1인 기본값"을 우선으로 한 번 더 시도
            if (foreignKey != KeyNone)
            {
                k3 = new TripleKey((int)kind, titleKey, KeyNone);
                if (_byKindTitleForeignCur != null && _byKindTitleForeignCur.TryGetValue(k3, out v)) return v;
                if (_byKindTitleForeignFb != null && _byKindTitleForeignFb.TryGetValue(k3, out v)) return v;
            }

            if (titleKey != KeyNone)
            {
                k3 = new TripleKey((int)kind, KeyNone, foreignKey);
                if (_byKindTitleForeignCur != null && _byKindTitleForeignCur.TryGetValue(k3, out v)) return v;
                if (_byKindTitleForeignFb != null && _byKindTitleForeignFb.TryGetValue(k3, out v)) return v;
            }

            // 4) 마지막 안전망: 2-key 캐시
            if (titleKey != KeyNone)
            {
                long k2 = Pack2((int)kind, titleKey);
                if (_byKindTitleCur != null && _byKindTitleCur.TryGetValue(k2, out v)) return v;
                if (_byKindTitleFb != null && _byKindTitleFb.TryGetValue(k2, out v)) return v;
            }

            if (foreignKey != KeyNone)
            {
                long k2 = Pack2((int)kind, foreignKey);
                if (_byKindForeignCur != null && _byKindForeignCur.TryGetValue(k2, out v)) return v;
                if (_byKindForeignFb != null && _byKindForeignFb.TryGetValue(k2, out v)) return v;
            }
        }

#if UNITY_EDITOR
        Debug.LogWarning($"[GameTextDB] Missing (kind,TitleKey,ForeignKey): ({kind},{titleKey},{foreignKey})");
#endif
        return string.Empty;
    }

    // 사용처: kindName 문자열 기반 foreignKey 조회(외부 데이터가 kindName만 들고 있을 때)
    public static string GetByForeign(string kindName, int foreignKey)
    {
        if (!_kindMap.TryGetValue(kindName, out var k)) return string.Empty;
        return GetByForeignKey(k, foreignKey);
    }

    // 사용처: (kind, foreignKey) -> idx 역조회(대표 1개)
    public static int GetIdxByForeignKey(TextKind kind, int foreignKey, int notFound = -1)
    {
        if (foreignKey == KeyNone) return notFound;
        if (_idxByKindForeign == null) return notFound;

        long k = Pack2((int)kind, foreignKey);
        return _idxByKindForeign.TryGetValue(k, out var idx) ? idx : notFound;
    }

    // 사용처: kindName + foreignKey -> idx 역조회(대표 1개)
    public static int GetIdxByForeign(string kindName, int foreignKey, int notFound = -1)
    {
        if (string.IsNullOrEmpty(kindName) || foreignKey == KeyNone) return notFound;
        if (!_kindMap.TryGetValue(kindName, out var k)) return notFound;
        return GetIdxByForeignKey(k, foreignKey, notFound);
    }

    // 사용처: idx 역조회가 필요한 로직에서 Try 패턴으로 사용
    public static bool TryGetIdxByForeignKey(TextKind kind, int foreignKey, out int idx)
    {
        idx = -1;
        if (foreignKey == KeyNone || _idxByKindForeign == null) return false;

        long k = Pack2((int)kind, foreignKey);
        return _idxByKindForeign.TryGetValue(k, out idx);
    }

    // 사용처: (kindA, keyB) 2-key 패킹(딕셔너리 키)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static long Pack2(int a, int b)
    {
        unchecked
        {
            return ((long)a << 32) ^ (uint)b;
        }
    }

    // 사용처: Entry에서 언어별 텍스트 꺼내기
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string PickLang(Entry e, string lang)
    {
        if (e == null || string.IsNullOrEmpty(lang)) return null;

        switch (lang)
        {
            case "kr": return string.IsNullOrEmpty(e.kr) ? null : e.kr;
            case "en": return string.IsNullOrEmpty(e.en) ? null : e.en;
            case "jp": return string.IsNullOrEmpty(e.jp) ? null : e.jp;
            default: return string.IsNullOrEmpty(e.kr) ? null : e.kr;
        }
    }

    // 아래는 프로젝트에서 쓰던 토큰 치환 관련 유틸(기존 유지)
        // 사용처: 기존 프로젝트 호환(언어 설정을 RogueLikeData/PlayerPrefs에서 가져와 로드)
    // - RogueLikeData 타입/필드명이 바뀌어도 컴파일이 깨지지 않도록 리플렉션으로 접근한다.
    public static void LoadFromRogueLike()
    {
        string lang = ResolveLanguageFromRogueLikeOrPrefs();
        if (string.IsNullOrEmpty(lang)) lang = "kr";

        // 기존 코드들이 LoadFromRogueLike()만 호출하는 경우가 많아서,
        // fallback은 기본 en으로 고정(필요하면 PlayerPrefs에서 별도 키로 확장 가능)
        Load(lang, "en");
    }

    static string ResolveLanguageFromRogueLikeOrPrefs()
    {
        // 1) PlayerPrefs 우선
        string lang = null;

        if (PlayerPrefs.HasKey("Language")) lang = PlayerPrefs.GetString("Language");
        else if (PlayerPrefs.HasKey("Lang")) lang = PlayerPrefs.GetString("Lang");
        else if (PlayerPrefs.HasKey("language")) lang = PlayerPrefs.GetString("language");

        lang = NormalizeLang(lang);
        if (!string.IsNullOrEmpty(lang)) return lang;

        // 2) RogueLikeData.Instance.* 에서 추론 (리플렉션)
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type t = null;

            // 이름 정확 일치 우선
            for (int i = 0; i < assemblies.Length && t == null; i++)
            {
                t = assemblies[i].GetType("RogueLikeData", throwOnError: false);
            }

            // 못 찾으면 끝
            if (t == null) return null;

            object instance = null;

            // Instance / instance (static property 우선)
            var pInst = t.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (pInst != null) instance = pInst.GetValue(null, null);

            if (instance == null)
            {
                var fInst = t.GetField("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                         ?? t.GetField("instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

                if (fInst != null) instance = fInst.GetValue(null);
            }

            if (instance == null) return null;

            // 흔한 후보들: Language / Lang / language / lang / languageCode / locale
            lang = ReadStringMember(instance, "Language")
                ?? ReadStringMember(instance, "Lang")
                ?? ReadStringMember(instance, "language")
                ?? ReadStringMember(instance, "lang")
                ?? ReadStringMember(instance, "languageCode")
                ?? ReadStringMember(instance, "locale");

            lang = NormalizeLang(lang);
            if (!string.IsNullOrEmpty(lang)) return lang;

            // enum 같은 경우도 있을 수 있어 ToString()으로 대응
            object v = ReadObjectMember(instance, "Language")
                    ?? ReadObjectMember(instance, "Lang")
                    ?? ReadObjectMember(instance, "language")
                    ?? ReadObjectMember(instance, "lang");

            if (v != null)
            {
                lang = NormalizeLang(v.ToString());
                if (!string.IsNullOrEmpty(lang)) return lang;
            }
        }
        catch
        {
            // 로딩 단계에서 예외로 게임 멈추는 걸 막는다.
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string ReadStringMember(object obj, string name)
    {
        object v = ReadObjectMember(obj, name);
        return v as string;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static object ReadObjectMember(object obj, string name)
    {
        if (obj == null || string.IsNullOrEmpty(name)) return null;

        var t = obj.GetType();

        var p = t.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (p != null) return p.GetValue(obj, null);

        var f = t.GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (f != null) return f.GetValue(obj);

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string NormalizeLang(string s)
    {
        if (string.IsNullOrEmpty(s)) return null;

        s = s.Trim().ToLowerInvariant();

        // 흔한 표현들을 프로젝트 내부 코드(kr/en/jp)로 정규화
        // ko/kor/korean -> kr, ja/jpn/japanese -> jp
        if (s == "kr" || s == "kor" || s == "ko" || s == "korean") return "kr";
        if (s == "en" || s == "eng" || s == "english") return "en";
        if (s == "jp" || s == "ja" || s == "jpn" || s == "japanese") return "jp";

        // 혹시 "Korean"처럼 길게 들어오면 앞 2글자로도 처리
        if (s.Length >= 2)
        {
            string p2 = s.Substring(0, 2);
            if (p2 == "ko") return "kr";
            if (p2 == "en") return "en";
            if (p2 == "ja") return "jp";
        }

        return null;
    }

    // 아래는 프로젝트에서 쓰던 토큰 치환 관련 유틸(기존 유지 + IList 지원 오버로드 추가)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string ReplaceTokens(string s, string[] requireNames, string[] resultTokens, string[] textTokens)
    {
        // 기존 시그니처 유지(호환)
        return ReplaceTokens(s, (IList<string>)requireNames, (IList<string>)resultTokens, (IList<string>)textTokens);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string ReplaceTokens(string s, IList<string> requireNames, IList<string> resultTokens, IList<string> textTokens)
    {
        if (string.IsNullOrEmpty(s)) return s;

        if (requireNames != null)
        {
            for (int i = 0; i < requireNames.Count; i++)
            {
                string key = "{require[" + i + "]}";
                if (s.IndexOf(key, StringComparison.Ordinal) >= 0)
                    s = s.Replace(key, requireNames[i] ?? string.Empty);
            }
        }

        if (resultTokens != null)
        {
            for (int i = 0; i < resultTokens.Count; i++)
            {
                string key = "{result[" + i + "]}";
                if (s.IndexOf(key, StringComparison.Ordinal) >= 0)
                    s = s.Replace(key, resultTokens[i] ?? string.Empty);
            }
        }

        if (textTokens != null)
        {
            for (int i = 0; i < textTokens.Count; i++)
            {
                string key = "{text[" + i + "]}";
                if (s.IndexOf(key, StringComparison.Ordinal) >= 0)
                    s = s.Replace(key, textTokens[i] ?? string.Empty);
            }
        }

        return s;
    }

    // 사용처: 이벤트 결과 문장 합성 등(라인 배열/List를 하나의 문자열로)
    // 기존 시그니처 유지 + List 지원 오버로드 추가
    public static string ComposeLines(string[] lines, string[] requireNames, string[] resultTokens, string[] textTokens)
        => ComposeLines((IList<string>)lines, (IList<string>)requireNames, (IList<string>)resultTokens, (IList<string>)textTokens);

    public static string ComposeLines(string[] lines, string[] requireNames, string[] resultTokens)
        => ComposeLines((IList<string>)lines, (IList<string>)requireNames, (IList<string>)resultTokens, null);

    public static string ComposeLines(IList<string> lines, IList<string> requireNames, IList<string> resultTokens, IList<string> textTokens)
    {
        if (lines == null || lines.Count == 0) return string.Empty;

        var sb = new System.Text.StringBuilder(lines.Count * 16);
        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            if (string.IsNullOrEmpty(line))
            {
                sb.AppendLine();
                continue;
            }

            string s = ReplaceTokens(line, requireNames, resultTokens, textTokens);
            sb.AppendLine(s);
        }

        return sb.ToString();
    }

    public static string ComposeLines(IList<string> lines, IList<string> requireNames, IList<string> resultTokens)
        => ComposeLines(lines, requireNames, resultTokens, null);


    // 사용처: 버프/디버프 툴팁 제목 생성
    private static string GetBuffDeBuffName(int id, int grade)
    {
        string name = GameTextDB.GetByForeignKey(TextKind.BuffDeBuff, id);

        if (string.IsNullOrEmpty(name))
            name = GetFallbackBuffDeBuffName(id);

        if (id == 0)
            return $"{name} {Mathf.Clamp(grade, 1, 3)}단계";

        return name;
    }

    // 사용처: 버프/디버프 툴팁 설명 생성
    private static string GetBuffDeBuffDescription(int id, int grade, int duration)
    {
        int titleIdx = GameTextDB.GetIdxByForeignKey(TextKind.BuffDeBuff, id);
        string description = titleIdx >= 0
            ? GameTextDB.Get(TextKind.BuffDeBuff, titleIdx, id)
            : string.Empty;

        if (string.IsNullOrEmpty(description))
            description = GetFallbackBuffDeBuffDescription(id);

        string durationText = duration < 0 ? "지속" : $"{duration}턴";

        if (id == 0)
            return $"{description}\n현재 단계: {Mathf.Clamp(grade, 1, 3)} / 남은 시간: {durationText}";

        return $"{description}\n남은 시간: {durationText}";
    }

    // 사용처: GameTextDB 누락 시 최소 표시용 이름 반환
    private static string GetFallbackBuffDeBuffName(int id)
    {
        switch (id)
        {
            case 0: return "작열";
            case 1: return "상흔";
            case 2: return "연막";
            case 3: return "추적자 표식";
            case 4: return "제국 시너지";
            case 8: return "위압";
            default: return "알 수 없는 효과";
        }
    }

    // 사용처: GameTextDB 누락 시 최소 표시용 설명 반환
    private static string GetFallbackBuffDeBuffDescription(int id)
    {
        switch (id)
        {
            case 0: return "매 턴 최대 체력에 비례한 피해를 받습니다.";
            case 1: return "치유 효과를 받을 수 없습니다.";
            case 2: return "회피율이 증가합니다.";
            case 3: return "장갑이 감소한 상태입니다.";
            case 4: return "회피율이 증가합니다.";
            case 8: return "기동력이 크게 낮아진 상태입니다.";
            default: return "";
        }
    }


}
