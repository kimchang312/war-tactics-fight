using System;
using System.Collections.Generic;
using System.Linq;
using static EventManager;
using static RelicManager;

public class ForbiddenRitual : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        switch (choice)
        {
            case 0: // 의식을 완수한다 → 저주 유산 + 희생 + 확률로 영웅 유닛
                {
                    var cursed = RelicManager.HandleRandomRelic(grade: 0, action: RelicAction.Acquire);

                    float chance = unit.rarity switch
                    {
                        1 => 0.3f,
                        2 => 0.6f,
                        3 => 1.0f,
                        _ => 0f
                    };

                    string result = $"'{unit.unitName}'을(를) 의식에 희생했습니다.\n저주 유산 '{cursed.name}'을 획득했습니다.";

                    if (UnityEngine.Random.value < chance)
                    {
                        // 이미 보유한 영웅 유닛 idx 수집
                        var myUnits = RogueLikeData.Instance.GetMyUnits();
                        var ownedHeroIdx = new HashSet<int>(myUnits.Where(u => u.idx >= 52 && u.idx <= 66).Select(u => u.idx));

                        // 전체 영웅 유닛 후보에서 미보유 유닛 필터링
                        var heroCandidates = GoogleSheetLoader.Instance.GetAllUnitsAsObject()
                            .Where(u => u.rarity == 4 && !ownedHeroIdx.Contains(u.idx))
                            .ToList();

                        if (heroCandidates.Count > 0)
                        {
                            var selected = heroCandidates[UnityEngine.Random.Range(0, heroCandidates.Count)];
                            var row = GoogleSheetLoader.Instance.GetRowUnitData(selected.idx);
                            if (row != null)
                            {
                                var heroUnit = RogueUnitDataBase.ConvertToUnitDataBase(row);
                                myUnits.Add(heroUnit);
                                RogueLikeData.Instance.SetAllMyUnits(myUnits);
                                result += $"\n[텍스트 처리] 의식의 힘으로 무작위 영웅 유닛 '{heroUnit.unitName}'이(가) 소환되었습니다.";
                            }
                        }
                        else
                        {
                            result += "\n[텍스트 처리] 이미 모든 영웅 유닛을 보유하고 있어 아무 일도 일어나지 않았습니다.";
                        }
                    }

                    return result;
                }

            default: // 정화 → 사기 + 기력 회복
                {
                    int morale = RogueLikeData.Instance.GetMorale();
                    RogueLikeData.Instance.SetMorale(Math.Min(100, morale + 20));

                    unit.energy = unit.maxEnergy;

                    return $"당신은 제단을 정화했습니다. 사기 +20, '{unit.unitName}'의 기력이 완전히 회복되었습니다.";
                }
        }
    }
}
