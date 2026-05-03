// 사용처: 유닛 bool 필드명(lightArmor 등)을 GameTextDB Idx로 매핑
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static class AbilityIdMap
{
    // 규칙: 영어(en)에서 소문자 시작 + 띄어쓰기 제거
    public static readonly Dictionary<string, int> map = new Dictionary<string, int>
    {
        { "lightArmor", 105 },
        { "heavyArmor", 106 },
        { "rangedAttack", 107 },
        { "blunt", 108 },
        { "pierce", 109 },
        { "agility", 110 },
        { "strongCharge", 111 },
        { "perfectAccuracy", 112 },
        { "slaughter", 113 },

        { "charge", 123 },
        { "defense", 124 },
        { "javelinThrow", 125 },
        { "skirmish", 126 },
        { "guard", 127 },
        { "assasination", 128 },  // 레거시 오타 유지 (기존 데이터 호환)
        { "assassination", 128 }, // RogueUnitDataBase 실제 필드명
        { "drain", 129 },
        { "overwhelm", 130 },

        { "binding", 139 },
        { "bravery", 140 },
        { "suppression", 141 },
        { "plunder", 142 },
        { "doubleshot", 143 },
        { "scorch", 144 },
        { "thorns", 145 },
        { "endless", 146 },
        { "impact", 147 },
        { "healing", 148 },
        { "lifesteal", 149 },

        { "martyrdom", 162 },
        { "wound", 163 },
        { "vengeance", 164 },
        { "counter", 165 },
        { "firstStrike", 166 },
        { "challenge", 167 },
        { "smokescreen", 168 },
        { "bluntWeapon", 108 },
        { "throwSpear", 125 },
        { "guerrilla", 126 },
        { "martyr", 162 },
        { "firststrike", 166 }
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetIdx(string abilityFieldName)
    {
        if (string.IsNullOrEmpty(abilityFieldName)) return -1;
        if (map.TryGetValue(abilityFieldName, out var id))
            return id;
        return -1;
    }
}
