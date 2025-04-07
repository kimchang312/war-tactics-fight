using System;
using static EventManager;

public class ChancellorSupport : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        int morale = RogueLikeData.Instance.GetMorale();

        // 조건 검사
        if (morale < 35 || morale > 50)
        {
            return "사기 상태가 맞지 않아 이벤트가 발생하지 않았습니다.";
        }

        switch (choice)
        {
            case 0: // 후원을 받는다 → 고정 유물 + 사기 -30
                {
                    RogueLikeData.Instance.AcquireRelic(49);
                    RogueLikeData.Instance.SetMorale(Math.Max(0, morale - 30));
                    return "재상의 보증서를 받았습니다. 사기가 감소했습니다.";
                }

            case 1: // 음모를 무너뜨린다 → 전투 발생
                // 전투 트리거 + 전투 후 보상은 반 구현
                return "[전투 발생] 재상의 수하들과 싸웁니다. 승리 시 사기 +40, 엘리트 등급 유산 획득! (반 구현)";

            default: // 제안을 거절
                return "재상의 제안을 거절했습니다.";
        }
    }
}
