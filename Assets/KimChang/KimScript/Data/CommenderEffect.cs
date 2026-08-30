using System;
using System.Collections.Generic;
using UnityEngine;

public static class CommenderEffect
{
    public static void ApplyBattleStart(
        int commanderId,
        List<RogueUnitDataBase> myUnits,
        List<RogueUnitDataBase> enemyUnits,
        Func<int, int, int> randomRange)
    {
        if (!CommanderCatalog.IsKnownId(commanderId))
            return;

        myUnits ??= new List<RogueUnitDataBase>();
        enemyUnits ??= new List<RogueUnitDataBase>();

        RemoveCommanderModifiers(myUnits, commanderId);
        RemoveCommanderModifiers(enemyUnits, commanderId);

        switch (commanderId)
        {
            case 101: ApplyHendrix(myUnits, commanderId); break;
            case 103: ApplyMorrison(myUnits, enemyUnits, commanderId); break;
            case 106: ApplyOzzy(enemyUnits); break;
            case 107: ApplySlash(enemyUnits, commanderId); break;
            case 111: ApplyBowie(myUnits, enemyUnits, commanderId); break;
            case 113: ApplyUlrich(myUnits, enemyUnits, commanderId); break;
            case 115: ApplyBonJovi(myUnits, commanderId); break;
            case 116: ApplyTyler(myUnits); break;
            case 201: ApplyBruno(enemyUnits, commanderId); break;
            case 205: ApplyErebos(myUnits, enemyUnits); break;
            case 206: ApplyLazarus(enemyUnits); break;
            case 207: ApplyAgmar(myUnits, enemyUnits, commanderId); break;
            case 208: ApplyTordan(myUnits, enemyUnits, commanderId); break;
            case 211: ApplyHosh(myUnits, commanderId); break;
            case 212: ApplyStein(enemyUnits, commanderId); break;
            case 213: ApplyAziras(enemyUnits, commanderId); break;
            case 214: ApplyChromhold(enemyUnits); break;
            case 215: ApplyBelphegor(myUnits, enemyUnits, randomRange); break;
            case 216: ApplyMelsedec(myUnits, commanderId); break;
        }

        ApplyModifiers(myUnits);
        ApplyModifiers(enemyUnits);
    }

    private static void ApplyHendrix(List<RogueUnitDataBase> myUnits, int commanderId)
    {
        for (int i = 0; i < myUnits.Count; i++)
        {
            RogueUnitDataBase unit = myUnits[i];
            if (unit != null && unit.branchIdx == 5)
                AddModifier(unit, StatType.Mobility, -1f, commanderId, false);
        }
    }

    private static void ApplyMorrison(
        List<RogueUnitDataBase> myUnits,
        List<RogueUnitDataBase> enemyUnits,
        int commanderId)
    {
        ApplyCavalryMobilityPenalty(myUnits, commanderId, -2f);
        ApplyCavalryMobilityPenalty(enemyUnits, commanderId, -2f);

        for (int i = 0; i < enemyUnits.Count; i++)
        {
            RogueUnitDataBase unit = enemyUnits[i];
            if (unit == null || unit.branchIdx != 2)
                continue;

            AddModifier(unit, StatType.Range, 1f, commanderId, false);
            AddModifier(unit, StatType.AttackDamage, 0.15f, commanderId, true);
        }
    }

    private static void ApplyCavalryMobilityPenalty(List<RogueUnitDataBase> units, int commanderId, float value)
    {
        for (int i = 0; i < units.Count; i++)
        {
            RogueUnitDataBase unit = units[i];
            if (unit != null && (unit.branchIdx == 5 || unit.branchIdx == 6))
                AddModifier(unit, StatType.Mobility, value, commanderId, false);
        }
    }

    private static void ApplyOzzy(List<RogueUnitDataBase> enemyUnits)
    {
        for (int i = 0; i < enemyUnits.Count; i++)
        {
            RogueUnitDataBase unit = enemyUnits[i];
            if (unit != null && unit.branchIdx == 3)
                unit.martyrdom = true;
        }
    }

    private static void ApplySlash(List<RogueUnitDataBase> enemyUnits, int commanderId)
    {
        for (int i = 0; i < enemyUnits.Count; i++)
        {
            RogueUnitDataBase unit = enemyUnits[i];
            if (unit == null || (unit.branchIdx != 1 && unit.branchIdx != 4))
                continue;

            AddModifier(unit, StatType.AttackDamage, 0.20f, commanderId, true);
            AddModifier(unit, StatType.Health, -0.10f, commanderId, true);
        }
    }

    private static void ApplyBowie(
        List<RogueUnitDataBase> myUnits,
        List<RogueUnitDataBase> enemyUnits,
        int commanderId)
    {
        for (int i = 0; i < myUnits.Count; i++)
        {
            RogueUnitDataBase unit = myUnits[i];
            if (unit != null)
                AddModifier(unit, StatType.Health, -5f * CountUnitTrait(unit), commanderId, false);
        }

        for (int i = 0; i < enemyUnits.Count; i++)
        {
            if (enemyUnits[i] != null)
                enemyUnits[i].suppression = true;
        }
    }

    private static void ApplyUlrich(
        List<RogueUnitDataBase> myUnits,
        List<RogueUnitDataBase> enemyUnits,
        int commanderId)
    {
        ApplyHeavyInfantryBonus(myUnits, commanderId);
        ApplyHeavyInfantryBonus(enemyUnits, commanderId);
    }

    private static void ApplyHeavyInfantryBonus(List<RogueUnitDataBase> units, int commanderId)
    {
        for (int i = 0; i < units.Count; i++)
        {
            RogueUnitDataBase unit = units[i];
            if (unit == null || unit.branchIdx != 3)
                continue;

            AddModifier(unit, StatType.Health, 150f, commanderId, false);
            AddModifier(unit, StatType.AttackDamage, -0.10f, commanderId, true);
        }
    }

    private static void ApplyBonJovi(List<RogueUnitDataBase> myUnits, int commanderId)
    {
        for (int i = 0; i < myUnits.Count; i++)
        {
            if (myUnits[i] != null)
                AddModifier(myUnits[i], StatType.AttackDamage, -10f, commanderId, false);
        }
    }

    private static void ApplyTyler(List<RogueUnitDataBase> myUnits)
    {
        const int burningId = 0;
        for (int i = 0; i < myUnits.Count; i++)
        {
            RogueUnitDataBase unit = myUnits[i];
            if (unit != null && !unit.effectDictionary.ContainsKey(burningId))
                unit.effectDictionary[burningId] = new BuffDebuffData(burningId, 1, 1, 2);
        }
    }

    private static void ApplyBruno(List<RogueUnitDataBase> enemyUnits, int commanderId)
    {
        for (int i = 0; i < enemyUnits.Count; i++)
        {
            RogueUnitDataBase unit = enemyUnits[i];
            if (unit != null && unit.branchIdx == 1)
                AddModifier(unit, StatType.AttackDamage, 0.15f, commanderId, true);
        }
    }

    private static void ApplyErebos(List<RogueUnitDataBase> myUnits, List<RogueUnitDataBase> enemyUnits)
    {
        ClearSkills(myUnits);
        ClearSkills(enemyUnits);
    }

    private static void ClearSkills(List<RogueUnitDataBase> units)
    {
        for (int i = 0; i < units.Count; i++)
        {
            RogueUnitDataBase unit = units[i];
            if (unit == null)
                continue;

            ClearUnitSkill(unit);
            unit.effectDictionary.Remove(12);
            unit.effectDictionary.Remove(13);
        }
    }

    private static void ApplyLazarus(List<RogueUnitDataBase> enemyUnits)
    {
        for (int i = 0; i < enemyUnits.Count; i++)
        {
            RogueUnitDataBase unit = enemyUnits[i];
            if (unit == null)
                continue;

            unit.drain = true;
            unit.lifeDrain = true;
        }
    }

    private static void ApplyAgmar(
        List<RogueUnitDataBase> myUnits,
        List<RogueUnitDataBase> enemyUnits,
        int commanderId)
    {
        for (int i = 0; i < myUnits.Count; i++)
        {
            if (myUnits[i] != null)
                AddModifier(myUnits[i], StatType.Armor, -2f, commanderId, false);
        }

        for (int i = 0; i < enemyUnits.Count; i++)
        {
            RogueUnitDataBase unit = enemyUnits[i];
            if (unit == null || !unit.heavyArmor)
                continue;

            AddModifier(unit, StatType.AttackDamage, 0.15f, commanderId, true);
            AddModifier(unit, StatType.Armor, 0.15f, commanderId, true);
        }
    }

    private static void ApplyTordan(
        List<RogueUnitDataBase> myUnits,
        List<RogueUnitDataBase> enemyUnits,
        int commanderId)
    {
        ApplyLightArmorHealth(myUnits, commanderId);
        ApplyLightArmorHealth(enemyUnits, commanderId);

        for (int i = 0; i < enemyUnits.Count; i++)
        {
            RogueUnitDataBase unit = enemyUnits[i];
            if (unit == null)
                continue;

            if (unit.branchIdx == 0)
                unit.vengeance = true;
            if (unit.branchIdx == 2)
                AddModifier(unit, StatType.AttackDamage, 10f, commanderId, false);
        }
    }

    private static void ApplyLightArmorHealth(List<RogueUnitDataBase> units, int commanderId)
    {
        for (int i = 0; i < units.Count; i++)
        {
            RogueUnitDataBase unit = units[i];
            if (unit != null && unit.lightArmor)
                AddModifier(unit, StatType.Health, 50f, commanderId, false);
        }
    }

    private static void ApplyHosh(List<RogueUnitDataBase> myUnits, int commanderId)
    {
        for (int i = 0; i < myUnits.Count; i++)
        {
            RogueUnitDataBase unit = myUnits[i];
            if (unit == null)
                continue;

            float currentMobility = unit.stats?.GetStat(StatType.Mobility) ?? unit.Mobility;
            AddModifier(unit, StatType.Mobility, 1f - currentMobility, commanderId, false);
        }
    }

    private static void ApplyStein(List<RogueUnitDataBase> enemyUnits, int commanderId)
    {
        for (int i = 0; i < enemyUnits.Count; i++)
        {
            RogueUnitDataBase unit = enemyUnits[i];
            if (unit == null)
                continue;

            if (unit.idx == 52)
                AddModifier(unit, StatType.Health, 1f, commanderId, true);

            AddModifier(unit, StatType.AttackDamage, 0.05f * CountUnitTrait(unit), commanderId, true);
        }
    }

    private static void ApplyAziras(List<RogueUnitDataBase> enemyUnits, int commanderId)
    {
        for (int i = 0; i < enemyUnits.Count; i++)
        {
            if (enemyUnits[i] != null)
                AddModifier(enemyUnits[i], StatType.AttackDamage, 0.50f, commanderId, true, true);
        }
    }

    private static void ApplyChromhold(List<RogueUnitDataBase> enemyUnits)
    {
        for (int i = 0; i < enemyUnits.Count; i++)
        {
            RogueUnitDataBase unit = enemyUnits[i];
            if (unit == null)
                continue;

            if (unit.branchIdx == 0)
                unit.throwSpear = true;
            if (unit.branchIdx == 1)
                unit.assassination = true;
        }
    }

    private static void ApplyBelphegor(
        List<RogueUnitDataBase> myUnits,
        List<RogueUnitDataBase> enemyUnits,
        Func<int, int, int> randomRange)
    {
        int candidateCount = 0;
        for (int i = 0; i < myUnits.Count; i++)
        {
            if (myUnits[i] != null && myUnits[i].rarity != 4)
                candidateCount++;
        }

        if (candidateCount == 0)
            return;

        int selectedCandidate = randomRange != null
            ? Mathf.Clamp(randomRange(0, candidateCount), 0, candidateCount - 1)
            : 0;

        int selectedIndex = -1;
        for (int i = 0; i < myUnits.Count; i++)
        {
            if (myUnits[i] == null || myUnits[i].rarity == 4)
                continue;

            if (selectedCandidate-- == 0)
            {
                selectedIndex = i;
                break;
            }
        }

        if (selectedIndex < 0)
            return;

        RogueUnitDataBase selectedUnit = myUnits[selectedIndex];
        myUnits.RemoveAt(selectedIndex);
        enemyUnits.Insert(0, selectedUnit);
    }

    private static void ApplyMelsedec(List<RogueUnitDataBase> myUnits, int commanderId)
    {
        for (int i = 0; i < myUnits.Count; i++)
        {
            if (myUnits[i] != null)
                AddModifier(myUnits[i], StatType.AttackDamage, -0.10f, commanderId, true);
        }
    }

    public static void RefreshSteinTraitAttack(RogueUnitDataBase unit)
    {
        if (unit == null || unit.stats == null)
            return;

        unit.stats.RemoveModifiersBySourceAndId(SourceType.Commander, 212);
        if (unit.idx == 52)
            AddModifier(unit, StatType.Health, 1f, 212, true);
        AddModifier(unit, StatType.AttackDamage, 0.05f * CountUnitTrait(unit), 212, true);
        unit.ApplyModifiers(true);
    }

    public static void RemoveCommanderEffect(List<RogueUnitDataBase> units, int commanderId)
    {
        if (units == null)
            return;

        for (int i = 0; i < units.Count; i++)
        {
            RogueUnitDataBase unit = units[i];
            if (unit == null || unit.stats == null)
                continue;

            unit.stats.RemoveModifiersBySourceAndId(SourceType.Commander, commanderId);
            unit.ApplyModifiers(true);
        }
    }

    public static void AddCommanderPercentAttack(
        List<RogueUnitDataBase> units,
        int commanderId,
        float percent)
    {
        if (units == null)
            return;

        for (int i = 0; i < units.Count; i++)
        {
            RogueUnitDataBase unit = units[i];
            if (unit == null)
                continue;

            AddModifier(unit, StatType.AttackDamage, percent, commanderId, true);
            unit.ApplyModifiers(true);
        }
    }

    public static int CountUnitTrait(RogueUnitDataBase unit)
    {
        if (unit == null)
            return 0;

        int count = 0;
        if (unit.lightArmor) count++;
        if (unit.heavyArmor) count++;
        if (unit.rangedAttack) count++;
        if (unit.bluntWeapon) count++;
        if (unit.pierce) count++;
        if (unit.agility) count++;
        if (unit.strongCharge) count++;
        if (unit.perfectAccuracy) count++;
        if (unit.slaughter) count++;
        if (unit.bindingForce) count++;
        if (unit.bravery) count++;
        if (unit.suppression) count++;
        if (unit.plunder) count++;
        if (unit.doubleShot) count++;
        if (unit.scorching) count++;
        if (unit.thorns) count++;
        if (unit.endless) count++;
        if (unit.impact) count++;
        if (unit.healing) count++;
        if (unit.lifeDrain) count++;
        return count;
    }

    public static void ClearUnitSkill(RogueUnitDataBase unit)
    {
        if (unit == null)
            return;

        unit.charge = false;
        unit.defense = false;
        unit.throwSpear = false;
        unit.guerrilla = false;
        unit.guard = false;
        unit.assassination = false;
        unit.drain = false;
        unit.overwhelm = false;
        unit.martyrdom = false;
        unit.wounding = false;
        unit.vengeance = false;
        unit.counter = false;
        unit.firstStrike = false;
        unit.challenge = false;
        unit.smokeScreen = false;
    }

    private static void AddModifier(
        RogueUnitDataBase unit,
        StatType stat,
        float value,
        int commanderId,
        bool isPercent,
        bool isMultiplicativePercent = false)
    {
        if (unit == null)
            return;

        if (unit.stats == null)
            unit.NormalizeStatBlock();

        unit.stats.AddModifier(new StatModifier
        {
            stat = stat,
            value = value,
            source = SourceType.Commander,
            modifierId = commanderId,
            isPercent = isPercent,
            isMultiplicativePercent = isMultiplicativePercent
        });
    }

    private static void RemoveCommanderModifiers(List<RogueUnitDataBase> units, int commanderId)
    {
        for (int i = 0; i < units.Count; i++)
        {
            RogueUnitDataBase unit = units[i];
            if (unit?.stats != null)
                unit.stats.RemoveModifiersBySourceAndId(SourceType.Commander, commanderId);
        }
    }

    private static void ApplyModifiers(List<RogueUnitDataBase> units)
    {
        for (int i = 0; i < units.Count; i++)
        {
            RogueUnitDataBase unit = units[i];
            if (unit != null && unit.stats != null && unit.health > 0f)
                unit.ApplyModifiers(true);
        }
    }
}
