using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Random = UnityEngine.Random;

public class AutoBattleManager : MonoBehaviour
{

    [SerializeField] private AutoBattleUI autoBattleUI;       //UI 관리 스크립트

    private float waittingTime=0.2f; //    

    private GoogleSheetLoader sheetLoader = new GoogleSheetLoader();
    
    private float heavyArmorValue=15.0f;                //중갑 고정 데미지 감소량
    private float bluntWeaponValue = 15.0f;             //둔기 고정 데미지
    private float throwSpearValue = 50.0f;              //투창 고정 데미지
    private float overwhelmValue = 1.0f;                //위압 기동력 고정값
    private float strongChargeValue = 0.5f;             //강한 돌격 데미지 배수
    private float defenseValue = 15.0f;                 //수배 태세 고정 데미지 감소량
    private float slaughterValue = 10.0f;               //도살 고정 데미지
    private float assassinationValue = 2.0f;            //암살 데미지 배율
    private float drainHealValue = 20.0f;               //착취 고정 회복량
    private float drainGainAttackValue = 10.0f;         //착취 고정 공격력 증가량

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
        // 나와 적의 유닛을 호출
        (UnitDataBase[] myUnits, UnitDataBase[] enemyUnits) = await GetUnits(_myUnitIds, _enemyUnitIds);

        // 인덱스를 사용해서 현재 전투에 참여하는 유닛 추적
        int myUnitIndex = 0;
        int enemyUnitIndex = 0;

        //전투 페이즈 0준비, 1 전투, 2 종료
        int fightPhase = 0;

        //최초의 유닛 갯수
        int myUnitMax = _myUnitIds.Length;
        int enemyUnitMax = _enemyUnitIds.Length;

        //나의 원거리 유닛 위치
        List<int> myRangeUnits= new List<int>();
        //상대 원거리 유닛 위치
        List<int> enemyRangeUnits= new List<int>();
        //나의 유닛 최대 체력
        float myUnitMaxHp = myUnits[myUnitIndex].health;
        //상대 유닛 최대 체력
        float enemyUnitMaxHp=enemyUnits[enemyUnitIndex].health;

        // 나의 피해량
        float myDamage= 0;
        // 적의 피해량
        float enemyDamage = 0;

        // 첫 공격
        bool isFirstAttack = true;
        
        // 나의 중갑 계산
        bool isMyHeavyArmor = CalculateHeavyArmor(myUnits[myUnitIndex].heavyArmor, enemyUnits[enemyUnitIndex].branchIdx);
        //적의 중갑 계산
        bool isEnemyHeavyArmor=CalculateHeavyArmor(enemyUnits[myUnitIndex].heavyArmor, myUnits[enemyUnitIndex].branchIdx);

        //나의 둔기 계산
        bool isMyBluntWeapon = CalculateBluntWeapon(myUnits[myUnitIndex].bluntWeapon, enemyUnits[enemyUnitIndex].heavyArmor);
        //적의 둔기 계산
        bool isEnemyBluntWeapon = CalculateBluntWeapon(enemyUnits[myUnitIndex].bluntWeapon, myUnits[enemyUnitIndex].heavyArmor);

        //나의 원거리 유닛 위치값 초기화
        for (int i = 0; i < myUnitMax; i++)
        {
            if (myUnits[i].rangedAttack)
            {
                myRangeUnits.Add(i);
            }
        }
        //상대 원거리 유닛 위치값 초기화
        for (int i = 0; i < enemyUnitMax; i++)
        {
            if (enemyUnits[i].rangedAttack)
            {
                enemyRangeUnits.Add(i);
            }
        }

        //유닛 수 UI 초기화 함수 호출
        UpdateUnitCount(_myUnitIds.Length,_enemyUnitIds.Length);

        //유닛 Hp UI 초기화 함수 호출
        UpdateUnitHp(myUnits[0].health, enemyUnits[0].health);

        UpdateUnitName(myUnits[0].unitBranch, enemyUnits[0].unitBranch);

        // 전투 반복
        while (myUnitIndex < myUnits.Length && enemyUnitIndex < enemyUnits.Length)
        {
            UpdateUnitName(myUnits[myUnitIndex].unitBranch, enemyUnits[enemyUnitIndex].unitBranch);

            //나의 추가 피해량
            float myAddDamage = 0;
            //적의 추가 피해량
            float enemyAddDamage = 0;
            //나의 고정 피해 감소량
            float myReduceDamage= 0;
            //적의 고정 피해 감소량
            float enemyReduceDamage=0;
            //나의 데미지 배율
            float myMultiDamage=1.0f;
            //적의 데미지 배율
            float enemyMultiDamage = 1.0f;

            if (isFirstAttack)      //첫 공격일 때
            {
                switch (fightPhase) //전투
                {   
                    case 0:         //준비
                        //암살 대상 hp 없으면 기본값 1
                        float myBackUnitHP = 1;
                        float enemyBackUnitHP = 1;
                        //내 기술 먼저
                        //위압
                        if (myUnits[myUnitIndex].overwhelm)
                        {
                            enemyUnits[enemyUnitIndex].mobility = overwhelmValue;
                        }
                        //암살
                        if (myUnits[myUnitIndex].assassination ||  enemyUnitMax- enemyUnitIndex > 1)//암살 있고 상대 유닛 1마리 초과
                        {
                            List<float> enemyUnitHealth = new List<float>();
                            foreach (var unit in enemyUnits)
                            {
                                enemyUnitHealth.Add(unit.health);   //상대 유닛 체력 순서대로 배열화
                            }
                            //수호
                            if (enemyUnits[enemyUnitIndex].guard)   
                            {
                                if (!CalculateAccuracy(enemyUnits[enemyUnitIndex], myUnits[myUnitIndex].perfectAccuracy))    //상대 회피 실패 시
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
                        //투창
                        if (myUnits[myUnitIndex].throwSpear)
                        {
                            if (!CalculateAccuracy(enemyUnits[enemyUnitIndex], myUnits[myUnitIndex].perfectAccuracy))    //상대 회피 실패 시
                            {
                                enemyUnits[enemyUnitIndex].health -= throwSpearValue;
                            }

                        }
                        

                        //적 기술
                        //위압
                        if (enemyUnits[enemyUnitIndex].overwhelm)
                        {
                            myUnits[myUnitIndex].mobility = overwhelmValue;
                        }

                        //암살
                        if (enemyUnits[enemyUnitIndex].assassination || myUnitMax - myUnitIndex > 1)//암살 있고 상대 유닛 1마리 초과
                        {
                            List<float> myUnitHealth = new List<float>();
                            foreach (var unit in enemyUnits)
                            {
                                myUnitHealth.Add(unit.health);   //상대 유닛 체력 순서대로 배열화
                            }
                            //수호
                            if (myUnits[myUnitIndex].guard)
                            {
                                if (!CalculateAccuracy(myUnits[myUnitIndex], enemyUnits[enemyUnitIndex].perfectAccuracy))    //내가 회피 실패 시
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

                        //투창
                        if (enemyUnits[enemyUnitIndex].throwSpear)
                        {
                            if (!CalculateAccuracy(myUnits[myUnitIndex], enemyUnits[enemyUnitIndex].perfectAccuracy))    //내가 회피 실패 시
                            {
                                myUnits[myUnitIndex].health -= throwSpearValue;
                            }
                            
                        }


                        UpdateUnitHp(myUnits[myUnitIndex].health, enemyUnits[enemyUnitIndex].health);
                        await WaitForSecondsAsync(0.5f);

                        //유닛 사망시 일단 새로운 페이즈
                        if (enemyUnits[enemyUnitIndex].health <= 0 || myUnits[myUnitIndex].health <= 0)
                        {
                            //나의 유격
                            if (myUnits[myUnitIndex].guerrilla && myUnitMax - myUnitIndex > 1 &&(enemyUnits[enemyUnitIndex].health<=0 || enemyBackUnitHP<=0) )
                            {
                                UnitDataBase thisUnit = myUnits[myUnitIndex];
                                myUnits[myUnitIndex] = myUnits[myUnitIndex + 1];
                                myUnits[myUnitIndex + 1] = thisUnit;
                            }
                            //나의 착취
                            if (myUnits[myUnitIndex].drain && (enemyUnits[enemyUnitIndex].health <= 0 || enemyBackUnitHP <= 0) && myUnits[myUnitIndex].health>0)
                            {
                                myUnits[myUnitIndex].health = drainHealValue + myUnits[myUnitIndex].health > myUnitMaxHp ? myUnitMaxHp : drainHealValue + myUnits[myUnitIndex].health;
                            }
                            //상대 유격
                            if (enemyUnits[enemyUnitIndex].guerrilla && enemyUnitMax - enemyUnitIndex > 1 && (myUnits[myUnitIndex].health <= 0 || myBackUnitHP <= 0))
                            {
                                UnitDataBase thisUnit = enemyUnits[enemyUnitIndex];
                                enemyUnits[myUnitIndex] = enemyUnits[enemyUnitIndex + 1];
                                enemyUnits[enemyUnitIndex + 1] = thisUnit;
                            }
                            //상대 착취
                            if (enemyUnits[enemyUnitIndex].drain && (myUnits[myUnitIndex].health <= 0 || myBackUnitHP <= 0) && enemyUnits[enemyUnitIndex].health>0)
                            {
                                enemyUnits[enemyUnitIndex].health = drainHealValue + enemyUnits[enemyUnitIndex].health > enemyUnitMaxHp ? enemyUnitMaxHp : drainHealValue + enemyUnits[enemyUnitIndex].health;
                            }
                            break;
                        }
                            continue;
                    case 1:         //충돌
                        //내 전투 충돌
                        if (!CalculateAccuracy(enemyUnits[enemyUnitIndex], myUnits[myUnitIndex].perfectAccuracy))    //상대 회피 실패 시
                        {
                            // 돌격
                            myMultiDamage = CalculateCharge(myUnits[myUnitIndex].charge, myUnits[myUnitIndex].mobility);
                            //강한 돌격
                            if (myUnits[myUnitIndex].strongCharge)
                            {
                                myMultiDamage += strongChargeValue;
                            }
                            //수비 태세 상대 데미지 감소
                            if (myUnits[myUnitIndex].charge && enemyUnits[enemyUnitIndex].defense)
                            {
                                myReduceDamage += defenseValue;
                            }
                            //수비 태세 나의 데미지 증가
                            if (enemyUnits[enemyUnitIndex].charge && myUnits[myUnitIndex].defense)
                            {
                                myAddDamage += defenseValue;
                            }
                            //둔기
                            if (isMyBluntWeapon)
                            {
                                myAddDamage += bluntWeaponValue;
                            }
                            //도살
                            if (myUnits[myUnitIndex].slaughter && enemyUnits[enemyUnitIndex].lightArmor)
                            {
                                myAddDamage += slaughterValue;
                            }
                            //대기병
                            if (enemyUnits[enemyUnitIndex].branchIdx == 5)
                            {
                                myAddDamage += myUnits[myUnitIndex].antiCavalry;
                            }
                            //관통
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

                        //상대 전투 충돌
                        if (!CalculateAccuracy(myUnits[myUnitIndex], enemyUnits[enemyUnitIndex].perfectAccuracy))        //내가 회피 실패 시
                        {
                            
                                // 돌격
                                enemyMultiDamage = CalculateCharge(enemyUnits[enemyUnitIndex].charge, enemyUnits[enemyUnitIndex].mobility);
                                //강한 돌격
                                if (enemyUnits[enemyUnitIndex].strongCharge)
                                {
                                    enemyMultiDamage += strongChargeValue;
                                }
                                //수비 태세 나의 데미지 감소
                                if (enemyUnits[enemyUnitIndex].charge && myUnits[myUnitIndex].defense)
                                {
                                    enemyReduceDamage += defenseValue;
                                }
                                //수비 태세 상대 데미지 증가
                                if (myUnits[myUnitIndex].charge && enemyUnits[enemyUnitIndex].defense)
                                {
                                    myAddDamage += defenseValue;
                                }
                                //둔기
                                if (isEnemyBluntWeapon)
                                {
                                    enemyAddDamage += bluntWeaponValue;
                                }
                                //도살
                                if (enemyUnits[enemyUnitIndex].slaughter && myUnits[myUnitIndex].lightArmor)
                                {
                                    enemyAddDamage += slaughterValue;
                                }
                                //대기병
                                if (myUnits[myUnitIndex].branchIdx == 5)
                                {
                                    enemyAddDamage += enemyUnits[enemyUnitIndex].antiCavalry;
                                }
                                //관통
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

                        //유닛 사망시 일단 새로운 페이즈
                        if (enemyUnits[enemyUnitIndex].health <= 0 || myUnits[myUnitIndex].health <= 0)
                        {
                            //나의 유격
                            if (myUnits[myUnitIndex].guerrilla && myUnitMax - myUnitIndex > 1 && enemyUnits[enemyUnitIndex].health <= 0)
                            {
                                UnitDataBase thisUnit = myUnits[myUnitIndex];
                                myUnits[myUnitIndex] = myUnits[myUnitIndex + 1];
                                myUnits[myUnitIndex + 1] = thisUnit;
                            }
                            //나의 착취
                            if (myUnits[myUnitIndex].drain && enemyUnits[enemyUnitIndex].health <= 0 && myUnits[myUnitIndex].health > 0)
                            {
                                myUnits[myUnitIndex].health = drainHealValue + myUnits[myUnitIndex].health > myUnitMaxHp ? myUnitMaxHp : drainHealValue + myUnits[myUnitIndex].health;
                            }
                            //상대 유격
                            if (enemyUnits[enemyUnitIndex].guerrilla && enemyUnitMax - enemyUnitIndex > 1 && myUnits[myUnitIndex].health <= 0)
                            {
                                UnitDataBase thisUnit = enemyUnits[enemyUnitIndex];
                                enemyUnits[myUnitIndex] = enemyUnits[enemyUnitIndex + 1];
                                enemyUnits[enemyUnitIndex + 1] = thisUnit;
                            }
                            //상대 착취
                            if (enemyUnits[enemyUnitIndex].drain && myUnits[myUnitIndex].health <= 0 && enemyUnits[enemyUnitIndex].health > 0)
                            {
                                enemyUnits[enemyUnitIndex].health = drainHealValue + enemyUnits[enemyUnitIndex].health > enemyUnitMaxHp ? enemyUnitMaxHp : drainHealValue + enemyUnits[enemyUnitIndex].health;
                            }
                            break;
                        }


                        continue;
                    case 2:         //지원
                        //내 유닛 지원
                        //원거리 공격
                        for (int i = myRangeUnits[0]; i < myRangeUnits.Count; i++)
                        {
                            if(i > myUnitIndex && myUnits[i].range - (i - myUnitIndex) >= 1 )
                            {
                                if (!CalculateAccuracy(enemyUnits[enemyUnitIndex], myUnits[myRangeUnits[i]].perfectAccuracy))    //상대 회피 실패 시
                                {
                                    //중갑
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

                        //상대 유닛 지원
                        //원거리 공격
                        for (int i = enemyRangeUnits[0]; i < enemyRangeUnits.Count; i++)
                        {
                            if (i > enemyUnitIndex && enemyUnits[i].range - (i - enemyUnitIndex) >= 1)
                            {
                                if (!CalculateAccuracy(myUnits[myUnitIndex], enemyUnits[enemyRangeUnits[i]].perfectAccuracy))    //상대 회피 실패 시
                                {
                                    //중갑
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

                        //유닛 사망시 
                        if (enemyUnits[enemyUnitIndex].health <= 0 || myUnits[myUnitIndex].health <= 0)
                        {
                            //나의 유격
                            if (myUnits[myUnitIndex].guerrilla && myUnitMax - myUnitIndex > 1 && enemyUnits[enemyUnitIndex].health <= 0)
                            {
                                UnitDataBase thisUnit = myUnits[myUnitIndex];
                                myUnits[myUnitIndex] = myUnits[myUnitIndex + 1];
                                myUnits[myUnitIndex + 1] = thisUnit;
                            }
                            //나의 착취
                            if (myUnits[myUnitIndex].drain && enemyUnits[enemyUnitIndex].health <= 0 && myUnits[myUnitIndex].health > 0)
                            {
                                myUnits[myUnitIndex].health = drainHealValue + myUnits[myUnitIndex].health > myUnitMaxHp ? myUnitMaxHp : drainHealValue + myUnits[myUnitIndex].health;
                            }
                            //상대 유격
                            if (enemyUnits[enemyUnitIndex].guerrilla && enemyUnitMax - enemyUnitIndex > 1 && myUnits[myUnitIndex].health <= 0)
                            {
                                UnitDataBase thisUnit = enemyUnits[enemyUnitIndex];
                                enemyUnits[myUnitIndex] = enemyUnits[enemyUnitIndex + 1];
                                enemyUnits[enemyUnitIndex + 1] = thisUnit;
                            }
                            //상대 착취
                            if (enemyUnits[enemyUnitIndex].drain && myUnits[myUnitIndex].health <= 0 && enemyUnits[enemyUnitIndex].health > 0)
                            {
                                enemyUnits[enemyUnitIndex].health = drainHealValue + enemyUnits[enemyUnitIndex].health > enemyUnitMaxHp ? enemyUnitMaxHp : drainHealValue + enemyUnits[enemyUnitIndex].health;
                            }
                            break;
                        }

                        break;
                }
            }
            else                    //첫 공격이 아닐 때
            {
                switch (fightPhase) //전투
                {
                    case 0:         //준비
                        continue;
                    case 1:         //충돌
                        //내 전투 충돌
                        if (!CalculateAccuracy(enemyUnits[enemyUnitIndex], myUnits[myUnitIndex].perfectAccuracy))    //상대 회피 실패 시
                        {
                            
                            //둔기
                            if (isMyBluntWeapon)
                            {
                                myAddDamage += bluntWeaponValue;
                            }
                            //도살
                            if (myUnits[myUnitIndex].slaughter && enemyUnits[enemyUnitIndex].lightArmor)
                            {
                                myAddDamage += slaughterValue;
                            }
                            //대기병
                            if (enemyUnits[enemyUnitIndex].branchIdx == 5)
                            {
                                myAddDamage += myUnits[myUnitIndex].antiCavalry;
                            }
                            //관통
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

                        //상대 전투 충돌
                        if (!CalculateAccuracy(myUnits[myUnitIndex], enemyUnits[enemyUnitIndex].perfectAccuracy))        //내가 회피 실패 시
                        {
                            
                            //둔기
                            if (isEnemyBluntWeapon)
                            {
                                enemyAddDamage += bluntWeaponValue;
                            }

                            //도살
                            if (enemyUnits[enemyUnitIndex].slaughter && myUnits[myUnitIndex].lightArmor)
                            {
                                enemyAddDamage += slaughterValue;
                            }

                            //대기병
                            if (myUnits[myUnitIndex].branchIdx == 5)
                            {
                                enemyAddDamage += enemyUnits[enemyUnitIndex].antiCavalry;
                            }

                            //관통
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

                        //유닛 사망시 일단 새로운 페이즈
                        if (enemyUnits[enemyUnitIndex].health <= 0 || myUnits[myUnitIndex].health <= 0)
                        {
                            //나의 유격
                            if (myUnits[myUnitIndex].guerrilla && myUnitMax - myUnitIndex > 1 && enemyUnits[enemyUnitIndex].health <= 0)
                            {
                                UnitDataBase thisUnit = myUnits[myUnitIndex];
                                myUnits[myUnitIndex] = myUnits[myUnitIndex + 1];
                                myUnits[myUnitIndex + 1] = thisUnit;
                            }
                            //나의 착취
                            if (myUnits[myUnitIndex].drain && enemyUnits[enemyUnitIndex].health <= 0 && myUnits[myUnitIndex].health > 0)
                            {
                                myUnits[myUnitIndex].health = drainHealValue + myUnits[myUnitIndex].health > myUnitMaxHp ? myUnitMaxHp : drainHealValue + myUnits[myUnitIndex].health;
                            }
                            //상대 유격
                            if (enemyUnits[enemyUnitIndex].guerrilla && enemyUnitMax - enemyUnitIndex > 1 && myUnits[myUnitIndex].health <= 0)
                            {
                                UnitDataBase thisUnit = enemyUnits[enemyUnitIndex];
                                enemyUnits[myUnitIndex] = enemyUnits[enemyUnitIndex + 1];
                                enemyUnits[enemyUnitIndex + 1] = thisUnit;
                            }
                            //상대 착취
                            if (enemyUnits[enemyUnitIndex].drain && myUnits[myUnitIndex].health <= 0 && enemyUnits[enemyUnitIndex].health > 0)
                            {
                                enemyUnits[enemyUnitIndex].health = drainHealValue + enemyUnits[enemyUnitIndex].health > enemyUnitMaxHp ? enemyUnitMaxHp : drainHealValue + enemyUnits[enemyUnitIndex].health;
                            }
                            break;
                        }

                        continue;
                    case 2:         //지원
                        //내 유닛 지원
                        //원거리 공격
                        for (int i = myRangeUnits[0]; i < myRangeUnits.Count; i++)
                        {
                            if (i > myUnitIndex && myUnits[i].range - (i - myUnitIndex) >= 1)
                            {
                                if (!CalculateAccuracy(enemyUnits[enemyUnitIndex], myUnits[myRangeUnits[i]].perfectAccuracy))    //상대 회피 실패 시
                                {
                                    //중갑
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
                        //상대 유닛 지원
                        //원거리 공격
                        for (int i = enemyRangeUnits[0]; i < enemyRangeUnits.Count; i++)
                        {
                            if (i > enemyUnitIndex && enemyUnits[i].range - (i - enemyUnitIndex) >= 1)
                            {
                                if (!CalculateAccuracy(myUnits[myUnitIndex], enemyUnits[enemyRangeUnits[i]].perfectAccuracy))    //상대 회피 실패 시
                                {
                                    //중갑
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

                        //유닛 사망시 
                        if (enemyUnits[enemyUnitIndex].health <= 0 || myUnits[myUnitIndex].health <= 0)
                        {
                            //나의 유격
                            if (myUnits[myUnitIndex].guerrilla && myUnitMax - myUnitIndex > 1 && enemyUnits[enemyUnitIndex].health <= 0)
                            {
                                UnitDataBase thisUnit = myUnits[myUnitIndex];
                                myUnits[myUnitIndex] = myUnits[myUnitIndex + 1];
                                myUnits[myUnitIndex + 1] = thisUnit;
                            }
                            //나의 착취
                            if (myUnits[myUnitIndex].drain && enemyUnits[enemyUnitIndex].health <= 0 && myUnits[myUnitIndex].health > 0)
                            {
                                myUnits[myUnitIndex].health = drainHealValue + myUnits[myUnitIndex].health > myUnitMaxHp ? myUnitMaxHp : drainHealValue + myUnits[myUnitIndex].health;
                            }
                            //상대 유격
                            if (enemyUnits[enemyUnitIndex].guerrilla && enemyUnitMax - enemyUnitIndex > 1 && myUnits[myUnitIndex].health <= 0)
                            {
                                UnitDataBase thisUnit = enemyUnits[enemyUnitIndex];
                                enemyUnits[myUnitIndex] = enemyUnits[enemyUnitIndex + 1];
                                enemyUnits[enemyUnitIndex + 1] = thisUnit;
                            }
                            //상대 착취
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

            // 유닛의 체력이 0 이하일 경우, 다음 유닛으로 넘어감
            if (myUnits[myUnitIndex].health <= 0) //내 유닛 사망
            {
                Debug.Log("내 유닛 " + myUnits[myUnitIndex].unitName + "사망");

                myUnitIndex++;  // 다음 내 유닛
                isFirstAttack = true;
                if(myUnitIndex!= myUnitMax)
                {
                    myUnitMaxHp = myUnits[myUnitIndex].health;
                    isMyHeavyArmor = CalculateHeavyArmor(myUnits[myUnitIndex].heavyArmor, enemyUnits[enemyUnitIndex].branchIdx);
                    isMyBluntWeapon = CalculateBluntWeapon(myUnits[myUnitIndex].bluntWeapon, enemyUnits[enemyUnitIndex].heavyArmor);


                }
                UpdateUnitCount(myUnitMax - myUnitIndex, enemyUnitMax - enemyUnitIndex);
                
            }
            if (enemyUnits[enemyUnitIndex].health <= 0) //적 유닛 사망
            {
                Debug.Log("적 유닛 " + enemyUnits[enemyUnitIndex].unitName + "사망");

                enemyUnitIndex++;  // 다음 적 유닛
                isFirstAttack = true;

                if (enemyUnitIndex != enemyUnitMax)
                {
                    enemyUnitMaxHp = enemyUnits[enemyUnitIndex].health;
                    isMyHeavyArmor = CalculateHeavyArmor(enemyUnits[enemyUnitIndex].heavyArmor, myUnits[myUnitIndex].branchIdx);
                    isEnemyBluntWeapon = CalculateBluntWeapon( enemyUnits[enemyUnitIndex].bluntWeapon, myUnits[myUnitIndex].heavyArmor);

                }
                UpdateUnitCount(myUnitMax - myUnitIndex, enemyUnitMax - enemyUnitIndex);
                
            }


            //유닛 체력 UI관련
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

            //턴 종료
            fightPhase = 0;
            await WaitForSecondsAsync(0.5f);

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


    //자동전투를 다른코드에서 호출할수 있게끔 대신 호출해주는 함수
    public async Task<int> StartBattle(int[] _myUnitIds, int[] _enemyUnitIds)
    {
        if (autoBattleUI == null)
        {
            autoBattleUI = FindObjectOfType<AutoBattleUI>();
        }
        int result = await AutoBattle(_myUnitIds,_enemyUnitIds);
        return result;
    }

    //유닛 수 UI 초기화 함수
    private void UpdateUnitCount(int myUnitLength,int enemyUnitLength)
    {
          autoBattleUI.UpdateUnitCountUI(myUnitLength, enemyUnitLength);
    }


    //유닛 Hp UI 초기화 함수
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


    //기다리는 함수
    private async Task WaitForSecondsAsync(float seconds)
    {
        await Task.Delay((int)(seconds * 1000)); // 밀리초 단위
    }

    //유닛 이름 갱신
    private void UpdateUnitName(string myUnitName, string enemyUnitName)
    {
        autoBattleUI.UpdateName(myUnitName, enemyUnitName);
    }

    //회피율 계산
    private float CalculateDodge(UnitDataBase unit)
    {
        float dodge = 0.0f;
        dodge = (2 + (13 / 9) * (unit.mobility - 1)) +(unit.agility?10.0f:0);

        return dodge;
    }

    //회피 계산
    private bool CalculateAccuracy(UnitDataBase unit,bool isPerfectAccuracy)
    {
        if (!isPerfectAccuracy)
        {
            return CalculateDodge(unit) >= Random.Range(1, 101);
        }
        return false;
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

    

    //돌격 계산
    private float CalculateCharge(bool isCharge,float mobility)
    {
        float result=1.0f;
        if (isCharge)
        {
            return result = 1.1f + (1.9f / (9 * (mobility - 1)));
        }

        return result;
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
        
        return minHealthNumber;
    }

    //원거리 계산
    private bool RangeAttack(UnitDataBase offendingUnit,UnitDataBase deffendingUnit)
    {
        if (!CalculateAccuracy(deffendingUnit, offendingUnit.perfectAccuracy))   //상대 회피 실패 시
        {
            return true;
        }
        return false;
    }
    
}
