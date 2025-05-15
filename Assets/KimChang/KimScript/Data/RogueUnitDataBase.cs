using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEditor.Sprites;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

[System.Serializable]
public class RogueUnitDataBase 
{
    // 기본 정보
    public int idx;            // 유닛의 고유 인덱스
    public string unitName;    // 유닛 이름
    public string unitBranch;  // 병종 이름
    public int branchIdx;      // 병종 인덱스
    public string unitId;       // 유닛 아이디
    public string unitExplain;  // 유닛 설명
    public string unitImg;     // 유닛 이미지 ID
    public string unitFaction; // 유닛이 속한 진영
    public int factionIdx;     // 진영 인덱스
    public string tag;      // 유닛 태그
    public int tagIdx;
    public int unitPrice;      // 유닛 가격
    public int rarity;         // 희귀도

    // 기본 스탯      
    public float baseHealth;
    public float baseArmor;
    public float baseAttackDamage;
    public float baseMobility;
    public float baseRange;
    public float baseAntiCavalry;
    public int baseEnergy;

    // 스탯 정보
    public float health;       // 유닛 체력
    public float armor;        // 유닛 장갑
    public float attackDamage; // 공격력
    public float mobility;     // 기동성
    public float range;        // 사거리
    public float antiCavalry;  // 대기병 능력
    public int energy;       // 기력

    // 특성
    public bool lightArmor;     // 경갑 
    public bool heavyArmor;     // 중갑 
    public bool rangedAttack;   // 원거리 공격 
    public bool bluntWeapon;    // 둔기 
    public bool pierce;         // 관통 
    public bool agility;        // 날쌤 
    public bool strongCharge;   // 강한 돌격 
    public bool perfectAccuracy;// 필중 
    public bool slaughter;      // 도살
    public bool bindingForce;     // 결속
    public bool bravery;        // 용맹
    public bool suppression;    // 제압
    public bool plunder;        // 약탈
    public bool doubleShot;      // 연발
    public bool scorching;        // 작열
    public bool thorns;         // 가시
    public bool endless;       // 무한
    public bool impact;         // 충격
    public bool healing;           // 치유
    public bool lifeDrain;   // 흡혈

    //public string blink = "빈"; // 빈칸

    // 추가적인 능력치
    public bool charge;         // 돌격
    public bool defense;        // 수비 태세
    public bool throwSpear;     // 투창
    public bool guerrilla;      // 유격
    public bool guard;          // 수호
    public bool assassination;  // 암살
    public bool drain;          // 착취
    public bool overwhelm;      // 위압
    public bool martyrdom;      // 순교
    public bool wounding;           // 상흔
    public bool vengeance;        // 복수
    public bool counter;        // 반격
    public bool firstStrike;        // 선제 타격
    public bool challenge;      // 도전
    public bool smokeScreen;      // 연막

    public float maxHealth;    // 유닛 최대 체력 ==health
    public int maxEnergy;       //최대 기력
    public bool alive;         // 생존 유무 (기본값 true)
    public bool fStriked;      // 선제 타격 사용 유무
    public int UniqueId { get; set; } // 유닛 고유 ID, 기본값 -1로 설정

    public Dictionary<int, BuffDebuffData> effectDictionary = new Dictionary<int, BuffDebuffData>();

    // 생성자
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
        //기본 스탯
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
        this.UniqueId = BuildUnitUniqueId(branchIdx,idx);
        this.effectDictionary = effectDictionary??new Dictionary<int, BuffDebuffData>();
    }
    public static RogueUnitDataBase ConvertToUnitDataBase(List<string> rowData,bool isTeam=true)
    {
        if (rowData == null || rowData.Count == 0) return null;

        int idx, branchIdx, factionIdx, tagIdx, unitPrice, rarity,energy, maxEnergy, baseEnergy;
        float health, armor, attackDamage, mobility, range, antiCavalry, maxHealth, baseHealth, baseArmor, baseAttackDamage, baseMobility, baseRange, baseAntiCavalry;

        int.TryParse(rowData[0], out idx); // idx
        int.TryParse(rowData[3], out branchIdx); // branchIdx
        int.TryParse(rowData[8], out factionIdx); // factionIdx
        int.TryParse(rowData[10], out tagIdx); // tagIdx
        int.TryParse(rowData[11], out unitPrice); // unitPrice
        int.TryParse(rowData[12], out rarity); // rarity
        int.TryParse(rowData[19], out energy); // energy

        float.TryParse(rowData[13], out health); // health
        float.TryParse(rowData[14], out armor); // armor
        float.TryParse(rowData[15], out attackDamage); // attackDamage
        float.TryParse(rowData[16], out mobility); // mobility
        float.TryParse(rowData[17], out range); // range
        float.TryParse(rowData[18], out antiCavalry); // antiCavalry

        baseHealth = health;
        baseArmor = armor;
        baseAttackDamage = attackDamage;
        baseMobility = mobility;
        baseRange = range;
        baseAntiCavalry = antiCavalry;
        baseEnergy = energy;
        maxHealth = health; // 최대 체력은 기본적으로 health와 동일
        maxEnergy = energy;

        // 문자열 데이터 파싱
        string unitName = rowData[1]; // 유닛 이름
        string unitBranch = rowData[2]; // 병종 이름
        string unitId = rowData[4]; // 유닛 ID
        string unitExplain = rowData[5]; // 유닛 설명
        string unitImg = rowData[6]; // 유닛 이미지
        string unitFaction = rowData[7];
        string tag = rowData[9]; // 태그 정보

        // 특성 및 기술 불리언 값 파싱
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

        // 추가 능력치
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
    // 새로운 복사본을 만드는 메서드
    public RogueUnitDataBase Clone()
    {
        // 이 객체의 복사본을 새로 생성
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
            new Dictionary<int, BuffDebuffData>(this.effectDictionary) // 효과 딕셔너리도 복사
        );
    }

    public static int BuildUnitUniqueId(int branchIdx, int unitIdx, bool isTeam=true)
    {
        int serial = RogueLikeData.Instance.GetNextUnitUniqueId();
        int teamBit = isTeam ? 0 : 1;
        return (teamBit << 31) | (branchIdx << 24) | ((serial & 0x3FFF) << 10) | (unitIdx & 0x3FF);
    }

    //전직 확률
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

        // 새로운 희귀도의 유닛 중 랜덤 선택
        var pool = UnitLoader.Instance.GetAllCachedUnits()
                     .Where(u => u.rarity == newRarity)
                     .ToList();

        if (pool.Count == 0) return null;

        var picked = pool[UnityEngine.Random.Range(0, pool.Count)];
        if (picked == null) return null;

        RogueUnitDataBase recreated = UnitLoader.Instance.GetCloneUnitById(picked.idx);
        return recreated;
    }
    //희귀도에 따른 무작위 유닛 획득
    public static RogueUnitDataBase GetRandomUnitByRarity(int rarity)
    {
        var allUnits = UnitLoader.Instance.GetAllCachedUnits();

        var filtered = allUnits.Where(u => u.rarity == rarity).ToList();

        if (filtered.Count == 0)
            return null;

        int idx = UnityEngine.Random.Range(0, filtered.Count);
        var selected = filtered[idx];
        RogueUnitDataBase unit = UnitLoader.Instance.GetCloneUnitById(selected.idx);
        // UniqueId 자동 할당
        selected.UniqueId = RogueLikeData.Instance.GetNextUnitUniqueId();

        return selected;
    }
    //내 유닛 정상화
    public static List<RogueUnitDataBase> SetMyUnitsNormalize()
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        
        foreach (var unit in myUnits)
        {
            unit.health = unit.maxHealth;
            unit.attackDamage = unit.baseAttackDamage;
            unit.armor = unit.baseArmor;
            unit.mobility = unit.baseMobility;
            unit.range = unit.baseRange;
            unit.antiCavalry = unit.baseAntiCavalry;
        }
        return myUnits;
    }
    //내 유닛들로 저장된 유닛 초기화
    public static void SetSavedUnitsByMyUnits()
    {
        RogueLikeData.Instance.ClearSavedMyUnits();
        var myTeam = RogueLikeData.Instance.GetMyTeam();
        foreach(var unit in myTeam)
        {
            int id = unit.idx;
            RogueUnitDataBase newUnit = UnitLoader.Instance.GetCloneUnitById(id);
            RogueLikeData.Instance.AddSavedMyUnits(newUnit);
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
