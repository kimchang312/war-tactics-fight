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
    public int defaultPrice;
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

    public StatBlock stats = new(); // 여기엔 계산된 결과 + modifier 임시 적용

    public RogueUnitDataBase(
    int idx, string unitName, string unitBranch, int branchIdx,string unitId,
    string unitExplain, string unitImg,
    string unitFaction, int factionIdx, string tag, int tagIdx, int unitPrice,int defaultPrice, int rarity,
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
        this.defaultPrice = defaultPrice;
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

        NormalizeStatBlock();
    }
    public RogueUnitDataBase Clone()
    {
        return new RogueUnitDataBase(
            this.idx, this.unitName, this.unitBranch, this.branchIdx, this.unitId, this.unitExplain, this.unitImg, this.unitFaction, this.factionIdx,
            this.tag, this.tagIdx, this.unitPrice,this.defaultPrice, this.rarity,
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
        float rand = RogueLikeData.Instance.GetRandomFloat();
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

        var picked = pool[RogueLikeData.Instance.GetRandomInt(0, pool.Count)];
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

        int idx = RogueLikeData.Instance.GetRandomInt(0, filtered.Count);
        RogueUnitDataBase selected = filtered[idx];

        RogueUnitDataBase unit = UnitLoader.Instance.GetCloneUnitById(selected.idx);

        return unit;
    }

    public static List<RogueUnitDataBase> GetBaseUnits()
    {
        List<RogueUnitDataBase> units = new()
        {
            UnitLoader.Instance.GetCloneUnitById(0),
            UnitLoader.Instance.GetCloneUnitById(1),
            UnitLoader.Instance.GetCloneUnitById(2)
        };
        return units;
    }
    public void NormalizeStatBlock()
    {
        stats = new StatBlock
        {
            baseHealth = baseHealth,
            baseAttackDamage = baseAttackDamage,
            baseArmor = baseArmor,
            baseRange = baseRange,
            baseMobility = baseMobility
        };
    }

    public void NormalizeStateModifiers()
    {
        //NormalizeStatBlock();
        RogueUnitDataBase unitEx = UnitLoader.Instance.GetUnitById(idx);
        
        lightArmor = unitEx.lightArmor;
        heavyArmor = unitEx.heavyArmor;
        rangedAttack =unitEx.rangedAttack;
        bluntWeapon =unitEx.bluntWeapon;
        pierce = unitEx.pierce;
        agility =unitEx.agility;
        strongCharge =unitEx.strongCharge;
        perfectAccuracy =unitEx.perfectAccuracy;
        slaughter =unitEx.slaughter;
        bindingForce =unitEx.bindingForce;
        bravery = unitEx.bravery;
        suppression =unitEx.suppression;
        plunder =unitEx.plunder;
        doubleShot = unitEx.doubleShot;
        scorching = unitEx.scorching;
        thorns = unitEx.thorns;
        impact = unitEx.impact;
        healing = unitEx.healing;
        lifeDrain = unitEx.lifeDrain;
        charge = unitEx.charge;
        defense = unitEx.defense;
        throwSpear = unitEx.throwSpear;
        guerrilla = unitEx.guerrilla;
        guard = unitEx.guard;
        assassination = unitEx.assassination;
        drain = unitEx.drain;
        overwhelm = unitEx.overwhelm;
        martyrdom = unitEx.martyrdom;
        wounding = unitEx.wounding;
        vengeance = unitEx.vengeance;
        counter = unitEx.counter;
        firstStrike = unitEx.firstStrike;
        challenge = unitEx.challenge;
        smokeScreen = unitEx.smokeScreen;

        fStriked = false;
        alive =false;

        effectDictionary.Clear();
    }
    public void ApplyModifiers(bool isBattle =false)
    {
        float newMaxHealth = Mathf.Round(stats.GetStat(StatType.Health));
        if (isBattle)
        {
            float addHealth = newMaxHealth - maxHealth;
            health += addHealth;

        }
        else
        {
            // 각각의 스탯 값을 StateBlock에서 계산하여 적용
            health = Mathf.Round(stats.GetStat(StatType.Health));
        }
        maxHealth = newMaxHealth;
        armor = Mathf.Round(stats.GetStat(StatType.Armor));
        attackDamage = Mathf.Round(stats.GetStat(StatType.AttackDamage));
        mobility = Mathf.Min(1,(int)stats.GetStat(StatType.Mobility));
        range = (int)stats.GetStat(StatType.Range);
    }

    public static void AddBizarreBishopMyTeam()
    {
        List<RogueUnitDataBase> myTeam = RogueLikeData.Instance.GetMyTeam();
        int removed = 0;

        // 조건: branchIdx != 1 && rarity < 3 인 유닛
        for (int i = myTeam.Count - 1; i >= 0 && removed < 3; i--)
        {
            var unit = myTeam[i];
            if (unit.branchIdx != 1 && unit.rarity < 3)
            {
                myTeam.RemoveAt(i);
                removed++;
            }
        }
        RogueLikeData.Instance.SetMyTeam(myTeam);
    }

    public static void PassiveBizarreBishop()
    {
        List<RogueUnitDataBase> myTeam = RogueLikeData.Instance.GetMyTeam();
        foreach (var unit in myTeam)
        {
            if(unit.branchIdx == 1)
            {
                unit.wounding = true;
                unit.counter=true;
            }

        }
    }

}
