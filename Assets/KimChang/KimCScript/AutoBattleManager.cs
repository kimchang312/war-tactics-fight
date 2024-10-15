using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;


public class AutoBattleManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI myUnitContUI;      //�� ���ּ�UI�� ������ ����
    [SerializeField] private TextMeshProUGUI enemyUnitCountUI;  //��� ���ּ�UI�� ������ ����
    [SerializeField] private TextMeshProUGUI myUnitHpUI;        //�� ���� HP UI�� ������ ����
    [SerializeField] private TextMeshProUGUI enemyUnitHpUI;     //��� ���� HP UI�� ������ ����
    private float waittingTime=0.2f; //    

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
    private int AutoBattle(int[] _myUnitIds, int[] _enemyUnitIds)
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

        //������ ���� ����
        int myUnitMax= _myUnitIds.Length;
        int enemyUnitMax= _enemyUnitIds.Length;



        //���� �� UI �ʱ�ȭ �Լ� ȣ��
        UpdateUnitCount(_myUnitIds.Length,_enemyUnitIds.Length);

        //���� Hp UI �ʱ�ȭ �Լ� ȣ��
        UpdateUnitHp(myUnits[0].health, enemyUnits[0].health);

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

            UpdateUnitCount(myUnitMax - myUnitIndex, enemyUnitMax - enemyUnitIndex);
            if(myUnitIndex== myUnitMax)
            {
                UpdateUnitHp(myUnits[myUnitMax - 1].health, enemyUnits[enemyUnitIndex].health);
            }
            else if (enemyUnitIndex==enemyUnitMax)
            {
                UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitMax-1].health);
            }
            else
            {
                UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitIndex].health);
            }
            

            if (this.gameObject.activeInHierarchy)
            {
                StartCoroutine(WaitForSecondsExample());
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
    public int StartBattle(int[] _myUnitIds, int[] _enemyUnitIds)
    {
        int result = AutoBattle(_myUnitIds,_enemyUnitIds);
        return result;
    }

    //���� �� UI �ʱ�ȭ �Լ�
    private void UpdateUnitCount(int myUnitLength,int enemyUnitLength)
    {
        myUnitContUI.text= $"{myUnitLength}";
        enemyUnitCountUI.text = $"{enemyUnitLength}";
    }


    //���� Hp UI �ʱ�ȭ �Լ�
    private void UpdateUnitHp(float myUnitHp,float enemyHp)
    {
        if(myUnitHp < 0)
        {
            myUnitHp = 0;
        }
        if (enemyHp < 0)
        {
            enemyHp = 0;
        }
        myUnitHpUI.text=$"{ Math.Floor(myUnitHp)}";
        enemyUnitHpUI.text=$"{ Math.Floor(enemyHp)}";
    }


    //��ٸ��� �ϴ� �Լ�
    IEnumerator WaitForSecondsExample()
    {
       
        // 0.2�� ���
        yield return new WaitForSeconds(waittingTime);
       
    }

}
