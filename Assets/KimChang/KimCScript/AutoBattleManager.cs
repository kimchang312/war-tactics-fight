using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Random = UnityEngine.Random;

public class AutoBattleManager : MonoBehaviour
{

    [SerializeField] private AutoBattleUI autoBattleUI;       //UI ���� ��ũ��Ʈ

    private float waittingTime=0.2f; //    

    private GoogleSheetLoader sheetLoader = new GoogleSheetLoader();
    
    private float heavyArmorValue=15.0f;                //�߰� ���� ������ ���ҷ�
    private float bluntWeaponValue = 15.0f;             //�б� ���� ������
    private float throwSpearValue = 50.0f;              //��â ���� ������
    private float overwhelmValue = 1.0f;                //���� �⵿�� ������
    private float strongChargeValue = 0.5f;             //���� ���� ������ ���
    private float defenseValue = 15.0f;                 //���� �¼� ���� ������ ���ҷ�
    private float slaughterValue = 10.0f;               //���� ���� ������
    private float assassinationValue = 2.0f;            //�ϻ� ������ ����
    private float drainHealValue = 20.0f;               //���� ���� ȸ����
    private float drainGainAttackValue = 10.0f;         //���� ���� ���ݷ� ������

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
        // ���� ���� ������ ȣ��
        (UnitDataBase[] myUnits, UnitDataBase[] enemyUnits) = await GetUnits(_myUnitIds, _enemyUnitIds);

        // �ε����� ����ؼ� ���� ������ �����ϴ� ���� ����
        int myUnitIndex = 0;
        int enemyUnitIndex = 0;

        //���� ������ 0�غ�, 1 ����, 2 ����
        int fightPhase = 0;

        //������ ���� ����
        int myUnitMax = _myUnitIds.Length;
        int enemyUnitMax = _enemyUnitIds.Length;

        //���� ���Ÿ� ���� ��ġ
        List<int> myRangeUnits= new List<int>();
        //��� ���Ÿ� ���� ��ġ
        List<int> enemyRangeUnits= new List<int>();
        //���� ���� �ִ� ü��
        float myUnitMaxHp = myUnits[myUnitIndex].health;
        //��� ���� �ִ� ü��
        float enemyUnitMaxHp=enemyUnits[enemyUnitIndex].health;

        // ���� ���ط�
        float myDamage= 0;
        // ���� ���ط�
        float enemyDamage = 0;

        // ù ����
        bool isFirstAttack = true;
        
        // ���� �߰� ���
        bool isMyHeavyArmor = CalculateHeavyArmor(myUnits[myUnitIndex].heavyArmor, enemyUnits[enemyUnitIndex].branchIdx);
        //���� �߰� ���
        bool isEnemyHeavyArmor=CalculateHeavyArmor(enemyUnits[myUnitIndex].heavyArmor, myUnits[enemyUnitIndex].branchIdx);

        //���� �б� ���
        bool isMyBluntWeapon = CalculateBluntWeapon(myUnits[myUnitIndex].bluntWeapon, enemyUnits[enemyUnitIndex].heavyArmor);
        //���� �б� ���
        bool isEnemyBluntWeapon = CalculateBluntWeapon(enemyUnits[myUnitIndex].bluntWeapon, myUnits[enemyUnitIndex].heavyArmor);

        //���� ���Ÿ� ���� ��ġ�� �ʱ�ȭ
        for (int i = 0; i < myUnitMax; i++)
        {
            if (myUnits[i].rangedAttack)
            {
                myRangeUnits.Add(i);
            }
        }
        //��� ���Ÿ� ���� ��ġ�� �ʱ�ȭ
        for (int i = 0; i < enemyUnitMax; i++)
        {
            if (enemyUnits[i].rangedAttack)
            {
                enemyRangeUnits.Add(i);
            }
        }

        //���� �� UI �ʱ�ȭ �Լ� ȣ��
        UpdateUnitCount(_myUnitIds.Length,_enemyUnitIds.Length);

        //���� Hp UI �ʱ�ȭ �Լ� ȣ��
        UpdateUnitHp(myUnits[0].health, enemyUnits[0].health);

        UpdateUnitName(myUnits[0].unitBranch, enemyUnits[0].unitBranch);

        // ���� �ݺ�
        while (myUnitIndex < myUnits.Length && enemyUnitIndex < enemyUnits.Length)
        {
            UpdateUnitName(myUnits[myUnitIndex].unitBranch, enemyUnits[enemyUnitIndex].unitBranch);

            //���� �߰� ���ط�
            float myAddDamage = 0;
            //���� �߰� ���ط�
            float enemyAddDamage = 0;
            //���� ���� ���� ���ҷ�
            float myReduceDamage= 0;
            //���� ���� ���� ���ҷ�
            float enemyReduceDamage=0;
            //���� ������ ����
            float myMultiDamage=1.0f;
            //���� ������ ����
            float enemyMultiDamage = 1.0f;

            if (isFirstAttack)      //ù ������ ��
            {
                switch (fightPhase) //����
                {   
                    case 0:         //�غ�
                        //�ϻ� ��� hp ������ �⺻�� 1
                        float myBackUnitHP = 1;
                        float enemyBackUnitHP = 1;
                        //�� ��� ����
                        //����
                        if (myUnits[myUnitIndex].overwhelm)
                        {
                            enemyUnits[enemyUnitIndex].mobility = overwhelmValue;
                        }
                        //�ϻ�
                        if (myUnits[myUnitIndex].assassination ||  enemyUnitMax- enemyUnitIndex > 1)//�ϻ� �ְ� ��� ���� 1���� �ʰ�
                        {
                            List<float> enemyUnitHealth = new List<float>();
                            foreach (var unit in enemyUnits)
                            {
                                enemyUnitHealth.Add(unit.health);   //��� ���� ü�� ������� �迭ȭ
                            }
                            //��ȣ
                            if (enemyUnits[enemyUnitIndex].guard)   
                            {
                                if (!CalculateAccuracy(enemyUnits[enemyUnitIndex], myUnits[myUnitIndex].perfectAccuracy))    //��� ȸ�� ���� ��
                                {
                                    enemyUnits[enemyUnitIndex].health -= myUnits[myUnitIndex].attackDamage*(1 - (enemyUnits[enemyUnitIndex].armor) / (10 + enemyUnits[enemyUnitIndex].armor)) * assassinationValue;
                                }
                            }
                            else
                            {

                                enemyBackUnitHP= enemyUnits[CalculateAssassination(enemyUnitHealth)].health ;
                                enemyBackUnitHP -= myUnits[myUnitIndex].attackDamage * assassinationValue;
                            }

                            
                        }
                        //��â
                        if (myUnits[myUnitIndex].throwSpear)
                        {
                            if (!CalculateAccuracy(enemyUnits[enemyUnitIndex], myUnits[myUnitIndex].perfectAccuracy))    //��� ȸ�� ���� ��
                            {
                                enemyUnits[enemyUnitIndex].health -= throwSpearValue;
                            }

                        }
                        

                        //�� ���
                        //����
                        if (enemyUnits[enemyUnitIndex].overwhelm)
                        {
                            myUnits[myUnitIndex].mobility = overwhelmValue;
                        }

                        //�ϻ�
                        if (enemyUnits[enemyUnitIndex].assassination || myUnitMax - myUnitIndex > 1)//�ϻ� �ְ� ��� ���� 1���� �ʰ�
                        {
                            List<float> myUnitHealth = new List<float>();
                            foreach (var unit in enemyUnits)
                            {
                                myUnitHealth.Add(unit.health);   //��� ���� ü�� ������� �迭ȭ
                            }
                            //��ȣ
                            if (myUnits[myUnitIndex].guard)
                            {
                                if (!CalculateAccuracy(myUnits[myUnitIndex], enemyUnits[enemyUnitIndex].perfectAccuracy))    //���� ȸ�� ���� ��
                                {
                                    myUnits[enemyUnitIndex].health -= enemyUnits[enemyUnitIndex].attackDamage * (1 - (myUnits[myUnitIndex].armor) / (10 + myUnits[myUnitIndex].armor)) * assassinationValue;
                                }
                            }
                            else
                            {
                                myBackUnitHP= enemyUnits[CalculateAssassination(myUnitHealth)].health ;
                                myBackUnitHP -= enemyUnits[myUnitIndex].attackDamage * assassinationValue;
                            }
                        }

                        //��â
                        if (enemyUnits[enemyUnitIndex].throwSpear)
                        {
                            if (!CalculateAccuracy(myUnits[myUnitIndex], enemyUnits[enemyUnitIndex].perfectAccuracy))    //���� ȸ�� ���� ��
                            {
                                myUnits[myUnitIndex].health -= throwSpearValue;
                            }
                            
                        }


                        UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitIndex].health);
                        await WaitForSecondsAsync(0.5f);

                        //���� ����� �ϴ� ���ο� ������
                        if (enemyUnits[enemyUnitIndex].health <= 0 || myUnits[myUnitIndex].health <= 0)
                        {
                            //���� ����
                            if (myUnits[myUnitIndex].guerrilla && myUnitMax - myUnitIndex > 1 &&(enemyUnits[enemyUnitIndex].health<=0 || enemyBackUnitHP<=0) )
                            {
                                UnitDataBase thisUnit = myUnits[myUnitIndex];
                                myUnits[myUnitIndex] = myUnits[myUnitIndex + 1];
                                myUnits[myUnitIndex + 1] = thisUnit;
                            }
                            //���� ����
                            if (myUnits[myUnitIndex].drain && (enemyUnits[enemyUnitIndex].health <= 0 || enemyBackUnitHP <= 0) && myUnits[myUnitIndex].health>0)
                            {
                                myUnits[myUnitIndex].health = drainHealValue + myUnits[myUnitIndex].health > myUnitMaxHp ? myUnitMaxHp : drainHealValue + myUnits[myUnitIndex].health;
                            }
                            //��� ����
                            if (enemyUnits[enemyUnitIndex].guerrilla && enemyUnitMax - enemyUnitIndex > 1 && (myUnits[myUnitIndex].health <= 0 || myBackUnitHP <= 0))
                            {
                                UnitDataBase thisUnit = enemyUnits[enemyUnitIndex];
                                enemyUnits[myUnitIndex] = enemyUnits[enemyUnitIndex + 1];
                                enemyUnits[enemyUnitIndex + 1] = thisUnit;
                            }
                            //��� ����
                            if (enemyUnits[enemyUnitIndex].drain && (myUnits[myUnitIndex].health <= 0 || myBackUnitHP <= 0) && enemyUnits[enemyUnitIndex].health>0)
                            {
                                enemyUnits[enemyUnitIndex].health = drainHealValue + enemyUnits[enemyUnitIndex].health > enemyUnitMaxHp ? enemyUnitMaxHp : drainHealValue + enemyUnits[enemyUnitIndex].health;
                            }
                            break;
                        }
                            continue;
                    case 1:         //�浹
                        //�� ���� �浹
                        if (!CalculateAccuracy(enemyUnits[enemyUnitIndex], myUnits[myUnitIndex].perfectAccuracy))    //��� ȸ�� ���� ��
                        {
                            // ����
                            myMultiDamage = CalculateCharge(myUnits[myUnitIndex].charge, myUnits[myUnitIndex].mobility);
                            //���� ����
                            if (myUnits[myUnitIndex].strongCharge)
                            {
                                myMultiDamage += strongChargeValue;
                            }
                            //���� �¼� ��� ������ ����
                            if (myUnits[myUnitIndex].charge && enemyUnits[enemyUnitIndex].defense)
                            {
                                myReduceDamage += defenseValue;
                            }
                            //���� �¼� ���� ������ ����
                            if (enemyUnits[enemyUnitIndex].charge && myUnits[myUnitIndex].defense)
                            {
                                myAddDamage += defenseValue;
                            }
                            //�б�
                            if (isMyBluntWeapon)
                            {
                                myAddDamage += bluntWeaponValue;
                            }
                            //����
                            if (myUnits[myUnitIndex].slaughter && enemyUnits[enemyUnitIndex].lightArmor)
                            {
                                myAddDamage += slaughterValue;
                            }
                            //��⺴
                            if (enemyUnits[enemyUnitIndex].branchIdx == 5)
                            {
                                myAddDamage += myUnits[myUnitIndex].antiCavalry;
                            }
                            //����
                            if (myUnits[myUnitIndex].pierce)
                            {
                                myDamage = myUnits[myUnitIndex].attackDamage * myMultiDamage + myAddDamage- myReduceDamage;
                            }
                            else
                            {
                                myDamage = myUnits[myUnitIndex].attackDamage * (1 - (enemyUnits[enemyUnitIndex].armor) / (10 + enemyUnits[enemyUnitIndex].armor)) * myMultiDamage + myAddDamage - myReduceDamage;
                            }
                            if (myDamage > 0)
                            {
                                enemyUnits[enemyUnitIndex].health -= myDamage;
                            }
                        }

                        //��� ���� �浹
                        if (!CalculateAccuracy(myUnits[myUnitIndex], enemyUnits[enemyUnitIndex].perfectAccuracy))        //���� ȸ�� ���� ��
                        {
                            
                                // ����
                                enemyMultiDamage = CalculateCharge(enemyUnits[enemyUnitIndex].charge, enemyUnits[enemyUnitIndex].mobility);
                                //���� ����
                                if (enemyUnits[enemyUnitIndex].strongCharge)
                                {
                                    enemyMultiDamage += strongChargeValue;
                                }
                                //���� �¼� ���� ������ ����
                                if (enemyUnits[enemyUnitIndex].charge && myUnits[myUnitIndex].defense)
                                {
                                    enemyReduceDamage += defenseValue;
                                }
                                //���� �¼� ��� ������ ����
                                if (myUnits[myUnitIndex].charge && enemyUnits[enemyUnitIndex].defense)
                                {
                                    myAddDamage += defenseValue;
                                }
                                //�б�
                                if (isEnemyBluntWeapon)
                                {
                                    enemyAddDamage += bluntWeaponValue;
                                }
                                //����
                                if (enemyUnits[enemyUnitIndex].slaughter && myUnits[myUnitIndex].lightArmor)
                                {
                                    enemyAddDamage += slaughterValue;
                                }
                                //��⺴
                                if (myUnits[myUnitIndex].branchIdx == 5)
                                {
                                    enemyAddDamage += enemyUnits[enemyUnitIndex].antiCavalry;
                                }
                                //����
                                if (enemyUnits[enemyUnitIndex].pierce)
                                {
                                    enemyDamage = enemyUnits[enemyUnitIndex].attackDamage * enemyMultiDamage + enemyAddDamage - enemyReduceDamage;
                                }
                                else
                                {
                                    enemyDamage = enemyUnits[enemyUnitIndex].attackDamage * (1 - (myUnits[myUnitIndex].armor) / (10 + myUnits[myUnitIndex].armor)) * enemyMultiDamage + enemyAddDamage - enemyReduceDamage;
                                }
                                if (enemyDamage > 0)
                                {
                                    myUnits[myUnitIndex].health -= enemyDamage;
                                }
                            }

                        UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitIndex].health);
                        await WaitForSecondsAsync(0.5f);

                        //���� ����� �ϴ� ���ο� ������
                        if (enemyUnits[enemyUnitIndex].health <= 0 || myUnits[myUnitIndex].health <= 0)
                        {
                            //���� ����
                            if (myUnits[myUnitIndex].guerrilla && myUnitMax - myUnitIndex > 1 && enemyUnits[enemyUnitIndex].health <= 0)
                            {
                                UnitDataBase thisUnit = myUnits[myUnitIndex];
                                myUnits[myUnitIndex] = myUnits[myUnitIndex + 1];
                                myUnits[myUnitIndex + 1] = thisUnit;
                            }
                            //���� ����
                            if (myUnits[myUnitIndex].drain && enemyUnits[enemyUnitIndex].health <= 0 && myUnits[myUnitIndex].health > 0)
                            {
                                myUnits[myUnitIndex].health = drainHealValue + myUnits[myUnitIndex].health > myUnitMaxHp ? myUnitMaxHp : drainHealValue + myUnits[myUnitIndex].health;
                            }
                            //��� ����
                            if (enemyUnits[enemyUnitIndex].guerrilla && enemyUnitMax - enemyUnitIndex > 1 && myUnits[myUnitIndex].health <= 0)
                            {
                                UnitDataBase thisUnit = enemyUnits[enemyUnitIndex];
                                enemyUnits[myUnitIndex] = enemyUnits[enemyUnitIndex + 1];
                                enemyUnits[enemyUnitIndex + 1] = thisUnit;
                            }
                            //��� ����
                            if (enemyUnits[enemyUnitIndex].drain && myUnits[myUnitIndex].health <= 0 && enemyUnits[enemyUnitIndex].health > 0)
                            {
                                enemyUnits[enemyUnitIndex].health = drainHealValue + enemyUnits[enemyUnitIndex].health > enemyUnitMaxHp ? enemyUnitMaxHp : drainHealValue + enemyUnits[enemyUnitIndex].health;
                            }
                            break;
                        }


                        continue;
                    case 2:         //����
                        //�� ���� ����
                        //���Ÿ� ����
                        for (int i = myRangeUnits[0]; i < myRangeUnits.Count; i++)
                        {
                            if(i > myUnitIndex && myUnits[i].range - (i - myUnitIndex) >= 1 )
                            {
                                if (!CalculateAccuracy(enemyUnits[enemyUnitIndex], myUnits[myRangeUnits[i]].perfectAccuracy))    //��� ȸ�� ���� ��
                                {
                                    //�߰�
                                    if (isEnemyHeavyArmor)
                                    {
                                        if (myUnits[myRangeUnits[i]].attackDamage > heavyArmorValue)
                                        {
                                            enemyUnits[enemyUnitIndex].health -= myUnits[myRangeUnits[i]].attackDamage+ heavyArmorValue;
                                        }
                                    }
                                    else
                                    {
                                        enemyUnits[enemyUnitIndex].health -= myUnits[myRangeUnits[i]].attackDamage;
                                    }
                                }
                            }

                            UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitIndex].health);
                            await WaitForSecondsAsync(0.5f);
                        }

                        //��� ���� ����
                        //���Ÿ� ����
                        for (int i = enemyRangeUnits[0]; i < enemyRangeUnits.Count; i++)
                        {
                            if (i > enemyUnitIndex && enemyUnits[i].range - (i - enemyUnitIndex) >= 1)
                            {
                                if (!CalculateAccuracy(myUnits[myUnitIndex], enemyUnits[enemyRangeUnits[i]].perfectAccuracy))    //��� ȸ�� ���� ��
                                {
                                    //�߰�
                                    if (isMyHeavyArmor)
                                    {
                                        if (enemyUnits[enemyRangeUnits[i]].attackDamage > heavyArmorValue)
                                        {
                                            myUnits[myUnitIndex].health -= enemyUnits[enemyRangeUnits[i]].attackDamage + heavyArmorValue;
                                        }
                                    }
                                    else
                                    {
                                        myUnits[myUnitIndex].health -= enemyUnits[enemyRangeUnits[i]].attackDamage;
                                    }
                                }
                            }

                            UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitIndex].health);
                            await WaitForSecondsAsync(0.5f);
                        }

                        //���� ����� 
                        if (enemyUnits[enemyUnitIndex].health <= 0 || myUnits[myUnitIndex].health <= 0)
                        {
                            //���� ����
                            if (myUnits[myUnitIndex].guerrilla && myUnitMax - myUnitIndex > 1 && enemyUnits[enemyUnitIndex].health <= 0)
                            {
                                UnitDataBase thisUnit = myUnits[myUnitIndex];
                                myUnits[myUnitIndex] = myUnits[myUnitIndex + 1];
                                myUnits[myUnitIndex + 1] = thisUnit;
                            }
                            //���� ����
                            if (myUnits[myUnitIndex].drain && enemyUnits[enemyUnitIndex].health <= 0 && myUnits[myUnitIndex].health > 0)
                            {
                                myUnits[myUnitIndex].health = drainHealValue + myUnits[myUnitIndex].health > myUnitMaxHp ? myUnitMaxHp : drainHealValue + myUnits[myUnitIndex].health;
                            }
                            //��� ����
                            if (enemyUnits[enemyUnitIndex].guerrilla && enemyUnitMax - enemyUnitIndex > 1 && myUnits[myUnitIndex].health <= 0)
                            {
                                UnitDataBase thisUnit = enemyUnits[enemyUnitIndex];
                                enemyUnits[myUnitIndex] = enemyUnits[enemyUnitIndex + 1];
                                enemyUnits[enemyUnitIndex + 1] = thisUnit;
                            }
                            //��� ����
                            if (enemyUnits[enemyUnitIndex].drain && myUnits[myUnitIndex].health <= 0 && enemyUnits[enemyUnitIndex].health > 0)
                            {
                                enemyUnits[enemyUnitIndex].health = drainHealValue + enemyUnits[enemyUnitIndex].health > enemyUnitMaxHp ? enemyUnitMaxHp : drainHealValue + enemyUnits[enemyUnitIndex].health;
                            }
                            break;
                        }

                        break;
                }
            }
            else                    //ù ������ �ƴ� ��
            {
                switch (fightPhase) //����
                {
                    case 0:         //�غ�
                        continue;
                    case 1:         //�浹
                        //�� ���� �浹
                        if (!CalculateAccuracy(enemyUnits[enemyUnitIndex], myUnits[myUnitIndex].perfectAccuracy))    //��� ȸ�� ���� ��
                        {
                            
                            //�б�
                            if (isMyBluntWeapon)
                            {
                                myAddDamage += bluntWeaponValue;
                            }
                            //����
                            if (myUnits[myUnitIndex].slaughter && enemyUnits[enemyUnitIndex].lightArmor)
                            {
                                myAddDamage += slaughterValue;
                            }
                            //��⺴
                            if (enemyUnits[enemyUnitIndex].branchIdx == 5)
                            {
                                myAddDamage += myUnits[myUnitIndex].antiCavalry;
                            }
                            //����
                            if (myUnits[myUnitIndex].pierce)
                            {
                                myDamage = myUnits[myUnitIndex].attackDamage * myMultiDamage + myAddDamage - myReduceDamage;
                            }
                            else
                            {
                                myDamage = myUnits[myUnitIndex].attackDamage * (1 - (enemyUnits[enemyUnitIndex].armor) / (10 + enemyUnits[enemyUnitIndex].armor)) * myMultiDamage + myAddDamage - myReduceDamage;
                            }
                            if (myDamage > 0)
                            {
                                enemyUnits[enemyUnitIndex].health -= myDamage;
                            }
                        }

                        //��� ���� �浹
                        if (!CalculateAccuracy(myUnits[myUnitIndex], enemyUnits[enemyUnitIndex].perfectAccuracy))        //���� ȸ�� ���� ��
                        {
                            
                            //�б�
                            if (isEnemyBluntWeapon)
                            {
                                enemyAddDamage += bluntWeaponValue;
                            }

                            //����
                            if (enemyUnits[enemyUnitIndex].slaughter && myUnits[myUnitIndex].lightArmor)
                            {
                                enemyAddDamage += slaughterValue;
                            }

                            //��⺴
                            if (myUnits[myUnitIndex].branchIdx == 5)
                            {
                                enemyAddDamage += enemyUnits[enemyUnitIndex].antiCavalry;
                            }

                            //����
                            if (enemyUnits[enemyUnitIndex].pierce)
                            {
                                enemyDamage = enemyUnits[enemyUnitIndex].attackDamage * enemyMultiDamage + enemyAddDamage - enemyReduceDamage;
                            }
                            else
                            {
                                enemyDamage = enemyUnits[enemyUnitIndex].attackDamage * (1 - (myUnits[myUnitIndex].armor) / (10 + myUnits[myUnitIndex].armor)) * enemyMultiDamage + enemyAddDamage - enemyReduceDamage;
                            }
                            if (enemyDamage > 0)
                            {
                                myUnits[myUnitIndex].health -= enemyDamage;
                            }
                        }


                        UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitIndex].health);
                        await WaitForSecondsAsync(0.5f);

                        //���� ����� �ϴ� ���ο� ������
                        if (enemyUnits[enemyUnitIndex].health <= 0 || myUnits[myUnitIndex].health <= 0)
                        {
                            //���� ����
                            if (myUnits[myUnitIndex].guerrilla && myUnitMax - myUnitIndex > 1 && enemyUnits[enemyUnitIndex].health <= 0)
                            {
                                UnitDataBase thisUnit = myUnits[myUnitIndex];
                                myUnits[myUnitIndex] = myUnits[myUnitIndex + 1];
                                myUnits[myUnitIndex + 1] = thisUnit;
                            }
                            //���� ����
                            if (myUnits[myUnitIndex].drain && enemyUnits[enemyUnitIndex].health <= 0 && myUnits[myUnitIndex].health > 0)
                            {
                                myUnits[myUnitIndex].health = drainHealValue + myUnits[myUnitIndex].health > myUnitMaxHp ? myUnitMaxHp : drainHealValue + myUnits[myUnitIndex].health;
                            }
                            //��� ����
                            if (enemyUnits[enemyUnitIndex].guerrilla && enemyUnitMax - enemyUnitIndex > 1 && myUnits[myUnitIndex].health <= 0)
                            {
                                UnitDataBase thisUnit = enemyUnits[enemyUnitIndex];
                                enemyUnits[myUnitIndex] = enemyUnits[enemyUnitIndex + 1];
                                enemyUnits[enemyUnitIndex + 1] = thisUnit;
                            }
                            //��� ����
                            if (enemyUnits[enemyUnitIndex].drain && myUnits[myUnitIndex].health <= 0 && enemyUnits[enemyUnitIndex].health > 0)
                            {
                                enemyUnits[enemyUnitIndex].health = drainHealValue + enemyUnits[enemyUnitIndex].health > enemyUnitMaxHp ? enemyUnitMaxHp : drainHealValue + enemyUnits[enemyUnitIndex].health;
                            }
                            break;
                        }

                        continue;
                    case 2:         //����
                        //�� ���� ����
                        //���Ÿ� ����
                        for (int i = myRangeUnits[0]; i < myRangeUnits.Count; i++)
                        {
                            if (i > myUnitIndex && myUnits[i].range - (i - myUnitIndex) >= 1)
                            {
                                if (!CalculateAccuracy(enemyUnits[enemyUnitIndex], myUnits[myRangeUnits[i]].perfectAccuracy))    //��� ȸ�� ���� ��
                                {
                                    //�߰�
                                    if (isEnemyHeavyArmor)
                                    {
                                        if (myUnits[myRangeUnits[i]].attackDamage > heavyArmorValue)
                                        {
                                            enemyUnits[enemyUnitIndex].health -= myUnits[myRangeUnits[i]].attackDamage + heavyArmorValue;
                                        }
                                    }
                                    else
                                    {
                                        enemyUnits[enemyUnitIndex].health -= myUnits[myRangeUnits[i]].attackDamage;
                                    }
                                }
                            }

                        }
                        //��� ���� ����
                        //���Ÿ� ����
                        for (int i = enemyRangeUnits[0]; i < enemyRangeUnits.Count; i++)
                        {
                            if (i > enemyUnitIndex && enemyUnits[i].range - (i - enemyUnitIndex) >= 1)
                            {
                                if (!CalculateAccuracy(myUnits[myUnitIndex], enemyUnits[enemyRangeUnits[i]].perfectAccuracy))    //��� ȸ�� ���� ��
                                {
                                    //�߰�
                                    if (isMyHeavyArmor)
                                    {
                                        if (enemyUnits[enemyRangeUnits[i]].attackDamage > heavyArmorValue)
                                        {
                                            myUnits[myUnitIndex].health -= enemyUnits[enemyRangeUnits[i]].attackDamage + heavyArmorValue;
                                        }
                                    }
                                    else
                                    {
                                        myUnits[myUnitIndex].health -= enemyUnits[enemyRangeUnits[i]].attackDamage;
                                    }
                                }
                            }

                        }

                        UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitIndex].health);
                        await WaitForSecondsAsync(0.5f);

                        //���� ����� 
                        if (enemyUnits[enemyUnitIndex].health <= 0 || myUnits[myUnitIndex].health <= 0)
                        {
                            //���� ����
                            if (myUnits[myUnitIndex].guerrilla && myUnitMax - myUnitIndex > 1 && enemyUnits[enemyUnitIndex].health <= 0)
                            {
                                UnitDataBase thisUnit = myUnits[myUnitIndex];
                                myUnits[myUnitIndex] = myUnits[myUnitIndex + 1];
                                myUnits[myUnitIndex + 1] = thisUnit;
                            }
                            //���� ����
                            if (myUnits[myUnitIndex].drain && enemyUnits[enemyUnitIndex].health <= 0 && myUnits[myUnitIndex].health > 0)
                            {
                                myUnits[myUnitIndex].health = drainHealValue + myUnits[myUnitIndex].health > myUnitMaxHp ? myUnitMaxHp : drainHealValue + myUnits[myUnitIndex].health;
                            }
                            //��� ����
                            if (enemyUnits[enemyUnitIndex].guerrilla && enemyUnitMax - enemyUnitIndex > 1 && myUnits[myUnitIndex].health <= 0)
                            {
                                UnitDataBase thisUnit = enemyUnits[enemyUnitIndex];
                                enemyUnits[myUnitIndex] = enemyUnits[enemyUnitIndex + 1];
                                enemyUnits[enemyUnitIndex + 1] = thisUnit;
                            }
                            //��� ����
                            if (enemyUnits[enemyUnitIndex].drain && myUnits[myUnitIndex].health <= 0 && enemyUnits[enemyUnitIndex].health > 0)
                            {
                                enemyUnits[enemyUnitIndex].health = drainHealValue + enemyUnits[enemyUnitIndex].health > enemyUnitMaxHp ? enemyUnitMaxHp : drainHealValue + enemyUnits[enemyUnitIndex].health;
                            }
                            break;
                        }

                        break;
                }
            }
            
            isFirstAttack = false;

            // ������ ü���� 0 ������ ���, ���� �������� �Ѿ
            if (myUnits[myUnitIndex].health <= 0) //�� ���� ���
            {
                Debug.Log("�� ���� " + myUnits[myUnitIndex].unitName + "���");

                myUnitIndex++;  // ���� �� ����
                isFirstAttack = true;
                if(myUnitIndex!= myUnitMax)
                {
                    myUnitMaxHp = myUnits[myUnitIndex].health;
                    isMyHeavyArmor = CalculateHeavyArmor(myUnits[myUnitIndex].heavyArmor, enemyUnits[enemyUnitIndex].branchIdx);
                    isMyBluntWeapon = CalculateBluntWeapon(myUnits[myUnitIndex].bluntWeapon, enemyUnits[enemyUnitIndex].heavyArmor);


                }
                UpdateUnitCount(myUnitMax - myUnitIndex, enemyUnitMax - enemyUnitIndex);
                
            }
            if (enemyUnits[enemyUnitIndex].health <= 0) //�� ���� ���
            {
                Debug.Log("�� ���� " + enemyUnits[enemyUnitIndex].unitName + "���");

                enemyUnitIndex++;  // ���� �� ����
                isFirstAttack = true;

                if (enemyUnitIndex != enemyUnitMax)
                {
                    enemyUnitMaxHp = enemyUnits[enemyUnitIndex].health;
                    isMyHeavyArmor = CalculateHeavyArmor(enemyUnits[enemyUnitIndex].heavyArmor, myUnits[myUnitIndex].branchIdx);
                    isEnemyBluntWeapon = CalculateBluntWeapon( enemyUnits[enemyUnitIndex].bluntWeapon, myUnits[myUnitIndex].heavyArmor);

                }
                UpdateUnitCount(myUnitMax - myUnitIndex, enemyUnitMax - enemyUnitIndex);
                
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

            //�� ����
            fightPhase = 0;
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

    //���� �̸� ����
    private void UpdateUnitName(string myUnitName, string enemyUnitName)
    {
        autoBattleUI.UpdateName(myUnitName, enemyUnitName);
    }

    //ȸ���� ���
    private float CalculateDodge(UnitDataBase unit)
    {
        float dodge = 0.0f;
        dodge = (2 + (13 / 9) * (unit.mobility - 1)) +(unit.agility?10.0f:0);

        return dodge;
    }

    //ȸ�� ���
    private bool CalculateAccuracy(UnitDataBase unit,bool isPerfectAccuracy)
    {
        if (!isPerfectAccuracy)
        {
            return CalculateDodge(unit) >= Random.Range(1, 101);
        }
        return false;
    }

    //�߰� ���
    private bool CalculateHeavyArmor(bool offendingUnitHeavyArmor, int deffendingUnitBranchIdx)
    {
        if (offendingUnitHeavyArmor && deffendingUnitBranchIdx == 2)
        {
            return true;
        }
        return false;
    }

    //�б� ���
    private bool CalculateBluntWeapon(bool offendingUnitBluntWeapon, bool deffendingUnitHeavyArmor)
    {
        if (offendingUnitBluntWeapon && deffendingUnitHeavyArmor)
        {
            return true;
        }
        return false;
    }

    

    //���� ���
    private float CalculateCharge(bool isCharge,float mobility)
    {
        float result=1.0f;
        if (isCharge)
        {
            return result = 1.1f + (1.9f / (9 * (mobility - 1)));
        }

        return result;
    }

    //�ϻ� ���
    private int CalculateAssassination(List<float> unitHealth)
    {
        int minHealthNumber = 1;
        
        float lastHealth = unitHealth[0];
        foreach (var unit in unitHealth)
        {
            if (lastHealth>unit)
            {
                lastHealth = unit;
                minHealthNumber++;
            }
        }
        
        return minHealthNumber;
    }

    //���Ÿ� ���
    private bool RangeAttack(UnitDataBase offendingUnit,UnitDataBase deffendingUnit)
    {
        if (!CalculateAccuracy(deffendingUnit, offendingUnit.perfectAccuracy))   //��� ȸ�� ���� ��
        {
            return true;
        }
        return false;
    }
    
}
