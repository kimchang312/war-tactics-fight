using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class CombatStatisticsManager
{
    private Dictionary<int, UnitCombatStatics> unitStats;

    public CombatStatisticsManager(Dictionary<int, UnitCombatStatics> unitStats)
    {
        this.unitStats = unitStats;
    }

    // 유닛 통계 저장 메서드
    public void SaveStatisticsToFile(string filePath)
    {
        // JSON으로 직렬화
        string json = JsonConvert.SerializeObject(unitStats, Formatting.Indented);

        // 파일에 저장
        File.WriteAllText(filePath, json);
    }
}
