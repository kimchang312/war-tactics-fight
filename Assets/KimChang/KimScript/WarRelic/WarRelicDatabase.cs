using System.Collections.Generic;

public static class WarRelicDatabase
{
    public static List<WarRelic> relics = new List<WarRelic>();

    static WarRelicDatabase()
    {
        // 일반 유산
        relics.Add(new WarRelic(1, "할인패", 1,"병영의 모든 가격 일정 비율 감소.", RelicType.StatBoost));
        relics.Add(new WarRelic(2, "단단한 모루",1, "군사 아카데미의 강화 비용 일정 비율 감소.", RelicType.StatBoost));
        relics.Add(new WarRelic(3, "인내력의 깃발",1, "기력이 감소할 때 일정 확률로 감소하지 않음.", RelicType.StatBoost));
        relics.Add(new WarRelic(4, "추가 보급 명령서",1, "병영과 군사 아카데미의 리롤 1회 무료 OR 리롤 기회 1회 추가.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(5, "도금 망원경",1, "지도에 보이는 스테이지 정보 1칸 증가.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(6, "행운의 금화 주머니",1, "얻는 금화의 양이 일정 배수 증가.", RelicType.StatBoost));
        relics.Add(new WarRelic(7, "전설의 도굴꾼의 삽", 1, "다음 일반 유산 보상 선택지가 전설 유산 보상으로 바뀜. 효과를 발휘한 후에 이 전쟁 유산이 바로 소멸됨.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(8, "고고학 키트", 1, "일반 유산 보상 선택지에서 3번까지 리롤 할 수 있다. 추가 리롤 3회 소모시 소멸.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(9, "순금 검", 1, "소지 중인 금화 양에 비례해서 아군 공격력 증가.", RelicType.StatBoost));
        relics.Add(new WarRelic(38, "덧댐 장갑판", 1, "모든 아군 유닛의 장갑 1 증가.", RelicType.StatBoost, armorAdd: 1));
        relics.Add(new WarRelic(39, "", 1, "모든 아군 유닛의 공격력 일정배수(5~10%) 증가.", RelicType.StatBoost, attackAdd: 5));
        relics.Add(new WarRelic(40, "전쟁나팔", 1, "보스 스테이지 진입 시 사기 일정수치 증가.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(41, "무료 배식권", 1, "병영 스테이지 진입 시 모든 아군 기력 1 회복.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(42, "", 1, "일반, 엘리트 전투 시작 시 모든 적의 체력이 일정 배수(약 10%) 깎이고 시작.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(43, "자율 개발 명령서", 1, "이 전쟁 유산 획득 후 즉시 랜덤한 아군 일정인원(2~3) 강화.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(44, "적장 정찰 보고서", 1, "엘리트 및 보스 스테이지에서 주는 피해 일정배수 증가.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(45, "실수한 발주 영수증", 1, "상점 리롤 시 이미 구매한 유닛, 전쟁 유산 칸이 다시 입고됨.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(47, "기술 비급서", 1, "기술로 주는 피해가 일정배수 증가.", RelicType.StatBoost));
        relics.Add(new WarRelic(48, "보물지도", 1, "이 전쟁 유산 획득 후 다음에 진입한 이벤트 스테이지가 보물 스테이지로 변경되고 이 전쟁 유산이 소멸함.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(49, "무지개 열쇠", 1, "이 전쟁 유산 획득 후 다음 3회 이동하는 동안 맵에서 길이 이어지지 않은 스테이지로 이동 가능. 3회 이동 후 소멸.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(50, "외상 허가서", 1, "병영, 군사 아카데미에서 일정 골드까지 외상이 가능해짐.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(52, "", 1, "군사 아카데미의 무작위로 정해지는 강화 슬롯이 원래 2개에서 3개로 증가.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(53, "", 1, "전쟁 유산 보상의 선택지 한개 증가.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(55, "", 1, "아군 유닛 사망 시 전열 적 유닛에게 피해.", RelicType.SpecialEffect));

        // 전설 유산
        relics.Add(new WarRelic(10, "예리한 양날도끼", 10,"적이 받는 피해가 일정 배수 증가하고, 아군이 받는 피해는 비교적 적은 배수로 증가.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(11, "광기의 깃발", 10,"적이 받는 피해가 일정 배수 증가하고, 아군의 장갑 2 감소.", RelicType.StatBoost, armorAdd: -2));
        relics.Add(new WarRelic(12, "혼돈의 깃발", 10,"아군의 배치가 무작위 순서로 배치된다. 체력과 공격력이 일정수치 증가, 궁병의 사거리 1 증가.", RelicType.StatBoost, healthAdd: 10, attackAdd: 10));
        relics.Add(new WarRelic(13, "위대한 지휘관의 훈장", 10,"중복된 유닛이 없을 때 유닛 상한이 일정 수치 증가, 궁병의 사거리 1 증가.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(14, "유연성의 부적", 10,"모든 종류의 병종을 소지할 시 체력, 공격력이 일정수치 증가.", RelicType.StatBoost, healthAdd: 15, attackAdd: 15));
        relics.Add(new WarRelic(15, "반응성의 가시 갑옷", 10,"중갑을 가진 유닛이 피해를 입을 때마다 상대 전열에게 자신의 장갑에 일정수치에 비례한 데미지를 준다.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(16, "제국 수호자의 훈장", 10, "수호 스킬을 가진 유닛의 장갑 일정 수치 증가, 받는 모든 피해를 두차례로 나눠서 받음.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(17, "매우 가벼운 군복 바지", 10, "중갑병과 기병을 제외한 아군 유닛의 회피율 2배 증가.", RelicType.StatBoost));
        relics.Add(new WarRelic(18, "팔랑크스 전술서", 10, "아군의 모든 유닛이 창병일 때 궁병에게 받는 피해 일정 배수 감소, 장갑 일정 수치 증가, 사거리 1 증가.", RelicType.StatBoost));
        relics.Add(new WarRelic(19, "정예 기병대 안장", 10, "아군의 모든 유닛이 기병일 때 기동력이 일정 수치 증가하고, 돌격 스킬이 두번 발동한다.", RelicType.StatBoost));
        relics.Add(new WarRelic(20, "정예 궁병 부대 깃털모자", 10, "아군의 모든 유닛이 궁병일 때 사거리가 일정 수치 증가하고, 회피율이 높게 증가.", RelicType.StatBoost));
        relics.Add(new WarRelic(21, "민병대 나팔", 10, "공용 유닛이 20인 이상일 때, 유닛 상한이 30인으로 증가. 단, 증가한 유닛 상한에는 공용 유닛만 편성 가능.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(22, "소름끼치는 구슬", 10, "소지 중인 저주 유산 당 아군 유닛의 체력, 공격력 일정 수치 증가.", RelicType.StatBoost));
        relics.Add(new WarRelic(23, "해주 부적", 10, "저주 유산의 효과가 전부 무효화됨.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(54, "", 10, "주는 모든 피해가 2배로 적용됨. 단, 이 전쟁 유산을 획득할 때 높은 확률로 랜덤한 다른 전설 전쟁 유산으로 대체됨.", RelicType.SpecialEffect));

        // 저주 유산
        relics.Add(new WarRelic(30, "부러진 직검", 0,"아군이 공격시 낮은 확률로 데미지 일정 수치 감소.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(31, "깨진 투구", 0, "모든 아군의 최대 기력 1 감소.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(32, "해진 군화", 0, "기동력 1 감소.", RelicType.StatBoost, mobilityAdd: -1));
        relics.Add(new WarRelic(33, "갈라진 방패", 0, "아군이 피격시 낮은 확률로 장갑을 무시하고 데미지를 받음.", RelicType.SpecialEffect));
        relics.Add(new WarRelic(34, "황폐한 깃발", 0, "사기가 감소할 때 일정 수치 더 감소함.", RelicType.SpecialEffect));
    }

    public static WarRelic GetRelicById(int id)
    {
        return relics.Find(relic => relic.id == id);
    }
}