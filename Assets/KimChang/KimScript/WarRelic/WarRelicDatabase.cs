using DG.Tweening.Plugins.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using static RogueLikeData;


public static class WarRelicDatabase
{
    public static List<WarRelic> relics = new List<WarRelic>();

    private static System.Random random = new();
    static WarRelicDatabase()
    {
        relics.Add(new WarRelic(0, "할인패", 1, "용병단과 상단의 금화 가격이 20% 감소한다.", RelicType.SpecialEffect, () => DiscountCard()));
        relics.Add(new WarRelic(1, "연구 예산 지원금", 1, "군사 아카데미의 강화 비용이 20% 감소한다.", RelicType.SpecialEffect, () => SolidAnvil()));
        relics.Add(new WarRelic(2, "인내력의 깃발", 1, "기력이 감소할 때마다 25% 확률로 감소하지 않는다.", RelicType.SpecialEffect, () => FlagOfEndurance()));
        relics.Add(new WarRelic(3, "추가 보급 명령서", 1, "병영에서 유닛 훈련에 요구하는 턴이 1 감소한다. (1 미만으로 내려가지 않음)", RelicType.SpecialEffect, () => ExtraSupplyOrder()));
        relics.Add(new WarRelic(4, "도금 망원경", 1, "아군 궁병 중 가장 앞에 있는 궁병에 한해서 사거리가 1 증가한다.", RelicType.StateBoost, () => GoldPlatedTelescope()));
        relics.Add(new WarRelic(5, "행운의 금화 주머니", 1, "얻는 금화량이 15% 증가한다.", RelicType.SpecialEffect, () => LuckyCoinPouch()));
        relics.Add(new WarRelic(6, "전설의 도굴꾼의 삽", 1, "다음에 전쟁 유산 보상을 얻을때 전설 전쟁 유산이 확정으로 등장한다. 보스에선 적용되지 않는다. 전설 전쟁 유산을 얻은 이후 이 유산은 소멸한다.", RelicType.SpecialEffect, () => LegendaryDiggerShovel()));
        relics.Add(new WarRelic(7, "기이한 돋보기", 20, "엘리트 전투에서 승리할때 리롤을 1회 얻고, 보물을 열때 리롤을 3회 얻는다.", RelicType.SpecialEffect, () => ArchaeologyKit()));
        relics.Add(new WarRelic(8, "순금 검", 1, "소유 중인 금화 100당 아군 유닛의 공격력이 1% 증가한다.", RelicType.StateBoost, () => GoldenSword()));
        relics.Add(new WarRelic(9, "예리한 양날도끼", 10, "적과 아군이 받는 피해가 각 30% / 10% 증가한다.", RelicType.StateBoost, () => SharpDoubleAxe()));
       
        relics.Add(new WarRelic(10, "광기의 깃발", 10, "적이 받는 피해가 20% 증가하고, 아군 유닛의 장갑이 1 감소한다. (0 미만으로 내려가지 않음)", RelicType.StateBoost, () => FlagOfMadness()));
        relics.Add(new WarRelic(11, "혼돈의 깃발", 10, "아군의 배치가 무작위 순서로 배치된다. 아군 유닛의 체력과 공격력이 30% 증가하고, 아군 궁병의 사거리가 1 증가한다.", RelicType.StateBoost, () => FlagOfChaos()));
        relics.Add(new WarRelic(12, "위대한 지휘관의 훈장", 10, "배치된 유닛들의 이름 중에 중복되는 이름이 없다면 부대 상한이 3 증가하며, 체력과 공격력이 20% 증가한다.", RelicType.StateBoost, () => MedalOfGreatCommander()));
        relics.Add(new WarRelic(13, "유연성의 부적", 10, "가진 유닛 병종의 종류가 늘어날 시 아군 유닛의 체력과 공격력이 6% 증가한다. 모든 병종의 유닛을 각 둘 이상 소유했다면, 모든 강화의 단계가 1단계 상승한다.", RelicType.StateBoost, () => AmuletOfFlexibility()));
        relics.Add(new WarRelic(14, "가시 갑옷", 10, "중갑을 특성을 가진 아군 유닛에게 가시 특성을 부여한다. 원래 가시를 가진 유닛은 장갑이 2 증가한다.", RelicType.StateBoost, () => ReactiveThornArmor()));
        relics.Add(new WarRelic(15, "수호자의 훈장", 10, "수호 스킬을 가진 아군 유닛의 장갑이 4 증가하고, 공격 받을때 피해를 둘로 나눠서 받는다.", RelicType.ActiveState, () => MedalOfImperialGuard()));
        relics.Add(new WarRelic(16, "헤르메스 신발", 10, "경갑 유닛의 기동력이 8 증가한다.", RelicType.StateBoost, () => VeryLightMilitaryPants()));
        relics.Add(new WarRelic(17, "팔랑크스 전술서", 10, "창병 유닛의 대기병이 50% 증가한다.", RelicType.StateBoost, () => PhalanxTacticsBook()));
        relics.Add(new WarRelic(18, "정예 기병대 안장", 10, "기병 유닛에게 \"맹진\" 특성을 부여한다. 맹진: 4회 공격마다 돌격을 다시 시전한다.", RelicType.StateBoost, () => EliteCavalrySaddle()));
        relics.Add(new WarRelic(19, "정예 궁병 부대 깃털모자", 10, "궁병의 공격력이 20% 증가하며, 암살 기술에 대한 회피율이 3배 증가한다.", RelicType.ActiveState, () => EliteArcherFeatherHat()));
       
        relics.Add(new WarRelic(20, "민병대 나팔", 10, "희귀도 1 유닛의 체력과 공격력이 15% 증가한다.", RelicType.StateBoost, () => MilitiaHorn()));
        relics.Add(new WarRelic(21, "소름끼치는 구슬", 10, "소유 중인 저주등급 유산 한개마다 아군 유닛의 체력과 공격력이 10% 증가한다.", RelicType.StateBoost, () => CreepyOrb()));
        relics.Add(new WarRelic(22, "해주 부적", 20, "저주등급 유산의 해로운 효과를 받지 않게 된다.", RelicType.AllEffect, () => HaejuAmulet()));
        relics.Add(new WarRelic(23, "비어있는 보석 건틀렛", 10, "당장에는 아무런 효과도 없다. 작은 구멍 여러개와 커다란 구멍 하나가 있는 건틀렛이다.", RelicType.AllEffect, () => EmptyGemGauntlet()));
        relics.Add(new WarRelic(24, "작은 보석 더미", 10, "당장에는 아무런 효과도 없다. 작은 보석 여러개의 더미로, 색상이 다양하다.", RelicType.AllEffect, () => SmallGemPile()));
        relics.Add(new WarRelic(25, "커다란 보석", 10, "당장에는 아무런 효과도 없다. 커다랗고 하얀 보석이다.", RelicType.AllEffect, () => LargeGem()));
        relics.Add(new WarRelic(26, "완성된 보석 건틀렛", 50, "마침내 완성시킨 보석 건틀렛이다. 적의 체력이 절반으로 감소한다.", RelicType.StateBoost, () => CompletedGemGauntlet()));
        relics.Add(new WarRelic(27, "하트 보석 목걸이", 20, "아군 전열 유닛의 체력이 0이 되는 공격을 받을 때 체력의 최대치까지 회복한다. 전투에 한번만 발동한다.", RelicType.BattleActive, () => HeartGemNecklace()));
        relics.Add(new WarRelic(28, "용기의 깃발", 20, "사기로 인한 모든 효과의 적용 기준치가 10 낮아진다.", RelicType.StateBoost, () => FlagOfCourage()));
        relics.Add(new WarRelic(29, "부러진 직검", 0, "아군 유닛의 공격력이 15% 감소한다.", RelicType.StateBoost, () => BrokenStraightSword()));
       
        relics.Add(new WarRelic(30, "깨진 투구", 0, "아군 유닛의 최대 기력이 1 감소한다. (1 미만으로 내려가지 않음)", RelicType.SpecialEffect, () => CrackedHelmet()));
        relics.Add(new WarRelic(31, "해진 군화", 0, "아군 유닛의 기동력이 2 감소한다. (1 미만으로 내려가지 않음)", RelicType.StateBoost, () => WornOutBoots()));
        relics.Add(new WarRelic(32, "갈라진 방패", 0, "아군 유닛의 장갑이 2 감소한다. (1 미만으로 내려가지 않음)", RelicType.StateBoost, () => SplitShield()));
        relics.Add(new WarRelic(33, "황폐한 깃발", 0, "사기의 감소량이 20% 증가한다.", RelicType.SpecialEffect, () => WastedFlag()));
        relics.Add(new WarRelic(34, "생존자의 넝마떼기", 20, "아군 유닛이 사망할때마다 다른 아군 유닛의 공격력이 3% 증가한다. 마지막에 남은 아군 유닛은 공격력이 추가로 30%, 기동력이 4 증가한다.", RelicType.BattleActive, () => SurvivorOfRag()));
        relics.Add(new WarRelic(35, "정복자의 인장", 10, "엘리트 전투 승리 시에 전쟁 유산 보상이 한번 더 등장한다.", RelicType.SpecialEffect, () => ConquerorOfSeal()));
        relics.Add(new WarRelic(36, "맹인전사의 안대", 10, "적의 배치 정보가 모두 숨겨져 알 수 없게 되지만, 아군 유닛의 체력과 공격력이 20%, 아군 궁병의 사거리가 1 증가하고, 부대 상한이 3 증가한다.", RelicType.ActiveState, () => BlindWarriorEyepatch()));
        relics.Add(new WarRelic(37, "덧댐 장갑판", 1, "모든 아군 유닛의 장갑 1 증가.", RelicType.StateBoost, () => ReinforcedArmorPlate()));
        relics.Add(new WarRelic(38, "장식된 단검", 1, "모든 아군 유닛의 공격력 일정배수 10% 증가.", RelicType.StateBoost, () => DecoratedDagger()));
        relics.Add(new WarRelic(39, "전쟁나팔", 1, "보스 전투에 진입할 때 사기가 15 증가한다.", RelicType.StateBoost, () => WarHorn()));
        
        relics.Add(new WarRelic(40, "누군가의 무료 배식권", 1, "상단에 진입할 때 모든 아군 유닛의 기력이 1 회복한다.", RelicType.SpecialEffect, () => FreeMealTicket()));
        relics.Add(new WarRelic(41, "파괴공작용 대포", 1, "일반, 엘리트 전투 시작 전에 적의 체력을 10% 감소시킨다.", RelicType.StateBoost, () => CannonForSabotage()));
        relics.Add(new WarRelic(42, "자율 개발 명령서", 1, "획득 시 무작위의 병종 강화 3종을 1단계 강화시킨다.", RelicType.GetEffect, () => AutonomousDevelopmentOrder()));
        relics.Add(new WarRelic(43, "해진 정찰 보고서", 1, "엘리트, 보스 전투에서 아군이 주는 피해가 15% 증가한다.", RelicType.StateBoost, () => EnemyGeneralScoutReport()));
        relics.Add(new WarRelic(44, "추가 징병 계획서", 1, "용병단의 판매 유닛 슬롯 수가 4 증가한다.", RelicType.SpecialEffect, () => MistakenOrderReceipt()));
        relics.Add(new WarRelic(45, "전술적 단일화 교본", 10, "배치한 유닛의 병종이 한 종류일 경우 유닛의 체력과 공격력이 10% 증가하고, 경갑 유닛의 기동력이 5, 중갑 유닛의 장갑이 5 증가한다.", RelicType.StateBoost, () => TacticalUnificationManual()));
        relics.Add(new WarRelic(46, "기술 비급서", 1, "아군 유닛이 기술로 주는 피해가 20% 증가한다.", RelicType.BattleActive, () => TechnicalManual()));
        relics.Add(new WarRelic(47, "보물지도", 1, "획득 시 다음에 진입한 이벤트 지역이 보물 지역으로 변경된다.", RelicType.GetEffect, () => TreasureMap()));
        relics.Add(new WarRelic(48, "무지개 열쇠", 1, "한 챕터에 두번, 길이 이어지지 않은 지역으로 이동할 수 있다.", RelicType.SpecialEffect, () => RainbowKey()));
        relics.Add(new WarRelic(49, "재상의 보증서", 1, "금화를 소모할 때, 500금화까지 빌려서 사용할 수 있게된다.", RelicType.SpecialEffect, () => CreditAuthorization()));
        
        relics.Add(new WarRelic(50, "순금 나팔", 10, "금화를 소모할 때 소모한 금화 100당 아군 유닛의 공격력이 1% 증가한다. 소모한 금화량이 저장되고 소지한 금화로 취급되어 관련 효과를 적용받을 수 있다.", RelicType.StateBoost, () => GoldenHorn()));
        relics.Add(new WarRelic(51, "두꺼운 전술 교범", 1, "군사 아카데미에서 강화할 수 있는 선택지가 하나 더 추가된다.", RelicType.SpecialEffect, () => ThickTacticalManual()));
        relics.Add(new WarRelic(52, "탐험가의 나침반", 1, "보상으로 전쟁 유산을 선택할 때 선택지를 하나 더 추가한다.", RelicType.SpecialEffect, () => ExplorerCompass()));
        relics.Add(new WarRelic(53, "불운의 황금 동전", 10, "적 유닛이 받는 피해가 50% 증가한다. 단, 이 전쟁 유산을 획득할 때 75% 확률로 랜덤한 다른 전설 전쟁 유산으로 대체된다. ", RelicType.StateBoost, () => GoldenCoinOfUnLuck()));
        relics.Add(new WarRelic(54, "저주 인형", 1, "아군 유닛 사망 시 전열 적 유닛에게 사망한 아군 유닛 체력의 10%만큼 피해를 준다.", RelicType.BattleActive, () => CurseDoll()));
        relics.Add(new WarRelic(55, "혼돈의 주사위", 20, "아군 유닛들의 체력, 공격력이 매 전투마다 최소 60%에서 최대 200%까지 무작위로 분배된다.", RelicType.StateBoost, () => ChaosDice()));
        relics.Add(new WarRelic(56, "광전사의 머리칼", 10, "전투에서 적을 처치한 수 만큼 사기를 1 회복한다. 전투가 없는 지역으로 이동시 사기가 5 감소한다.", RelicType.SpecialEffect, () => LightWarriorHair()));
        relics.Add(new WarRelic(57, "용사의 훈장", 20, "영웅 유닛 최대 보유수 1 증가.", RelicType.SpecialEffect, () => MedalOfBrave()));
        relics.Add(new WarRelic(58, "횡령 증거품", 0, "용병단, 상단, 군사 아카데미의 금화 가격이 20% 증가한다.", RelicType.SpecialEffect, () => EvidenceOfEmbezzlement()));
        relics.Add(new WarRelic(59, "승전보", 0, "보스 전투 승리 시 아군 유닛의 모든 기력이 최대로 회복된다.", RelicType.SpecialEffect, () => VictoryNews()));
        
        relics.Add(new WarRelic(60, "무명의 군단 배지", 10, "영웅 유닛을 보유하지 않을 시 아군 유닛의 체력과 공격력이 20% 증가하고, 장갑이 2 증가한다.", RelicType.StateBoost, () => NamelessLegionBadge()));
        relics.Add(new WarRelic(61, "뜨거운 심장 모형", 1, "아군 유닛의 체력이 10% 증가한다.", RelicType.StateBoost,()=>HotHeartModel()));
        relics.Add(new WarRelic(62, "약자낙인 인두", 0, "아군 유닛의 체력이 15% 감소한다.", RelicType.StateBoost,()=>UnderDogStigma()));
        relics.Add(new WarRelic(63, "매우 진한 스프", 20, "아군 유닛의 체력과 공격력이 10%, 장갑과 기동력이 1 증가한다.", RelicType.StateBoost, () =>VeryThickSoup()));
        relics.Add(new WarRelic(64, "전쟁 군주의 투구", 20, "아군 유닛의 체력이 25% 증가한다.", RelicType.StateBoost, () =>WarlordHelm()));
        relics.Add(new WarRelic(65, "전쟁 군주의 검", 20, "아군 유닛의 공격력이 25% 증가한다.", RelicType.StateBoost, () =>WarlordSword()));
        relics.Add(new WarRelic(66, "확장 진형도", 1, "부대 상한이 1 증가한다.", RelicType.SpecialEffect, () =>ExpandedFormationDiagram()));
        relics.Add(new WarRelic(67, "전쟁 군주의 휘장", 20, "부대 상한이 3 증가한다.", RelicType.SpecialEffect, () =>WarlordInsignia()));
        relics.Add(new WarRelic(68, "전리품 주머니", 1, "약탈로 획득하는 금화가 20 증가하고, 전투 시작 시 약탈을 가진 유닛이 존재할 경우 약탈이 없는 무작위 유닛 둘에게 약탈을 부여한다.", RelicType.BattleActive, () =>LootBag()));
        relics.Add(new WarRelic(69, "전위대의 갑옷", 1, "아군의 첫 전열 유닛에 한해서 장갑이 3 증가한다.", RelicType.StateBoost, () =>VanguardArmor()));
        
        relics.Add(new WarRelic(70, "선봉대 군화", 1, "아군의 첫 전열 유닛에 한해서 기동력이 3 증가한다.", RelicType.StateBoost, () =>VanguardBoots()));
        relics.Add(new WarRelic(71, "녹슨 쇠말뚝", 1, "아군의 모든 공격마다 장갑을 무시하는 고정 피해 5가 추가된다.", RelicType.BattleActive, () =>RustyIronStake()));
        relics.Add(new WarRelic(72, "수상한 부등변다면체", 10, "전투에 아군을 도와주는 영웅 유닛을 무작위로 하나 소환한다. 단, 10% 확률로 적군에 소환된다.", RelicType.StateBoost, () => SuspiciousScalenePolyhedron()));
        relics.Add(new WarRelic(73, "뭐든지 들어있는 상자", 1, "모든 유산 중에 하나를 무작위로 얻는다.", RelicType.SpecialEffect, () =>AnythingBox()));
        relics.Add(new WarRelic(74, "신성한 문서", 1, "'순교' 기술이 다음 순서 유닛의 체력을 이 유닛의 공격력만큼 증가시킨다.", RelicType.BattleActive, () =>SacredDocument()));
        relics.Add(new WarRelic(75, "푯대", 1, "기동력으로 증가하는 회피율이 2배가 된다.", RelicType.StateBoost, () =>Signpost()));
        relics.Add(new WarRelic(76, "경량 갑옷", 1, "경갑 특성을 지닌 유닛의 회피율이 5% 증가한다.", RelicType.StateBoost, () =>LightWeightArmor()));
        relics.Add(new WarRelic(77, "뿔피리", 1, "돌격 피해 배수가 30% 증가한다.", RelicType.BattleActive, () =>Horn()));
        relics.Add(new WarRelic(78, "사리 유산", 50, "중첩된 횟수당 모든 유닛의 체력과 공격력이 1% 증가한다. 중첩은 고승이 적을 처치할 때마다 1, 전투에서 승리할 때마다 3 증가한다.", RelicType.StateBoost, () =>SariHeritage()));
        relics.Add(new WarRelic(79, "기이한 조각", 1, "획득 시 원하는 유닛에게 무한 특성을 부여한다.", RelicType.SpecialEffect, () =>StrangePiece()));
        
        relics.Add(new WarRelic(80, "합금 박차", 1, "내 첫 번째 유닛이 기병이라면 해당 유닛의 체력이 15% 증가하고, 강한 돌격 특성을 얻는다.", RelicType.StateBoost, () =>AlloySpur()));
        relics.Add(new WarRelic(81, "벼려진 마창", 1, "유닛의 공격력이 기동력에 비례해 증가한다.", RelicType.StateBoost, () =>ForgedStable()));
        relics.Add(new WarRelic(82, "반응 갑옷", 1, "유닛의 공격력이 장갑에 비례해 증가한다.", RelicType.StateBoost, () =>ReactiveArmor()));
        relics.Add(new WarRelic(83, "창술 교범", 1, "수비 태세의 패널티가 사라지며, 수비 태세 기술을 가지고 있는 유닛의 체력이 20 증가한다.", RelicType.ActiveState, () =>SpearManual()));
        relics.Add(new WarRelic(84, "치유석", 1, "치유량이 30% 증가한다.", RelicType.BattleActive, () =>HealingStone()));
        relics.Add(new WarRelic(85, "정예병 모집서", 1, "희귀도 1 유닛의 등장 확률이 15% 감소한다.", RelicType.SpecialEffect, () =>EliteRecruitment()));
    }

    public static WarRelic GetRelicById(int id)
    {
        return relics.Find(relic => relic.id == id);
    }

    //할인패 0
    private static void DiscountCard()
    {

    }
    //단단한 모루 1
    private static void SolidAnvil()
    {

    }
    //인내력의 깃발 2
    private static void FlagOfEndurance()
    {

    }
    //추가 보급 명령서 3
    private static void ExtraSupplyOrder()
    {

    }
    //도금 망원경 4
    private static void GoldPlatedTelescope()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in units)
        {
            if (unit.branchIdx == 2)
            {
                unit.range++;
                break;
            }
        }
    }

    //행운의 금화 주머니 5
    private static void LuckyCoinPouch()
    {

    }

    //전설의 도굴꾼의 삽 6
    private static void LegendaryDiggerShovel()
    {

    }

    //고고학 키트 7
    private static void ArchaeologyKit()
    {

    }

    //순금 검 8
    private static void GoldenSword()
    {
        int gold= RogueLikeData.Instance.GetCurrentGold();
        var units = RogueLikeData.Instance.GetMyUnits();
        if (gold > 0)
        {
            units = RogueLikeData.Instance.GetMyUnits();
            foreach (var unit in units)
            {
                unit.attackDamage += MathF.Round(unit.baseAttackDamage*(gold/100));
            }
        }
    }

    //예리한 양날도끼 9
    private static void SharpDoubleAxe()
    {
        RogueLikeData.Instance.AddMyMultipleDamage(0.3f);
        RogueLikeData.Instance.AddEnemyMultipleDamage(0.1f);
    }

    //광기의 깃발 10
    private static void FlagOfMadness()
    {
        RogueLikeData.Instance.AddMyMultipleDamage(0.2f);
        var units = RogueLikeData.Instance.GetMyUnits();

        foreach(var unit in units)
        {
            unit.armor = UnityEngine.Mathf.Max(0, unit.armor - 1);
        }
    }

    // 혼돈의 깃발 11
    private static void FlagOfChaos()
    {
        // RogueLikeData 싱글톤 사용
        var units = RogueLikeData.Instance.GetMyUnits();

        if (units == null || units.Count == 0)
        {
            return;
        }

        // 유닛 리스트 무작위로 섞기
        System.Random random = new();
        for (int i = units.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (units[i], units[j]) = (units[j], units[i]);
        }

        // 유닛 데이터 수정
        foreach (var unit in units)
        {
            unit.maxHealth += MathF.Round(unit.baseHealth*0.3f);
            unit.health = unit.maxHealth; // 체력 증가
            unit.attackDamage +=MathF.Round(unit.baseAttackDamage * 0.3f); // 공격력 증가

            if (unit.rangedAttack) // 원거리 공격 유닛의 경우
            {
                unit.range++; // 사거리 증가
            }
        }

        RogueLikeData.Instance.SetAllMyUnits(units); // 변경된 데이터 저장
    }


    //위대한 지휘관의 훈장 12
    private static void MedalOfGreatCommander()
    {
        //유닛 상한은 미구현
        var units = RogueLikeData.Instance.GetMyUnits();
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
                unit.maxHealth += MathF.Round(unit.baseHealth * 0.2f);
                unit.health = unit.maxHealth;
                unit.attackDamage += MathF.Round(unit.baseAttackDamage * 0.2f);
            }
        }
    }

    //유연성의 부적 13
    private static void AmuletOfFlexibility()
    {

    }

    //가시 갑옷 14
    private static void ReactiveThornArmor()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in units)
        {
            if (unit.thorns)
            {
                unit.armor += 2;
            }
            if(unit.heavyArmor && !unit.thorns)
            {
                unit.thorns = true;
            }
        }
    }

    //수호자의 훈장 15
    private static void MedalOfImperialGuard()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in units)
        {
            if (unit.guard)
            {
                unit.armor+=4;
            }
        }
    }

    //헤르메스 신발 16
    private static void VeryLightMilitaryPants()
    {
        // RogueLikeData 싱글톤 사용
        var units = RogueLikeData.Instance.GetMyUnits();

        foreach (var unit in units)
        {
            if(unit.lightArmor)
            {
                unit.mobility += 8;
            }
        }
    }
    
    //팔랑크스 전술서 17
    private static void PhalanxTacticsBook()
    {
        // RogueLikeData 싱글톤 사용
        var units = RogueLikeData.Instance.GetMyUnits();
        foreach(var unit in units)
        {
            if (unit.branchIdx == 0)
            {
                unit.antiCavalry += Mathf.Round(unit.baseAntiCavalry + 0.5f);
            }
        }
    }

    //정예 기병대 안장 18 맹진 미구현
    private static void EliteCavalrySaddle()
    {
       
    }

    //정예 궁병 부대 깃털모자 19
    private static void EliteArcherFeatherHat()
    {
        // RogueLikeData 싱글톤 사용
        var units = RogueLikeData.Instance.GetMyUnits();

        foreach (var unit in units)
        {
            if (unit.branchIdx == 2)
            {
                unit.attackDamage += MathF.Round(unit.baseAttackDamage * 0.2f);
            }
        }
    }

    //민병대 나팔 20
    private static void MilitiaHorn()
    {
        // RogueLikeData 싱글톤 사용
        var units = RogueLikeData.Instance.GetMyUnits();

        foreach (var unit in units)
        {
            if (unit.rarity == 1)
            {
                unit.maxHealth += MathF.Round(unit.baseHealth * 0.15f);
                unit.health = unit.maxHealth;
                unit.attackDamage += MathF.Round(unit.baseAttackDamage * 0.15f);
            }
        }
    }

    //소름끼치는 구슬 21
    private static void CreepyOrb()
    {
        // RogueLikeData 싱글톤 사용
        int curseCount = RogueLikeData.Instance.GetRelicsByGrade(0).Count;
        if (curseCount == 0) return;

        var units = RogueLikeData.Instance.GetMyUnits();

        foreach (var unit in units)
        {
            unit.maxHealth += MathF.Round(unit.baseHealth * (curseCount * 0.1f));
            unit.health = unit.maxHealth;
            unit.attackDamage += MathF.Round(unit.baseAttackDamage * (curseCount * 0.1f));
        }
    }

    //해주 부적 22
    private static void HaejuAmulet()
    {

    }

    //비어있는 보석 건틀렛 23
    private static void EmptyGemGauntlet()
    {

    }

    //작은 보석 더미 24
    private static void SmallGemPile()
    {

    }

    //커다란 보석 25
    private static void LargeGem()
    {

    }

    //완성된 보석 건틀렛 26
    private static void CompletedGemGauntlet()
    {
        // RogueLikeData 싱글톤 사용
        var units = RogueLikeData.Instance.GetEnemyUnits();

        foreach (var unit in units)
        {
            unit.maxHealth -= MathF.Round(unit.baseHealth * 0.5f);
            unit.health = unit.maxHealth;
        }
    }

    //하트 보석 목걸이 27
    private static void HeartGemNecklace()
    {

    }

    //용기의 깃발 28
    private static void FlagOfCourage()
    {

    }

    //부러진 직검 29
    private static void BrokenStraightSword()
    {
        // RogueLikeData 싱글톤 사용
        var units = RogueLikeData.Instance.GetMyUnits();

        foreach (var unit in units)
        {
            unit.attackDamage -= MathF.Round(unit.baseAttackDamage*0.15f);
        }
    }

    //깨진 투구 30
    private static void CrackedHelmet()
    {

    }

    //해진 군화 31 
    private static void WornOutBoots()
    {
        // RogueLikeData 싱글톤 사용
        var units = RogueLikeData.Instance.GetMyUnits();

        foreach (var unit in units)
        {
            unit.mobility = UnityEngine.Mathf.Max(1, unit.mobility - 2);
        }
    }

    //갈라진 방패 32
    private static void SplitShield()
    {
        // RogueLikeData 싱글톤 사용
        var units = RogueLikeData.Instance.GetMyUnits();

        foreach (var unit in units)
        {
            unit.armor = UnityEngine.Mathf.Max(1, unit.armor - 2);
        }
    }

    //황폐한 깃발 33
    private static void WastedFlag()
    {

    }

    //생존자의 넝마떼기 34
    private static void SurvivorOfRag()
    {

    }

    //정복자의 인장 35
    private static void ConquerorOfSeal()
    {

    }

    //맹인전사의 안대 36
    private static void BlindWarriorEyepatch()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        foreach(var unit in units)
        {
            unit.attackDamage += MathF.Round(unit.baseAttackDamage *0.2f);
            unit.maxHealth += MathF.Round(unit.baseHealth*0.2f);
            unit.health =unit.maxHealth;
            if (unit.rangedAttack)
            {
                unit.range++;
            }
        }
    }

    //덧댐 장갑판 37
    private static void ReinforcedArmorPlate()
    {
        // RogueLikeData 싱글톤 사용
        var units = RogueLikeData.Instance.GetMyUnits();

        foreach (var unit in units)
        {
            unit.armor += 1;
        }
    }

    //장식된 단검 38
    private static void DecoratedDagger()
    {
        // RogueLikeData 싱글톤 사용
        var units = RogueLikeData.Instance.GetMyUnits();

        foreach (var unit in units)
        {
            unit.attackDamage += MathF.Round(unit.baseAttackDamage*0.1f);
        }
    }

    //전쟁나팔 39
    private static void WarHorn()
    {

    }

    //무료 배식권 40
    private static void FreeMealTicket()
    {

    }

    //파괴공작용 대포 41
    private static void CannonForSabotage()
    {
        StageType stage = RogueLikeData.Instance.GetCurrentStageType();
        if(stage == StageType.Combat || stage == StageType.Elite)
        {
            // RogueLikeData 싱글톤 사용
            var units = RogueLikeData.Instance.GetEnemyUnits();

            foreach (var unit in units)
            {
                unit.maxHealth -= MathF.Round(unit.baseHealth*0.1f);
                unit.health = unit.maxHealth;
            }
        }
    }

    // 자율 개발 명령서 42
    private static void AutonomousDevelopmentOrder()
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        var unitTypes = myUnits.Select(u => u.branchIdx)
                               .Distinct()
                               .Where(t => t >= 0 && t < 8)
                               .ToList();

        if (unitTypes.Count == 0) return;

        for (int i = 0; i < unitTypes.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, unitTypes.Count);
            (unitTypes[i], unitTypes[r]) = (unitTypes[r], unitTypes[i]);
        }

        int count = Mathf.Min(3, unitTypes.Count);
        for (int i = 0; i < count; i++)
        {
            int type = unitTypes[i];
            bool isAttack = UnityEngine.Random.value < 0.5f;
            RogueLikeData.Instance.IncreaseUpgrade(type, isAttack, false);
        }
    }

    //해진 정찰 보고서 43
    private static void EnemyGeneralScoutReport()
    {
        StageType currentStage = RogueLikeData.Instance.GetCurrentStageType();
        if(currentStage== StageType.Elite || currentStage == StageType.Boss)
        {
            RogueLikeData.Instance.AddMyMultipleDamage(0.15f);
        }
    }

    //실수한 발주 영수증 44
    private static void MistakenOrderReceipt()
    {

    }

    //전술적 단일화 교본 45
    private static void TacticalUnificationManual()
    {
        // RogueLikeData 싱글톤 사용
        var units = RogueLikeData.Instance.GetEnemyUnits();
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
        foreach (var unit in units)
        {
            unit.attackDamage += MathF.Round(unit.baseAttackDamage * 0.2f);
            if (unit.lightArmor) unit.mobility += 5;
            else unit.armor += 5;
        }
    }

    //기술 비급서 46
    private static void TechnicalManual()
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

    //외상 허가서 49
    private static void CreditAuthorization()
    {

    }

    //순금 나팔 50
    private static void GoldenHorn()
    {

    }

    //두꺼운 전술 교범 51
    private static void ThickTacticalManual()
    {

    }

    //탐험가의 나침반 52
    private static void ExplorerCompass()
    {

    }

    //불운의 황금 동전 53
    private static void GoldenCoinOfUnLuck()
    {
        RogueLikeData.Instance.AddMyMultipleDamage(0.5f);
    }

    //저주 인형 54
    private static void CurseDoll()
    {

    }

    // 혼돈의 주사위 (Relic 55) - 최소 60%, 최대 200% 값으로 설정 55
    private static void ChaosDice()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        System.Random random = new System.Random();

        foreach (var unit in units)
        {
            // 체력과 공격력을 각각 60%~200% 사이의 랜덤 값으로 조정
            float healthMultiplier = (float)random.Next(60, 201) / 100f;   // 60% ~ 200%
            float attackMultiplier = (float)random.Next(60, 201) / 100f;  // 60% ~ 200%

            unit.maxHealth += MathF.Round(unit.baseHealth * (1-healthMultiplier)); // 원래 체력의 60~200% 적용
            unit.health = unit.maxHealth;
            unit.attackDamage += MathF.Round(unit.baseAttackDamage * (1-attackMultiplier)); // 원래 공격력의 60~200% 적용
        }
    }


    //광전사의 머리칼 56
    private static void LightWarriorHair()
    {

    }

    //용사의 훈장 57
    private static void MedalOfBrave()
    {

    }

    //횡령증거품 58
    private static void EvidenceOfEmbezzlement()
    {

    }

    //승전보 59
    private static void VictoryNews()
    {

    }

    //무명의 군단 배지 60
    private static void NamelessLegionBadge()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in units)
        {
            if (unit.branchIdx == 8) return;
            unit.maxHealth += MathF.Round(unit.baseHealth*0.2f);
            unit.health = unit.maxHealth;
            unit.attackDamage += MathF.Round(unit.baseAttackDamage*0.2f);
            unit.armor += 2;
        }
    }

    //뜨거운 심장 모형 61
    private static void HotHeartModel()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in units)
        {
            unit.maxHealth += MathF.Round(unit.baseHealth*0.1f);
            unit.health =unit.maxHealth;
        }
    }
    //약자낙인 인두 62
    private static void UnderDogStigma()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in units)
        {
            unit.maxHealth -= MathF.Round(unit.baseHealth*0.15f);
            unit.health = unit.maxHealth;
        }
    }
    //매우 진한 스프 63
    private static void VeryThickSoup()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in units)
        {
            unit.maxHealth += MathF.Round(unit.baseHealth*0.1f);
            unit.health = unit.maxHealth;
            unit.attackDamage += MathF.Round(unit.baseAttackDamage*0.1f);
            unit.armor += 1;
            unit.mobility += 1;
        }
    }
    //전쟁 군주의 투구 64
    private static void WarlordHelm()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in units)
        {
            unit.maxHealth += MathF.Round(unit.baseHealth*0.25f);
            unit.health = unit.maxHealth;
        }
    }
    //전쟁 군주의 검 65
    private static void WarlordSword()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in units)
        {
            unit.attackDamage += MathF.Round(unit.baseAttackDamage*0.25f);
        }
    }
    //확장 진형도 66
    private static void ExpandedFormationDiagram()
    {

    }
    //전쟁 군주의 휘장 67
    private static void WarlordInsignia()
    {

    }
    //전리품 주머니 68
    private static void LootBag()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        var plunder = units.FirstOrDefault(u=>u.plunder);
        if (plunder == null) return;
        // plunder가 false인 유닛들을 필터링합니다.
        var availableUnits = units.Where(u => !u.plunder).ToList();

        // availableUnits를 랜덤하게 섞은 후, 최대 2개를 선택합니다.
        var selectedUnits = availableUnits.OrderBy(u => random.Next()).Take(2);

        // 선택된 유닛들의 plunder 값을 true로 변경합니다.
        foreach (var sUnit in selectedUnits)
        {
            sUnit.plunder = true;
        }
    }
    //전위대의 갑옷 69
    private static void VanguardArmor()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        units[0].armor += 3;
    }
    //선봉대 군화 70
    private static void VanguardBoots()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        units[0].mobility += 3;
    }
    //녹슨 쇠말뚝 71
    private static void RustyIronStake()
    {

    }
    //수상한 부등변다면체 72
    private static void SuspiciousScalenePolyhedron()
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        var enemyUnits= RogueLikeData.Instance.GetEnemyUnits();
        //영웅 유닛의 인덱스 범위
        int minIdx = 52;
        int maxIdx = 66;

        //영웅 유닛 목록 가져오기
        var allUnits = RogueLikeData.Instance.GetMyUnits();
        var heroUnits = allUnits.FindAll(unit => unit.idx >= minIdx && unit.idx <= maxIdx && unit.health > 0);

        if (heroUnits.Count == 0) return; //영웅 유닛이 없으면 함수 종료

        //랜덤으로 영웅 유닛 하나 선택
        var randomUnit = heroUnits[new System.Random().Next(0, heroUnits.Count)];

        //유닛의 복사본 생성
        RogueUnitDataBase newUnit = new RogueUnitDataBase(
            randomUnit.idx, randomUnit.unitName, randomUnit.unitBranch, randomUnit.branchIdx, randomUnit.unitId,
            randomUnit.unitExplain, randomUnit.unitImg, randomUnit.unitFaction, randomUnit.factionIdx,
            randomUnit.tag, randomUnit.tagIdx, randomUnit.unitPrice, randomUnit.rarity,
            randomUnit.health, randomUnit.armor, randomUnit.attackDamage, randomUnit.mobility,
            randomUnit.range, randomUnit.antiCavalry, randomUnit.energy,
            randomUnit.baseHealth, randomUnit.baseArmor, randomUnit.baseAttackDamage, randomUnit.baseMobility,
            randomUnit.baseRange, randomUnit.baseAntiCavalry, randomUnit.baseEnergy,
            randomUnit.lightArmor, randomUnit.heavyArmor, randomUnit.rangedAttack, randomUnit.bluntWeapon,
            randomUnit.pierce, randomUnit.agility, randomUnit.strongCharge, randomUnit.perfectAccuracy,
            randomUnit.slaughter, randomUnit.bindingForce, randomUnit.bravery, randomUnit.suppression,
            randomUnit.plunder, randomUnit.doubleShot, randomUnit.scorching, randomUnit.thorns,
            randomUnit.endless, randomUnit.impact, randomUnit.healing, randomUnit.lifeDrain,
            randomUnit.charge, randomUnit.defense, randomUnit.throwSpear, randomUnit.guerrilla,
            randomUnit.guard, randomUnit.assassination, randomUnit.drain, randomUnit.overwhelm,
            randomUnit.martyrdom, randomUnit.wounding, randomUnit.vengeance, randomUnit.counter,
            randomUnit.firstStrike, randomUnit.challenge, randomUnit.smokeScreen,
            randomUnit.maxHealth, randomUnit.maxEnergy, true, false, -1, new Dictionary<int, BuffDebuffData>()
        );
        //10% 확률로 enemyUnits에 추가
        if (UnityEngine.Random.value <= 0.1f)
        {
            enemyUnits.Insert(UnityEngine.Random.Range(0, enemyUnits.Count + 1), newUnit);
            RogueLikeData.Instance.SetAllEnemyUnits(enemyUnits);
        }
        else
        {
            //myUnits에 랜덤 위치로 삽입
            myUnits.Insert(UnityEngine.Random.Range(0, myUnits.Count + 1), newUnit);
            RogueLikeData.Instance.SetAllMyUnits(myUnits);
        }
    }

    //뭐든지 들어있는 상자 73
    private static void AnythingBox()
    {

    }
    //신성한 문서 74
    private static void SacredDocument()
    {

    }
    //푯대 75
    private static void Signpost()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in units)
        {
            int id = 5, type = 0, rank = 1, duration = -1;
            unit.effectDictionary[id]=new(id, type, rank,duration);
        }
    }
    //경랑 갑옷 76
    private static void LightWeightArmor()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
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
        var units = RogueLikeData.Instance.GetMyUnits();
        int sariStack = RogueLikeData.Instance.GetSariStack();
        foreach(var unit in units)
        {
            unit.maxHealth += Mathf.Round(unit.baseHealth * 0.01f * sariStack);
            unit.health = unit.maxHealth;
            unit.attackDamage = Mathf.Round(unit.baseAttackDamage *0.01f*sariStack);
        }
    }
    //기이한 조각 79
    private static void StrangePiece()
    {

    }
    //합금 박차 80
    private static void AlloySpur()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        RogueUnitDataBase front =units[0];
        if (front.branchIdx==5 || front.branchIdx == 6)
        {
            front.maxHealth += MathF.Round(front.baseHealth*0.15f);
            front.health =front.maxHealth;
            front.strongCharge =true;
        }
    }
    //벼려진 마창 81
    private static void ForgedStable()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        RogueUnitDataBase front = units[0];
        foreach (var unit in units)
        {
            unit.attackDamage += MathF.Floor((unit.baseMobility * (25 / 9)) - (25 / 9)); 
        }
    }
    //반응 갑옷 82
    private static void ReactiveArmor()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in units)
        {
            unit.attackDamage += MathF.Floor(unit.baseArmor*1.7f);
        }
    }
    //창술 교범 83
    private static void SpearManual()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in units)
        {
            if (unit.defense)
            {
                unit.maxHealth += 20;
                unit.health = unit.maxHealth;
            }
        }
    }
    //치유석 84
    private static void HealingStone()
    {

    }
    //정예병 모집서 85
    private static void EliteRecruitment()
    {

    }

}
