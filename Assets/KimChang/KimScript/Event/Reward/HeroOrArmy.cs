using static EventManager;

public class HeroOrArmy : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0: // 영웅 선택 → 희귀도 1 유닛만 가능
                if (unit.rarity != 1)
                    return $"'{unit.unitName}'은 희귀도 1이 아니므로 영웅으로 각성할 수 없습니다.";

                return $"[텍스트 처리] '{unit.unitName}'이(가) 전장의 의지를 받아 영웅 유닛으로 각성했습니다. (희귀도 4)";

            case 1: // 훈련된 군대 → 무작위 병종 강화
                return "[텍스트 처리] 무작위 병종의 병사들이 전술 훈련을 통해 강화되었습니다.";

            default: // 아무 일 없음
                return "승리를 향한 고민은 잠시 뒤로 하고 발걸음을 옮겼습니다.";
        }
    }
}
