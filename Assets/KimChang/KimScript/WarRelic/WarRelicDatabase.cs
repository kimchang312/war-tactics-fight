using DG.Tweening.Plugins.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using static RogueLikeData;


public static class WarRelicDatabase
{
    public static List<WarRelic> relics = new List<WarRelic>();

    private static Random random = new Random();
    static WarRelicDatabase()
    {
        relics.Add(new WarRelic(0, "할인패", 1, "용병단과 상단의 금화 가격이 20% 감소한다.", RelicType.SpecialEffect, () => DiscountCard()));
        relics.Add(new WarRelic(1, "연구 예산 지원금", 1, "군사 아카데미의 강화 비용이 20% 감소한다.", RelicType.SpecialEffect, () => SolidAnvil()));
        relics.Add(new WarRelic(2, "인내력의 깃발", 1, "기력이 감소할 때마다 25% 확률로 감소하지 않는다.", RelicType.SpecialEffect, () => FlagOfEndurance()));
        relics.Add(new WarRelic(3, "추가 보급 명령서", 1, "병영에서 유닛 훈련에 요구하는 턴이 1 감소한다. (1 미만으로 내려가지 않음)", RelicType.SpecialEffect, () => ExtraSupplyOrder()));
        relics.Add(new WarRelic(4, "도금 망원경", 1, "전투 시작 전, 아군 궁병 중 가장 앞에 있는 궁병에 한해서 사거리가 1 증가한다.", RelicType.StateBoost, () => GoldPlatedTelescope()));
        relics.Add(new WarRelic(5, "행운의 금화 주머니", 1, "얻는 금화량이 15% 증가한다.", RelicType.SpecialEffect, () => LuckyCoinPouch()));
        relics.Add(new WarRelic(6, "전설의 도굴꾼의 삽", 1, "다음에 전쟁 유산 보상을 얻을때 전설 전쟁 유산이 확정으로 등장한다. 보스에선 적용되지 않는다. 전설 전쟁 유산을 얻은 이후 이 유산은 소멸한다.", RelicType.SpecialEffect, () => LegendaryDiggerShovel()));
        relics.Add(new WarRelic(7, "기이한 돋보기", 20, "보물을 열거나, 보스에게 승리할 때마다 리롤을 1회 얻는다.", RelicType.SpecialEffect, () => ArchaeologyKit()));
        relics.Add(new WarRelic(8, "순금 검", 1, "소유 중인 금화 100당 아군 유닛의 공격력이 1% 증가한다.", RelicType.StateBoost, () => GoldenSword()));
        relics.Add(new WarRelic(9, "예리한 양날도끼", 10, "적과 아군이 받는 피해가 각 30% / 10% 증가한다.", RelicType.StateBoost, () => SharpDoubleAxe()));
       
        relics.Add(new WarRelic(10, "광기의 깃발", 10, "적이 받는 피해가 20% 증가하고, 아군 유닛의 장갑이 1 감소한다. (0 미만으로 내려가지 않음)", RelicType.StateBoost, () => FlagOfMadness()));
        relics.Add(new WarRelic(11, "혼돈의 깃발", 10, "아군의 배치가 무작위 순서로 배치된다. 아군 유닛의 체력과 공격력이 30% 증가하고, 아군 궁병의 사거리가 1 증가한다.", RelicType.StateBoost, () => FlagOfChaos()));
        relics.Add(new WarRelic(12, "위대한 지휘관의 훈장", 10, "가진 유닛 중에 이름이 같은 유닛이 중복되지 않는다면, 유닛 상한이 n 증가하며, 아군 궁병의 사거리가 1 증가한다.", RelicType.StateBoost, () => MedalOfGreatCommander()));
        relics.Add(new WarRelic(13, "유연성의 부적", 10, "가진 유닛 병종의 종류가 늘어날 시 아군 유닛의 체력이 n 증가, 공격력이 n 증가한다. 모든 병종의 유닛을 각 둘 이상 소지했다면, 모든 강화의 단계가 1단계 상승한다.", RelicType.StateBoost, () => AmuletOfFlexibility()));
        relics.Add(new WarRelic(14, "가시 갑옷", 10, "중갑을 특성을 가진 아군 유닛에게 가시 특성을 부여한다. 원래 가시를 가진 유닛은 장갑이 2 증가한다.", RelicType.BattleActive, () => ReactiveThornArmor()));
        relics.Add(new WarRelic(15, "수호자의 훈장", 10, "수호 스킬을 가진 아군 유닛의 장갑이 n 증가하고, 공격 받을때 데미지를 둘로 나눠서 받는다.", RelicType.ActiveState, () => MedalOfImperialGuard()));
        relics.Add(new WarRelic(16, "매우 가벼운 군복 바지", 10, "중보병과 기병을 제외한 아군 유닛의 기동력이 8 증가한다.", RelicType.StateBoost, () => VeryLightMilitaryPants()));
        relics.Add(new WarRelic(17, "팔랑크스 전술서", 10, "아군 창병 유닛의 공격력 n 증가, 아군의 모든 유닛이 창병이라면 궁병에게 받는 피해가 n% 감소하고, 아군 유닛의 장갑이 n 증가한다.", RelicType.ActiveState, () => PhalanxTacticsBook()));
        relics.Add(new WarRelic(18, "정예 기병대 안장", 10, "아군 경기병 유닛의 공격력 n 증가, 아군의 모든 유닛이 경기병이라면 아군 유닛의 기동력이 n 증가하고, n번 공격 후 돌격 스킬을 한번 더 사용한다.", RelicType.ActiveState, () => EliteCavalrySaddle()));
        relics.Add(new WarRelic(19, "정예 궁병 부대 깃털모자", 10, "아군 궁병 유닛의 공격력 n 증가, 아군의 모든 유닛이 궁병이라면 사거리가 n 증가하고, 기동력이 n 증가한다.", RelicType.StateBoost, () => EliteArcherFeatherHat()));
       
        relics.Add(new WarRelic(20, "민병대 나팔", 10, "\"민병대\" 유닛의 공격력 n 증가, 아군의 모든 유닛이 \"민병대\" 유닛이라면 아군 유닛의 장갑이 n 증가하고, \"민병대\" 유닛만 수용되는 유닛 상한이 n 증가한다.", RelicType.StateBoost, () => MilitiaHorn()));
        relics.Add(new WarRelic(21, "소름끼치는 구슬", 10, "소지 중인 저주등급 유산 한개마다 아군 유닛의 체력이 n 증가, 공격력이 n 증가한다.", RelicType.StateBoost, () => CreepyOrb()));
        relics.Add(new WarRelic(22, "해주 부적", 20, "저주등급 유산의 해로운 효과를 받지 않게 된다.", RelicType.AllEffect, () => HaejuAmulet()));
        relics.Add(new WarRelic(23, "비어있는 보석 건틀렛", 10, "당장에는 아무런 효과도 없다. 작은 구멍 여러개와 커다란 구멍 하나가 있는 건틀렛이다.", RelicType.AllEffect, () => EmptyGemGauntlet()));
        relics.Add(new WarRelic(24, "작은 보석 더미", 10, "당장에는 아무런 효과도 없다. 작은 보석 여러개의 더미로, 색상이 다양하다.", RelicType.AllEffect, () => SmallGemPile()));
        relics.Add(new WarRelic(25, "커다란 보석", 10, "당장에는 아무런 효과도 없다. 커다랗고 하얀 보석이다.", RelicType.AllEffect, () => LargeGem()));
        relics.Add(new WarRelic(26, "완성된 보석 건틀렛", 50, "마침내 완성시킨 보석 건틀렛이다. 적의 체력이 절반으로 감소한다.", RelicType.StateBoost, () => CompletedGemGauntlet()));
        relics.Add(new WarRelic(27, "하트 보석 목걸이", 20, "아군 전열 유닛의 체력이 0이 되는 공격을 받을 때 체력의 최대치까지 회복한다. 전투에 한번만 발동한다.", RelicType.BattleActive, () => HeartGemNecklace()));
        relics.Add(new WarRelic(28, "용기의 깃발", 20, "사기의 효과 적용 선이 플레이어에게 유리하게 바뀜. (일반 상태를 제외한 모든 효과의 적용 선이 일정 수치 감소.)", RelicType.SpecialEffect, () => HeartGemNecklace()));
        relics.Add(new WarRelic(29, "부러진 직검", 0, "아군이 공격시 낮은 확률로 데미지 일정 수치 감소.", RelicType.StateBoost, () => BrokenStraightSword()));
       
        relics.Add(new WarRelic(30, "깨진 투구", 0, "모든 아군의 최대 기력 1 감소.", RelicType.SpecialEffect, () => CrackedHelmet()));
        relics.Add(new WarRelic(31, "해진 군화", 0, "기동력 1 감소. (1 미만으로 내려가지 않음)", RelicType.StateBoost, () => WornOutBoots()));
        relics.Add(new WarRelic(32, "갈라진 방패", 0, "아군 유닛의 장갑이 2 감소한다. (1 미만으로 내려가지 않음)", RelicType.BattleActive, () => SplitShield()));
        relics.Add(new WarRelic(33, "황폐한 깃발", 0, "사기가 감소할 때 일정 수치 더 감소함.", RelicType.SpecialEffect, () => WastedFlag()));
        relics.Add(new WarRelic(34, "생존자의 넝마떼기", 20, "아군이 사망할수록 다른 아군 공격력 일정수치 증가, 마지막에 남은 아군의 공격력, 장갑, 기동성 일정수치 증가.", RelicType.BattleActive, () => Relic35()));
        relics.Add(new WarRelic(35, "정복자의 인장", 10, "엘리트 스테이지 승리 시 전쟁 유산 보상이 한번 더 등장함.", RelicType.SpecialEffect, () => Relic36()));
        relics.Add(new WarRelic(36, "맹인전사의 안대", 10, "적의 배치 정보가 모두 숨겨져 알 수 없게 되지만, 아군 유닛의 체력과 공격력이 20%, 아군 궁병의 사거리가 1 증가하고, 부대 상한이 3 증가한다.", RelicType.ActiveState, () => BlindWarriorEyepatch()));
        relics.Add(new WarRelic(37, "덧댐 장갑판", 1, "모든 아군 유닛의 장갑 1 증가.", RelicType.StateBoost, () => ReinforcedArmorPlate()));
        relics.Add(new WarRelic(38, "장식된 단검", 1, "모든 아군 유닛의 공격력 일정배수 10% 증가.", RelicType.StateBoost, () => Relic39()));
        relics.Add(new WarRelic(39, "전쟁나팔", 1, "보스 스테이지 진입 시 사기 일정수치 증가.", RelicType.SpecialEffect, () => WarHorn()));
        
        relics.Add(new WarRelic(40, "누군가의 무료 배식권", 1, "상단에 진입할 때 모든 아군 유닛의 기력이 1 회복한다.", RelicType.SpecialEffect, () => FreeMealTicket()));
        relics.Add(new WarRelic(41, "파괴공작용 대포", 1, "일반, 엘리트 전투 시작 시 모든 적의 체력이 일정 배수(약 10%) 깎이고 시작.", RelicType.StateBoost, () => Relic42()));
        relics.Add(new WarRelic(42, "자율 개발 명령서", 1, "이 전쟁 유산 획득 후 즉시 랜덤한 아군 일정인원(2~3) 강화.", RelicType.SpecialEffect, () => AutonomousDevelopmentOrder()));
        relics.Add(new WarRelic(43, "해진 정찰 보고서", 1, "엘리트 및 보스 스테이지에서 주는 피해 일정배수 증가.", RelicType.StateBoost, () => EnemyGeneralScoutReport()));
        relics.Add(new WarRelic(44, "추가 징병 계획서", 1, "용병단의 판매 유닛 슬롯 수가 4 증가한다.", RelicType.SpecialEffect, () => MistakenOrderReceipt()));
        relics.Add(new WarRelic(45, "전술적 단일화 교본", 10, "유닛을 병종 한 종류로만 배치 시 아군 공격력 일정수치 증가.", RelicType.StateBoost, () => Relic46()));
        relics.Add(new WarRelic(46, "기술 비급서", 1, "기술로 주는 피해가 일정배수 증가.", RelicType.BattleActive, () => TechnicalManual()));
        relics.Add(new WarRelic(47, "보물지도", 1, "이 전쟁 유산 획득 후 다음에 진입한 이벤트 스테이지가 보물 스테이지로 변경되고 이 전쟁 유산이 소멸함.", RelicType.SpecialEffect, () => TreasureMap()));
        relics.Add(new WarRelic(48, "무지개 열쇠", 1, "이 전쟁 유산 획득 후 다음 3회 이동하는 동안 맵에서 길이 이어지지 않은 스테이지로 이동 가능. 3회 이동 후 소멸.", RelicType.SpecialEffect, () => RainbowKey()));
        relics.Add(new WarRelic(49, "재상의 보증서", 1, "금화를 소모할 때, 500금화까지 빌려서 사용할 수 있게된다.", RelicType.SpecialEffect, () => CreditAuthorization()));
        
        relics.Add(new WarRelic(50, "순금 나팔", 10, "금화 소모 시 소모량에 비례해 공격력 증가, 이 전쟁 유산을 획득한 이후 소모한 금화의 양이 소지한 금화 양에 비례하는 효과에 같이 적용됨.", RelicType.StateBoost, () => GoldenHorn()));
        relics.Add(new WarRelic(51, "두꺼운 전술 교범", 1, "군사 아카데미에서 강화할 수 있는 선택지가 하나 더 추가된다.", RelicType.SpecialEffect, () => Relic52()));
        relics.Add(new WarRelic(52, "탐험가의 나침반", 1, "전쟁 유산 보상의 선택지 한개 증가.", RelicType.SpecialEffect, () => Relic53()));
        relics.Add(new WarRelic(53, "불운의 황금 동전", 10, "주는 모든 피해가 2배로 적용됨. 단, 이 전쟁 유산을 획득할 때 높은 확률로 랜덤한 다른 전설 전쟁 유산으로 대체됨.", RelicType.StateBoost, () => Relic54()));
        relics.Add(new WarRelic(54, "저주 인형", 1, "아군 유닛 사망 시 전열 적 유닛에게 피해.", RelicType.BattleActive, () => Relic55()));
        relics.Add(new WarRelic(55, "혼돈의 주사위", 20, "아군 유닛들의 체력, 공격력이 매 전투마다 최소 60%에서 최대 200%까지 무작위로 분배된다.", RelicType.StateBoost, () => Relic56()));
        relics.Add(new WarRelic(56, "광전사의 머리칼", 10, "(1) 전투 승리 시 사기를 (상대한 적 유닛 수 * 1) 증가시킴.  (2) 비전투 레벨로 이동 시 사기 5 감소.", RelicType.BattleActive, () => Relic57()));
        relics.Add(new WarRelic(57, "용사의 훈장", 20, "영웅 유닛 최대 보유수 1 증가.", RelicType.SpecialEffect, () => Relic58()));
        relics.Add(new WarRelic(58, "횡령 증거품", 0, "용병단, 상단, 군사 아카데미의 금화 가격이 20% 증가한다.", RelicType.SpecialEffect, () => Relic59()));
        relics.Add(new WarRelic(59, "승전보", 0, "군사 아카데미 리롤 기회 -1.", RelicType.SpecialEffect, () => Relic60()));
        relics.Add(new WarRelic(60, "무명의 군단 배지", 10, "영웅 유닛을 보유하지 않을 시 아군 유닛의 체력과 공격력이 20% 증가하고, 장갑이 2 증가한다.", RelicType.SpecialEffect, () => NamelessLegionBadge()));
        relics.Add(new WarRelic(61, "뜨거운 심장 모형", 1, "아군 유닛의 체력이 10% 증가한다.", RelicType.StateBoost,()=>HotHeartModel()));
        relics.Add(new WarRelic(62, "약자낙인 인두", 0, "아군 유닛의 체력이 15% 감소한다.", RelicType.StateBoost,()=>UnderDogStigma()));
        relics.Add(new WarRelic(63, "매우 진한 스프", 20, "아군 유닛의 체력과 공격력이 10%, 장갑과 기동력이 1 증가한다.", RelicType.StateBoost, () =>VeryThickSoup()));
        relics.Add(new WarRelic(64, "전쟁 군주의 투구", 20, "아군 유닛의 체력이 25% 증가한다.", RelicType.StateBoost, () =>WarlordHelm()));
        relics.Add(new WarRelic(65, "전쟁 군주의 검", 20, "아군 유닛의 공격력이 25% 증가한다.", RelicType.StateBoost, () =>WarlordSword()));
        relics.Add(new WarRelic(66, "확장 진형도", 1, "부대 상한이 1 증가한다.", RelicType.SpecialEffect, () =>ExpandedFormationDiagram()));
        relics.Add(new WarRelic(67, "전쟁 군주의 휘장", 20, "부대 상한이 3 증가한다.", RelicType.SpecialEffect, () =>WarlordInsignia()));
        relics.Add(new WarRelic(68, "전리품 주머니", 1, "약탈로 획득하는 금화가 20 증가하고, 전투 시작 시 약탈이 없는 무작위 유닛 둘에게 약탈을 부여한다.", RelicType.BattleActive, () =>LootBag()));
        relics.Add(new WarRelic(69, "전위대의 갑옷", 1, "아군의 첫 전열 유닛에 한해서 장갑이 3 증가한다.", RelicType.StateBoost, () =>VanguardArmor()));
        relics.Add(new WarRelic(70, "선봉대 군화", 1, "아군의 첫 전열 유닛에 한해서 기동력이 3 증가한다.", RelicType.StateBoost, () =>VanguardBoots()));
        relics.Add(new WarRelic(71, "녹슨 쇠말뚝", 1, "아군의 모든 공격마다 장갑을 무시하는 고정 피해 5가 추가된다.", RelicType.BattleActive, () =>RustyIronStake()));
        relics.Add(new WarRelic(72, "수상한 부등변다면체", 10, "전투에 아군을 도와주는 영웅 유닛을 무작위로 하나 소환한다. 단, 10% 확률로 적군에 소환된다.", RelicType.StateBoost, () => SuspiciousScalenePolyhedron()));
        relics.Add(new WarRelic(73, "뭐든지 들어있는 상자", 1, "모든 유산 중에 하나를 무작위로 얻는다.", RelicType.SpecialEffect, () =>AnythingBox()));
        relics.Add(new WarRelic(74, "신성한 문서", 1, "'순교' 기술이 다음 순서 유닛의 체력을 이 유닛의 공격력만큼 증가시킨다.", RelicType.BattleActive, () =>SacredDocument()));
        relics.Add(new WarRelic(75, "푯대", 1, "기동력으로 증가하는 회피율이 2배가 된다.", RelicType.BattleActive, () =>Signpost()));
        relics.Add(new WarRelic(76, "경량 갑옷", 1, "경갑 특성을 지닌 유닛의 회피율이 5% 증가한다.", RelicType.BattleActive, () =>LightWeightArmor()));
        relics.Add(new WarRelic(77, "뿔피리", 1, "돌격 피해 배수가 30% 증가한다.", RelicType.BattleActive, () =>Horn()));
        relics.Add(new WarRelic(78, "사리 유산", 50, "중첩된 횟수당 모든 유닛의 체력과 공격력이 1% 증가한다. 중첩은 고승이 적을 처치할 때마다 1, 전투에서 승리할 때마다 3 증가한다.", RelicType.StateBoost, () =>SariHeritage()));
        relics.Add(new WarRelic(79, "기이한 조각", 1, "획득 시 원하는 유닛에게 무한 특성을 부여한다.", RelicType.SpecialEffect, () =>StrangePiece()));
        relics.Add(new WarRelic(80, "합금 박차", 1, "내 첫 번째 유닛이 기병이라면 해당 유닛의 체력이 15% 증가하고, 강한 돌격 특성을 얻는다.", RelicType.StateBoost, () =>AlloySpur()));
        relics.Add(new WarRelic(81, "벼려진 마창", 1, "유닛의 공격력이 기동력에 비례해 증가한다.", RelicType.StateBoost, () =>ForgedStable()));
        relics.Add(new WarRelic(82, "반응 갑옷", 1, "유닛의 공격력이 장갑에 비례해 증가한다.", RelicType.StateBoost, () =>ReactiveArmor()));
        relics.Add(new WarRelic(83, "창술 교범", 1, "수비 태세의 패널티가 사라지며, 수비 태세 기술을 가지고 있는 유닛의 체력이 20 증가한다.", RelicType.StateBoost, () =>SpearManual()));
        relics.Add(new WarRelic(84, "치유석", 1, "치유량이 30% 증가한다.", RelicType.BattleActive, () =>HealingStone()));
        relics.Add(new WarRelic(85, "정예병 모집서", 1, "희귀도 1 유닛의 등장 확률이 15% 감소한다.", RelicType.SpecialEffect, () =>EliteRecruitment()));
    }

    public static WarRelic GetRelicById(int id)
    {
        return relics.Find(relic => relic.id == id);
    }

    //할인패
    private static void DiscountCard()
    {

    }

    //단단한 모루
    private static void SolidAnvil()
    {

    }

    //인내력의 깃발
    private static void FlagOfEndurance()
    {

    }

    //추가 보급 명령서
    private static void ExtraSupplyOrder()
    {

    }

    //도금 망원경
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
        RogueLikeData.Instance.AllMyUnits(units); // 변경된 데이터 저장
    }

    //행운의 금화 주머니
    private static void LuckyCoinPouch()
    {

    }

    //전설의 도굴꾼의 삽
    private static void LegendaryDiggerShovel()
    {

    }

    //고고학 키트
    private static void ArchaeologyKit()
    {

    }

    //순금 검
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
        RogueLikeData.Instance.AllMyUnits(units); // 변경된 데이터 저장
    }

    //예리한 양날도끼
    private static void SharpDoubleAxe()
    {
        RogueLikeData.Instance.AddMyMultipleDamage(0.3f);
        RogueLikeData.Instance.AddEnemyMultipleDamage(0.1f);
    }

    //광기의 깃발
    private static void FlagOfMadness()
    {
        RogueLikeData.Instance.AddMyMultipleDamage(0.2f);
        var units = RogueLikeData.Instance.GetMyUnits();

        foreach(var unit in units)
        {
            unit.armor = UnityEngine.Mathf.Max(0, unit.armor - 1);
        }
        RogueLikeData.Instance.AllMyUnits(units); // 변경된 데이터 저장
    }

    // 혼돈의 깃발
    private static void FlagOfChaos()
    {
        // RogueLikeData 싱글톤 사용
        var units = RogueLikeData.Instance.GetMyUnits();

        if (units == null || units.Count == 0)
        {
            return;
        }

        // 유닛 리스트 무작위로 섞기
        var random = new Random();
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

        RogueLikeData.Instance.AllMyUnits(units); // 변경된 데이터 저장
    }


    //위대한 지휘관의 훈장
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
            if (unit.rangedAttack)
            {
                unit.range++;
            }
        }
        if (!hasDuplicate)
        {
            RogueLikeData.Instance.AllMyUnits(units);
        }
    }

    //유연성의 부적
    private static void AmuletOfFlexibility()
    {

    }

    //가시 갑옷
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
        RogueLikeData.Instance.AllMyUnits(units); // 변경된 데이터 저장
    }

    //수호자의 훈장
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
        RogueLikeData.Instance.AllMyUnits(units); // 변경된 데이터 저장
    }

    //매우 가벼운 군복 바지
    private static void VeryLightMilitaryPants()
    {
        // RogueLikeData 싱글톤 사용
        var units = RogueLikeData.Instance.GetMyUnits();

        foreach (var unit in units)
        {
            if(unit.branchIdx !=3 || unit.branchIdx != 5 || unit.branchIdx != 6)
            {
                unit.mobility += 8;
            }
        }
        
        RogueLikeData.Instance.AllMyUnits(units); // 변경된 데이터 저장
    }
    
    //팔랑크스 전술서
    private static void PhalanxTacticsBook()
    {
        // RogueLikeData 싱글톤 사용
        var units = RogueLikeData.Instance.GetMyUnits();

        bool isAllSame=true;
        foreach (var unit in units)
        {
            if(unit.branchIdx == 0)
            {
                unit.attackDamage += MathF.Round(unit.baseAttackDamage * 1.2f);
            }
            else
            {
                isAllSame=false;
            }

        }
        if (isAllSame)
        {
            WarRelic relic = RogueLikeData.Instance.GetOwnedRelicById(18);
            WarRelic updateRelic = new WarRelic(
                relic.id,
                relic.name,
                relic.grade,
                relic.tooltip,
                relic.type,
                relic.executeAction,
                relic.used = true
                );
            RogueLikeData.Instance.UpdateRelic(18, updateRelic);
            foreach (var unit in units)
            {
                unit.armor += 2;
            }
        }

        RogueLikeData.Instance.AllMyUnits(units); // 변경된 데이터 저장
    }

    //정예 기병대 안장  절반 구현
    private static void EliteCavalrySaddle()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        bool isAllSame=true;

        foreach (var unit in units)
        {
            if (unit.branchIdx == 5 || unit.branchIdx==6)
            {
                unit.attackDamage += MathF.Round(unit.baseAttackDamage * 1.2f);
            }
            else
            {
                isAllSame = false;
            }
        }
        if (isAllSame)
        {
            foreach(var unit in units)
            {
                unit.mobility += 3;
            }
        }

        RogueLikeData.Instance.AllMyUnits(units); // 변경된 데이터 저장
    }

    //정예 궁병 부대 깃털모자
    private static void EliteArcherFeatherHat()
    {
        // RogueLikeData 싱글톤 사용
        var units = RogueLikeData.Instance.GetMyUnits();
        bool isAllSame= true;

        foreach (var unit in units)
        {
            if (unit.branchIdx == 2)
            {
                unit.attackDamage += MathF.Round(unit.baseAttackDamage * 1.2f);
            }
            else
            {
                isAllSame=false;
            }
        }
        if (isAllSame)
        {
            foreach(var unit in units)
            {
                unit.range += 2;
                unit.mobility += 4;
            }
        }

        RogueLikeData.Instance.AllMyUnits(units); // 변경된 데이터 저장
    }

    //민병대 나팔
    private static void MilitiaHorn()
    {

    }

    //소름끼치는 구슬 
    private static void CreepyOrb()
    {
        // RogueLikeData 싱글톤 사용
        var curseRelic = RogueLikeData.Instance.GetRelicsByGrade(0);
        int curseCount = curseRelic.Count;
        if (curseCount > 0)
        {
            var units = RogueLikeData.Instance.GetMyUnits();

            foreach (var unit in units)
            {
                unit.maxHealth +=  MathF.Round(unit.baseHealth * (curseCount * 0.1f));
                unit.health = unit.maxHealth;
                unit.attackDamage += MathF.Round(unit.baseAttackDamage * (curseCount * 0.1f));
            }
            RogueLikeData.Instance.AllMyUnits(units); // 변경된 데이터 저장
        }
        return;
    }

    //해주 부적
    private static void HaejuAmulet()
    {

    }

    //비어있는 보석 건틀렛
    private static void EmptyGemGauntlet()
    {

    }

    //작은 보석 더미
    private static void SmallGemPile()
    {

    }

    //커다란 보석
    private static void LargeGem()
    {

    }

    //완성된 보석 건틀렛
    private static void CompletedGemGauntlet()
    {
        // RogueLikeData 싱글톤 사용
        var units = RogueLikeData.Instance.GetEnemyUnits();

        foreach (var unit in units)
        {
            unit.maxHealth -= MathF.Round(unit.baseHealth * 0.5f);
            unit.health = unit.maxHealth;
        }

        RogueLikeData.Instance.AllEnemyUnits(units); // 변경된 데이터 저장
    }

    //하트 보석 목걸이
    private static void HeartGemNecklace()
    {

    }

    //용기의 깃발
    private static void FlagOfCourage()
    {

    }

    //부러진 직검
    private static void BrokenStraightSword()
    {
        // RogueLikeData 싱글톤 사용
        var units = RogueLikeData.Instance.GetMyUnits();

        foreach (var unit in units)
        {
            unit.attackDamage -= MathF.Round(unit.baseAttackDamage*0.15f);
        }

        RogueLikeData.Instance.AllMyUnits(units); // 변경된 데이터 저장
    }

    //깨진 투구
    private static void CrackedHelmet()
    {

    }

    //해진 군화
    private static void WornOutBoots()
    {
        // RogueLikeData 싱글톤 사용
        var units = RogueLikeData.Instance.GetMyUnits();

        foreach (var unit in units)
        {
            unit.mobility = UnityEngine.Mathf.Max(1, unit.mobility - 2);
        }

        RogueLikeData.Instance.AllMyUnits(units); // 변경된 데이터 저장
    }

    //갈라진 방패
    private static void SplitShield()
    {
        // RogueLikeData 싱글톤 사용
        var units = RogueLikeData.Instance.GetMyUnits();

        foreach (var unit in units)
        {
            unit.armor = UnityEngine.Mathf.Max(1, unit.armor - 2);
        }

        RogueLikeData.Instance.AllMyUnits(units); // 변경된 데이터 저장
    }

    //황폐한 깃발
    private static void WastedFlag()
    {

    }

    //35
    private static void Relic35()
    {

    }

    //36
    private static void Relic36()
    {

    }

    //맹인전사의 안대
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
        RogueLikeData.Instance.AllMyUnits(units); // 변경된 데이터 저장
    }

    //덧댐 장갑판
    private static void ReinforcedArmorPlate()
    {
        // RogueLikeData 싱글톤 사용
        var units = RogueLikeData.Instance.GetMyUnits();

        foreach (var unit in units)
        {
            unit.armor += 1;
        }

        RogueLikeData.Instance.AllMyUnits(units); // 변경된 데이터 저장
    }

    //장식된 단검
    private static void Relic39()
    {
        // RogueLikeData 싱글톤 사용
        var units = RogueLikeData.Instance.GetMyUnits();

        foreach (var unit in units)
        {
            unit.attackDamage += MathF.Round(unit.baseAttackDamage*0.1f);
        }

        RogueLikeData.Instance.AllMyUnits(units); // 변경된 데이터 저장
    }

    //전쟁나팔
    private static void WarHorn()
    {

    }

    //무료 배식권
    private static void FreeMealTicket()
    {

    }

    //파괴공작용 대포
    private static void Relic42()
    {
        var stage = RogueLikeData.Instance.GetCurrentStage();
        if(stage.type == RogueLikeData.StageType.Battle || stage.type == RogueLikeData.StageType.Elite)
        {
            // RogueLikeData 싱글톤 사용
            var units = RogueLikeData.Instance.GetEnemyUnits();

            foreach (var unit in units)
            {
                unit.maxHealth -= MathF.Round(unit.baseHealth*0.1f);
                unit.health = unit.maxHealth;
            }

            RogueLikeData.Instance.AllMyUnits(units); // 변경된 데이터 저장
        }
    }

    //자율 개발 명령서
    private static void AutonomousDevelopmentOrder()
    {

    }

    //해진 정찰 보고서
    private static void EnemyGeneralScoutReport()
    {
        var currentStage = RogueLikeData.Instance.GetCurrentStage();
        if(currentStage.type== StageType.Elite || currentStage.type == StageType.Boss)
        {
            RogueLikeData.Instance.AddMyMultipleDamage(0.15f);
        }
    }

    //실수한 발주 영수증
    private static void MistakenOrderReceipt()
    {

    }

    //전술적 단일화 교본
    private static void Relic46()
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
            unit.attackDamage += MathF.Round(unit.baseAttackDamage*0.2f);

        }
        if (allCorret)
        {
            RogueLikeData.Instance.AllMyUnits(units); // 변경된 데이터 저장
        }
    }

    //기술 비급서
    private static void TechnicalManual()
    {

    }

    //보물지도
    private static void TreasureMap()
    {

    }

    //무지개 열쇠
    private static void RainbowKey()
    {

    }

    //외상 허가서
    private static void CreditAuthorization()
    {

    }

    //순금 나팔
    private static void GoldenHorn()
    {

    }

    //51
    private static void Relic52()
    {

    }

    //불운의 황금 동전
    private static void Relic53()
    {

    }

    //53
    private static void Relic54()
    {
        RogueLikeData.Instance.AddMyMultipleDamage(1);
    }

    //54
    private static void Relic55()
    {

    }

    // 혼돈의 주사위 (Relic 55) - 최소 60%, 최대 200% 값으로 설정
    private static void Relic56()
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

        // 변경된 유닛 데이터를 저장
        RogueLikeData.Instance.AllMyUnits(units);
    }


    //56
    private static void Relic57()
    {

    }

    //57
    private static void Relic58()
    {

    }

    //58
    private static void Relic59()
    {

    }

    //59
    private static void Relic60()
    {

    }

    //60
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
        RogueLikeData.Instance.AllMyUnits(units);
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
        RogueLikeData.Instance.AllMyUnits(units);
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
        RogueLikeData.Instance.AllMyUnits(units);
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
        RogueLikeData.Instance.AllMyUnits(units);
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
        RogueLikeData.Instance.AllMyUnits(units);
    }
    //전쟁 군주의 검 65
    private static void WarlordSword()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in units)
        {
            unit.attackDamage += MathF.Round(unit.baseAttackDamage*0.25f);
        }
        RogueLikeData.Instance.AllMyUnits(units);
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
        foreach (var unit in units)
        {
            if(unit.plunder)
            {
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
        }
        RogueLikeData.Instance.AllMyUnits(units);
    }
    //전위대의 갑옷 69
    private static void VanguardArmor()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        units[0].armor += 3;
        RogueLikeData.Instance.AllMyUnits(units);
    }
    //선봉대 군화 70
    private static void VanguardBoots()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        units[0].mobility += 3;
        RogueLikeData.Instance.AllMyUnits(units);
    }
    //녹슨 쇠말뚝 71
    private static void RustyIronStake()
    {

    }
    //수상한 부등변다면체 72
    private static void SuspiciousScalenePolyhedron()
    {

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
        RogueLikeData.Instance.AllMyUnits(units);
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
        RogueLikeData.Instance.AllMyUnits(units);
    }
    //뿔피리 77
    private static void Horn()
    {

    }
    //사리 유산 78
    private static void SariHeritage()
    {

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
        RogueLikeData.Instance.AllMyUnits(units);
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
        RogueLikeData.Instance.AllMyUnits(units);
    }
    //반응 갑옷 82
    private static void ReactiveArmor()
    {
        var units = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in units)
        {
            unit.attackDamage += MathF.Floor(unit.baseArmor*1.7f);
        }
        RogueLikeData.Instance.AllMyUnits(units);
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
        RogueLikeData.Instance.AllMyUnits(units);
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
