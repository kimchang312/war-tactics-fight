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
    System = 40,
    Tutorial = 41,
    Ability = 50,

    Branch = 51,
    Stat = 52,
    BattlefieldEffect = 55,
    Commander = 56,

}

public static class GameTextDB
{
    [Serializable]
    class Entry
    {
        // 모든 key는 정수
        public int Idx;
        public int kind;        // TextKind 값
        public string kindName; // 표시용(옵션)
        public int TitleKey;    // 제목/대표 키 (예: 이벤트ID, UI페이지ID 등)
        public int ForeignKey;  // 외래 키 (예: 유닛ID, 유산ID 등)
        public string kr;
        public string en;
        public string jp;
    }

    [Serializable] class Root { public Entry[] entries; }

    // 1-key: Idx
    static Dictionary<int, string> _byIdxCur;
    static Dictionary<int, string> _byIdxFb;

    // 2-key: (kind, TitleKey)
    static Dictionary<long, string> _byKindTitleCur;
    static Dictionary<long, string> _byKindTitleFb;

    // 2-key: (kind, ForeignKey)
    static Dictionary<long, string> _byKindForeignCur;
    static Dictionary<long, string> _byKindForeignFb;

    // 3-key: (kind, TitleKey, ForeignKey)
    struct TripleKey : IEquatable<TripleKey>
    {
        public int a, b, c;
        public TripleKey(int a, int b, int c) { this.a = a; this.b = b; this.c = c; }
        public bool Equals(TripleKey other) => a == other.a && b == other.b && c == other.c;
        public override bool Equals(object obj) => obj is TripleKey t && Equals(t);
        public override int GetHashCode()
        {
            // 간단하고 충돌 적은 혼합
            unchecked
            {
                int h = a;
                h = (h * 397) ^ b;
                h = (h * 397) ^ c;
                return h;
            }
        }
    }
    static Dictionary<TripleKey, string> _byKindTitleForeignCur;
    static Dictionary<TripleKey, string> _byKindTitleForeignFb;

    static readonly Dictionary<string, TextKind> _kindMap =
      new Dictionary<string, TextKind>(StringComparer.OrdinalIgnoreCase)
      {
          // 기본
          ["None"] = TextKind.None,
          ["UI"] = TextKind.UI,
          ["System"] = TextKind.System,
          ["Tutorial"] = TextKind.Tutorial,

          // 신규/통합
          ["Unit"] = TextKind.Unit,
          ["Ability"] = TextKind.Ability, 
          ["Tag"] = TextKind.Tag,
          ["UnitRarity"] = TextKind.UnitRarity,
          ["RelicRarity"] = TextKind.RelicRarity,
          ["RelicName"] = TextKind.RelicName,
          ["EventTitle"] = TextKind.EventTitle,
          ["Branch"] = TextKind.Branch,
          ["Stat"] = TextKind.Stat,
          ["BattlefieldEffect"] = TextKind.BattlefieldEffect,
          ["Commander"] = TextKind.Commander,
  
          // 필요하면 여기에 ItemName/ItemDesc, EventDescription 등도 매핑 가능
      };
    static string _curLang = "kr";
    static string _fbLang = "en";
    static string _resourcePath = "JsonData/GameTextData"; // Resources 경로(확장자 제외)

    // 사용처: 부팅 시 저장된 언어로 즉시 로드
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Boot()
    {
        var saved = PlayerPrefs.GetString("lang", "kr");
        Load(saved, "en");
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

            _byIdxFb = null;
            _byKindTitleFb = null;
            _byKindForeignFb = null;
            _byKindTitleForeignFb = null;
            return;
        }

        var root = JsonUtility.FromJson<Root>(ta.text);
        var arr = root?.entries ?? Array.Empty<Entry>();
        int cap = Mathf.Max(16, arr.Length * 2);

        _byIdxCur = new Dictionary<int, string>(cap);
        _byKindTitleCur = new Dictionary<long, string>(cap);
        _byKindForeignCur = new Dictionary<long, string>(cap);
        _byKindTitleForeignCur = new Dictionary<TripleKey, string>(cap);

        bool useFb = _fbLang != null;
        _byIdxFb = useFb ? new Dictionary<int, string>(cap) : null;
        _byKindTitleFb = useFb ? new Dictionary<long, string>(cap) : null;
        _byKindForeignFb = useFb ? new Dictionary<long, string>(cap) : null;
        _byKindTitleForeignFb = useFb ? new Dictionary<TripleKey, string>(cap) : null;

        for (int i = 0; i < arr.Length; i++)
        {
            var e = arr[i];
            if (e == null) continue;

            var cur = PickLang(e, _curLang);
            var fb = useFb ? PickLang(e, _fbLang) : null;

            // Idx
            if (cur != null) _byIdxCur[e.Idx] = cur;
            else if (useFb && fb != null) _byIdxFb[e.Idx] = fb;

            // (kind, TitleKey)
            if (e.TitleKey != 0 || e.kind != 0)
            {
                long k2 = Pack2(e.kind, e.TitleKey);
                if (cur != null) _byKindTitleCur[k2] = cur;
                else if (useFb && fb != null) _byKindTitleFb[k2] = fb;
            }

            // (kind, ForeignKey)
            if (e.ForeignKey != 0 || e.kind != 0)
            {
                long k2f = Pack2(e.kind, e.ForeignKey);
                if (cur != null) _byKindForeignCur[k2f] = cur;
                else if (useFb && fb != null) _byKindForeignFb[k2f] = fb;
            }

            // (kind, TitleKey, ForeignKey)
            if ((e.TitleKey != 0 || e.ForeignKey != 0) || e.kind != 0)
            {
                var k3 = new TripleKey(e.kind, e.TitleKey, e.ForeignKey);
                if (cur != null) _byKindTitleForeignCur[k3] = cur;
                else if (useFb && fb != null) _byKindTitleForeignFb[k3] = fb;
            }
        }

        Resources.UnloadAsset(ta);
    }

    // 사용처: 리소스 파일 경로(확장자 제외) 변경
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetResourcePath(string resourcesPathWithoutExt)
    {
        if (!string.IsNullOrEmpty(resourcesPathWithoutExt))
            _resourcePath = resourcesPathWithoutExt;
    }

    // 사용처: Idx로 단일 텍스트 조회
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Get(int idx)
    {
        if (_byIdxCur != null && _byIdxCur.TryGetValue(idx, out var v)) return v;
        if (_byIdxFb != null && _byIdxFb.TryGetValue(idx, out v)) return v;
#if UNITY_EDITOR
        Debug.LogWarning($"[GameTextDB] Missing Idx: {idx}");
#endif
        return string.Empty;
    }

    // 사용처: (kind, TitleKey)로 조회
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    // 사용처: (kind, ForeignKey)로 조회
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    // 사용처: (kind, TitleKey, ForeignKey)로 조회
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Get(TextKind kind, int titleKey, int foreignKey)
    {
        var k = new TripleKey((int)kind, titleKey, foreignKey);
        if (_byKindTitleForeignCur != null && _byKindTitleForeignCur.TryGetValue(k, out var v)) return v;
        if (_byKindTitleForeignFb != null && _byKindTitleForeignFb.TryGetValue(k, out v)) return v;
#if UNITY_EDITOR
        Debug.LogWarning($"[GameTextDB] Missing (kind,TitleKey,ForeignKey): ({kind},{titleKey},{foreignKey})");
#endif
        return string.Empty;
    }

    // 사용처: 문자열 kindName + TitleKey로 조회
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Get(string kindName, int titleKey)
    {
        if (string.IsNullOrEmpty(kindName)) return string.Empty;
        return _kindMap.TryGetValue(kindName, out var k) ? GetByTitleKey(k, titleKey) : string.Empty;
    }

    // 사용처: 문자열 kindName + ForeignKey로 조회
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetByForeign(string kindName, int foreignKey)
    {
        if (string.IsNullOrEmpty(kindName)) return string.Empty;
        return _kindMap.TryGetValue(kindName, out var k) ? GetByForeignKey(k, foreignKey) : string.Empty;
    }

    // 사용처: 문자열 kindName + (TitleKey, ForeignKey)로 조회
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Get(string kindName, int titleKey, int foreignKey)
    {
        if (string.IsNullOrEmpty(kindName)) return string.Empty;
        return _kindMap.TryGetValue(kindName, out var k) ? Get(k, titleKey, foreignKey) : string.Empty;
    }

    // 사용처: RogueLikeData의 언어 코드와 동기화(코드 유지)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string GetLangCodeFromRogueLike()
    {
        int lang = RogueLikeData.Instance.GetLanguage();
        if (lang == 0) return "kr";
        if (lang == 2) return "jp";
        return "en";
    }

    // 사용처: RogueLikeData의 언어 설정으로 재로딩
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LoadFromRogueLike()
    {
        var code = GetLangCodeFromRogueLike();
        Load(code, "en");
    }

    // 사용처: 로케일별 문자열 선택
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string PickLang(Entry e, string lang)
    {
        if (lang == "kr") { return string.IsNullOrEmpty(e.kr) ? null : e.kr; }
        if (lang == "jp") { return string.IsNullOrEmpty(e.jp) ? null : e.jp; }
        return string.IsNullOrEmpty(e.en) ? null : e.en;
    }

    // 내부: 2-key 패킹
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static long Pack2(int a, int b)
    {
        unchecked
        {
            return ((long)a << 32) | (uint)b;
        }
    }

    public static string ReplaceTokens(
        string line,
        List<string> requireNames,
        List<string> resultTokens,
        List<string> textTokens)
    {
        if (string.IsNullOrEmpty(line))
            return line;

        string s = line;

        // {require[i]} 치환 (이벤트 유닛 이름용)
        if (requireNames != null)
        {
            for (int i = 0; i < requireNames.Count; i++)
            {
                s = s.Replace($"{{require[{i}]}}", requireNames[i] ?? "");
            }
        }

        // {result[i]} 치환 (이벤트 결과 토큰용)
        if (resultTokens != null)
        {
            for (int i = 0; i < resultTokens.Count; i++)
            {
                string val = resultTokens[i] ?? "";
                s = s.Replace($"{{result[{i}]}}", val);

                // {text[i]}를 resultTokens와 동기로 쓰고 싶다면 여기서도 같이 치환
                s = s.Replace($"{{text[{i}]}}", val);
            }
        }

        // 별도로 넘겨주는 textTokens가 있으면 그것도 {text[i]}에 매핑
        if (textTokens != null)
        {
            for (int i = 0; i < textTokens.Count; i++)
            {
                string val = textTokens[i] ?? "";
                s = s.Replace($"{{text[{i}]}}", val);
            }
        }

        return s;
    }

    // 사용처: 여러 줄(List<string>) 템플릿을 합쳐 결과 문자열 만들 때 사용
    public static string ComposeLines(
        IReadOnlyList<string> lines,
        List<string> requireNames,
        List<string> resultTokens,
        List<string> textTokens = null)
    {
        if (lines == null || lines.Count == 0)
            return string.Empty;

        var sb = new System.Text.StringBuilder(256);

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

    public static string GetFormattedByForeignKey(
    TextKind kind,
    int foreignKey,
    params string[] textTokens)
    {
        string raw = GetByForeignKey(kind, foreignKey);
        if (string.IsNullOrEmpty(raw) || textTokens == null || textTokens.Length == 0)
            return raw;

        // textTokens를 List로 래핑해서 {text[i]} 치환
        var textList = new List<string>(textTokens.Length);
        for (int i = 0; i < textTokens.Length; i++)
            textList.Add(textTokens[i]);

        // 한 줄짜리 텍스트라서 lines = new[] { raw } 로 넘김
        return ComposeLines(
            new[] { raw },   // lines
            null,            // requireNames 없음
            null,            // resultTokens 없음
            textList         // {text[i]}에 들어갈 값
        );
    }
}
