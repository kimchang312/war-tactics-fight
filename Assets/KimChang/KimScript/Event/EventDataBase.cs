using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventDataBase
{
    public static List<GameEvent> events = new List<GameEvent>();

    static EventDataBase()
    {
        events.Add(new GameEvent(0, "여행자의 쉼터", "숲 속에서 발견한 허름한 쉼터. 병사들은 한개의 침대를 두고 눈치를 보기 시작했다.", EventType.Normal, new string[] { "한 병사를 침대에 눕힌다.", "모두가 똑같이 바닥에서 쉰다.", "길을 서두른다." }, new int[] { 0, 0, 0 },new int[] { 1, 2, 3 }));
        events.Add(new GameEvent(1, "기묘한 꿈", "깊은 잠에 빠진 밤, 기묘한 꿈을 꾸었다.", EventType.Special, new string[] { "꿈의 의미를 탐구한다.", "꿈을 무시한다." }, new int[] { 0, 0 }, new int[] { 1, 2, 3 }));
        events.Add(new GameEvent(2, "지나가던 사범", "길을 걷다 우연히 검을 휘두르는 사내를 만났다. 한 병사가 가르침을 원하는 듯 하다.", EventType.Special, new string[] { "병사를 맡긴다.", "예의를 갖추고 지나친다." }, new int[] { 0, 0 }, new int[] { 1, 2, 3 }));
        events.Add(new GameEvent(3, "성수의 축복", "신성한 성수가 가득한 작은 제단을 발견했다.", EventType.Special, new string[] { "성수를 사용한다.", "자리를 뜬다." }, new int[] { 0, 0 }, new int[] { 1, 2, 3 }));
        events.Add(new GameEvent(4, "보물이 가득한 방", "깊은 유적을 탐험하던 중, 금빛으로 빛나는 문을 발견했다.", EventType.Reward, new string[] { "작은 행운을 누린다.", "더 많은 것을 원한다!", "함정을 의심한다." }, new int[] { 0, 0, 0 }, new int[] { 1, 2, 3 }));
        events.Add(new GameEvent(5, "수상한 상인", "낡은 로브의 상인이 미소 지으며 거래를 제안했다.", EventType.Shop, new string[] { "금화로 거래한다.", "군량미로 거래한다.", "그를 무시한다." }, new int[] { 0, 0, 0 }, new int[] { 1, 2, 3 }));
        events.Add(new GameEvent(6, "깊은 밤의 유혹", "야영지엔 깊은 정적이 감돌고, 당신도 점점 눈꺼풀이 무거워진다.", EventType.Special, new string[] { "잠깐 눈을 붙인다.", "몸을 눕히고 깊이 잠든다.", "긴장을 놓지 않는다." }, new int[] { 0, 0, 0 }, new int[] { 1, 2, 3 }));
        events.Add(new GameEvent(7, "피의 서약", "야영지 근처에서 피로 물든 제단을 발견했다. 검은 망토를 쓴 자가 서약을 요구한다.", EventType.Special, new string[] { "서약을 맺는다.", "피를 바친다.", "그 자를 외면하고 떠난다." }, new int[] { 0, 0, 0 }, new int[] { 1, 2, 3 }));
        events.Add(new GameEvent(8, "작은 마을", "여정을 이어가던 중, 조용한 작은 마을을 발견했다.", EventType.Normal, new string[] { "주민을 징병한다.", "마을을 약탈한다.", "대접을 요청한다.", "가볍게 정비하고 떠난다." }, new int[] { 0, 0, 0, 0 }, new int[] { 1, 2, 3 }));
        events.Add(new GameEvent(9, "유랑하는 상인", "상인이 호위 병사를 찾는다며 금화를 내보였다.", EventType.Shop, new string[] { "병사를 고용해 보낸다.", "이 제안을 거절한다." }, new int[] { 0, 0 }, new int[] { 1, 2, 3 }));

        events.Add(new GameEvent(10, "도적들의 습격", "수풀 뒤에서 도적들이 튀어나왔다. 날이 선 무기를 들고 금화를 내놓으라 협박하고 있다.", EventType.Battle, new string[] { "금화를 건넨다.", "싸운다!" }, new int[] { 0, 0 }, new int[] { 1, 2, 3 }));
        events.Add(new GameEvent(11, "재상의 지원", "부패한 재상이 군대를 후원할 의사가 있다고 한다.", EventType.Special, new string[] { "후원을 받는다.", "재상의 음모를 무너뜨린다.", "제안을 거절한다." }, new int[] { 0, 0, 0 }, new int[] { 1, 2, 3 }));
        events.Add(new GameEvent(12, "샘의 정령", "전쟁 유산이 샘에 빠지자, 수면이 일렁였다. 그 안에서 정령이 모습을 드러내더니 조용히 묻는다.", EventType.Special, new string[] { "정직하게 대답한다.", "거짓을 말한다." }, new int[] { 0, 0 }, new int[] { 1 }));
        events.Add(new GameEvent(13, "병사 훈련소", "훈련소의 병사들이 땀을 흘리며 움직이고 있다.", EventType.Special, new string[] { "병사들을 훈련에 참가시킨다.", "병사들에게 자유 훈련을 허락한다.", "우리는 다른 일정이 있다." }, new int[] { 0, 0, 0 }, new int[] { 1 }));
        events.Add(new GameEvent(14, "마차의 잔해", "불에 그을려 있는 마차의 잔해를 발견했다. 검게 탄 자국에는 아직 연기가 가시지 않았다.", EventType.Special, new string[] { "조심히 잔해를 뒤져본다.", "그대로 자리를 뜬다." }, new int[] { 0, 0 }, new int[] { 1 }));
        events.Add(new GameEvent(15, "무너지는 사원", "사원의 천장이 무너져 내리고 있다. 안쪽에 반쯤 드러난 성물이 은은히 빛나고 있었다.", EventType.Reward, new string[] { "성물을 급히 들고 달아난다.", "병사의 희생으로 안전히 탐색한다.", "잔해를 치우고 조심히 접근한다.", "위험을 감수할 필요는 없다." }, new int[] { 0, 0, 0, 0 }, new int[] { 1 }));
        events.Add(new GameEvent(16, "뜻밖의 인연", "주점에서 낯익은 얼굴들과 마주쳤다. 그들은 지금, 적도 아군도 아닌 용병이었다.", EventType.Special, new string[] { "그들을 고용한다.", "술을 함께 마신다.", "그냥 지나친다." }, new int[] { 0, 0, 0 }, new int[] { 1 }));
        events.Add(new GameEvent(17, "늪지대", "병사의 발목을 붙잡는 늪지대가 펼쳐졌다.", EventType.Normal, new string[] { "늪지대를 가로지른다.", "우회로에서 한 병사가 부상을 당했다." }, new int[] { 0, 0 }, new int[] { 1 }));
        events.Add(new GameEvent(18, "부대 편성의 기회", "전열을 가다듬을 기회가 생겼다.", EventType.Special, new string[] { "신병을 모집한다.", "병사를 승급시킨다.", "보급품을 여유있게 확보한다." }, new int[] { 0, 0, 0 }, new int[] { 1 }));
        events.Add(new GameEvent(19, "바위에 박힌 검", "호숫가에 꽂힌 검을 본 병사들은 숨을 죽였다.", EventType.Special, new string[] { "독실한 병사가 검에 기도한다.", "힘 있는 병사가 검을 뽑으려 한다.", "검을 바라보다 조용히 자리를 뜬다." }, new int[] { 0, 0, 0 }, new int[] { 1 }));

        events.Add(new GameEvent(20, "저주받은 주둔지", "폐허의 주둔지 위로 검은 불꽃이 맴돌았다. 그 불길한 기운은 천천히 병사들을 감싸기 시작했다", EventType.Special, new string[] { "병사들이 저주받았다.", "전력으로 도망친다." }, new int[] { 0, 0 }, new int[] { 1 }));
        events.Add(new GameEvent(21, "짙은 안개의 갈림길", "짙은 안개가 깔린 갈림길. '힘'과 '재물', 두 개의 길이 있다. 어디로 나아가야 할까?", EventType.Special, new string[] { "힘의 길을 택한다.", "재물의 길을 택한다." }, new int[] { 0, 0 }, new int[] { 1 }));
        events.Add(new GameEvent(22, "고대의 무덤", "무너진 흙더미 속에서 봉인된 듯한 무덤이 드러났다. 검게 빛나는 석관 위에 낯선 언어로 쓰인 문장이 새겨져 있다.", EventType.Special, new string[] { "봉인을 풀고 석관을 열어본다.", "건드리지 않고 지나간다." }, new int[] { 0, 0 }, new int[] { 2 }));
        events.Add(new GameEvent(23, "용병 모집", "전장 근처 야영지에 용병들이 모여들었다. 그들은 우리 군에 합류하고 싶다 말했다.", EventType.Special, new string[] { "용병단 전체를 고용한다.", "개별 용병만 고용한다.", "용병은 받아들이지 않는다." }, new int[] { 0, 0, 0 }, new int[] { 2 }));
        events.Add(new GameEvent(24, "잊혀진 지휘관", "바위 언덕은 마치 오래전 누군가의 마지막 진지 같았다. 낡은 지도에는 전투의 흐름이 새겨져 있다.", EventType.Special, new string[] { "전술을 계승한다.", "유물만 챙기고 자리를 뜬다." }, new int[] { 0, 0 }, new int[] { 2 }));
        events.Add(new GameEvent(25, "유랑하는 연금술사", "떠돌이 연금술사가 말을 걸어 왔다. '좋은 물약 있어요. 필요한 걸로 골라보시죠.'", EventType.Shop, new string[] { "강화 물약", "기력 물약", "해주 물약", "필요 없다" }, new int[] { 0, 0, 0, 0 }, new int[] { 2 }));
        events.Add(new GameEvent(26, "경주의 승자는?", "한 도시에서 유명한 기병 경주가 열리고 있다. 많은 병사들이 경기를 구경하며 내기를 하고 있다.", EventType.Special, new string[] { "챔피언에게 돈을 건다.", "유망주에게 돈을 건다.", "우리 병사로 우승을 노린다.", "잠시 긴장을 풀며 구경한다." }, new int[] { 0, 0, 0, 0 }, new int[] { 2 }));
        events.Add(new GameEvent(27, "검은 기사의 결투", "전장 근처 폐허에 나타난 검은 기사는 말없이 검을 뽑아 들고 결투를 청해왔다.", EventType.Battle, new string[] { "용감한 병사 하나를 결투에 내보낸다.", "정정당당한 결투 따위 없다!" }, new int[] { 0, 0 }, new int[] { 2 }));
        events.Add(new GameEvent(28, "초원의 방랑자들", "광활한 초원을 지나던 중, 작은 유목 부족을 발견했다.", EventType.Battle, new string[] { "야만인을 약탈한다.", "무시하고 지나간다." }, new int[] { 0, 0 }, new int[] { 2 }));
        events.Add(new GameEvent(29, "폭풍우 치는 밤에", "물자가 부족한 채 작은 오두막에 피신했지만, 폭풍은 쉽게 그칠 기세가 아니다.", EventType.Special, new string[] { "폭풍을 뚫고 진군한다.", "물자를 아껴가며 버틴다." }, new int[] { 0, 0 }, new int[] { 2 }));

        events.Add(new GameEvent(30, "영혼의 계곡", "깊고 어두운 계곡. 강력한 힘을 얻기 위해선 반드시 희생을 치러야 한다.", EventType.Special, new string[] { "영혼을 바친다.", "거절하고 떠난다." }, new int[] { 0, 0 }, new int[] { 2 }));
        events.Add(new GameEvent(31, "적습이다!!!", "적의 정찰대가 부대를 발견하고 기습을 감행했다.", EventType.Battle, new string[] { "전투를 준비한다." }, new int[] { 0 }, new int[] { 2 }));
        events.Add(new GameEvent(32, "영웅인가, 군대인가", "전쟁에서 승리를 결정짓는 것은 한 명의 영웅인가, 아니면 철저히 훈련된 군대인가.", EventType.Special, new string[] { "한명의 영웅", "훈련된 군대" }, new int[] { 0, 0 }, new int[] { 3 }));
        events.Add(new GameEvent(33, "바닥을 드러내는 식량", "풍족했던 저장고는 이제 바닥을 드러냈다. 더는 미룰 수 없는 결정이 눈앞에 와 있다.", EventType.Special, new string[] { "병사와 함께 굶어 결속을 다진다.", "돈으로라도 위기를 막는다." }, new int[] { 0, 0 }, new int[] { 3 }));
        events.Add(new GameEvent(34, "방황하는 영웅", "그는 한때 영웅이라 불렸다. 그러나 지금, 낡은 망토를 두른 채 폐허를 떠도는 나그네일 뿐이다.", EventType.Special, new string[] { "그를 설득하여 동료로 만든다.", "그를 무시한다." }, new int[] { 0, 0 }, new int[] { 3 }));
        events.Add(new GameEvent(35, "금지된 의식", "검은 안개가 서린 폐허 속, 이상한 문양이 새겨진 제단이 눈에 띄었다.", EventType.Special, new string[] { "의식을 완수한다.", "제단을 정화한다." }, new int[] { 0, 0 }, new int[] { 3 }));
        events.Add(new GameEvent(36, "끝없는 향락", "정신을 차려보니, 눈앞에는 금빛 술잔이 놓여 있었고, 그 안에서 포도주가 끝없이 솟구치고 있었다.", EventType.Special, new string[] { "모두와 함께 포도주를 마신다.", "전쟁포도주를 무기와 갑옷에 바른다." }, new int[] { 0, 0 }, new int[] { 3 }));
        events.Add(new GameEvent(37, "쌍둥이 악마", "똑같이 생긴 두 악마가 묻는다. '인간의 진짜 본성은 무엇인가?'", EventType.Special, new string[] { "유혹을 뿌리치고 떠난다.", "악마를 처단한다." }, new int[] { 0, 0 }, new int[] { 3 }));
        events.Add(new GameEvent(38, "전쟁의 신전", "전쟁의 신을 모시는 신전 앞에 도달했다. 피와 검으로 지어진 제단이 조용히 선택을 기다린다.", EventType.Special, new string[] { "기도한다.", "신에게 의지하지 않는다." }, new int[] { 0, 0 }, new int[] { 3 }));
        events.Add(new GameEvent(39, "최후의 결단", "부대는 지쳐가고 있지만, 전장에서 승리를 거둘 수 있는 강한 결의를 다질 기회가 왔다.", EventType.Special, new string[] { "사기를 불태우고 최후의 전투로 나선다!", "천천히 전진한다." }, new int[] { 0, 0 }, new int[] { 3 }));

        events.Add(new GameEvent(40, "인심 좋은 주모", "한 주점에 들렀더니 주모가 우리를 반겨준다. 풍성한 한 상 차림이 준비되어 있다.", EventType.Special, new string[] { "한상 차림", "풍성한 한상 차림" }, new int[] { 0, 0 }, new int[] { 3 }));
        events.Add(new GameEvent(41, "기이한 조각", "숲속을 걷던 병사가 이상한 빛을 내는 조각을 발견했다.", EventType.Special, new string[] { "병사에게 가져오게 한다.", "무시한다." }, new int[] { 0, 0 }, new int[] { 3 }));
        events.Add(new GameEvent(42, "산과 숲", "앞을 가로막는 것은 산과 숲. 길을 돌아갈 수는 없다. 어느 쪽으로 진군해야 할까?", EventType.Normal, new string[] { "산을 향해 진군한다.", "숲을 향해 진군한다." }, new int[] { 0, 0 }, new int[] { 1, 2 }));
        events.Add(new GameEvent(43, "숲과 늪지", "앞을 가로막는 것은 숲과 늪지. 길을 돌아갈 수는 없다. 어느 쪽으로 진군해야 할까?", EventType.Normal, new string[] { "숲을 향해 진군한다.", "늪지를 향해 진군한다." }, new int[] { 0, 0 }, new int[] { 1, 2 }));
        events.Add(new GameEvent(44, "늪지와 산", "앞을 가로막는 것은 늪지와 산. 길을 돌아갈 수는 없다. 어느 쪽으로 진군해야 할까?", EventType.Normal, new string[] { "늪지를 향해 진군한다.", "산을 향해 진군한다." }, new int[] { 0, 0 }, new int[] { 1, 2 }));
        events.Add(new GameEvent(45, "정신 나간 기사", "허름한 갑옷을 입은 노인이 큰 칼을 끌며 길을 가로막는다. '돈을 내라. 전투란 게 뭔지, 제대로 보여주지.'", EventType.Special, new string[] { "정신 나간 자를 걷어찬다.", "저런 병사는 필요 없다.", "내버려 둔다" }, new int[] { 0, 0, 0 }, new int[] { 1, 2 }));
        events.Add(new GameEvent(46, "칠흑같은 동굴", "한치 앞도 보이지 않는 동굴. 그러나 무언가 귀중한 것이 그 깊은 어둠 속에 있다.", EventType.Reward, new string[] { "어둠 속으로 발을 내딛는다.", "무시하고 지나간다." }, new int[] { 0, 0 }, new int[] { 1, 2 }));
        events.Add(new GameEvent(47, "마을의 골칫덩이", "작은 마을 근처에서 한 무리가 주민들에게 시비를 걸고 있었다. 분명 군대는 아니지만, 싸움에는 익숙해 보인다.", EventType.Battle, new string[] { "무력으로 제압한다.", "돈으로 회유한다." }, new int[] { 0, 0 }, new int[] { 2, 3 }));
        events.Add(new GameEvent(48, "저주받은 마법 거울", "오래된 유적의 중심에서 이질적인 마법 기운이 흘러나오는 고대 거울을 발견했다.", EventType.Special, new string[] { "거울을 가져간다", "거울을 깨뜨린다" }, new int[] { 0, 0 }, new int[] { 2, 3 }));
        events.Add(new GameEvent(49, "운명의 상자", "제단 위에 상자가 놓여 있다. 상자에는 '축복과 저주, 그 중 하나가 이 안에 있다.' 라는 글귀가 쓰여 있다.", EventType.Special, new string[] { "상자를 열어본다.", "그냥 지나친다." }, new int[] { 0, 0 }, new int[] { 2, 3 }));

    }


    public static GameEvent GetEventById(int id)
    {
        return events.Find(evt => evt.id == id);
    }
}