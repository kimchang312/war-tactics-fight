using System;
using System.Collections.Generic;

/// <summary>
/// Stage preset data uses 1-20 for elite commanders and Korean names for bosses.
/// Battle code uses the canonical commander IDs from the commander specification.
/// </summary>
public static class CommanderCatalog
{
    private static readonly Dictionary<string, int> BossIds = new(StringComparer.Ordinal)
    {
        { "아마록", 200 },
        { "브루노", 201 },
        { "시리온", 202 },
        { "발레릭", 203 },
        { "그론달", 204 },
        { "에레보스", 205 },
        { "라자루스", 206 },
        { "아그마르", 207 },
        { "토르단", 208 },
        { "오르테온", 209 },
        { "아스모데우스", 210 },
        { "호쉬", 211 },
        { "슈타인", 212 },
        { "아지라스", 213 },
        { "크롬홀드", 214 },
        { "벨페고르", 215 },
        { "멜세덱", 216 }
    };

    public static int GetId(StagePreset preset)
    {
        if (preset == null)
            return 0;

        return GetId(preset.StageType, preset.CommanderNumericId, preset.Commander);
    }

    public static int GetId(StageType stageType, int? eliteCommanderNumericId, string commanderName)
    {
        return GetId(stageType.ToString(), eliteCommanderNumericId, commanderName);
    }

    public static int GetId(string stageType, int? eliteCommanderNumericId, string commanderName)
    {
        if (string.Equals(stageType, "elite", StringComparison.OrdinalIgnoreCase)
            || string.Equals(stageType, nameof(StageType.Elite), StringComparison.OrdinalIgnoreCase))
        {
            int numericId = eliteCommanderNumericId ?? 0;
            return numericId >= 1 && numericId <= 20 ? 99 + numericId : 0;
        }

        if (!string.Equals(stageType, "boss", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(stageType, nameof(StageType.Boss), StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (string.IsNullOrWhiteSpace(commanderName))
            return 0;

        return BossIds.TryGetValue(commanderName.Trim(), out int id) ? id : 0;
    }

    public static bool IsKnownId(int commanderId)
    {
        return (commanderId >= 100 && commanderId <= 119)
            || (commanderId >= 200 && commanderId <= 216);
    }
}
