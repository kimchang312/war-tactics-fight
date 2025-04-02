using static EventManager;

public class PassingOffender : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0:
                return $"{unit.unitName}이 전직했습니다!-반 구현";
            default:
                return "아무 일도 일어나지 않았습니다.";
        }
    }
}