using System;
using static EventManager;

public class KindInnkeeper : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        int gold = RogueLikeData.Instance.GetCurrentGold();

        if (gold < 100)
            return "금화가 부족해 음식을 주문할 수 없습니다.";

        switch (choice)
        {
            case 0: // 한상 차림 → 금화 -100, 기력 회복 (소)
                {
                    RogueLikeData.Instance.SetCurrentGold(gold - 100);

                    foreach (var u in myUnits)
                        u.energy = Math.Min(u.maxEnergy, u.energy + 1);

                    return "따뜻한 음식을 먹고 병사들의 기력이 조금 회복되었습니다. (금화 -100)";
                }

            case 1: // 풍성한 한상 차림 → 금화 -200, 기력 회복(대), 사기 +20
                {
                    if (gold < 200)
                        return "금화가 부족해 풍성한 식사를 할 수 없습니다.";

                    RogueLikeData.Instance.SetCurrentGold(gold - 200);

                    foreach (var u in myUnits)
                        u.energy = u.maxEnergy;

                    int morale = RogueLikeData.Instance.GetMorale();
                    RogueLikeData.Instance.SetMorale(Math.Min(100, morale + 20));

                    return "풍성한 식사를 하고 병사들이 기력을 완전히 회복했습니다. 사기도 상승했습니다. (금화 -200, 사기 +20)";
                }

            default: // 그냥 떠남
                return "가볍게 정비만 하고 주점을 나왔습니다.";
        }
    }

    public static bool CheckCondition()
    {
        return RogueLikeData.Instance.GetCurrentGold() >= 100;
    }
}
