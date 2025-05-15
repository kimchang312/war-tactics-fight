using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarracksManager
{
    //병영 훈련 비용
    public static int GetBarracksCost(RogueUnitDataBase unit)
    {
        int price = unit.unitPrice;
        if (RogueLikeData.Instance.GetOwnedRelicById(3) !=null)
        {
            price =  (int)(price*0.8f); 
        }
        return price;
    }

    //병영 훈련 == 유닛 훈련
    public static bool UnitTrainning(RogueUnitDataBase unit, int price)
    {
        int gold = RogueLikeData.Instance.GetCurrentGold();
        if (RogueLikeData.Instance.GetOwnedRelicById(49) != null)
        {
            gold += 500;
        }
        if (gold > price)
        {
            RogueLikeData.Instance.ReduceGold(price);
            RogueLikeData.Instance.AddMyTeam(unit);
            return true;
        }
        return false;
    }
}
