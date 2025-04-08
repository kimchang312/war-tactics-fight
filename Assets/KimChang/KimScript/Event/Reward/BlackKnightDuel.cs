using System.Linq;
using static EventManager;
using static RelicManager;

public class BlackKnightDuel : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();

        switch (choice)
        {
            case 0: // 1:1 결투
                {
                    float chance = unit.rarity switch
                    {
                        1 => 0.25f,
                        2 => 0.5f,
                        3 => 0.75f,
                        4 => 0.9f,
                        _ => 0.0f
                    };

                    if (UnityEngine.Random.value < chance)
                    {
                        // 승리 → 엘리트 보상: 80% 일반 / 20% 전설
                        int grade = UnityEngine.Random.value < 0.8f ? 1 : 10;
                        var relic = RelicManager.HandleRandomRelic(grade, RelicAction.Acquire);
                        return $"'{unit.unitName}'이(가) 검은 기사를 쓰러뜨렸습니다! 엘리트 유산 '{relic.name}'을 획득했습니다.";
                    }
                    else
                    {
                        return $"'{unit.unitName}'이(가) 검은 기사에게 패배했습니다... 유닛은 전사했습니다. [텍스트 처리]";
                    }
                }

            case 1: // 다구리 + 전직 3명 + 저주 유산
                {
                    var random = new System.Random();
                    var selectedUnits = myUnits.OrderBy(_ => random.Next()).Take(3).ToList();

                    string result = "반 구현[텍스트 처리] 다음 유닛들이 전직의 기회를 얻었습니다:\n";
                    foreach (var u in selectedUnits)
                    {
                        int newRarity = RollPromotion(u.rarity);
                        result += $"- {u.unitName}: 희귀도 {u.rarity} → {newRarity}\n";
                    }

                    var cursedRelic = RelicManager.HandleRandomRelic(0, RelicAction.Acquire);
                    result += $"\n전쟁 유산(저주) '{cursedRelic.name}'을 획득했습니다.";
                    return result;
                }

            default:
                return "결투 요청을 무시했습니다.";
        }
    }

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
