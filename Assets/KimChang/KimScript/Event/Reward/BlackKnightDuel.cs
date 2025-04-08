using System.Collections.Generic;
using System.Linq;
using static EventManager;
using static RelicManager;

public class BlackKnightDuel : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();

        switch (choice)
        {
            case 0: // 1:1 결투
                {
                    float chance = unit.rarity switch
                    {
                        1 => 0.25f,
                        2 => 0.5f,
                        3 => 0.75f,
                        4 => 0.9f,
                        _ => 0.0f
                    };

                    if (UnityEngine.Random.value < chance)
                    {
                        int grade = UnityEngine.Random.value < 0.8f ? 1 : 10;
                        var relic = HandleRandomRelic(grade, RelicAction.Acquire);
                        return $"'{unit.unitName}'이(가) 검은 기사를 쓰러뜨렸습니다! 엘리트 유산 '{relic.name}'을 획득했습니다.";
                    }
                    else
                    {
                        myUnits.Remove(unit);
                        RogueLikeData.Instance.SetAllMyUnits(myUnits);
                        return $"'{unit.unitName}'이(가) 검은 기사에게 패배했습니다... 유닛은 전사했습니다.";
                    }
                }

            case 1: // 다구리 + 유닛 3명 전직 + 저주 유산
                {
                    var random = new System.Random();
                    var selectedUnits = myUnits.OrderBy(_ => random.Next()).Take(3).ToList();
                    List<string> resultLines = new();

                    resultLines.Add("검은 기사를 전력으로 제압했습니다. 다음 유닛들이 전직했습니다:");

                    foreach (var u in selectedUnits)
                    {
                        int newRarity = RogueUnitDataBase.RollPromotion(u.rarity);
                        var pool = GoogleSheetLoader.Instance.GetAllUnitsAsObject()
                                     .Where(x => x.rarity == newRarity).ToList();

                        if (pool.Count > 0)
                        {
                            var picked = pool[UnityEngine.Random.Range(0, pool.Count)];
                            var newUnit = RogueUnitDataBase.ConvertToUnitDataBase(
                                GoogleSheetLoader.Instance.GetRowUnitData(picked.idx));

                            int idx = myUnits.IndexOf(u);
                            if (idx >= 0)
                            {
                                myUnits[idx] = newUnit;
                                resultLines.Add($"- {u.unitName} → {newUnit.unitName} (희귀도 {u.rarity} → {newUnit.rarity})");
                            }
                        }
                        else
                        {
                            resultLines.Add($"- {u.unitName}는 전직에 실패했습니다 (전직 대상 없음)");
                        }
                    }

                    RogueLikeData.Instance.SetAllMyUnits(myUnits);

                    var cursed = HandleRandomRelic(0, RelicAction.Acquire);
                    resultLines.Add($"\n전쟁 유산(저주) '{cursed.name}'을 획득했습니다.");

                    return string.Join("\n", resultLines);
                }

            default:
                return "결투 요청을 무시했습니다.";
        }
    }
}
