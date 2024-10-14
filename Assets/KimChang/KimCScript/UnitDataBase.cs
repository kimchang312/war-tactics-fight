using System.Collections.Generic;
using UnityEngine;

public class UnitDataBase
{
    // 기본 정보
    public string name;         // 유닛 이름
    //public int branch;          // 유닛 병종 (예: 0 = 보병)
    public int unitId;          // 유닛 ID
    public int unitImg;         // 유닛 이미지
    public int faction;         // 유닛 진영
    public int unitPrice;       // 유닛 가격

    // 스탯 정보
    public float health;        // 유닛 체력
    public float attackPower;   // 유닛 공격력
    public float armor;         // 유닛 장갑
    public float speed;         // 유닛 기동력
    public float range;         // 유닛 사거리
    public float antiCavalry;   // 유닛 대기병 능력

    // 특성 및 기술
    public bool lightArmor;     // 경갑 유무
    public bool heavyArmor;     // 중갑 유무   
    public bool rangedAttack;   // 원거리 공격 유무
    public bool bluntWeapon;    // 둔기 유무
    public bool pierce;         // 관통 유무
    public bool agility;        // 날쌤 유무
    public bool strongCharge;   // 강한 돌격 유무
    public bool perfectAccuracy;// 필중 유무

    //유닛 생성자
    public UnitDataBase(string name, /*int branch,*/ int faction,
                float health, float armor, float attackPower,
                float speed, float range, float antiCavalry,
                bool lightArmor, bool heavyArmor, bool rangedAttack, bool bluntWeapon, bool pierce, bool agility,
                bool strongCharge, bool perfectAccuracy)
    {
        this.name = name;
        this.faction = faction;
        //this.branch = branch;
        this.health = health;
        this.armor = armor;
        this.attackPower = attackPower;
        this.speed = speed;
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
    }

    // 유닛 생성 함수들
    //이름, 진영, 체력, 장갑, 공격력, 기동력, 사거리, 대기병, 경갑, 중갑, 원거리 공격, 둔기, 관통, 날쌤, 강한 돌격, 필중
    private static UnitDataBase AddSpearman()
    {
        return new UnitDataBase("민병대 창병",  0, 110.0f, 3.0f, 30.0f, 4.0f, 1.0f, 25.0f,
            true, false, false, false, false, false, false, false);
    }

    private static UnitDataBase AddPikeman()
    {
        return new UnitDataBase("장창병",  0, 130.0f, 3.0f, 35.0f, 3.0f, 1.0f, 35.0f,
            true, false, false, false, false, false, false, false);
    }

    private static UnitDataBase AddSwordsman()
    {
        return new UnitDataBase("도병",  0, 130.0f, 3.0f, 60.0f, 5.0f, 1.0f, 0.0f,
            true, false, false, false, false, false, false, false);
    }

    private static UnitDataBase AddMilitiaBowman()
    {
        return new UnitDataBase("민병대 궁병",  0, 90.0f, 1.0f, 20.0f, 5.0f, 3.0f, 0.0f,
            true, false, true, false, false, false, false, false);
    }

    private static UnitDataBase AddSkirmisher()
    {
        return new UnitDataBase("척후병",  0, 100.0f, 1.0f, 25.0f, 6.0f, 3.0f, 0.0f,
            true, false, true, false, false, false, false, false);
    }

    private static UnitDataBase AddShieldBearer()
    {
        return new UnitDataBase("방패병",  0, 150.0f, 9.0f, 35.0f, 1.0f, 1.0f, 0.0f,
            false, true, false, false, false, false, false, false);
    }

    private static UnitDataBase AddMaceBearer()
    {
        return new UnitDataBase("철퇴병",  0, 130.0f, 6.0f, 60.0f, 2.0f, 1.0f, 0.0f,
            false, true, false, true, false, false, false, false);
    }

    private static UnitDataBase AddAssassin()
    {
        return new UnitDataBase("암살단원",  0, 100.0f, 1.0f, 100.0f, 6.0f, 1.0f, 0.0f,
            true, false, false, false, false, true, false, false);
    }

    private static UnitDataBase AddLancer()
    {
        return new UnitDataBase("창기병",  0, 170.0f, 3.0f, 40.0f, 10.0f, 0.0f, 15.0f,
            true, false, false, false, false, false, false, false);
    }

    private static UnitDataBase AddHorseArcher()
    {
        return new UnitDataBase("궁기병",  0, 160.0f, 2.0f, 35.0f, 9.0f, 2.0f, 0.0f,
            true, false, true, false, false, false, false, false);
    }

    private static UnitDataBase AddGuard()
    {
        return new UnitDataBase("친위대",   0, 180.0f, 7.0f, 60.0f, 7.0f, 1.0f, 10.0f,
            false, true, false, false, true, false, false, false);
    }

    private static UnitDataBase AddJavelinThrower()
    {
        return new UnitDataBase("투창병",  1, 130.0f, 5.0f, 40.0f, 3.0f, 1.0f, 15.0f,
            true, false, false, false, false, false, false, false);
    }

    private static UnitDataBase AddDoubleAxeBearer()
    {
        return new UnitDataBase("양손 도끼병", 1, 140.0f, 1.0f, 90.0f, 6.0f, 1.0f, 0.0f,
            true, false, false, false, false, false, false, false);
    }

    private static UnitDataBase AddVanguard()
    {
        return new UnitDataBase("선봉장",  1, 150.0f, 7.0f, 80.0f, 3.0f, 1.0f, 0.0f,
            false, true, false, false, false, false, false, false);
    }

    private static UnitDataBase AddRaider()
    {
        return new UnitDataBase("습격자",  1, 190.0f, 3.0f, 30.0f, 9.0f, 1.0f, 0.0f,
            true, false, false, false, false, false, true, false);
    }

    private static UnitDataBase AddDrakeRider()
    {
        return new UnitDataBase("드레이크 기수",  1, 240.0f, 7.0f, 120.0f, 10.0f, 1.0f, 30.0f,
            false, true, false, false, false, false, false, true);
    }



    // 유닛 id를 기반으로 유닛을 반환하는 함수
    public static UnitDataBase GetUnitById(int unitId)
    {
        switch (unitId)
        {
            case 0: return AddSpearman();
            case 1: return AddPikeman();
            case 2: return AddSwordsman();
            case 3: return AddMilitiaBowman();
            case 4: return AddSkirmisher();
            case 5: return AddShieldBearer();
            case 6: return AddMaceBearer();
            case 7: return AddAssassin();
            case 8: return AddLancer();
            case 9: return AddHorseArcher();
            case 10: return AddGuard();
            case 11: return AddJavelinThrower();
            case 12: return AddDoubleAxeBearer();
            case 13: return AddVanguard();
            case 14: return AddRaider();
            case 15: return AddDrakeRider();
            default: return null;  // 유효하지 않은 unitId일 경우 null 반환
        }
    }


}
