using System;
using static EventManager;
using static RelicManager;

public class CollapsingTemple : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();

        switch (choice)
        {
            case 0: // 성물을 들고 달아난다 → 엘리트 유산 + 저주 유산
                {
                    int eliteGrade = UnityEngine.Random.value < 0.8f ? 1 : 10;
                    var elite = HandleRandomRelic(eliteGrade, RelicAction.Acquire);
                    var curse = HandleRandomRelic(0, RelicAction.Acquire);

                    return $"무너지는 사원에서 성물을 들고 달아났습니다.\n엘리트 유산 '{elite.name}'과 저주 유산 '{curse.name}'을 획득했습니다.";
                }

            case 1: // 병사 희생 → 엘리트 유산 + 무작위 유닛 기력 1
                {
                    int eliteGrade = UnityEngine.Random.value < 0.8f ? 1 : 10;
                    var elite = HandleRandomRelic(eliteGrade, RelicAction.Acquire);

                    var candidates = myUnits.FindAll(u => u.energy > 1);
                    if (candidates.Count > 0)
                    {
                        var target = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                        target.energy = 1;
                        return $"병사의 희생으로 탐색에 성공했습니다. '{target.unitName}'의 기력이 1이 되었습니다.\n엘리트 유산 '{elite.name}'을 획득했습니다.";
                    }

                    return $"기력이 충분한 병사가 없어 희생은 발생하지 않았습니다.\n엘리트 유산 '{elite.name}'을 획득했습니다.";
                }

            case 2: // 조심히 접근 → 엘리트 유산 + 사기 -30
                {
                    int eliteGrade = UnityEngine.Random.value < 0.8f ? 1 : 10;
                    var elite = HandleRandomRelic(eliteGrade, RelicAction.Acquire);

                    int morale = RogueLikeData.Instance.GetMorale();
                    RogueLikeData.Instance.SetMorale(Math.Max(0, morale - 30));

                    return $"잔해를 조심히 치우며 접근했습니다. 엘리트 유산 '{elite.name}'을 획득했지만 사기가 30 감소했습니다.";
                }

            default: // 위험 회피
                return "위험을 감수하지 않고 사원을 떠났습니다.";
        }
    }
}
