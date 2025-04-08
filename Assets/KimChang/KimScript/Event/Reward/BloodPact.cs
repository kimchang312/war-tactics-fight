using System.Linq;
using static EventManager;
using static RelicManager;

public class BloodPact : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();

        switch (choice)
        {
            case 0: // 서약을 맺는다 → 무작위 희귀도 2~3 유닛 희생, 전설 등급 전쟁 유산
                {
                    var candidates = myUnits.FindAll(u => u.rarity == 2 || u.rarity == 3);
                    if (candidates.Count == 0)
                        return "희귀도 2 또는 3 유닛이 없어 서약을 맺을 수 없습니다.";

                    var random = new System.Random();
                    var victim = candidates[random.Next(candidates.Count)];

                    myUnits.Remove(victim);
                    RogueLikeData.Instance.SetAllMyUnits(myUnits);

                    var relic = HandleRandomRelic(10, RelicAction.Acquire);
                    return $"유닛 '{victim.unitName}'을(를) 희생했습니다. 전설 등급 유산 '{relic.name}'을 획득했습니다.";
                }

            case 1: // 피를 바친다 → 무작위 유닛의 기력 1로 설정, 일반 전쟁 유산 획득
                {
                    var candidates = myUnits.FindAll(u => u.energy > 1);
                    if (candidates.Count == 0)
                        return "기력이 1 초과인 유닛이 없어 피를 바칠 수 없습니다.";

                    var random = new System.Random();
                    var target = candidates[random.Next(candidates.Count)];
                    target.energy = 1;

                    var relic = HandleRandomRelic(1, RelicAction.Acquire);
                    return $"유닛 '{target.unitName}'의 기력이 1로 감소했습니다. 일반 등급 유산 '{relic.name}'을 획득했습니다.";
                }

            case 2: // 무시
                return "그 자를 외면하고 떠났습니다.";

            default:
                return "잘못된 선택입니다.";
        }
    }

    public bool CanAppear()
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        int count = myUnits.Count(u => u.energy > 1);
        return count >= 2;
    }
}
