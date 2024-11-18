using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleSheetLoader
{
    private List<List<string>> excel; // 데이터를 저장할 리스트
    private const string googleSheetURL = "https://docs.google.com/spreadsheets/d/1B_imPK0NF8GQ4tmMgGxOykMRguaBjDgnou8zLAYejCU/export?format=tsv&range=A4:AH";



    //메서드를 추가하여 접근할 수 있게 하는방법 (임시) UnitDataManager 접근불가
    public List<List<string>> GetExcelData()
    {
        return excel;
    }

    // 데이터를 로드하는 비동기 메서드
    public async Task LoadGoogleSheetData()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(googleSheetURL))
        {
            var operation = www.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
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
        string[] rows = sheetData.Split('\n');
        excel = new List<List<string>>(); // 리스트 초기화

        foreach (string row in rows)
        {
            string[] columns = row.Split('\t');
            excel.Add(new List<string>(columns)); // 리스트에 각 열을 추가
        }
    }

    // 특정 행의 데이터를 반환하는 메서드
    public List<string> GetRowData(int rowIndex)
    {
        if (excel != null && rowIndex >= 0 && rowIndex < excel.Count)
        {

            return excel[rowIndex]; // 지정된 행의 데이터를 반환
        }
        else
        {
            return null;
        }
    }
}

