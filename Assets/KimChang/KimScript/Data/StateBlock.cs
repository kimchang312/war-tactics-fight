using System;
using System.Collections.Generic;
using System.Linq;

public enum StatType { Health, AttackDamage, Armor, Range, Mobility }
public enum SourceType { Relic, Trait, Skill, Buff, Morale, Synergy, Upgrade, Field,Passive }

[Serializable]
public class StatModifier
{
    public StatType stat;
    public float value;
    public SourceType source;
    public int modifierId;
    public bool isPercent;
}

[Serializable]
public class StatBlock
{
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
        var mod = modifiers.FirstOrDefault(m => m.source == source && m.stat == stat);
        if (mod != null) mod.value = newValue;
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

        float flatBonus = modifiers.Where(m => m.stat == type && !m.isPercent).Sum(m => m.value);
        float percentBonus = modifiers.Where(m => m.stat == type && m.isPercent).Sum(m => m.value);

        return (baseValue + flatBonus) * (1 + percentBonus);
    }
    public IEnumerable<StatModifier> GetAllModifiers() => modifiers;


}