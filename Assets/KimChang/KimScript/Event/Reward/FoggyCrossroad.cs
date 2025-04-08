using System.Collections.Generic;
using static EventManager;

public class FoggyCrossroad : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        int morale = RogueLikeData.Instance.GetMorale();

        if (morale < 25)
            return "사기가 부족해 안개의 길을 제대로 인식할 수 없습니다.";

        switch (choice)
        {
            case 0: // 힘의 길 → 선택 유닛 전직 + 사기 -5 + 반복
                {
                    RogueLikeData.Instance.SetMorale(morale - 5);

                    var myUnits = RogueLikeData.Instance.GetMyUnits();
                    int index = myUnits.IndexOf(unit);
                    if (index < 0)
                        return "전직하려는 유닛을 부대에서 찾을 수 없습니다.";

                    var promoted = RogueUnitDataBase.RandomUnitReForm(new List<RogueUnitDataBase> { unit });
                    if (promoted == null)
                        return $"'{unit.unitName}'은(는) 전직할 수 없습니다. (사기 -5)\n[이벤트 반복] 다시 갈림길이 나타납니다...";

                    var newUnit = promoted;

                    // 부대 유닛 교체
                    myUnits[index] = newUnit;
                    RogueLikeData.Instance.SetAllMyUnits(myUnits);

                    return $"'{unit.unitName}'이(가) 전직의 길을 걸었습니다 → '{newUnit.unitName}'으로 전직했습니다. (사기 -5)\n[이벤트 반복] 다시 갈림길이 나타납니다...";
                }

            case 1: // 재물의 길 → 금화(소), 사기 -5 + 반복
                int gold = UnityEngine.Random.Range(50, 101);
                RogueLikeData.Instance.AddGoldByEventChapter(gold);
                RogueLikeData.Instance.SetMorale(morale - 5);
                return $"금화 {gold}를 얻었지만 마음이 무거워졌습니다. (사기 -5)\n[이벤트 반복] 안개는 아직 걷히지 않았습니다...";

            default: // 되돌아감 → 사기 -5 + 종료
                RogueLikeData.Instance.SetMorale(morale - 5);
                return "사기를 잃으며 길을 되돌아갔습니다. 더 이상 갈림길은 나타나지 않습니다. (사기 -5)";
        }
    }

    public bool CanAppear()
    {
        return RogueLikeData.Instance.GetMorale() >= 25;
    }
}
