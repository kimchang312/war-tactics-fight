using System.Linq;
using static EventManager;
using static RelicManager;

public class TwinDemons : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();

        switch (choice)
        {
            case 0: // 돈을 원한다 → 금화 +2000, 저주 유산 3회
                {
                    RogueLikeData.Instance.AddGoldByEventChapter(2000);

                    var result = "악마가 금화를 흘려보냈습니다. 금화 +2000\n";
                    for (int i = 0; i < 3; i++)
                    {
                        var cursed = RelicManager.HandleRandomRelic(0, RelicAction.Acquire);
                        result += $"- 저주 유산 '{cursed.name}' 획득\n";
                    }

                    return result.TrimEnd();
                }

            case 1: // 힘을 원한다 → 희귀도 1 유닛 전직, 영광의 대가 유산
                {
                    var tier1Units = myUnits.Where(u => u.rarity == 1).ToList();

                    if (tier1Units.Count == 0)
                        return "희귀도 1 유닛이 없어 악마의 힘을 받을 자가 없습니다.";

                    string report = "[텍스트 처리] 다음 유닛들이 악마의 힘으로 전직합니다:\n";
                    foreach (var u in tier1Units)
                    {
                        int newRarity = RollPromotion(u.rarity);
                        report += $"- {u.unitName}: 희귀도 1 → {newRarity}\n";
                    }

                    var relic = WarRelicDatabase.relics.Find(r => r.name == "영광의 대가");
                    if (relic != null)
                        RogueLikeData.Instance.AcquireRelic(relic.id);

                    report += $"\n전쟁 유산 '{relic?.name ?? "영광의 대가"}'을 획득했습니다.";
                    return report;
                }

            case 2: // 처단 → 엘리트 전투, 전설 유산
                return "[전투 발생] 두 악마와의 결전을 시작합니다! 승리 시 전설 유산을 획득합니다.";

            default: // 떠남
                return "악마의 유혹을 거부하고 자리를 떴습니다.";
        }
    }

    // 전직 확률 (지정된 규칙 기반)
    private int RollPromotion(int currentRarity)
    {
        float rand = UnityEngine.Random.value;
        return currentRarity switch
        {
            1 => rand < 0.5f ? 1 : (rand < 0.95f ? 2 : 3),
            2 => rand < 0.75f ? 2 : (rand < 0.95f ? 3 : 10),
            3 => rand < 0.9f ? 3 : 10,
            _ => currentRarity
        };
    }

}
