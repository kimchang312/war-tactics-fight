using static EventManager;
public class StrangeDream : IEventRewardHandler
{

    public string GetReward(int choice, RogueUnitDataBase unit=null)
    {
        switch (choice)
        {
            case 0:
                //일반 등급 전쟁 유산, 저주 등급 전쟁 유산, 무작위 유닛 전직, 무작위 유닛 기력 1로, 무작위 유닛 획득, 무작위 병종 무작위 강화, 사기 회복(최대), 사기 -25, 금화(중) 중에서 무작위 1
                return $"";
            default:
                return "아무 일도 일어나지 않았습니다.";
        }
    }
}
