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
                    RogueLikeData.Instance.AcquireRelic(GetRandomRelicId(grade: 1));
                    return "금화 100을 지불하고 전쟁 유산을 획득했습니다.";
                }
                return "금화가 부족합니다.";

            case 1: // 사기로 거래
                if (morale >= 31)
                {
                    RogueLikeData.Instance.SetMorale(morale - 30);
                    RogueLikeData.Instance.AcquireRelic(GetRandomRelicId(grade: 1));
                    return "사기 30을 지불하고 전쟁 유산을 획득했습니다.";
                }
                return "사기가 부족합니다.";

            default: // 무시
                return "아무 일도 일어나지 않았습니다.";
        }
    }

    // 조건: 일반 등급 전쟁 유산 중 중복되지 않은 무작위 ID 반환
    private int GetRandomRelicId(int grade)
    {
        var allRelics = WarRelicDatabase.relics;
        var ownedIds = RogueLikeData.Instance.GetAllOwnedRelicIds();

        var candidates = allRelics.FindAll(r => r.grade == grade && !ownedIds.Contains(r.id));

        if (candidates.Count == 0)
        {
            // 예외: 중복이라도 하나는 주기
            candidates = allRelics.FindAll(r => r.grade == grade);
        }

        var random = new System.Random();
        int index = random.Next(candidates.Count);
        return candidates[index].id;
    }
}
