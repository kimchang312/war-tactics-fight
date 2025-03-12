using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RewardManager
{
    // 랜덤으로 1개의 보유하지 않은 유산을 반환하는 함수
    public static int GetRandomWarRelicId(int stage = 0)
    {
        // 현재 보유 중인 유산 ID를 가져옴
        List<int> ownedRelicIds = RogueLikeData.Instance.GetAllOwnedRelicIds();

        // 보유하지 않은 유산 중 grade가 1~10인 것만 필터링
        List<WarRelic> availableRelics = WarRelicDatabase.relics
            .Where(relic => !ownedRelicIds.Contains(relic.id) && relic.grade >= 1 && relic.grade <= 10)
            .ToList();

        // 보유하지 않은 유산이 없다면 -1 반환
        if (availableRelics.Count == 0)
        {
            Debug.LogWarning("보유하지 않은 유산이 없습니다.");
            return -1;
        }

        // 랜덤으로 1개의 유산 선택
        System.Random random = new System.Random();
        int index = random.Next(availableRelics.Count);

        return availableRelics[index].id;
    }
}
