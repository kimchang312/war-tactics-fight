using System.Linq;
using static EventManager;

public class ValleyOfSouls : IEventRewardHandler
{

    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0: // 영혼을 바친다
                {
                    
                        return "더 이상 받을 보석 유산이 없습니다.";
                    
                }

            default: // 거절
                return "계곡의 속삭임을 무시하고 자리를 떴습니다.";
        }
    }
}
