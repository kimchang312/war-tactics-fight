using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class StagePreset
{
    public int PresetID;
    public int Chapter;
    public int Level;
    public string StageType;
    public int Value;
    public List<int> UnitList;
    public int UnitCount;
    public string Faction;
    public string Commander;
    public string CommanderID;
    public string Description;
}

public class StagePresetLoader : MonoBehaviour
{
    [Header("Google Sheet 설정")]
    [Tooltip("https://docs.google.com/spreadsheets/d/<<SHEET_ID>>/gviz/tq?tqx=out:json&sheet=시트이름")]
    public string sheetUrl =
        "https://docs.google.com/spreadsheets/d/" +
        "1wrav9yCZ4Cr_OXKW9IHfsdV76smjkZCNoVB0k0dJhTw" +
        "/gviz/tq?tqx=out:json&sheet=%ED%94%84%EB%A6%AC%EC%85%8B";

    [HideInInspector] public List<StagePreset> presets = new List<StagePreset>();

    void Start()
    {
        StartCoroutine(DownloadAndParseSheet());
    }

    IEnumerator DownloadAndParseSheet()
    {
        using var www = UnityWebRequest.Get(sheetUrl);
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[SheetLoader] 다운로드 오류: {www.error}");
            yield break;
        }

        // 1) JSONP 래퍼 제거
        string raw = www.downloadHandler.text;
        int idxOpen = raw.IndexOf('(');
        int idxClose = raw.LastIndexOf(')');
        if (idxOpen < 0 || idxClose < 0 || idxClose <= idxOpen)
        {
            Debug.LogError("[SheetLoader] JSONP 래퍼를 찾을 수 없습니다.");
            yield break;
        }
        string payload = raw.Substring(idxOpen + 1, idxClose - idxOpen - 1);

        // 2) table: 블록만 잘라내기
        int tblKey = payload.IndexOf("table:");
        if (tblKey < 0)
        {
            Debug.LogError("[SheetLoader] table 키를 찾을 수 없습니다.");
            yield break;
        }
        int braceOpen = payload.IndexOf('{', tblKey);
        int braceClose = FindMatchingBraceIndex(payload, braceOpen);
        if (braceOpen < 0 || braceClose < 0)
        {
            Debug.LogError("[SheetLoader] table 블록의 중괄호를 찾을 수 없습니다.");
            yield break;
        }
        string tableJson = payload.Substring(braceOpen, braceClose - braceOpen + 1);

        // 3) unquoted key → quoted key, single-quote → double-quote
        //    ex) {cols:[…],rows:[…]} → {"cols":[…],"rows":[…]}
        string fixedJson = Regex.Replace(tableJson, @"([\{,])\s*([A-Za-z0-9_]+)\s*:", "$1\"$2\":");
        fixedJson = fixedJson.Replace("'", "\"");

        // 4) 최종 감싸기: {"table":{…}}
        string finalJson = "{\"table\":" + fixedJson + "}";

        // 5) 파싱
        VisualizationResponse vis;
        try
        {
            vis = JsonUtility.FromJson<VisualizationResponse>(finalJson);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SheetLoader] JSON 파싱 실패: {e.Message}\n{finalJson.Substring(0, Mathf.Min(200, finalJson.Length))}…");
            yield break;
        }

        // 6) rows → StagePreset 리스트 변환
        presets.Clear();
        foreach (var row in vis.table.rows)
        {
            var c = row.c;
            presets.Add(new StagePreset
            {
                PresetID = ToInt(c, 0),
                Chapter = ToInt(c, 1),
                Level = ToInt(c, 2),
                StageType = ToStr(c, 3),
                Value = ToInt(c, 4),
                UnitList = ToIntList(c, 5),
                UnitCount = ToInt(c, 6),
                Faction = ToStr(c, 7),
                Commander = ToStr(c, 8),
                CommanderID = ToStr(c, 9),
                Description = ToStr(c, 10),
            });
        }

        Debug.Log($"[SheetLoader] 로드 완료: {presets.Count}개 레코드");
    }

    #region Helpers

    int ToInt(List<VisualizationResponse.Cell> cells, int idx)
    {
        if (idx < cells.Count && cells[idx]?.v != null &&
            int.TryParse(cells[idx].v.ToString(), out int x))
            return x;
        return 0;
    }

    string ToStr(List<VisualizationResponse.Cell> cells, int idx)
    {
        if (idx < cells.Count && cells[idx]?.v != null)
            return cells[idx].v.ToString();
        return "";
    }

    List<int> ToIntList(List<VisualizationResponse.Cell> cells, int idx)
    {
        var list = new List<int>();
        if (idx < cells.Count && cells[idx]?.v != null)
        {
            foreach (var part in cells[idx].v.ToString()
                                         .Split(',', StringSplitOptions.RemoveEmptyEntries))
                if (int.TryParse(part.Trim(), out int x))
                    list.Add(x);
        }
        return list;
    }

    // 중괄호 짝 맞추기
    int FindMatchingBraceIndex(string s, int openIdx)
    {
        int depth = 0;
        for (int i = openIdx; i < s.Length; i++)
        {
            if (s[i] == '{') depth++;
            else if (s[i] == '}')
            {
                depth--;
                if (depth == 0) return i;
            }
        }
        return -1;
    }

    #endregion

    [Serializable]
    private class VisualizationResponse
    {
        public Table table;
        [Serializable]
        public class Table
        {
            public List<Column> cols;
            public List<Row> rows;
        }
        [Serializable] public class Column { public string id; public string label; }
        [Serializable] public class Row { public List<Cell> c; }
        [Serializable] public class Cell { public object v; }
    }
}
