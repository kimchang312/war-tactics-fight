using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Random = UnityEngine.Random;
using System.Linq;
using System.IO;
using Unity.VisualScripting;


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

    //유닛 전투 통계
    private Dictionary<int, UnitCombatStatics> unitStats = new();

    //보유 유물
    private List<WarRelic> ownedRelics = new List<WarRelic>();

    //유닛별 고유 id
    private static int globalUnitId = 0;

    // 유닛 순서
    int myUnitIndex;
    int enemyUnitIndex;

    //유닛 총 갯수
    int myUnitMax;
    int enemyUnitMax;

    //사망 유닛 위치
    int myDeathUnitIndex;
    int enemyDeathUnitIndex =0;

    //후열 유닛 체력 (암살에 사용)
    float myBackUnitHp;
    float enemyBackUnitHp;

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

    private bool curseBlock = false;

    //데미지 배율
    private float myFinalDamage = 0;
    private float enemyFinalDamage = 0;

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
                    //고유 id 추가
                    unit.UniqueId = GenerateUniqueUnitId(unit.branchIdx, true,unit.idx);
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
                    //고유 id 추가
                    unit.UniqueId = GenerateUniqueUnitId(unit.branchIdx, false, unit.idx);

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
        //범위 확인
        int myCurrentIndex = myUnitIndex >= myUnitMax ? myUnitMax - 1 : myUnitIndex;
        int enemyCurrentIndex = enemyUnitIndex >= enemyUnitMax ? enemyUnitMax - 1 : enemyUnitIndex;
        float myUnitHp = myUnits[myCurrentIndex].health;
        float enemyHp = enemyUnits[enemyCurrentIndex].health;
        float myMAxHp = myUnits[myCurrentIndex].maxHealth;
        float enemyMaxHp = enemyUnits[enemyCurrentIndex].maxHealth;
       
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

    //회피율 계산
    private float CalculateDodge(UnitDataBase unit)
    {
        float dodge ;
        dodge = (2 + (13 / 9) * (unit.mobility - 1)) +(unit.agility?10.0f:0);

        return Mathf.Clamp(dodge, 0, 100);
    }

    //회피 유무 계산
    private bool CalculateAccuracy(UnitDataBase unit,bool isPerfectAccuracy)
    {
        if (isPerfectAccuracy)
            return false; // 필중 특성인 경우 회피 불가

        float dogeRate= CalculateDodge(unit);

        bool result= dogeRate > Random.Range(0, 100);

        return result;
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
    private void CallDamageText(float damage,string text,bool team,int unitIndex=0)
    {
        autoBattleUI.ShowDamage(MathF.Floor( damage), text, team,unitIndex);
    }

    // 유닛 생성UI 호출
    private void CallCreateUnit()
    {
        List<UnitDataBase> myRangeUnits=new();
        List<UnitDataBase> enemyRangUnits=new();
        List<UnitDataBase> myAliveUnits = new();
        List<UnitDataBase> enemyAliveUnits = new();
        //범위 확인
        int myCurrentIndex = myUnitIndex >= myUnitMax ? myUnitMax - 1 : myUnitIndex;
        int enemyCurrentIndex = enemyUnitIndex >= enemyUnitMax ? enemyUnitMax - 1 : enemyUnitIndex;
        //원거리 공격이 가능한지 검사
        for (int i = myCurrentIndex + 1; i < myUnits.Count; i++)
        {
            if (myUnits[i].rangedAttack && (myUnits[i].range - (i - myCurrentIndex) > 0) && myUnits[i].health>0)
            {
                myRangeUnits.Add(myUnits[i]);
            }
        }
        for (int i = enemyCurrentIndex + 1; i < enemyUnits.Count; i++)
        {
            if (enemyUnits[i].rangedAttack && (enemyUnits[i].range - (i - enemyCurrentIndex) > 0) && enemyUnits[i].health > 0)
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
        if(myAliveUnits.Count == 0)
        {
            myAliveUnits.Add(myUnits.Last());
        }
        if(enemyAliveUnits.Count == 0)
        {
            enemyAliveUnits.Add(enemyUnits.Last());
        }


        autoBattleUI.CreateUnitBox(myAliveUnits, enemyAliveUnits, CalculateDodge(myUnits[myCurrentIndex]), CalculateDodge(enemyUnits[enemyCurrentIndex]),myRangeUnits,enemyRangUnits);
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

            //총 데미지
            float myAllDamage;
            float enemyAllDamage;

            //유산 추가 데미지
            float relicDamage=0;

            //암살 위치
            int myMinHealthIndex;
            int enemyMinHealthIndex;

            (myAllDamage,enemyMinHealthIndex) = ProcessPreparation(
                myUnits[myUnitIndex], ref enemyUnits, enemyUnitIndex, ref mySkills, ref enemyBackUnitHp, ref enemyDeathUnitIndex,myFinalDamage,true);
            (enemyAllDamage,myMinHealthIndex) = ProcessPreparation(
                enemyUnits[enemyUnitIndex],ref myUnits, myUnitIndex, ref enemySkills,ref myBackUnitHp,ref myDeathUnitIndex,enemyFinalDamage,false);

            if (enemyAllDamage > 0)
            {
                //유산 28
                if (HeartGemNecklace(myUnits[myUnitIndex].health)) 
                {
                    myUnits[myUnitIndex].health = myUnits[myUnitIndex].maxHealth;

                    CallDamageText(-myUnits[myUnitIndex].maxHealth, "회복", false);
                }

                //15
                relicDamage += ReactiveThornArmor(myUnits[myUnitIndex]);

                myAllDamage += relicDamage;

                //통계 기록: 입힌 피해, 받은 피해
                unitStats[enemyUnits[enemyUnitIndex].UniqueId].AddDamageDealt(enemyAllDamage);
                CallDamageText(enemyAllDamage, enemySkills, false, myMinHealthIndex);
            }

            if (myAllDamage > 0)
            {
                //통계 기록: 입힌 피해, 받은 피해
                unitStats[myUnits[myUnitIndex].UniqueId].AddDamageDealt(myAllDamage);
                CallDamageText(myAllDamage, mySkills, true,enemyMinHealthIndex);
                
            }
        }
    }

    //충돌 페이즈
    private void ChrashPhase()
    {
        float myDamage;
        float enemyDamage;

        string mySkills ="충돌 ";
        string enemySkills ="충돌 ";

        float relicDamage = 0;

        //내 유닛 공격
        myDamage =
             ProcessChrash(myUnits[myUnitIndex],ref enemyUnits,enemyUnitIndex , ref mySkills, ref enemySkills,myFinalDamage);
        //상대 유닛 공격
        enemyDamage =
             ProcessChrash(enemyUnits[enemyUnitIndex],ref myUnits,myUnitIndex, ref enemySkills, ref mySkills,enemyFinalDamage);

        //유산 30
        myDamage = BrokenStraightSword(myDamage);

        if (enemyDamage > 0)
        {
            //유산 28
            if (HeartGemNecklace(myUnits[myUnitIndex].health)) myUnits[myUnitIndex].health = myUnits[myUnitIndex].maxHealth;

            //15
            relicDamage += ReactiveThornArmor(myUnits[myUnitIndex]);

            myDamage += relicDamage;
        }

        //통계 기록: 입힌 피해, 받은 피해, 전투 충돌
        unitStats[myUnits[myUnitIndex].UniqueId].AddDamageDealt(myDamage);
        unitStats[myUnits[myUnitIndex].UniqueId].AddDamageTaken(enemyDamage);
        unitStats[enemyUnits[enemyUnitIndex].UniqueId].AddDamageDealt(enemyDamage);
        unitStats[enemyUnits[enemyUnitIndex].UniqueId].AddDamageTaken(myDamage);
        unitStats[myUnits[myUnitIndex].UniqueId].IncrementCombatEncounters();
        unitStats[enemyUnits[enemyUnitIndex].UniqueId].IncrementCombatEncounters();

        //
        CallDamageText(myDamage, mySkills, true);
        //
        CallDamageText(enemyDamage, enemySkills, false);
    }

    //지원 페이즈
    private void SupportPhase()
    {
        float myDamage = 0;
        float enemyDamage = 0;

        float relicDamage = 0;
        
        //상대 공격
        enemyDamage=ProcessRangeAttack(enemyUnits, ref myUnits, enemyRangeUnits, enemyUnitIndex, myUnitIndex, false,enemyFinalDamage);
        if (enemyDamage > 0)
        {
            myUnits[myUnitIndex].health -= enemyDamage;
            
            //유산 28
            if(HeartGemNecklace(myUnits[myUnitIndex].health)) myUnits[myUnitIndex].health = myUnits[myUnitIndex].maxHealth;

            CallDamageText(enemyDamage, "원거리 ", true);

            //통계 기록: 받은 피해
            unitStats[myUnits[myUnitIndex].UniqueId].AddDamageTaken(enemyDamage);

            //유산 작동
            //15
            relicDamage = ReactiveThornArmor(myUnits[myUnitIndex]);
            myDamage += relicDamage;
        }

        //내 공격
        myDamage = ProcessRangeAttack(myUnits, ref enemyUnits, myRangeUnits, myUnitIndex, enemyUnitIndex, true, myFinalDamage);
        if (myDamage > 0)
        {
            enemyUnits[enemyUnitIndex].health -= myDamage;

            CallDamageText(myDamage, "원거리 ", true);

            //통계 기록: 받은 피해
            unitStats[enemyUnits[enemyUnitIndex].UniqueId].AddDamageTaken(myDamage);
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
            ProcessDeath(ref myUnits,ref enemyUnits, myUnitIndex, enemyUnitIndex, enemyBackUnitHp, enemyDeathUnitIndex, ref enemyAdd, true);
            ProcessDeath(ref enemyUnits,ref myUnits, enemyUnitIndex,myUnitIndex,myBackUnitHp,myDeathUnitIndex,ref myAdd, false);
            // 암살+유격으로 죽은게 아닌 경우 == 다음 유닛으로 넘어감
            if (myBackUnitHp > 0 && enemyBackUnitHp > 0)
            {
                myUnitIndex += myAdd;
                enemyUnitIndex += enemyAdd;
            }
            if (myAdd > 0 && Relic55())
            {
                enemyUnits[enemyUnitIndex].health -= 9;

                ManageUnitDeath();
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

        /*
        // 첫 번째 유닛으로 군단병 추가
        UnitDataBase legionnaire = TestUnit.CreateLegionnaire();
        legionnaire.UniqueId = GenerateUniqueUnitId(legionnaire.branchIdx, true, legionnaire.idx);
        // 첫 번째 유닛으로 추가
        myUnits.Insert(0, legionnaire);
        //결속 계산
        CalculataeSolidarity(ref myUnits);
        CalculataeSolidarity(ref enemyUnits);
        */

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

        //배율 초기화
        RogueLikeData.Instance.ResetFinalDamage();

        //유산 저장
        GetRelicData();
        CheckFusion();
        // 아군, 적군 데이터를 RogueLikeData 싱글톤에 저장
        RogueLikeData.Instance.AllMyUnits(myUnits);
        RogueLikeData.Instance.AllEnemyUnits(enemyUnits);

        //예시 유물
        //RogueLikeData.Instance.AcquireRelic(12);

        //해주 유산 확인
        curseBlock = RogueLikeData.Instance.GetOwnedRelicById(23) == null ? false : true;

        //스탯 유산 적용
        RunStateRelic();

        myFinalDamage = RogueLikeData.Instance.GetMyMultipleDamage();
        enemyFinalDamage = RogueLikeData.Instance.GetEnemyMultipleDamage();

        myUnits = RogueLikeData.Instance.GetMyUnits();


        // 통계 초기화
        foreach (var unit in myUnits)
        {
            if (!unitStats.ContainsKey(unit.UniqueId))
            {
                unitStats[unit.UniqueId] = new UnitCombatStatics(unit.UniqueId, unit.unitName); // 아군
            }
        }

        foreach (var unit in enemyUnits)
        {
            if (!unitStats.ContainsKey(unit.UniqueId))
            {
                unitStats[unit.UniqueId] = new UnitCombatStatics(unit.UniqueId, unit.unitName); // 적군
            }
        }

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

    //종료관리 전투가 끝났을때 나오게 될것들
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

            //종료 시 UI 표기
            autoBattleUI.FightEnd(result);

            //점수 표기
            ManageScore(result);

            // 전투 통계 저장
            SaveCombatStatistics();

            //통계 표시 
            ViewStatics();

            //ui마지막 업데이트
            //유닛 숫자 UI 최신화
            UpdateUnitCount();

            //유닛 체력 UI 최신화
            UpdateUnitHp();

            return true;
        }
        return false;
    }

    //준비 계산
    private (float,int) ProcessPreparation(UnitDataBase attacker,ref List<UnitDataBase> defenders,int defenderIndex, ref string attackerSkill, ref float backUnitHp,ref int deathUnitIndex,float finalDamage,bool isTeam)
    {
        float allDamage = 0;
        int minHealthIndex = 0;
        //위압
        if (attacker.overwhelm)
        {
            attackerSkill += "위압 ";
            defenders[defenderIndex].mobility =overwhelmValue;

            //통계 기록: 위압 스킬 사용
            unitStats[attacker.UniqueId].IncrementSkillUsage("위압");
        }
        //투창
        if (attacker.throwSpear)
        {
            attackerSkill += "투창 ";

            float damage = throwSpearValue*finalDamage;

            //유산 작동
            if (isTeam)
            {
                damage = BrokenStraightSword(damage);
                damage *= TechnicalManual() ? 9 : 1;
            }

            //통계 기록: 투창 발동
            unitStats[attacker.UniqueId].IncrementSkillUsage("투창");
            //회피 계산 
            if (!CalculateAccuracy(defenders[defenderIndex], attacker.perfectAccuracy))
            {
                allDamage += damage;
                defenders[defenderIndex].health -=damage;

                //통계 기록: 투창 피해, 받은 피해
                unitStats[attacker.UniqueId].AddDamageBySkill("투창", damage);
                unitStats[defenders[defenderIndex].UniqueId].AddDamageTaken(damage);
            }
            else
            {
                attackerSkill = "회피 ";

                //통계 기록: 회피, 회피한 피해
                unitStats[defenders[defenderIndex].UniqueId].IncrementDodges();
                unitStats[defenders[defenderIndex].UniqueId].AddDodgedDamage(damage);

            }
        }
        //암살 && 방어자 후열 유닛 존재
        if(attacker.assassination && CheckAssassination(defenders, defenderIndex))
        {
            //Debug.Log("암살");

            attackerSkill += "암살 ";
            float damage = attacker.attackDamage * assassinationValue*finalDamage;

            //유산 작동
            if (isTeam)
            {
                damage = BrokenStraightSword(damage);
                damage *= TechnicalManual() ? 9 : 1;
            }

            // 통계 기록: 암살 스킬 사용
            unitStats[attacker.UniqueId].IncrementSkillUsage("암살");
            if (defenders[defenderIndex].guard)
            {
                attackerSkill += "수호 ";

                // 통계 기록: 수호 스킬 사용
                unitStats[defenders[defenderIndex].UniqueId].IncrementSkillUsage("수호");
                if (!CalculateAccuracy(defenders[defenderIndex], attacker.perfectAccuracy))
                {
                    float actualDamage = damage;

                    //유산 33
                    if (!SplitShield())
                    {
                        actualDamage = damage * (1 - (defenders[defenderIndex].armor) / (10 + defenders[defenderIndex].armor));
                    }

                    defenders[defenderIndex].health -= actualDamage;

                    allDamage += actualDamage;

                    // 통계 기록: 암살 피해, 장갑 감소 피해, 받은 피해
                    unitStats[attacker.UniqueId].AddDamageBySkill("암살", actualDamage);
                    unitStats[defenders[defenderIndex].UniqueId].AddArmorDamageReduced(actualDamage);
                    unitStats[defenders[defenderIndex].UniqueId].AddDamageTaken(damage-actualDamage);
                }
                else
                {
                    //통계 기록: 회피, 회피한 피해
                    unitStats[defenders[defenderIndex].UniqueId].IncrementDodges();
                    unitStats[defenders[defenderIndex].UniqueId].AddDodgedDamage(damage);

                    attackerSkill += "회피 ";
                }
            }
            else
            {
                //상대 후열 유닛 체력 저장
                List<float> defendersHealth = new();
                for (int i = enemyUnitIndex + 1; i < enemyUnitMax; i++)
                {
                    defendersHealth.Add(defenders[i].health);
                }

                minHealthIndex = defenderIndex + CalculateAssassination(defendersHealth);

                backUnitHp = defenders[minHealthIndex].health;
                backUnitHp -= damage;
                defenders[minHealthIndex].health = backUnitHp;

                //최종 피해량 추가
                allDamage += damage;

                //통계 기록: 암살 피해, 받은 피해
                unitStats[attacker.UniqueId].AddDamageBySkill("암살", damage);
                unitStats[defenders[minHealthIndex].UniqueId].AddDamageTaken(damage);

                int deadUnitCount = 0;
                for (int i = defenderIndex + 1; i < minHealthIndex; i++)
                {
                    if (defenders[i].health <= 0)
                    {
                        deadUnitCount++;
                    }
                }
                minHealthIndex = minHealthIndex - defenderIndex - deadUnitCount;
                //후열 유닛 사망시 대입
                if (backUnitHp <= 0)
                {
                    deathUnitIndex = minHealthIndex;

                    //통계 기록: 암살로 처치
                    unitStats[attacker.UniqueId].IncrementAssassinations();
                }
            }
        }

        return (allDamage,minHealthIndex);
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

                    //통계 기록: 강한돌격 발동
                    unitStats[attacker.UniqueId].IncrementTraitUsage("강한돌격");
                }
                else
                {
                    attackerSkill += "돌격 ";

                    //통계 기록: 돌격 발동
                    unitStats[attacker.UniqueId].IncrementTraitUsage("돌격");
                }
                //수비태세 = 방어자가 수배태세 일때
                //공격자 데미지 감소
                if (defender.defense)
                {
                    reduceDamage += defenseValue;
                    attackerSkill += "수비태세 ";
                    defenderSkill += "수비태세 ";

                    //통계 기록: 돌격 발동
                    unitStats[defender.UniqueId].IncrementTraitUsage("수비태세");
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
    private float ProcessChrash(UnitDataBase attacker,ref List<UnitDataBase> defenders,int defenderIndex, ref string attackerSkill,ref string defenderSkill,float finalDamage)
    {
        float multiplier;
        float reduceDamage;
        float damage;
        float actualDamage;

        (multiplier, reduceDamage) = CalculateChrashIsFirstAttack(attacker, defenders[defenderIndex], ref attackerSkill,ref defenderSkill);

        float bonusDamage = 0;

        //둔기
        if (attacker.bluntWeapon && defenders[defenderIndex].heavyArmor)
        {
            bonusDamage += bluntWeaponValue;
            attackerSkill += "둔기 ";

            //통계 기록: 둔기 발동
            unitStats[attacker.UniqueId].IncrementTraitUsage("둔기");
        }
        //도살
        if (attacker.slaughter && defenders[defenderIndex].lightArmor)
        {
            bonusDamage += slaughterValue;
            attackerSkill += "도살 ";

            //통계 기록: 도살 발동
            unitStats[attacker.UniqueId].IncrementTraitUsage("도살");
        }
        //대기병
        if (defenders[defenderIndex].branchIdx == 5 || defenders[defenderIndex].branchIdx == 6)
        {
            bonusDamage += attacker.antiCavalry;
            attackerSkill += "대기병 ";

            //통계 기록: 대기병 발동, 피해
            unitStats[attacker.UniqueId].IncrementTraitUsage("대기병");
            unitStats[attacker.UniqueId].AddAdditionalDamageByTrait("대기병", attacker.antiCavalry);
        }

        damage = attacker.attackDamage * multiplier;
        actualDamage = damage+ bonusDamage - reduceDamage;
        
        //관통
        if (attacker.perfectAccuracy)
        {
            attackerSkill += "관통 ";

            //통계 기록: 관통 발동, 관통 추가 피해
            unitStats[attacker.UniqueId].IncrementTraitUsage("관통");
            unitStats[attacker.UniqueId].AddPierceDamageGained(damage - (damage * (1 - (defenders[defenderIndex].armor) / (10 + defenders[defenderIndex].armor))));
        }
        else
        {
            //유산 33
            if (!SplitShield())
            {
                //장갑 계산
                damage = attacker.attackDamage * (1 - (defenders[defenderIndex].armor) / (10 + defenders[defenderIndex].armor));
            }
        }

        damage = (damage+ bonusDamage - reduceDamage)*finalDamage;

        //방어자의 회피 계산
        if (CalculateAccuracy(defenders[defenderIndex], attacker.perfectAccuracy))
        {
            attackerSkill = "충돌 회피 ";

            //통계 기록: 회피 발동, 회피한 피해
            unitStats[defenders[defenderIndex].UniqueId].IncrementDodges();
            unitStats[defenders[defenderIndex].UniqueId].AddDodgedDamage(actualDamage);

            damage = 0;
        }
        else
        {
            defenders[defenderIndex].health -= damage;

            //통계 기록: 장갑 감소 피해
            unitStats[defenders[defenderIndex].UniqueId].AddArmorDamageReduced(actualDamage-damage);
        }

        return damage;
    }

    //원거리 공격
    private float ProcessRangeAttack(List<UnitDataBase> attackers,ref List<UnitDataBase> defenders,List<int> rangeUnits,int attackerIndex,int defenderIndex,bool isTeam,float finalDamage)
    {
        float allDamage = 0;
        if (rangeUnits.Count > 0)
        {
            for (int i = 0; i < rangeUnits.Count; i++)
            {
                if(rangeUnits[i] > attackerIndex && attackers[rangeUnits[i]].range - (rangeUnits[i] - attackerIndex) >= 1 && attackers[rangeUnits[i]].health > 0)
                {
                    
                    float damage = attackers[rangeUnits[i]].attackDamage*finalDamage;

                    //팔랑크스가 발동 되었을 때 (임의: 데미지 10%감소)
                    if (ownedRelics.FirstOrDefault(relic => relic.id == 18).used)
                    {
                        damage *= 0.9f;
                    }
                    //유산 30
                    if (isTeam)
                    {
                        damage = BrokenStraightSword(damage);
                    }

                    if (!CalculateAccuracy(defenders[defenderIndex], attackers[rangeUnits[i]].perfectAccuracy))    //상대 회피 실패
                    {
                        //공격받는 유닛(상대) 중갑 유무
                        if (defenders[defenderIndex].heavyArmor)
                        {
                            if (attackers[rangeUnits[i]].attackDamage > heavyArmorValue)
                            {
                                damage -= heavyArmorValue;
                                allDamage += damage;
                            }
                            //통계 기록: 중갑 발동
                            unitStats[defenders[defenderIndex].UniqueId].IncrementTraitUsage("중갑");
                        }
                        else
                        {
                            allDamage += damage;
                        }
                        //통계 기록: 입힌 피해, 원거리 피해
                        unitStats[attackers[rangeUnits[i]].UniqueId].AddDamageDealt(damage);
                        unitStats[attackers[rangeUnits[i]].UniqueId].AddRangedDamageDealt(damage);
                    }
                    else
                    {
                        //통계 기록: 회피, 회피한 피해
                        unitStats[defenders[defenderIndex].UniqueId].IncrementDodges();
                        unitStats[defenders[defenderIndex].UniqueId].AddDodgedDamage(damage);
                    }

                }
            }

        }
        return allDamage;
    }


    //사망 처리
    private void ProcessDeath(ref List<UnitDataBase> attackers,ref List<UnitDataBase> defenders,int attackerIndex,int defenderIndex,float backUnitHp,int deathUnitIndex,ref int unitAdd,bool isTeam )
    {
        if (defenders[defenderIndex].health <=0 || backUnitHp <= 0)
        {
            //통계 기록: 처치
            unitStats[attackers[attackerIndex].UniqueId].IncrementKills();

            //유격
            if (attackers[attackerIndex].guerrilla && attackers[attackerIndex].health > 0 && attackerIndex+1 < attackers.Count)
            {
                //Debug.Log("유격");

                CallDamageText(0, "유격 ", !isTeam);

                //통계 기록: 유격 발동
                unitStats[attackers[attackerIndex].UniqueId].IncrementSkillUsage("유격");

                (attackers[attackerIndex + 1], attackers[attackerIndex]) = (attackers[attackerIndex],attackers[attackerIndex+1]);
            }
            //착취
            if (attackers[attackerIndex].drain && attackers[attackerIndex].health > 0)
            {
                //Debug.Log("착취");

                CallDamageText(0, "착취 ", !isTeam);

                attackers[attackerIndex].health = MathF.Min(attackers[attackerIndex].maxHealth, drainHealValue + attackers[attackerIndex].health);
                attackers[attackerIndex].attackDamage += drainGainAttackValue;

                //통계 기록: 착취 발동
                unitStats[attackers[attackerIndex].UniqueId].IncrementSkillUsage("착취");
            }
            if (defenders[defenderIndex].health <= 0)
            {
                defenders[defenderIndex].alive=false;

            }
            if (backUnitHp <= 0)
            {
                defenders[deathUnitIndex].alive=false;

            }

            ChangeInvisibile(!isTeam, deathUnitIndex);
            unitAdd++;
        }

    } 

    //점수 관리
    private void ManageScore(int result)
    {
        float score = 0;

        //상대 사망 유닛 계산
        foreach (UnitDataBase enemy in enemyUnits)
        {
            if(enemy.health <= 0)
            {
                score += enemy.unitPrice;
            }
        }
        if(result ==0)
        {
            //아군 생존 유닛
            foreach (UnitDataBase my in myUnits)
            {
                if (my.health > 0)
                {
                    float healthRatio = my.health / my.maxHealth;
                    score += (healthRatio * my.unitPrice) * 0.5f;
                }
            }

            //남은 금액 덧셈
            score += PlayerData.currency;
        }

        autoBattleUI.UpdateScore((int)score);
    }

    // 유닛 별 고유 ID 생성
    private int GenerateUniqueUnitId(int branchIdx, bool isTeam, int unitIdx)
    {
        int teamBit = isTeam ? 0 : 1; // 팀 정보
        return (teamBit << 31)             // 팀 비트 (31번째 비트)
             | (branchIdx << 21)          // 병종 정보 (21~30번째 비트)
             | ((globalUnitId++ & 0x7FF) << 10) // 글로벌 ID (10~20번째 비트, 11비트 제한)
             | (unitIdx & 0x3FF);         // 유닛 고유 번호 (0~9번째 비트, 10비트 제한)
    }


    // 전투 통계 저장
    private void SaveCombatStatistics()
    {
        // 현재 시간 기반 파일 이름 생성
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"); // 형식: 2023-12-31_23-59-59
        // 지정된 경로 설정
        string directoryPath = @"C:\Users\admin\Desktop\BattleStatics";
        string filePath = Path.Combine(directoryPath, $"BattleStatics.json_{timestamp}");

        // 폴더가 없으면 저장 작업 중단
        if (!Directory.Exists(directoryPath))
        {
            Debug.LogWarning($"전투 통계 저장 실패: 폴더가 존재하지 않습니다. ({directoryPath})");
            return;
        }

        // 통계 저장
        CombatStatisticsManager statsManager = new CombatStatisticsManager(unitStats);
        statsManager.SaveStatisticsToFile(filePath);

        Debug.Log($"전투 통계가 저장되었습니다: {filePath}");
    }

    //전투 종료 통계 표시
    private void ViewStatics()
    {
        // Dictionary -> List 변환
        List<UnitCombatStatics> unitStatsList = unitStats.Values.ToList();

        UnitCombatStatics myMostDam = UnitCombatStatics.GetAllyWithHighestDamageDealt(unitStatsList);
        UnitCombatStatics myMostTaken = UnitCombatStatics.GetAllyWithHighestDamageTaken(unitStatsList);
        UnitCombatStatics enemyMostDam = UnitCombatStatics.GetEnemyWithHighestDamageDealt(unitStatsList);
        UnitCombatStatics enemyMostTaken = UnitCombatStatics.GetEnemyWithHighestDamageTaken(unitStatsList);

        //num 0
        autoBattleUI.ViewStatics(
            $"아군 피해량의 {(myMostDam.TotalDamageDealt/UnitCombatStatics.GetTotalDamageDealt(unitStatsList,true))*100}%",
            UnitCombatStatics.FilterHighDamage(myMostDam), 0,(myMostDam.UnitID) & 0x3FF);
        //num 1
        autoBattleUI.ViewStatics(
            $"아군 받은 피해량의 {myMostTaken.TotalDamageTaken/(UnitCombatStatics.GetTotalDamageTaken(unitStatsList, false))*100}%",
            UnitCombatStatics.FilterHighTaken(myMostTaken), 1, (myMostTaken.UnitID) & 0x3FF);
        //num 2
        autoBattleUI.ViewStatics(
            $"적군 피해량의 {enemyMostDam.TotalDamageDealt/(UnitCombatStatics.GetTotalDamageDealt(unitStatsList, true))*100}%",
            UnitCombatStatics.FilterHighDamage(enemyMostDam), 2, (enemyMostDam.UnitID) & 0x3FF);
        //num 3
        autoBattleUI.ViewStatics(
            $"적군 받은 피해량의 {enemyMostTaken.TotalDamageTaken/(UnitCombatStatics.GetTotalDamageTaken(unitStatsList, false))*100}%",
            UnitCombatStatics.FilterHighTaken(enemyMostTaken), 3, (enemyMostTaken.UnitID) & 0x3FF);
    }

    //결속 발동
    private void CalculataeSolidarity(ref List<UnitDataBase> units)
    {
        foreach (var unit in units)
        {
            if (unit.solidarity) // 결속이 활성화된 유닛만 체크
            {
                foreach (var checkUnit in units)
                {
                    // checkUnit.unitTag와 unit.unitTag에 공통된 태그가 하나라도 있으면 공격력 +5
                    if (unit.unitTag.Any(tag => checkUnit.unitTag.Contains(tag)))
                    {
                        unit.attackDamage += 5;
                    }
                }
            }
        }
    }

    //유물 저장
    private void GetRelicData()
    {
        ownedRelics = RogueLikeData.Instance.GetRelicsByType(RelicType.AllEffect);
        ownedRelics.AddRange(RogueLikeData.Instance.GetRelicsByType(RelicType.SpecialEffect));
        ownedRelics.AddRange(RogueLikeData.Instance.GetRelicsByType(RelicType.StateBoost));
        ownedRelics.AddRange(RogueLikeData.Instance.GetRelicsByType(RelicType.BattleActive));
        ownedRelics.AddRange(RogueLikeData.Instance.GetRelicsByType(RelicType.ActiveState));
    }

    // 보유한 유물 중 ID 24, 25, 26을 모두 가지고 있는지 확인하고, 있으면 유물 27 추가
    private void CheckFusion()
    {
        // 이미 유물 27을 보유하고 있으면 실행할 필요 없음
        if (ownedRelics.Any(relic => relic.id == 27)) return;

        HashSet<int> requiredRelicIds = new HashSet<int> { 24, 25, 26 };
        HashSet<int> ownedRelicIds = new HashSet<int>(ownedRelics.Select(relic => relic.id));

        // 모든 필수 유물(24, 25, 26)을 보유하고 있는지 확인
        if (requiredRelicIds.All(id => ownedRelicIds.Contains(id)))
        {
            // RogueLikeData에 유물 27 추가
            RogueLikeData.Instance.AcquireRelic(27);

            // 유물이 정상적으로 존재하는 경우만 추가
            ownedRelics.Add(WarRelicDatabase.GetRelicById(27));
        }
    }


    //스탯 유물 발동
    private void RunStateRelic()
    {
        List<WarRelic> stateRelics = ownedRelics.Where(relic => relic.type == RelicType.StateBoost || relic.type == RelicType.ActiveState).ToList();

        if (stateRelics.Count <= 0) return;

        foreach (var relic in stateRelics)
        {
            if(curseBlock && relic.grade==0)
                continue;

            relic.Execute();
        }
    }

    //전투 유물 발동
    private void RunBattleRelic()
    {

    }

    //유산 15 
    private float ReactiveThornArmor(UnitDataBase unit)
    {
        if(ownedRelics.FirstOrDefault(relic => relic.id == 15)==null) return 0;

        if (unit.heavyArmor)
        {
            Debug.Log("반응성의 가시 갑옷 발동");
            return (unit.armor * 9);
        }
        return 0;
    }

    //유산 28
    private bool HeartGemNecklace(float health)
    {
        if (health > 0) return false;
        var relic = ownedRelics.FirstOrDefault(relic => relic.id == 28);
        if (relic == null || !relic.used) return false;
        Debug.Log("하트 보석 목걸이 발동");
        relic.used = true;
        return true;
    }

    // 유산 30
    private float BrokenStraightSword(float damage)
    {
        if (ownedRelics.FirstOrDefault(relic=>relic.id==30) != null)
        {
            // 10% 확률 (0~99 중 0~9가 나올 경우)
            if (UnityEngine.Random.Range(0, 100) < 10)
            {
                Debug.Log("부러진 직검 발동");
                return damage * 0.9f; // 데미지 10% 감소
            }
        }

        return damage; // 원래 데미지 반환
    }

    // 유산 33
    private bool SplitShield()
    {
        if (ownedRelics.FirstOrDefault(relic => relic.id == 33) != null)
        {
            // 10% 확률 (0~99 중 0~9가 나올 경우)
            if (UnityEngine.Random.Range(0, 100) < 10)
            {
                Debug.Log("갈라진 방패 발동");
                return true; 
            }
        }

        return false;
    }

    //유산 35
    private void Relic35(ref List<UnitDataBase> units)
    {
        if (ownedRelics.FirstOrDefault(relic => relic.id == 35) == null) return;
        int onlyOne=0;
        int unitIndex = -1;
        for (int i=0;i< units.Count;i++)
        {
            if (units[i].health > 0)
            {
                onlyOne++;
                unitIndex = i;
                units[i].attackDamage += 9;
            }
        }
        if (onlyOne == 1)
        {
            units[unitIndex].attackDamage += 9;
            units[unitIndex].armor += 9;
            units[unitIndex].mobility += 9;
        }
    }

    //유산 47
    private bool TechnicalManual()
    {
        if (ownedRelics.FirstOrDefault(relic => relic.id == 47) == null) return false;
        return true;
    }

    //유산 55
    private bool Relic55()
    {
        if (ownedRelics.FirstOrDefault(relic => relic.id == 55) == null) return false;
        return true;
    }
}


