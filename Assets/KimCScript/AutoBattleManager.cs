using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoBattleManager : MonoBehaviour
{
    // ���� ����, ���� ����
    private int[] _myUnitIds = { 1, 2 };
    private int[] _enemyUnitIds = { 3, 4 };


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

    private int AutoBattle()
    {
        //var= GetUnits(_myUnitIds, _enemyUnitIds);

        return 0;
    }

}
