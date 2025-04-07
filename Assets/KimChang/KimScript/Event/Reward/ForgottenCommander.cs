using System;
using static EventManager;
using static RelicManager;

public class ForgottenCommander : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0: // 전술 계승 → 무작위 병종 무작위 강화 2회 (텍스트)
                return "[텍스트 처리] 무작위 병종의 무작위 강화가 2회 적용되었습니다.";

            case 1: // 유물만 챙긴다 → 일반 등급 유산 획득
                {
                    var relic = RelicManager.HandleRandomRelic(grade: 1, action: RelicAction.Acquire);
                    return $"전투 지도 아래에서 유물을 발견했습니다. 일반 등급 유산 '{relic.name}'을 획득했습니다.";
                }

            default: // 예를 갖추고 떠남 → 사기 회복(중)
                {
                    int morale = RogueLikeData.Instance.GetMorale();
                    RogueLikeData.Instance.SetMorale(Math.Min(100, morale + 40));
                    return "고인의 정신을 기리며 물러났습니다. 사기가 크게 회복되었습니다. (사기 +40)";
                }
        }
    }
}
