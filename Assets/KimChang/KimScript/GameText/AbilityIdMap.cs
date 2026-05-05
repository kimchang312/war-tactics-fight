// 사용처: 유닛 bool 필드명(lightArmor 등)을 GameTextDB Idx로 매핑
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static class AbilityIdMap
{
    // 규칙: 영어(en)에서 소문자 시작 + 띄어쓰기 제거
    private static readonly Dictionary<string, int> map = new()
{
    { "lightArmor", 105 },
    { "heavyArmor", 106 },
    { "rangedAttack", 107 },
    { "bluntWeapon", 108 },
    { "pierce", 109 },
    { "agility", 110 },
    { "strongCharge", 111 },
    { "perfectAccuracy", 112 },
    { "slaughter", 113 },

    { "solidarity", 139 },
    { "bravery", 140 },
    { "subjugation", 141 },
    { "looting", 142 },
    { "rapidFire", 143 },
    { "burning", 144 },
    { "thorns", 145 },
    { "infinity", 146 },
    { "impact", 147 },
    { "cure", 148 },
    { "bloodSucking", 149 },

    { "charge", 123 },
    { "defense", 124 },
    { "throwSpear", 125 },
    { "guerrilla", 126 },
    { "guard", 127 },
    { "assassination", 128 },
    { "drain", 129 },
    { "overwhelm", 130 },

    { "martyrdom", 162 },
    { "scar", 163 },
    { "revenge", 164 },
    { "counterattack", 165 },
    { "preemptiveStrike", 166 },
    { "challenge", 167 },
    { "smokeBomb", 168 },
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
