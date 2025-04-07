using static EventManager;

public class WanderingMerchant : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();

        switch (choice)
        {
            case 0: // 병사를 고용해 보낸다.
                {
                    if (myUnits.Count < 2)
                        return "보유한 유닛이 부족하여 병사를 보낼 수 없습니다.";

                    var random = new System.Random();
                    var target = myUnits[random.Next(myUnits.Count)];

                    int goldReward = 0;
                    string result = "";

                    switch (target.rarity)
                    {
                       
                        case 1:
                            goldReward = GetGoldByTier("소");
                            result = $"[반 구현] 희귀도 1 유닛 '{target.unitName}'을 고용병으로 보냈습니다. 금화 {goldReward} 획득.";
                            break; /*
                        case 2:
                            goldReward = GetGoldByTier("중");
                            result = $"[반 구현] 희귀도 2 유닛 '{target.unitName}'을 고용병으로 보냈습니다. 금화 {goldReward} 획득.";
                            break;
                        case 3:
                            goldReward = GetGoldByTier("중");
                            int relicId = GetRandomRelicId(grade: 1);
                            RogueLikeData.Instance.AcquireRelic(relicId);
                            result = $"[반 구현] 희귀도 3 유닛 '{target.unitName}'을 고용병으로 보냈습니다. 금화 {goldReward}와 일반 전쟁 유산을 획득했습니다.";
                            break;
                        default:*/
                            return "유효한 희귀도를 가진 유닛이 없습니다.";
                    }

                    RogueLikeData.Instance.AddGoldByEventChapter(goldReward);
                    // 실제 유닛 제거는 미구현
                    return result;
                }

            default: // 제안을 거절한다
                return "상인의 제안을 거절했습니다.";
        }
    }

    // 금화 등급에 따라 값 반환
    private int GetGoldByTier(string size)
    {
        var random = new System.Random();
        return size switch
        {
            "소" => random.Next(50, 101),    // 50~100
            "중" => random.Next(150, 301),   // 150~300
            "대" => random.Next(250, 501),
            _ => 0,
        };
    }

    private int GetRandomRelicId(int grade)
    {
        var allRelics = WarRelicDatabase.relics;
        var ownedIds = RogueLikeData.Instance.GetAllOwnedRelicIds();

        var candidates = allRelics.FindAll(r => r.grade == grade && !ownedIds.Contains(r.id));
        if (candidates.Count == 0)
        {
            candidates = allRelics.FindAll(r => r.grade == grade);
        }

        var random = new System.Random();
        return candidates[random.Next(candidates.Count)].id;
    }
}
