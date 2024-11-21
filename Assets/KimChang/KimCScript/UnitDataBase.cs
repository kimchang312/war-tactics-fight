using System.Collections.Generic;
using UnityEngine;

public class UnitDataBase
{
    // 기본 정보
    public int idx;            // 유닛의 고유 인덱스
    public string unitName;    // 유닛 이름
    public string unitBranch;  // 병종 이름
    public int branchIdx;      // 병종 인덱스
    public int unitId;         // 유닛 ID
    public string unitExplain; // 유닛 설명

    public string unitImg;        // 유닛 이미지 ID

    public string unitFaction; // 유닛이 속한 진영
    public int factionIdx;     // 진영 인덱스
    public int unitPrice;      // 유닛 가격

    // 스탯 정보
    public float health;       // 유닛 체력
    public float armor;        // 유닛 장갑
    public float attackDamage; // 공격력
    public float mobility;     // 기동성
    public float range;        // 사거리
    public float antiCavalry;  // 대기병 능력

    // 특성 및 기술
    public bool lightArmor;     // 경갑 유무
    public bool heavyArmor;     // 중갑 유무
    public bool rangedAttack;   // 원거리 공격 유무
    public bool bluntWeapon;    // 둔기 유무
    public bool pierce;         // 관통 유무
    public bool agility;        // 날쌤 유무
    public bool strongCharge;   // 강한 돌격 유무
    public bool perfectAccuracy;// 필중 유무


    public string blink = "빈";           //빈칸


    // 추가적인 능력치
    public bool charge;         // 돌격
    public bool defense;        // 방어
    public bool throwSpear;     // 창 던지기
    public bool slaughter;      // 학살
    public bool guerrilla;      // 게릴라
    public bool guard;          // 경호
    public bool assassination;  // 암살
    public bool drain;          // 흡수
    public bool overwhelm;      // 압도

    // 생성자
    public UnitDataBase(int idx, string unitName, string unitBranch, int branchIdx, int unitId,

                        string unitExplain, string unitImg, string unitFaction, int factionIdx, int unitPrice,

                        float health, float armor, float attackDamage, float mobility, float range, float antiCavalry,
                        bool lightArmor, bool heavyArmor, bool rangedAttack, bool bluntWeapon, bool pierce,
                        bool agility, bool strongCharge, bool perfectAccuracy, bool slaughter,
                        bool charge, bool defense, bool throwSpear,  bool guerrilla,
                        bool guard, bool assassination, bool drain, bool overwhelm,string blink)
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
        this.unitPrice = unitPrice;
        this.health = health;
        this.armor = armor;
        this.attackDamage = attackDamage;
        this.mobility = mobility;
        this.range = range;
        this.antiCavalry = antiCavalry;
        this.lightArmor = lightArmor;
        this.heavyArmor = heavyArmor;
        this.rangedAttack = rangedAttack;
        this.bluntWeapon = bluntWeapon;
        this.pierce = pierce;
        this.agility = agility;
        this.strongCharge = strongCharge;
        this.perfectAccuracy = perfectAccuracy;                      
        this.charge = charge;
        this.defense = defense;
        this.throwSpear = throwSpear;
        this.slaughter = slaughter;
        this.guerrilla = guerrilla;
        this.guard = guard;
        this.assassination = assassination;
        this.drain = drain;
        this.overwhelm = overwhelm;
        this.blink = blink;   //빈칸

    }


    public static UnitDataBase ConvertToUnitDataBase(List<string> rowData)
    {
        if (rowData == null || rowData.Count == 0) return null;


        int idx, branchIdx, unitId, factionIdx, unitPrice;

        float health, armor, attackDamage, mobility, range, antiCavalry; //chargeDamage = 0;
        bool lightArmor, heavyArmor, rangedAttack, bluntWeapon, pierce, agility, strongCharge, perfectAccuracy;
        bool charge, defense, throwSpear, slaughter, guerrilla, guard, assassination, drain, overwhelm;

        // 파싱 시도, 실패할 경우 기본값 할당
        int.TryParse(rowData[0], out idx); // idx
        int.TryParse(rowData[3], out branchIdx); // branchIdx
        int.TryParse(rowData[4], out unitId); // unitId

        //int.TryParse(rowData[6], out unitImg); // unitImg

        int.TryParse(rowData[8], out factionIdx); // factionIdx
        int.TryParse(rowData[9], out unitPrice); // unitPrice

        float.TryParse(rowData[10], out health); // health
        float.TryParse(rowData[11], out armor); // armor
        float.TryParse(rowData[12], out attackDamage); // attackDamage
        float.TryParse(rowData[13], out mobility); // mobility
        float.TryParse(rowData[14], out range); // range
        float.TryParse(rowData[15], out antiCavalry); // antiCavalry
        //float.TryParse(rowData[33], out chargeDamage); // chargeDamage

        // Bool 값 파싱 (문자열이 "True" 또는 "False"이어야 함)
        bool.TryParse(rowData[16], out lightArmor); // lightArmor
        bool.TryParse(rowData[17], out heavyArmor); // heavyArmor
        bool.TryParse(rowData[18], out rangedAttack); // rangedAttack
        bool.TryParse(rowData[19], out bluntWeapon); // bluntWeapon
        bool.TryParse(rowData[20], out pierce); // pierce
        bool.TryParse(rowData[21], out agility); // agility
        bool.TryParse(rowData[22], out strongCharge); // strongCharge
        bool.TryParse(rowData[23], out perfectAccuracy); // perfectAccuracy
        bool.TryParse(rowData[24], out slaughter); // slaughter
        bool.TryParse(rowData[25], out charge); // charge
        bool.TryParse(rowData[26], out defense); // defense
        bool.TryParse(rowData[27], out throwSpear); // throwSpear
        bool.TryParse(rowData[28], out guerrilla); // guerrilla
        bool.TryParse(rowData[29], out guard); // guard
        bool.TryParse(rowData[30], out assassination); // assassination
        bool.TryParse(rowData[31], out drain); // drain
        bool.TryParse(rowData[32], out overwhelm); // overwhelm

        // rowData에서 값을 추출하여 UnitDataBase 객체 생성
        return new UnitDataBase(
            idx, rowData[1], rowData[2], branchIdx, unitId,

            rowData[5], rowData[6], rowData[7], factionIdx, unitPrice,

            health, armor, attackDamage, mobility, range, antiCavalry,
            lightArmor, heavyArmor, rangedAttack, bluntWeapon, pierce, agility, strongCharge, perfectAccuracy,
            charge, defense, throwSpear, slaughter, guerrilla, guard, assassination, drain, overwhelm, "빈"
        );
    }

}
