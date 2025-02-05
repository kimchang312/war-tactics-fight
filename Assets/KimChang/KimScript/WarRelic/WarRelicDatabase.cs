using System;
using System.Collections.Generic;
using static RogueLikeData;


public static class WarRelicDatabase
{
    public static List<WarRelic> relics = new List<WarRelic>();

    static WarRelicDatabase()
    {
        relics.Add(new WarRelic(1, "할인패", 1, "병영의 모든 가격 일정 비율 감소.", RelicType.SpecialEffect, () => DiscountCard()));
        relics.Add(new WarRelic(2, "단단한 모루", 1, "군사 아카데미의 강화 비용 일정 비율 감소.", RelicType.SpecialEffect, () => SolidAnvil()));
        relics.Add(new WarRelic(3, "인내력의 깃발", 1, "기력이 감소할 때마다 일정 확률로 감소하지 않는다.", RelicType.SpecialEffect, () => FlagOfEndurance()));
        relics.Add(new WarRelic(4, "추가 보급 명령서", 1, "병영과 군사 아카데미에서 무료 새로고침 기회를 한번 얻는다.", RelicType.SpecialEffect, () => ExtraSupplyOrder()));
        relics.Add(new WarRelic(5, "도금 망원경", 1, "지도에 보이는 스테이지 정보 1칸 증가.", RelicType.SpecialEffect, () => GoldPlatedTelescope()));
        relics.Add(new WarRelic(6, "행운의 금화 주머니", 1, "얻는 금화량이 일정 비율 증가한다.", RelicType.SpecialEffect, () => LuckyCoinPouch()));
        relics.Add(new WarRelic(7, "전설의 도굴꾼의 삽", 1, "다음의 일반등급 유산 보상이 전설등급 유산 보상으로 변경된다.", RelicType.SpecialEffect, () => LegendaryDiggerShovel()));
        relics.Add(new WarRelic(8, "고고학 키트", 1, "일반등급 유산 보상을 누적으로 3번 새로고침 할 수 있게 해준다.", RelicType.SpecialEffect, () => ArchaeologyKit()));
        relics.Add(new WarRelic(9, "순금 검", 1, "소지 중인 금화량에 비례해 아군 유닛의 공격력이 증가한다.", RelicType.StateBoost, () => GoldenSword()));
        relics.Add(new WarRelic(10, "예리한 양날도끼", 10, "적과 아군이 받는 피해가 각각 n% / n% 증가한다.", RelicType.StateBoost, () => SharpDoubleAxe()));
       
        relics.Add(new WarRelic(11, "광기의 깃발", 10, "적이 받는 피해가 n% 증가하고, 아군 유닛의 장갑이 2 감소한다. (0 미만으로 내려가지 않음)", RelicType.StateBoost, () => FlagOfMadness()));
        relics.Add(new WarRelic(12, "혼돈의 깃발", 10, "아군의 배치가 무작위 순서로 배치된다. 아군 유닛의 체력이 n 증가, 공격력이 n 증가하고, 아군 궁병의 사거리가 1 증가한다.", RelicType.StateBoost, () => FlagOfChaos()));
        relics.Add(new WarRelic(13, "위대한 지휘관의 훈장", 10, "가진 유닛 중에 이름이 같은 유닛이 중복되지 않는다면, 유닛 상한이 n 증가하며, 아군 궁병의 사거리가 1 증가한다.", RelicType.StateBoost, () => MedalOfGreatCommander()));
        relics.Add(new WarRelic(14, "유연성의 부적", 10, "가진 유닛 병종의 종류가 늘어날 시 아군 유닛의 체력이 n 증가, 공격력이 n 증가한다. 모든 병종의 유닛을 각 둘 이상 소지했다면, 모든 강화의 단계가 1단계 상승한다.", RelicType.StateBoost, () => AmuletOfFlexibility()));
        relics.Add(new WarRelic(15, "반응성의 가시 갑옷", 10, "중갑을 가진 아군 유닛이 전열에서 피해를 입을 때 적 전열의 유닛에게 자신의 장갑의 n배 데미지로 반사한다.", RelicType.BattleActive, () => ReactiveThornArmor()));
        relics.Add(new WarRelic(16, "수호자의 훈장", 10, "수호 스킬을 가진 아군 유닛의 장갑이 n 증가하고, 공격 받을때 데미지를 둘로 나눠서 받는다.", RelicType.ActiveState, () => MedalOfImperialGuard()));
        relics.Add(new WarRelic(17, "매우 가벼운 군복 바지", 10, "중보병과 기병을 제외한 아군 유닛의 기동력이 n 증가한다.", RelicType.StateBoost, () => VeryLightMilitaryPants()));
        relics.Add(new WarRelic(18, "팔랑크스 전술서", 10, "아군 창병 유닛의 공격력 n 증가, 아군의 모든 유닛이 창병이라면 궁병에게 받는 피해가 n% 감소하고, 아군 유닛의 장갑이 n 증가한다.", RelicType.ActiveState, () => PhalanxTacticsBook()));
        relics.Add(new WarRelic(19, "정예 기병대 안장", 10, "아군 경기병 유닛의 공격력 n 증가, 아군의 모든 유닛이 경기병이라면 아군 유닛의 기동력이 n 증가하고, n번 공격 후 돌격 스킬을 한번 더 사용한다.", RelicType.ActiveState, () => EliteCavalrySaddle()));
        relics.Add(new WarRelic(20, "정예 궁병 부대 깃털모자", 10, "아군 궁병 유닛의 공격력 n 증가, 아군의 모든 유닛이 궁병이라면 사거리가 n 증가하고, 기동력이 n 증가한다.", RelicType.StateBoost, () => EliteArcherFeatherHat()));
       
        relics.Add(new WarRelic(21, "민병대 나팔", 10, "\"민병대\" 유닛의 공격력 n 증가, 아군의 모든 유닛이 \"민병대\" 유닛이라면 아군 유닛의 장갑이 n 증가하고, \"민병대\" 유닛만 수용되는 유닛 상한이 n 증가한다.", RelicType.StateBoost, () => MilitiaHorn()));
        relics.Add(new WarRelic(22, "소름끼치는 구슬", 10, "소지 중인 저주등급 유산 한개마다 아군 유닛의 체력이 n 증가, 공격력이 n 증가한다.", RelicType.StateBoost, () => CreepyOrb()));
        relics.Add(new WarRelic(23, "해주 부적", 10, "저주등급 유산의 해로운 효과를 받지 않게 된다.", RelicType.AllEffect, () => HaejuAmulet()));
        relics.Add(new WarRelic(24, "비어있는 보석 건틀렛", 10, "당장에는 아무런 효과도 없다. 작은 구멍 여러개와 커다란 구멍 하나가 있는 건틀렛이다.", RelicType.AllEffect, () => EmptyGemGauntlet()));
        relics.Add(new WarRelic(25, "작은 보석 더미", 10, "당장에는 아무런 효과도 없다. 작은 보석 여러개의 더미로, 색상이 다양하다.", RelicType.AllEffect, () => SmallGemPile()));
        relics.Add(new WarRelic(26, "커다란 보석", 10, "당장에는 아무런 효과도 없다. 커다랗고 하얀 보석이다.", RelicType.AllEffect, () => LargeGem()));
        relics.Add(new WarRelic(27, "완성된 보석 건틀렛", 10, "마침내 완성시킨 보석 건틀렛이다. 적의 체력이 절반으로 감소한다.", RelicType.StateBoost, () => CompletedGemGauntlet()));
        relics.Add(new WarRelic(28, "하트 보석 목걸이", 10, "아군 전열 유닛의 체력이 0이 되는 공격을 받을 때 체력의 최대치까지 회복한다. 전투에 한번만 발동한다.", RelicType.BattleActive, () => HeartGemNecklace()));
        relics.Add(new WarRelic(29, "용기의 깃발", 10, "사기의 효과 적용 선이 플레이어에게 유리하게 바뀜. (일반 상태를 제외한 모든 효과의 적용 선이 일정 수치 감소.)", RelicType.SpecialEffect, () => HeartGemNecklace()));
        relics.Add(new WarRelic(30, "부러진 직검", 0, "아군이 공격시 낮은 확률로 데미지 일정 수치 감소.", RelicType.StateBoost, () => BrokenStraightSword()));
       
        relics.Add(new WarRelic(31, "깨진 투구", 0, "모든 아군의 최대 기력 1 감소.", RelicType.SpecialEffect, () => CrackedHelmet()));
        relics.Add(new WarRelic(32, "해진 군화", 0, "기동력 1 감소. (1 미만으로 내려가지 않음)", RelicType.StateBoost, () => WornOutBoots()));
        relics.Add(new WarRelic(33, "갈라진 방패", 0, "아군 유닛의 장갑이 2 감소한다. (1 미만으로 내려가지 않음)", RelicType.BattleActive, () => SplitShield()));
        relics.Add(new WarRelic(34, "황폐한 깃발", 0, "사기가 감소할 때 일정 수치 더 감소함.", RelicType.SpecialEffect, () => WastedFlag()));
        relics.Add(new WarRelic(35, "생존자의 넝마떼기", 10, "아군이 사망할수록 다른 아군 공격력 일정수치 증가, 마지막에 남은 아군의 공격력, 장갑, 기동성 일정수치 증가.", RelicType.BattleActive, () => Relic35()));
        relics.Add(new WarRelic(36, "정복자의 인장", 10, "엘리트 스테이지 승리 시 전쟁 유산 보상이 한번 더 등장함.", RelicType.SpecialEffect, () => Relic36()));
        relics.Add(new WarRelic(37, "맹인전사의 안대", 10, "적의 배치 정보가 모두 숨겨져 알 수 없게 됨. 모든 아군 유닛이 전투 시작 시 관통 특성을 얻고, 궁병의 사거리 1 증가.", RelicType.ActiveState, () => BlindWarriorEyepatch()));
        relics.Add(new WarRelic(38, "덧댐 장갑판", 1, "모든 아군 유닛의 장갑 1 증가.", RelicType.StateBoost, () => ReinforcedArmorPlate()));
        relics.Add(new WarRelic(39, "장식된 단검", 1, "모든 아군 유닛의 공격력 일정배수 10% 증가.", RelicType.StateBoost, () => Relic39()));
        relics.Add(new WarRelic(40, "전쟁나팔", 1, "보스 스테이지 진입 시 사기 일정수치 증가.", RelicType.SpecialEffect, () => WarHorn()));
        
        relics.Add(new WarRelic(41, "무료 배식권", 1, "병영 스테이지 진입 시 모든 아군 기력 1 회복.", RelicType.SpecialEffect, () => FreeMealTicket()));
        relics.Add(new WarRelic(42, "파괴공작용 대포", 1, "일반, 엘리트 전투 시작 시 모든 적의 체력이 일정 배수(약 10%) 깎이고 시작.", RelicType.StateBoost, () => Relic42()));
        relics.Add(new WarRelic(43, "자율 개발 명령서", 1, "이 전쟁 유산 획득 후 즉시 랜덤한 아군 일정인원(2~3) 강화.", RelicType.SpecialEffect, () => AutonomousDevelopmentOrder()));
        relics.Add(new WarRelic(44, "해진 정찰 보고서", 1, "엘리트 및 보스 스테이지에서 주는 피해 일정배수 증가.", RelicType.StateBoost, () => EnemyGeneralScoutReport()));
        relics.Add(new WarRelic(45, "실수한 발주 영수증", 1, "상점 리롤 시 이미 구매한 유닛, 전쟁 유산 칸이 다시 입고됨.", RelicType.SpecialEffect, () => MistakenOrderReceipt()));
        relics.Add(new WarRelic(46, "전술적 단일화 교본", 10, "유닛을 병종 한 종류로만 배치 시 아군 공격력 일정수치 증가.", RelicType.StateBoost, () => Relic46()));
        relics.Add(new WarRelic(47, "기술 비급서", 1, "기술로 주는 피해가 일정배수 증가.", RelicType.BattleActive, () => TechnicalManual()));
        relics.Add(new WarRelic(48, "보물지도", 1, "이 전쟁 유산 획득 후 다음에 진입한 이벤트 스테이지가 보물 스테이지로 변경되고 이 전쟁 유산이 소멸함.", RelicType.SpecialEffect, () => TreasureMap()));
        relics.Add(new WarRelic(49, "무지개 열쇠", 1, "이 전쟁 유산 획득 후 다음 3회 이동하는 동안 맵에서 길이 이어지지 않은 스테이지로 이동 가능. 3회 이동 후 소멸.", RelicType.SpecialEffect, () => RainbowKey()));
        relics.Add(new WarRelic(50, "외상 허가서", 1, "병영, 군사 아카데미에서 일정 골드까지 외상이 가능해짐.", RelicType.SpecialEffect, () => CreditAuthorization()));
        
        relics.Add(new WarRelic(51, "순금 나팔", 10, "금화 소모 시 소모량에 비례해 공격력 증가, 이 전쟁 유산을 획득한 이후 소모한 금화의 양이 소지한 금화 양에 비례하는 효과에 같이 적용됨.", RelicType.StateBoost, () => GoldenHorn()));
        relics.Add(new WarRelic(52, "", 1, "군사 아카데미의 무작위로 정해지는 강화 슬롯이 원래 2개에서 3개로 증가.", RelicType.SpecialEffect, () => Relic52()));
        relics.Add(new WarRelic(53, "탐험가의 나침반", 1, "전쟁 유산 보상의 선택지 한개 증가.", RelicType.SpecialEffect, () => Relic53()));
        relics.Add(new WarRelic(54, "불운의 황금 동전", 10, "주는 모든 피해가 2배로 적용됨. 단, 이 전쟁 유산을 획득할 때 높은 확률로 랜덤한 다른 전설 전쟁 유산으로 대체됨.", RelicType.StateBoost, () => Relic54()));
        relics.Add(new WarRelic(55, "저주 인형", 1, "아군 유닛 사망 시 전열 적 유닛에게 피해.", RelicType.BattleActive, () => Relic55()));
        relics.Add(new WarRelic(56, "", 10, "아군 유닛들의 체력, 공격력이 무작위로 분배됨.", RelicType.StateBoost, () => Relic56()));
        relics.Add(new WarRelic(57, "광전사의 머리칼", 10, "전투에서 적 처치로도 사기를 일정 수치 회복하게 된다.", RelicType.BattleActive, () => Relic57()));
        relics.Add(new WarRelic(58, "용사의 훈장", 10, "영웅 유닛 최대 보유수 1 증가.", RelicType.SpecialEffect, () => Relic58()));
        relics.Add(new WarRelic(59, "횡령 증거품", 0, "병영 리롤 기회 -1.", RelicType.SpecialEffect, () => Relic59()));
        relics.Add(new WarRelic(60, "승전보", 0, "군사 아카데미 리롤 기회 -1.", RelicType.SpecialEffect, () => Relic60()));
        relics.Add(new WarRelic(61, "무명의 군단 배지", 10, "영웅 유닛을 보유하지 않을 시 아군 유닛의 체력과 공격력이 20% 증가하고, 장갑이 2 증가한다.", RelicType.SpecialEffect, () => Relic61()));

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

        if (gold > 0)
        {
            var units= RogueLikeData.Instance.GetMyUnits();
            foreach (var unit in units)
            {
                unit.attackDamage += (gold/100);
            }
        }
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
            unit.maxHealth *= 1.3f;
            unit.health = unit.maxHealth; // 체력 증가
            unit.attackDamage *= 1.3f; // 공격력 증가

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

    //반응성의 가시 갑옷
    private static void ReactiveThornArmor()
    {


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
                unit.mobility += 3;
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
                unit.attackDamage *= 1.2f;
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
                unit.attackDamage *= 1.2f;
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
                unit.attackDamage *= 1.2f;
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
                unit.maxHealth *=  1+(curseCount*0.1f);
                unit.health = unit.maxHealth;
                unit.attackDamage *= 1+(curseCount*0.1f);
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
            unit.maxHealth = unit.maxHealth * 0.5f;
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
            unit.attackDamage *= 0.85f;
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
        var unis = RogueLikeData.Instance.GetMyUnits();
        foreach(var unit in unis)
        {
            unit.pierce = true;
            if (unit.rangedAttack)
            {
                unit.range++;
            }
        }

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
            unit.attackDamage *= 1.1f;
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
                unit.maxHealth *= 0.9f;
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
            unit.attackDamage *= 1.2f;

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

    //52
    private static void Relic52()
    {

    }

    //불운의 황금 동전
    private static void Relic53()
    {

    }

    //54
    private static void Relic54()
    {
        RogueLikeData.Instance.AddMyMultipleDamage(1);
    }

    //55
    private static void Relic55()
    {

    }

    //56        
    private static void Relic56()
    {
        //기준이 없길래 1~999사이의 값으로 함
        var units = RogueLikeData.Instance.GetMyUnits();
        System.Random random = new System.Random();

        foreach (var unit in units)
        {
            unit.health = random.Next(1, 1000);        // 1~999 사이의 랜덤 체력
            unit.attackDamage = random.Next(1, 1000);  // 1~999 사이의 랜덤 공격력
        }

        // 변경된 유닛 데이터를 저장
        RogueLikeData.Instance.AllMyUnits(units);
    }

    //57
    private static void Relic57()
    {

    }

    //58
    private static void Relic58()
    {

    }

    //59
    private static void Relic59()
    {

    }

    //60
    private static void Relic60()
    {

    }

    //61
    private static void Relic61()
    {

    }
}
