using System.Linq;
using static EventManager;

public class CursedGarrison : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        var candidates = myUnits.Where(u => u.rarity == 1 || u.rarity == 2).ToList();

        if (candidates.Count < 5)
        {
            return "희귀도 1~2 유닛이 부족하여 저주가 퍼지지 않았습니다.";
        }

        switch (choice)
        {
            case 0: // 병사들이 저주받음 → 전직 텍스트만
                {
                    var random = new System.Random();
                    var selected = candidates.OrderBy(_ => random.Next()).Take(5).ToList();

                    string result = "[텍스트 처리] 다음 유닛들이 저주받은 기사로 전직되었습니다:\n";
                    foreach (var u in selected)
                    {
                        result += $"- {u.unitName}\n";
                    }
                    return result;
                }

            default: // 전력 도주
                return "병사들은 폐허를 피해 무사히 벗어났습니다.";
        }
    }
}
