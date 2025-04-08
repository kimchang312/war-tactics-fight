using static EventManager;
using static RelicManager;

public class BoxOfFate : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0: // 상자를 연다
                {
                    bool isBlessing = UnityEngine.Random.value < 0.5f;

                    if (isBlessing)
                    {
                        var relic = RelicManager.HandleRandomRelic(10, RelicAction.Acquire); // 전설
                        return $"상자를 열자 빛이 퍼졌습니다. 전설 유산 '{relic.name}'을 획득했습니다!";
                    }
                    else
                    {
                        var cursed = RelicManager.HandleRandomRelic(0, RelicAction.Acquire); // 저주
                        return $"상자 안에서 어둠이 흘러나옵니다... 저주 유산 '{cursed.name}'을 손에 넣었습니다.";
                    }
                }

            default: // 그냥 지나침
                return "당신은 상자의 유혹을 뿌리치고 조용히 지나쳤습니다.";
        }
    }
}
