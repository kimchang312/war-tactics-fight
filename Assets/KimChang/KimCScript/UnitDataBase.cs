using System.Collections.Generic;
using UnityEngine;

public class UnitDataBase
{
    // �⺻ ����
    public int idx;            // ������ ���� �ε���
    public string unitName;    // ���� �̸�
    public string unitBranch;  // ���� �̸�
    public int branchIdx;      // ���� �ε���
    public int unitId;         // ���� ID
    public string unitExplain; // ���� ����
    public string unitImg;        // ���� �̹��� ID
    public string unitFaction; // ������ ���� ����
    public int factionIdx;     // ���� �ε���
    public int unitPrice;      // ���� ����

    // ���� ����
    public float health;       // ���� ü��
    public float armor;        // ���� �尩
    public float attackDamage; // ���ݷ�
    public float mobility;     // �⵿��
    public float range;        // ��Ÿ�
    public float antiCavalry;  // ��⺴ �ɷ�

    // Ư�� �� ���
    public bool lightArmor;     // �氩 ����
    public bool heavyArmor;     // �߰� ����
    public bool rangedAttack;   // ���Ÿ� ���� ����
    public bool bluntWeapon;    // �б� ����
    public bool pierce;         // ���� ����
    public bool agility;        // ���� ����
    public bool strongCharge;   // ���� ���� ����
    public bool perfectAccuracy;// ���� ����

    public string blink = "��";           //��ĭ

    // �߰����� �ɷ�ġ
    public bool charge;         // ����
    public bool defense;        // ���
    public bool throwSpear;     // â ������
    public bool slaughter;      // �л�
    public bool guerrilla;      // �Ը���
    public bool guard;          // ��ȣ
    public bool assassination;  // �ϻ�
    public bool drain;          // ���
    public bool overwhelm;      // �е�

    // ������
    public UnitDataBase(int idx, string unitName, string unitBranch, int branchIdx, int unitId,
                        string unitExplain, string unitImg, string unitFaction, int factionIdx, int unitPrice,
                        float health, float armor, float attackDamage, float mobility, float range, float antiCavalry,
                        bool lightArmor, bool heavyArmor, bool rangedAttack, bool bluntWeapon, bool pierce,
                        bool agility, bool strongCharge, bool perfectAccuracy, string blink,
                        bool charge, bool defense, bool throwSpear, bool slaughter, bool guerrilla,
                        bool guard, bool assassination, bool drain, bool overwhelm)
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
        this.blink = blink;                         //��ĭ
        this.charge = charge;
        this.defense = defense;
        this.throwSpear = throwSpear;
        this.slaughter = slaughter;
        this.guerrilla = guerrilla;
        this.guard = guard;
        this.assassination = assassination;
        this.drain = drain;
        this.overwhelm = overwhelm;

    }


    public static UnitDataBase ConvertToUnitDataBase(List<string> rowData)
    {
        if (rowData == null || rowData.Count == 0) return null;

        int idx, branchIdx, unitId, factionIdx, unitPrice;
        float health, armor, attackDamage, mobility, range, antiCavalry, chargeDamage = 0;
        bool lightArmor, heavyArmor, rangedAttack, bluntWeapon, pierce, agility, strongCharge, perfectAccuracy;
        bool charge, defense, throwSpear, slaughter, guerrilla, guard, assassination, drain, overwhelm;

        // �Ľ� �õ�, ������ ��� �⺻�� �Ҵ�
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
        float.TryParse(rowData[33], out chargeDamage); // chargeDamage

        // Bool �� �Ľ� (���ڿ��� "True" �Ǵ� "False"�̾�� ��)
        bool.TryParse(rowData[16], out lightArmor); // lightArmor
        bool.TryParse(rowData[17], out heavyArmor); // heavyArmor
        bool.TryParse(rowData[18], out rangedAttack); // rangedAttack
        bool.TryParse(rowData[19], out bluntWeapon); // bluntWeapon
        bool.TryParse(rowData[20], out pierce); // pierce
        bool.TryParse(rowData[21], out agility); // agility
        bool.TryParse(rowData[22], out strongCharge); // strongCharge
        bool.TryParse(rowData[23], out perfectAccuracy); // perfectAccuracy
        bool.TryParse(rowData[25], out charge); // charge
        bool.TryParse(rowData[26], out defense); // defense
        bool.TryParse(rowData[27], out throwSpear); // throwSpear
        bool.TryParse(rowData[28], out slaughter); // slaughter
        bool.TryParse(rowData[29], out guerrilla); // guerrilla
        bool.TryParse(rowData[30], out guard); // guard
        bool.TryParse(rowData[31], out assassination); // assassination
        bool.TryParse(rowData[32], out drain); // drain
        bool.TryParse(rowData[33], out overwhelm); // overwhelm

        // rowData���� ���� �����Ͽ� UnitDataBase ��ü ����
        return new UnitDataBase(
            idx, rowData[1], rowData[2], branchIdx, unitId,
            rowData[5], rowData[6], rowData[7], factionIdx, unitPrice,
            health, armor, attackDamage, mobility, range, antiCavalry,
            lightArmor, heavyArmor, rangedAttack, bluntWeapon, pierce, agility, strongCharge, perfectAccuracy, "��",
            charge, defense, throwSpear, slaughter, guerrilla, guard, assassination, drain, overwhelm
        );
    }

}