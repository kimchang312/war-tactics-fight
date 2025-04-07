using System;
using static EventManager;

public class UnexpectedCompanions : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        int gold = RogueLikeData.Instance.GetCurrentGold();

        if (gold < 100)
        {
            return "소지한 금화가 부족해 어떤 선택도 할 수 없습니다.";
        }

        switch (choice)
        {
            case 0: // 고용 → 금화 -300, 유닛 3명 획득 (텍스트 처리)
                if (gold < 300)
                    return "금화가 부족해 용병들을 고용할 수 없습니다.";

                RogueLikeData.Instance.SetCurrentGold(gold - 300);
                return "[텍스트 처리] 용병 3명을 고용했습니다. 무작위 희귀도 1~2 유닛 3명이 합류합니다.";

            case 1: // 술 마시기 → 금화 -100, 사기 +20
                RogueLikeData.Instance.SetCurrentGold(gold - 100);

                int morale = RogueLikeData.Instance.GetMorale();
                RogueLikeData.Instance.SetMorale(Math.Min(100, morale + 20));

                return "함께 술을 마시며 즐거운 시간을 보냈습니다. 사기가 조금 회복되었습니다.";

            default: // 그냥 지나친다
                return "그들과 마주쳤지만 인연을 만들지 않았습니다.";
        }
    }
}
