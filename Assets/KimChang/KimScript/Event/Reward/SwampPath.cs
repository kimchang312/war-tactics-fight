using System.Linq;
using static EventManager;

public class SwampPath : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();

        // 조건: 기력 2 이상 유닛이 최소 1명 있어야 함
        bool hasAvailable = myUnits.Any(u => u.energy >= 2);
        if (!hasAvailable)
        {
            return "기력이 충분한 병사가 없어 선택지를 실행할 수 없습니다.";
        }

        switch (choice)
        {
            case 0: // 늪지대를 통과 → 늪지 전장 효과
                // 실제 전장 효과 지정은 전투 시스템과 연동 필요
                return "[전투 설정] 다음 전투에서 '늪지대' 전장 효과가 적용됩니다.";

            case 1: // 우회로 → 평원 전장 효과 + 유닛 기력 감소
                {
                    var candidates = myUnits.Where(u => u.energy >= 2).ToList();
                    var target = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                    target.energy = 1;

                    return $"[전투 설정] 다음 전투에서 '평원' 전장 효과가 적용됩니다.\n'{target.unitName}'이(가) 부상을 입어 기력이 1이 되었습니다.";
                }

            default: // 무시
                return "우리는 늪지대 인근을 조용히 지나쳤습니다.";
        }
    }

    public bool CanAppear()
    {
        return RogueLikeData.Instance.GetMyUnits().Any(unit => unit.energy >= 2);
    }
}
