using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RogueUnitDataBase 
{
    // 기본 정보
    public int idx;            // 유닛의 고유 인덱스
    public string unitName;    // 유닛 이름
    public string unitBranch;  // 병종 이름
    public int branchIdx;      // 병종 인덱스
    public string unitImg;     // 유닛 이미지 ID
    public string unitFaction; // 유닛이 속한 진영
    public int factionIdx;     // 진영 인덱스
    public string tag;      // 유닛 태그
    public int tagIdx;
    public int unitPrice;      // 유닛 가격
    public int rarity;         // 희귀도


    // 스탯 정보
    public float health;       // 유닛 체력
    public float armor;        // 유닛 장갑
    public float attackDamage; // 공격력
    public float mobility;     // 기동성
    public float range;        // 사거리
    public float antiCavalry;  // 대기병 능력
    public float energy;       // 기력

    // 특성
    public bool lightArmor;     // 경갑 유무
    public bool heavyArmor;     // 중갑 유무
    public bool rangedAttack;   // 원거리 공격 유무
    public bool bluntWeapon;    // 둔기 유무
    public bool pierce;         // 관통 유무
    public bool agility;        // 날쌤 유무
    public bool strongCharge;   // 강한 돌격 유무
    public bool perfectAccuracy;// 필중 유무
    public bool slaughter;      // 도살
    public bool solidarity;     // 결속
    public bool bravery;        // 용맹
    public bool subjugation;    // 제압
    public bool looting;        // 약탈
    public bool rapidFire;      // 연발
    public bool burning;        // 작열
    public bool thorns;         // 가시
    public bool infinity;       // 무한
    public bool impact;         // 충격
    public bool cure;           // 치유
    public bool bloodSucking;   // 흡혈

    public string blink = "빈"; // 빈칸

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
    public bool scar;           // 상흔
    public bool revenge;        // 복수
    public bool counterattack;  // 반격
    public bool preemptiveStrike;// 선제 타격
    public bool challenge;      // 도전
    public bool smokeBomb;      // 연막

    public float maxHealth;    // 유닛 최대 체력 ==health
    public bool alive;         // 생존 유무 (기본값 true)
    public int UniqueId { get; set; } // 유닛 고유 ID, 기본값 -1로 설정

    // 생성자
    public RogueUnitDataBase(
    int idx, string unitName, string unitBranch, int branchIdx, string unitImg,
    string unitFaction, int factionIdx, string tag, int tagIdx, int unitPrice, int rarity,
    float health, float armor, float attackDamage, float mobility, float range,
    float antiCavalry, float energy, bool lightArmor, bool heavyArmor, bool rangedAttack,
    bool bluntWeapon, bool pierce, bool agility, bool strongCharge, bool perfectAccuracy,
    bool slaughter, bool solidarity, bool bravery, bool subjugation, bool looting,
    bool rapidFire, bool burning, bool thorns, bool infinity, bool impact, bool cure,
    bool bloodSucking, bool charge, bool defense, bool throwSpear, bool guerrilla,
    bool guard, bool assassination, bool drain, bool overwhelm, bool martyrdom, bool scar,
    bool revenge, bool counterattack, bool preemptiveStrike, bool challenge, bool smokeBomb,
    float maxHealth, bool alive = true, int uniqueId = -1)
    {
        this.idx = idx;
        this.unitName = unitName;
        this.unitBranch = unitBranch;
        this.branchIdx = branchIdx;
        this.unitImg = unitImg;
        this.unitFaction = unitFaction;
        this.factionIdx = factionIdx;
        this.tag = tag;
        this.tagIdx = tagIdx;
        this.unitPrice = unitPrice;
        this.rarity = rarity;
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
        this.solidarity = solidarity;
        this.bravery = bravery;
        this.subjugation = subjugation;
        this.looting = looting;
        this.rapidFire = rapidFire;
        this.burning = burning;
        this.thorns = thorns;
        this.infinity = infinity;
        this.impact = impact;
        this.cure = cure;
        this.bloodSucking = bloodSucking;
        this.charge = charge;
        this.defense = defense;
        this.throwSpear = throwSpear;
        this.guerrilla = guerrilla;
        this.guard = guard;
        this.assassination = assassination;
        this.drain = drain;
        this.overwhelm = overwhelm;
        this.martyrdom = martyrdom;
        this.scar = scar;
        this.revenge = revenge;
        this.counterattack = counterattack;
        this.preemptiveStrike = preemptiveStrike;
        this.challenge = challenge;
        this.smokeBomb = smokeBomb;
        this.maxHealth = maxHealth;
        this.alive = alive;
        this.UniqueId = uniqueId;
    }
    public static RogueUnitDataBase ConvertToUnitDataBase(List<string> rowData)
    {
        if (rowData == null || rowData.Count == 0) return null;

        int idx, branchIdx, factionIdx, tagIdx, unitPrice, rarity;
        float health, armor, attackDamage, mobility, range, antiCavalry, energy, maxHealth;

        int.TryParse(rowData[0], out idx); // idx
        int.TryParse(rowData[3], out branchIdx); // branchIdx
        int.TryParse(rowData[6], out factionIdx); // factionIdx
        int.TryParse(rowData[8], out tagIdx); // tagIdx
        int.TryParse(rowData[9], out unitPrice); // unitPrice
        int.TryParse(rowData[10], out rarity); // rarity

        float.TryParse(rowData[11], out health); // health
        float.TryParse(rowData[12], out armor); // armor
        float.TryParse(rowData[13], out attackDamage); // attackDamage
        float.TryParse(rowData[14], out mobility); // mobility
        float.TryParse(rowData[15], out range); // range
        float.TryParse(rowData[16], out antiCavalry); // antiCavalry
        float.TryParse(rowData[17], out energy); // energy

        maxHealth = health; // 최대 체력은 기본적으로 health와 동일

        // 특성 및 기술 불리언 값 파싱
        bool lightArmor = rowData[18] == "TRUE";
        bool heavyArmor = rowData[19] == "TRUE";
        bool rangedAttack = rowData[20] == "TRUE";
        bool bluntWeapon = rowData[21] == "TRUE";
        bool pierce = rowData[22] == "TRUE";
        bool agility = rowData[23] == "TRUE";
        bool strongCharge = rowData[24] == "TRUE";
        bool perfectAccuracy = rowData[25] == "TRUE";
        bool slaughter = rowData[26] == "TRUE";
        bool solidarity = rowData[27] == "TRUE";
        bool bravery = rowData[28] == "TRUE";
        bool subjugation = rowData[29] == "TRUE";
        bool looting = rowData[30] == "TRUE";
        bool rapidFire = rowData[31] == "TRUE";
        bool burning = rowData[32] == "TRUE";
        bool thorns = rowData[33] == "TRUE";
        bool infinity = rowData[34] == "TRUE";
        bool impact = rowData[35] == "TRUE";
        bool cure = rowData[36] == "TRUE";
        bool bloodSucking = rowData[37] == "TRUE";

        // 흡혈과 돌격 사이 값이 없음 → blink를 빈 값으로 설정
        string blink = "빈";

        // 추가 능력치
        bool charge = rowData[39] == "TRUE";
        bool defense = rowData[40] == "TRUE";
        bool throwSpear = rowData[41] == "TRUE";
        bool guerrilla = rowData[42] == "TRUE";
        bool guard = rowData[43] == "TRUE";
        bool assassination = rowData[44] == "TRUE";
        bool drain = rowData[45] == "TRUE";
        bool overwhelm = rowData[46] == "TRUE";
        bool martyrdom = rowData[47] == "TRUE";
        bool scar = rowData[48] == "TRUE";
        bool revenge = rowData[49] == "TRUE";
        bool counterattack = rowData[50] == "TRUE";
        bool preemptiveStrike = rowData[51] == "TRUE";
        bool challenge = rowData[52] == "TRUE";
        bool smokeBomb = rowData[53] == "TRUE";

        return new RogueUnitDataBase(
            idx, rowData[1], rowData[2], branchIdx, rowData[4], rowData[5], factionIdx, rowData[7], tagIdx, unitPrice, rarity,
            health, armor, attackDamage, mobility, range, antiCavalry, energy,
            lightArmor, heavyArmor, rangedAttack, bluntWeapon, pierce, agility, strongCharge, perfectAccuracy, slaughter,
            solidarity, bravery, subjugation, looting, rapidFire, burning, thorns, infinity, impact, cure, bloodSucking,
            charge, defense, throwSpear, guerrilla, guard, assassination, drain, overwhelm,
            martyrdom, scar, revenge, counterattack, preemptiveStrike, challenge, smokeBomb,
            maxHealth, true, -1
        );
    }

}
