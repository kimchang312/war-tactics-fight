using System;
using System.Linq;
using static EventManager;

public class CrazyKnight : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        int gold = RogueLikeData.Instance.GetCurrentGold();
        int morale = RogueLikeData.Instance.GetMorale();

        switch (choice)
        {
            case 0: // 병사 전직 (무작위 유닛 1명)
                if (gold < 100)
                    return "금화가 부족하여 훈련을 맡길 수 없습니다.";

                RogueLikeData.Instance.SetCurrentGold(gold - 100);

                var myUnits = RogueLikeData.Instance.GetMyUnits();
                var promotable = myUnits.Where(u => u.rarity < 4).ToList();
                if (promotable.Count == 0)
                    return "전직 가능한 유닛이 없습니다.";

                var target = promotable[UnityEngine.Random.Range(0, promotable.Count)];
                int newRarity = RogueUnitDataBase.RollPromotion(target.rarity);

                var pool = GoogleSheetLoader.Instance.GetAllUnitsAsObject()
                            .Where(u => u.rarity == newRarity).ToList();

                if (pool.Count == 0)
                    return "해당 희귀도에 해당하는 전직 유닛이 존재하지 않아 전직에 실패했습니다.";

                var selected = pool[UnityEngine.Random.Range(0, pool.Count)];
                var newUnit = RogueUnitDataBase.ConvertToUnitDataBase(
                    GoogleSheetLoader.Instance.GetRowUnitData(selected.idx));

                int idx = myUnits.IndexOf(target);
                if (idx >= 0)
                {
                    myUnits[idx] = newUnit;
                    RogueLikeData.Instance.SetAllMyUnits(myUnits);
                }

                return $"'{target.unitName}'이(가) 훈련을 통해 '{newUnit.unitName}'으로 전직했습니다. (희귀도 {target.rarity} → {newUnit.rarity})";

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

    public bool CanAppear()
    {
        int gold = RogueLikeData.Instance.GetCurrentGold();
        int morale = RogueLikeData.Instance.GetMorale();
        return gold>=100 || morale>=6;
    }


}
