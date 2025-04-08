using System;
using static EventManager;
using static RelicManager;

public class ForbiddenRitual : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0: // 의식을 완수한다 → 저주 유산 + 희생 + 확률로 영웅 유닛
                {
                    var cursed = RelicManager.HandleRandomRelic(grade: 0, action: RelicAction.Acquire);

                    float chance = unit.rarity switch
                    {
                        1 => 0.3f,
                        2 => 0.6f,
                        3 => 1.0f,
                        _ => 0f
                    };

                    string result = $"[텍스트 처리] '{unit.unitName}'을(를) 의식에 희생했습니다.\n저주 유산 '{cursed.name}'을 획득했습니다.";

                    if (UnityEngine.Random.value < chance)
                    {
                        result += $"\n[텍스트 처리] 의식의 힘으로 무작위 영웅 유닛이 소환되었습니다. (희귀도 10)";
                    }

                    return result;
                }

            default: // 정화 → 사기 + 기력 회복
                {
                    int morale = RogueLikeData.Instance.GetMorale();
                    RogueLikeData.Instance.SetMorale(Math.Min(100, morale + 20));

                    unit.energy = unit.maxEnergy;

                    return $"당신은 제단을 정화했습니다. 사기 +20, '{unit.unitName}'의 기력이 완전히 회복되었습니다.";
                }
        }
    }
}
