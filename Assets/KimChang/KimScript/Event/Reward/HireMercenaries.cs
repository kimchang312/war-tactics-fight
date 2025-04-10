using System.Collections.Generic;
using System.Linq;
using static EventManager;

public class HireMercenaries : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        int gold = RogueLikeData.Instance.GetCurrentGold();

        var allUnits = GoogleSheetLoader.Instance.GetAllUnitsAsObject();
        var candidates = allUnits.Where(u => u.rarity == 2 || u.rarity == 3).ToList();

        switch (choice)
        {
            case 0: // 전체 고용 → 금화 -300, 유닛 3명
                if (gold < 300)
                    return "금화가 부족하여 용병단 전체를 고용할 수 없습니다.";

                RogueLikeData.Instance.SetCurrentGold(gold - 300);

                var hired3 = new List<string>();
                for (int i = 0; i < 3; i++)
                {
                    var pick = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                    var row = GoogleSheetLoader.Instance.GetRowUnitData(pick.idx);
                    var hired = RogueUnitDataBase.ConvertToUnitDataBase(row);
                    RogueLikeData.Instance.SetAddMyUnis(hired);
                    hired3.Add(hired.unitName);
                }

                return $"용병단 전체를 고용했습니다.\n합류한 병사: {string.Join(", ", hired3)}";

            case 1: // 개별 고용 → 금화 -100, 유닛 1명
                RogueLikeData.Instance.SetCurrentGold(gold - 100);

                var onePick = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                var oneRow = GoogleSheetLoader.Instance.GetRowUnitData(onePick.idx);
                var oneUnit = RogueUnitDataBase.ConvertToUnitDataBase(oneRow);
                RogueLikeData.Instance.SetAddMyUnis(oneUnit);
                return $"용병 1명을 고용했습니다: {oneUnit.unitName}";

            default: // 거절
                return "용병들의 제안을 거절하고 야영지를 떠났습니다.";
        }
    }

    public bool CanAppear()
    {
        return RogueLikeData.Instance.GetCurrentGold() >= 100;
    }
}
