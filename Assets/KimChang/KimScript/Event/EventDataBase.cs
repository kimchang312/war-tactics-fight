using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventDataBase
{
    public static List<GameEvent> events = new List<GameEvent>();

    static EventDataBase()
    {
        events.Add(new GameEvent(0, "여행자의 쉼터", "숲 속에서 발견한 허름한 쉼터. 병사들은 한개의 침대를 두고 눈치를 보기 시작했다.", EventType.Normal, new string[] { "한 병사를 침대에 눕힌다.", "모두가 똑같이 바닥에서 쉰다.", "길을 서두른다." }, new int[] { 0, 0, 0 }));
        events.Add(new GameEvent(1, "기묘한 꿈", "깊은 잠에 빠진 밤, 기묘한 꿈을 꾸었다.", EventType.Special, new string[] { "꿈의 의미를 탐구한다.", "꿈을 무시한다." }, new int[] { 0, 0 }));
        events.Add(new GameEvent(2, "지나가던 사범", "길을 걷다 우연히 검을 휘두르는 사내를 만났다. 한 병사가 가르침을 원하는 듯 하다.", EventType.Special, new string[] { "병사를 맡긴다.", "예의를 갖추고 지나친다." }, new int[] { 0, 0 }));
        events.Add(new GameEvent(3, "성수의 축복", "신성한 성수가 가득한 작은 제단을 발견했다.", EventType.Special, new string[] { "성수를 사용한다.", "자리를 뜬다." }, new int[] { 0, 0 }));
        events.Add(new GameEvent(4, "보물이 가득한 방", "깊은 유적을 탐험하던 중, 금빛으로 빛나는 문을 발견했다.", EventType.Reward, new string[] { "작은 행운을 누린다.", "더 많은 것을 원한다!", "함정을 의심한다." }, new int[] { 0, 0, 0 }));
        events.Add(new GameEvent(5, "수상한 상인", "낡은 로브의 상인이 미소 지으며 거래를 제안했다.", EventType.Shop, new string[] { "금화로 거래한다.", "군량미로 거래한다.", "그를 무시한다." }, new int[] { 0, 0, 0 }));
        events.Add(new GameEvent(6, "깊은 밤의 유혹", "야영지엔 깊은 정적이 감돌고, 당신도 점점 눈꺼풀이 무거워진다.", EventType.Special, new string[] { "잠깐 눈을 붙인다.", "몸을 눕히고 깊이 잠든다.", "긴장을 놓지 않는다." }, new int[] { 0, 0, 0 }));
        events.Add(new GameEvent(7, "피의 서약", "야영지 근처에서 피로 물든 제단을 발견했다. 검은 망토를 쓴 자가 서약을 요구한다.", EventType.Special, new string[] { "서약을 맺는다.", "피를 바친다.", "그 자를 외면하고 떠난다." }, new int[] { 0, 0, 0 }));
        events.Add(new GameEvent(8, "작은 마을", "여정을 이어가던 중, 조용한 작은 마을을 발견했다.", EventType.Normal, new string[] { "주민을 징병한다.", "마을을 약탈한다.", "대접을 요청한다.", "가볍게 정비하고 떠난다." }, new int[] { 0, 0, 0, 0 }));
        events.Add(new GameEvent(9, "유랑하는 상인", "상인이 호위 병사를 찾는다며 금화를 내보였다.", EventType.Shop, new string[] { "병사를 고용해 보낸다.", "이 제안을 거절한다." }, new int[] { 0, 0 }));
        events.Add(new GameEvent(10, "도적들의 습격", "수풀 뒤에서 도적들이 튀어나왔다. 날이 선 무기를 들고 금화를 내놓으라 협박하고 있다.", EventType.Battle, new string[] { "금화를 건넨다.", "싸운다!" }, new int[] { 0, 0 }));
        events.Add(new GameEvent(11, "재상의 지원", "부패한 재상이 군대를 후원할 의사가 있다고 한다.", EventType.Special, new string[] { "후원을 받는다.", "재상의 음모를 무너뜨린다.", "제안을 거절한다." }, new int[] { 0, 0, 0 }));
        events.Add(new GameEvent(12, "샘의 정령", "전쟁 유산이 샘에 빠지자, 수면이 일렁였다. 그 안에서 정령이 모습을 드러내더니 조용히 묻는다.", EventType.Special, new string[] { "정직하게 대답한다.", "거짓을 말한다." }, new int[] { 0, 0 }));
        events.Add(new GameEvent(13, "병사 훈련소", "훈련소의 병사들이 땀을 흘리며 움직이고 있다.", EventType.Special, new string[] { "병사들을 훈련에 참가시킨다.", "병사들에게 자유 훈련을 허락한다.", "우리는 다른 일정이 있다." }, new int[] { 0, 0, 0 }));
        events.Add(new GameEvent(14, "마차의 잔해", "불에 그을려 있는 마차의 잔해를 발견했다. 검게 탄 자국에는 아직 연기가 가시지 않았다.", EventType.Special, new string[] { "조심히 잔해를 뒤져본다.", "그대로 자리를 뜬다." }, new int[] { 0, 0 }));
        events.Add(new GameEvent(15, "무너지는 사원", "사원의 천장이 무너져 내리고 있다. 안쪽에 반쯤 드러난 성물이 은은히 빛나고 있었다.", EventType.Reward, new string[] { "성물을 급히 들고 달아난다.", "병사의 희생으로 안전히 탐색한다.", "잔해를 치우고 조심히 접근한다.", "위험을 감수할 필요는 없다." }, new int[] { 0, 0, 0, 0 }));
        events.Add(new GameEvent(16, "뜻밖의 인연", "주점에서 낯익은 얼굴들과 마주쳤다. 그들은 지금, 적도 아군도 아닌 용병이었다.", EventType.Special, new string[] { "그들을 고용한다.", "술을 함께 마신다.", "그냥 지나친다." }, new int[] { 0, 0, 0 }));
        events.Add(new GameEvent(17, "늪지대", "병사의 발목을 붙잡는 늪지대가 펼쳐졌다.", EventType.Normal, new string[] { "늪지대를 가로지른다.", "우회로에서 한 병사가 부상을 당했다." }, new int[] { 0, 0 }));
        events.Add(new GameEvent(18, "부대 편성의 기회", "전열을 가다듬을 기회가 생겼다.", EventType.Special, new string[] { "신병을 모집한다.", "병사를 승급시킨다.", "보급품을 여유있게 확보한다." }, new int[] { 0, 0, 0 }));
        events.Add(new GameEvent(19, "바위에 박힌 검", "호숫가에 꽂힌 검을 본 병사들은 숨을 죽였다.", EventType.Special, new string[] { "독실한 병사가 검에 기도한다.", "힘 있는 병사가 검을 뽑으려 한다.", "검을 바라보다 조용히 자리를 뜬다." }, new int[] { 0, 0, 0 }));
    }
        public static GameEvent GetEventById(int id)
    {
        return events.Find(evt => evt.id == id);
    }
}