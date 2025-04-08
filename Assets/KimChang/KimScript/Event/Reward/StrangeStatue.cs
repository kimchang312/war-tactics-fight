using static EventManager;

public class StrangeStatue : IEventRewardHandler
{
    private const string targetRelicName = "기이한 조각";

    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0: // 유산 획득 + 유닛 기력 1로
                {
                    var myUnits = RogueLikeData.Instance.GetMyUnits();
                    if (myUnits.Count == 0)
                        return "조각을 가져올 병사가 없습니다.";

                    // 무작위 유닛 1명
                    var selected = myUnits[UnityEngine.Random.Range(0, myUnits.Count)];
                    selected.energy = 1;

                    RogueLikeData.Instance.SetAllMyUnits(myUnits);

                    var relic = WarRelicDatabase.relics.Find(r => r.name == targetRelicName);
                    if (relic != null)
                        RogueLikeData.Instance.AcquireRelic(relic.id);

                    return $"[전쟁 유산 획득] '{targetRelicName}'을 획득했습니다.\n'{selected.unitName}'은 조각의 영향으로 기력이 1이 되었습니다.";
                }

            default: // 무시
                return "기묘한 조각을 지나쳐 조용히 발걸음을 옮겼습니다.";
        }
    }

}
