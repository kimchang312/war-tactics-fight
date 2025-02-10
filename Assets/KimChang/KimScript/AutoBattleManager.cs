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
    // 중복 방지를 위한 HashSet
    private HashSet<int> ownedRelicIds = new HashSet<int>();

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
        else    //유닛 사망 시 승패무 채크 후 준비 페이즈로
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
                if (RelicManager.HeartGemNecklace(myUnits[myUnitIndex].health, ownedRelics)) 
                {
                    myUnits[myUnitIndex].health = myUnits[myUnitIndex].maxHealth;

                    CallDamageText(-myUnits[myUnitIndex].maxHealth, "회복", false);
                }

                //15
                relicDamage += RelicManager.ReactiveThornArmor(myUnits[myUnitIndex], ownedRelics);

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
             ProcessChrash(myUnits[myUnitIndex],ref enemyUnits,enemyUnitIndex , ref mySkills, ref enemySkills,myFinalDamage,true);
        //상대 유닛 공격
        enemyDamage =
             ProcessChrash(enemyUnits[enemyUnitIndex],ref myUnits,myUnitIndex, ref enemySkills, ref mySkills,enemyFinalDamage,false);

        //유산 30
        //myDamage = RelicManager.BrokenStraightSword(myDamage, ownedRelics);

        if (enemyDamage > 0)
        {
            //유산 28
            if (RelicManager.HeartGemNecklace(myUnits[myUnitIndex].health, ownedRelics)) myUnits[myUnitIndex].health = myUnits[myUnitIndex].maxHealth;

            //15
            relicDamage += RelicManager.ReactiveThornArmor(myUnits[myUnitIndex], ownedRelics);

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
            if(RelicManager.HeartGemNecklace(myUnits[myUnitIndex].health, ownedRelics)) myUnits[myUnitIndex].health = myUnits[myUnitIndex].maxHealth;

            CallDamageText(enemyDamage, "원거리 ", false);

            //통계 기록: 받은 피해
            unitStats[myUnits[myUnitIndex].UniqueId].AddDamageTaken(enemyDamage);

            //유산 작동
            //15
            relicDamage = RelicManager.ReactiveThornArmor(myUnits[myUnitIndex], ownedRelics);
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
            int myAdd=0;
            int enemyAdd=0;
            bool isGuerrilla=false;
            ProcessDeath(ref myUnits,ref enemyUnits, myUnitIndex, enemyUnitIndex, enemyBackUnitHp, enemyDeathUnitIndex, ref enemyAdd, true,ref isGuerrilla);
            ProcessDeath(ref enemyUnits,ref myUnits, enemyUnitIndex,myUnitIndex,myBackUnitHp,myDeathUnitIndex,ref myAdd, false,ref isGuerrilla);
            // 암살+유격으로 죽은게 아닌 경우 == 다음 유닛으로 넘어감
            if (myBackUnitHp > 0 && enemyBackUnitHp > 0)
            {
                myUnitIndex += myAdd;
                enemyUnitIndex += enemyAdd;

                isFirstAttack = true;
            }
            else if (isGuerrilla)
            {
                isFirstAttack = true;
            }
            else
            {
                myDeathUnitIndex = 0;
                enemyDeathUnitIndex = 0;

                return false;
            }
            if (myAdd > 0 && RelicManager.Relic55(ownedRelics))
            {
                if (myBackUnitHp > 0)
                {
                    enemyUnits[enemyUnitIndex].health -= myUnits[myUnitIndex].health*0.1f;
                }
                else
                {
                    enemyUnits[enemyUnitIndex].health -= myUnits[myDeathUnitIndex].health * 0.1f;
                }

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

        //로딩창 시작
        autoBattleUI.ToggleLoadingWindow();

        //test 유산 추가
        //RogueLikeData.Instance.AcquireRelic(1);
        //RogueLikeData.Instance.AcquireRelic(27);
        
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

        //유산 및 데이터 저장
        ProcessRelic();

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

        //로딩창 종료
        autoBattleUI.ToggleLoadingWindow();

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
                Debug.Log($"나의 승리");
                return 0;  // 나의 승리
            }
            else if (enemyUnitIndex < enemyUnitMax && myUnitIndex >= myUnitMax)
            {
                Debug.Log($"나의 패배");
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

            //통계 표시 
            UnitCombatStatics.ProcessBattleStatics(unitStats,autoBattleUI);

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
                damage *= RelicManager.TechnicalManual(ownedRelics) ? 1.2f : 1;
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
                damage *= RelicManager.TechnicalManual(ownedRelics) ? 1.2f : 1;
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
                    float actualDamage = damage * (1 - defenders[defenderIndex].armor / (10 + defenders[defenderIndex].armor));

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
                }
            }
            if (attacker.defense)
            {
                if (defender.charge)
                {
                    reduceDamage -= defenseValue;
                }
                else
                {
                    reduceDamage += defenseValue;
                }
                attackerSkill += "수비태세";

                //통계 기록: 수비태세 발동
                unitStats[attacker.UniqueId].IncrementTraitUsage("수비태세");
            }
        }
        
        return (multiplier, reduceDamage);
    }

    //충돌 계산
    private float ProcessChrash(UnitDataBase attacker,ref List<UnitDataBase> defenders,int defenderIndex, ref string attackerSkill,ref string defenderSkill,float finalDamage,bool isTeam)
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
        if (attacker.antiCavalry>0 &&(defenders[defenderIndex].branchIdx == 5 || defenders[defenderIndex].branchIdx == 6))
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
        if (attacker.pierce)
        {
            attackerSkill += "관통 ";

            //통계 기록: 관통 발동, 관통 추가 피해
            unitStats[attacker.UniqueId].IncrementTraitUsage("관통");
            unitStats[attacker.UniqueId].AddPierceDamageGained(damage - (damage * (1 - (defenders[defenderIndex].armor) / (10 + defenders[defenderIndex].armor))));
        }
        else
        {
            //장갑 계산
            damage = attacker.attackDamage * (1 - (defenders[defenderIndex].armor) / (10 + defenders[defenderIndex].armor));
        }

        damage = (damage+ bonusDamage - reduceDamage)*finalDamage;

        //팔랑크스
        if (!isTeam && attacker.branchIdx == 2 && RelicManager.PhalanxTacticsBook(ownedRelics))
        {
            damage *= 0.5f;
        }

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

                    if (!isTeam)
                    {
                        //팔랑크스가 발동 되었을 때 ( 데미지 50%감소)
                        if (RelicManager.PhalanxTacticsBook(ownedRelics))
                        {
                            damage *= 0.5f;
                        }
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
    private void ProcessDeath(ref List<UnitDataBase> attackers,ref List<UnitDataBase> defenders,int attackerIndex,int defenderIndex,float backUnitHp,int deathUnitIndex,ref int unitAdd,bool isTeam ,ref bool isGuerrilla)
    {
        if (defenders[defenderIndex].health <=0 || backUnitHp <= 0)
        {
            //통계 기록: 처치
            unitStats[attackers[attackerIndex].UniqueId].IncrementKills();

            //유격
            if (attackers[attackerIndex].guerrilla && attackers[attackerIndex].health > 0 && attackerIndex+1 < attackers.Count)
            {
                CallDamageText(0, "유격 ", !isTeam);

                //통계 기록: 유격 발동
                unitStats[attackers[attackerIndex].UniqueId].IncrementSkillUsage("유격");

                (attackers[attackerIndex + 1], attackers[attackerIndex]) = (attackers[attackerIndex],attackers[attackerIndex+1]);
                
                isGuerrilla = true;
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

    //유산 총괄
    private void ProcessRelic()
    {
        //배율 초기화
        RogueLikeData.Instance.ResetFinalDamage();
        
        //유산 저장
        GetRelicData();
        RelicManager.CheckFusion(ref ownedRelics);
        // 아군, 적군 데이터를 RogueLikeData 싱글톤에 저장
        RogueLikeData.Instance.AllMyUnits(myUnits);
        RogueLikeData.Instance.AllEnemyUnits(enemyUnits);

        //해주 유산 확인
        curseBlock = RogueLikeData.Instance.GetOwnedRelicById(23) == null ? false : true;

        //스탯 유산 적용
        RelicManager.RunStateRelic(ownedRelics,curseBlock);

        myFinalDamage = RogueLikeData.Instance.GetMyMultipleDamage();
        enemyFinalDamage = RogueLikeData.Instance.GetEnemyMultipleDamage();

        myUnits = RogueLikeData.Instance.GetMyUnits();

        //유산 이미지 생성
        autoBattleUI.CreateWarRelic();
    }

    //유산 저장
    private void GetRelicData()
    {
        ownedRelics.Clear();
        ownedRelicIds.Clear();

        // 중복을 방지하며 유산 추가하는 메서드
        void AddRelics(List<WarRelic> relics)
        {
            foreach (var relic in relics)
            {
                if (ownedRelicIds.Add(relic.id)) // HashSet에 추가 성공하면 중복 아님
                {
                    ownedRelics.Add(relic);
                }
            }
        }

        // 유물 데이터 가져오기
        AddRelics(RogueLikeData.Instance.GetRelicsByType(RelicType.AllEffect));
        AddRelics(RogueLikeData.Instance.GetRelicsByType(RelicType.SpecialEffect));
        AddRelics(RogueLikeData.Instance.GetRelicsByType(RelicType.StateBoost));
        AddRelics(RogueLikeData.Instance.GetRelicsByType(RelicType.BattleActive));
        AddRelics(RogueLikeData.Instance.GetRelicsByType(RelicType.ActiveState));
    }

   
}


