using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
public class UnitDataManager : MonoBehaviour
{
    public static UnitDataManager Instance { get; private set; }
    public List<UnitDataBase> unitDataList = new List<UnitDataBase>(); // 유닛 데이터를 저장할 리스트

    // GetRowCount() 대신에 Excel 데이터의 Count를 직접 참조
    // int rowCount = GoogleSheetLoader.excel.Count;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("UnitDataManager가 초기화되었습니다.", this);
        }
        else
        {
            Debug.LogWarning("UnitDataManager가 중복 생성되었습니다.", this);
            Destroy(gameObject);
        }
    }

    private async void Start()
    {
        // 데이터를 비동기적으로 로드
        await LoadUnitDataAsync();
    }

    public async Task LoadUnitDataAsync()
    {
        GoogleSheetLoader googleSheetLoader = new GoogleSheetLoader();
        await googleSheetLoader.LoadGoogleSheetData(); // Google Sheet 데이터를 비동기적으로 로드

        // GetExcelData()로 데이터를 가져와서 처리
        List<List<string>> excelData = googleSheetLoader.GetExcelData();
        int rowCount = excelData.Count;  // Excel 데이터의 행 수

        for (int i = 0; i < rowCount; i++)
        {
            List<string> rowData = excelData[i];

            if (rowData != null && rowData.Count > 0)
            {
                UnitDataBase unitData = UnitDataBase.ConvertToUnitDataBase(rowData);
                if (unitData != null)
                {
                    if (IsValidUnitData(unitData)) { 
                    // 중복된 유닛 데이터가 있는지 확인 후 추가
                    if (!unitDataList.Exists(u => u.unitName == unitData.unitName))
                    {
                        unitDataList.Add(unitData); // 데이터를 리스트에 추가
                        Debug.Log($"유닛 추가됨: {unitData.unitName}"); // 추가된 유닛을 출력
                                                                   
                        Debug.Log("유닛 이미지 값 확인: " + unitData.unitImg);// 유닛 데이터의 unitImg 값 확인
                    }
                }
                }
                else
                {
                    Debug.LogWarning("유닛 데이터 변환 실패");
                }
            }
            
        }

        // 유닛 로드 후 처리
        if (unitDataList.Count > 0)
        {
            Debug.Log($"총 {unitDataList.Count}개의 유닛이 로드되었습니다.");
        }
        else
        {
            Debug.LogWarning("유닛 데이터가 로드되지 않았습니다.");
        }
    }

    // 유효한 유닛 데이터인지 체크하는 메서드
    private bool IsValidUnitData(UnitDataBase unit)
    {
        return !string.IsNullOrEmpty(unit.unitName) && unit.unitPrice > 0 && !string.IsNullOrEmpty(unit.unitImg);
    }


    public List<UnitDataBase> GetAllUnits()
    {
        return unitDataList;
    }

}