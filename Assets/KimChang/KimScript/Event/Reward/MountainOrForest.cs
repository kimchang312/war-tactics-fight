using static EventManager;

public class MountainOrForest : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0: // 산 선택
                return "[전투 설정] 다음 전투에서 '산' 전장 효과가 적용됩니다.";

            default: // 숲 선택
                return "[전투 설정] 다음 전투에서 '숲' 전장 효과가 적용됩니다.";
        }
    }
}
