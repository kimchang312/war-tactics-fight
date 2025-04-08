using static EventManager;

public class VillageTroublemakers : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0: // 무력 제압 → 전투 발생
                return "[전투 발생] 골칫덩이들과의 전투가 시작됩니다! 승리 시 희귀도 1~2 유닛 3명을 영입합니다. [텍스트 처리]";

            case 1: // 금화로 회유
                {
                    int gold = RogueLikeData.Instance.GetCurrentGold();
                    if (gold < 300)
                        return "금화가 부족하여 이들을 회유할 수 없습니다.";

                    RogueLikeData.Instance.SetCurrentGold(gold - 300);

                    string result = "[텍스트 처리] 다음 병사들이 회유되어 부대에 합류했습니다:\n";
                    for (int i = 0; i < 3; i++)
                    {
                        int rarity = UnityEngine.Random.value < 0.5f ? 1 : 2;
                        result += $"- 희귀도 {rarity} 유닛\n";
                    }

                    return $"금화 300을 지불하고 병사들을 포섭했습니다.\n{result.TrimEnd()}";
                }

            default: // 무시
                return "당신은 그들을 무시하고 마을을 떠났습니다.";
        }
    }
}
