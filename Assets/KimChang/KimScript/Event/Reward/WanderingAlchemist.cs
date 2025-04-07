using static EventManager;
using static RelicManager;

public class WanderingAlchemist : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        int gold = RogueLikeData.Instance.GetCurrentGold();

        if (gold < 200)
            return "금화가 부족해 물약을 구매할 수 없습니다.";

        switch (choice)
        {
            case 0: // 강화 물약 → 금화 -200, 무작위 병종 강화 2회 (텍스트만)
                RogueLikeData.Instance.SetCurrentGold(gold - 200);
                return "[텍스트 처리] 무작위 병종의 무작위 강화가 2회 적용되었습니다.";

            case 1: // 기력 물약 → 금화 -200, 선택 유닛 기력 회복(대)
                RogueLikeData.Instance.SetCurrentGold(gold - 200);
                unit.energy = unit.maxEnergy;
                return $"'{unit.unitName}'이 기력 물약을 마시고 기력을 완전히 회복했습니다.";

            case 2: // 해주 물약 → 금화 -200, 저주 유산 제거 1개
                RogueLikeData.Instance.SetCurrentGold(gold - 200);
                var removedName = RelicManager.HandleRandomRelic(grade: 0, action: RelicAction.Remove)?.name;

                if (removedName != null)
                    return $"해주 물약을 사용해 저주 유산 '{removedName}'을 제거했습니다.";
                else
                    return "저주 유산이 없어 해주 물약이 헛되이 사용되었습니다.";

            default: // 필요 없다
                return "연금술사의 제안을 거절했습니다.";
        }
    }
}
