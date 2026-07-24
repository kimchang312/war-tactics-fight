using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class UpgradeManager
{
    // 병종별 강화 수치를 저장하는 클래스
    public class UpgradeValues
    {
        public float healthBoost = 0;
        public float armorBoost = 0;
        public float attackDamageBoost = 0;
        public float mobilityBoost = 0;
        public float rangeBoost = 0;
        public float antiCavalryBoost = 0;
    }

    // 병종별 고정 리스트
    private readonly UpgradeValues[] upgradeValues;

    // 싱글톤 패턴 적용
    private static UpgradeManager instance;

    public static UpgradeManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new UpgradeManager();
            }
            return instance;
        }
    }

    // private 생성자로 외부에서 생성 방지
    private UpgradeManager()
    {
        // 병종 수 9개 고정
        upgradeValues = new UpgradeValues[9];
        for (int i = 0; i < 9; i++)
        {
            upgradeValues[i] = new UpgradeValues();
        }
    }

    // 특정 병종의 현재 강화 수치를 반환하는 함수
    public UpgradeValues GetUpgradeValues(int branchIdx)
    {
        return upgradeValues[branchIdx];
    }

    public void ProcessUpgrade()
    {
        int id = 1;
        var myUnits = RogueLikeData.Instance.GetMyTeam();
        foreach (var unit in myUnits)
        {
            unit.stats.RemoveModifiersBySource(SourceType.Upgrade);
        }

        int helmetValue = RelicManager.CheckRelicById(64) ? 2 : 1;

        foreach (var unit in myUnits)
        {
            int idx = unit.branchIdx;
            int atkLv = RogueLikeData.Instance.GetUpgrade(idx, true);
            int defLv = RogueLikeData.Instance.GetUpgrade(idx, false);

            bool isRunChess = RelicManager.RunChessboard();

            if (isRunChess)
            {
                atkLv = Mathf.Min(atkLv + 1, 5);
                defLv = Mathf.Min(defLv + 1, 5);
            }

            // 기본 강화 적용
            if (atkLv > 0)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.AttackDamage,
                    value = 0.1f * atkLv* helmetValue,
                    source = SourceType.Upgrade,
                    modifierId = id,
                    isPercent = false
                });
            }

            if (defLv > 0)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Health,
                    value = 0.1f * defLv * helmetValue,
                    source = SourceType.Upgrade,
                    modifierId = id,
                    isPercent = false
                });
            }

            // 병종별 특수 강화
            switch (idx)
            {
                case 0:
                    //if (atkLv == 5)
                    //unit.antiCavalry += Mathf.Floor(unit.baseAntiCavalry * 0.3f);
                    break;

                case 1:
                    if (atkLv == 5)
                    {
                        unit.stats.AddModifier(new StatModifier
                        {
                            stat = StatType.AttackDamage,
                            value = unit.baseAttackDamage * 0.15f * helmetValue,
                            source = SourceType.Upgrade,
                            modifierId = id,
                            isPercent = false
                        });
                    }
                    break;

                case 2:
                    if (atkLv == 5)
                    {
                        unit.stats.AddModifier(new StatModifier
                        {
                            stat = StatType.Range,
                            value = 1 * helmetValue,
                            source = SourceType.Upgrade,
                            modifierId = id,
                            isPercent = false
                        });
                    }
                    if (defLv == 5)
                    {
                        unit.stats.AddModifier(new StatModifier
                        {
                            stat = StatType.Mobility,
                            value = 5 * helmetValue,
                            source = SourceType.Upgrade,
                            modifierId = id,
                            isPercent = false
                        });
                    }
                    break;

                case 3:
                    if (atkLv == 5)
                    {
                        unit.stats.AddModifier(new StatModifier
                        {
                            stat = StatType.AttackDamage,
                            value = unit.baseAttackDamage * 0.15f * helmetValue,
                            source = SourceType.Upgrade,
                            modifierId = id,
                            isPercent = false
                        });
                    }
                    break;

                case 4:
                    if (atkLv == 5)
                    {
                        unit.stats.AddModifier(new StatModifier
                        {
                            stat = StatType.AttackDamage,
                            value = unit.baseAttackDamage * 0.15f * helmetValue,
                            source = SourceType.Upgrade,
                            modifierId = id,
                            isPercent = false
                        });
                    }
                    if (defLv == 5)
                    {
                        unit.stats.AddModifier(new StatModifier
                        {
                            stat = StatType.Mobility,
                            value = 5 * helmetValue,
                            source = SourceType.Upgrade,
                            modifierId = id,
                            isPercent = false
                        });
                    }
                    break;

                case 5:
                case 6:
                    if (atkLv == 5)
                    {
                        unit.stats.AddModifier(new StatModifier
                        {
                            stat = StatType.Mobility,
                            value = 5 * helmetValue,
                            source = SourceType.Upgrade,
                            modifierId = id,
                            isPercent = false
                        });
                    }
                    break;

                case 7:
                    if (atkLv == 5)
                    {
                        unit.stats.AddModifier(new StatModifier
                        {
                            stat = StatType.Range,
                            value = 1 * helmetValue,
                            source = SourceType.Upgrade,
                            modifierId = id,
                            isPercent = false
                        });
                    }
                    break;
            }

        }



    }
    private const int ClassCount = 8;

    // affinity[attacker][defender] = 배율
    private static float[,] affinity = new float[ClassCount, ClassCount]
    {
        // Defender → Spearman, Warrior, Archer, HeavyInfantry, Assassin, LightCavalry, HeavyCavalry, Support
        // Spearman
        { 0f,   0f,  0f,  0f,  0f,  0.25f,  0.25f,  0f },
        // Warrior
        { 0.35f, 0f, 0.2f, 0f, 0f, 0f, 0f, 0f },
        // Archer
        { 0f, 0f, 0f, -0.4f, 0f, 0f, -0.4f, 0f },
        // HeavyInfantry
        { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f },
        // Assassin
        { 0f, 0f, 0.4f, -0.2f, 0f, 0f, -0.2f, 0.4f },
        // LightCavalry
        { -0.25f, 0.3f, 0.25f, 0f, 0f, 0f, 0f, 0f },
        // HeavyCavalry
        { -0.25f, 0.1f, 0f, 0.4f, 0f, 0f, 0f, 0f },
        // Support
        { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f }
    };
    public static float[,] GetAffinity()
    {
        return affinity;
    }
    // 공격자와 방어자의 병종 인덱스를 입력받아 상성 배율 반환
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetAffinityMultiplier(int attackerClass, int defenderClass,bool isTeam)
    {
        if (attackerClass < 0 || attackerClass >= ClassCount ||
            defenderClass < 0 || defenderClass >= ClassCount)
            return 0f;

        var value = affinity[attackerClass, defenderClass];
        if (RelicManager.CheckRelicById(118))
        {
            WarRelic relic = RelicManager.GetRelicById(118);
            var vals = relic.GetAllValuesAsFloatListOrNull();
            if(vals != null)
            {
                if (isTeam && defenderClass == 1)
                {
                    value += vals[0];
                }else if(!isTeam && (attackerClass == 5 || attackerClass == 6))
                {
                    value += vals[1];
                }
            }
        }
        WarRelic imperialThornWall = RelicManager.GetRelicById(119);
        if (imperialThornWall != null && imperialThornWall.used)
        {
            var vals = imperialThornWall.GetAllValuesAsFloatListOrNull();
            if (vals != null && vals.Count > 1)
            {
                float reduction = vals[1] > 1f ? vals[1] * 0.01f : vals[1];
                if (!isTeam && defenderClass == 0)
                {
                    value -= reduction;
                }
            }
        }
        if (RelicManager.CheckRelicById(121) && isTeam)
        {
            WarRelic relic = RelicManager.GetRelicById(121);
            var vals = relic.GetAllValuesAsFloatListOrNull();
            if (vals != null)
            {
                if (attackerClass == 1 && defenderClass == 3)
                {
                    value += vals[0];
                }
            }
        }
        if (RelicManager.CheckRelicById(122) && !isTeam)
        {
            WarRelic relic = RelicManager.GetRelicById(122);
            var vals = relic.GetAllValuesAsFloatListOrNull();
            if(vals != null)
            {
                value += vals[0];
            }
        }
        if(RelicManager.CheckRelicById(131) && isTeam)
        {
            if(attackerClass == 5 &&  defenderClass == 6)
            {
                WarRelic relic = RelicManager.GetRelicById(131);
                var vals = relic?.GetAllValuesAsFloatListOrNull();
                if (vals != null)
                {
                    value += vals[0];
                }
            }
        }
        if (RelicManager.CheckRelicById(134))
        {
            WarRelic relic = RelicManager.GetRelicById(134);
            var vals = relic?.GetAllValuesAsFloatListOrNull();
            if (vals != null)
            {
                if (isTeam && attackerClass == 6 && defenderClass == 0)
                {
                    value += vals[0];
                }
                else if(!isTeam && attackerClass == 0  && defenderClass == 6)
                {
                    value += vals[1];
                }
            }
        }

        return value;
    }
    public static void SetAffinityMultiplier(int attackerClass, int defenderClass, float multiplier)
    {
        if (attackerClass < 0 || attackerClass >= ClassCount ||
            defenderClass < 0 || defenderClass >= ClassCount)
            return;

        affinity[attackerClass, defenderClass] = multiplier;
    }


}
[System.Serializable]
public class UnitUpgrade
{
    public int attackLevel = 0;
    public int defenseLevel = 0;
}
