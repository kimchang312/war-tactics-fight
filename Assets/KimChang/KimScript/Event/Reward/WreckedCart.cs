using static EventManager;

public class WreckedCart : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0: // 조심히 뒤져본다 → 50% 엘리트 유산, 50% 엘리트 전투
                {
                    if (UnityEngine.Random.value < 0.5f)
                    {
                        return "잔해 속에서 귀중한 물건을 발견했습니다. 엘리트 등급 전쟁 유산을 획득했습니다.";
                    }
                    else
                    {
                        // 실제 전투는 외부 호출로 처리
                        return "[전투 발생] 잔해를 뒤지던 중 엘리트 몬스터의 매복을 받았습니다!";
                    }
                }

            default: // 자리를 뜬다
                return "불길한 느낌이 들어 잔해를 건드리지 않고 자리를 떴습니다.";
        }
    }

}
