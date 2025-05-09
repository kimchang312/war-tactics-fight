using System.Collections.Generic;

[System.Serializable]
public class BattleRewardData
{
    public int gold;                    // 획득 골드
    public int morale;                 // 획득 사기
    public int rerollChance;            //추가 리롤
    public List<int> relicIds;      // 획득 유산
    public List<RogueUnitDataBase> newUnits;   // 새로 획득한 유닛
    public List<RogueUnitDataBase> changedUnits; // 전직된 유닛

    public BattleRewardData()
    {
        newUnits = new List<RogueUnitDataBase>();
        changedUnits = new List<RogueUnitDataBase>();
    }
}
