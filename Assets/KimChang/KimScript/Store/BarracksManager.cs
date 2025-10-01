using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarracksManager
{
    //병영 훈련 비용
    public static int GetBarracksCost(RogueUnitDataBase unit)
    {
        int price = unit.unitPrice;
        float sale = 1;
        if (RelicManager.CheckRelicById(3))
        {
            sale -= 0.2f;
        }
        if (RelicManager.CheckRelicById(91))
        {
            sale += 0.2f;
        }
        price = (int)(price * sale);
        return price;
    }

    //병영 훈련 == 유닛 훈련
    public static bool UnitTrainning(RogueUnitDataBase unit, int price)
    {
        int gold = RogueLikeData.Instance.GetCurrentGold();
        if (RelicManager.CheckRelicById(49))
        {
            gold += 500;
        }
        if (gold > price)
        {
            RogueLikeData.Instance.ReduceGold(price);
            RogueLikeData.Instance.AddMyUnis(unit);
            return true;
        }
        return false;
    }
}
