using System;
using static EventManager;

public class FoodShortage : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        var random = new System.Random();

        switch (choice)
        {
            case 0: // 누명을 씌움 → 무작위 유닛 희생 + 사기 회복 최대
                {
                    if (myUnits.Count == 0)
                        return "희생시킬 병사가 없어 누명을 씌울 수 없었습니다.";

                    var selected = myUnits[random.Next(myUnits.Count)];
                    RogueLikeData.Instance.SetMorale(100);
                    myUnits.Remove(selected);
                    RogueLikeData.Instance.SetAllMyUnits(myUnits);
                    return $"[텍스트 처리] '{selected.unitName}'에게 누명을 씌워 희생시켰습니다. 사기가 완전히 회복되었습니다.";
                }

            case 1: // 배급 감소 → 사기 -40, 금화(대)
                {
                    int gainedGold = RogueLikeData.Instance.AddGoldByEventChapter(250);

                    return $"식량을 줄이는 대신 예산을 확보했습니다. 금화 {gainedGold} 획득, 사기 -40.";
                }

            case 2: // 병사들과 함께 굶음 → 기력 -1, 강화 2회 (텍스트)
                {
                    foreach (var u in myUnits)
                        u.energy = Math.Max(1, u.energy - 1);
                    return "[텍스트 처리] 병사들과 함께 굶으며 결속을 다졌습니다. 모든 유닛 기력이 1 감소했습니다.\n무작위 병종의 강화가 2회 발생했습니다.";
                }

            default: // 금으로 해결
                {
                    int gold = RogueLikeData.Instance.GetCurrentGold();
                    RogueLikeData.Instance.SetCurrentGold(Math.Max(0, gold - 500));
                    return "충분한 금화를 투입해 위기를 넘겼습니다. (금화 -500)";
                }
        }
    }
}
