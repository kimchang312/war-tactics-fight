using static EventManager;

public class FinalResolve : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0: // 사기 회복 + 보스 진입
                {
                    RogueLikeData.Instance.SetMorale(100);
                    return "[보스 진입] 부대는 사기를 불태우며 최후의 결전을 향해 나아갑니다! (사기 최대)";
                }

            default: // 천천히 전진
                return "부대는 조용히 앞으로 나아갑니다.";
        }
    }
}
