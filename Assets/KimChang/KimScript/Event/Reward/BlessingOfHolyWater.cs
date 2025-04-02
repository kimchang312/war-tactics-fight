using static EventManager;

public class BlessingOfHolyWaterd : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0:
                return UseHolyWater();
            default:
                return "아무 일도 일어나지 않았습니다.";
        }
    }

    private string UseHolyWater()
    {
        string name = RelicManager.RemoveRandomCursedRelic();
        return $"성수의 힘으로{name}이 제거되었습니다.";
    }
}