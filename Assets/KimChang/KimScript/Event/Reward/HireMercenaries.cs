using static EventManager;

public class HireMercenaries : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        int gold = RogueLikeData.Instance.GetCurrentGold();

        if (gold < 100)
        {
            return "금화가 부족하여 어떤 용병도 고용할 수 없습니다.";
        }

        switch (choice)
        {
            case 0: // 전체 고용 → 금화 -300, 희귀도 2~3 유닛 3명 (텍스트)
                if (gold < 300)
                    return "금화가 부족하여 용병단 전체를 고용할 수 없습니다.";

                RogueLikeData.Instance.SetCurrentGold(gold - 300);
                return "[텍스트 처리] 희귀도 2~3 유닛 3명이 합류했습니다.";

            case 1: // 개별 고용 → 금화 -100, 희귀도 2~3 유닛 1명 (텍스트)
                RogueLikeData.Instance.SetCurrentGold(gold - 100);
                return "[텍스트 처리] 희귀도 2~3 유닛 1명이 고용되었습니다.";

            default: // 거절
                return "용병들의 제안을 거절하고 야영지를 떠났습니다.";
        }
    }
}
