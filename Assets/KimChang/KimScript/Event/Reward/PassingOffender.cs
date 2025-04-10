using System.Collections.Generic;
using static EventManager;

public class PassingOffender : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0:
                if (unit.rarity >= 4)
                    return $"{unit.unitName}은 희귀도가 너무 높아 전직할 수 없습니다.";

                // 유닛 전직 실행
                var newUnit = RogueUnitDataBase.RandomUnitReForm(unit);

                // 기존 유닛 제거
                var myUnits = RogueLikeData.Instance.GetMyUnits();
                myUnits.Remove(newUnit);

                // 새 유닛 추가
                RogueLikeData.Instance.SetAddMyUnis(newUnit);

                return $"{unit.unitName}이(가) 전직하여 {newUnit.unitName}으로 거듭났습니다!";
            default:
                return "아무 일도 일어나지 않았습니다.";
        }
    }
}