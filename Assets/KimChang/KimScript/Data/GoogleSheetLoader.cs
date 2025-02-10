using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleSheetLoader
{
    private List<List<string>> excel; // 기존 데이터 리스트
    private List<List<string>> unitExcel; // 유닛 데이터 리스트

    private const string googleSheetURL = "https://docs.google.com/spreadsheets/d/1B_imPK0NF8GQ4tmMgGxOykMRguaBjDgnou8zLAYejCU/export?format=tsv&range=A4:AH";
    private const string unitStateSheetURL = "https://docs.google.com/spreadsheets/d/1jWndX9egtj8QCLwxqWRKbU1rKKDpY5ybf99HY0MVUig/export?format=tsv&range=A4:BA62";

    public List<List<string>> GetExcelData() => excel;
    public List<List<string>> GetUnitExcelData() => unitExcel;

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

    // 유닛 데이터 로드
    public async Task LoadUnitSheetData()
    {
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
        string[] rows = sheetData.Split('\n');

        foreach (string row in rows)
        {
            string[] columns = row.Split('\t');
            List<string> rowData = new List<string>(columns);

            // BB62까지 데이터가 부족한 경우 빈 값 추가
            while (rowData.Count < 54) // BB - A = 54
            {
                rowData.Add(""); // 빈 값 채우기
            }

            unitExcel.Add(rowData);
        }
    }

    // 특정 행의 데이터를 반환하는 메서드
    public List<string> GetRowData(int rowIndex)
    {
        if (excel != null && rowIndex >= 0 && rowIndex < excel.Count)
            return excel[rowIndex];
        return null;
    }

    public List<string> GetRowUnitData(int rowIndex)
    {
        if (unitExcel != null && rowIndex >= 0 && rowIndex < unitExcel.Count)
            return unitExcel[rowIndex];
        return null;
    }
}
