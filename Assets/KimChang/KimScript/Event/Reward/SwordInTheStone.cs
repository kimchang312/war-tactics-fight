using System.Linq;
using static EventManager;
using static RelicManager;

public class SwordInTheStone : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();

        switch (choice)
        {
            case 0: // 기도 → 전직 텍스트 처리
                return $"[텍스트 처리] 유닛 '{unit.unitName}'이 신성한 기운을 받아 전직의 기회를 얻었습니다.";

            case 1: // 검 뽑기 → 공격력 조건 + 엘리트 유산
                {
                    bool hasStrongUnit = myUnits.Any(u => u.attackDamage >= 90);
                    if (!hasStrongUnit)
                        return "검은 요지부동입니다. 병사 중 누구도 충분한 힘을 지니지 못했습니다.";

                    var relic = RelicManager.HandleRandomRelic(grade: 2, action: RelicAction.Acquire);
                    if (relic == null)
                        return "검을 뽑아냈지만 새로운 유산은 얻지 못했습니다.";

                    return $"검을 뽑아냈습니다. 엘리트 등급 유산 '{relic.name}'을 획득했습니다.";
                }

            default: // 그냥 떠남
                return "병사들은 검을 잠시 바라보다가 자리를 떴습니다.";
        }
    }
}
