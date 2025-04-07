using static EventManager;
using static RelicManager;

public class SpiritOfTheSpring : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        // 조건: 일반 등급 유산 1개 이상
        var normalRelics = RogueLikeData.Instance.GetRelicsByGrade(1);
        if (normalRelics.Count == 0)
            return "샘은 잠잠하기만 합니다. 정령이 모습을 드러내지 않았습니다.";

        switch (choice)
        {
            case 0: // 정직하게 대답
                {
                    string toRemove = RelicManager.HandleRandomRelic(1,action: RelicAction.Acquire).name;

                    string toGet = RelicManager.HandleRandomRelic(10, action: RelicAction.Acquire).name;

                    return $"정직하게 대답했습니다. '{toRemove}'을 샘에 바치고 전설 등급 전쟁 유산{toGet}을 얻었습니다.";
                }

            case 1: // 거짓을 말한다
                {
                    string legendaryName = RelicManager.HandleRandomRelic(10, action: RelicAction.Acquire).name;
                    string curseName = RelicManager.HandleRandomRelic(0, action: RelicAction.Acquire).name;

                    return $"거짓을 말했습니다. 정령이 당신에게 전설 유산{legendaryName}과 저주 유산{curseName}을 동시에 안겨주었습니다.";
                }

            default: // 무시
                return "정령에게 아무런 응답도 하지 않았습니다.";
        }
    }

    private int GetRandomRelicId(int grade)
    {
        var allRelics = WarRelicDatabase.relics;
        var ownedIds = RogueLikeData.Instance.GetAllOwnedRelicIds();

        var candidates = allRelics.FindAll(r => r.grade == grade && !ownedIds.Contains(r.id));
        if (candidates.Count == 0)
            candidates = allRelics.FindAll(r => r.grade == grade); // 중복 허용

        var random = new System.Random();
        return candidates[random.Next(candidates.Count)].id;
    }
}
