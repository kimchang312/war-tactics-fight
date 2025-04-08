using static EventManager;

public class BanditAmbush : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        int gold = RogueLikeData.Instance.GetCurrentGold();

        if (gold < 1)
        {
            return "소지한 금화가 없어 도적들이 흥미를 잃고 떠났습니다.";
        }

        switch (choice)
        {
            case 0: // 금화를 건넨다 → 금화를 0으로
                RogueLikeData.Instance.SetCurrentGold(0);
                return "도적에게 금화를 모두 빼앗겼습니다...";

            default: // 싸운다 → 전투 발생, 승리 시 금화(중)
                // 실제 전투 트리거는 외부 시스템에서 처리 필요
                return "[전투 발생] 도적들과 싸웁니다! 승리 시 금화(중)를 획득합니다.";
        }
    }

    public bool CanAppear()
    {
        return RogueLikeData.Instance.GetCurrentGold() >0;
    }
}
