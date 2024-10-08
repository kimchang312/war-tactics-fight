using System.Collections.Generic;
using UnityEngine;

public class UnitDataBase
{
    // �⺻ ����
    public string name;         // ���� �̸�
    public int branch;          // ���� ���� (��: 0 = ����)
    public int unitId;          // ���� ID
    public int unitImg;         // ���� �̹���
    public int faction;         // ���� ����
    public int unitPrice;       // ���� ����

    // ���� ����
    public float health;        // ���� ü��
    public float attackPower;   // ���� ���ݷ�
    public float armor;         // ���� �尩
    public float speed;         // ���� �⵿��
    public float range;         // ���� ��Ÿ�
    public float antiCavalry;   // ���� ��⺴ �ɷ�

    // Ư�� �� ���
    public bool lightArmor;     // �氩 ����
    public bool heavyArmor;     // �߰� ����   
    public bool rangedAttack;   // ���Ÿ� ���� ����
    public bool bluntWeapon;    // �б� ����
    public bool pierce;         // ���� ����
    public bool agility;        // ���� ����
    public bool strongCharge;   // ���� ���� ����
    public bool perfectAccuracy;// ���� ����

    // ������
    public UnitDataBase(string name, int branch, int faction,
                float health, float armor, float attackPower,
                float speed, float range, float antiCavalry,
                bool lightArmor, bool heavyArmor, bool rangedAttack, bool bluntWeapon, bool pierce, bool agility,
                bool strongCharge, bool perfectAccuracy)
    {
        this.name = name;
        this.faction = faction;
        this.branch = branch;
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

    // ���� ���� �Լ���
    private static UnitDataBase AddSpearman()
    {
        return new UnitDataBase("�κ��� â��", 0, 0, 110.0f, 3.0f, 30.0f, 4.0f, 1.0f, 25.0f,
            true, false, false, false, false, false, false, false);
    }

    private static UnitDataBase AddPikeman()
    {
        return new UnitDataBase("��â��", 0, 0, 130.0f, 3.0f, 35.0f, 3.0f, 1.0f, 35.0f,
            true, false, false, false, false, false, false, false);
    }

    private static UnitDataBase AddSwordsman()
    {
        return new UnitDataBase("����", 0, 0, 130.0f, 3.0f, 60.0f, 5.0f, 1.0f, 0.0f,
            true, false, false, false, false, false, false, false);
    }

    private static UnitDataBase AddMilitiaBowman()
    {
        return new UnitDataBase("�κ��� �ú�", 1, 0, 90.0f, 1.0f, 20.0f, 5.0f, 3.0f, 0.0f,
            true, false, true, false, false, false, false, false);
    }

    private static UnitDataBase AddSkirmisher()
    {
        return new UnitDataBase("ô�ĺ�", 1, 0, 100.0f, 1.0f, 25.0f, 6.0f, 3.0f, 0.0f,
            true, false, true, false, false, false, false, false);
    }

    private static UnitDataBase AddShieldBearer()
    {
        return new UnitDataBase("���к�", 2, 0, 150.0f, 9.0f, 35.0f, 1.0f, 1.0f, 0.0f,
            false, true, false, false, false, false, false, false);
    }

    private static UnitDataBase AddMaceBearer()
    {
        return new UnitDataBase("ö��", 2, 0, 130.0f, 6.0f, 60.0f, 2.0f, 1.0f, 0.0f,
            false, true, false, true, false, false, false, false);
    }

    private static UnitDataBase AddAssassin()
    {
        return new UnitDataBase("�ϻ�ܿ�", 3, 0, 100.0f, 1.0f, 100.0f, 6.0f, 1.0f, 0.0f,
            true, false, false, false, false, true, false, false);
    }

    private static UnitDataBase AddLancer()
    {
        return new UnitDataBase("â�⺴", 4, 0, 170.0f, 3.0f, 40.0f, 10.0f, 0.0f, 15.0f,
            true, false, false, false, false, false, false, false);
    }

    private static UnitDataBase AddHorseArcher()
    {
        return new UnitDataBase("�ñ⺴", 4, 0, 160.0f, 2.0f, 35.0f, 9.0f, 2.0f, 0.0f,
            true, false, true, false, false, false, false, false);
    }

    private static UnitDataBase AddGuard()
    {
        return new UnitDataBase("ģ����", 5, 0, 180.0f, 7.0f, 60.0f, 7.0f, 1.0f, 10.0f,
            false, true, false, false, true, false, false, false);
    }

    private static UnitDataBase AddJavelinThrower()
    {
        return new UnitDataBase("��â��", 0, 1, 130.0f, 5.0f, 40.0f, 3.0f, 1.0f, 15.0f,
            true, false, false, false, false, false, false, false);
    }

    private static UnitDataBase AddDoubleAxeBearer()
    {
        return new UnitDataBase("��� ������", 0, 1, 140.0f, 1.0f, 90.0f, 6.0f, 1.0f, 0.0f,
            true, false, false, false, false, false, false, false);
    }

    private static UnitDataBase AddVanguard()
    {
        return new UnitDataBase("������", 2, 1, 150.0f, 7.0f, 80.0f, 3.0f, 1.0f, 0.0f,
            false, true, false, false, false, false, false, false);
    }

    private static UnitDataBase AddRaider()
    {
        return new UnitDataBase("������", 4, 1, 190.0f, 3.0f, 30.0f, 9.0f, 1.0f, 0.0f,
            true, false, false, false, false, false, true, false);
    }

    private static UnitDataBase AddDrakeRider()
    {
        return new UnitDataBase("�巹��ũ ���", 5, 1, 240.0f, 7.0f, 120.0f, 10.0f, 1.0f, 30.0f,
            false, true, false, false, false, false, false, true);
    }

    // ���� id�� ������� ������ ��ȯ�ϴ� �Լ�
    public static UnitDataBase GetUnitById(int unitId)
    {
        switch (unitId)
        {
            case 1: return AddSpearman();
            case 2: return AddPikeman();
            case 3: return AddSwordsman();
            case 4: return AddMilitiaBowman();
            case 5: return AddSkirmisher();
            case 6: return AddShieldBearer();
            case 7: return AddMaceBearer();
            case 8: return AddAssassin();
            case 9: return AddLancer();
            case 10: return AddHorseArcher();
            case 11: return AddGuard();
            case 12: return AddJavelinThrower();
            case 13: return AddDoubleAxeBearer();
            case 14: return AddVanguard();
            case 15: return AddRaider();
            case 16: return AddDrakeRider();
            default: return null;  // ��ȿ���� ���� unitId�� ��� null ��ȯ
        }
    }
}
