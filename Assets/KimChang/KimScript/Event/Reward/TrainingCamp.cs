using System.Linq;
using static EventManager;

public class TrainingCamp : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();

        if (myUnits.Count == 0)
            return "참가할 병사가 없어 훈련에 참가하지 못했습니다.";

        switch (choice)
        {
            case 0: // 병종 무작위 강화
                {
                    return $"[미 구현] 병종의 유닛이을 강화받았습니다.";
                }

            case 1: // 무작위 2명 전직 (반 구현)
                {
                    return "[미 구현] 아래 유닛들이 전직되었습니다:\n";
                }

            default: // 무시
                return "우리는 훈련소를 지나쳤습니다.";
        }
    }
}
