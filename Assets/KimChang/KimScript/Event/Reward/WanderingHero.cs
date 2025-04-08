using static EventManager;

public class WanderingHero : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0: // 설득 → 전투 발생, 승리 시 영웅 유닛
                return "[전투 발생] 설득은 실패했습니다. 전투가 시작됩니다! 승리 시 영웅 유닛 1명을 동료로 영입합니다. [텍스트 처리]";

            default: // 무시
                return "그의 존재를 무시하고 발걸음을 재촉했습니다.";
        }
    }

}
