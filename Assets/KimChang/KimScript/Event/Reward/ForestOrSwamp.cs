using static EventManager;

public class ForestOrSwamp : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0: // 숲 선택
                return "[전투 설정] 다음 전투에서 '숲' 전장 효과가 적용됩니다.";

            default: // 늪지대 선택
                return "[전투 설정] 다음 전투에서 '늪지대' 전장 효과가 적용됩니다.";
        }
    }
}
