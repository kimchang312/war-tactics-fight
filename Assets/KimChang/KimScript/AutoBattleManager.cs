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

    private bool isGarria =false;                               //전투중 유격 발동 여부

    private List<int> _enemyIds = new List<int> { 0 };    //상대 유닛 id 추후 삭제

    public bool isPause=false;                                  //게임 멈춤 인지

    //추후 삭제
    private void TrueGarria()
    {
        isGarria = true;
    }
    //추후 삭제
    private void FalseGarria() 
    { 
        isGarria = false; 
    }

    //이 씬이 로드되었을 때== 구매 배치로 전투 씬 입장했을때
    private async void Start()
    {

        List<int> myIds = PlayerData.Instance.ShowPlacedUnitList();
        List<int> enemyIds = PlayerData.Instance.GetEnemyUnitIndexes();
        if (myIds.Count <= 0) return;
        
        await StartBattle(myIds, enemyIds);
    }

    //���� ����
    private async Task<(UnitDataBase[], UnitDataBase[])> GetUnits(List<int> myUnitIds, List<int> enemyUnitIds)
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
    private async Task<int> AutoBattle(List<int> _myUnitIds, List<int> _enemyUnitIds)
    {
        if (autoBattleUI == null)
        {
            autoBattleUI = FindObjectOfType<AutoBattleUI>();
        }

        // ���� ���� ������ ȣ��
        (UnitDataBase[] myUnits, UnitDataBase[] enemyUnits) = await GetUnits(_myUnitIds, _enemyUnitIds);

        // �ε����� ����ؼ� ���� ������ �����ϴ� ���� ����
        int myUnitIndex = 0;
        int enemyUnitIndex = 0;

        //������ ���� ����
        int myUnitMax = myUnits.Length;
        int enemyUnitMax = enemyUnits.Length;

        //���� ���Ÿ� ���� ��ġ
        List<int> myRangeUnits= new ();
        //��� ���Ÿ� ���� ��ġ
        List<int> enemyRangeUnits= new ();
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

        // 전투 반복
        while (myUnitIndex < myUnits.Length && enemyUnitIndex < enemyUnits.Length)
        {
            Debug.Log(isPause);
            if (isPause)
            {
               
            }

            // 암살로 인한 시체 유닛 발생 시
            if (myUnits[myUnitIndex].health <= 0)
            {
                myUnitIndex++;

                isMyHeavyArmor = CalculateHeavyArmor(myUnits[myUnitIndex].heavyArmor, enemyUnits[enemyUnitIndex].branchIdx);
                isMyBluntWeapon = CalculateBluntWeapon(myUnits[myUnitIndex].bluntWeapon, enemyUnits[enemyUnitIndex].heavyArmor);

                myUnitMaxHp = myUnits[myUnitIndex].health;

                continue;
            }
            if (enemyUnits[enemyUnitIndex].health <= 0)
            {
                enemyUnitIndex++;

                isEnemyHeavyArmor = CalculateHeavyArmor(enemyUnits[myUnitIndex].heavyArmor, myUnits[myUnitIndex].branchIdx);
                isEnemyBluntWeapon = CalculateBluntWeapon(enemyUnits[enemyUnitIndex].bluntWeapon, myUnits[myUnitIndex].heavyArmor);

                enemyUnitMaxHp = enemyUnits[enemyUnitIndex].health;

                continue;
            }

            UpdateUnitUI(myUnits, enemyUnits, myUnitIndex, enemyUnitIndex, myUnitMax, enemyUnitMax, myUnitMaxHp, enemyUnitMaxHp);
            await WaitForSecondsAsync();
            
            // 준비
            (myUnits, enemyUnits) = await PreparationPhase(
                myUnits, enemyUnits, isFirstAttack, myUnitIndex, enemyUnitIndex, myUnitMax, enemyUnitMax, myUnitMaxHp, enemyUnitMaxHp);
            UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitIndex].health, myUnitMaxHp, enemyUnitMaxHp);
            await WaitForSecondsAsync();

            // 사망 처리: 내 유닛
            if (HandleUnitDeath(myUnits, ref myUnitIndex, myUnitMax, enemyUnits, enemyUnitIndex, ref isFirstAttack, ref isMyHeavyArmor, ref isMyBluntWeapon,
                UpdateUnitCount, UpdateUnitHp, CallCreateUnit,ref myUnitMaxHp))
                continue; // 사망 시 준비부터 다시 시작

            // 사망 처리: 적 유닛
            if (HandleUnitDeath(enemyUnits, ref enemyUnitIndex, enemyUnitMax, myUnits, myUnitIndex, ref isFirstAttack, ref isEnemyHeavyArmor, ref isEnemyBluntWeapon,
                UpdateUnitCount, UpdateUnitHp, CallCreateUnit, ref enemyUnitMaxHp))
                continue; // 사망 시 준비부터 다시 시작

            // 충돌
            (myUnits, enemyUnits) = await CombatPhase(
                myUnits, enemyUnits, isFirstAttack, myUnitIndex, enemyUnitIndex, isMyBluntWeapon, isEnemyBluntWeapon, myUnitMax, enemyUnitMax, myUnitMaxHp, enemyUnitMaxHp);
            UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitIndex].health, myUnitMaxHp, enemyUnitMaxHp);
            await WaitForSecondsAsync();

            // 사망 처리: 내 유닛
            if (HandleUnitDeath(myUnits, ref myUnitIndex, myUnitMax, enemyUnits, enemyUnitIndex, ref isFirstAttack, ref isMyHeavyArmor, ref isMyBluntWeapon,
                UpdateUnitCount, UpdateUnitHp, CallCreateUnit, ref myUnitMaxHp))
                continue; // 사망 시 준비부터 다시 시작

            // 사망 처리: 적 유닛
            if (HandleUnitDeath(enemyUnits, ref enemyUnitIndex, enemyUnitMax, myUnits, myUnitIndex, ref isFirstAttack, ref isEnemyHeavyArmor, ref isEnemyBluntWeapon,
                UpdateUnitCount, UpdateUnitHp, CallCreateUnit, ref enemyUnitMaxHp))
                continue; // 사망 시 준비부터 다시 시작

            // 지원
            if (!(myRangeUnits.Count == 0 && enemyRangeUnits.Count == 0))
            {
                (myUnits, enemyUnits) = await SupportPhase(
                    myUnits, enemyUnits, myRangeUnits, enemyRangeUnits, myUnitIndex, enemyUnitIndex, isMyHeavyArmor, isEnemyHeavyArmor, myUnitMax, enemyUnitMax, myUnitMaxHp, enemyUnitMaxHp);
            
            UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitIndex].health, myUnitMaxHp, enemyUnitMaxHp);
            await WaitForSecondsAsync();

            // 사망 처리: 내 유닛
            if (HandleUnitDeath(myUnits, ref myUnitIndex, myUnitMax, enemyUnits, enemyUnitIndex, ref isFirstAttack, ref isMyHeavyArmor, ref isMyBluntWeapon,
                UpdateUnitCount, UpdateUnitHp, CallCreateUnit, ref myUnitMaxHp))
                continue; // 사망 시 준비부터 다시 시작

            // 사망 처리: 적 유닛
            if (HandleUnitDeath(enemyUnits, ref enemyUnitIndex, enemyUnitMax, myUnits, myUnitIndex, ref isFirstAttack, ref isEnemyHeavyArmor, ref isEnemyBluntWeapon,
                UpdateUnitCount, UpdateUnitHp, CallCreateUnit, ref enemyUnitMaxHp))
                continue; // 사망 시 준비부터 다시 시작
            }

            isFirstAttack = false;
        }


        // 전투 종료 후 승리 여부 판단
        if (myUnitIndex < myUnits.Length && enemyUnitIndex >= enemyUnits.Length)
        {
            Debug.Log($"나의 승리 {myUnits[myUnitIndex].unitName + myUnits[myUnitIndex].health}");
            return 0;  // 내가 승리
        }
        else if (enemyUnitIndex < enemyUnits.Length && myUnitIndex >= myUnits.Length)
        {
            Debug.Log($"나의 패배 {enemyUnits[enemyUnitIndex].unitName + enemyUnits[enemyUnitIndex].health}");
            return 1;  // 적이 승리
        }
        else
        {
            Debug.Log("무승부");
            return 2;  // 양쪽 모두 사망
        }
        
    }

    //유닛 데이터 받고 전투 시작
    public async Task<int> StartBattle(List<int> _myUnitIds, List<int> _enemyUnitIds)
    {
        int result = await AutoBattle(_myUnitIds,_enemyUnitIds);
        autoBattleUI.FightEnd(result);
        return result;
    }

    //유닛 갯수 최신화
    private void UpdateUnitCount(int myUnitLength,int enemyUnitLength)
    {
          autoBattleUI.UpdateUnitCountUI(myUnitLength, enemyUnitLength);
    }


    //유닛 체력 최신화
    private void  UpdateUnitHp(float myUnitHp,float enemyHp,float myMAxHp,float enemyMaxHp)
    {
        if(myUnitHp < 0)
        {
            myUnitHp = 0;
        }
        if (enemyHp < 0)
        {
            enemyHp = 0;
        }
        autoBattleUI.UpateUnitHPUI(MathF.Floor(myUnitHp),MathF.Floor(enemyHp), MathF.Floor(myMAxHp), MathF.Floor(enemyMaxHp));

    }


    //대기 함수
    private async Task WaitForSecondsAsync(float seconds= 0.5f)

    {
        await Task.Delay((int)(seconds * 1000)); // 밀리초 단위
    }

    //유닛 이름 곧 삭제
    private void UpdateUnitName(string myUnitName, string enemyUnitName)
    {
        
    }

    //회피율 계산
    private float CalculateDodge(UnitDataBase unit)
    {
        float dodge ;
        dodge = (2 + (13 / 9) * (unit.mobility - 1)) +(unit.agility?10.0f:0);

        return dodge;
    }

    //회피 유무 계산
    private bool CalculateAccuracy(UnitDataBase unit,bool isPerfectAccuracy)
    {
        bool result=false;
        if (!isPerfectAccuracy)
        {
            result = CalculateDodge(unit) >= Random.Range(1, 101);

            if (result)
            {
                Debug.Log("회피");
            }
        }
        return result;
    }

    //중갑 계산
    private bool CalculateHeavyArmor(bool offendingUnitHeavyArmor, int deffendingUnitBranchIdx)
    {
        if (offendingUnitHeavyArmor && deffendingUnitBranchIdx == 2)
        {
            return true;
        }
        return false;
    }

    //둔기 계산
    private bool CalculateBluntWeapon(bool offendingUnitBluntWeapon, bool deffendingUnitHeavyArmor)
    {
        if (offendingUnitBluntWeapon && deffendingUnitHeavyArmor)
        {
            return true;
        }
        return false;
    }

    //돌진 계산
    private float CalculateCharge(bool isCharge,float mobility)
    {
        if (isCharge)
        {
            return 1.1f + (1.9f / (9 * (mobility - 1)));
        }

        return 1;
    }

    //암살 계산
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

    //데미지 ui 호출
    private void CallDamageText(float damage,string text,bool team)
    {
        autoBattleUI.ShowDamage(MathF.Floor( damage), text, team);
    }

    // 유닛 생성UI 호출
    private void CallCreateUnit(UnitDataBase[] myUnits, UnitDataBase[] enemyUnits, int myUnitIndex, int enemyUnitIndex)
    {
        List<UnitDataBase> myRangeUnits=new();
        List<UnitDataBase> enemyRangUnits=new();
        for (int i = myUnitIndex + 1; i < myUnits.Length; i++)
        {
            if (myUnits[i].rangedAttack && (myUnits[i].range - (i - myUnitIndex) > 0))
            {
                myRangeUnits.Add(myUnits[i]);
            }
        }
        for (int i = enemyUnitIndex + 1; i < enemyUnits.Length; i++)
        {
            if (enemyUnits[i].rangedAttack && (enemyUnits[i].range - (i - enemyUnitIndex) > 0))
            {
                enemyRangUnits.Add(enemyUnits[i]);
            }
        }
        autoBattleUI.CreateUnitBox(myUnits, enemyUnits, myUnitIndex, enemyUnitIndex, CalculateDodge(myUnits[myUnitIndex]), CalculateDodge(enemyUnits[enemyUnitIndex]),myRangeUnits.Count,enemyRangUnits.Count);
    }

    //준비 페이즈
    private async Task<(UnitDataBase[], UnitDataBase[])> PreparationPhase(UnitDataBase[] myUnits, UnitDataBase[] enemyUnits, bool isFirstAttack,int myUnitIndex,int enemyUnitIndex, int myUnitMax,int enemyUnitMax,float myUnitMaxHp, float enemyUnitMaxHp)
    {
        string mySkills ="";
        string enemySkills="" ;

        //첫공격
        if (isFirstAttack)
        {
            //후열 유닛 체력
            float myBackUnitHP=1 ;
            float enemyBackUnitHP=1 ;

            //나의 준비
            //위협
            if (myUnits[myUnitIndex].overwhelm)
            {
                mySkills +="위협 ";

                enemyUnits[enemyUnitIndex].mobility = overwhelmValue;

                CallDamageText(0, mySkills, false);
            }
            //암살
            if (myUnits[myUnitIndex].assassination && enemyUnitMax - enemyUnitIndex > 1)//상대 후방유닛 있고 내가 암살
            {
                mySkills += "암살 ";

                Debug.Log("암살");

                List<float> enemyUnitHealth = new ();
                foreach (var unit in enemyUnits)
                {
                    enemyUnitHealth.Add(unit.health);   //
                }
                //수호
                if (enemyUnits[enemyUnitIndex].guard)
                {
                    enemySkills += "수호 ";

                    CallDamageText(myUnits[myUnitIndex].attackDamage * (1 - (enemyUnits[enemyUnitIndex].armor) / (10 + enemyUnits[enemyUnitIndex].armor)) * assassinationValue, mySkills, true);

                    enemyUnits[enemyUnitIndex].health -= myUnits[myUnitIndex].attackDamage * (1 - (enemyUnits[enemyUnitIndex].armor) / (10 + enemyUnits[enemyUnitIndex].armor)) * assassinationValue;
                    
                }
                else
                {
                    CallDamageText(myUnits[myUnitIndex].attackDamage * assassinationValue, mySkills, true);

                    enemyBackUnitHP = enemyUnits[CalculateAssassination(enemyUnitHealth)].health;
                    enemyBackUnitHP -= myUnits[myUnitIndex].attackDamage * assassinationValue;
                    enemyUnits[CalculateAssassination(enemyUnitHealth)].health=enemyBackUnitHP;
                }
            }
            //투창
            if (myUnits[myUnitIndex].throwSpear)
            {
                if (!CalculateAccuracy(enemyUnits[enemyUnitIndex], myUnits[myUnitIndex].perfectAccuracy))    //상대 회피 실패
                {
                    mySkills += "투창 ";

                    enemyUnits[enemyUnitIndex].health -= throwSpearValue;

                    CallDamageText(throwSpearValue, mySkills, true);
                }
                else
                {
                    CallDamageText(0, "투창 회피", false);
                }
            }

            //상대 준비
            //위협
            if (enemyUnits[enemyUnitIndex].overwhelm)
            {
                enemySkills += "위협 ";

                myUnits[myUnitIndex].mobility = overwhelmValue;

                CallDamageText(0, enemySkills, true);
            }

            //암살
            if (enemyUnits[enemyUnitIndex].assassination && myUnitMax - myUnitIndex > 1)//내 후방 유닛 있고 상대 암살
            {
                enemySkills += "암살 ";

                Debug.Log("암살");

                List<float> myUnitHealth = new ();
                foreach (var unit in enemyUnits)
                {
                    myUnitHealth.Add(unit.health);   //암살 할 유닛 체력 확인
                }
                //수호
                if (myUnits[myUnitIndex].guard)
                {
                    mySkills += "수호 ";

                    CallDamageText(enemyUnits[enemyUnitIndex].attackDamage * (1 - (myUnits[myUnitIndex].armor) / (10 + myUnits[myUnitIndex].armor)) * assassinationValue, mySkills, false);

                    myUnits[enemyUnitIndex].health -= enemyUnits[enemyUnitIndex].attackDamage * (1 - (myUnits[myUnitIndex].armor) / (10 + myUnits[myUnitIndex].armor)) * assassinationValue;
                    
                }
                else
                {
                    CallDamageText(enemyUnits[myUnitIndex].attackDamage * assassinationValue, mySkills, false);

                    myBackUnitHP = myUnits[CalculateAssassination(myUnitHealth)].health;
                    myBackUnitHP -= myUnits[myUnitIndex].attackDamage * assassinationValue;
                    myUnits[CalculateAssassination(myUnitHealth)].health=myBackUnitHP;
                }
            }

            //투창
            if (enemyUnits[enemyUnitIndex].throwSpear)
            {
                enemySkills += "투창 ";

                if (!CalculateAccuracy(myUnits[myUnitIndex], enemyUnits[enemyUnitIndex].perfectAccuracy))    //나의 회피 실패
                {
                    CallDamageText(throwSpearValue, enemySkills, false);

                    myUnits[myUnitIndex].health -= throwSpearValue;
                }
                else
                {
                    CallDamageText(0, "투창 회피", true);
                }
            }

            return await UnitDeathSkill(myUnits, enemyUnits, myUnitIndex, enemyUnitIndex, myUnitMax, enemyUnitMax, myUnitMaxHp, enemyUnitMaxHp,myBackUnitHP,enemyBackUnitHP);
        }
        return (myUnits, enemyUnits);
    }

    //전투 페이즈
    private async Task<(UnitDataBase[], UnitDataBase[])> CombatPhase(UnitDataBase[] myUnits, UnitDataBase[] enemyUnits ,bool isFirstAttack, int myUnitIndex, int enemyUnitIndex,bool isMyBluntWeapon,bool isEnemyBluntWeapon,int myUnitMax,int enemyUnitMax,float myUnitMaxHp,float enemyUnitMaxHp)
    {
        float myMultiDamage = 1.0f;
        float enemyMultiDamage=1.0f;
        float myReduceDamage = 0f;
        float enemyReduceDamage = 0f;

        float myAddDamage = 0;
        float enemyAddDamage = 0;

        float myDamage;
        float enemyDamage;

        string mySkills ="전투 ";
        string enemySkills ="전투 ";

        //첫 공격
        if (isFirstAttack)
        {
            //나의 공격
            // 돌격
            myMultiDamage = CalculateCharge(myUnits[myUnitIndex].charge, myUnits[myUnitIndex].mobility);

            if (myMultiDamage > 1) mySkills+="돌격 ";


            //강한 돌격
            if (myUnits[myUnitIndex].strongCharge)
            {
                mySkills += "강한 돌격 ";

                myMultiDamage += strongChargeValue;
            }
            //수비 태세 상대 데미지 감소
            if (myUnits[myUnitIndex].charge && enemyUnits[enemyUnitIndex].defense)
            {
                enemySkills += "-수비 태세 ";

                myReduceDamage += defenseValue;
            }
            //수비 태세 나의 데미지 증가
            if (enemyUnits[enemyUnitIndex].charge && myUnits[myUnitIndex].defense)
            {
                mySkills += "수비 태세 ";

                myAddDamage += defenseValue;
            }

            //상대 공격
            // 돌진
            enemyMultiDamage = CalculateCharge(enemyUnits[enemyUnitIndex].charge, enemyUnits[enemyUnitIndex].mobility);

            if (enemyMultiDamage > 1) enemySkills += "돌격 ";

            //강한 돌진
            if (enemyUnits[enemyUnitIndex].strongCharge)
            {
                enemySkills += "강한 돌격 ";

                enemyMultiDamage += strongChargeValue;
            }
            //수비 태세 나의 피해 감소
            if (enemyUnits[enemyUnitIndex].charge && myUnits[myUnitIndex].defense)
            {
                mySkills += "수비 태세 ";

                enemyReduceDamage += defenseValue;
            }
            //수비 태세 상대 피해 증가
            if (myUnits[myUnitIndex].charge && enemyUnits[enemyUnitIndex].defense)
            {
                enemySkills += "수비 태세 ";

                myAddDamage += defenseValue;
            }
        }

        //상대 회피 실패 시 == 나의 공격
        if (!CalculateAccuracy(enemyUnits[enemyUnitIndex], myUnits[myUnitIndex].perfectAccuracy))
        {

            //둔기
            if (isMyBluntWeapon)
            {
                mySkills += "둔기 ";

                myAddDamage += bluntWeaponValue;
            }
            //도살
            if (myUnits[myUnitIndex].slaughter && enemyUnits[enemyUnitIndex].lightArmor)
            {
                mySkills += "도살 ";

                myAddDamage += slaughterValue;
            }
            //대기병
            if (enemyUnits[enemyUnitIndex].branchIdx == 5)
            {
                mySkills += "대기병 ";

                myAddDamage += myUnits[myUnitIndex].antiCavalry;
            }
            //수비 태세
            if (myUnits[myUnitIndex].defense && !isFirstAttack)
            {
                mySkills += "수비 태세 ";

                myReduceDamage -= defenseValue;
            }
            //관통
            if (myUnits[myUnitIndex].pierce)
            {
                mySkills += "관통 ";

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
            else
            {
                myDamage = 0;
            }

            CallDamageText(myDamage, mySkills, true);
        }
        else
        {
            CallDamageText(0, "충돌 회피", false);
        }

            //상대 공격
            if (!CalculateAccuracy(myUnits[myUnitIndex], enemyUnits[enemyUnitIndex].perfectAccuracy))
            {
                
                //둔기
                if (isEnemyBluntWeapon)
                {
                    enemySkills += "둔기 ";

                    enemyAddDamage += bluntWeaponValue;
                }
                //도살
                if (enemyUnits[enemyUnitIndex].slaughter && myUnits[myUnitIndex].lightArmor)
                {
                    enemySkills += "도살 ";

                    enemyAddDamage += slaughterValue;
                }
                //대기병
                if (myUnits[myUnitIndex].branchIdx == 5)
                {
                    enemySkills += "대기병 ";

                    enemyAddDamage += enemyUnits[enemyUnitIndex].antiCavalry;
                }
            //수비 태세
            if (enemyUnits[enemyUnitIndex].defense && !isFirstAttack)
            {
                enemySkills += "수비 태세 ";

                enemyReduceDamage -= defenseValue;
            }
            //관통
            if (enemyUnits[enemyUnitIndex].pierce)
                {
                    enemySkills += "관통 ";

                    enemyDamage = enemyUnits[enemyUnitIndex].attackDamage * enemyMultiDamage + enemyAddDamage - enemyReduceDamage;
                }
                else
                {
                    enemyDamage = enemyUnits[enemyUnitIndex].attackDamage * (1 - (myUnits[myUnitIndex].armor) / (10 + myUnits[myUnitIndex].armor)) * enemyMultiDamage + enemyAddDamage - enemyReduceDamage;
                }
            //상대 데미지가 0보다 큰지
            if (enemyDamage > 0)
            {
                myUnits[myUnitIndex].health -= enemyDamage;
            }
            else
            {
                enemyDamage = 0;
            }

            CallDamageText(enemyDamage, enemySkills, false);
        }
        else
        {
            CallDamageText(0, "충돌 회피", true);
        }
         return await UnitDeathSkill(myUnits, enemyUnits, myUnitIndex, enemyUnitIndex, myUnitMax, enemyUnitMax, myUnitMaxHp, enemyUnitMaxHp);
    }

    //지원 페이즈
    private async Task<(UnitDataBase[], UnitDataBase[])> SupportPhase(UnitDataBase[] myUnits, UnitDataBase[] enemyUnits, List<int> myRangeUnits, List<int> enemyRangeUnits, int myUnitIndex,int enemyUnitIndex,bool isEnemyHeavyArmor, bool isMyHeavyArmor, int myUnitMax, int enemyUnitMax, float myUnitMaxHp, float enemyUnitMaxHp)
    {
        //나의 지원
        //원거리 공격
        if (myRangeUnits.Count>0)
        {
            float allDamage = 0;
            for (int i = 0; i < myRangeUnits.Count; i++)
            {
                if (myRangeUnits[i] > myUnitIndex && myUnits[myRangeUnits[i]].range - (myRangeUnits[i] - myUnitIndex) >= 1)
                {
                    if (!CalculateAccuracy(enemyUnits[enemyUnitIndex], myUnits[myRangeUnits[i]].perfectAccuracy))    //상대 회피 실패
                    {
                        //중갑
                        if (isEnemyHeavyArmor)
                        {
                            if (myUnits[myRangeUnits[i]].attackDamage > heavyArmorValue)
                            {
                                allDamage+= myUnits[myRangeUnits[i]].attackDamage + heavyArmorValue;

                                enemyUnits[enemyUnitIndex].health -= myUnits[myRangeUnits[i]].attackDamage + heavyArmorValue;
                            }
                        }
                        else
                        {
                            allDamage += myUnits[myRangeUnits[i]].attackDamage;

                            enemyUnits[enemyUnitIndex].health -= myUnits[myRangeUnits[i]].attackDamage;
                        }
                    }
                }
            }
            CallDamageText(allDamage, "원거리", true);
        }

        //상대 지원 페이즈
        //원거리 공격
        if (enemyRangeUnits.Count>0)
        {
            float allDamage = 0;
            for (int i = 0; i < enemyRangeUnits.Count; i++)
            {
                if (i > enemyUnitIndex && enemyUnits[i].range - (i - enemyUnitIndex) >= 1)
                {
                    if (!CalculateAccuracy(myUnits[myUnitIndex], enemyUnits[enemyRangeUnits[i]].perfectAccuracy))    //나의 회피 실패
                    {
                        //중갑
                        if (isMyHeavyArmor)
                        {
                            if (enemyUnits[enemyRangeUnits[i]].attackDamage > heavyArmorValue)
                            {
                                allDamage += enemyUnits[enemyRangeUnits[i]].attackDamage + heavyArmorValue;

                                myUnits[myUnitIndex].health -= enemyUnits[enemyRangeUnits[i]].attackDamage + heavyArmorValue;
                            }
                        }
                        else
                        {
                            allDamage += enemyUnits[enemyRangeUnits[i]].attackDamage;

                            myUnits[myUnitIndex].health -= enemyUnits[enemyRangeUnits[i]].attackDamage;
                        }
                    }
                }

            }

            CallDamageText(allDamage, "원거리", false);
        }

        return await UnitDeathSkill(myUnits, enemyUnits, myUnitIndex, enemyUnitIndex, myUnitMax, enemyUnitMax, myUnitMaxHp, enemyUnitMaxHp);
    }

    // 유닛 사망 시 작동 스킬 호출 함수
    private async Task<(UnitDataBase[], UnitDataBase[])> UnitDeathSkill(
        UnitDataBase[] myUnits, UnitDataBase[] enemyUnits,
        int myUnitIndex, int enemyUnitIndex,
        int myUnitMax, int enemyUnitMax,
        float myUnitMaxHp, float enemyUnitMaxHp,
        float myBackUnitHp = 1, float enemyBackUnitHp = 1)
    {
        // 인덱스가 유효한지 확인 
        if (myUnitIndex >= myUnitMax || enemyUnitIndex >= enemyUnitMax)
        {
            return (myUnits, enemyUnits);
        }

        // 유닛이 죽었는지 확인 내 유닛이 죽었거나 상대유닛이 죽었을때
        if (enemyUnits[enemyUnitIndex].health <= 0 || myUnits[myUnitIndex].health <= 0 || myBackUnitHp <= 0 || enemyBackUnitHp <= 0)
        {
            await WaitForSecondsAsync();

            // 나의 유격 유격 o 내 다음 유닛 존재 상대 유닛 사망
            if (myUnits[myUnitIndex].guerrilla && myUnitIndex < myUnitMax - 1 && (enemyUnits[enemyUnitIndex].health <= 0 || enemyBackUnitHp <= 0))
            {
                CallDamageText(0, "유격", true);
                Debug.Log("나의 유격");

                // 다음 유닛으로 교체
                UnitDataBase tempUnit = myUnits[myUnitIndex];
                myUnits[myUnitIndex] = myUnits[myUnitIndex + 1];
                myUnits[myUnitIndex + 1] = tempUnit;

                TrueGarria();
            }

            // 나의 착취 착취o 내 유닛 체력 0이상 상대 유닛 사망
            if (myUnits[myUnitIndex].drain && myUnits[myUnitIndex].health > 0 && (enemyUnits[enemyUnitIndex].health <= 0 || enemyBackUnitHp <= 0))
            {
                CallDamageText(0, "착취", true);

                // 체력 회복 값의 오버플로우 방지 
                myUnits[myUnitIndex].health = Mathf.Min(myUnitMaxHp, drainHealValue + myUnits[myUnitIndex].health);
                myUnits[myUnitIndex].attackDamage += drainGainAttackValue;
            }

            // 상대 유격
            if (enemyUnits[enemyUnitIndex].guerrilla && enemyUnitIndex < enemyUnitMax - 1 && (myUnits[myUnitIndex].health <= 0 || myBackUnitHp <= 0))
            {
                CallDamageText(0, "유격", false);
                Debug.Log("상대 유격");

                // 다음 유닛으로 교체
                UnitDataBase tempUnit = enemyUnits[enemyUnitIndex];
                enemyUnits[enemyUnitIndex] = enemyUnits[enemyUnitIndex + 1];
                enemyUnits[enemyUnitIndex + 1] = tempUnit;

                TrueGarria();
            }

            // 상대 착취
            if (enemyUnits[enemyUnitIndex].drain && enemyUnits[enemyUnitIndex].health > 0 && (myUnits[myUnitIndex].health <= 0 || myBackUnitHp <= 0))
            {
                CallDamageText(0, "착취", false);

                // 체력 회복 값의 오버플로우 방지
                enemyUnits[enemyUnitIndex].health = Mathf.Min(enemyUnitMaxHp, drainHealValue + enemyUnits[enemyUnitIndex].health);
                enemyUnits[enemyUnitIndex].attackDamage += drainGainAttackValue;
            }
        }

        return (myUnits, enemyUnits);
    }

    //유닛 사망 처리 true ==사망 false == 생존
    private bool HandleUnitDeath(
    UnitDataBase[] units,
    ref int unitIndex,
    int unitMax,
    UnitDataBase[] enemyUnits,
    int enemyUnitIndex,
    ref bool isFirstAttack,
    ref bool isHeavyArmor,
    ref bool isBluntWeapon,
    Action<int, int> updateUnitCount,
    Action<float, float,float,float> updateUnitHp,
    Action<UnitDataBase[], UnitDataBase[], int, int> callCreateUnit,
    ref float unitMaxHp)
    {
        //유격 발동 시
        if (isGarria)
        {
            FalseGarria();
            isFirstAttack = true;

            isHeavyArmor = CalculateHeavyArmor(units[unitIndex].heavyArmor, enemyUnits[enemyUnitIndex].branchIdx);
            isBluntWeapon = CalculateBluntWeapon(units[unitIndex].bluntWeapon, enemyUnits[enemyUnitIndex].heavyArmor);

            unitMaxHp = units[unitIndex].health;

            Debug.Log("유격 성공");

            return true;

        }
        // 유닛 사망 여부 확인 범위 밖으로 나갔다 == 유닛전멸 게임종료
        if (unitIndex >= unitMax)
        {
            return true;
        }
        else if (units[unitIndex].health > 0 )   //유닛이 죽지 않았다 == 사망처리 x
        {
            return false;   
        }

        Debug.Log($"유닛 {units[unitIndex].unitName} 사망");

        unitIndex++; // 다음 유닛으로 이동
        isFirstAttack = true;

        // 유닛이 남아있다면 상태값 초기화
        if (unitIndex < unitMax)            
        {
            isHeavyArmor = CalculateHeavyArmor(units[unitIndex].heavyArmor, enemyUnits[enemyUnitIndex].branchIdx);
            isBluntWeapon = CalculateBluntWeapon(units[unitIndex].bluntWeapon, enemyUnits[enemyUnitIndex].heavyArmor);

            unitMaxHp = units[unitIndex].health;
        }

        return true; // 유닛 사망 처리 완료
    }

    private void UpdateUnitUI(UnitDataBase[] myUnits, UnitDataBase[] enemyUnits, int myUnitIndex, int enemyUnitIndex, int myUnitMax,int enemyUnitMax,float myUnitMaxHp,float enemyUnitMaxHp)
    {

        //유닛 생성 UI
        CallCreateUnit(myUnits, enemyUnits, myUnitIndex, enemyUnitIndex);

        //유닛 숫자 UI 최신화
        UpdateUnitCount(myUnitMax-myUnitIndex, enemyUnitMax-enemyUnitIndex);

        //유닛 체력 UI 최신화
        UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitIndex].health, myUnitMaxHp, enemyUnitMaxHp);
    }

}

