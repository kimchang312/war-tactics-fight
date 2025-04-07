using static EventManager;

public class DeepNightTemptation : IEventRewardHandler
{
    // 선택지에 따른 보상 처리
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        int morale = RogueLikeData.Instance.GetMorale();

        switch (choice)
        {
            case 0: // 잠깐 눈을 붙인다 → 사기 회복 (소)
                RogueLikeData.Instance.SetMorale(morale + 20);
                return "조금 눈을 붙였습니다. 사기가 약간 회복됩니다.";

            case 1: // 깊이 잠든다 → 사기 회복 (중), 50% 확률로 일반 몬스터 전투
                RogueLikeData.Instance.SetMorale(morale + 40);

                if (UnityEngine.Random.value < 0.5f)
                {
                    // 전투 발생 처리 필요 (이벤트 외부에서 제어하는 방식 권장)
                    return "깊이 잠들었습니다. 사기가 크게 회복되었지만, 몬스터에게 습격당했습니다! 반 구현";
                }
                else
                {
                    return "푹 자고 일어났습니다. 사기가 크게 회복되었습니다.";
                }

            case 2: // 긴장을 놓지 않는다
                return "긴장을 늦추지 않았습니다. 아무 일도 일어나지 않았습니다.";

            default:
                return "잘못된 선택입니다.";
        }
    }
}
