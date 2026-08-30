using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class CommanderEffectsTests
{
    private static Type UnitType => FindType("RogueUnitDataBase");
    private static Type AbilityManagerType => FindType("AbilityManager");

    [Test]
    public void CommanderCatalog_MapsEverySpecificationId()
    {
        Type catalogType = FindType("CommanderCatalog");
        MethodInfo getId = catalogType.GetMethod(
            "GetId",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string), typeof(int?), typeof(string) },
            null);

        Assert.That(getId, Is.Not.Null);
        for (int numericId = 1; numericId <= 20; numericId++)
        {
            int actual = (int)getId.Invoke(null, new object[] { "elite", (int?)numericId, string.Empty });
            Assert.That(actual, Is.EqualTo(99 + numericId), $"elite Commander ID {numericId}");
        }

        string[] bossNames =
        {
            "아마록", "브루노", "시리온", "발레릭", "그론달", "에레보스", "라자루스",
            "아그마르", "토르단", "오르테온", "아스모데우스", "호쉬", "슈타인",
            "아지라스", "크롬홀드", "벨페고르", "멜세덱"
        };

        for (int i = 0; i < bossNames.Length; i++)
        {
            int actual = (int)getId.Invoke(null, new object[] { "boss", null, bossNames[i] });
            Assert.That(actual, Is.EqualTo(200 + i), bossNames[i]);
        }
    }

    [Test]
    public void StagePresetData_ConnectsEveryCommanderSpecificationId()
    {
        TextAsset json = Resources.Load<TextAsset>("PresetData/StagePresets");
        Assert.That(json, Is.Not.Null);

        Type presetType = FindType("StagePreset");
        Type listType = typeof(List<>).MakeGenericType(presetType);
        Type jsonConvertType = FindType("Newtonsoft.Json.JsonConvert");
        MethodInfo deserialize = jsonConvertType.GetMethod(
            "DeserializeObject",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string), typeof(Type) },
            null);
        Assert.That(deserialize, Is.Not.Null);
        IList presets = (IList)deserialize.Invoke(null, new object[] { json.text, listType });

        Type catalogType = FindType("CommanderCatalog");
        MethodInfo getId = catalogType.GetMethod(
            "GetId",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { presetType },
            null);
        var ids = new HashSet<int>();

        foreach (object preset in presets)
        {
            string commander = (string)presetType.GetField("Commander").GetValue(preset);
            if (string.IsNullOrWhiteSpace(commander))
                continue;

            int id = (int)getId.Invoke(null, new[] { preset });
            Assert.That(id, Is.Not.Zero, commander);
            ids.Add(id);
        }

        for (int id = 100; id <= 119; id++)
            Assert.That(ids.Contains(id), Is.True, $"missing commander preset {id}");
        for (int id = 200; id <= 216; id++)
            Assert.That(ids.Contains(id), Is.True, $"missing commander preset {id}");
    }

    [TestCase(1, "Combat", 1, false)]
    [TestCase(2, "Combat", 1, true)]
    [TestCase(3, "Combat", 50, true)]
    [TestCase(1, "Elite", 136, false)]
    [TestCase(1, "Elite", 190, true)]
    [TestCase(2, "Boss", 193, false)]
    [TestCase(1, "Combat", -1, true)]
    public void EnemyComposition_UsesRuntimeDataOnlyForDynamicStages(
        int chapter,
        string stageTypeName,
        int presetId,
        bool expected)
    {
        Type managerType = FindType("AutoBattleManager");
        Type stageType = FindType("StageType");
        MethodInfo method = managerType.GetMethod(
            "ShouldUseRuntimeEnemyUnits",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);

        bool actual = (bool)method.Invoke(
            null,
            new[] { (object)chapter, Enum.Parse(stageType, stageTypeName), presetId });
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void BuildSettings_AndUnityVersionMatchSupportedProjectConfiguration()
    {
        string[] expected =
        {
            "Title", "Main", "AutoBattleScene", "Difficulty", "Faction",
            "Unit_UI", "Test", "RLmap", "Event"
        };
        string[] enabledScenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => System.IO.Path.GetFileNameWithoutExtension(scene.path))
            .ToArray();

        CollectionAssert.AreEqual(expected, enabledScenes);
        Assert.That(Application.unityVersion, Is.EqualTo("2022.3.25f1"));
    }

    [Test]
    public void Belphegor_KoreanRuntimeTooltipUsesLegendaryRarityCondition()
    {
        Type gameTextType = FindType("GameTextDB");
        gameTextType.GetMethod("Load", BindingFlags.Public | BindingFlags.Static)
            .Invoke(null, new object[] { "kr", "en" });

        Type stageType = FindType("StageType");
        object boss = Enum.Parse(stageType, "Boss");
        Type skillType = FindType("CommanderSkillData");
        MethodInfo getText = skillType.GetMethod(
            "GetSkillText",
            BindingFlags.Public | BindingFlags.Static,
            null,
            new[] { typeof(string), stageType, typeof(int?) },
            null);
        string text = (string)getText.Invoke(null, new object[] { "벨페고르", boss, null });

        StringAssert.Contains("전설이 아닌", text);
        StringAssert.DoesNotContain("영웅이 아닌", text);
    }

    [Test]
    public void BattleStart_StatEffectsMatchCommanderSheet()
    {
        object myLightCavalry = CreateUnit(branch: 5, health: 100, armor: 10, attack: 100, mobility: 5);
        IList my = CreateUnitList(myLightCavalry);
        InvokeBattleStart(101, my, CreateUnitList());
        AssertUnit(myLightCavalry, mobility: 4);

        object myHeavyCavalry = CreateUnit(branch: 6, health: 100, armor: 10, attack: 100, mobility: 5);
        object enemyArcher = CreateUnit(branch: 2, health: 100, armor: 10, attack: 100, mobility: 3, range: 2, ranged: true);
        my = CreateUnitList(myHeavyCavalry);
        IList enemy = CreateUnitList(enemyArcher);
        InvokeBattleStart(103, my, enemy);
        AssertUnit(myHeavyCavalry, mobility: 3);
        AssertUnit(enemyArcher, attack: 115, range: 3);

        object enemyWarrior = CreateUnit(branch: 1, health: 100, armor: 10, attack: 100);
        InvokeBattleStart(107, CreateUnitList(), CreateUnitList(enemyWarrior));
        AssertUnit(enemyWarrior, health: 90, maxHealth: 90, attack: 120);

        object traitedPlayer = CreateUnit(branch: 0, health: 100, armor: 10, attack: 100, lightArmor: true, pierce: true);
        object bowieEnemy = CreateUnit(branch: 1, health: 100, armor: 10, attack: 100);
        InvokeBattleStart(111, CreateUnitList(traitedPlayer), CreateUnitList(bowieEnemy));
        AssertUnit(traitedPlayer, health: 90, maxHealth: 90);
        Assert.That(GetField<bool>(bowieEnemy, "suppression"), Is.True);

        object heavyInfantry = CreateUnit(branch: 3, health: 100, armor: 10, attack: 100);
        InvokeBattleStart(113, CreateUnitList(heavyInfantry), CreateUnitList());
        AssertUnit(heavyInfantry, health: 250, maxHealth: 250, attack: 90);

        object bonJoviPlayer = CreateUnit(branch: 0, health: 100, armor: 10, attack: 100);
        InvokeBattleStart(115, CreateUnitList(bonJoviPlayer), CreateUnitList());
        AssertUnit(bonJoviPlayer, attack: 90);

        object brunoWarrior = CreateUnit(branch: 1, health: 100, armor: 10, attack: 100);
        InvokeBattleStart(201, CreateUnitList(), CreateUnitList(brunoWarrior));
        AssertUnit(brunoWarrior, attack: 115);

        object agmarPlayer = CreateUnit(branch: 0, health: 100, armor: 10, attack: 100);
        object heavyArmorEnemy = CreateUnit(branch: 2, health: 100, armor: 20, attack: 100, heavyArmor: true);
        InvokeBattleStart(207, CreateUnitList(agmarPlayer), CreateUnitList(heavyArmorEnemy));
        AssertUnit(agmarPlayer, armor: 8);
        AssertUnit(heavyArmorEnemy, armor: 23, attack: 115);

        object lightPlayer = CreateUnit(branch: 4, health: 100, armor: 10, attack: 100, lightArmor: true);
        object tordanArcher = CreateUnit(branch: 2, health: 100, armor: 10, attack: 100, lightArmor: true, ranged: true);
        InvokeBattleStart(208, CreateUnitList(lightPlayer), CreateUnitList(tordanArcher));
        AssertUnit(lightPlayer, health: 150, maxHealth: 150);
        AssertUnit(tordanArcher, health: 150, maxHealth: 150, attack: 110);

        object hoshPlayer = CreateUnit(branch: 0, health: 100, armor: 10, attack: 100, mobility: 8);
        InvokeBattleStart(211, CreateUnitList(hoshPlayer), CreateUnitList());
        AssertUnit(hoshPlayer, mobility: 1);

        object elderKnight = CreateUnit(idx: 52, branch: 1, health: 100, armor: 10, attack: 100, heavyArmor: true);
        InvokeBattleStart(212, CreateUnitList(), CreateUnitList(elderKnight));
        AssertUnit(elderKnight, health: 200, maxHealth: 200, attack: 105);

        object azirasEnemy = CreateUnit(branch: 0, health: 100, armor: 10, attack: 100);
        InvokeBattleStart(213, CreateUnitList(), CreateUnitList(azirasEnemy));
        AssertUnit(azirasEnemy, attack: 150);

        object melsedecPlayer = CreateUnit(branch: 0, health: 100, armor: 10, attack: 100);
        InvokeBattleStart(216, CreateUnitList(melsedecPlayer), CreateUnitList());
        AssertUnit(melsedecPlayer, attack: 90);
    }

    [Test]
    public void Aziras_UsesCommanderSheetsMultiplicativePercentRule()
    {
        object enemy = CreateUnit(attack: 100);
        AddStatModifier(enemy, "AttackDamage", 0.20f, "Morale", 0, true);
        InvokePublic(enemy, "ApplyModifiers", true);
        AssertUnit(enemy, attack: 120);

        InvokeBattleStart(213, CreateUnitList(), CreateUnitList(enemy));

        AssertUnit(enemy, attack: 180);
    }

    [Test]
    public void BattleStart_SkillAndStateEffectsMatchCommanderSheet()
    {
        object ozzyHeavyInfantry = CreateUnit(branch: 3);
        InvokeBattleStart(106, CreateUnitList(), CreateUnitList(ozzyHeavyInfantry));
        Assert.That(GetField<bool>(ozzyHeavyInfantry, "martyrdom"), Is.True);

        object tylerPlayer = CreateUnit();
        InvokeBattleStart(116, CreateUnitList(tylerPlayer), CreateUnitList());
        Assert.That(GetEffectDictionary(tylerPlayer).Contains(0), Is.True);

        object erebosPlayer = CreateUnit();
        SetField(erebosPlayer, "charge", true);
        SetField(erebosPlayer, "smokeScreen", true);
        AddEffect(erebosPlayer, 12);
        AddEffect(erebosPlayer, 13);
        InvokeBattleStart(205, CreateUnitList(erebosPlayer), CreateUnitList());
        Assert.That(GetField<bool>(erebosPlayer, "charge"), Is.False);
        Assert.That(GetField<bool>(erebosPlayer, "smokeScreen"), Is.False);
        Assert.That(GetEffectDictionary(erebosPlayer).Contains(12), Is.False);
        Assert.That(GetEffectDictionary(erebosPlayer).Contains(13), Is.False);

        object lazarusEnemy = CreateUnit();
        InvokeBattleStart(206, CreateUnitList(), CreateUnitList(lazarusEnemy));
        Assert.That(GetField<bool>(lazarusEnemy, "drain"), Is.True);
        Assert.That(GetField<bool>(lazarusEnemy, "lifeDrain"), Is.True);

        object chromholdSpear = CreateUnit(branch: 0);
        object chromholdWarrior = CreateUnit(branch: 1);
        InvokeBattleStart(214, CreateUnitList(), CreateUnitList(chromholdSpear, chromholdWarrior));
        Assert.That(GetField<bool>(chromholdSpear, "throwSpear"), Is.True);
        Assert.That(GetField<bool>(chromholdWarrior, "assassination"), Is.True);
    }

    [Test]
    public void Belphegor_StealsOnlyNonLegendaryAndPlacesItFirstWithoutStatBoost()
    {
        object legendary = CreateUnit(idx: 70, rarity: 4, attack: 200);
        object normal = CreateUnit(idx: 7, rarity: 2, attack: 100);
        object existingEnemy = CreateUnit(idx: 8);
        IList my = CreateUnitList(legendary, normal);
        IList enemy = CreateUnitList(existingEnemy);

        InvokeBattleStart(215, my, enemy, (min, max) => 0);

        Assert.That(my.Count, Is.EqualTo(1));
        Assert.That(my[0], Is.SameAs(legendary));
        Assert.That(enemy.Count, Is.EqualTo(2));
        Assert.That(enemy[0], Is.SameAs(normal));
        AssertUnit(normal, attack: 100);
    }

    [Test]
    public void CommanderChance_RunsDeterministicTriggerAndNonTriggerCases()
    {
        object amarokTrigger = CreateAbilityManager(() => 0.329f, (min, max) => min, () => 200);
        object amarokMiss = CreateAbilityManager(() => 0.33f, (min, max) => min, () => 200);
        object kurtTrigger = CreateAbilityManager(() => 0.499f, (min, max) => min, () => 104);

        Assert.That(InvokeCommanderChance(amarokTrigger, 200, 0.33f, false, true), Is.True);
        Assert.That(InvokeCommanderChance(amarokMiss, 200, 0.33f, false, true), Is.False);
        Assert.That(InvokeCommanderChance(amarokTrigger, 200, 0.33f, true, true), Is.False);
        Assert.That(InvokeCommanderChance(kurtTrigger, 104, 0.50f, true, false), Is.True);
    }

    [Test]
    public void DodgeCommanders_ApplyCorrectSideAndExactValue()
    {
        object enemyLightCavalry = CreateUnit(branch: 5, mobility: 5);
        IList my = CreateUnitList(CreateUnit());
        IList enemy = CreateUnitList(enemyLightCavalry);

        object baseline = CreateAbilityManager(() => 1f, (min, max) => min, () => 0);
        InvokePublic(baseline, "ProcessCommenderEffect", my, enemy);
        float baselineDodge = (float)InvokePublic(baseline, "CalculateDodge", enemyLightCavalry, false, false);

        object hendrix = CreateAbilityManager(() => 1f, (min, max) => min, () => 101);
        InvokePublic(hendrix, "ProcessCommenderEffect", my, enemy);
        float hendrixDodge = (float)InvokePublic(hendrix, "CalculateDodge", enemyLightCavalry, false, false);
        Assert.That(hendrixDodge, Is.EqualTo(baselineDodge + 5f));

        object dylanEnemy = CreateUnit(mobility: 5);
        object dylan = CreateAbilityManager(() => 1f, (min, max) => min, () => 112);
        InvokePublic(dylan, "ProcessCommenderEffect", CreateUnitList(CreateUnit()), CreateUnitList(dylanEnemy));
        float dylanDodge = (float)InvokePublic(dylan, "CalculateDodge", dylanEnemy, false, false);

        object noCommander = CreateAbilityManager(() => 1f, (min, max) => min, () => 0);
        InvokePublic(noCommander, "ProcessCommenderEffect", CreateUnitList(CreateUnit()), CreateUnitList(dylanEnemy));
        float noCommanderDodge = (float)InvokePublic(noCommander, "CalculateDodge", dylanEnemy, false, false);
        Assert.That(dylanDodge, Is.EqualTo(noCommanderDodge + 10f));

        object hoshPlayer = CreateUnit(mobility: 9);
        object hosh = CreateAbilityManager(() => 1f, (min, max) => min, () => 211);
        InvokePublic(hosh, "ProcessCommenderEffect", CreateUnitList(hoshPlayer), CreateUnitList(CreateUnit()));
        Assert.That((float)InvokePublic(hosh, "CalculateDodge", hoshPlayer, true, false), Is.Zero);
    }

    [Test]
    public void EventCommanders_AttributeDamageToTheCorrectUnitAndSide()
    {
        object zanderPlayer = CreateUnit(armor: 10);
        object zanderEnemy = CreateUnit(armor: 10);
        object zander = CreateAbilityManager(() => 1f, (min, max) => min, () => 105);
        IList zanderPlayers = CreateUnitList(zanderPlayer);
        IList zanderEnemies = CreateUnitList(zanderEnemy);
        InvokePublic(zander, "ProcessCommenderEffect", zanderPlayers, zanderEnemies);
        InvokePrivate(zander, "ApplyCommanderDamageEffects", zanderPlayers, 0, 10f, true, null);
        AssertUnit(zanderPlayer, armor: 8);
        InvokePrivate(zander, "ApplyCommanderDamageEffects", zanderEnemies, 0, 10f, false, null);
        AssertUnit(zanderEnemy, armor: 10);

        object front = CreateUnit(attack: 100);
        object rear = CreateUnit(attack: 100);
        object cobain = CreateAbilityManager(() => 1f, (min, max) => min, () => 108);
        InvokePrivate(cobain, "ApplyCommanderCobain", 1, CreateUnitList(front, rear));
        AssertUnit(front, health: 100, attack: 100);
        AssertUnit(rear, health: 80, attack: 90);

        object exactThreshold = CreateUnit(health: 50);
        object aboveThreshold = CreateUnit(health: 51);
        object axl = CreateAbilityManager(() => 1f, (min, max) => min, () => 110);
        InvokePrivate(axl, "ApplyCommanderAxlExecution", CreateUnitList(exactThreshold), CreateUnitList(aboveThreshold));
        AssertUnit(exactThreshold, health: 0);
        AssertUnit(aboveThreshold, health: 51);

        object perryFront = CreateUnit(health: 100);
        object perryRear = CreateUnit(health: 100);
        object perryEnemy = CreateUnit(branch: 1);
        object perry = CreateAbilityManager(() => 0f, (min, max) => min, () => 114);
        InvokePublic(perry, "ProcessCommenderEffect", CreateUnitList(perryFront, perryRear), CreateUnitList(perryEnemy));
        InvokePrivate(perry, "TryApplyCommanderPerryBacklineDamage", CreateUnitList(perryFront, perryRear), 25f, false, -1, perryEnemy);
        AssertUnit(perryFront, health: 100);
        AssertUnit(perryRear, health: 75);
    }

    [Test]
    public void Lennon_OnlyCreditsAPlayerDeathDamagedByAnEnemyWarrior()
    {
        object playerA = CreateUnit();
        object playerB = CreateUnit();
        object enemyWarrior = CreateUnit(branch: 1);
        IList players = CreateUnitList(playerA, playerB);
        IList enemies = CreateUnitList(enemyWarrior);
        object manager = CreateAbilityManager(() => 1f, (min, max) => min, () => 102);
        InvokePublic(manager, "ProcessCommenderEffect", players, enemies);

        SetField(playerB, "health", 0f);
        InvokePrivate(manager, "ApplyCommanderDamageEffects", players, 1, 100f, true, enemyWarrior);

        Assert.That((int)InvokePrivate(manager, "CountLennonWarriorKills", CreateUnitList(playerB)), Is.EqualTo(1));
        Assert.That((int)InvokePrivate(manager, "CountLennonWarriorKills", CreateUnitList(playerA)), Is.Zero);
    }

    [Test]
    public void Lennon_DoesNotCreditAnEarlierNonlethalWarriorHit()
    {
        object player = CreateUnit(health: 100);
        object enemyWarrior = CreateUnit(branch: 1);
        object enemyAssassin = CreateUnit(branch: 4);
        IList players = CreateUnitList(player);
        object manager = CreateAbilityManager(() => 1f, (min, max) => min, () => 102);

        SetField(player, "health", 70f);
        InvokePrivate(manager, "ApplyCommanderDamageEffects", players, 0, 30f, true, enemyWarrior);
        SetField(player, "health", 0f);
        InvokePrivate(manager, "ApplyCommanderDamageEffects", players, 0, 70f, true, enemyAssassin);

        Assert.That((int)InvokePrivate(manager, "CountLennonWarriorKills", CreateUnitList(player)), Is.Zero);
    }

    [Test]
    public void Belphegor_TransferredUnitRemainsTrackedForBattleEndEnergy()
    {
        object survivingPlayer = CreateUnit(idx: 1, uniqueId: 101);
        object stolenPlayer = CreateUnit(idx: 2, uniqueId: 102);
        object ordinaryEnemy = CreateUnit(idx: 3, uniqueId: -103);
        Type managerType = FindType("AutoBattleManager");
        MethodInfo method = managerType.GetMethod(
            "BuildBattleEndTrackedMyUnits",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);

        IList result = (IList)method.Invoke(
            null,
            new object[]
            {
                CreateUnitList(survivingPlayer),
                CreateUnitList(),
                CreateUnitList(stolenPlayer, ordinaryEnemy),
                CreateUnitList(),
                CreateUnitList(survivingPlayer, stolenPlayer)
            });

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Contains(survivingPlayer), Is.True);
        Assert.That(result.Contains(stolenPlayer), Is.True);
        Assert.That(result.Contains(ordinaryEnemy), Is.False);
    }

    [Test]
    public void Bruno_DamagesEveryLivingPlayerExactlyOncePerTurn()
    {
        object playerA = CreateUnit(health: 100);
        object playerB = CreateUnit(health: 80);
        IList my = CreateUnitList(playerA, playerB);
        IList enemy = CreateUnitList(CreateUnit());
        object manager = CreateAbilityManager(() => 1f, (min, max) => min, () => 201);

        Assert.That((bool)InvokePublic(manager, "ProcessCommanderTurnEnd", 1, my, enemy), Is.True);
        AssertUnit(playerA, health: 95);
        AssertUnit(playerB, health: 75);
        Assert.That((bool)InvokePublic(manager, "ProcessCommanderTurnEnd", 1, my, enemy), Is.False);
        AssertUnit(playerA, health: 95);
        Assert.That((bool)InvokePublic(manager, "ProcessCommanderTurnEnd", 2, my, enemy), Is.True);
        AssertUnit(playerA, health: 90);
    }

    [Test]
    public void Ortheon_EnemyDeathDelaysExecutionByOneTurn()
    {
        object player = CreateUnit(health: 100);
        IList my = CreateUnitList(player);
        IList enemy = CreateUnitList(CreateUnit());
        object manager = CreateAbilityManager(() => 1f, (min, max) => 0, () => 209);

        InvokePrivate(
            manager,
            "ApplyCommanderDeathEffects",
            CreateUnitList(),
            CreateUnitList(CreateUnit()),
            my,
            enemy,
            false);

        Assert.That((bool)InvokePublic(manager, "ProcessCommanderTurnEnd", 4, my, enemy), Is.False);
        AssertUnit(player, health: 100);
        Assert.That((bool)InvokePublic(manager, "ProcessCommanderTurnEnd", 5, my, enemy), Is.True);
        AssertUnit(player, health: 0);
    }

    [Test]
    public void Asmodeus_DamagesOnceWhenArmorFirstReachesZero()
    {
        object player = CreateUnit(health: 100, armor: 2);
        IList my = CreateUnitList(player);
        object manager = CreateAbilityManager(() => 1f, (min, max) => min, () => 210);

        InvokePrivate(manager, "ApplyCommanderAsmodeus", 1, my);
        AssertUnit(player, health: 100, armor: 1);
        InvokePrivate(manager, "ApplyCommanderAsmodeus", 1, my);
        AssertUnit(player, health: 70, armor: 0);
        InvokePrivate(manager, "ApplyCommanderAsmodeus", 1, my);
        AssertUnit(player, health: 70, armor: 0);
    }

    [Test]
    public void Grondal_AttackStacksCapAtFiftyPercent()
    {
        object enemy = CreateUnit(attack: 100);
        IList enemies = CreateUnitList(enemy);
        object manager = CreateAbilityManager(() => 1f, (min, max) => min, () => 204);

        InvokePrivate(manager, "ApplyCommanderGrondal", 8, enemies);
        AssertUnit(enemy, attack: 150);
    }

    [Test]
    public void TimedCommanders_ExpireAndSwitchOnSpecifiedTurn()
    {
        object azirasEnemy = CreateUnit(attack: 100);
        IList azirasEnemies = CreateUnitList(azirasEnemy);
        InvokeBattleStart(213, CreateUnitList(), azirasEnemies);
        object aziras = CreateAbilityManager(() => 1f, (min, max) => min, () => 213);
        Assert.That((bool)InvokePublic(aziras, "ProcessCommanderTurnEnd", 5, CreateUnitList(), azirasEnemies), Is.True);
        AssertUnit(azirasEnemy, attack: 100);

        object player = CreateUnit(attack: 100);
        object enemy = CreateUnit(attack: 100);
        IList players = CreateUnitList(player);
        IList enemies = CreateUnitList(enemy);
        InvokeBattleStart(216, players, enemies);
        object melsedec = CreateAbilityManager(() => 1f, (min, max) => min, () => 216);
        Assert.That((bool)InvokePublic(melsedec, "ProcessCommanderTurnEnd", 10, players, enemies), Is.True);
        AssertUnit(player, attack: 100);
        AssertUnit(enemy, attack: 125);
    }

    [Test]
    public void CommanderModifiers_CanBeRemovedWithoutLeavingNextBattleStats()
    {
        object player = CreateUnit(attack: 100);
        IList players = CreateUnitList(player);
        InvokeBattleStart(216, players, CreateUnitList());
        AssertUnit(player, attack: 90);

        Type effectType = FindType("CommenderEffect");
        effectType.GetMethod("RemoveCommanderEffect", BindingFlags.Public | BindingFlags.Static)
            .Invoke(null, new object[] { players, 216 });
        AssertUnit(player, attack: 100);
    }

    [Test]
    public void ArmorFloor_AllowsAsmodeusZeroButHonorsSplitShieldMinimumOne()
    {
        object normal = CreateUnit(armor: 1);
        AddStatModifier(normal, "Armor", -5f, "Commander", 210, false);
        InvokePublic(normal, "ApplyModifiers", true);
        AssertUnit(normal, armor: 0);

        object splitShield = CreateUnit(armor: 1);
        AddStatModifier(splitShield, "Armor", -5f, "Relic", 32, false);
        InvokePublic(splitShield, "ApplyModifiers", true);
        AssertUnit(splitShield, armor: 1);
    }

    [Test]
    public void LootBag_AddsPlunderToExactlyTwoEligiblePlayerUnits()
    {
        object owner = CreateUnit();
        object candidateA = CreateUnit();
        object candidateB = CreateUnit();
        object candidateC = CreateUnit();
        SetField(owner, "plunder", true);
        IList units = CreateUnitList(owner, candidateA, candidateB, candidateC);

        MethodInfo method = AbilityManagerType.GetMethod(
            "ApplyLootBagPlunder",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);
        int applied = (int)method.Invoke(null, new object[] { units, (Func<int, int, int>)((min, max) => min) });

        Assert.That(applied, Is.EqualTo(2));
        Assert.That(GetField<bool>(candidateA, "plunder"), Is.True);
        Assert.That(GetField<bool>(candidateB, "plunder"), Is.True);
        Assert.That(GetField<bool>(candidateC, "plunder"), Is.False);
    }

    [TestCase(0, false)]
    [TestCase(1, true)]
    [TestCase(2, true)]
    public void EmptyRoster_IsNotCheckedBeforeVictoryRewards(int result, bool expected)
    {
        Type rewardType = FindType("RewardManager");
        MethodInfo method = rewardType.GetMethod(
            "ShouldCheckEmptyRoster",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);
        Assert.That((bool)method.Invoke(null, new object[] { result }), Is.EqualTo(expected));
    }

    private static void InvokeBattleStart(
        int commanderId,
        IList myUnits,
        IList enemyUnits,
        Func<int, int, int> randomRange = null)
    {
        Type effectType = FindType("CommenderEffect");
        MethodInfo method = effectType.GetMethod("ApplyBattleStart", BindingFlags.Public | BindingFlags.Static);
        Assert.That(method, Is.Not.Null);
        method.Invoke(null, new object[] { commanderId, myUnits, enemyUnits, randomRange ?? ((min, max) => min) });
    }

    private static object CreateAbilityManager(
        Func<float> randomValue,
        Func<int, int, int> randomRange,
        Func<int> commanderId)
    {
        return Activator.CreateInstance(AbilityManagerType, randomValue, randomRange, commanderId);
    }

    private static bool InvokeCommanderChance(
        object manager,
        int commanderId,
        float probability,
        bool sourceIsMyTeam,
        bool enemyOnly)
    {
        return (bool)InvokePrivate(
            manager,
            "ShouldTriggerCommanderChance",
            commanderId,
            probability,
            sourceIsMyTeam,
            enemyOnly);
    }

    private static object InvokePublic(object instance, string methodName, params object[] args)
    {
        MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
        Assert.That(method, Is.Not.Null, methodName);
        return method.Invoke(instance, args);
    }

    private static object InvokePrivate(object instance, string methodName, params object[] args)
    {
        MethodInfo method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null, methodName);
        return method.Invoke(instance, args);
    }

    private static IList CreateUnitList(params object[] units)
    {
        IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(UnitType));
        for (int i = 0; i < units.Length; i++)
            list.Add(units[i]);
        return list;
    }

    private static object CreateUnit(
        int idx = 1,
        int branch = 0,
        int rarity = 1,
        float health = 100,
        int armor = 10,
        float attack = 100,
        int mobility = 3,
        int range = 1,
        bool lightArmor = false,
        bool heavyArmor = false,
        bool ranged = false,
        bool pierce = false,
        int uniqueId = 0)
    {
#pragma warning disable SYSLIB0050
        object unit = FormatterServices.GetUninitializedObject(UnitType);
#pragma warning restore SYSLIB0050
        SetField(unit, "idx", idx);
        SetField(unit, "branchIdx", branch);
        SetField(unit, "rarity", rarity);
        SetField(unit, "baseHealth", health);
        SetField(unit, "baseArmor", armor);
        SetField(unit, "baseAttackDamage", attack);
        SetField(unit, "baseMobility", mobility);
        SetField(unit, "baseRange", range);
        SetField(unit, "health", health);
        SetField(unit, "attackDamage", attack);
        SetField(unit, "range", range);
        SetField(unit, "maxHealth", health);
        SetField(unit, "lightArmor", lightArmor);
        SetField(unit, "heavyArmor", heavyArmor);
        SetField(unit, "rangedAttack", ranged);
        SetField(unit, "pierce", pierce);
        SetField(unit, "alive", true);
        SetField(unit, "UniqueId", uniqueId);
        SetProperty(unit, "Armor", armor);
        SetProperty(unit, "Mobility", mobility);

        Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(int), FindType("BuffDebuffData"));
        SetField(unit, "effectDictionary", Activator.CreateInstance(dictionaryType));
        UnitType.GetMethod("NormalizeStatBlock", BindingFlags.Public | BindingFlags.Instance).Invoke(unit, null);
        return unit;
    }

    private static void AddEffect(object unit, int effectId)
    {
        Type effectType = FindType("BuffDebuffData");
        object effect = Activator.CreateInstance(effectType, effectId, 0, 1, 1);
        GetEffectDictionary(unit).Add(effectId, effect);
    }

    private static void AddStatModifier(
        object unit,
        string statName,
        float value,
        string sourceName,
        int modifierId,
        bool isPercent)
    {
        Type modifierType = FindType("StatModifier");
        object modifier = Activator.CreateInstance(modifierType);
        SetField(modifier, "stat", Enum.Parse(FindType("StatType"), statName));
        SetField(modifier, "value", value);
        SetField(modifier, "source", Enum.Parse(FindType("SourceType"), sourceName));
        SetField(modifier, "modifierId", modifierId);
        SetField(modifier, "isPercent", isPercent);

        object stats = UnitType.GetField("stats", BindingFlags.Public | BindingFlags.Instance).GetValue(unit);
        stats.GetType().GetMethod("AddModifier", BindingFlags.Public | BindingFlags.Instance)
            .Invoke(stats, new[] { modifier });
    }

    private static IDictionary GetEffectDictionary(object unit)
    {
        return (IDictionary)UnitType.GetField("effectDictionary", BindingFlags.Public | BindingFlags.Instance).GetValue(unit);
    }

    private static void AssertUnit(
        object unit,
        float? health = null,
        float? maxHealth = null,
        int? armor = null,
        float? attack = null,
        int? mobility = null,
        int? range = null)
    {
        if (health.HasValue)
            Assert.That(GetField<float>(unit, "health"), Is.EqualTo(health.Value).Within(0.001f));
        if (maxHealth.HasValue)
            Assert.That(GetField<float>(unit, "maxHealth"), Is.EqualTo(maxHealth.Value).Within(0.001f));
        if (armor.HasValue)
            Assert.That((int)GetProperty(unit, "Armor"), Is.EqualTo(armor.Value));
        if (attack.HasValue)
            Assert.That(GetField<float>(unit, "attackDamage"), Is.EqualTo(attack.Value).Within(0.001f));
        if (mobility.HasValue)
            Assert.That((int)GetProperty(unit, "Mobility"), Is.EqualTo(mobility.Value));
        if (range.HasValue)
            Assert.That(GetField<int>(unit, "range"), Is.EqualTo(range.Value));
    }

    private static T GetField<T>(object instance, string fieldName)
    {
        return (T)instance.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(instance);
    }

    private static void SetField(object instance, string fieldName, object value)
    {
        FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.That(field, Is.Not.Null, fieldName);
        field.SetValue(instance, value);
    }

    private static object GetProperty(object instance, string propertyName)
    {
        return instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance).GetValue(instance);
    }

    private static void SetProperty(object instance, string propertyName, object value)
    {
        PropertyInfo property = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        Assert.That(property, Is.Not.Null, propertyName);
        property.SetValue(instance, value);
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
