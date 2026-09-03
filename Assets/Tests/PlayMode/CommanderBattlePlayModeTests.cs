using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NUnit.Framework;
using UnityEngine.TestTools;

public class CommanderBattlePlayModeTests
{
    [UnityTest]
    public IEnumerator Melsedec_RemainsActiveThroughTurnNineAndSwitchesAtTurnTen()
    {
        Type unitType = FindType("RogueUnitDataBase");
        object player = CreateUnit(unitType, 100f);
        object enemy = CreateUnit(unitType, 100f);
        IList players = CreateList(unitType, player);
        IList enemies = CreateList(unitType, enemy);

        Type effectType = FindType("CommenderEffect");
        effectType.GetMethod("ApplyBattleStart", BindingFlags.Public | BindingFlags.Static)
            .Invoke(null, new object[] { 216, players, enemies, (Func<int, int, int>)((min, max) => min) });
        Assert.That(GetField<float>(player, "attackDamage"), Is.EqualTo(90f));

        Type managerType = FindType("AbilityManager");
        object manager = Activator.CreateInstance(
            managerType,
            (Func<float>)(() => 1f),
            (Func<int, int, int>)((min, max) => min),
            (Func<int>)(() => 216));
        MethodInfo processTurnEnd = managerType.GetMethod(
            "ProcessCommanderTurnEnd",
            BindingFlags.Public | BindingFlags.Instance);

        for (int turn = 1; turn <= 9; turn++)
        {
            Assert.That((bool)processTurnEnd.Invoke(manager, new object[] { turn, players, enemies }), Is.False);
            Assert.That(GetField<float>(player, "attackDamage"), Is.EqualTo(90f));
            Assert.That(GetField<float>(enemy, "attackDamage"), Is.EqualTo(100f));
            yield return null;
        }

        Assert.That((bool)processTurnEnd.Invoke(manager, new object[] { 10, players, enemies }), Is.True);

        Assert.That(GetField<float>(player, "attackDamage"), Is.EqualTo(100f));
        Assert.That(GetField<float>(enemy, "attackDamage"), Is.EqualTo(125f));
    }

    private static object CreateUnit(Type unitType, float attack)
    {
#pragma warning disable SYSLIB0050
        object unit = FormatterServices.GetUninitializedObject(unitType);
#pragma warning restore SYSLIB0050
        SetField(unit, "idx", 1);
        SetField(unit, "branchIdx", 0);
        SetField(unit, "rarity", 1);
        SetField(unit, "baseHealth", 100f);
        SetField(unit, "baseArmor", 10);
        SetField(unit, "baseAttackDamage", attack);
        SetField(unit, "baseMobility", 3);
        SetField(unit, "baseRange", 1);
        SetField(unit, "health", 100f);
        SetField(unit, "attackDamage", attack);
        SetField(unit, "range", 1);
        SetField(unit, "maxHealth", 100f);
        SetField(unit, "alive", true);

        unitType.GetProperty("Armor").SetValue(unit, 10);
        unitType.GetProperty("Mobility").SetValue(unit, 3);
        Type effectType = FindType("BuffDebuffData");
        SetField(unit, "effectDictionary", Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(typeof(int), effectType)));
        unitType.GetMethod("NormalizeStatBlock", BindingFlags.Public | BindingFlags.Instance).Invoke(unit, null);
        return unit;
    }

    private static IList CreateList(Type unitType, params object[] units)
    {
        IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(unitType));
        foreach (object unit in units)
            list.Add(unit);
        return list;
    }

    private static T GetField<T>(object instance, string name)
    {
        return (T)instance.GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);
    }

    private static void SetField(object instance, string name, object value)
    {
        instance.GetType().GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, value);
    }

    private static Type FindType(string name)
    {
        Type type = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(name, false))
            .FirstOrDefault(candidate => candidate != null);
        Assert.That(type, Is.Not.Null, name);
        return type;
    }
}
