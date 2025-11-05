using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameTextDB
{
    [Serializable]
    class Entry
    {
        public int Idx;
        public string Name;
        public string Kind;
        public string kr;
        public string en;
        public string jp;
        public string TitleKey;
        public string ForeignKey;
    }

    [Serializable] class Root { public Entry[] entries; }

    // 현재언어/폴백언어 사전 (Name / Idx)
    static Dictionary<string, string> _byNameCur;
    static Dictionary<int, string> _byIdxCur;
    static Dictionary<string, string> _byNameFb;
    static Dictionary<int, string> _byIdxFb;

    // 현재언어/폴백언어 사전 (TitleKey / ForeignKey)
    static Dictionary<string, string> _byTitleKeyCur;
    static Dictionary<string, string> _byForeignKeyCur;
    static Dictionary<string, string> _byTitleKeyFb;
    static Dictionary<string, string> _byForeignKeyFb;

    static string _curLang = "kr";
    static string _fbLang = "en";
    static string _resourcePath = "JsonData/GameTextData"; // Resources 경로(확장자 제외)

    // 사용처: 부팅 시 저장된 언어로 즉시 로드
    public static void Boot()
    {
        var saved = PlayerPrefs.GetString("lang", "kr");
        Load(saved, "en");
    }

    // 사용처: 옵션 등에서 언어 변경 시 재로딩. 예) GameTextDB.Load("jp")
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
            _byNameCur = new Dictionary<string, string>(0);
            _byIdxCur = new Dictionary<int, string>(0);
            _byTitleKeyCur = new Dictionary<string, string>(0);
            _byForeignKeyCur = new Dictionary<string, string>(0);

            _byNameFb = null;
            _byIdxFb = null;
            _byTitleKeyFb = null;
            _byForeignKeyFb = null;
            return;
        }

        var root = JsonUtility.FromJson<Root>(ta.text);
        var arr = root?.entries ?? Array.Empty<Entry>();
        int cap = Mathf.Max(16, arr.Length * 2); // 충돌여유로 여유 용량 확보

        _byNameCur = new Dictionary<string, string>(cap);
        _byIdxCur = new Dictionary<int, string>(cap);
        _byTitleKeyCur = new Dictionary<string, string>(cap);
        _byForeignKeyCur = new Dictionary<string, string>(cap);

        bool useFb = _fbLang != null;
        _byNameFb = useFb ? new Dictionary<string, string>(cap) : null;
        _byIdxFb = useFb ? new Dictionary<int, string>(cap) : null;
        _byTitleKeyFb = useFb ? new Dictionary<string, string>(cap) : null;
        _byForeignKeyFb = useFb ? new Dictionary<string, string>(cap) : null;

        for (int i = 0; i < arr.Length; i++)
        {
            var e = arr[i];
            if (e == null) continue;

            var cur = PickLang(e, _curLang);
            var fb = useFb ? PickLang(e, _fbLang) : null;

            // Idx 매핑
            if (cur != null) _byIdxCur[e.Idx] = cur;
            else if (useFb && fb != null) _byIdxFb[e.Idx] = fb;

            // Name 매핑
            if (!string.IsNullOrEmpty(e.Name))
            {
                if (cur != null) _byNameCur[e.Name] = cur;
                else if (useFb && fb != null) _byNameFb[e.Name] = fb;
            }

            // TitleKey 매핑
            if (!string.IsNullOrEmpty(e.TitleKey))
            {
                if (cur != null) _byTitleKeyCur[e.TitleKey] = cur;
                else if (useFb && fb != null) _byTitleKeyFb[e.TitleKey] = fb;
            }

            // ForeignKey 매핑
            if (!string.IsNullOrEmpty(e.ForeignKey))
            {
                if (cur != null) _byForeignKeyCur[e.ForeignKey] = cur;
                else if (useFb && fb != null) _byForeignKeyFb[e.ForeignKey] = fb;
            }
        }

        Resources.UnloadAsset(ta);
    }

    // 사용처: 리소스 파일 경로(확장자 제외) 변경이 필요할 때
    public static void SetResourcePath(string resourcesPathWithoutExt)
    {
        if (!string.IsNullOrEmpty(resourcesPathWithoutExt))
            _resourcePath = resourcesPathWithoutExt;
    }

    // 사용처: Name 키로 텍스트 조회
    public static string Get(string name)
    {
        if (name != null && _byNameCur != null && _byNameCur.TryGetValue(name, out var v)) return v;
        if (name != null && _byNameFb != null && _byNameFb.TryGetValue(name, out v)) return v;
#if UNITY_EDITOR
        Debug.LogWarning($"[GameTextDB] Missing Name: {name}");
#endif
        return name ?? string.Empty;
    }

    // 사용처: Idx 키로 텍스트 조회
    public static string Get(int idx)
    {
        if (_byIdxCur != null && _byIdxCur.TryGetValue(idx, out var v)) return v;
        if (_byIdxFb != null && _byIdxFb.TryGetValue(idx, out v)) return v;
#if UNITY_EDITOR
        Debug.LogWarning($"[GameTextDB] Missing Idx: {idx}");
#endif
        return string.Empty;
    }

    /// <summary>
    /// GameText의 id
    /// </summary>
    public static string GetByTitleKey(string titleKey)
    {
        if (titleKey != null && _byTitleKeyCur != null && _byTitleKeyCur.TryGetValue(titleKey, out var v)) return v;
        if (titleKey != null && _byTitleKeyFb != null && _byTitleKeyFb.TryGetValue(titleKey, out v)) return v;
#if UNITY_EDITOR
        Debug.LogWarning($"[GameTextDB] Missing TitleKey: {titleKey}");
#endif
        return titleKey ?? string.Empty;
    }

   /// <summary>
   /// 유닛idx,유산idx,이벤트idx,아이템idx 등
   /// </summary>
    public static string GetByForeignKey(string foreignKey)
    {
        if (foreignKey != null && _byForeignKeyCur != null && _byForeignKeyCur.TryGetValue(foreignKey, out var v)) return v;
        if (foreignKey != null && _byForeignKeyFb != null && _byForeignKeyFb.TryGetValue(foreignKey, out v)) return v;
#if UNITY_EDITOR
        Debug.LogWarning($"[GameTextDB] Missing ForeignKey: {foreignKey}");
#endif
        return foreignKey ?? string.Empty;
    }

    // 사용처: Name 기반 서식 문자열 포맷
    public static string F(string name, params object[] args)
    {
        var raw = Get(name);
        return (args == null || args.Length == 0) ? raw : string.Format(raw, args);
    }

    // 사용처: Idx 기반 서식 문자열 포맷
    public static string F(int idx, params object[] args)
    {
        var raw = Get(idx);
        return (args == null || args.Length == 0) ? raw : string.Format(raw, args);
    }

    // 사용처: TitleKey 기반 서식 문자열 포맷
    public static string FTitle(string titleKey, params object[] args)
    {
        var raw = GetByTitleKey(titleKey);
        return (args == null || args.Length == 0) ? raw : string.Format(raw, args);
    }

    // 사용처: ForeignKey 기반 서식 문자열 포맷
    public static string FForeign(string foreignKey, params object[] args)
    {
        var raw = GetByForeignKey(foreignKey);
        return (args == null || args.Length == 0) ? raw : string.Format(raw, args);
    }

    // 사용처: 언어 코드별 텍스트 선택
    static string PickLang(Entry e, string lang)
    {
        if (lang == "kr") { return string.IsNullOrEmpty(e.kr) ? null : e.kr; }
        if (lang == "jp") { return string.IsNullOrEmpty(e.jp) ? null : e.jp; }
        return string.IsNullOrEmpty(e.en) ? null : e.en;
    }

    // 사용처: RogueLikeData의 언어 설정과 동기화
    static string GetLangCodeFromRogueLike()
    {
        int lang = RogueLikeData.Instance.GetLanguage();
        if (lang == 0) return "kr";
        if (lang == 2) return "jp";
        return "en";
    }

    // 사용처: RogueLikeData의 언어 설정으로 재로딩
    public static void LoadFromRogueLike()
    {
        var code = GetLangCodeFromRogueLike();
        Load(code, "en");
    }
}
