using System.Collections.Generic;

[System.Serializable]
public class BattleRewardData
{
    public int battleResult;
    public int gold=0;
    public int morale=0;
    public int rerollChance = 0;
    public List<int> relicGrade=new();
    public List<int> relicIds=new();
    public List<int> unitGrade = new();
    public List<RogueUnitDataBase> newUnits = new();
    public List<RogueUnitDataBase> changedUnits = new();

}
