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
    
    private readonly float heavyArmorValue=15.0f;                //
    private readonly float bluntWeaponValue = 15.0f;             //
    private readonly float throwSpearValue = 50.0f;              //
    private readonly float overwhelmValue = 1.0f;                //
    private readonly float strongChargeValue = 0.5f;             //
    private readonly float defenseValue = 15.0f;                 //
    private readonly float slaughterValue = 10.0f;               //
    private readonly float assassinationValue = 2.0f;            //
    private readonly float drainHealValue = 20.0f;               //
    private readonly float drainGainAttackValue = 10.0f;         //

    private bool isGarria =false;                               //전투중 유격 발동 여부
    private bool isMyKill=false;                                //나의 살인
    private bool isEnemyKill = false;                           //상대 살인

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

    //
    private async Task<(List<UnitDataBase>, List<UnitDataBase>)> GetUnits(List<int> myUnitIds, List<int> enemyUnitIds)
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
        return (myUnits, enemyUnits);
    }


    //자동전투
    private async Task<int> AutoBattle(List<int> _myUnitIds, List<int> _enemyUnitIds)
    {
        if (autoBattleUI == null)
        {
            autoBattleUI = FindObjectOfType<AutoBattleUI>();
        }

        // 유닛 데이터 받아옴
        (List<UnitDataBase> myUnits, List<UnitDataBase> enemyUnits) = await GetUnits(_myUnitIds, _enemyUnitIds);

        // 유닛 순서
        int myUnitIndex = 0;
        int enemyUnitIndex = 0;

        //
        int myUnitMax = myUnits.Count;
        int enemyUnitMax = enemyUnits.Count;

        // 아군 원거리 유닛 갯수
        List<int> myRangeUnits= new ();
        // 상대 원거리 유닛 갯수
        List<int> enemyRangeUnits= new ();
        // 아군 전열 유닛 최대 체력
        float myUnitMaxHp = myUnits[myUnitIndex].health;
        // 적군 전열 유닛 최대 체력
        float enemyUnitMaxHp=enemyUnits[enemyUnitIndex].health;

        // 첫 공격
        bool isFirstAttack = true;

        //
        for (int i = 0; i < myUnitMax; i++)
        {
            if (myUnits[i].rangedAttack)
            {
                myRangeUnits.Add(i);
            }
        }
        //
        for (int i = 0; i < enemyUnitMax; i++)
        {
            if (enemyUnits[i].rangedAttack)
            {
                enemyRangeUnits.Add(i);
            }
        }        

        // 전투 반복
        while (myUnitIndex < myUnitMax && enemyUnitIndex < enemyUnitMax)
        {
            Debug.Log(isPause);
            if (isPause)
            {
               
            }

            // 암살로 인한 시체 유닛 발생 시
            if (myUnits[myUnitIndex].health <= 0)
            {
                myUnitIndex++;

                myUnitMaxHp = myUnits[myUnitIndex].health;

                continue;
            }
            if (enemyUnits[enemyUnitIndex].health <= 0)
            {
                enemyUnitIndex++;

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
            if (HandleUnitDeath(myUnits, ref myUnitIndex, myUnitMax, enemyUnits, enemyUnitIndex, ref isFirstAttack,
                ref myUnitMaxHp))
                continue; // 사망 시 준비부터 다시 시작

            // 사망 처리: 적 유닛
            if (HandleUnitDeath(enemyUnits, ref enemyUnitIndex, enemyUnitMax, myUnits, myUnitIndex, ref isFirstAttack,
                ref enemyUnitMaxHp))
                continue; // 사망 시 준비부터 다시 시작

            // 충돌
            (myUnits, enemyUnits) = await CombatPhase(
                myUnits, enemyUnits, isFirstAttack, myUnitIndex, enemyUnitIndex, myUnitMax, enemyUnitMax, myUnitMaxHp, enemyUnitMaxHp);
            UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitIndex].health, myUnitMaxHp, enemyUnitMaxHp);
            await WaitForSecondsAsync();

            // 사망 처리: 내 유닛
            if (HandleUnitDeath(myUnits, ref myUnitIndex, myUnitMax, enemyUnits, enemyUnitIndex, ref isFirstAttack, ref myUnitMaxHp))
                continue; // 사망 시 준비부터 다시 시작

            // 사망 처리: 적 유닛
            if (HandleUnitDeath(enemyUnits, ref enemyUnitIndex, enemyUnitMax, myUnits, myUnitIndex, ref isFirstAttack, ref enemyUnitMaxHp))
                continue; // 사망 시 준비부터 다시 시작

            // 지원
            if (!(myRangeUnits.Count == 0 && enemyRangeUnits.Count == 0))
            {
                (myUnits, enemyUnits) = await SupportPhase(
                    myUnits, enemyUnits, myRangeUnits, enemyRangeUnits, myUnitIndex, enemyUnitIndex, myUnitMax, enemyUnitMax, myUnitMaxHp, enemyUnitMaxHp);
            
            UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitIndex].health, myUnitMaxHp, enemyUnitMaxHp);
            await WaitForSecondsAsync();

            // 사망 처리: 내 유닛
            if (HandleUnitDeath(myUnits, ref myUnitIndex, myUnitMax, enemyUnits, enemyUnitIndex, ref isFirstAttack, ref myUnitMaxHp))
                continue; // 사망 시 준비부터 다시 시작

            // 사망 처리: 적 유닛
            if (HandleUnitDeath(enemyUnits, ref enemyUnitIndex, enemyUnitMax, myUnits, myUnitIndex, ref isFirstAttack, ref enemyUnitMaxHp))
                continue; // 사망 시 준비부터 다시 시작
            }

            isFirstAttack = false;
        }


        // 전투 종료 후 승리 여부 판단
        if (myUnitIndex < myUnitMax && enemyUnitIndex >= enemyUnitMax)
        {
            Debug.Log($"나의 승리 {myUnits[myUnitIndex].unitName + myUnits[myUnitIndex].health}");
            return 0;  // 나의 승리
        }
        else if (enemyUnitIndex < enemyUnitMax && myUnitIndex >= myUnitMax)
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
    private void UpdateUnitCount(List<UnitDataBase> myUnits, List<UnitDataBase> enemyUnits)
    {
        int myUnitCount = 0;
        int enemyUnitCount = 0;

        foreach (UnitDataBase unitData in myUnits)
        {
            if (unitData.health > 0)
            {
                myUnitCount++;
            }
        }
        foreach (UnitDataBase unitData in enemyUnits)
        {
            if(unitData.health > 0)
            {
                enemyUnitCount++;
            }
        }

          autoBattleUI.UpdateUnitCountUI(myUnitCount, enemyUnitCount);
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

    //도살 계산 공격 유닛 도살, 방어 유닛 경갑
    private bool CalculateSlaughter(bool offendingUnitSlaughter,bool deffendingUnitLightArmor)
    {
        return offendingUnitSlaughter && deffendingUnitLightArmor;
    }

    //중갑 계산 방어 유닛 중갑, 병종

    //둔기 계산 공격 유닛 둔기, 방어 유닛 중갑
    private bool CalculateBluntWeapon(bool offendingUnitBluntWeapon, bool deffendingUnitHeavyArmor)
    {
        return offendingUnitBluntWeapon && deffendingUnitHeavyArmor;
    }

    //돌진 계산
    private float CalculateCharge(float mobility)
    {
         return 1.1f + (1.9f / (9 * (mobility - 1)));
    }

    //암살 몇 번째 유닛 암살할지 계산
    private int CalculateAssassination(List<float> unitsHealth)
    {
        //최소 체력 유닛 번호
        int minHealthNumber = 1;
        //최소 체력 기준
        float lastHealth = unitsHealth[0];
        for (int i = 1; i < unitsHealth.Count; i++)
        {
            //유닛 체력이 더 적은지 확인
            if (unitsHealth[i] < lastHealth)
            {
                lastHealth = unitsHealth[i];
                minHealthNumber = i;
            }
        }

        return minHealthNumber;
    }

    //데미지 ui 호출
    private void CallDamageText(float damage,string text,bool team)
    {
        autoBattleUI.ShowDamage(MathF.Floor( damage), text, team);
    }

    // 유닛 생성UI 호출
    private void CallCreateUnit(List<UnitDataBase> myUnits, List<UnitDataBase> enemyUnits, int myUnitIndex, int enemyUnitIndex)
    {
        List<UnitDataBase> myRangeUnits=new();
        List<UnitDataBase> enemyRangUnits=new();
        List<UnitDataBase> myAliveUnits = new();
        List<UnitDataBase> enemyAliveUnits = new();
        //원거리 공격이 가능한지 검사
        for (int i = myUnitIndex + 1; i < myUnits.Count; i++)
        {
            if (myUnits[i].rangedAttack && (myUnits[i].range - (i - myUnitIndex) > 0))
            {
                myRangeUnits.Add(myUnits[i]);
            }
        }
        for (int i = enemyUnitIndex + 1; i < enemyUnits.Count; i++)
        {
            if (enemyUnits[i].rangedAttack && (enemyUnits[i].range - (i - enemyUnitIndex) > 0))
            {
                enemyRangUnits.Add(enemyUnits[i]);
            }
        }
        foreach (UnitDataBase unit in myUnits)
        {
            if (unit.health > 0)
            {
                myAliveUnits.Add(unit);
            }
        }
        foreach(UnitDataBase unit in enemyUnits)
        {
            if(unit.health > 0)
            {
                enemyAliveUnits.Add(unit);
            }
        }
        
        autoBattleUI.CreateUnitBox(myAliveUnits, enemyAliveUnits, CalculateDodge(myUnits[myUnitIndex]), CalculateDodge(enemyUnits[enemyUnitIndex]),myRangeUnits,enemyRangUnits);
    }

    //준비 페이즈
    private async Task<(List<UnitDataBase>, List<UnitDataBase>)> PreparationPhase(List<UnitDataBase> myUnits, List<UnitDataBase> enemyUnits, bool isFirstAttack,int myUnitIndex,int enemyUnitIndex, int myUnitMax,int enemyUnitMax,float myUnitMaxHp, float enemyUnitMaxHp)
    {
        //첫공격
        if (isFirstAttack)
        {
            string mySkills = "";
            string enemySkills = "";

            float myDamage=0;
            float enemyDamage=0;

            //후열 유닛 체력
            float myBackUnitHP=1 ;
            float enemyBackUnitHP=1 ;

            //나의 준비
            //위압
            if (myUnits[myUnitIndex].overwhelm)
            {
                mySkills +="위압 ";

                enemyUnits[enemyUnitIndex].mobility = overwhelmValue;

            }
            //암살 = 나의 암살, 상대 후열 유닛 존재
            if (myUnits[myUnitIndex].assassination && enemyUnitMax - enemyUnitIndex > 1)
            {
                mySkills += "암살 ";

                Debug.Log("암살");
                
                //상대 수호
                if (enemyUnits[enemyUnitIndex].guard)
                {
                    mySkills += "수호 ";

                    //상대 회피 계산
                    if (!CalculateAccuracy(enemyUnits[enemyUnitIndex], myUnits[myUnitIndex].perfectAccuracy))    
                    {
                        mySkills += "회피 ";
                    }
                    else
                    {
                        myDamage += myUnits[myUnitIndex].attackDamage * (1 - (enemyUnits[enemyUnitIndex].armor) / (10 + enemyUnits[enemyUnitIndex].armor)) * assassinationValue;

                        enemyUnits[enemyUnitIndex].health -= myDamage;
                    }
                    
                }
                //후열 암살 발동
                else
                {
                    //상대 후열 유닛 체력 저장
                    List<float> enemyUnitsHealth = new();
                    for(int i = enemyUnitIndex+1; i < enemyUnitMax; i++)
                    {
                        enemyUnitsHealth.Add(enemyUnits[i].health);
                    }
                    int minHealthNum = enemyUnitIndex + CalculateAssassination(enemyUnitsHealth);

                    myDamage += myUnits[myUnitIndex].attackDamage * assassinationValue;

                    enemyBackUnitHP = enemyUnits[minHealthNum].health;
                    enemyBackUnitHP -= myDamage;
                    enemyUnits[minHealthNum].health=enemyBackUnitHP;
                }
            }
            //투창
            if (myUnits[myUnitIndex].throwSpear)
            {
                mySkills += "투창 ";

                //상대 회피 실패
                if (!CalculateAccuracy(enemyUnits[enemyUnitIndex], myUnits[myUnitIndex].perfectAccuracy))    
                {
                    enemyUnits[enemyUnitIndex].health -= throwSpearValue;
                    myDamage += throwSpearValue;
                }
                else
                {
                    mySkills += "회피 ";
                }
            }

            //상대 준비
            //위압
            if (enemyUnits[enemyUnitIndex].overwhelm)
            {
                enemySkills += "위압 ";

                myUnits[myUnitIndex].mobility = overwhelmValue;
            }

            //암살 = 상대의 암살, 나의 후열 유닛 존재
            if (enemyUnits[enemyUnitIndex].assassination && myUnitMax - myUnitIndex > 1)
            {
                enemySkills += "암살 ";

                Debug.Log("암살");

                //나의 수호
                if (myUnits[myUnitIndex].guard)
                {
                    enemySkills += "수호 ";

                    //나의 회피 계산
                    if (!CalculateAccuracy(myUnits[myUnitIndex], enemyUnits[enemyUnitIndex].perfectAccuracy))
                    {
                        enemySkills += "회피 ";
                    }
                    else
                    {
                        enemyDamage += enemyUnits[enemyUnitIndex].attackDamage * (1 - (myUnits[myUnitIndex].armor) / (10 + myUnits[myUnitIndex].armor)) * assassinationValue;

                        myUnits[myUnitIndex].health -= enemyDamage;
                    }

                }
                //후열 암살 발동
                else
                {
                    //나의 후열 유닛 체력 저장
                    List<float> myUnitsHealth = new();
                    for (int i = myUnitIndex + 1; i < myUnitMax; i++)
                    {
                        myUnitsHealth.Add(myUnits[i].health);
                    }
                    int minHealthNum = myUnitIndex + CalculateAssassination(myUnitsHealth);

                    enemyDamage += enemyUnits[enemyUnitIndex].attackDamage * assassinationValue;

                    myBackUnitHP = myUnits[minHealthNum].health;
                    myBackUnitHP -= enemyDamage;
                    myUnits[minHealthNum].health = myBackUnitHP;
                }
            }

            //투창
            if (enemyUnits[enemyUnitIndex].throwSpear)
            {
                enemySkills += "투창 ";

                if (!CalculateAccuracy(myUnits[myUnitIndex], enemyUnits[enemyUnitIndex].perfectAccuracy))    //나의 회피 실패
                {
                    myUnits[myUnitIndex].health -= throwSpearValue;
                    enemyDamage += throwSpearValue;
                }
                else
                {
                    enemySkills += "회피 ";
                }
            }

            if (mySkills != "")
            {
                CallDamageText(myDamage, mySkills, true);
            }
            if (enemySkills != "")
            {
                CallDamageText(enemyDamage, enemySkills, false);
            }

            return await UnitDeathSkill(myUnits, enemyUnits, myUnitIndex, enemyUnitIndex, myUnitMax, enemyUnitMax, myUnitMaxHp, enemyUnitMaxHp,myBackUnitHP,enemyBackUnitHP);
        }
        return (myUnits, enemyUnits);
    }

    //전투 페이즈
    private async Task<(List<UnitDataBase>, List<UnitDataBase>)> CombatPhase(List<UnitDataBase> myUnits, List<UnitDataBase> enemyUnits ,bool isFirstAttack, int myUnitIndex, int enemyUnitIndex,int myUnitMax,int enemyUnitMax,float myUnitMaxHp,float enemyUnitMaxHp)
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
            if (myUnits[myUnitIndex].charge)
            {
                myMultiDamage = CalculateCharge(myUnits[myUnitIndex].mobility);

                //강한 돌격
                if (myUnits[myUnitIndex].strongCharge)
                {
                    myMultiDamage += strongChargeValue;

                    mySkills += "강한 돌격 ";
                }
                else
                {
                    mySkills += "돌격 ";
                }

                //수비 태세 = 상대가 수비태세 일때 
                // 나의 데미지 감소
                if (enemyUnits[enemyUnitIndex].defense)
                {
                    mySkills += "수비 태세 ";
                    myReduceDamage += defenseValue;
                }

            }

            //상대 공격
            // 돌격
            if (enemyUnits[enemyUnitIndex].charge)
            {
                enemyMultiDamage = CalculateCharge(enemyUnits[enemyUnitIndex].mobility);

                //강한 돌격
                if (enemyUnits[enemyUnitIndex].strongCharge)
                {
                    enemyMultiDamage += strongChargeValue;

                    enemySkills += "강한 돌격 ";
                }
                else
                {
                    enemySkills += "돌격 ";
                }

                //수비 태세 = 내가 수비태세 일때 
                // 상대 데미지 감소
                if (myUnits[myUnitIndex].defense)
                {
                    enemySkills += "수비 태세 ";
                    enemyReduceDamage += defenseValue;
                }
            }
        }

        //나의 공격 ==상대 회피 실패 상대 회피 , 내 필중 계산
        if (!CalculateAccuracy(enemyUnits[enemyUnitIndex], myUnits[myUnitIndex].perfectAccuracy))
        {

            //둔기
            if (CalculateBluntWeapon(myUnits[myUnitIndex].bluntWeapon, enemyUnits[enemyUnitIndex].heavyArmor))
            {
                mySkills += "둔기 ";

                myAddDamage += bluntWeaponValue;
            }
            //도살
            if (CalculateSlaughter(myUnits[myUnitIndex].slaughter, enemyUnits[enemyUnitIndex].lightArmor))
            {
                mySkills += "도살 ";

                myAddDamage += slaughterValue;
            }
            //대기병 = 상대 유닛 병종==5(기병) 일 때
            if (enemyUnits[enemyUnitIndex].branchIdx == 5)
            {
                mySkills += "대기병 ";

                myAddDamage += myUnits[myUnitIndex].antiCavalry;
            }
            //수비 태세
            if (myUnits[myUnitIndex].defense )
            {
                mySkills += "수비 태세 ";

                myReduceDamage += defenseValue;
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
            if (myDamage >= 0)
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
            CallDamageText(0, "충돌 회피", true);
        }

            //상대 공격 == 나의 회피 실패 나의 회피, 상대 필중 계산
            if (!CalculateAccuracy(myUnits[myUnitIndex], enemyUnits[enemyUnitIndex].perfectAccuracy))
            {
                
                //둔기
                if (CalculateBluntWeapon(enemyUnits[enemyUnitIndex].bluntWeapon, myUnits[myUnitIndex].heavyArmor))
                {
                    enemySkills += "둔기 ";

                    enemyAddDamage += bluntWeaponValue;
                }
                //도살
                if (CalculateSlaughter(enemyUnits[enemyUnitIndex].slaughter, myUnits[myUnitIndex].lightArmor))
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
            if (enemyUnits[enemyUnitIndex].defense )
            {
                enemySkills += "수비 태세 ";

                enemyReduceDamage += defenseValue;
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
            if (enemyDamage >= 0)
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
            CallDamageText(0, "충돌 회피", false);
        }
         return await UnitDeathSkill(myUnits, enemyUnits, myUnitIndex, enemyUnitIndex, myUnitMax, enemyUnitMax, myUnitMaxHp, enemyUnitMaxHp);
    }

    //지원 페이즈
    private async Task<(List<UnitDataBase>, List<UnitDataBase>)> SupportPhase(List<UnitDataBase> myUnits, List<UnitDataBase> enemyUnits, List<int> myRangeUnits, List<int> enemyRangeUnits, int myUnitIndex,int enemyUnitIndex, int myUnitMax, int enemyUnitMax, float myUnitMaxHp, float enemyUnitMaxHp)
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
                        //공격받는 유닛(상대) 중갑 유무
                        if (enemyUnits[enemyUnitIndex].heavyArmor)
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
            if (allDamage > 0)
            {
                CallDamageText(allDamage, "원거리", true);
            }
            
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
                        //공격받는 유닛(아군) 중갑 유무
                        if (myUnits[myUnitIndex].heavyArmor)
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
            if (allDamage > 0)
            {
                CallDamageText(allDamage, "원거리", false);
            }
            
        }

        return await UnitDeathSkill(myUnits, enemyUnits, myUnitIndex, enemyUnitIndex, myUnitMax, enemyUnitMax, myUnitMaxHp, enemyUnitMaxHp);
    }

    // 유닛 사망 시 작동 스킬 호출 함수
    private async Task<(List<UnitDataBase>, List<UnitDataBase>)> UnitDeathSkill(
        List<UnitDataBase> myUnits, List<UnitDataBase> enemyUnits,
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
    List<UnitDataBase> units,
    ref int unitIndex,
    int unitMax,
    List<UnitDataBase> enemyUnits,
    int enemyUnitIndex,
    ref bool isFirstAttack,
    ref float unitMaxHp)
    {
        //유격 발동 시
        if (isGarria)
        {
            FalseGarria();
            isFirstAttack = true;

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

            unitMaxHp = units[unitIndex].health;
        }

        return true; // 유닛 사망 처리 완료
    }

    private void UpdateUnitUI(List<UnitDataBase> myUnits, List<UnitDataBase> enemyUnits, int myUnitIndex, int enemyUnitIndex, int myUnitMax,int enemyUnitMax,float myUnitMaxHp,float enemyUnitMaxHp)
    {

        //유닛 생성 UI
        CallCreateUnit(myUnits, enemyUnits, myUnitIndex, enemyUnitIndex);

        //유닛 숫자 UI 최신화
        UpdateUnitCount(myUnits, enemyUnits);

        //유닛 체력 UI 최신화
        UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitIndex].health, myUnitMaxHp, enemyUnitMaxHp);
    }

}

