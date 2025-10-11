using DG.Tweening.Plugins.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using static RogueLikeData;


public static class WarRelicDatabase
{
    public static List<WarRelic> relics = new List<WarRelic>();

    private static System.Random random = RogueLikeData.Instance.GetRandomBySeed();
    // 실행 함수 캐시: id -> Action<WarRelic>
    private static Action<WarRelic>[] s_execById;
    // 값 캐시: id -> JSON value 배열
    private static Dictionary<int, string[]> s_valuesById;

    static WarRelicDatabase()
    {
        RegisterExec(0, DiscountCoupon); // Discount Coupon
        RegisterExec(1, ResearchBudgetGrant); // Research Budget Grant
        RegisterExec(2, IonDrink); // Ion Drink
        RegisterExec(3, TrainingBaton); // Training Baton
        RegisterExec(4, GildedTelescope); // Gilded Telescope
        RegisterExec(5, LuckyPouch); // Lucky Pouch
        RegisterExec(6, GraveRobbersShovel); // Grave Robber's Shovel
        RegisterExec(7, StrangeMagnifyingGlass); // Strange Magnifying Glass
        RegisterExec(8, PureGoldSword); // Pure Gold Sword
        RegisterExec(9, PulsatingDoll); // Pulsating Doll
        RegisterExec(10, BerserkersArmor); // Berserker's Armor
        RegisterExec(11, MushroomOfCourage); // Mushroom of Courage
        RegisterExec(12, PantheonModel); // Pantheon Model
        RegisterExec(13, Chessboard); // Chessboard
        RegisterExec(14, SpikedArmor); // Spiked Armor
        RegisterExec(15, TwinShields); // Twin Shields
        RegisterExec(16, BootsOfHermes); // Boots of Hermes
        RegisterExec(17, Halberd); // Halberd
        RegisterExec(18, EliteCavalrySaddle); // Elite Cavalry Saddle
        RegisterExec(19, EliteArchersFeatheredCap); // Elite Archer's Feathered Cap
        RegisterExec(20, MilitiaHorn); // Militia Horn
        RegisterExec(21, EerieOrb); // Eerie Orb
        RegisterExec(22, HaejuTalisman); // Haeju Talisman
        RegisterExec(23, EmptyGemGauntlet); // Empty Gem Gauntlet
        RegisterExec(24, SmallPileOfGems); // Small Pile of Gems
        RegisterExec(25, LargeGem); // Large Gem
        RegisterExec(26, CompletedGemGauntlet); // Completed Gem Gauntlet
        RegisterExec(27, HeartGemNecklace); // Heart Gem Necklace
        RegisterExec(28, FlagOfCourage); // Flag of Courage
        RegisterExec(29, BrokenStraightSword); // Broken Straight Sword
        RegisterExec(30, CrackedHelmet); // Cracked Helmet
        RegisterExec(31, WornOutBoots); // Worn-out Boots
        RegisterExec(32, SplitShield); // Split Shield
        RegisterExec(33, DesolateFlag); // Desolate Flag
        RegisterExec(34, SurvivorsRags); // Survivor's Rags
        RegisterExec(35, ConquerorsSeal); // Conqueror's Seal
        RegisterExec(36, BlindWarriorsEyepatch); // Blind Warrior's Eyepatch
        RegisterExec(37, ReinforcedArmorPlate); // Reinforced Armor Plate
        RegisterExec(38, OrnamentedDagger); // Ornamented Dagger
        RegisterExec(39, WarHorn); // War Horn
        RegisterExec(40, FreeMealTicket); // Free Meal Ticket
        RegisterExec(41, SabotageCannon); // Sabotage Cannon
        RegisterExec(42, AutonomousDevelopmentOrder); // Autonomous Development Order
        RegisterExec(43, EnemyGeneralScoutReport); // Enemy General Scout Report
        RegisterExec(44, MistakenOrderReceipt); // Mistaken Order Receipt
        RegisterExec(45, SymbolOfUnity); // Symbol of Unity
        RegisterExec(46, TechnicalSecretTome); // Technical Secret Tome
        RegisterExec(47, TreasureMap); // Treasure Map
        RegisterExec(48, RainbowKey); // Rainbow Key
        RegisterExec(49, CreditAuthorization); // Credit Authorization
        RegisterExec(50, GoldenHorn); // Golden Horn
        RegisterExec(51, ThickTacticsManual); // Thick Tactics Manual
        RegisterExec(52, ExplorersCompass); // Explorer's Compass
        RegisterExec(53, UnluckyGoldCoin); // Unlucky Gold Coin
        RegisterExec(54, CursedDoll); // Cursed Doll
        RegisterExec(55, DiceOfChaos); // Dice of Chaos
        RegisterExec(56, BerserkersHair); // Berserker’s Hair
        RegisterExec(57, MedalOfBravery); // Medal of Bravery
        RegisterExec(58, EvidenceOfEmbezzlement); // Evidence of Embezzlement
        RegisterExec(59, WarReport); // War Report
        RegisterExec(60, BadgeOfNamelessLegion); // Badge of Nameless Legion
        RegisterExec(61, HotHeartModel); // Hot Heart Model
        RegisterExec(62, BrandOfTheUnderdog); // Brand of the Underdog
        RegisterExec(63, VeryThickSoup); // Very Thick Soup
        RegisterExec(64, WarlordsHelm); // Warlord’s Helm
        RegisterExec(65, WarlordsSword); // Warlord’s Sword
        RegisterExec(66, ExpandedFormationDiagram); // Expanded Formation Diagram
        RegisterExec(67, WarlordsInsignia); // Warlord’s Insignia
        RegisterExec(68, LootBag); // Loot Bag
        RegisterExec(69, VanguardArmor); // Vanguard Armor
        RegisterExec(70, VanguardBoots); // Vanguard Boots
        RegisterExec(71, RustyIronStake); // Rusty Iron Stake
        RegisterExec(72, SuspiciousScalenePolyhedron); // Suspicious Scalene Polyhedron
        RegisterExec(73, AnythingBox); // Anything Box
        RegisterExec(74, SacredDocument); // Sacred Document
        RegisterExec(75, Signpost); // Signpost
        RegisterExec(76, LightweightArmor); // Lightweight Armor
        RegisterExec(77, Horn); // Horn
        RegisterExec(78, SariHeritage); // Sari Heritage
        RegisterExec(79, StrangePiece); // Strange Piece
        RegisterExec(80, AlloySpur); // Alloy Spur
        RegisterExec(81, ForgedLance); // Forged Lance
        RegisterExec(82, ReactiveArmor); // Reactive Armor
        RegisterExec(83, SpearManual); // Spear Manual
        RegisterExec(84, HealingStone); // Healing Stone
        RegisterExec(85, EliteRecruitmentOrder); // Elite Recruitment Order
        RegisterExec(86, LargeCart); // Large Cart
        RegisterExec(87, FiveOfAKind); // Five of a Kind
        RegisterExec(88, RoyalStraightFlush); // Royal Straight Flush
        RegisterExec(89, TornList); // Torn List
        RegisterExec(90, BrokenMirror); // Broken Mirror
        RegisterExec(91, OminousShackles); // Ominous Shackles
        RegisterExec(92, DeliciousRations); // Delicious Rations
        RegisterExec(93, DeliciousSpecialMeal); // Delicious Special Meal
        RegisterExec(94, CastIronHelmet); // Cast Iron Helmet
        RegisterExec(95, ShoesOfFullSpeedAhead); // Shoes of Full Speed Ahead
        RegisterExec(96, DecoratedRosary); // Decorated Rosary
        RegisterExec(97, EmergencyEscapeManual); // Emergency Escape Manual
                                                 // 98 번은 시트에 없음
        RegisterExec(99, JarOfDesire); // Jar of Desire
        RegisterExec(100, BeginningOfTheRainbow); // Beginning of the Rainbow
        RegisterExec(101, EndOfTheHorizon); // End of the Horizon
        RegisterExec(102, GamblersFate); // Gambler’s Fate
        RegisterExec(103, ObsidianHeart); // Obsidian Heart
        RegisterExec(104, DoubleEdgedAxeOfPride); // Double-Edged Axe of Pride
        RegisterExec(105, CursedArmor); // Cursed Armor
        RegisterExec(106, ControlTorch); // Control Torch
        RegisterExec(107, PileOfMedals); // Pile of Medals
        RegisterExec(108, AmbiguousApocalypse); // Ambiguous Apocalypse
        RegisterExec(109, TrainingSandbagsOfWar); // Training Sandbags of War
        RegisterExec(110, Epic); // Epic
        RegisterExec(111, OrbOfContempt); // Orb of Contempt
        RegisterExec(112, ToughWhip); // Tough Whip
        RegisterExec(113, BloodSoakedDye); // Blood-soaked Dye
        RegisterExec(114, TerracottaArmy); // Terracotta Army
        RegisterExec(115, ThrowingJavelin); // Throwing Javelin
        RegisterExec(116, PriceOfGlory); // Price of Glory
        RegisterExec(117, ReallyLongSpear); // Really Long Spear
        RegisterExec(118, DuelistsGreatsword); // Duelist’s Greatsword
        RegisterExec(119, ImperialThornWall); // Imperial Thorn Wall
        RegisterExec(120, TrophyPouch); // Trophy Pouch
        RegisterExec(121, CitadelBreaker); // Citadel Breaker
        RegisterExec(122, BloodstainedOath); // Bloodstained Oath
        RegisterExec(123, EndlessBarrage); // Endless Barrage
        RegisterExec(124, PartingShot); // Parting Shot
        RegisterExec(125, ObsidianArrowhead); // Obsidian Arrowhead
        RegisterExec(126, HeavyMace); // Heavy Mace
        RegisterExec(127, GuardiansCloak); // Guardian’s Cloak
        RegisterExec(128, LegacyOfTrust); // Legacy of Trust
        RegisterExec(129, ContractInvoice); // Contract Invoice
        RegisterExec(130, SpectersCowl); // Specter’s Cowl
        RegisterExec(131, SpursOfDoom); // Spurs of Doom
        RegisterExec(132, TyphoonCallingEye); // Typhoon-Calling Eye
        RegisterExec(133, SwordOfRighteousLight); // Sword of Righteous Light
        RegisterExec(134, RecklessKnightsHelm); // Reckless Knight’s Helm
        RegisterExec(135, SmallBatteringRam); // Small Battering Ram
        RegisterExec(136, FakeOrb); // Fake Orb

        BindExecOnAllRelics();
    }

    #region 유산 로드 관련

    // 사용처: id로 유산 객체 얻기(반환 직전 1회 바인딩 보정)
    public static WarRelic GetRelicById(int id)
    {
        var r = relics.Find(relic => relic.id == id);
        TryBindOnCreate(r);
        return r;
    }



    // 배열 용량 보정(최소 복사)
    private static void EnsureExecCapacity(int maxId)
    {
        if (maxId < 0) return;

        if (s_execById == null)
        {
            int cap = Math.Max(64, maxId + 1);
            s_execById = new Action<WarRelic>[cap];
            return;
        }
        if (maxId < s_execById.Length) return;

        int newLen = s_execById.Length;
        do newLen <<= 1; while (newLen <= maxId);

        var newArr = new Action<WarRelic>[newLen];
        Array.Copy(s_execById, newArr, s_execById.Length);
        s_execById = newArr;
    }

    // id로 실행 함수 등록
    public static void RegisterExec(int id, Action<WarRelic> fn)
    {
        if (id < 0 || fn == null) return;
        EnsureExecCapacity(id);
        s_execById[id] = fn;
    }

    // 사용처: 무인자 핸들러 호환 등록 (기존 함수 계속 사용 가능)
    public static void RegisterExec(int id, Action fnNoArg)
    {
        if (fnNoArg == null) return;
        RegisterExec(id, _ => fnNoArg());
    }
    // “0..N” 순번과 id가 일치할 때 대량 등록
    public static void RegisterExecBulk(params Action<WarRelic>[] actionsByIdOrder)
    {
        if (actionsByIdOrder == null || actionsByIdOrder.Length == 0) return;
        EnsureExecCapacity(actionsByIdOrder.Length - 1);
        for (int id = 0; id < actionsByIdOrder.Length; id++)
        {
            var act = actionsByIdOrder[id];
            if (act != null) s_execById[id] = act;
        }
    }

    // 런타임 객체 생성 직후 실행/값 바인딩 보정
    private static void TryBindOnCreate(WarRelic relic)
    {
        if (relic == null || relic.id < 0) return;

        var arr = s_execById;
        if (arr != null && relic.id < arr.Length)
        {
            var act = arr[relic.id];
            if (act != null) relic.BindExecute(act); // WarRelic.BindExecute(Action<WarRelic>)
        }

        if (s_valuesById != null && s_valuesById.TryGetValue(relic.id, out var vals))
        {
            relic.BindConfig(vals); // WarRelic.BindConfig(string[])
        }
    }

    // DB에 만들어 둔 모든 유산에 실행 함수 일괄 바인딩
    public static void BindExecOnAllRelics()
    {
        var list = relics;
        if (list == null || list.Count == 0) return;

        int maxId = -1;
        for (int i = 0; i < list.Count; i++)
        {
            var r = list[i];
            if (r != null && r.id > maxId) maxId = r.id;
        }
        EnsureExecCapacity(maxId);

        for (int i = 0; i < list.Count; i++)
        {
            var r = list[i];
            if (r == null) continue;
            var act = (r.id >= 0 && r.id < s_execById.Length) ? s_execById[r.id] : null;
            if (act != null) r.BindExecute(act);
        }
    }

    // 카탈로그에서 읽은 값 사전 주입(한 번만 호출하면 됨)
    public static void BindValuesFromCatalog(Dictionary<int, string[]> valuesById)
    {
        s_valuesById = valuesById ?? new Dictionary<int, string[]>(0);
    }

    // 모든 유산에 값 일괄 바인딩(카탈로그 로드 직후 보정용)
    public static void BindValuesOnAllRelics()
    {
        if (s_valuesById == null || relics == null) return;

        for (int i = 0; i < relics.Count; i++)
        {
            var r = relics[i];
            if (r == null) continue;
            if (s_valuesById.TryGetValue(r.id, out var vals))
                r.BindConfig(vals);
        }
    }

    #endregion



    //할인패 0
    private static void DiscountCoupon()
    {

    }
    //단단한 모루 1
    private static void ResearchBudgetGrant()
    {

    }
    //인내력의 깃발 2
    private static void IonDrink()
    {

    }
    //추가 보급 명령서 3
    private static void TrainingBaton()
    {

    }
    //도금 망원경 4
    private static void GildedTelescope()
    {
        int id = 4;
        var units = RogueLikeData.Instance.GetMyTeam();
        foreach (var unit in units)
        {
            if (unit.branchIdx == 2)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Range,
                    value = 1,
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
                break;
            }
        }
    }

    //행운의 주머니 5
    private static void LuckyPouch()
    {

    }

    //도굴꾼의 삽 6
    private static void GraveRobbersShovel()
    {

    }

    //기이한 돋보기 7
    private static void StrangeMagnifyingGlass()
    {

    }

    //순금 검 8
    private static void PureGoldSword(WarRelic relic)
    {
        int id = 8;
        int gold= RogueLikeData.Instance.GetCurrentGold();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;
        float addValue = gold / vals[0] * vals[1];

        var units = RogueLikeData.Instance.GetMyTeam();
        if (gold > 0)
        {
            foreach (var unit in units)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Health,
                    value = unit.baseHealth * addValue,
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
            }
        }
    }

    //맥동하는 인형 9
    private static void PulsatingDoll(WarRelic relic)
    {
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        int id = 9;

        var units = RogueLikeData.Instance.GetMyTeam();
        float addValue = vals[0];        
        foreach (var unit in units)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = unit.baseAttackDamage * addValue,
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });

        }
      
    }

    //광전사의 갑옷 10
    private static void BerserkersArmor(WarRelic relic)
    {
        var vals = relic?.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        int id = 10;
        RogueLikeData.Instance.AddMyMultipleDamage(vals[0]);
        var units = RogueLikeData.Instance.GetMyTeam();

        foreach (var unit in units)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Armor,
                value = vals[1],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }
    }

    // 용기의 버섯 11
    private static void MushroomOfCourage(WarRelic relic)
    {
        var vals = relic?.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        int id = 11;
        var units = RogueLikeData.Instance.GetMyUnits();

        // 유닛 데이터 수정
        foreach (var unit in units)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Health,
                value = unit.baseHealth * vals[0],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = unit.baseAttackDamage * vals[0],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
            if (unit.rangedAttack)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Range,
                    value = vals[1],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
            }
        }

        if (units == null || units.Count < 2) return;

        for (int i = units.Count - 1; i > 0; i--)
        {
            int j = RogueLikeData.Instance.GetRandomInt(0, i + 1);
            if (j == i) continue;
            (units[i], units[j]) = (units[j], units[i]);
        }

        RogueLikeData.Instance.SetAllMyUnits(units);
    }

    //만신전 모형 12
    private static void PantheonModel()
    {
        int id = 12;

        var units = RogueLikeData.Instance.GetMyTeam();
        HashSet<int> unitIds = new HashSet<int>();
        bool hasDuplicate = false;

        foreach (var unit in units)
        {
            if(unitIds.Contains(unit.idx))
            {
                hasDuplicate = true;
                break;
            }
            unitIds.Add(unit.idx);
        }
        if (!hasDuplicate)
        {
            foreach (var unit in units)
            {
                unitIds.Add(unit.idx); 
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Health,
                    value = unit.baseHealth * 0.2f,
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Health,
                    value = unit.baseAttackDamage * 0.2f,
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
            }
        }
    }

    //체스판 13
    private static void Chessboard(WarRelic relic)
    {
        int id = 13;
        
        var myTeam = RogueLikeData.Instance.GetMyTeam();

        int typeCount = myTeam
        .Where(u => u.branchIdx < 8) // 8 이상은 제외
        .Select(u => u.branchIdx)
        .Distinct()
        .Count();

        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        foreach (RogueUnitDataBase unit in myTeam)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Health,
                value = unit.baseHealth * vals[0]*typeCount,
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = unit.baseAttackDamage * vals[0] * typeCount,
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }
    }

    //가시 갑옷 14
    private static void SpikedArmor(WarRelic relic)
    {
        int id = 14;
        var units = RogueLikeData.Instance.GetMyTeam();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        foreach (var unit in units)
        {
            if (unit.thorns)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Armor,
                    value = vals[0],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
            }
            if(unit.heavyArmor && !unit.thorns)
            {
                unit.thorns = true;
            }
        }
    }

    //쌍둥이 방패 15
    private static void TwinShields(WarRelic relic)
    {
        int id = 15;
        var units = RogueLikeData.Instance.GetMyTeam();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;
        foreach (var unit in units)
        {
            if (unit.guard)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Armor,
                    value = vals[0],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
            }
        }
    }

    //헤르메스 신발 16
    private static void BootsOfHermes(WarRelic relic)
    {
        int id = 16;

        var units = RogueLikeData.Instance.GetMyTeam();

        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        foreach (var unit in units)
        {
            if(unit.lightArmor)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Mobility,
                    value = vals[0],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
            }
        }
    }
    
    //할버드 17
    private static void Halberd(WarRelic relic)
    {
        int id = 17;
        var units = RogueLikeData.Instance.GetMyUnits();

        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        var sortUnits = RogueUnitDataBase.OrderStrongUnits(units);
        foreach(RogueUnitDataBase unit  in sortUnits)
        {
            if(unit.branchIdx == 0)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Health,
                    value = unit.baseHealth*vals[0],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.AttackDamage,
                    value = unit.baseAttackDamage * vals[0],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
                return;
            }

        }

    }

    //정예 기병대 안장 18
    private static void EliteCavalrySaddle()
    {
       
    }

    //정예 궁병 부대 깃털모자 19
    private static void EliteArchersFeatheredCap(WarRelic relic)
    {
        int id = 19;

        var units = RogueLikeData.Instance.GetMyUnits();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if(vals == null) return;

        var sortUnits = RogueUnitDataBase.OrderStrongUnits(units);

        foreach (var unit in sortUnits)
        {
            if (unit.branchIdx == 2)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.AttackDamage,
                    value = unit.baseAttackDamage * vals[0],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Range,
                    value = vals[1],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
                return;
            }
        }
    }

    //민병대 나팔 20
    private static void MilitiaHorn(WarRelic relic)
    {
        int id = 20;
        var units = RogueLikeData.Instance.GetMyTeam();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if(vals == null) return;

        foreach (var unit in units)
        {
            if (unit.rarity == 1)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Health,
                    value = unit.baseHealth * vals[0],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.AttackDamage,
                    value = unit.baseAttackDamage * vals[0],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
            }
            if(unit.tagIdx == 2)
            {
                unit.bindingForce = true;
            }
        }
    }

    //소름끼치는 구슬 21
    private static void EerieOrb(WarRelic relic)
    {
        int id = 21;
        int curseCount = RogueLikeData.Instance.GetRelicsByGrade(0).Count;
        if (curseCount == 0) return;
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if(vals ==null) return;

        var units = RogueLikeData.Instance.GetMyTeam();

        foreach (var unit in units)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Health,
                value = unit.baseHealth * (curseCount * vals[0]),
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = unit.baseAttackDamage * (curseCount * vals[0]),
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }
    }

    //해주 부적 22
    private static void HaejuTalisman()
    {

    }

    //비어있는 보석 건틀렛 23
    private static void EmptyGemGauntlet()
    {
        RelicManager.CheckFusion();
    }

    //작은 보석 더미 24
    private static void SmallPileOfGems()
    {
        RelicManager.CheckFusion();
    }

    //커다란 보석 25
    private static void LargeGem()
    {
        RelicManager.CheckFusion();
    }

    //완성된 보석 건틀렛 26
    private static void CompletedGemGauntlet()
    {
        int id = 26;
        
        var units = RogueLikeData.Instance.GetEnemyUnits();

        foreach (var unit in units)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Health,
                value = -0.5f,
                source = SourceType.Relic,
                modifierId = id,
                isPercent = true
            });
        }
    }

    //하트 보석 목걸이 27
    private static void HeartGemNecklace()
    {

    }

    //용기의 깃발 28
    private static void FlagOfCourage(WarRelic relic)
    {
        int morale = RogueLikeData.Instance.GetMorale();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        float addValue;
        if(morale <= vals[0])
        {
            addValue = vals[1];
        }else if(morale >= vals[2])
        {
            addValue = vals[3];
        }
        else
        {
            return;
        }

        int id = 28;
        var myTeam = RogueLikeData.Instance.GetMyTeam();
        
        foreach (var unit in myTeam)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = addValue,
                source = SourceType.Relic,
                modifierId = id,
                isPercent = true,
            });
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Health,
                value = addValue,
                source = SourceType.Relic,
                modifierId = id,
                isPercent = true,
            });
        }

    }

    //부러진 직검 29
    private static void BrokenStraightSword(WarRelic relic)
    {
        int id = 29;
        var units = RogueLikeData.Instance.GetMyTeam();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        foreach (var unit in units)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = -unit.baseAttackDamage * vals[0],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }
    }

    //깨진 투구 30
    private static void CrackedHelmet()
    {


    }

    //해진 군화 31 
    private static void WornOutBoots(WarRelic relic)
    {
        int id = 31;
        var units = RogueLikeData.Instance.GetMyTeam();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        foreach (var unit in units)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Mobility,
                value = vals[0],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }
    }

    //갈라진 방패 32
    private static void SplitShield(WarRelic relic)
    {
        int id = 32;
        
        var units = RogueLikeData.Instance.GetMyTeam();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;
        foreach (var unit in units)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Armor,
                value = vals[0],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }
    }

    //황폐한 깃발 33
    private static void DesolateFlag()
    {

    }

    //생존자의 넝마떼기 34
    private static void SurvivorsRags()
    {

    }

    //정복자의 인장 35
    private static void ConquerorsSeal()
    {

    }

    //맹인전사의 안대 36
    private static void BlindWarriorsEyepatch(WarRelic relic)
    {
        int id = 36;
        var units = RogueLikeData.Instance.GetMyTeam();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if(vals == null) return;

        foreach (var unit in units)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = unit.baseAttackDamage * vals[0],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            }); 
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Health,
                value = unit.baseHealth * vals[0],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
            if (unit.rangedAttack)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Range,
                    value = vals[1],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
            }
        }
    }

    //덧댐 장갑판 37
    private static void ReinforcedArmorPlate(WarRelic relic)
    {
        int id = 37;
        
        var units = RogueLikeData.Instance.GetMyTeam();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if(vals == null) return;

        foreach (var unit in units)
        {
            if (unit.heavyArmor)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Armor,
                    value = vals[0],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Mobility,
                    value = vals[1],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
            }
        }
    }

    //장식된 단검 38
    private static void OrnamentedDagger(WarRelic relic)
    {
        int id = 38;
        
        var units = RogueLikeData.Instance.GetMyUnits();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if(vals == null) return;

        var sortUnits = RogueUnitDataBase.OrderStrongUnits(units);

        foreach (var unit in sortUnits)
        {
            if(unit.branchIdx == 4)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.AttackDamage,
                    value = unit.baseAttackDamage * vals[0],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
                return;
            }
            
        }
    }

    //전쟁나팔 39
    private static void WarHorn()
    {

    }

    //무료 배식권 40
    private static void FreeMealTicket(WarRelic relic)
    {
        var myTeam = RogueLikeData.Instance.GetMyTeam();
        var vals = relic?.GetAllValuesAsFloatListOrNull();
        if(vals == null) return;
        foreach (var unit in myTeam)
        {
            unit.Energy += (int)vals[0];
        }
    }

    //파괴공작용 대포 41
    private static void SabotageCannon(WarRelic relic)
    {
        StageType stage = RogueLikeData.Instance.GetCurrentStageType();
        if(stage == StageType.Combat || stage == StageType.Elite)
        {
            int id = 41;
            var units = RogueLikeData.Instance.GetEnemyUnits();
            var vals = relic.GetAllValuesAsFloatListOrNull();
            if(vals == null) return;


            foreach (var unit in units)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Health,
                    value = unit.baseHealth * vals[0],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
            }
        }
    }

    // 자율 개발 명령서 42
    private static void AutonomousDevelopmentOrder()
    {
        var myUnits = RogueLikeData.Instance.GetMyTeam();
        var unitTypes = myUnits.Select(u => u.branchIdx)
                               .Distinct()
                               .Where(t => t >= 0 && t < 8)
                               .ToList();

        if (unitTypes.Count == 0) return;

        for (int i = 0; i < unitTypes.Count; i++)
        {
            int r = RogueLikeData.Instance.GetRandomInt((int)i, (int)unitTypes.Count);
            (unitTypes[i], unitTypes[r]) = (unitTypes[r], unitTypes[i]);
        }

        int count = Mathf.Min(3, unitTypes.Count);
        for (int i = 0; i < count; i++)
        {
            int type = unitTypes[i];
            bool isAttack = RogueLikeData.Instance.GetRandomFloat() < 0.5f;
            RogueLikeData.Instance.IncreaseUpgrade(type, isAttack, false);
        }
    }

    //해진 정찰 보고서 43
    private static void EnemyGeneralScoutReport(WarRelic relic)
    {
        StageType currentStage = RogueLikeData.Instance.GetCurrentStageType();
        if(currentStage== StageType.Elite || currentStage == StageType.Boss)
        {
            var vals = relic.GetAllValuesAsFloatListOrNull();
            if (vals == null) return;

            RogueLikeData.Instance.AddMyMultipleDamage(vals[0]);
        }
    }

    //추가 징병 계획서 44
    private static void MistakenOrderReceipt(WarRelic relic)
    {
        int id = 44;

        var myUnits = RogueLikeData.Instance.GetMyUnits();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        int maxUnit = RogueLikeData.Instance.GetMaxUnits();

        if (myUnits.Count >= maxUnit) return;

        foreach (var unit in myUnits)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Health,
                value = unit.baseHealth * vals[0],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }

    }

    //결속의 상징 45
    private static void SymbolOfUnity(WarRelic relic)
    {
        int id = 45;
        
        var units = RogueLikeData.Instance.GetMyUnits();
        bool allCorret=true;
        int branchIdx = units[0].branchIdx;
        foreach (var unit in units)
        {
            if (branchIdx != unit.branchIdx)
            {
                allCorret = false;
                break;
            }
        }
        if (!allCorret) return;

        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        foreach (var unit in units)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = unit.baseAttackDamage * vals[0],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Health,
                value = unit.baseHealth * vals[0],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }
    }

    //기술 비급서 46
    private static void TechnicalSecretTome()
    {

    }

    //보물지도 47
    private static void TreasureMap()
    {

    }

    //무지개 열쇠 48
    private static void RainbowKey()
    {

    }

    //재상의 보증서 49
    private static void CreditAuthorization()
    {

    }

    //순금 나팔 50
    private static void GoldenHorn(WarRelic relic)
    {
        int id = 50;
        int spentGold = RogueLikeData.Instance.GetSpentGold();

        var myTeam = RogueLikeData.Instance.GetMyTeam();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        float addValue = spentGold / vals[0] * vals[1];
        foreach (var unit in myTeam)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = unit.baseAttackDamage * addValue,
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }

    }

    //두꺼운 전술 교범 51
    private static void ThickTacticsManual()
    {

    }

    //탐험가의 나침반 52
    private static void ExplorersCompass()
    {

    }

    //불운의 황금 동전 53
    private static void UnluckyGoldCoin(WarRelic relic)
    {
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        RogueLikeData.Instance.AddMyMultipleDamage(vals[0]);
    }

    //저주 인형 54
    private static void CursedDoll()
    {

    }

    // 혼돈의 주사위 (Relic 55) - 최소 60%, 최대 200% 값으로 설정 55
    private static void DiceOfChaos(WarRelic relic)
    {
        int id = 55;
        var units = RogueLikeData.Instance.GetMyTeam();
        System.Random random = RogueLikeData.Instance.GetRandomBySeed();

        var vals = relic.GetAllValuesAsFloatListOrNull();
        if(vals == null) return;

        foreach (var unit in units)
        {
            // 체력과 공격력을 각각 60%~200% 사이의 랜덤 값으로 조정
            float healthMultiplier = (float)(random.NextDouble() * (vals[1] - vals[0]) + vals[0]);
            float attackMultiplier = (float)(random.NextDouble() * (vals[1] - vals[0]) + vals[0]);

            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Health,
                value = unit.baseHealth * (healthMultiplier-1),
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = unit.baseAttackDamage * (attackMultiplier-1),
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }
    }


    //광전사의 머리칼 56
    private static void BerserkersHair()
    {

    }

    //용사의 훈장 57
    private static void MedalOfBravery()
    {

    }

    //횡령증거품 58
    private static void EvidenceOfEmbezzlement()
    {

    }

    //비축된 하몽 59
    private static void WarReport()
    {

    }

    //무명의 군단 배지 60
    private static void BadgeOfNamelessLegion()
    {
        
    }

    //뜨거운 심장 모형 61
    private static void HotHeartModel(WarRelic relic)
    {
        int id = 61;
        var units = RogueLikeData.Instance.GetMyTeam();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        foreach (var unit in units)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Health,
                value = unit.baseHealth * vals[0],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }
    }
    //약자낙인 인두 62
    private static void BrandOfTheUnderdog(WarRelic relic)
    {
        int id = 62;
        var units = RogueLikeData.Instance.GetMyTeam();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        foreach (var unit in units)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Health,
                value = unit.baseHealth * vals[0],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }
    }
    //매우 진한 스프 63
    private static void VeryThickSoup()
    {
        var units = RogueLikeData.Instance.GetMyTeam();
        foreach (var unit in units)
        {
            unit.Energy = unit.MaxEnergy;
        }
    }
    //전쟁 군주의 투구 64
    private static void WarlordsHelm(WarRelic relic)
    {
        int id = 64;
        var units = RogueLikeData.Instance.GetMyTeam();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        foreach (var unit in units)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Armor,
                value = vals[0],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }
    }
    //전쟁 군주의 검 65
    private static void WarlordsSword(WarRelic relic)
    {
        int id = 65;
        var units = RogueLikeData.Instance.GetMyTeam();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if(vals == null) return;

        foreach (var unit in units)
        {
            if (unit.Energy >= vals[0])
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.AttackDamage,
                    value = unit.baseAttackDamage * vals[1],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Health,
                    value = unit.baseHealth * vals[1],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
            }
            else
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.AttackDamage,
                    value = unit.baseAttackDamage * vals[2],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Health,
                    value = unit.baseHealth * vals[2],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
            }

        }
    }
    //확장 진형도 66
    private static void ExpandedFormationDiagram()
    {

    }
    //전쟁 군주의 휘장 67
    private static void WarlordsInsignia(WarRelic relic)
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        bool[] seen = new bool[8];
        int typeCount = 0;

        for (int i = 0; i < myUnits.Count; i++)
        {
            int b = myUnits[i].branchIdx;
            if ((uint)b < 8u) // 0~7만 집계
            {
                if (!seen[b])
                {
                    seen[b] = true;
                    typeCount++;
                }
            }
        }
        if (typeCount > vals[1]) return;
        int id = 67;

        foreach (var unit in myUnits)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Health,
                value = unit.baseHealth * vals[2],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }
    }
    //전리품 주머니 68
    private static void LootBag(WarRelic relic)
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null || units == null || units.Count == 0) return;

        for (int i = 0; i < units.Count; i++)
        {
            if (units[i].plunder) return;
        }

        List<RogueUnitDataBase> candidates = new List<RogueUnitDataBase>(units.Count);
        for (int i = 0; i < units.Count; i++)
        {
            if (!units[i].plunder)
                candidates.Add(units[i]);
        }
        if (candidates.Count == 0) return;

        int giveCount = (int)vals[1];
        if (giveCount <= 0) return;
        if (giveCount > candidates.Count) giveCount = candidates.Count;

        System.Random rnd = RogueLikeData.Instance.GetRandomBySeed();
        int n = candidates.Count;
        for (int i = 0; i < giveCount; i++)
        {
            int j = i + rnd.Next(n - i);
            var tmp = candidates[i];
            candidates[i] = candidates[j];
            candidates[j] = tmp;

            candidates[i].plunder = true;
        }
    }
    //전위대의 갑옷 69
    private static void VanguardArmor(WarRelic relic)
    {
        int id = 69;
        var units = RogueLikeData.Instance.GetMyUnits();
 
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        var sortUnits = RogueUnitDataBase.OrderStrongUnits(units);

        foreach (var unit in sortUnits)
        {
            if(unit.branchIdx == 3)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Health,
                    value = unit.baseHealth * vals[0],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });

                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Armor,
                    value = vals[1],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });

                return;
            }
        }
       
    }
    //선봉대 군화 70
    private static void VanguardBoots(WarRelic relic)
    {
        var units = RogueLikeData.Instance.GetMyTeam(); // 사용처: 내 팀 중 branchIdx==5 유닛 2명 무작위 선택
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null || units == null || units.Count == 0) return;

        // 후보 수집: branchIdx == 5만
        List<RogueUnitDataBase> candidates = new List<RogueUnitDataBase>(units.Count);
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i].branchIdx == 5)
                candidates.Add(units[i]);
        }
        if (candidates.Count == 0) return;

        // 선택할 수: 최대 2명
        int pick = candidates.Count >= (int)vals[0] ? (int)vals[0] : candidates.Count;

        // 부분 셔플로 서로 다른 유닛 pick개 선택 (연산속도 우선)
        System.Random rnd = RogueLikeData.Instance.GetRandomBySeed();
        int n = candidates.Count;
        for (int i = 0; i < pick; i++)
        {
            int j = i + rnd.Next(n - i); // [i, n-1]
            var tmp = candidates[i];
            candidates[i] = candidates[j];
            candidates[j] = tmp;
        }

        // 선택된 유닛들: candidates[0..pick-1]
        var selectedUnits = new List<RogueUnitDataBase>(pick);
        for (int i = 0; i < pick; i++)
            selectedUnits.Add(candidates[i]);

        foreach (var unit in selectedUnits)
        {
            unit.pierce = true;
        }


    }
    //녹슨 쇠말뚝 71
    private static void RustyIronStake(WarRelic relic)
    {
        var myTeam = RogueLikeData.Instance.GetMyTeam();
        foreach (var unit in myTeam)
        {
            unit.perfectAccuracy = true;
        }
    }
    //수상한 부등변다면체 72
    private static void SuspiciousScalenePolyhedron(WarRelic relic)
    {
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        var hero = RogueUnitDataBase.GetRandomUnitByRarity(4);

        if (RogueLikeData.Instance.GetRandomFloat() >= vals[0])
        {
            var myUnits = RogueLikeData.Instance.GetMyUnits();
            int insertIndex = RogueLikeData.Instance.GetRandomInt(0, myUnits.Count + 1);
            myUnits.Insert(insertIndex, hero);
            RogueLikeData.Instance.SetAllMyUnits(myUnits);
        }
        else
        {
            var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();
            int insertIndex = RogueLikeData.Instance.GetRandomInt(0, enemyUnits.Count + 1);
            enemyUnits.Insert(insertIndex, hero);
            RogueLikeData.Instance.SetAllEnemyUnits(enemyUnits);
        }
    }

    //뭐든지 들어있는 상자 73
    private static void AnythingBox(WarRelic relic)
    {
        relic.used = true;
        RelicManager.HandleRandomRelicAllGrades(RelicManager.RelicAction.Acquire);
    }
    //신성한 문서 74
    private static void SacredDocument()
    {

    }
    //푯대 75
    private static void Signpost()
    {
        var units = RogueLikeData.Instance.GetMyTeam();
        foreach (var unit in units)
        {
            int id = 5, type = 0, rank = 1, duration = -1;
            unit.effectDictionary[id]=new(id, type, rank,duration);
        }
    }
    //경랑 갑옷 76
    private static void LightweightArmor()
    {
        var units = RogueLikeData.Instance.GetMyTeam();
        foreach (var unit in units)
        {
            int id = 6, type = 0, rank = 1, duration = -1;
            if (unit.lightArmor)
            {
                unit.effectDictionary[id] = new(id, type, rank, duration);
            }
        }
    }
    //뿔피리 77
    private static void Horn()
    {

    }
    //사리 유산 78
    private static void SariHeritage()
    {
        int id = 78;
        var units = RogueLikeData.Instance.GetMyTeam();
        int sariStack = RogueLikeData.Instance.GetSariStack();
        foreach(var unit in units)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Health,
                value = unit.baseHealth * 0.01f * sariStack,
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = unit.baseAttackDamage * 0.01f * sariStack,
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }
    }
    //기이한 조각 79
    private static void StrangePiece()
    {
    }
    //합금 박차 80
    private static void AlloySpur(WarRelic relic)
    {
        int id = 80;
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        var units = RogueLikeData.Instance.GetMyTeam();
        RogueUnitDataBase front =units[0];
        if (front.branchIdx==5 || front.branchIdx == 6)
        {
            front.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = front.baseHealth * vals[0],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
            front.strongCharge =true;
        }
    }
    //벼려진 마창 81
    private static void ForgedLance()
    {
        int id = 81;
        var units = RogueLikeData.Instance.GetMyTeam();
        foreach (var unit in units)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = (unit.baseMobility * (20 / 9)) - (20 / 9),
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }
    }
    //반응 갑옷 82
    private static void ReactiveArmor()
    {
        int id = 82;
        var units = RogueLikeData.Instance.GetMyTeam();
        foreach (var unit in units)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = (unit.baseArmor * (20 / 9)) - (20 / 9),
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }
    }
    //말뚝 방책 83
    private static void SpearManual(WarRelic relic)
    {
        int id = 83;
        var units = RogueLikeData.Instance.GetMyTeam();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        foreach (var unit in units)
        {
            if (unit.defense)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Health,
                    value = vals[0],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
            }
        }
    }
    //치유석 84
    private static void HealingStone(WarRelic relic)
    {
        int id = 84;
        var myTeam = RogueLikeData.Instance.GetMyTeam();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if(vals == null) return;

        foreach(var unit in myTeam)
        {
            if (unit.healing)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.AttackDamage,
                    value = unit.baseAttackDamage * vals[0],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Health,
                    value = unit.baseHealth * vals[1],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
            }
            
        }


    }
    //정예병 모집서 85
    private static void EliteRecruitmentOrder()
    {

    }
    //커다란 짐수레 86
    private static void LargeCart()
    {

    }
    //파이브오브어카인드 87
    private static void FiveOfAKind(WarRelic relic)
    {
        int id = 87;
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        var distinctBranches = new HashSet<int>();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if(vals == null) return;

        // 병종 수 체크
        foreach (var unit in myUnits)
        {
            distinctBranches.Add(unit.branchIdx);
            if (distinctBranches.Count > vals[0]) break;
        }

        if (distinctBranches.Count < vals[0]) return;

        // 체력 +15%
        foreach (var unit in myUnits)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Health,
                value = unit.baseHealth * vals[1],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }
    }
    //로열 스트레이트 플러시 88
    private static void RoyalStraightFlush(WarRelic relic)
    {
        int id = 88;
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        var distinctBranches = new HashSet<int>();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if(vals == null) return;

        // 병종 수 체크
        foreach (var unit in myUnits)
        {
            distinctBranches.Add(unit.branchIdx);
            if(distinctBranches.Count > vals[0]) break;
        }

        foreach (var unit in myUnits)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = unit.baseAttackDamage * vals[1],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }
    }
    //찢겨진 명단 89
    private static void TornList()
    {

    }
    //깨진 거울 90
    private static void BrokenMirror()
    {

    }
    //불길한 족쇄 91
    private static void OminousShackles()
    {

    }
    //맛있는 군용식량 92
    private static void DeliciousRations(WarRelic relic)
    {
        int id = 92;
        var myUnits = RogueLikeData.Instance.GetMyTeam();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if(vals == null) return;

        foreach(var unit in myUnits)
        {
            if (unit.rarity == 1)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Health,
                    value = unit.baseHealth * vals[0],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });

            }else if(unit.rarity == 4)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Health,
                    value = unit.baseHealth * vals[1],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
            }
            else if(unit.rarity == 3)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Health,
                    value = unit.baseHealth * vals[2],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
            }
        }
    }
    //맛있는 특별식 93
    private static void DeliciousSpecialMeal(WarRelic relic)
    {
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;
        RogueLikeData.Instance.ChangeMorale((int)vals[0]);
    }
    //무쇠 투구 94
    private static void CastIronHelmet()
    {

    }

    //전속전진의 신발 95
    private static void ShoesOfFullSpeedAhead(WarRelic relic)
    {
        int id = 95;
        var myUnits = RogueLikeData.Instance.GetMyTeam();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        foreach (var unit in myUnits)
        {
            if (unit.impact)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Mobility,
                    value = vals[0],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
            }
            if (unit.charge)
            {
                unit.impact = true;
            }
        }
    }
    //장식된 로자리오 96
    private static void DecoratedRosary()
    {

    }
    //골판지 상자 97
    private static void EmergencyEscapeManual(WarRelic relic)
    {
        int id = 12, type = 0, rank = 1, duration = 1;
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        duration = (int)vals[0];
        foreach( var unit in myUnits)
        {
            if (unit.branchIdx == 2 || unit.branchIdx == 7)
            {
                unit.effectDictionary[id] = new BuffDebuffData(id, type, rank, duration);
            }
        }
    }
    // 98
    //욕망의 항아리 99
    private static void JarOfDesire()
    {
        
    }
    //무지개의 시작 100
    private static void BeginningOfTheRainbow(WarRelic relic)
    {
        int id = 100;
        RogueUnitDataBase unit = RogueLikeData.Instance.GetMyTeam()[0];
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        unit.SetRandomTraits((int)vals[0]);
        unit.stats.AddModifier(new StatModifier
        {
            stat = StatType.AttackDamage,
            value = vals[1],
            source = SourceType.Relic,
            modifierId = id,
            isPercent = false
        });
    }
    //지평선의 끝 101
    private static void EndOfTheHorizon(WarRelic relic)
    {
        int id = 101;
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        var last = myUnits[myUnits.Count - 1];
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        last.stats.AddModifier(new StatModifier
        {
            stat = StatType.Health,
            value = last.baseHealth * vals[0],
            source = SourceType.Relic,
            modifierId = id,
            isPercent = false
        });
        last.stats.AddModifier(new StatModifier
        {
            stat = StatType.AttackDamage,
            value = last.baseAttackDamage * vals[0],
            source = SourceType.Relic,
            modifierId = id,
            isPercent = false
        });
    }
    //현자의 돌 102
    private static void GamblersFate(WarRelic relic)
    {
        int id = 102;
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;
        int gold = RogueLikeData.Instance.GetCurrentGold();
        int spentGold = (int)(gold * vals[0]);
        if (spentGold == 0) return;
        RogueLikeData.Instance.ReduceGold(spentGold);
        float addAttack = spentGold * 0.001f;
        float addHealth = spentGold * 0.0005f;
        var myUnits = RogueLikeData.Instance.GetMyTeam();
        foreach (var unit in myUnits)
        {
            if (unit.tagIdx != 1) continue;
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = unit.baseHealth * addHealth,
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = unit.baseAttackDamage * addAttack,
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }
    }
    //흑요석 심장 103
    private static void ObsidianHeart(WarRelic relic)
    {
        int id = 14, type = 0, rank = 1, duration = 2;
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;
        duration = (int)vals[0];

        var myUnits = RogueLikeData.Instance.GetMyUnits();
        var sortUnit = RogueUnitDataBase.OrderStrongUnits(myUnits)[0];

        sortUnit.effectDictionary[id] = new BuffDebuffData(id, type, rank, duration);

    }
    //긍지의 양날도끼 104
    private static void DoubleEdgedAxeOfPride(WarRelic relic)
    {
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        var myUnits = RogueLikeData.Instance.GetMyUnits();
        var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();

        // 로컬 함수로 공통 처리
        void Process(List<RogueUnitDataBase> units)
        {
            if (units == null || units.Count <= 1) return;

            int n = units.Count;
            int pick = n >= (int)vals[0] ? (int)vals[0] : n;

            for (int i = 0; i < pick; i++)
            {
                int r = RogueLikeData.Instance.GetRandomInt(i, n);
                (units[i], units[r]) = (units[r], units[i]);
            }

            for (int i = 0; i < pick; i++)
            {
                RogueUnitDataBase u = units[i];
                if (u == null) continue;
            }
        }

        Process(myUnits);
        Process(enemyUnits);

        RogueLikeData.Instance.SetAllMyUnits(myUnits);
        RogueLikeData.Instance.SetAllEnemyUnits(enemyUnits);

    }
    //저주받은 갑옷 105
    private static void CursedArmor(WarRelic relic)
    {
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if(vals ==null) return;

        RogueLikeData.Instance.AddEnemyMultipleDamage(vals[0]);
    }
    //제어 횟불 106
    private static void ControlTorch()
    {

    }
    //훈장 무더기 107
    private static void PileOfMedals(WarRelic relic)
    {
        int id = 107;
        var myUnits = RogueLikeData.Instance.GetMyTeam();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if(vals ==null) return;

        foreach (var unit in myUnits)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Health,
                value = unit.baseHealth * vals[0],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = unit.baseAttackDamage * vals[0],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });

        }
    }
    //애매한 묵시록 108
    private static void AmbiguousApocalypse(WarRelic relic)
    {
        int id = 108;
        int morale = RogueLikeData.Instance.GetMorale();
        var myUnits = RogueLikeData.Instance.GetMyTeam();
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if(vals ==null) return;

        int addAttack = 0;
        if(morale >= (int)vals[0])
        {
            addAttack = (int)vals[1];
        }else if(morale <= (int)vals[2])
        {
            addAttack = (int)vals[3];
        }
        foreach (var unit in myUnits)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = addAttack,
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }
    }
    //훈련용 모래주머니 109
    private static void TrainingSandbagsOfWar()
    {
 
    }
    //대서사시 110
    private static void Epic()
    {
        RogueUnitDataBase unit = RogueUnitDataBase.GetRandomUnitByRarity(4);
        RogueLikeData.Instance.AddMyTeam(unit);
        RogueLikeData.Instance.AcquireRelic(89);
    }
    //멸시의 오브 111
    private static void OrbOfContempt()
    {

    }
    //질긴 채찍 112
    private static void ToughWhip(WarRelic relic)
    {
        int id = 112;
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        var myUnits = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in myUnits)
        {
            if(unit.tagIdx == 1)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Health,
                    value = unit.baseHealth* vals[0],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
            }
            if(unit.branchIdx == 0)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Health,
                    value = unit.baseHealth * vals[1],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
            }
        }
    }
    //핏빛 염료 113
    private static void BloodSoakedDye(WarRelic relic)
    {
        int id = 113;
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        int addAttack = (int)vals[4] / 5;
        if (addAttack > 0)
        {
            foreach (var unit in myUnits)
            {
                if(unit.range == 1)
                {
                    unit.stats.AddModifier(new StatModifier
                    {
                        stat = StatType.AttackDamage,
                        value = addAttack,
                        source = SourceType.Relic,
                        modifierId = id,
                        isPercent = false
                    });
                }
            }
        }
    }
    // 병마용 114
    private static void TerracottaArmy()
    {

    }
    //투창용 깃창 115
    private static void ThrowingJavelin()
    {
        var myUnits = RogueLikeData.Instance.GetMyTeam();
        foreach(var unit in myUnits)
        {
            if (unit.branchIdx == 0)
            {
                unit.throwSpear = true;
            }
        }
    }
    //영광의 대가 116
    private static void PriceOfGlory() 
    { 
    }
    //정말 긴 창 117
    private static void ReallyLongSpear()
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();

        foreach(var unit in myUnits)
        {
            if(unit.branchIdx == 0)
            {
                unit.defense = false;
                unit.charge = true;
                unit.impact = true;
            }
        }
    }
    //결투가의 양날검 118
    private static void DuelistsGreatsword(WarRelic relic)
    {


    }
    //제국의 가시벽 119
    private static void ImperialThornWall(WarRelic relic)
    {
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        var myUnits = RogueLikeData.Instance.GetMyUnits();
        int spearCount = 0;
        foreach (var unit in myUnits)
        {
            if(unit.branchIdx == 0)
            {
                spearCount++;
            }
            else
            {
                spearCount = 0;
            }
            if (spearCount >= vals[0]) break;
        }

        if (spearCount >= vals[0])
        {
            foreach (var unit in myUnits)
            {
                if(unit.branchIdx == 0)
                {
                    unit.thorns = true;
                }
            }
            relic.used = true;
        }

    }
    //수급 주머니 120
    private static void TrophyPouch(WarRelic relic)
    {
        int id = 120;
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        var myUnits = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in myUnits)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = vals[0],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }

    }
    //성채 파괴자 121
    private static void CitadelBreaker()
    {

    }
    //피에 젖은 서약 122
    private static void BloodstainedOath(WarRelic relic)
    {
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        if (vals[3] < vals[1]) return;
        int id = 122;
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        foreach(var unit in myUnits)
        {
            if(unit.branchIdx == 1)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.AttackDamage,
                    value = unit.baseAttackDamage * vals[2],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
            }
        }
    }
    //끝없는 탄막 123
    private static void EndlessBarrage(WarRelic relic)
    {
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;
        if (vals[1] == 0) return;
        int id = 123;
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        foreach( var unit in myUnits)
        {
            if (unit.branchIdx == 2)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.AttackDamage,
                    value = vals[1],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
            }
        }
    }
    //작별의 납탄 124
    private static void PartingShot()
    {

    }
    //흑요석 화살촉 125
    private static void ObsidianArrowhead(WarRelic relic)
    {
        int id = 125;
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        var myUnits = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in myUnits)
        {
            if (unit.branchIdx == 2)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Health,
                    value = unit.baseHealth*vals[0],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.AttackDamage,
                    value = unit.baseAttackDamage*vals[1],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
            }

        }

    }
    //무거운 철퇴 126
    private static void HeavyMace()
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in myUnits)
        {
            if(unit.branchIdx == 3)
            {
                unit.bluntWeapon = true;
            }
        }
    }
    //수호신의 망토 127
    private static void GuardiansCloak(WarRelic relic)
    {
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        RogueUnitDataBase strongUnit = null;
        foreach (var unit in myUnits)
        {
            if (unit.heavyArmor)
            {
                if(strongUnit != null || strongUnit.maxHealth < unit.maxHealth)
                {
                    strongUnit = unit;
                }
            }
        }
        if (strongUnit != null)
        {
            int id = 127;
            int buffId = 15, type = 0, rank = 1, duration = -1;

            strongUnit.effectDictionary[buffId] = new BuffDebuffData(buffId, type, rank, duration);

            strongUnit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = strongUnit.baseAttackDamage * vals[1],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }

        relic.used =true;
    }
    //신뢰의 유산 128
    private static void LegacyOfTrust()
    {
        int id = 128;
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        int addArmor = 0;
        foreach (var unit in myUnits)
        {
            if(addArmor > 0)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Armor,
                    value = addArmor,
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });
            }
            if(unit.branchIdx == 3)
            {
                addArmor++;
            }
        }
    }
    //청부 명세서 129
    private static void ContractInvoice()
    {

    }
    //망령의 두건 130
    private static void SpectersCowl()
    {

    }
    //파멸의 박차 131
    private static void SpursOfDoom(WarRelic relic)
    {
        int id = 131;
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in myUnits)
        {
            if (unit.branchIdx == 5)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Mobility,
                    value = vals[1],
                    source = SourceType.Relic,
                    modifierId = id,
                    isPercent = false
                });

            }
        }

    }
    //태풍을 부르는 눈알 132
    private static void TyphoonCallingEye(WarRelic relic)
    {
        int id = 132;
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;

        int buffId = 16, type = 0, rank = 1, duration = (int)vals[0];
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        foreach(var unit in myUnits)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Mobility,
                value = vals[1],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
            unit.effectDictionary[buffId] = new BuffDebuffData(buffId, type, rank,duration);
        }


    }
    //정의로운 빛의 검 133
    private static void SwordOfRighteousLight()
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in myUnits)
        {
            if (unit.pierce)
            {
                unit.perfectAccuracy = true;
            }
        }

    }
    //무모한 기사의 투구 134
    private static void RecklessKnightsHelm()
    {

    }
    //작은 공성퇴 135
    private static void SmallBatteringRam()
    {

    }
    //엉터리 오브 136
    private static void FakeOrb(WarRelic relic)
    {
        int id = 136;
        var vals = relic.GetAllValuesAsFloatListOrNull();
        if (vals == null) return;
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in myUnits)
        {
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Range,
                value = vals[0],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Health,
                value = unit.baseHealth * vals[1],
                source = SourceType.Relic,
                modifierId = id,
                isPercent = false
            });
        }

    }

}
