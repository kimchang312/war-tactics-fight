using static EventManager;

public class SuspiciousMerchant : IEventRewardHandler
{
    // 선택지에 따른 보상 처리
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        int gold = RogueLikeData.Instance.GetCurrentGold();
        int morale = RogueLikeData.Instance.GetMorale();

        switch (choice)
        {
            case 0: // 금화로 거래
                if (gold >= 100)
                {
                    string name = RelicManager.HandleRandomRelic(1, RelicManager.RelicAction.Acquire).name;
                    return $"금화 100을 지불하고 전쟁 유산{name}을 획득했습니다.";
                }
                return "금화가 부족합니다.";

            case 1: // 사기로 거래
                if (morale >= 31)
                {
                    RogueLikeData.Instance.SetMorale(morale - 30);
                    string name = RelicManager.HandleRandomRelic(1, RelicManager.RelicAction.Acquire).name;
                    return $"사기 30을 지불하고 전쟁 유산{name}을 획득했습니다.";
                }
                return "사기가 부족합니다.";

            default: // 무시
                return "아무 일도 일어나지 않았습니다.";
        }
    }

}
