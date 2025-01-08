using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class AutoBattleManager : MonoBehaviour
{

    [SerializeField] private AutoBattleUI autoBattleUI;       //UI 관리 스크립트

    private readonly GoogleSheetLoader sheetLoader = new();
    
    private readonly float heavyArmorValue=15.0f;                //중갑 피해 감소
    private readonly float bluntWeaponValue = 15.0f;             //둔기 추가 피해
    private readonly float throwSpearValue = 50.0f;              //투창 추가 피해
    private readonly float overwhelmValue = 1.0f;                //위압 기동력 고정
    private readonly float strongChargeValue = 0.5f;             //강한 돌진 값
    private readonly float defenseValue = 15.0f;                 //수비태세 값
    private readonly float slaughterValue = 10.0f;               //도살 추가 피해
    private readonly float assassinationValue = 2.0f;            //암살 배율
    private readonly float drainHealValue = 20.0f;               //착취 회복량
    private readonly float drainGainAttackValue = 10.0f;         //착취 공격력 증가량

    private float waittingTime = 500;

    List<UnitDataBase> myUnits = new List<UnitDataBase>();
    List<UnitDataBase> enemyUnits = new List<UnitDataBase>();

    // 유닛 순서
    int myUnitIndex = 0;
    int enemyUnitIndex = 0;

    //유닛 총 갯수
    int myUnitMax;
    int enemyUnitMax;

    //사망 유닛 위치
    int myDeathUnitIndex = 0;
    int enemyDeathUnitIndex =0;

    //후열 유닛 체력 (암살에 사용)
    float myBackUnitHp = 1;
    float enemyBackUnitHp = 1;

    // 아군 원거리 유닛 갯수
    List<int> myRangeUnits = new();
    // 상대 원거리 유닛 갯수
    List<int> enemyRangeUnits = new();

    // 첫 공격
    bool isFirstAttack = true;

    private enum BattleState
    {
        None,
        Preparation,
        Crash,
        Support,
        Death,
        End
    }

    private BattleState currentState = BattleState.None;

    private bool isProcessing= false;

    //이 씬이 로드되었을 때== 구매 배치로 전투 씬 입장했을때
    private async void Start()
    {
        currentState = BattleState.None;

        List<int> myIds = PlayerData.Instance.ShowPlacedUnitList();
        List<int> enemyIds = PlayerData.Instance.GetEnemyUnitIndexes();
        if (myIds.Count <= 0) return;


        await InitializeBattle(myIds, enemyIds);
    }

    private async void Update()
    {
        if(isProcessing || Time.timeScale == 0) return;

        switch (currentState) 
        { 
            case BattleState.None:
                break;
            case BattleState.Preparation:
                if(HandleEnd()) break;
                // 암살로 인한 시체 유닛 발생 시
                if (myUnits[myUnitIndex].health <= 0)
                {
                    myUnitIndex++;
                    break;
                }
                if (enemyUnits[enemyUnitIndex].health <= 0)
                {
                    enemyUnitIndex++;
                    break;
                }
                await HandlePhase(HandlePreparation);
                break;
            case BattleState.Crash:
                await HandlePhase(HandleCrash);
                break;
            case BattleState.Support:
                await HandlePhase(HandleSupport);
                break;
            case BattleState.End:
                break;
        }

    }

    //페이즈 관리
    private async Task HandlePhase(Func<Task> phaseHandler)
    {
        isProcessing = true;
        await phaseHandler();
        UpdateUnitHp();
        //유닛이 죽지 않았고 끝나지 않음
        if (!ManageUnitDeath() && !(myUnitIndex >= myUnitMax || enemyUnitIndex >= enemyUnitMax))
        {
            switch (currentState)
            {
                case BattleState.Preparation:
                    currentState = BattleState.Crash;
                    break;
                case BattleState.Crash:
                    currentState = BattleState.Support;
                    break;
                case BattleState.Support:
                    isFirstAttack = false;
                    currentState = BattleState.Preparation;
                    break;
            }
        }
        else
        {
            HandleEnd();
        }
        await Task.Delay((int)waittingTime); // 0.5초 대기
        isProcessing = false;
        
    }

    //유닛 id를 바탕으로 유닛 데이터 저장
    private async Task<(List<UnitDataBase>, List<UnitDataBase>)> GetUnits(List<int> myUnitIds, List<int> enemyUnitIds)
    {
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
                    //강화 해서 추가
                    myUnits.Add(UpgradeManager.Instance.UpgradeUnit(unit));  // List에 유닛 추가
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


    //유닛 데이터 받고 전투 시작
    public async Task StartBattle(List<int> _myUnitIds, List<int> _enemyUnitIds)
    {
        await InitializeBattle(_myUnitIds, _enemyUnitIds);
    }

    //유닛 갯수 최신화
    private void UpdateUnitCount()
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
    private void  UpdateUnitHp()
    {
        float myUnitHp = myUnits[myUnitIndex].health;
        float enemyHp = enemyUnits[enemyUnitIndex].health;
        float myMAxHp = myUnits[myUnitIndex].maxHealth;
        float enemyMaxHp=enemyUnits[enemyUnitIndex].maxHealth;
        if (myUnitHp < 0)
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

    //암살 발동 가능한지 확인: 상대 후열 유닛 생존
    private bool CheckAssassination(List<UnitDataBase> units,int unitIndex)
    {
        for (int i = unitIndex + 1; i < units.Count; i++)
        {
            if (units[i].health > 0)
            {
                return true;
            }
        }

        return false;
    }


    // 암살 대상 계산: 체력이 가장 낮은 유닛의 인덱스를 반환
    private int CalculateAssassination(List<float> unitsHealth)
    {
        // 최소 체력 유닛 번호 (-1은 유효한 유닛이 없는 경우를 대비)
        int minHealthNumber = -1;
        // 최소 체력 기준 (초기값은 float.MaxValue로 설정)
        float lastHealth = float.MaxValue;

        for (int i = 0; i < unitsHealth.Count; i++)
        {
            // 체력이 0보다 크고, 현재 최소 체력보다 작은 경우 갱신
            if (unitsHealth[i] > 0 && unitsHealth[i] < lastHealth)
            {
                lastHealth = unitsHealth[i];
                minHealthNumber = i;
            }
        }

        // 최소 체력 유닛 번호 반환 (유효한 유닛이 없으면 -1 반환)
        return minHealthNumber+1;
    }


    //데미지 ui 호출
    private void CallDamageText(float damage,string text,bool team)
    {
        autoBattleUI.ShowDamage(MathF.Floor( damage), text, team);
    }

    // 유닛 생성UI 호출
    private void CallCreateUnit()
    {
        List<UnitDataBase> myRangeUnits=new();
        List<UnitDataBase> enemyRangUnits=new();
        List<UnitDataBase> myAliveUnits = new();
        List<UnitDataBase> enemyAliveUnits = new();
        //원거리 공격이 가능한지 검사
        for (int i = myUnitIndex + 1; i < myUnits.Count; i++)
        {
            if (myUnits[i].rangedAttack && (myUnits[i].range - (i - myUnitIndex) > 0) && myUnits[i].health>0)
            {
                myRangeUnits.Add(myUnits[i]);
            }
        }
        for (int i = enemyUnitIndex + 1; i < enemyUnits.Count; i++)
        {
            if (enemyUnits[i].rangedAttack && (enemyUnits[i].range - (i - enemyUnitIndex) > 0) && enemyUnits[i].health > 0)
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
    private void PreparationPhase()
    {
        //후열 유닛 체력
        myBackUnitHp = 1;
        enemyBackUnitHp = 1;

        //첫공격
        if (isFirstAttack)
        {
            string mySkills = "";
            string enemySkills = "";

            float myDamage=0;
            float enemyDamage=0;            

            //나의 준비
            //위압
            if (myUnits[myUnitIndex].overwhelm)
            {
                mySkills +="위압 ";

                enemyUnits[enemyUnitIndex].mobility = overwhelmValue;

            }
            //암살 = 나의 암살, 상대 후열 유닛 존재
            if (myUnits[myUnitIndex].assassination && CheckAssassination(enemyUnits,enemyUnitIndex))
            {
                mySkills += "암살 ";

                Debug.Log("암살");
                
                //상대 수호
                if (enemyUnits[enemyUnitIndex].guard)
                {
                    enemySkills += "수호 ";

                    //상대 회피 계산
                    if (!CalculateAccuracy(enemyUnits[enemyUnitIndex], myUnits[myUnitIndex].perfectAccuracy))    
                    {
                        myDamage += myUnits[myUnitIndex].attackDamage * (1 - (enemyUnits[enemyUnitIndex].armor) / (10 + enemyUnits[enemyUnitIndex].armor)) * assassinationValue;

                        enemyUnits[enemyUnitIndex].health -= myDamage;
                    }
                    else
                    {
                        mySkills += "회피 ";
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
                    
                    int minHealthIndex = enemyUnitIndex + CalculateAssassination(enemyUnitsHealth);

                    myDamage += myUnits[myUnitIndex].attackDamage * assassinationValue;

                    enemyBackUnitHp = enemyUnits[minHealthIndex].health;
                    enemyBackUnitHp -= myDamage;
                    enemyUnits[minHealthIndex].health=enemyBackUnitHp;

                    //후열 유닛 사망시 대입
                    if (enemyBackUnitHp <= 0)
                    {
                        int deadUnitCount = 0;
                        for (int i = enemyUnitIndex + 1; i < minHealthIndex; i++)
                        {
                            if (enemyUnits[i].health <= 0)
                            {
                                deadUnitCount++;
                            }
                        }
                        enemyDeathUnitIndex = minHealthIndex - enemyUnitIndex-deadUnitCount;
                    }
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
            if (enemyUnits[enemyUnitIndex].assassination && CheckAssassination(myUnits,myUnitIndex))
            {
                enemySkills += "암살 ";

                Debug.Log("암살");

                //나의 수호
                if (myUnits[myUnitIndex].guard)
                {
                    mySkills += "수호 ";

                    //나의 회피 계산
                    if (!CalculateAccuracy(myUnits[myUnitIndex], enemyUnits[enemyUnitIndex].perfectAccuracy))
                    {
                        enemyDamage += enemyUnits[enemyUnitIndex].attackDamage * (1 - (myUnits[myUnitIndex].armor) / (10 + myUnits[myUnitIndex].armor)) * assassinationValue;

                        myUnits[myUnitIndex].health -= enemyDamage;
                    }
                    else
                    {
                        enemySkills += "회피 ";
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
                    int minHealthIndex = myUnitIndex + CalculateAssassination(myUnitsHealth);

                    enemyDamage += enemyUnits[enemyUnitIndex].attackDamage * assassinationValue;

                    myBackUnitHp = myUnits[minHealthIndex].health;
                    myBackUnitHp -= enemyDamage;
                    myUnits[minHealthIndex].health = myBackUnitHp;

                    //후열 유닛 사망시 대입
                    if (myBackUnitHp <= 0)
                    {
                        int deadUnitCount = 0;
                        for (int i = myUnitIndex + 1; i < minHealthIndex; i++)
                        {
                            if (myUnits[i].health <= 0)
                            {
                                deadUnitCount++;
                            }
                        }
                        myDeathUnitIndex = minHealthIndex - myUnitIndex - deadUnitCount;
                    }
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

            
        }
    }

    //충돌 페이즈
    private void ChrashPhase()
    {
        float myMultiDamage = 1.0f;
        float enemyMultiDamage=1.0f;

        float myReduceDamage = 0f;
        float enemyReduceDamage = 0f;

        float myAddDamage = 0;
        float enemyAddDamage = 0;

        float myDamage;
        float enemyDamage;

        string mySkills ="충돌 ";
        string enemySkills ="충돌 ";

        //첫 공 일때 charge, strongCharge, defense 계산
        (myMultiDamage, myReduceDamage) = CalculateChrashIsFirstAttack(
            myUnits[myUnitIndex], enemyUnits[enemyUnitIndex], ref mySkills, ref enemySkills);

        (enemyMultiDamage, enemyReduceDamage) = CalculateChrashIsFirstAttack(
            enemyUnits[enemyUnitIndex], myUnits[myUnitIndex], ref enemySkills, ref mySkills);

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
            //관통
            if (myUnits[myUnitIndex].pierce)
            {
                mySkills += "관통 ";

                myDamage = myUnits[myUnitIndex].attackDamage * myMultiDamage + myAddDamage - myReduceDamage;
            }
            else
            {
                

                myDamage = myUnits[myUnitIndex].attackDamage * (1 - (enemyUnits[enemyUnitIndex].armor) / (10 + enemyUnits[enemyUnitIndex].armor)) * myMultiDamage + myAddDamage - myReduceDamage;
                Debug.Log(myDamage);
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
            CallDamageText(0, "충돌 회피 ", true);
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
            CallDamageText(0, "충돌 회피 ", false);
        }

        // CalculateChrash
    }

    //지원 페이즈
    private void SupportPhase()
    {
        //나의 지원
        //원거리 공격
        if (myRangeUnits.Count>0)
        {
            float allDamage = 0;
            for (int i = 0; i < myRangeUnits.Count; i++)
            {
                if (myRangeUnits[i] > myUnitIndex && myUnits[myRangeUnits[i]].range - (myRangeUnits[i] - myUnitIndex) >= 1 && myUnits[myRangeUnits[i]].health>0)
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
                CallDamageText(allDamage, "원거리 ", true);
            }
            
        }

        //상대 지원 페이즈
        //원거리 공격
        if (enemyRangeUnits.Count>0)
        {
            float allDamage = 0;
            for (int i = 0; i < enemyRangeUnits.Count; i++)
            {
                if (i > enemyUnitIndex && enemyUnits[i].range - (i - enemyUnitIndex) >= 1 && enemyUnits[enemyRangeUnits[i]].health > 0)
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
                CallDamageText(allDamage, "원거리 ", false);
            }
            
        }
    }
    

    //유닛 UI최신화
    private void UpdateUnitUI()
    {
        //유닛 생성 UI
        CallCreateUnit();

        //유닛 숫자 UI 최신화
        UpdateUnitCount();

        //유닛 체력 UI 최신화
        UpdateUnitHp();
    }


    //유닛 사망 처리 통합본
    private bool ManageUnitDeath()
    {
        //유닛 사망 검사
        if (myUnits[myUnitIndex].health <= 0 || enemyUnits[enemyUnitIndex].health <= 0 || myBackUnitHp <=0 || enemyBackUnitHp <=0)
        {
            isFirstAttack=true;

            int myAdd=0;
            int enemyAdd=0;

            //상대 유닛 사망
            if (enemyUnits[enemyUnitIndex].health <= 0 || enemyBackUnitHp <= 0)
            {
                //유격 발동
                if (myUnits[myUnitIndex].guerrilla && myUnitIndex+1 != myUnitMax)
                {
                    Debug.Log("내 유격");
                    CallDamageText(0,"유격 ",false);

                    (myUnits[myUnitIndex+1], myUnits[myUnitIndex]) = (myUnits[myUnitIndex], myUnits[myUnitIndex+1]);

                }
                //착취 발동
                if (myUnits[myUnitIndex].drain && myUnits[myUnitIndex].health>0)
                {
                    CallDamageText(0, "착취 ", false);

                    myUnits[myUnitIndex].health = Mathf.Min(myUnits[myUnitIndex].maxHealth, drainHealValue + myUnits[myUnitIndex].health);
                    myUnits[myUnitIndex].attackDamage += drainGainAttackValue;
                }
                ChangeInvisibile(false, enemyDeathUnitIndex);
                enemyAdd++;
            }
            //아군 유닛 사망
            if (myUnits[myUnitIndex].health <=0 || myBackUnitHp <= 0)
            {
                //유격 발동
                if (enemyUnits[enemyUnitIndex].guerrilla && enemyUnitIndex + 1 != enemyUnitMax)
                {
                    Debug.Log("상대 유격");
                    CallDamageText(0, "유격 ", true);

                    (enemyUnits[myUnitIndex + 1], enemyUnits[myUnitIndex]) = (enemyUnits[myUnitIndex], enemyUnits[enemyUnitIndex + 1]);

                }
                //착취 발동
                if (enemyUnits[enemyUnitIndex].drain && enemyUnits[enemyUnitIndex].health > 0)
                {
                    CallDamageText(0, "착취 ", true);

                    enemyUnits[enemyUnitIndex].health = Mathf.Min(enemyUnits[enemyUnitIndex].maxHealth, drainHealValue + enemyUnits[enemyUnitIndex].health);
                    enemyUnits[enemyUnitIndex].attackDamage += drainGainAttackValue;
                }

                ChangeInvisibile(true, myDeathUnitIndex);

                myAdd++;
            }

            // 암살+유격으로 죽은게 아닌 경우 == 다음 유닛으로 넘어감
            if (myBackUnitHp > 0 && enemyBackUnitHp > 0)
            {
                myUnitIndex += myAdd;
                enemyUnitIndex += enemyAdd;
            }

            myDeathUnitIndex =0;
            enemyDeathUnitIndex = 0;

            return true;
        }
        return false;
    }
    
    //유닛 데이터 초기화
    private async Task InitializeBattle(List<int> _myUnitIds, List<int> _enemyUnitIds)
    {
        if (autoBattleUI == null)
        {
            autoBattleUI = FindObjectOfType<AutoBattleUI>();
        }

        // 유닛 데이터 받아옴
        (myUnits, enemyUnits) = await GetUnits(_myUnitIds, _enemyUnitIds);

        myUnitIndex = 0;
        enemyUnitIndex = 0;

        myDeathUnitIndex = 0;
        enemyDeathUnitIndex = 0;

        myUnitMax = myUnits.Count;
        enemyUnitMax = enemyUnits.Count;

        myBackUnitHp = 1;
        enemyBackUnitHp = 1;

        myRangeUnits = new List<int>();
        enemyRangeUnits = new List<int>();

        isFirstAttack = true;

        // 원거리 유닛 필터링
        for (int i = 0; i < myUnitMax; i++)
            if (myUnits[i].rangedAttack) myRangeUnits.Add(i);

        for (int i = 0; i < enemyUnitMax; i++)
            if (enemyUnits[i].rangedAttack) enemyRangeUnits.Add(i);

        // 상태를 Preparation으로 설정
        currentState = BattleState.Preparation;
    }

    //준비 페이즈 관리
    private async Task HandlePreparation()
    {
        UpdateUnitUI();

        PreparationPhase();

        await Task.Yield();
    }

    //충돌 페이즈 관리
    private async Task HandleCrash()
    {
        ChrashPhase();

        await Task.Yield();
    }

    //지원 페이즈 관리
    private async Task HandleSupport()
    {
        SupportPhase();

        await Task.Yield();
    }

    //종료 확인
    private int CheckEnd()
    {
        if (myUnitIndex >= myUnitMax || enemyUnitIndex >= enemyUnitMax)
        {
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

        //유닛이 남았을 때
        return 3;
    }

    //사망시 투명하게 ui업뎃
    private void ChangeInvisibile(bool isMyUnit, int unitIndex)
    {
        autoBattleUI.ChangeInvisibleUnit(unitIndex, isMyUnit);
    }

    //속도 관리
    public void ChangeWaittingTime(float multiple)
    {
        waittingTime *= multiple;
    }

    //종료관리
    private bool HandleEnd()
    {
        int result = CheckEnd();
        if (result == 3)
        {
            currentState = BattleState.Preparation;
        }
        else
        {
            currentState = BattleState.End;
            autoBattleUI.FightEnd(result);
            return true;
        }
        return false;
    }

    //데미지 총괄 계산
    private float CalculateDamage(UnitDataBase attacker, UnitDataBase defender, float multiplier, float bonusDamage)
    {
        float damage = attacker.attackDamage * multiplier + bonusDamage;
        if (!attacker.pierce)
        {
            damage *= (1 - defender.armor / (10 + defender.armor));
        }
        return MathF.Max(damage, 0); // 데미지가 음수면 0으로 설정
    }

    //충돌 첫공 계산
    private (float, float) CalculateChrashIsFirstAttack(UnitDataBase attacker, UnitDataBase defender, ref string attackerSkill, ref string defenderSkill)
    {
        float multiplier = 1;
        float reduceDamage = 0;
        //첫 공 일 때
        if (isFirstAttack)
        {
            //돌격
            if (attacker.charge)
            {
                multiplier = CalculateCharge(attacker.mobility);
                //강한 돌격
                if (attacker.strongCharge)
                {
                    multiplier += strongChargeValue;
                    attackerSkill += "강한돌격 ";
                }
                else
                {
                    attackerSkill += "돌격 ";
                }
                //수비태세 = 방어자가 수배태세 일때
                //공격자 데미지 감소
                if (defender.defense)
                {
                    reduceDamage += defenseValue;
                    attackerSkill += "수비태세";
                    defenderSkill += "수비태세 ";
                }
            }
        }
        else
        {
            //첫 공 이 아닌데 상대가 돌격이 끝나고 공격자는 수비태세인 경우
            if (attacker.defense && defender.charge)
            {
                reduceDamage += defenseValue;
                attackerSkill += "수비태세 ";
            }
        }

        return (multiplier, reduceDamage);
    }

    //충돌 계산
    /*
     * //나의 공격 ==상대 회피 실패 상대 회피 , 내 필중 계산
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
            CallDamageText(0, "충돌 회피 ", true);
        }
     */
    private (UnitDataBase,float,bool) CalculateChrash(UnitDataBase attacker, UnitDataBase defender, ref string attackerSkill,ref string defenderSkill)
    {
        float multiplier;
        float reduceDamage;
        float damage = 0;
        bool isDodge=false;

        (multiplier, reduceDamage) = CalculateChrashIsFirstAttack(attacker,defender,ref attackerSkill,ref defenderSkill);

        //방어자의 회피 계산
        if (!CalculateAccuracy(defender, attacker.perfectAccuracy))
        {
            float bonusDamage = 0;
            

            //둔기
            if(attacker.bluntWeapon && defender.heavyArmor)
            {
                bonusDamage += bluntWeaponValue;
                attackerSkill += "둔기 ";
            }
            //도살
            if(attacker.slaughter && defender.lightArmor)
            {
                bonusDamage += slaughterValue;
                attackerSkill += "도살 ";
            }
            //대기병
            if (defender.branchIdx == 5)
            {
                bonusDamage += attacker.antiCavalry;
                attackerSkill += "대기병 ";
            }
            //관통
            if (attacker.perfectAccuracy)
            {
                damage = attacker.attackDamage * multiplier + bonusDamage - reduceDamage;
                attackerSkill += "관통 ";
            }
            else
            {
                damage = attacker.attackDamage * (1 - (defender.armor) / (10 + defender.armor)) * multiplier + bonusDamage - reduceDamage;
            }

            if (damage > 0)
            {
                defender.health -= damage;
            }
            else
            {
                damage = 0;
            }
        }
        else
        {
            defenderSkill = "회피 ";
            isDodge = true;
        }

        return (defender,damage,isDodge);
    }
}


