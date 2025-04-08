using static EventManager;
using static RelicManager;

public class AncientTomb : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0: // 봉인을 풀고 연다 → 엘리트 유산 + 50% 확률로 저주 유산
                {
                    // 엘리트 유산 확률 처리 (80% 일반, 20% 전설)
                    int eliteGrade = UnityEngine.Random.value < 0.8f ? 1 : 10;
                    var elite = HandleRandomRelic(grade: eliteGrade, action: RelicAction.Acquire);

                    string result = $"봉인을 해제하고 석관을 열었습니다. 엘리트 유산 '{elite.name}'을 획득했습니다.";

                    if (UnityEngine.Random.value < 0.5f)
                    {
                        var curse = RelicManager.HandleRandomRelic(grade: 0, action: RelicAction.Acquire);
                        result += $"\n그와 함께 어둠이 퍼졌습니다... 저주 유산 '{curse.name}'도 함께 손에 넣었습니다.";
                    }

                    return result;
                }

            default: // 그냥 지나감
                return "무덤을 건드리지 않고 조용히 지나쳤습니다.";
        }
    }
}
