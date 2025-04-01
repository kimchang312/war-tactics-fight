using static EventManager;

public class TravelersShelterReward : IEventRewardHandler
{
    private const int SmallMorale = 20;

    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0:
                unit.energy = unit.maxEnergy;
                return $"{unit.unitName}의 기력이 모두 회복되었습니다!";
            case 1:
                RogueLikeData.Instance.SetMorale(RogueLikeData.Instance.GetMorale() + SmallMorale);
                return $"모두가 바닥에서 쉬었습니다. 사기 회복+{SmallMorale}";
            default:
                return "아무 일도 일어나지 않았습니다.";
        }
    }
}