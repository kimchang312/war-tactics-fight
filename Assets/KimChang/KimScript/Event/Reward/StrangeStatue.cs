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

                    unit.energy = 1;
                    unit.endless = true;
                    RogueLikeData.Instance.SetAllMyUnits(myUnits);

                    var relic = WarRelicDatabase.relics.Find(r => r.id == 79);
                    if (relic != null)
                        RogueLikeData.Instance.AcquireRelic(relic.id);

                    return $"{targetRelicName}'을 획득했습니다.\n'{unit.unitName}'은 조각의 영향으로 기력이 1이 되었습니다.";
                }

            default: // 무시
                return "기묘한 조각을 지나쳐 조용히 발걸음을 옮겼습니다.";
        }
    }

    public bool CanAppear()
    {
        return RogueLikeData.Instance.GetOwnedRelicById(79)==null;
    }

}
