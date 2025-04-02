using static EventManager;

public class FullTreasureRoom : IEventRewardHandler
{
    private const int SmallMorale = 20;

    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0:
                int gold = RogueLikeData.Instance.AddGoldByEventChapter(50);
                return $"방에서 {gold}골드를 가져갔습니다.";
            case 1:
                return BeGreedy();
            default:
                return "아무 일도 일어나지 않았습니다.";
        }
    }
    private string BeGreedy()
    {
        int gold = RogueLikeData.Instance.AddGoldByEventChapter(150);
        string name = RelicManager.RemoveRandomCursedRelic();
        return $"{gold}골드를 획득 했지만 {name}저주를 받았습니다.";
    }
}