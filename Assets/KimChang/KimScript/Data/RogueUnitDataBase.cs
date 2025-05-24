using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class RogueUnitDataBase 
{
    public int idx;
    public string unitName; 
    public string unitBranch;
    public int branchIdx; 
    public string unitId;      
    public string unitExplain;  
    public string unitImg;   
    public string unitFaction; 
    public int factionIdx;   
    public string tag;    
    public int tagIdx;
    public int unitPrice;     
    public int rarity;        

    public float baseHealth;
    public float baseArmor;
    public float baseAttackDamage;
    public float baseMobility;
    public float baseRange;
    public float baseAntiCavalry;
    public int baseEnergy;

    public float health;       
    public float armor;       
    public float attackDamage; 
    public float mobility;    
    public float range;       
    public float antiCavalry;  
    public int energy; 


    public bool lightArmor; 
    public bool heavyArmor;   
    public bool rangedAttack;  
    public bool bluntWeapon;  
    public bool pierce; 
    public bool agility;   
    public bool strongCharge;   
    public bool perfectAccuracy;
    public bool slaughter;      
    public bool bindingForce;    
    public bool bravery;        
    public bool suppression;   
    public bool plunder;        
    public bool doubleShot;     
    public bool scorching;
    public bool thorns;
    public bool endless;
    public bool impact;
    public bool healing;
    public bool lifeDrain;

    public bool charge;
    public bool defense;
    public bool throwSpear;
    public bool guerrilla;
    public bool guard;
    public bool assassination;
    public bool drain;
    public bool overwhelm;
    public bool martyrdom;
    public bool wounding;
    public bool vengeance;
    public bool counter;
    public bool firstStrike;
    public bool challenge;
    public bool smokeScreen;

    public float maxHealth;
    public int maxEnergy;
    public bool alive;
    public bool fStriked;
    public int UniqueId;

    public Dictionary<int, BuffDebuffData> effectDictionary = new Dictionary<int, BuffDebuffData>();

    public RogueUnitDataBase(
    int idx, string unitName, string unitBranch, int branchIdx,string unitId,
    string unitExplain, string unitImg,
    string unitFaction, int factionIdx, string tag, int tagIdx, int unitPrice, int rarity,
    float health, float armor, float attackDamage, float mobility, float range,float antiCavalry, int energy,
    float baseHealth,float baseArmor,float baseAttackDamage,float baseMobility,float baseRange,float baseAntiCavalry,int baseEnergy,
    bool lightArmor, bool heavyArmor, bool rangedAttack,
    bool bluntWeapon, bool pierce, bool agility, bool strongCharge, bool perfectAccuracy,
    bool slaughter, bool bindingForce, bool bravery, bool suppression, bool plunder,
    bool doubleShot, bool scorching, bool thorns, bool endless, bool impact, bool healing,
    bool lifeDrain, bool charge, bool defense, bool throwSpear, bool guerrilla,
    bool guard, bool assassination, bool drain, bool overwhelm, bool martyrdom, bool wounding,
    bool vengeance, bool counter, bool firstStrike, bool challenge, bool smokeScreen,
    float maxHealth,int maxEnergy, bool alive = true, bool fStriked = false, int uniqueId = -1,
    Dictionary<int, BuffDebuffData> effectDictionary = null)
    {
        this.idx = idx;
        this.unitName = unitName;
        this.unitBranch = unitBranch;
        this.branchIdx = branchIdx;
        this.unitId = unitId;
        this.unitExplain = unitExplain;
        this.unitImg = unitImg;
        this.unitFaction = unitFaction;
        this.factionIdx = factionIdx;
        this.tag = tag;
        this.tagIdx = tagIdx;
        this.unitPrice = unitPrice;
        this.rarity = rarity;

        this.baseHealth = health;
        this.baseArmor = armor;
        this.baseAttackDamage = attackDamage;
        this.baseMobility = mobility;
        this.baseRange = range;
        this.baseAntiCavalry = antiCavalry;
        this.baseEnergy = energy;

        this.health = health;
        this.armor = armor;
        this.attackDamage = attackDamage;
        this.mobility = mobility;
        this.range = range;
        this.antiCavalry = antiCavalry;
        this.energy = energy;

        this.lightArmor = lightArmor;
        this.heavyArmor = heavyArmor;
        this.rangedAttack = rangedAttack;
        this.bluntWeapon = bluntWeapon;
        this.pierce = pierce;
        this.agility = agility;
        this.strongCharge = strongCharge;
        this.perfectAccuracy = perfectAccuracy;
        this.slaughter = slaughter;
        this.bindingForce = bindingForce;
        this.bravery = bravery;
        this.suppression = suppression;
        this.plunder = plunder;
        this.doubleShot = doubleShot;
        this.scorching = scorching;
        this.thorns = thorns;
        this.endless = endless;
        this.impact = impact;
        this.healing = healing;
        this.lifeDrain = lifeDrain;
        this.charge = charge;
        this.defense = defense;
        this.throwSpear = throwSpear;
        this.guerrilla = guerrilla;
        this.guard = guard;
        this.assassination = assassination;
        this.drain = drain;
        this.overwhelm = overwhelm;
        this.martyrdom = martyrdom;
        this.wounding = wounding;
        this.vengeance = vengeance;
        this.counter = counter;
        this.firstStrike = firstStrike;
        this.challenge = challenge;
        this.smokeScreen = smokeScreen;
        this.maxHealth = maxHealth;
        this.maxEnergy = maxEnergy;
        this.alive = alive;
        this.fStriked = fStriked;
        this.UniqueId = uniqueId;
        this.effectDictionary = effectDictionary??new Dictionary<int, BuffDebuffData>();
    }
    public static RogueUnitDataBase ConvertToUnitDataBase(List<string> rowData,bool isTeam=true)
    {
        if (rowData == null || rowData.Count == 0) return null;

        int idx, branchIdx, factionIdx, tagIdx, unitPrice, rarity,energy, maxEnergy, baseEnergy;
        float health, armor, attackDamage, mobility, range, antiCavalry, maxHealth, baseHealth, baseArmor, baseAttackDamage, baseMobility, baseRange, baseAntiCavalry;

        int.TryParse(rowData[0], out idx);
        int.TryParse(rowData[3], out branchIdx);
        int.TryParse(rowData[8], out factionIdx);
        int.TryParse(rowData[10], out tagIdx);
        int.TryParse(rowData[11], out unitPrice);
        int.TryParse(rowData[12], out rarity);
        int.TryParse(rowData[19], out energy);

        float.TryParse(rowData[13], out health);
        float.TryParse(rowData[14], out armor);
        float.TryParse(rowData[15], out attackDamage);
        float.TryParse(rowData[16], out mobility);
        float.TryParse(rowData[17], out range);
        float.TryParse(rowData[18], out antiCavalry);

        baseHealth = health;
        baseArmor = armor;
        baseAttackDamage = attackDamage;
        baseMobility = mobility;
        baseRange = range;
        baseAntiCavalry = antiCavalry;
        baseEnergy = energy;
        maxHealth = health;
        maxEnergy = energy;

        string unitName = rowData[1];
        string unitBranch = rowData[2];
        string unitId = rowData[4];
        string unitExplain = rowData[5];
        string unitImg = rowData[6];
        string unitFaction = rowData[7];
        string tag = rowData[9];

        bool lightArmor = rowData[20] == "TRUE";
        bool heavyArmor = rowData[21] == "TRUE";
        bool rangedAttack = rowData[22] == "TRUE";
        bool bluntWeapon = rowData[23] == "TRUE";
        bool pierce = rowData[24] == "TRUE";
        bool agility = rowData[25] == "TRUE";
        bool strongCharge = rowData[26] == "TRUE";
        bool perfectAccuracy = rowData[27] == "TRUE";
        bool slaughter = rowData[28] == "TRUE";
        bool bindingForce = rowData[29] == "TRUE";
        bool bravery = rowData[30] == "TRUE";
        bool suppression = rowData[31] == "TRUE";
        bool plunder = rowData[32] == "TRUE";
        bool doubleShot = rowData[33] == "TRUE";
        bool scorching = rowData[34] == "TRUE";
        bool thorns = rowData[35] == "TRUE";
        bool endless = rowData[36] == "TRUE";
        bool impact = rowData[37] == "TRUE";
        bool healing = rowData[38] == "TRUE";
        bool lifeDrain = rowData[39] == "TRUE";

        bool charge = rowData[41] == "TRUE";
        bool defense = rowData[42] == "TRUE";
        bool throwSpear = rowData[43] == "TRUE";
        bool guerrilla = rowData[44] == "TRUE";
        bool guard = rowData[45] == "TRUE";
        bool assassination = rowData[46] == "TRUE";
        bool drain = rowData[47] == "TRUE";
        bool overwhelm = rowData[48] == "TRUE";
        bool martyrdom = rowData[49] == "TRUE";
        bool wounding = rowData[50] == "TRUE";
        bool vengeance = rowData[51] == "TRUE";
        bool counter = rowData[52] == "TRUE";
        bool firstStrike = rowData[53] == "TRUE";
        bool challenge = rowData[54] == "TRUE";
        bool smokeScreen = rowData[55] == "TRUE";

        int UniqueId = BuildUnitUniqueId(branchIdx, idx, isTeam);
        Dictionary<int,BuffDebuffData> effectDictionary = new Dictionary<int, BuffDebuffData>();

        return new RogueUnitDataBase(
            idx, unitName, unitBranch, branchIdx, unitId, unitExplain, unitImg, unitFaction, factionIdx, tag, tagIdx, unitPrice, rarity,
            health, armor, attackDamage, mobility, range, antiCavalry, energy, baseHealth, baseArmor, baseAttackDamage, baseMobility, baseRange, baseAntiCavalry,baseEnergy,
            lightArmor, heavyArmor, rangedAttack, bluntWeapon, pierce, agility, strongCharge, perfectAccuracy, slaughter,
            bindingForce, bravery, suppression, plunder, doubleShot, scorching, thorns, endless, impact, healing, lifeDrain,
            charge, defense, throwSpear, guerrilla, guard, assassination, drain, overwhelm,
            martyrdom, wounding, vengeance, counter, firstStrike, challenge, smokeScreen,
            maxHealth,maxEnergy, true, false, UniqueId, effectDictionary
        );
    }
    public RogueUnitDataBase Clone()
    {
        return new RogueUnitDataBase(
            this.idx, this.unitName, this.unitBranch, this.branchIdx, this.unitId, this.unitExplain, this.unitImg, this.unitFaction, this.factionIdx,
            this.tag, this.tagIdx, this.unitPrice, this.rarity,
            this.health, this.armor, this.attackDamage, this.mobility, this.range, this.antiCavalry, this.energy,
            this.baseHealth, this.baseArmor, this.baseAttackDamage, this.baseMobility, this.baseRange, this.baseAntiCavalry, this.baseEnergy,
            this.lightArmor, this.heavyArmor, this.rangedAttack, this.bluntWeapon, this.pierce, this.agility,
            this.strongCharge, this.perfectAccuracy, this.slaughter, this.bindingForce, this.bravery, this.suppression,
            this.plunder, this.doubleShot, this.scorching, this.thorns, this.endless, this.impact, this.healing,
            this.lifeDrain, this.charge, this.defense, this.throwSpear, this.guerrilla, this.guard, this.assassination,
            this.drain, this.overwhelm, this.martyrdom, this.wounding, this.vengeance, this.counter, this.firstStrike,
            this.challenge, this.smokeScreen, this.maxHealth, this.maxEnergy, this.alive, this.fStriked, this.UniqueId,
            new Dictionary<int, BuffDebuffData>(this.effectDictionary) 
        );
    }

    public static int BuildUnitUniqueId(int branchIdx, int unitIdx, bool isTeam=true)
    {
        int serial = RogueLikeData.Instance.GetNextUnitUniqueId();
        int teamBit = isTeam ? 0 : 1;
        return (teamBit << 31) | (branchIdx << 24) | ((serial & 0x3FFF) << 10) | (unitIdx & 0x3FF);
    }
    public static int RollPromotion(int rarity)
    {
        float rand = UnityEngine.Random.value;
        return rarity switch
        {
            1 => rand < 0.5f ? 1 : (rand < 0.95f ? 2 : 3),
            2 => rand < 0.75f ? 2 : (rand < 0.95f ? 3 : 10),
            3 => rand < 0.9f ? 3 : 10,
            _ => rarity
        };
    }
    public static RogueUnitDataBase RandomUnitReForm(RogueUnitDataBase unit)
    {
        if (unit.rarity >= 4) return unit;

        int newRarity = RollPromotion(unit.rarity);

        var pool = UnitLoader.Instance.GetAllCachedUnits()
                     .Where(u => u.rarity == newRarity)
                     .ToList();

        if (pool.Count == 0) return null;

        var picked = pool[UnityEngine.Random.Range(0, pool.Count)];
        if (picked == null) return null;

        RogueUnitDataBase recreated = UnitLoader.Instance.GetCloneUnitById(picked.idx);
        return recreated;
    }
    public static RogueUnitDataBase GetRandomUnitByRarity(int rarity)
    {
        List<RogueUnitDataBase> myUnits = RogueLikeData.Instance.GetMyTeam();
        List<RogueUnitDataBase> allUnits = UnitLoader.Instance.GetAllCachedUnits();

        List<RogueUnitDataBase> filtered;

        if (rarity == 4)
        {
            HashSet<int> myUnitIds = myUnits
                .Where(u => u.rarity == 4)
                .Select(u => u.idx)
                .ToHashSet();

            filtered = allUnits
                .Where(u => u.rarity == 4 && !myUnitIds.Contains(u.idx))
                .ToList();
        }
        else
        {
            filtered = allUnits
                .Where(u => u.rarity == rarity)
                .ToList();
        }

        if (filtered.Count == 0)
            return null;

        int idx = Random.Range(0, filtered.Count);
        RogueUnitDataBase selected = filtered[idx];

        RogueUnitDataBase unit = UnitLoader.Instance.GetCloneUnitById(selected.idx);

        return unit;
    }
    public static List<RogueUnitDataBase> SetMyUnitsNormalize()
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        
        foreach (var unit in myUnits)
        {
            unit.maxHealth = unit.baseHealth;
            unit.health = unit.maxHealth;
            unit.attackDamage = unit.baseAttackDamage;
            unit.armor = unit.baseArmor;
            unit.mobility = unit.baseMobility;
            unit.range = unit.baseRange;
            unit.antiCavalry = unit.baseAntiCavalry;
            unit.fStriked =false;
        }
        return myUnits;
    }
    public static void SetMyTeamNoramlize()
    {
        var myTeam = RogueLikeData.Instance.GetMyTeam();
        foreach (var unit in myTeam)
        {
            unit.maxHealth = unit.baseHealth;
            unit.health = unit.maxHealth;
            unit.attackDamage= unit.baseAttackDamage;
            unit.armor = unit.baseArmor;
            unit.mobility= unit.baseMobility;
            unit.range = unit.baseRange;
            unit.antiCavalry= unit.baseAntiCavalry;
            unit.fStriked = false;
        }
    }

    public static List<RogueUnitDataBase> GetBaseUnits()
    {
        List<RogueUnitDataBase> units = new();
        units.Add(UnitLoader.Instance.GetCloneUnitById(0));
        units.Add(UnitLoader.Instance.GetCloneUnitById(2));
        units.Add(UnitLoader.Instance.GetCloneUnitById(3));
        return units;
    }

}
