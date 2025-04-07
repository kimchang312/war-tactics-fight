using System;
using static EventManager;

public class ReorganizeForces : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0: // 신병 모집 (희귀도 1)
                return "[텍스트 처리] 희귀도 1 등급의 무작위 유닛 1명을 모집했습니다.";

            case 1: // 전직 (선택 유닛)
                return $"[텍스트 처리] 유닛 '{unit.unitName}'이 전직 기회를 얻었습니다.";

            default: // 보급 → 금화 소, 사기 소
                {
                    int gainedGold = UnityEngine.Random.Range(50, 101);
                    RogueLikeData.Instance.AddGoldByEventChapter(gainedGold);

                    int morale = RogueLikeData.Instance.GetMorale();
                    RogueLikeData.Instance.SetMorale(Math.Min(100, morale + 20));

                    return $"보급품을 확보했습니다. 금화 {gainedGold} 획득, 사기 +20 회복.";
                }
        }
    }
}
