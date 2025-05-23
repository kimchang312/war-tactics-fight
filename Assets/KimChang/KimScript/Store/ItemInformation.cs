using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInformation : MonoBehaviour
{
    //아이템
    public bool isItem = true;

    public StoreItemData item = new();
    public int price;
    public List<RogueUnitDataBase> units = null;
    public int rerollCount = 0;

    //중복
    public int relicId = -1;

    //보상
    public RewardType type = RewardType.None;
    public int unitId = -1;
    public int abilityId = -1;
    
    public bool isUpgrade =false;
    public int upgradeId =-1;
}
public enum RewardType
{
    None,
    UnitGrade,
    NewUnit,
    ChangeUnit,
    RelicGrade,
    NewRelic
}