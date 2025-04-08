using System;
using UnityEngine;
using static EventManager;
using static RelicManager;

public class CavalryRace : IEventRewardHandler
{
    public string GetReward(int choice, RogueUnitDataBase unit)
    {
        int gold = RogueLikeData.Instance.GetCurrentGold();
        if (gold < 100)
            return "금화가 부족해 경기에 참여할 수 없습니다.";

        var rand = UnityEngine.Random.value;

        switch (choice)
        {
            case 0: // 챔피언에 배팅 → 75% 확률로 금화(중)
                RogueLikeData.Instance.SetCurrentGold(gold - 100);
                if (rand < 0.75f)
                {
                    int reward = RogueLikeData.Instance.AddGoldByEventChapter(150);
                    return $"챔피언이 승리했습니다! 금화 {reward}를 획득했습니다.";
                }
                else
                {
                    return "예상과 달리 챔피언이 패배했습니다... 금화를 잃었습니다.";
                }

            case 1: // 유망주에 배팅 → 25% 확률로 금화(대)
                RogueLikeData.Instance.SetCurrentGold(gold - 100);
                if (rand < 0.25f)
                {
                    int reward = RogueLikeData.Instance.AddGoldByEventChapter(250);
                    return $"유망주가 깜짝 우승했습니다! 금화 {reward}를 획득했습니다.";
                }
                else
                {
                    return "유망주의 패배로 금화를 잃었습니다.";
                }

            case 2: // 우리 병사 출전 → 기동력 기반 확률, 기력 -1
                RogueLikeData.Instance.SetCurrentGold(gold - 100);
                unit.energy = Math.Max(1, unit.energy - 1);

                float winChance = unit.mobility >= 12 ? 1f : unit.mobility * 0.09f;

                if (UnityEngine.Random.value < winChance)
                {
                    var relic = RelicManager.HandleRandomRelic(10, RelicAction.Acquire);
                    return $"'{unit.unitName}'이(가) 우승했습니다! 엘리트 보상 유산 '{relic.name}'을 획득했습니다.";
                }
                else
                {
                    return $"'{unit.unitName}'이(가) 분투했지만 우승하지 못했습니다. 기력이 1 감소했습니다.";
                }

            default: // 구경 → 사기 회복(소)
                int morale = RogueLikeData.Instance.GetMorale();
                RogueLikeData.Instance.SetMorale(Math.Min(100, morale + 20));
                return "즐겁게 구경하며 긴장을 풀었습니다. (사기 +20)";
        }
    }

    public bool CanAppear()
    {
        return RogueLikeData.Instance.GetCurrentGold() >= 100;
    }
}
