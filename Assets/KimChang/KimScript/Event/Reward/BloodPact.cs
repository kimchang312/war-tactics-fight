using static EventManager;

public class BloodPact : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();

        switch (choice)
        {
            case 0: // 서약을 맺는다 → 무작위 희귀도 2~3 유닛 희생, 전설 등급 전쟁 유산
                {
                    // 희귀도 2~3 유닛 필터
                    var candidates = myUnits.FindAll(u => u.rarity == 2 || u.rarity == 3);
                    if (candidates.Count == 0)
                        return "희귀도 2 또는 3 유닛이 없어 서약을 맺을 수 없습니다.";

                    var random = new System.Random();
                    var victim = candidates[random.Next(candidates.Count)];

                    // 실제 희생은 미구현 상태 (유닛 제거 기능 없음)
                    return $"[반 구현] 유닛 '{victim.unitName}'을 희생한 것으로 처리합니다. 전설 전쟁 유산을 획득했습니다.";
                }

            case 1: // 피를 바친다 → 무작위 유닛의 기력 1로 설정, 일반 전쟁 유산 획득
                {
                    // 사기가 1이 아닌 유닛만 대상
                    var candidates = myUnits.FindAll(u => u.energy > 1);
                    if (candidates.Count == 0)
                        return "기력이 1 초과인 유닛이 없어 피를 바칠 수 없습니다.";

                    var random = new System.Random();
                    var target = candidates[random.Next(candidates.Count)];
                    target.energy = 1;

                    RogueLikeData.Instance.AllMyUnits(myUnits); // 갱신

                    // 전쟁 유산 획득 (일반 등급, 중복 제거 포함)
                    int relicId = GetRandomRelicId(grade: 1);
                    RogueLikeData.Instance.AcquireRelic(relicId);
                    return $"유닛 '{target.unitName}'의 기력이 1로 줄었습니다. 일반 등급 전쟁 유산을 획득했습니다.";
                }

            case 2: // 무시
                return "그 자를 외면하고 떠났습니다.";

            default:
                return "잘못된 선택입니다.";
        }
    }

    private int GetRandomRelicId(int grade)
    {
        var allRelics = WarRelicDatabase.relics;
        var ownedIds = RogueLikeData.Instance.GetAllOwnedRelicIds();

        var candidates = allRelics.FindAll(r => r.grade == grade && !ownedIds.Contains(r.id));
        if (candidates.Count == 0)
        {
            candidates = allRelics.FindAll(r => r.grade == grade); // 중복 허용
        }

        var random = new System.Random();
        return candidates[random.Next(candidates.Count)].id;
    }
}
