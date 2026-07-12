using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public int baseArmor;
    public float baseAttackDamage;
    public int baseMobility;
    public int baseRange;
    public float baseAntiCavalry;
    public int baseEnergy;

    public float health;
    private int _armor;
    public int Armor
    {
        get => _armor;
        set
        {
            if(value < 1)
                _armor = 1;
            else
                _armor = value;
        }
    }

    public float attackDamage;
    private int _mobility;
    public int Mobility
    {
        get => _mobility;
        set
        {
            if (value < 1)
                _mobility = 1;
            else
                _mobility = value;
        }
    }
    public int range;
    public float antiCavalry;
    [SerializeField] private int _energy;
    public int Energy
    {
        get => _energy;
        set
        {
            if (_energy == value) return;
            if (IsEnergyLockedByRarity) return;

            int oldValue = _energy;
            int newValue = value;

            if (newValue < oldValue)
            {
                WarRelic ion = RelicManager.GetRelicById(2);
                if (ion != null)
                {
                    var vals = ion.GetAllValuesAsFloatListOrNull();
                    if (vals != null && RogueLikeData.Instance.GetRandomFloat() < vals[0])
                    {
                        return;
                    }
                }

                int diff = oldValue - newValue;
                WarRelic heavy = RelicManager.GetRelicById(105);
                if (heavy != null)
                {
                    var vals = heavy.GetAllValuesAsFloatListOrNull();
                    if (vals != null && vals.Count > 1 && RogueLikeData.Instance.GetRandomFloat() < vals[1])
                    {
                        int energyMultiplier = vals.Count > 2 ? Mathf.Max(1, Mathf.RoundToInt(vals[2])) : 2;
                        diff *= energyMultiplier;
                        newValue = oldValue - diff;
                    }
                }
            }

            if (newValue < 0) newValue = 0;
            if (newValue > MaxEnergy) newValue = MaxEnergy;

            _energy = newValue;
        }
    }
    public bool IsEnergyLockedByRarity => rarity == 4;
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

    [SerializeField] private int _maxEnergy;
    public int MaxEnergy
    {
        get
        {
            int result = _maxEnergy;

            if (RelicManager.CheckRelicById(30))
            {
                WarRelic relic = RelicManager.GetRelicById(30);
                var vals = relic.GetAllValuesAsFloatListOrNull();
                if (vals != null && vals.Count > 0)
                {
                    result += (int)vals[0];
                }
            }
            return result;
        }
        set
        {
            _maxEnergy = value;
        }
    }

    public bool alive;
    public bool fStriked;
    public int UniqueId;

    public Dictionary<int, BuffDebuffData> effectDictionary = new Dictionary<int, BuffDebuffData>();

    public StatBlock stats = new(); // 여기엔 계산된 결과 + modifier 임시 적용

    public DateTime acquiredDate;
    public RogueUnitDataBase(
    int idx, string unitName, string unitBranch, int branchIdx,string unitId,
    string unitExplain, string unitImg,
    string unitFaction, int factionIdx, string tag, int tagIdx, int unitPrice,int defaultPrice, int rarity,
    float health, int armor, float attackDamage, int mobility, int range,float antiCavalry, int energy,
    float baseHealth,float baseArmor,float baseAttackDamage,float baseMobility,float baseRange,float baseAntiCavalry,int baseEnergy,
    bool lightArmor, bool heavyArmor, bool rangedAttack,
    bool bluntWeapon, bool pierce, bool agility, bool strongCharge, bool perfectAccuracy,
    bool slaughter, bool bindingForce, bool bravery, bool suppression, bool plunder,
    bool doubleShot, bool scorching, bool thorns, bool endless, bool impact, bool healing,
    bool lifeDrain, bool charge, bool defense, bool throwSpear, bool guerrilla,
    bool guard, bool assassination, bool drain, bool overwhelm, bool martyrdom, bool wounding,
    bool vengeance, bool counter, bool firstStrike, bool challenge, bool smokeScreen,
    float maxHealth,int maxEnergy, bool alive = true, bool fStriked = false, int uniqueId = -1,
    Dictionary<int, BuffDebuffData> effectDictionary = null, DateTime? acquiredDate = null)
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
        this.Armor = armor;
        this.attackDamage = attackDamage;
        this.Mobility = mobility;
        this.range = range;
        this.antiCavalry = antiCavalry;
        this._energy = energy;

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
        this.MaxEnergy = maxEnergy;
        this.alive = alive;
        this.fStriked = fStriked;
        this.UniqueId = uniqueId;
        this.effectDictionary = effectDictionary??new Dictionary<int, BuffDebuffData>();
        this.acquiredDate = acquiredDate ?? DateTime.Now;
        NormalizeStatBlock();
    }
    public RogueUnitDataBase Clone()
    {
        return new RogueUnitDataBase(
            this.idx, this.unitName, this.unitBranch, this.branchIdx, this.unitId, this.unitExplain, this.unitImg, this.unitFaction, this.factionIdx,
            this.tag, this.tagIdx, this.unitPrice,this.defaultPrice, this.rarity,
            this.health, this.Armor, this.attackDamage, this.Mobility, this.range, this.antiCavalry, this.Energy,
            this.baseHealth, this.baseArmor, this.baseAttackDamage, this.baseMobility, this.baseRange, this.baseAntiCavalry, this.baseEnergy,
            this.lightArmor, this.heavyArmor, this.rangedAttack, this.bluntWeapon, this.pierce, this.agility,
            this.strongCharge, this.perfectAccuracy, this.slaughter, this.bindingForce, this.bravery, this.suppression,
            this.plunder, this.doubleShot, this.scorching, this.thorns, this.endless, this.impact, this.healing,
            this.lifeDrain, this.charge, this.defense, this.throwSpear, this.guerrilla, this.guard, this.assassination,
            this.drain, this.overwhelm, this.martyrdom, this.wounding, this.vengeance, this.counter, this.firstStrike,
            this.challenge, this.smokeScreen, this.maxHealth, this.MaxEnergy, this.alive, this.fStriked, this.UniqueId,
            new Dictionary<int, BuffDebuffData>(this.effectDictionary), DateTime.Now 
        );
    }
    public void SetEnergyDirect(int value)
    {
        _energy = Mathf.Clamp(value, 0, MaxEnergy);
    }
    public void SetMaxEnergyDirect(int value)
    {
        _maxEnergy = value;
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
            UnitLoader.Instance.GetCloneUnitById(2),

        };
        return units.Where(unit => unit != null).ToList();
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
        RogueUnitDataBase unitEx = UnitLoader.Instance.GetUnitById(idx);
        if (unitEx == null)
        {
            Debug.LogWarning($"[RogueUnitDataBase] Unit data not found for idx={idx}. State trait reset skipped.");
            return;
        }
        
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
        Armor = (int)stats.GetStat(StatType.Armor);
        attackDamage = Mathf.Round(stats.GetStat(StatType.AttackDamage));
        Mobility = (int)stats.GetStat(StatType.Mobility);
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

    /// <summary>
    /// 기괴한 주교 
    /// </summary>
    public void PassiveBizarreBishop()
    {
        if(branchIdx == 1)
        {
            wounding = true;
            counter = true;
        }
    }

    public static List<RogueUnitDataBase> OrderStrongUnits(List<RogueUnitDataBase> units)
    {
        if (units == null || units.Count == 0)
            return new List<RogueUnitDataBase>();

        var ordered = units
            .OrderByDescending(u => u.unitPrice)
            .ThenByDescending(u => u.Energy)
            .ThenBy(u => u.idx)
            .ToList();
        
        return ordered;
    }

    public static RogueUnitDataBase GetRandomUnitByBranchAndRarity(int branchIdx, int rarity)
    {
        // 모든 유닛 캐시에서 조건에 맞는 유닛만 필터링
        var allUnits = UnitLoader.Instance.GetAllCachedUnits();
        var filtered = allUnits
            .Where(u => u.branchIdx == branchIdx && u.rarity == rarity)
            .ToList();

        if (filtered.Count == 0)
            return null;

        // 무작위 선택
        int randomIndex = RogueLikeData.Instance.GetRandomInt(0, filtered.Count);
        RogueUnitDataBase selected = filtered[randomIndex];

        return UnitLoader.Instance.GetCloneUnitById(selected.idx);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float GetRarity1Reduce()
    {
        var r = RelicManager.GetRelicById(85);
        if (r == null) return 0f;
        var vals = r.GetAllValuesAsFloatListOrNull();
        if (vals == null || vals.Count == 0) return 0f;
        float v = Mathf.Abs(vals[0]);
        if (v <= 0f) return 0f;
        if (v >= 1f) return 1f;
        return v;
    }

    // 사용처: 희귀도1 가중치 감소를 반영한 룰렛 선택(리스트 인덱스 반환)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int PickIndexWithRarity1Penalty(List<RogueUnitDataBase> list, float reduce)
    {
        float r1w = 1f - reduce;              // rarity==1 가중치
        float total = 0f;
        for (int i = 0; i < list.Count; i++)
            total += (list[i].rarity == 1) ? r1w : 1f;

        if (total <= 0f)
            return RogueLikeData.Instance.GetRandomInt(0, list.Count);

        float roll = RogueLikeData.Instance.GetRandomFloat() * total;
        for (int i = 0; i < list.Count; i++)
        {
            roll -= (list[i].rarity == 1) ? r1w : 1f;
            if (roll <= 0f) return i;
        }
        return list.Count - 1;
    }
    // 사용처: 유닛에게 부여 가능한 15개 특성 중 현재 false인 것들에서 count개를 무작위로 true로 바꿀 때 호출
    public int SetRandomTraits(int count = 1)
    {
        Span<int> buf = stackalloc int[15];
        int n = 0;
        for (int i = 0; i < 15; i++)
        {
            if (!IsGrantableTraitSet(i))
                buf[n++] = i;
        }

        if (n == 0 || count <= 0)
            return 0;

        int k = count < n ? count : n;
        for (int i = 0; i < k; i++)
        {
            int r = RogueLikeData.Instance.GetRandomInt(i, n);
            (buf[i], buf[r]) = (buf[r], buf[i]);
            SetGrantableTraitTrue(buf[i]);
        }
        return k;
    }

    // 사용처: 내부 헬퍼 – 인덱스별로 현재 true 여부 확인
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private bool IsGrantableTraitSet(int i)
    {
        switch (i)
        {
            case 0: return bluntWeapon;
            case 1: return pierce;
            case 2: return agility;
            case 3: return strongCharge;
            case 4: return perfectAccuracy;
            case 5: return slaughter;
            case 6: return bravery;
            case 7: return suppression;
            case 8: return plunder;
            case 9: return doubleShot;
            case 10: return scorching;
            case 11: return thorns;
            case 12: return endless;
            case 13: return impact;
            case 14: return lifeDrain;
            default: return false;
        }
    }

    // 사용처: 내부 헬퍼 – 인덱스별로 true 설정
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private void SetGrantableTraitTrue(int i)
    {
        switch (i)
        {
            case 0: bluntWeapon = true; break;
            case 1: pierce = true; break;
            case 2: agility = true; break;
            case 3: strongCharge = true; break;
            case 4: perfectAccuracy = true; break;
            case 5: slaughter = true; break;
            case 6: bravery = true; break;
            case 7: suppression = true; break;
            case 8: plunder = true; break;
            case 9: doubleShot = true; break;
            case 10: scorching = true; break;
            case 11: thorns = true; break;
            case 12: endless = true; break;
            case 13: impact = true; break;
            case 14: lifeDrain = true; break;
            default: break;
        }
    }

}
