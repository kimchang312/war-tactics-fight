using System;
using static EventManager;
using static RelicManager;

public class EndlessPleasure : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0: // 모두와 함께 마심 → 사기 최대 + 저주 유산
                {
                    RogueLikeData.Instance.SetMorale(100);
                    string cursed = RelicManager.HandleRandomRelic(0, RelicAction.Acquire).name;
                    return $"군 전체가 향락에 빠졌습니다. 사기가 가득 찼고, 저주 유산 '{cursed}'이 따라붙었습니다.";
                }

            case 1: // 무기에 바름 → 강화 3회 텍스트 + 사기 -30
                {
                    int morale = RogueLikeData.Instance.GetMorale();
                    RogueLikeData.Instance.SetMorale(Math.Max(0, morale - 30));
                    return "[텍스트 처리] 무작위 병종이 3회 강화되었습니다. 사기가 30 감소했습니다.";
                }

            default: // 떠남 → 사기 -5
                {
                    int morale = RogueLikeData.Instance.GetMorale();
                    RogueLikeData.Instance.SetMorale(Math.Max(0, morale - 5));
                    return "끝없는 유혹을 거부했습니다. 사기가 조금 감소했습니다. (사기 -5)";
                }
        }
    }
}
