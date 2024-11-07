using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleSheetLoader
{
    private List<List<string>> excel; // �����͸� ������ ����Ʈ
    private const string googleSheetURL = "https://docs.google.com/spreadsheets/d/1B_imPK0NF8GQ4tmMgGxOykMRguaBjDgnou8zLAYejCU/export?format=tsv&range=A4:AH";


    //�޼��带 �߰��Ͽ� ������ �� �ְ� �ϴ¹�� (�ӽ�) UnitDataManager ���ٺҰ�
    public List<List<string>> GetExcelData()
    {
        return excel;
    }

    // �����͸� �ε��ϴ� �񵿱� �޼���
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
        excel = new List<List<string>>(); // ����Ʈ �ʱ�ȭ

        foreach (string row in rows)
        {
            string[] columns = row.Split('\t');
            excel.Add(new List<string>(columns)); // ����Ʈ�� �� ���� �߰�
        }
    }

    // Ư�� ���� �����͸� ��ȯ�ϴ� �޼���
    public List<string> GetRowData(int rowIndex)
    {
        if (excel != null && rowIndex >= 0 && rowIndex < excel.Count)
        {

            return excel[rowIndex]; // ������ ���� �����͸� ��ȯ
        }
        else
        {
            return null;
        }
    }
    
}