using System;
using static EventManager;

public class StormyNight : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        int morale = RogueLikeData.Instance.GetMorale();
        int gold = RogueLikeData.Instance.GetCurrentGold();

        switch (choice)
        {
            case 0: // 폭풍을 뚫고 진군 → 폭풍우 전장 효과
                return "[전투 설정] 다음 전투에서 '폭풍우' 전장 효과가 적용됩니다.";

            case 1: // 버티기 → 평원 전장 효과 + 사기 -20, 금화 -100
                RogueLikeData.Instance.SetMorale(Math.Max(0, morale - 20));
                RogueLikeData.Instance.SetCurrentGold(Math.Max(0, gold - 100));
                return "[전투 설정] 다음 전투에서 '평원' 전장 효과가 적용됩니다. (사기 -20, 금화 -100)";

            default:
                return "폭풍우 속에서 아무런 결정을 내리지 못했습니다.";
        }
    }
}
