using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Random = UnityEngine.Random;
using Unity.Mathematics;

public class AutoBattleManager : MonoBehaviour
{

    [SerializeField] private AutoBattleUI autoBattleUI;       //UI 관리 스크립트


    private readonly GoogleSheetLoader sheetLoader = new();
    
    private readonly float heavyArmorValue=15.0f;                //�߰� ���� ������ ���ҷ�
    private readonly float bluntWeaponValue = 15.0f;             //�б� ���� ������
    private readonly float throwSpearValue = 50.0f;              //��â ���� ������
    private readonly float overwhelmValue = 1.0f;                //���� �⵿�� ������
    private readonly float strongChargeValue = 0.5f;             //���� ���� ������ ���
    private readonly float defenseValue = 15.0f;                 //���� �¼� ���� ������ ���ҷ�
    private readonly float slaughterValue = 10.0f;               //���� ���� ������
    private readonly float assassinationValue = 2.0f;            //�ϻ� ������ ����
    private readonly float drainHealValue = 20.0f;               //���� ���� ȸ����
    private readonly float drainGainAttackValue = 10.0f;         //���� ���� ���ݷ� ������

    //���� ����

    private async Task<(UnitDataBase[], UnitDataBase[])> GetUnits(int[] myUnitIds, int[] enemyUnitIds)
    {
        List<UnitDataBase> myUnits = new List<UnitDataBase>();
        List<UnitDataBase> enemyUnits = new List<UnitDataBase>();

        // 구글 시트 데이터를 로드
        await sheetLoader.LoadGoogleSheetData();

        // 내 유닛 ID들을 기반으로 유닛을 가져와서 MyUnits에 저장
        foreach (int unitId in myUnitIds)
        {
            List<string> rowData = sheetLoader.GetRowData(unitId); // rowData는 List<string> 형식

            if (rowData != null)
            {
                // rowData를 UnitDataBase로 변환
                UnitDataBase unit = UnitDataBase.ConvertToUnitDataBase(rowData);

                if (unit != null)
                {
                    myUnits.Add(unit);  // List에 유닛 추가
                }
            }
        }

        // 적의 유닛 ID들을 기반으로 유닛을 가져와서 enemyUnits에 저장
        foreach (int unitId in enemyUnitIds)
        {
            List<string> rowData = sheetLoader.GetRowData(unitId); // rowData는 List<string> 형식

            if (rowData != null)
            {
                // rowData를 UnitDataBase로 변환
                UnitDataBase unit = UnitDataBase.ConvertToUnitDataBase(rowData);
                if (unit != null)
                {
                    enemyUnits.Add(unit);  // List에 유닛 추가
                }
            }
        }

        // List를 배열로 변환한 후 튜플로 반환
        return (myUnits.ToArray(), enemyUnits.ToArray());
    }


    //자동전투
    private async Task<int> AutoBattle(int[] _myUnitIds, int[] _enemyUnitIds)
    {
        // ���� ���� ������ ȣ��
        (UnitDataBase[] myUnits, UnitDataBase[] enemyUnits) = await GetUnits(_myUnitIds, _enemyUnitIds);

        // �ε����� ����ؼ� ���� ������ �����ϴ� ���� ����
        int myUnitIndex = 0;
        int enemyUnitIndex = 0;

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

        //���� �̸� UI�Լ� ���� �̹����� ����
        UpdateUnitName(myUnits[0].unitBranch, enemyUnits[0].unitBranch);

        //���� Hp UI �ʱ�ȭ �Լ� ȣ��

        UpdateUnitHp(myUnits[0].health, enemyUnits[0].health);
        await WaitForSecondsAsync();

        // 전투 반복
        while (myUnitIndex < myUnits.Length && enemyUnitIndex < enemyUnits.Length)
        {
            
            UpdateUnitName(myUnits[myUnitIndex].unitBranch, enemyUnits[enemyUnitIndex].unitBranch);

            //����
            //�غ�
            (myUnits, enemyUnits) =  PreparationPhase(
                        myUnits, enemyUnits, isFirstAttack, myUnitIndex, enemyUnitIndex, myUnitMax, enemyUnitMax, myUnitMaxHp, enemyUnitMaxHp);
                    
            UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitIndex].health);
            await WaitForSecondsAsync();
            
            //�浹
            (myUnits, enemyUnits) =  CombatPhase(
                            myUnits, enemyUnits, isFirstAttack, myUnitIndex, enemyUnitIndex, isMyBluntWeapon, isEnemyBluntWeapon, myUnitMax, enemyUnitMax, myUnitMaxHp, enemyUnitMaxHp);
                    
            UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitIndex].health);
            await WaitForSecondsAsync();


            //����
            if (!(myRangeUnits.Count == 0 && enemyRangeUnits.Count == 0))
            {
                (myUnits, enemyUnits) = SupportPhase(
                    myUnits, enemyUnits, myRangeUnits, enemyRangeUnits, enemyUnitIndex, enemyUnitIndex, isMyHeavyArmor, isEnemyHeavyArmor, myUnitMax, enemyUnitMax, myUnitMaxHp, enemyUnitMaxHp);

            }


            UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitIndex].health);
            await WaitForSecondsAsync();

            //�� ����
            isFirstAttack = false;


            // ������ ü���� 0 ������ ���, ���� �������� �Ѿ
            if (myUnits[myUnitIndex].health <1) //�� ���� ���
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
            if (enemyUnits[enemyUnitIndex].health <1) //�� ���� ���
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


            //index���� ������ ������ ��

            if(myUnitIndex == myUnitMax && enemyUnitIndex == enemyUnitMax)
            {
                UpdateUnitName(myUnits[myUnitMax - 1].unitBranch, enemyUnits[enemyUnitIndex-1].unitBranch);

                UpdateUnitHp(myUnits[myUnitMax - 1].health, enemyUnits[enemyUnitIndex-1].health);
                await WaitForSecondsAsync();
            }
            else if (myUnitIndex== myUnitMax)

            {
                UpdateUnitName(myUnits[myUnitMax - 1].unitBranch, enemyUnits[enemyUnitIndex].unitBranch);

                UpdateUnitHp(myUnits[myUnitMax - 1].health, enemyUnits[enemyUnitIndex].health);
                await WaitForSecondsAsync();
            }

            else if (enemyUnitIndex==enemyUnitMax)
            {
                UpdateUnitName(myUnits[myUnitIndex].unitBranch, enemyUnits[enemyUnitMax - 1].unitBranch);

                UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitMax-1].health);
                await WaitForSecondsAsync();
            }

        }

        // 전투 종료 후 승리 여부 판단
        if (myUnitIndex < myUnits.Length && enemyUnitIndex >= enemyUnits.Length)
        {
            Debug.Log($"나의 승리 {myUnits[myUnitIndex].unitName + myUnits[myUnitIndex].health}");
            return 0;  // 내가 승리
        }
        else if (enemyUnitIndex < enemyUnits.Length && myUnitIndex >= myUnits.Length)
        {
            Debug.Log($"나의 승리 {enemyUnits[enemyUnitIndex].unitName + enemyUnits[enemyUnitIndex].health}");
            return 1;  // 적이 승리
        }
        else
        {
            Debug.Log("무승부");
            return 2;  // 양쪽 모두 사망
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
    private void  UpdateUnitHp(float myUnitHp,float enemyHp)

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
    private async Task WaitForSecondsAsync(float seconds= 0.5f)

    {
        await Task.Delay((int)(seconds * 1000)); // 밀리초 단위
    }

    //���� �̸� ����
    private void UpdateUnitName(string myUnitName, string enemyUnitName)
    {
        autoBattleUI.UpdateName(myUnitName, enemyUnitName);
    }


    //ȸ���� ���
    private float CalculateDodge(UnitDataBase unit)
    {
        float dodge ;
        dodge = (2 + (13 / 9) * (unit.mobility - 1)) +(unit.agility?10.0f:0);

        return dodge;
    }

    //ȸ�� ���
    private bool CalculateAccuracy(UnitDataBase unit,bool isPerfectAccuracy)
    {
        bool result=false;
        if (!isPerfectAccuracy)
        {
            result = CalculateDodge(unit) >= Random.Range(1, 101);

            if (result) Debug.Log("ȸ��");

            return result;
        }
        return result;
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
        if (minHealthNumber >= unitHealth.Count)
        {
            minHealthNumber = unitHealth.Count - 1;
        }
        return minHealthNumber;
    }

    //������ui ȣ��
    private void CallDamageText(float damage,string text,bool team)
    {
        autoBattleUI.ShowDamage(MathF.Floor( damage), text, team);
    }

    //�غ� ������
    private (UnitDataBase[], UnitDataBase[]) PreparationPhase(UnitDataBase[] myUnits, UnitDataBase[] enemyUnits, bool isFirstAttack,int myUnitIndex,int enemyUnitIndex, int myUnitMax,int enemyUnitMax,float myUnitMaxHp, float enemyUnitMaxHp)
    {
        string mySkills ="";
        string enemySkills="" ;

        //ù ���� �϶�
        if (isFirstAttack)
        {
            //�ϻ� ��� hp ������ �⺻�� 1�� �Լ��� �Ҵ�����
            float myBackUnitHP ;
            float enemyBackUnitHP ;

            //�� ��� ����
            //����
            if (myUnits[myUnitIndex].overwhelm)
            {
                mySkills +="overwhelm ";

                enemyUnits[enemyUnitIndex].mobility = overwhelmValue;

                CallDamageText(0, mySkills, true);
            }
            //�ϻ�
            if (myUnits[myUnitIndex].assassination || enemyUnitMax - enemyUnitIndex > 1)//�ϻ� �ְ� ��� ���� 1���� �ʰ�
            {
                mySkills += "assassination ";

                List<float> enemyUnitHealth = new ();
                foreach (var unit in enemyUnits)
                {
                    enemyUnitHealth.Add(unit.health);   //��� ���� ü�� ������� �迭ȭ
                }
                //��ȣ
                if (enemyUnits[enemyUnitIndex].guard)
                {
                    enemySkills += "guard ";

                    if (!CalculateAccuracy(enemyUnits[enemyUnitIndex], myUnits[myUnitIndex].perfectAccuracy))    //��� ȸ�� ���� ��
                    {
                        CallDamageText(myUnits[myUnitIndex].attackDamage * (1 - (enemyUnits[enemyUnitIndex].armor) / (10 + enemyUnits[enemyUnitIndex].armor)) * assassinationValue, mySkills, true);

                        enemyUnits[enemyUnitIndex].health -= myUnits[myUnitIndex].attackDamage * (1 - (enemyUnits[enemyUnitIndex].armor) / (10 + enemyUnits[enemyUnitIndex].armor)) * assassinationValue;
                    }
                }
                else
                {
                    CallDamageText(myUnits[myUnitIndex].attackDamage * assassinationValue, mySkills, true);

                    enemyBackUnitHP = enemyUnits[CalculateAssassination(enemyUnitHealth)].health;
                    enemyBackUnitHP -= myUnits[myUnitIndex].attackDamage * assassinationValue;
                }


            }
            //��â
            if (myUnits[myUnitIndex].throwSpear)
            {
                if (!CalculateAccuracy(enemyUnits[enemyUnitIndex], myUnits[myUnitIndex].perfectAccuracy))    //��� ȸ�� ���� ��
                {
                    mySkills += "throwSpear ";

                    enemyUnits[enemyUnitIndex].health -= throwSpearValue;

                    CallDamageText(throwSpearValue, mySkills, true);
                }
            }

            //�� ���
            //����
            if (enemyUnits[enemyUnitIndex].overwhelm)
            {
                enemySkills += "overwhelm ";

                myUnits[myUnitIndex].mobility = overwhelmValue;

                CallDamageText(0, mySkills, false);
            }

            //�ϻ�
            if (enemyUnits[enemyUnitIndex].assassination || myUnitMax - myUnitIndex > 1)//�ϻ� �ְ� ��� ���� 1���� �ʰ�
            {
                enemySkills += "assassination ";

                List<float> myUnitHealth = new ();
                foreach (var unit in enemyUnits)
                {
                    myUnitHealth.Add(unit.health);   //��� ���� ü�� ������� �迭ȭ
                }
                //��ȣ
                if (myUnits[myUnitIndex].guard)
                {
                    mySkills += "guard ";

                    if (!CalculateAccuracy(myUnits[myUnitIndex], enemyUnits[enemyUnitIndex].perfectAccuracy))    //���� ȸ�� ���� ��
                    {
                        CallDamageText(enemyUnits[enemyUnitIndex].attackDamage * (1 - (myUnits[myUnitIndex].armor) / (10 + myUnits[myUnitIndex].armor)) * assassinationValue, mySkills, false);

                        myUnits[enemyUnitIndex].health -= enemyUnits[enemyUnitIndex].attackDamage * (1 - (myUnits[myUnitIndex].armor) / (10 + myUnits[myUnitIndex].armor)) * assassinationValue;
                    }
                }
                else
                {
                    CallDamageText(enemyUnits[myUnitIndex].attackDamage * assassinationValue, mySkills, false);

                    myBackUnitHP = enemyUnits[CalculateAssassination(myUnitHealth)].health;
                    myBackUnitHP -= enemyUnits[myUnitIndex].attackDamage * assassinationValue;
                }
            }

            //��â
            if (enemyUnits[enemyUnitIndex].throwSpear)
            {
                enemySkills += "throwSpear ";

                if (!CalculateAccuracy(myUnits[myUnitIndex], enemyUnits[enemyUnitIndex].perfectAccuracy))    //���� ȸ�� ���� ��
                {
                    CallDamageText(throwSpearValue, mySkills, false);

                    myUnits[myUnitIndex].health -= throwSpearValue;
                }
            }

            return UnitDeathSkill(myUnits, enemyUnits, myUnitIndex, enemyUnitIndex, myUnitMax, enemyUnitMax, myUnitMaxHp, enemyUnitMaxHp);
        }
        return (myUnits, enemyUnits);
    }

    //�浹 ������
    private (UnitDataBase[], UnitDataBase[]) CombatPhase(UnitDataBase[] myUnits, UnitDataBase[] enemyUnits ,bool isFirstAttack, int myUnitIndex, int enemyUnitIndex,bool isMyBluntWeapon,bool isEnemyBluntWeapon,int myUnitMax,int enemyUnitMax,float myUnitMaxHp,float enemyUnitMaxHp)
    {
        float myMultiDamage = 1.0f;
        float enemyMultiDamage=1.0f;
        float myReduceDamage = 0f;
        float enemyReduceDamage = 0f;

        float myAddDamage = 0;
        float enemyAddDamage = 0;

        float myDamage;
        float enemyDamage;

        string mySkills ="";
        string enemySkills ="";

        if (isFirstAttack)
        {
            //�� Ư�� ���
            // ����
            myMultiDamage = CalculateCharge(myUnits[myUnitIndex].charge, myUnits[myUnitIndex].mobility);

            if (myMultiDamage > 1) mySkills+="charge ";

            //���� ����
            if (myUnits[myUnitIndex].strongCharge)
            {
                mySkills += "strongCharge ";

                myMultiDamage += strongChargeValue;
            }
            //���� �¼� ���� ������ ����
            if (myUnits[myUnitIndex].charge && enemyUnits[enemyUnitIndex].defense)
            {
                enemySkills += "defense ";

                myReduceDamage += defenseValue;
            }
            //���� �¼� ���� ������ ����
            if (enemyUnits[enemyUnitIndex].charge && myUnits[myUnitIndex].defense)
            {
                mySkills += "defense ";

                myAddDamage += defenseValue;
            }

            //��� Ư�� ���
            // ����
            enemyMultiDamage = CalculateCharge(enemyUnits[enemyUnitIndex].charge, enemyUnits[enemyUnitIndex].mobility);

            if (enemyMultiDamage > 1) enemySkills += "charge ";

            //���� ����
            if (enemyUnits[enemyUnitIndex].strongCharge)
            {
                enemySkills += "strongCharge ";

                enemyMultiDamage += strongChargeValue;
            }
            //���� �¼� ��� ������ ����
            if (enemyUnits[enemyUnitIndex].charge && myUnits[myUnitIndex].defense)
            {
                mySkills += "defense ";

                enemyReduceDamage += defenseValue;
            }
            //���� �¼� ��� ������ ����
            if (myUnits[myUnitIndex].charge && enemyUnits[enemyUnitIndex].defense)
            {
                enemySkills += "defense ";

                myAddDamage += defenseValue;
            }
        }

        //�� ���� �浹 ������
        if (!CalculateAccuracy(enemyUnits[enemyUnitIndex], myUnits[myUnitIndex].perfectAccuracy))    //��� ȸ�� ���� ��
        {

            //�б�
            if (isMyBluntWeapon)
            {
                mySkills += "bluntWeapon ";

                myAddDamage += bluntWeaponValue;
            }
            //����
            if (myUnits[myUnitIndex].slaughter && enemyUnits[enemyUnitIndex].lightArmor)
            {
                mySkills += "slaughter ";

                myAddDamage += slaughterValue;
            }
            //��⺴
            if (enemyUnits[enemyUnitIndex].branchIdx == 5)
            {
                mySkills += "antiCavalry ";

                myAddDamage += myUnits[myUnitIndex].antiCavalry;
            }
            //����
            if (myUnits[myUnitIndex].pierce)
            {
                mySkills += "pierce ";

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

            CallDamageText(myDamage, mySkills, true);
        }

            //��� ���� �浹 ������
            if (!CalculateAccuracy(myUnits[myUnitIndex], enemyUnits[enemyUnitIndex].perfectAccuracy))        //���� ȸ�� ���� ��
            {
                
                //�б�
                if (isEnemyBluntWeapon)
                {
                    enemySkills += "bluntWeapon ";

                    enemyAddDamage += bluntWeaponValue;
                }
                //����
                if (enemyUnits[enemyUnitIndex].slaughter && myUnits[myUnitIndex].lightArmor)
                {
                    enemySkills += "slaughter ";

                    enemyAddDamage += slaughterValue;
                }
                //��⺴
                if (myUnits[myUnitIndex].branchIdx == 5)
                {
                    enemySkills += "antiCavalry ";

                    enemyAddDamage += enemyUnits[enemyUnitIndex].antiCavalry;
                }
                //����
                if (enemyUnits[enemyUnitIndex].pierce)
                {
                    enemySkills += "pierce ";

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

            CallDamageText(enemyDamage, mySkills, false);
        }
        

         return UnitDeathSkill(myUnits, enemyUnits, myUnitIndex, enemyUnitIndex, myUnitMax, enemyUnitMax, myUnitMaxHp, enemyUnitMaxHp);
    }

    //���� ������
    private (UnitDataBase[], UnitDataBase[]) SupportPhase(UnitDataBase[] myUnits, UnitDataBase[] enemyUnits, List<int> myRangeUnits, List<int> enemyRangeUnits, int myUnitIndex,int enemyUnitIndex,bool isEnemyHeavyArmor, bool isMyHeavyArmor, int myUnitMax, int enemyUnitMax, float myUnitMaxHp, float enemyUnitMaxHp)
    {
        //�� ���� ����
        //���Ÿ� ����
        if (myRangeUnits.Count>0)
        {
            for (int i = 0; i < myRangeUnits.Count; i++)
            {
                if (myRangeUnits[i] > myUnitIndex && myUnits[myRangeUnits[i]].range - (myRangeUnits[i] - myUnitIndex) >= 1)
                {
                    if (!CalculateAccuracy(enemyUnits[enemyUnitIndex], myUnits[myRangeUnits[i]].perfectAccuracy))    //��� ȸ�� ���� ��
                    {
                        //�߰�
                        if (isEnemyHeavyArmor)
                        {
                            if (myUnits[myRangeUnits[i]].attackDamage > heavyArmorValue)
                            {
                                Debug.Log("��� �߰� �ߵ�");
                                Debug.Log("�� ������" + (myUnits[myRangeUnits[i]].attackDamage- heavyArmorValue));
                                CallDamageText(myUnits[myRangeUnits[i]].attackDamage + heavyArmorValue, "rangeAttack heavyArmor", true);

                                enemyUnits[enemyUnitIndex].health -= myUnits[myRangeUnits[i]].attackDamage + heavyArmorValue;
                            }
                        }
                        else
                        {
                            Debug.Log("�� ������" + myUnits[myRangeUnits[i]].attackDamage);
                            CallDamageText(myUnits[myRangeUnits[i]].attackDamage , "rangeAttack", true);

                            enemyUnits[enemyUnitIndex].health -= myUnits[myRangeUnits[i]].attackDamage;
                        }
                    }
                }

            }
        }
        //��� ���� ����
        //���Ÿ� ����
        if (enemyRangeUnits.Count>0)
        {
            for (int i = 0; i < enemyRangeUnits.Count; i++)
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
                                Debug.Log("�� �߰� �ߵ�");
                                Debug.Log("��� ������" + (enemyUnits[enemyRangeUnits[i]].attackDamage + heavyArmorValue));
                                CallDamageText(enemyUnits[enemyRangeUnits[i]].attackDamage + heavyArmorValue, "rangeAttack heavyArmor", false);

                                myUnits[myUnitIndex].health -= enemyUnits[enemyRangeUnits[i]].attackDamage + heavyArmorValue;
                            }
                        }
                        else
                        {
                            Debug.Log("��� ������" +enemyUnits[enemyRangeUnits[i]].attackDamage );
                            CallDamageText(enemyUnits[enemyRangeUnits[i]].attackDamage , "rangeAttack", false);

                            myUnits[myUnitIndex].health -= enemyUnits[enemyRangeUnits[i]].attackDamage;
                        }
                    }
                }

            }
        }

        return UnitDeathSkill(myUnits, enemyUnits, myUnitIndex, enemyUnitIndex, myUnitMax, enemyUnitMax, myUnitMaxHp, enemyUnitMaxHp);

    }

    //���� ��� �� �ߵ� ��ų ó�� �Լ�
    private (UnitDataBase[], UnitDataBase[]) UnitDeathSkill(UnitDataBase[] myUnits, UnitDataBase[] enemyUnits, int myUnitIndex,int enemyUnitIndex, int myUnitMax,int enemyUnitMax, float myUnitMaxHp,float enemyUnitMaxHp)
    {
        //���� ����� 
        if (enemyUnits[enemyUnitIndex].health <= 0 || myUnits[myUnitIndex].health <= 0)
        {
            //���� ����
            if (myUnits[myUnitIndex].guerrilla && myUnitMax - myUnitIndex > 1 && enemyUnits[enemyUnitIndex].health <= 0)
            {
                CallDamageText(0, "guerrilla", true);

                UnitDataBase thisUnit = myUnits[myUnitIndex];
                myUnits[myUnitIndex] = myUnits[myUnitIndex + 1];
                myUnits[myUnitIndex + 1] = thisUnit;
            }
            //���� ����
            if (myUnits[myUnitIndex].drain && enemyUnits[enemyUnitIndex].health <= 0 && myUnits[myUnitIndex].health > 0)
            {
                CallDamageText(0, "drain", true);

                myUnits[myUnitIndex].health = drainHealValue + myUnits[myUnitIndex].health > myUnitMaxHp ? myUnitMaxHp : drainHealValue + myUnits[myUnitIndex].health;
                myUnits[myUnitIndex].attackDamage += drainGainAttackValue;
            }
            //��� ����
            if (enemyUnits[enemyUnitIndex].guerrilla && enemyUnitMax - enemyUnitIndex > 1 && myUnits[myUnitIndex].health <= 0)
            {
                CallDamageText(0, "guerrilla", false);

                UnitDataBase thisUnit = enemyUnits[enemyUnitIndex];
                enemyUnits[myUnitIndex] = enemyUnits[enemyUnitIndex + 1];
                enemyUnits[enemyUnitIndex + 1] = thisUnit;
            }
            //��� ����
            if (enemyUnits[enemyUnitIndex].drain && myUnits[myUnitIndex].health <= 0 && enemyUnits[enemyUnitIndex].health > 0)
            {
                CallDamageText(0, "drain", false);

                enemyUnits[enemyUnitIndex].health = drainHealValue + enemyUnits[enemyUnitIndex].health > enemyUnitMaxHp ? enemyUnitMaxHp : drainHealValue + enemyUnits[enemyUnitIndex].health;
                enemyUnits[enemyUnitIndex].attackDamage += drainGainAttackValue;
            }
        }

        return (myUnits, enemyUnits);
    }

}

