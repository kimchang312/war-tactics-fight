using System;
using System.Linq;
using static EventManager;

public class ReorganizeForces : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0: // 신병 모집 (희귀도 1)
                {
                    var pool = GoogleSheetLoader.Instance.GetAllUnitsAsObject()
                        .Where(u => u.rarity == 1)
                        .ToList();

                    if (pool.Count == 0)
                        return "모집 가능한 희귀도 1 유닛이 없습니다.";

                    var selected = pool[UnityEngine.Random.Range(0, pool.Count)];
                    var row = GoogleSheetLoader.Instance.GetRowUnitData(selected.idx);
                    if (row == null)
                        return "유닛 데이터를 불러오지 못했습니다.";

                    var newUnit = RogueUnitDataBase.ConvertToUnitDataBase(row);
                    RogueLikeData.Instance.SetAddMyUnis(newUnit);

                    return $"무작위로 '{newUnit.unitName}' 유닛을 모집했습니다.";
                }

            case 1: // 전직 (선택 유닛)
                {
                    if (unit.rarity >= 4)
                        return $"'{unit.unitName}'은 희귀도가 너무 높아 전직할 수 없습니다.";

                    var newUnit = RogueUnitDataBase.RandomUnitReForm(unit);

                    // 기존 유닛 제거
                    var myUnits = RogueLikeData.Instance.GetMyUnits();
                    myUnits.Remove(newUnit);

                    // 전직 유닛 추가
                    RogueLikeData.Instance.SetAddMyUnis(newUnit);

                    return $"'{unit.unitName}'이(가) '{newUnit.unitName}'으로 전직했습니다.";
                }

            default: // 보급 → 금화 소, 사기 소
                {
                    int gold = RogueLikeData.Instance.AddGoldByEventChapter(50);
                    int morale = RogueLikeData.Instance.GetMorale();
                    RogueLikeData.Instance.SetMorale(Math.Min(100, morale + 20));

                    return $"보급품을 확보했습니다. 금화 {gold} 획득, 사기 +20 회복.";
                }
        }
    }
}
