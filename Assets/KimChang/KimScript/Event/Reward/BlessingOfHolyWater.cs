using static EventManager;
using static RelicManager;

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
        string name = RelicManager.HandleRandomRelic(0, action: RelicAction.Remove).name;
        return $"성수의 힘으로{name}이 제거되었습니다.";
    }

    public bool CanAppear()
    {
        return RogueLikeData.Instance.GetRelicsByGrade(0).Count > 0;
    }
}