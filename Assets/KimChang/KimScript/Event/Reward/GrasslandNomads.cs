using static EventManager;

public class GrasslandNomads : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        int gold = RogueLikeData.Instance.GetCurrentGold();

        switch (choice)
        {
            case 0: // 궁기병 고용 → 금화 -200, 유닛 2명 (텍스트 처리)
                if (gold < 200)
                    return "금화가 부족하여 궁기병을 고용할 수 없습니다.";

                RogueLikeData.Instance.SetCurrentGold(gold - 200);
                return "[텍스트 처리] 궁기병 2명이 부대에 합류했습니다.";

            case 1: // 전술 습득 → 궁병/경기병 병종 중 강화 1회 (텍스트 처리)
                return "[텍스트 처리] 궁병 또는 경기병 병종의 무작위 강화가 1회 적용되었습니다.";

            case 2: // 야만인을 약탈 → 전투 발생, 승리 시 엘리트 유산
                return "[전투 발생] 유목 부족과의 전투가 시작됩니다. 승리 시 전쟁 유산(엘리트)을 획득합니다.";

            default:
                return "초원을 지나치며 아무 일도 일어나지 않았습니다.";
        }
    }
}
