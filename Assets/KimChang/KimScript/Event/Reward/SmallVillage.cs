using System;
using System.Collections.Generic;
using static EventManager;

public class SmallVillage : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0: // 주민을 징병한다 → 민병대 창병 3명 획득
                {
                    
                    var row = GoogleSheetLoader.Instance.GetRowUnitData(0);
                    for (int i = 0; i < 3; i++)
                    {
                        var newUnit = RogueUnitDataBase.ConvertToUnitDataBase(row);
                        RogueLikeData.Instance.SetAddMyUnis(newUnit);
                    }
                    return "마을에서 민병대 창병 3명을 징병했습니다.";
                }

            case 1: // 마을을 약탈한다 → 전체 기력 회복(소), 사기 -20
                {
                    var myUnits = RogueLikeData.Instance.GetMyUnits();
                    foreach (var u in myUnits)
                    {
                        u.energy = Math.Min(u.maxEnergy, u.energy + 1);
                    }

                    int morale = RogueLikeData.Instance.GetMorale();
                    RogueLikeData.Instance.SetMorale(Math.Max(0, morale - 20));
                    return "마을을 약탈했습니다. 모든 유닛의 기력이 소폭 회복되었고 사기가 감소했습니다.";
                }

            case 2: // 대접을 요청한다 → 금화 -100, 사기 +20
                {
                    int gold = RogueLikeData.Instance.GetCurrentGold();
                    if (gold < 100)
                        return "금화가 부족하여 대접을 받을 수 없습니다.";

                    RogueLikeData.Instance.SetCurrentGold(gold - 100);
                    int morale = RogueLikeData.Instance.GetMorale();
                    RogueLikeData.Instance.SetMorale(Math.Min(100, morale + 20));
                    return "마을에서 대접을 받고 사기가 회복되었습니다.";
                }

            default: // 무시
                return "마을을 지나쳤습니다. 아무 일도 일어나지 않았습니다.";
        }
    }
 
}
