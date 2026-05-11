using System.Collections.Generic;

[System.Serializable]
public class ItemInfoData
{
    public bool isItem = true;
    public bool isRelic = false;
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
    public int upgradeBranchIdx = -1;
    public bool upgradeIsAttack = true;
    public UpgradeTooltipMode upgradeTooltipMode = UpgradeTooltipMode.Single;

    public int gameTextId = -1;

    public bool isBuffDeBuff = false;
    public int buffDeBuffId = -1;
    public int buffDeBuffGrade = 0;
    public int buffDeBuffDuration = 0;
    public string buffDeBuffName = "";
    public string buffDeBuffDescription = "";
}

public enum UpgradeTooltipMode
{
    Single,
    BranchSummary
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