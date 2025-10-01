using System.Collections.Generic;

[System.Serializable]
public class ItemInfoData
{
    public bool isItem = true;
    public StoreItemData item = new();
    public int price;
    public List<RogueUnitDataBase> units = null;
    public int rerollCount = 0;

    public int relicId = -1;

    public RewardType type = RewardType.None;
    public int unitId = -1;
    public int abilityId = -1;

    public bool isUpgrade = false;
    public int upgradeId = -1;
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