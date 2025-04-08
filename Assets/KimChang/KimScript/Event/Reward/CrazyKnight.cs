using System;
using static EventManager;

public class CrazyKnight : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        int gold = RogueLikeData.Instance.GetCurrentGold();
        int morale = RogueLikeData.Instance.GetMorale();

        switch (choice)
        {
            case 0: // 병사 전직
                if (gold < 100)
                    return "금화가 부족하여 훈련을 맡길 수 없습니다.";

                RogueLikeData.Instance.SetCurrentGold(gold - 100);
                int newRarity = RollPromotion(unit.rarity);
                return $"[텍스트 처리] '{unit.unitName}'이(가) 훈련을 받고 희귀도 {unit.rarity} → {newRarity}로 전직했습니다.";

            case 1: // 병종 강화
                if (gold < 100)
                    return "금화가 부족하여 훈련을 맡길 수 없습니다.";

                RogueLikeData.Instance.SetCurrentGold(gold - 100);
                return "[텍스트 처리] 부대 전체가 훈련을 받아 무작위 병종이 강화되었습니다.";

            default: // 걷어참 → 사기 -5
                RogueLikeData.Instance.SetMorale(Math.Max(0, morale - 5));
                return "정신 나간 기사를 밀쳐내고 자리를 떴습니다. (사기 -5)";
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
