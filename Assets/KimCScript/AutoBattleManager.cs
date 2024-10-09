using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoBattleManager : MonoBehaviour
{
    // ���� ����, ���� ����
    private int[] _myUnitIds = { 3, 4,5,6 };
    private int[] _enemyUnitIds = { 1, 2,8,7,6 };


    // ���� id��� ���� �����͸� �����ϴ� �Լ�
    // Tuple�� ����Ͽ� �� ���� �迭�� ��ȯ
    private (UnitDataBase[] myUnits, UnitDataBase[] enemyUnits) GetUnits(int[] myUnitIds, int[] enemyUnitIds)
    {
        // ���� ������ ���ֵ� ���� (List�� ���)
        List<UnitDataBase> myUnits = new List<UnitDataBase>();

        // ���� ���� ����
        List<UnitDataBase> enemyUnits = new List<UnitDataBase>();

        // �� ���� ID���� ������� ������ �����ͼ� MyUnits�� ����
        foreach (int unitId in myUnitIds)
        {
            UnitDataBase unit = UnitDataBase.GetUnitById(unitId);
            if (unit != null)
            {
                myUnits.Add(unit);  // List�� ���� �߰�
            }
        }

        // ���� ���� ID���� ������� ������ �����ͼ� enemyUnits�� ����
        foreach (int unitId in enemyUnitIds)
        {
            UnitDataBase unit = UnitDataBase.GetUnitById(unitId);
            if (unit != null)
            {
                enemyUnits.Add(unit);  // List�� ���� �߰�
            }
        }

        // List�� �迭�� ��ȯ�� �� Ʃ�÷� ��ȯ
        return (myUnits.ToArray(), enemyUnits.ToArray());
    }


    //�ڵ�����
    private int AutoBattle()
    {
        // ���� ���ط�
        float myDamage;
        // ���� ���ط�
        float enemyDamage;

        // ���� ���� ������ ȣ��
        var units = GetUnits(_myUnitIds, _enemyUnitIds);
        UnitDataBase[] myUnits = units.myUnits;
        UnitDataBase[] enemyUnits = units.enemyUnits;

        // �ε����� ����ؼ� ���� ������ �����ϴ� ���� ����
        int myUnitIndex = 0;
        int enemyUnitIndex = 0;

        // ���� �ݺ�
        while (myUnitIndex < myUnits.Length && enemyUnitIndex < enemyUnits.Length)
        {
            // 1������ ������ ���: ������ = ���ݷ� * (1 - ���� �尩 / (10 + ���� �尩))
            myDamage = myUnits[myUnitIndex].attackPower * (1 - (enemyUnits[enemyUnitIndex].armor / (10 + enemyUnits[enemyUnitIndex].armor)));
            enemyDamage = enemyUnits[enemyUnitIndex].attackPower * (1 - (myUnits[myUnitIndex].armor / (10 + myUnits[myUnitIndex].armor)));

            // ü�� ����
            myUnits[myUnitIndex].health -= enemyDamage;
            enemyUnits[enemyUnitIndex].health -= myDamage;

            // ������ ü���� 0 ������ ���, ���� �������� �Ѿ
            if (myUnits[myUnitIndex].health <= 0)
            {
                Debug.Log("�� ����" + myUnits[myUnitIndex].name+"���");
                myUnitIndex++;  // ���� �� ����
            }
            if (enemyUnits[enemyUnitIndex].health <= 0)
            {
                Debug.Log("�� ����" + enemyUnits[enemyUnitIndex].name + "���");
                enemyUnitIndex++;  // ���� �� ����
            }
        }

        // ���� ���� �� �¸� ���� �Ǵ�
        if (myUnitIndex < myUnits.Length && enemyUnitIndex >= enemyUnits.Length)
        {
            return 0;  // ���� �¸�
        }
        else if (enemyUnitIndex < enemyUnits.Length && myUnitIndex >= myUnits.Length)
        {
            return 1;  // ���� �¸�
        }
        else
        {
            return 2;  // ���� ��� ���
        }
    }


    //�ڵ������� �ٸ��ڵ忡�� ȣ���Ҽ� �ְԲ� ��� ȣ�����ִ� �Լ�
    public int StartBattle()
    {
        int result = AutoBattle();
        return result;
    }



}
