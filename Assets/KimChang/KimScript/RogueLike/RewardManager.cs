using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RewardManager
{
    // 랜덤으로 3개의 보유하지 않은 유산을 반환하는 함수
    public static List<int> GetRandomWarRelicIds(int stage=0)
    {
        // 현재 보유 중인 유산 ID를 가져옴
        List<int> ownedRelicIds = RogueLikeData.Instance.GetAllOwnedRelicIds();

        // 보유하지 않은 유산 중 grade가 1~10인 것만 필터링
        List<WarRelic> availableRelics = WarRelicDatabase.relics
            .Where(relic => !ownedRelicIds.Contains(relic.id) && relic.grade >= 1 && relic.grade <= 10)
            .ToList();

        List<int> selectedRelicIds = new List<int>();

        // 랜덤으로 3개의 유산을 선택
        System.Random random = new System.Random();
        for (int i = 0; i < 3 && availableRelics.Count > 0; i++)
        {
            int index = random.Next(availableRelics.Count);
            selectedRelicIds.Add(availableRelics[index].id);
            availableRelics.RemoveAt(index); // 중복 방지를 위해 제거
        }

        return selectedRelicIds;
    }
}
