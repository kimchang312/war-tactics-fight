using System;
using System.Collections.Generic;

public enum StatType { Health, AttackDamage, Armor, Range, Mobility }
public enum SourceType { Relic, Trait, Skill, Buff, Morale, Synergy, Upgrade, Field, Passive, Commander }

[Serializable]
public class StatModifier
{
    public StatType stat;
    public float value;
    public SourceType source;
    public int modifierId;
    public bool isPercent;
    public bool isMultiplicativePercent;
}

[Serializable]
public class StatBlock
{
    private const float MinArmor = 0f;
    private const float MinMobility = 1f;

    public float baseHealth;
    public float baseAttackDamage;
    public float baseArmor;
    public float baseRange;
    public float baseMobility;

    private readonly List<StatModifier> modifiers = new();

    public void AddModifier(StatModifier mod) => modifiers.Add(mod);

    public void RemoveModifiersBySource(SourceType source)
        => modifiers.RemoveAll(m => m.source == source);

    public void UpdateModifierValue(SourceType source, StatType stat, float newValue)
    {
        for (int i = 0; i < modifiers.Count; i++)
        {
            StatModifier modifier = modifiers[i];
            if (modifier.source != source || modifier.stat != stat)
                continue;

            modifier.value = newValue;
            return;
        }
    }
    public  void RemoveModifiersBySourceAndId(SourceType source, int modifierId)
    {
        var list = modifiers;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            var m = list[i];
            if (m.source == source && m.modifierId == modifierId)
            {
                list.RemoveAt(i);
            }
        }

    }
    public float GetStat(StatType type)
    {
        float baseValue = type switch
        {
            StatType.Health => baseHealth,
            StatType.AttackDamage => baseAttackDamage,
            StatType.Armor => baseArmor,
            StatType.Range => baseRange,
            StatType.Mobility => baseMobility,
            _ => 0
        };

        float flatBonus = 0f;
        float percentBonus = 0f;
        float multiplicativePercent = 1f;
        bool splitShieldMinimum = false;

        for (int i = 0; i < modifiers.Count; i++)
        {
            StatModifier modifier = modifiers[i];
            if (modifier.stat != type)
                continue;

            if (modifier.isPercent)
            {
                if (modifier.isMultiplicativePercent)
                    multiplicativePercent *= 1f + modifier.value;
                else
                    percentBonus += modifier.value;
            }
            else
                flatBonus += modifier.value;

            if (type == StatType.Armor
                && modifier.source == SourceType.Relic
                && modifier.modifierId == 32)
            {
                splitShieldMinimum = true;
            }
        }

        float result = (baseValue + flatBonus) * (1 + percentBonus) * multiplicativePercent;

        if (type == StatType.Armor)
        {
            float minimumArmor = splitShieldMinimum ? 1f : MinArmor;
            return Math.Max(minimumArmor, result);
        }

        if (type == StatType.Mobility)
            return Math.Max(MinMobility, result);

        return result;
    }
    public IEnumerable<StatModifier> GetAllModifiers() => modifiers;

    public static bool HasModifier(StatBlock state, SourceType source, int modifierId)
    {
        if (state == null)
            return false;

        foreach (StatModifier modifier in state.modifiers)
        {
            if (modifier.source == source && modifier.modifierId == modifierId)
                return true;
        }

        return false;
    }
}
