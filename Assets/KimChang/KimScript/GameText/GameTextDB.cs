using System;
using System.Collections.Generic;
using UnityEngine;

/// 사용처: 게임 텍스트를 전역에서 빠르게 조회하기 위한 싱글톤 없는 정적 DB
/// - 게임 시작 시 1회 자동 로드 (RuntimeInitializeOnLoadMethod)
/// - 이후 O(1) 조회: 이름/Idx 둘 다 지원
/// - 언어 변경 시 Load 호출로 재구성
public static class GameTextDB
{
    [Serializable]
    class Entry
    {
        public int Idx;          // JSON: Idx
        public string Name;      // JSON: Name (비어있을 수 있음)
        public string Kind;      // JSON: Kind (참고용)
        public string kr;        // JSON: kr
        public string en;        // JSON: en
        public string jp;        // JSON: jp
        public string TitleKey;  // JSON: TitleKey (참고용)
        public string FoeignKey; // JSON: FoeignKey (원문 철자 유지)
    }

    [Serializable] class Root { public Entry[] entries; }

    // 현재언어/폴백언어 사전
    static Dictionary<string, string> _byNameCur;
    static Dictionary<int, string> _byIdxCur;
    static Dictionary<string, string> _byNameFb;
    static Dictionary<int, string> _byIdxFb;

    static string _curLang = "kr";
    static string _fbLang = "en";
    static string _resourcePath = "JsonData/GameTextData"; // Resources 내 경로


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
            Debug.LogError("[GameTextDB] Text/GameTextData.json 이 없거나 비었습니다.");
#endif
            _byNameCur = new Dictionary<string, string>(0);
            _byIdxCur = new Dictionary<int, string>(0);
            _byNameFb = null;
            _byIdxFb = null;
            return;
        }

        var root = JsonUtility.FromJson<Root>(ta.text);
        var arr = root?.entries ?? Array.Empty<Entry>();
        int cap = Mathf.Max(16, arr.Length * 2); // 해시 충돌 여유분

        _byNameCur = new Dictionary<string, string>(cap);
        _byIdxCur = new Dictionary<int, string>(cap);
        _byNameFb = _fbLang != null ? new Dictionary<string, string>(cap) : null;
        _byIdxFb = _fbLang != null ? new Dictionary<int, string>(cap) : null;

        // 성능: LINQ 미사용, for 루프
        for (int i = 0; i < arr.Length; i++)
        {
            var e = arr[i];
            if (e == null) continue;

            // 현재 언어 문자열
            var cur = PickLang(e, _curLang);
            // 폴백 언어 문자열
            var fb = _fbLang != null ? PickLang(e, _fbLang) : null;

            // Idx 인덱싱 (0 이상이면 사용)
            // 중복 Idx가 있다면 마지막 값으로 갱신
            if (cur != null) _byIdxCur[e.Idx] = cur;
            else if (_byIdxFb != null && fb != null) _byIdxFb[e.Idx] = fb;

            // Name 인덱싱 (비어있지 않을 때만)
            if (!string.IsNullOrEmpty(e.Name))
            {
                if (cur != null) _byNameCur[e.Name] = cur;
                else if (_byNameFb != null && fb != null) _byNameFb[e.Name] = fb;
            }
        }

        // 텍스트 애셋 즉시 언로드로 메모리 회수 유도
        Resources.UnloadAsset(ta);
    }

    // 사용처: 리소스 경로를 바꿔야 할 때(커스텀 빌드 파이프라인에서)
    public static void SetResourcePath(string resourcesPathWithoutExt)
    {
        _resourcePath = resourcesPathWithoutExt ?? _resourcePath;
    }

    // 사용처: UI 등에 Name 키로 즉시 조회
    public static string Get(string name)
    {
        if (name != null && _byNameCur != null && _byNameCur.TryGetValue(name, out var v)) return v;
        if (name != null && _byNameFb != null && _byNameFb.TryGetValue(name, out v)) return v;
#if UNITY_EDITOR
        Debug.LogWarning($"[GameTextDB] Missing Name: {name}");
#endif
        return name ?? string.Empty;
    }

    // 사용처: UI 등에 Idx로 즉시 조회
    public static string Get(int idx)
    {
        if (_byIdxCur != null && _byIdxCur.TryGetValue(idx, out var v)) return v;
        if (_byIdxFb != null && _byIdxFb.TryGetValue(idx, out v)) return v;
#if UNITY_EDITOR
        Debug.LogWarning($"[GameTextDB] Missing Idx: {idx}");
#endif
        return string.Empty;
    }

    // 사용처: 서식 문자열 치환(Name). 예) GameTextDB.F("UI_FACTION_IS", factionName)
    public static string F(string name, params object[] args)
    {
        var raw = Get(name);
        return (args == null || args.Length == 0) ? raw : string.Format(raw, args);
    }

    // 사용처: 서식 문자열 치환(Idx). 예) GameTextDB.F(14, playerName)
    public static string F(int idx, params object[] args)
    {
        var raw = Get(idx);
        return (args == null || args.Length == 0) ? raw : string.Format(raw, args);
    }

    // 내부: 엔트리에서 언어 선택
    static string PickLang(Entry e, string lang)
    {
        // 성능을 위해 분기 최소화
        if (lang == "kr") { return string.IsNullOrEmpty(e.kr) ? null : e.kr; }
        if (lang == "jp") { return string.IsNullOrEmpty(e.jp) ? null : e.jp; }
        /* default en */
        return string.IsNullOrEmpty(e.en) ? null : e.en;
    } 
    // 사용처: RogueLikeData의 language(int) → "kr"/"en"/"jp" 매핑
    static string GetLangCodeFromRogueLike()
    {
        int lang = RogueLikeData.Instance.GetLanguage();
        // 0=kr, 1=en, 2=jp (추가 언어 확장 여지)
        if (lang == 0) return "kr";
        if (lang == 2) return "jp";
        return "en";
    }
    // 사용처: RogueLikeData.SetLanguage(...) 이후 한 번만 호출해주면 즉시 반영
    public static void LoadFromRogueLike()
    {
        var code = GetLangCodeFromRogueLike();
        Load(code, "en");
    }
}
