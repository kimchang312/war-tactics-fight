using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
public class UnitDataManager : MonoBehaviour
{
    public static UnitDataManager Instance { get; private set; }
    public List<UnitDataBase> unitDataList = new List<UnitDataBase>(); // ���� �����͸� ������ ����Ʈ

    // GetRowCount() ��ſ� Excel �������� Count�� ���� ����
    // int rowCount = GoogleSheetLoader.excel.Count;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void Start()
    {
        // �����͸� �񵿱������� �ε�
        await LoadUnitDataAsync();
    }

    public async Task LoadUnitDataAsync()
    {
        GoogleSheetLoader googleSheetLoader = new GoogleSheetLoader();
        await googleSheetLoader.LoadGoogleSheetData(); // Google Sheet �����͸� �񵿱������� �ε�

        // GetExcelData()�� �����͸� �����ͼ� ó��
        List<List<string>> excelData = googleSheetLoader.GetExcelData();
        int rowCount = excelData.Count;  // Excel �������� �� ��

        for (int i = 0; i < rowCount; i++)
        {
            List<string> rowData = excelData[i];

            if (rowData != null && rowData.Count > 0)
            {
                UnitDataBase unitData = UnitDataBase.ConvertToUnitDataBase(rowData);
                if (unitData != null)
                {
                    // �ߺ��� ���� �����Ͱ� �ִ��� Ȯ�� �� �߰�
                    if (!unitDataList.Exists(u => u.unitName == unitData.unitName))
                    {
                        unitDataList.Add(unitData); // �����͸� ����Ʈ�� �߰�
                        Debug.Log($"���� �߰���: {unitData.unitName}"); // �߰��� ������ ���
                    }
                }
                else
                {
                    Debug.LogWarning("���� ������ ��ȯ ����");
                }
            }
            // ù ��° ���� ������ ��� (����Ʈ�� ������ �����ϴ� ���)
            if (unitDataList.Count > 0)
            {
                UnitDataBase firstUnit = unitDataList[0];
                Debug.Log($"ù ��° ���� ������ Ȯ��: �̸� = {firstUnit.unitName}, ���� = {firstUnit.unitPrice}, �̹��� = {firstUnit.unitImg}");
            }
        }
        // ���� �ε� �� ó��
        if (unitDataList.Count > 0)
        {
            Debug.Log($"�� {unitDataList.Count}���� ������ �ε�Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogWarning("���� �����Ͱ� �ε���� �ʾҽ��ϴ�.");
        }
    }

    public List<UnitDataBase> GetAllUnits()
    {
        return unitDataList;
    }
}