using static EventManager;

public class TempleOfWar : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0: // 시험 → 엘리트 전투 발생
                return "[전투 발생] 전쟁의 신이 당신을 시험합니다! 승리 시 선택 유닛 전직 5회 진행됩니다. [텍스트 처리]";

            case 1: // 공물 → 금화 -100, 전직 2회
                {
                    int gold = RogueLikeData.Instance.GetCurrentGold();
                    if (gold < 100)
                        return "금화가 부족해 신에게 공물을 바칠 수 없습니다.";

                    RogueLikeData.Instance.SetCurrentGold(gold - 100);

                    string result = $"[텍스트 처리] '{unit.unitName}'이(가) 공물을 바치고 전직의 은총을 받았습니다:\n";
                    for (int i = 0; i < 2; i++)
                    {
                        int newRarity = RollPromotion(unit.rarity);
                        result += $"- 전직 {i + 1}회: 희귀도 {unit.rarity} → {newRarity}\n";
                        unit.rarity = newRarity;
                    }

                    return result;
                }

            case 2: // 기도 → 무작위 병종 강화
                return "[텍스트 처리] 신에게 기도하며 무작위 병종의 전술이 향상되었습니다.";

            default: // 신 무시
                return "신전의 제단을 지나쳐 조용히 발걸음을 옮겼습니다.";
        }
    }

    // 전직 확률
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
