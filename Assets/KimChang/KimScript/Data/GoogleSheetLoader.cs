using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleSheetLoader
{
    private List<List<string>> excel;               // 일반 데이터
    private List<List<string>> unitExcel;           // 유닛 데이터 (원본 리스트)
    private Dictionary<int, List<string>> unitDataCache; // 유닛 데이터 캐시 (idx → row)
    private List<RogueUnitDataBase> allUnits;
    private const string googleSheetURL = "https://docs.google.com/spreadsheets/d/1B_imPK0NF8GQ4tmMgGxOykMRguaBjDgnou8zLAYejCU/export?format=tsv&range=A4:AH";
    private const string unitStateSheetURL = "https://docs.google.com/spreadsheets/d/1jWndX9egtj8QCLwxqWRKbU1rKKDpY5ybf99HY0MVUig/export?format=tsv&range=A4:BD70";

    public List<List<string>> GetExcelData() => excel;
    public List<List<string>> GetUnitExcelData() => unitExcel;
    
    private static GoogleSheetLoader _instance;
    public static GoogleSheetLoader Instance => _instance ??= new GoogleSheetLoader();

    private GoogleSheetLoader() {} // 외부 생성 금지
    // 일반 데이터 로드
    public async Task LoadGoogleSheetData()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(googleSheetURL))
        {
            var operation = www.SendWebRequest();

            while (!operation.isDone) await Task.Yield();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {www.error}");
            }
            else
            {
                string sheetData = www.downloadHandler.text;
                ProcessData(sheetData);
            }
        }
    }

    private void ProcessData(string sheetData)
    {
        excel = new List<List<string>>();
        string[] rows = sheetData.Split('\n');

        foreach (string row in rows)
        {
            string[] columns = row.Split('\t');
            excel.Add(new List<string>(columns));
        }
    }

    // 유닛 데이터 로드 + 캐싱
    public async Task LoadUnitSheetData()
    {
        // 이미 캐시되어 있다면 다시 불러오지 않음
        if (unitDataCache != null) return;

        using (UnityWebRequest www = UnityWebRequest.Get(unitStateSheetURL))
        {
            var operation = www.SendWebRequest();

            while (!operation.isDone) await Task.Yield();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {www.error}");
            }
            else
            {
                string sheetData = www.downloadHandler.text;
                ProcessUnitData(sheetData);
            }
        }
    }

    private void ProcessUnitData(string sheetData)
    {
        unitExcel = new List<List<string>>();
        unitDataCache = new Dictionary<int, List<string>>();

        string[] rows = sheetData.Split('\n');

        foreach (string row in rows)
        {
            string[] columns = row.Split('\t');
            List<string> rowData = new List<string>(columns);

            while (rowData.Count < 54) // BB까지 채우기
                rowData.Add("");

            unitExcel.Add(rowData);

            // idx는 0번째 열
            if (int.TryParse(rowData[0], out int idx))
            {
                if (!unitDataCache.ContainsKey(idx))
                    unitDataCache.Add(idx, rowData);
            }
        }
    }

    // 캐시된 유닛 데이터 가져오기
    public List<string> GetRowUnitData(int unitIdx)
    {
        if (unitDataCache != null && unitDataCache.TryGetValue(unitIdx, out var rowData))
            return rowData;
        return null;
    }

    // 캐시 초기화
    public void ClearUnitCache()
    {
        unitDataCache = null;
        unitExcel = null;
    }
    public RogueUnitDataBase GetRandomUnit()
    {
        if (unitDataCache == null || unitDataCache.Count == 0)
        {
            Debug.LogError("유닛 데이터가 로드되지 않았습니다.");
            return null;
        }

        // idx가 0~51 사이인 유닛만 필터링
        List<int> validKeys = new List<int>();
        foreach (var key in unitDataCache.Keys)
        {
            if (key >= 0 && key <= 51)
                validKeys.Add(key);
        }

        if (validKeys.Count == 0)
        {
            Debug.LogWarning("0~51 범위의 유닛이 존재하지 않습니다.");
            return null;
        }

        int randomIdx = validKeys[Random.Range(0, validKeys.Count)];
        List<string> row = GetRowUnitData(randomIdx);
        if (row != null)
        {
            return RogueUnitDataBase.ConvertToUnitDataBase(row);
        }

        return null;
    }
    public List<RogueUnitDataBase> GetAllUnitsAsObject()
    {
        if(allUnits !=null) return allUnits;
        if (unitDataCache == null) return new List<RogueUnitDataBase>();
        
        List<RogueUnitDataBase> result = new List<RogueUnitDataBase>();

        foreach (var row in unitDataCache.Values)
        {
            var unit = RogueUnitDataBase.ConvertToUnitDataBase(row);
            if (unit != null)
                result.Add(unit);
        }
        allUnits = result;
        return allUnits;
    }

}
