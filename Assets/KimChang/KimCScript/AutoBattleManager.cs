using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;
using System.Threading.Tasks;


public class AutoBattleManager : MonoBehaviour
{

    [SerializeField] private AutoBattleUI autoBattleUI;       //UI ���� ��ũ��Ʈ

    private float waittingTime=0.2f; //    

    private GoogleSheetLoader sheetLoader = new GoogleSheetLoader();

    


    private async Task<(UnitDataBase[], UnitDataBase[])> GetUnits(int[] myUnitIds, int[] enemyUnitIds)
    {
        List<UnitDataBase> myUnits = new List<UnitDataBase>();
        List<UnitDataBase> enemyUnits = new List<UnitDataBase>();

        // ���� ��Ʈ �����͸� �ε�
        await sheetLoader.LoadGoogleSheetData();

        // �� ���� ID���� ������� ������ �����ͼ� MyUnits�� ����
        foreach (int unitId in myUnitIds)
        {
            List<string> rowData = sheetLoader.GetRowData(unitId); // rowData�� List<string> ����
           
            if (rowData != null)
            {
                // rowData�� UnitDataBase�� ��ȯ
                UnitDataBase unit = UnitDataBase.ConvertToUnitDataBase(rowData);
            
                if (unit != null)
                {
                    myUnits.Add(unit);  // List�� ���� �߰�
                }
            }
        }

        // ���� ���� ID���� ������� ������ �����ͼ� enemyUnits�� ����
        foreach (int unitId in enemyUnitIds)
        {
            List<string> rowData = sheetLoader.GetRowData(unitId); // rowData�� List<string> ����
            if (rowData != null)
            {
                // rowData�� UnitDataBase�� ��ȯ
                UnitDataBase unit = UnitDataBase.ConvertToUnitDataBase(rowData);
                if (unit != null)
                {
                    enemyUnits.Add(unit);  // List�� ���� �߰�
                }
            }
        }

        // List�� �迭�� ��ȯ�� �� Ʃ�÷� ��ȯ
        return (myUnits.ToArray(), enemyUnits.ToArray());
    }


    //�ڵ�����
    private async Task<int> AutoBattle(int[] _myUnitIds, int[] _enemyUnitIds)
    {

        // ���� ���ط�
        float myDamage;
        // ���� ���ط�
        float enemyDamage;

        // ���� ���� ������ ȣ��
        (UnitDataBase[] myUnits, UnitDataBase[] enemyUnits) = await GetUnits(_myUnitIds, _enemyUnitIds);


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

        UpdateUnitName(myUnits[0].unitBranch, enemyUnits[0].unitBranch);

        // ���� �ݺ�
        while (myUnitIndex < myUnits.Length && enemyUnitIndex < enemyUnits.Length)
        {
            // 1������ ������ ���: ������ = ���ݷ� * (1 - ���� �尩 / (10 + ���� �尩))
            myDamage = myUnits[myUnitIndex].attackDamage * (1 - (enemyUnits[enemyUnitIndex].armor / (10 + enemyUnits[enemyUnitIndex].armor)));
            enemyDamage = enemyUnits[enemyUnitIndex].attackDamage * (1 - (myUnits[myUnitIndex].armor / (10 + myUnits[myUnitIndex].armor)));

            // ü�� ����
            myUnits[myUnitIndex].health -= enemyDamage;
            enemyUnits[enemyUnitIndex].health -= myDamage;

            UpdateUnitName(myUnits[myUnitIndex].unitBranch, enemyUnits[enemyUnitIndex].unitBranch);
            UpdateUnitName(myUnits[myUnitIndex].unitBranch, enemyUnits[enemyUnitIndex].unitBranch);
            UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitIndex].health);

            // ������ ü���� 0 ������ ���, ���� �������� �Ѿ
            if (myUnits[myUnitIndex].health <= 0)
            {
                Debug.Log("�� ���� " + myUnits[myUnitIndex].unitName + "���");
                myUnitIndex++;  // ���� �� ����
                UpdateUnitCount(myUnitMax - myUnitIndex, enemyUnitMax - enemyUnitIndex);
                //UpdateUnitName(myUnits[myUnitIndex].unitBranch, enemyUnits[enemyUnitIndex].unitBranch);
            }
            if (enemyUnits[enemyUnitIndex].health <= 0)
            {
                Debug.Log("�� ���� " + enemyUnits[enemyUnitIndex].unitName + "���");
                enemyUnitIndex++;  // ���� �� ����
                UpdateUnitCount(myUnitMax - myUnitIndex, enemyUnitMax - enemyUnitIndex);
                //UpdateUnitName(myUnits[myUnitIndex].unitBranch, enemyUnits[enemyUnitIndex].unitBranch);
            }


            //���� ü�� UI����
            if(myUnitIndex == myUnitMax && enemyUnitIndex == enemyUnitMax)
            {
                UpdateUnitName(myUnits[myUnitMax - 1].unitBranch, enemyUnits[enemyUnitIndex-1].unitBranch);
                UpdateUnitHp(myUnits[myUnitMax - 1].health, enemyUnits[enemyUnitIndex-1].health);
            }
            else if (myUnitIndex== myUnitMax)
            {
                UpdateUnitName(myUnits[myUnitMax - 1].unitBranch, enemyUnits[enemyUnitIndex].unitBranch);
                UpdateUnitHp(myUnits[myUnitMax - 1].health, enemyUnits[enemyUnitIndex].health);
            }
            else if (enemyUnitIndex==enemyUnitMax)
            {
                UpdateUnitName(myUnits[myUnitIndex].unitBranch, enemyUnits[enemyUnitMax - 1].unitBranch);
                UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitMax-1].health);
            }
           
            

            await WaitForSecondsAsync(0.5f);

        }

        // ���� ���� �� �¸� ���� �Ǵ�
        if (myUnitIndex < myUnits.Length && enemyUnitIndex >= enemyUnits.Length)
        {
            Debug.Log($"���� �¸� {myUnits[myUnitIndex].unitName + myUnits[myUnitIndex].health}");
            return 0;  // ���� �¸�
        }
        else if (enemyUnitIndex < enemyUnits.Length && myUnitIndex >= myUnits.Length)
        {
            Debug.Log($"���� �¸� {enemyUnits[enemyUnitIndex].unitName + enemyUnits[enemyUnitIndex].health}");
            return 1;  // ���� �¸�
        }
        else
        {
            Debug.Log("���º�");
            return 2;  // ���� ��� ���
        }
    }


    //�ڵ������� �ٸ��ڵ忡�� ȣ���Ҽ� �ְԲ� ��� ȣ�����ִ� �Լ�
    public async Task<int> StartBattle(int[] _myUnitIds, int[] _enemyUnitIds)
    {
        if (autoBattleUI == null)
        {
            autoBattleUI = FindObjectOfType<AutoBattleUI>();
        }
        int result = await AutoBattle(_myUnitIds,_enemyUnitIds);
        return result;
    }

    //���� �� UI �ʱ�ȭ �Լ�
    private void UpdateUnitCount(int myUnitLength,int enemyUnitLength)
    {
          autoBattleUI.UpdateUnitCountUI(myUnitLength, enemyUnitLength);
       

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

        autoBattleUI.UpateUnitHPUI(MathF.Floor( myUnitHp),MathF.Floor( enemyHp));
        
 
    }

    //��ٸ��� �Լ�
    private async Task WaitForSecondsAsync(float seconds)
    {
        await Task.Delay((int)(seconds * 1000)); // �и��� ����
    }


    private void UpdateUnitName(string myUnitName, string enemyUnitName)
    {
        autoBattleUI.UpdateName(myUnitName, enemyUnitName);
    }

}
