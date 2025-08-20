using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class UnitLoader
{
    // 캐시를 위한 딕셔너리 (idx → RogueUnitDataBase)
    private static Dictionary<int, RogueUnitDataBase> unitCache=new();

    private static UnitLoader instance;
    public static UnitLoader Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new UnitLoader();
            }
            return instance;
        }
    }
    private UnitLoader() { }

    // JSON 파일에서 유닛 데이터를 읽고 캐시함
    public void LoadUnitsFromJson()
    {
        if (unitCache.Count > 0)
            return;

        //unitCache.Clear();

        TextAsset jsonFile = Resources.Load<TextAsset>("JsonData/UnitStatus_Re");
        var unitsData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonFile.text);

        foreach (var unitData in unitsData)
        {
            int idx = Convert.ToInt32(unitData["idx"]);
            string unitName = unitData["unitName"].ToString();
            string unitBranch = unitData["unitBranch"].ToString();
            int branchIdx = Convert.ToInt32(unitData["branchIdx"]);
            string unitFaction = unitData["unitFaction"].ToString();
            int factionIdx = Convert.ToInt32(unitData["factionIdx"]);
            string tag = unitData["tag"].ToString();
            int tagIdx = Convert.ToInt32(unitData["tagIdx"]);
            int unitPrice = Convert.ToInt32(unitData["unitPrice"]);
            int defaultPrice = Convert.ToInt32(unitData["defaultPrice"]);
            int rarity = Convert.ToInt32(unitData["rarity"]);
            float health = Convert.ToSingle(unitData["health"]);
            float armor = Convert.ToSingle(unitData["armor"]);
            float attackDamage = Convert.ToSingle(unitData["attackDamage"]);
            float mobility = Convert.ToSingle(unitData["mobility"]);
            float range = Convert.ToSingle(unitData["range"]);
            int energy = Convert.ToInt32(unitData["energy"]);

            // 불리언 특성
            bool GetBool(string key) => unitData.ContainsKey(key) && Convert.ToBoolean(unitData[key]);

            RogueUnitDataBase newUnit = new RogueUnitDataBase(
                idx, unitName, unitBranch, branchIdx,
                unitId: unitData.ContainsKey("unitId") ? unitData["unitId"].ToString() : "",
                unitExplain: unitData.ContainsKey("unitExplain") ? unitData["unitExplain"].ToString() : "",
                unitImg: unitData.ContainsKey("unitImg") ? unitData["unitImg"].ToString() : "",
                unitFaction, factionIdx, tag, tagIdx, unitPrice,defaultPrice, rarity,
                health, armor, attackDamage, mobility, range, 0, energy,
                health, armor, attackDamage, mobility, range, 0, energy,

                GetBool("lightArmor"), GetBool("heavyArmor"), GetBool("rangedAttack"),
                GetBool("bluntWeapon"), GetBool("pierce"), GetBool("agility"),
                GetBool("strongCharge"), GetBool("perfectAccuracy"), GetBool("slaughter"),
                GetBool("binding"), GetBool("bravery"), GetBool("suppression"),
                GetBool("plunder"), GetBool("doubleshot"), GetBool("scorch"),
                GetBool("thorns"), GetBool("endless"), GetBool("impact"),
                GetBool("healing"), GetBool("lifesteal"),
                GetBool("charge"), GetBool("defense"), GetBool("throwSpear"),
                GetBool("guerrilla"), GetBool("guard"), GetBool("assassination"),
                GetBool("drain"), GetBool("overwhelm"), GetBool("martyr"),
                GetBool("wound"), GetBool("vengeance"), GetBool("counter"),
                GetBool("firststrike"), GetBool("challenge"), GetBool("smokescreen"),
                health, energy, true, false, -1, new Dictionary<int, BuffDebuffData>()
            );

            unitCache[idx] = newUnit;
        }
        Console.WriteLine("e");
    }


    // id 기반으로 유닛을 복사하여 반환하는 함수
    public RogueUnitDataBase GetCloneUnitById(int id ,bool isTeam=true)
    {
        if (unitCache != null && unitCache.ContainsKey(id))
        {
            RogueUnitDataBase unit = unitCache[id].Clone();
            unit.UniqueId = RogueUnitDataBase.BuildUnitUniqueId(unit.branchIdx, unit.idx,isTeam);
            return unit;
        }
        return null;
    }
    // 모든 캐시된 유닛을 리스트로 반환
    public List<RogueUnitDataBase> GetAllCachedUnits()
    {
        return unitCache?.Values.ToList() ?? new List<RogueUnitDataBase>();
    }
    public RogueUnitDataBase GetUnitById(int id)
    {
        return unitCache[id];
    }

}
