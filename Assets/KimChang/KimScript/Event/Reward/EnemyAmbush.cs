using static EventManager;

public class EnemyAmbush : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0: // 엘리트 전투 + 유산 2개 보상
                return "[전투 발생] 엘리트 전투가 시작됩니다! 승리 시 일반 전쟁 유산 2개를 획득합니다.";

            default: // 도망
                return "부대는 전투를 피하고 전열을 정비했습니다.";
        }
    }

    public bool CanAppear()
    {
        return RogueLikeData.Instance.GetCurrentStageX()>=9;
    }
}
