using System;
using static EventManager;
using static RelicManager;

public class PitchBlackCave : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        int morale = RogueLikeData.Instance.GetMorale();

        switch (choice)
        {
            case 0: // 어둠 속 진입 → 사기 -50, 전설 유산
                {
                    RogueLikeData.Instance.SetMorale(Math.Max(0, morale - 50));

                    var relic = RelicManager.HandleRandomRelic(grade: 10, action: RelicAction.Acquire);
                    return $"칠흑 같은 어둠 속에서 귀중한 유산을 발견했습니다. 전설 유산 '{relic.name}'을 획득했습니다. (사기 -50)";
                }

            default: // 무시
                return "부대는 어둠을 피해 조용히 길을 돌렸습니다.";
        }
    }

    public static bool CheckCondition()
    {
        return RogueLikeData.Instance.GetMorale() >= 51;
    }
}
