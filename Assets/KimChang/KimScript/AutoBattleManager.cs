using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections;


public class AutoBattleManager : MonoBehaviour
{
    [SerializeField] private AutoBattleUI autoBattleUI;
    private AbilityManager abilityManager = new AbilityManager();
    
    private float waittingTime = 500;

    List<RogueUnitDataBase> myUnits = new();
    List<RogueUnitDataBase> enemyUnits = new();
    List<RogueUnitDataBase> myDeathUnits = new();
    List<RogueUnitDataBase> enemyDeathUnits = new();
    RogueUnitDataBase myFrontUnit;
    RogueUnitDataBase enemyFrontUnit;

    bool isFirstAttack = true;
    private enum BattleState
    {
        None,
        Enter,        
        Check,        
        Start,        
        Preparation,
        Crash,
        Support,
        Animation,   
        Death,
        End
    }

    private BattleState currentState = BattleState.None;

    private bool isProcessing= false;

    //데미지 배율
    private float myFinalDamage = 0;
    private float enemyFinalDamage = 0;

    private bool isTest=false;

    //이 씬이 로드되었을 때== 구매 배치로 전투 씬 입장했을때
    private void Start()
    {
        //여기 코드 추후 삭제
        UnitLoader.Instance.LoadUnitsFromJson();

        if (autoBattleUI == null)
            autoBattleUI = FindObjectOfType<AutoBattleUI>();
        if (isTest) return;
        currentState = BattleState.None;
        InitializeRogueLike();
    }
    private async void Update()
    {
        if (isProcessing || Time.timeScale == 0) return;

        isProcessing = true;

        switch (currentState)
        {
            case BattleState.None:
                isProcessing = false;
                break;
            case BattleState.Check:
                await HandleCheck();
                break;
            case BattleState.Start:
                await HandlePhase(HandleStart);
                break;
            case BattleState.Preparation:
                await HandleOneTurn();
                if (!HandleEnd())
                    await HandlePhase(HandlePreparation);
                else
                    isProcessing = false;
                break;
            case BattleState.Crash:
                await HandlePhase(HandleCrash);
                break;
            case BattleState.Support:
                await HandlePhase(HandleSupport);
                break;
            case BattleState.Animation:
                await HandleAnimation();
                break;
            case BattleState.Death:
            case BattleState.End:
                isProcessing = false;
                break;
        }
    }


    //유닛 id를 기반으로 유닛 생성
    private List<RogueUnitDataBase> GetUnitsById(List<int> unitIds)
    {
        List<RogueUnitDataBase> units = new();
        foreach (int unitId in unitIds)
        {
            RogueUnitDataBase unit = UnitLoader.Instance.GetCloneUnitById(unitId, false);
            units.Add(unit);
        }
        return units;
    }


    //유닛 데이터 받고 전투 시작
    public void StartBattle(List<int> _myUnitIds, List<int> _enemyUnitIds)
    {
        SetTest();
        InitializeBattle(_myUnitIds, _enemyUnitIds);
    }

    private async Task HandleOneTurn()
    {
        bool isTrun =abilityManager.ProcessOneTurn();

        if (isTrun)
            await Task.Delay((int)waittingTime); // 0.5초 대기

        await Task.Yield();
    }

    //페이즈 관리
    private async Task HandlePhase(Func<Task<bool>> phaseHandler)
    {
        bool phaseHadEffect = await phaseHandler(); // 준비 페이즈 결과

        await Task.Delay((int)(waittingTime*0.52f));
        UpdateUnitHp();

        if (!ManageUnitDeath() && !(myUnits.Count == 0 || enemyUnits.Count == 0))
        {
            switch (currentState)
            {
                case BattleState.Start:
                    currentState = BattleState.Preparation;
                    break;
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

        bool skipWait = (currentState == BattleState.Crash && !phaseHadEffect);
        await Task.Delay(skipWait ? 0 : (int)waittingTime);

        isProcessing = false;
    }


    //유닛 갯수 최신화
    private void UpdateUnitCount()
    {
        int myUnitCount = 0;
        int enemyUnitCount = 0;

        foreach (RogueUnitDataBase unitData in myUnits)
        {
            if (unitData.health > 0)
            {
                myUnitCount++;
            }
        }
        foreach (RogueUnitDataBase unitData in enemyUnits)
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
        if (myUnits.Count== 0 || enemyUnits.Count==0) return;
        float myUnitHp = myUnits[0].health;
        float enemyHp = enemyUnits[0].health;
        float myMAxHp = myUnits[0].maxHealth;
        float enemyMaxHp = enemyUnits[0].maxHealth;
       
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

    // 유닛 생성UI 호출
    private void CallCreateUnit()
    {
        List<RogueUnitDataBase> myRangeUnits=new();
        List<RogueUnitDataBase> enemyRangUnits=new();
        List<RogueUnitDataBase> myAliveUnits = new();
        List<RogueUnitDataBase> enemyAliveUnits = new();
        //원거리 공격이 가능한지 검사
        for (int i = 1; i < myUnits.Count; i++)
        {
            if (myUnits[i].rangedAttack && (myUnits[i].range - i > 0) && myUnits[i].health>0)
            {
                myRangeUnits.Add(myUnits[i]);
            }
        }
        for (int i = 1; i < enemyUnits.Count; i++)
        {
            if (enemyUnits[i].rangedAttack && (enemyUnits[i].range - i > 0) && enemyUnits[i].health > 0)
            {
                enemyRangUnits.Add(enemyUnits[i]);
            }
        }
        
        autoBattleUI.CreateUnitBox(myUnits, enemyUnits, abilityManager.CalculateDodge(myUnits[0],true,isFirstAttack), abilityManager.CalculateDodge(enemyUnits[0],false,isFirstAttack),myRangeUnits,enemyRangUnits);
    }
    //전투 입장
    private void ProcessEnter()
    {
        abilityManager.ProcessEnter();
    }

    //전투 전 발동
    private void ProcessBeforeBattle(List<RogueUnitDataBase> units, List<RogueUnitDataBase> defenders, bool isTeam)
    {
        abilityManager.ProcessBeforeBattle(units, defenders, isTeam, autoBattleUI);
    }
    //전투 당 한번
    private bool StartBattlePhase()
    {
        myFrontUnit = myUnits[0];
        enemyFrontUnit = enemyUnits[0];

        return abilityManager.ProcessStartBattle(myUnits, enemyUnits, myFinalDamage, true) 
            | abilityManager.ProcessStartBattle(enemyUnits, myUnits, enemyFinalDamage, false);
    }
    //준비 페이즈
    private bool PreparationPhase()
    {
        //전열 유닛
        myFrontUnit = myUnits[0];
        enemyFrontUnit = enemyUnits[0];

        bool isPreparation =
            (abilityManager.ProcessPreparationAbility(myUnits, enemyUnits, isFirstAttack, true, myFinalDamage) |
             abilityManager.ProcessPreparationAbility(enemyUnits, myUnits, isFirstAttack, false, enemyFinalDamage));
        return isPreparation;
    }

    //충돌 페이즈
    private void ChrashPhase()
    {
        //전열 유닛
        myFrontUnit = myUnits[0];
        enemyFrontUnit = enemyUnits[0];

        abilityManager.ProcessChrashAbility(myUnits, enemyUnits, isFirstAttack, myFinalDamage, true);
        abilityManager.ProcessChrashAbility(enemyUnits,myUnits, isFirstAttack, enemyFinalDamage, false);
    }

    //지원 페이즈
    private void SupportPhase()
    {
        //전열 유닛
        myFrontUnit = myUnits[0];
        enemyFrontUnit = enemyUnits[0];

        abilityManager.ProcessSupportAbility(myUnits, enemyUnits, true, myFinalDamage, isFirstAttack);
        abilityManager.ProcessSupportAbility(enemyUnits,myUnits, false, enemyFinalDamage, isFirstAttack);

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
    //유닛 사망 처리
    private bool ManageUnitDeath()
    {
        return abilityManager.ProcessDeath(ref myUnits, ref enemyUnits, ref myDeathUnits, ref enemyDeathUnits, ref isFirstAttack,myFrontUnit,enemyFrontUnit);
    }
   
    //유닛 데이터 초기화
    private void InitializeBattle(List<int> _myUnitIds, List<int> _enemyUnitIds)
    {
        if (autoBattleUI == null)
        {
            autoBattleUI = FindObjectOfType<AutoBattleUI>();
        }

        // 유닛 데이터 받아옴
        myUnits = GetUnitsById(_myUnitIds);
        enemyUnits = GetUnitsById(_enemyUnitIds);

        isFirstAttack = true;

        //기본 데이터 설정
        SetBaseData();

        //데이터 저장
        //SaveData saveData = new SaveData();
        //saveData.SaveDataFile();

        //스탯 유산 실행
        ProcessRelic();

        //유닛 생성
        UpdateUnitUI();

        currentState = BattleState.Check;

        Debug.Log(currentState);
    }
    //로그라이크 모드일떄 초기화
    private void InitializeRogueLike()
    {
        int presetId = RogueLikeData.Instance.GetPresetID();
        if (presetId == -1) return;
        List<int> unitIds = StagePresetLoader.I.GetByID(presetId).UnitList;

        enemyUnits = GetUnitsById(unitIds);
        myUnits = RogueUnitDataBase.SetMyUnitsNormalize();
        RogueLikeData.Instance.ClearSavedMyUnits();

        RogueLikeData.Instance.SetBattleUnitCount(myUnits.Count);

        ProcessEnter();

        isFirstAttack = true;
        SetBaseData();
        
        //데이터 저장
        SaveData saveData = new SaveData();
        saveData.SaveDataFile();
        
        ProcessRelic();
         
        UpdateUnitUI();
        
        //로딩창 종료
        GameManager.Instance.CloseLoading();

        currentState = BattleState.Check;
    }

    // 확인 단계 처리 (전투 시작 전에 필요한 확인 작업 수행)
    private async Task HandleCheck()
    {
        //맵 효과
        abilityManager.CalculateFieldEffect();
        
        ProcessBeforeBattle(myUnits, enemyUnits, true);
        ProcessBeforeBattle(enemyUnits, myUnits, false);

        //유산
        if (RelicManager.CheckRelicById(114))
        {
            float sum = 0;
            foreach (var unit in myUnits)
            {
                sum += unit.maxHealth;
                if(sum >= 1700)
                {
                    RogueLikeData.Instance.EarnGold(200);
                    break;
                }
            }
        }

        await Task.Delay((int)waittingTime);
        currentState = BattleState.Start;
        isProcessing = false; // 체크가 끝난 후 상태를 변경
    }
    // 시작 단계 처리 (전투 시작을 위한 초기화)
    private async Task<bool> HandleStart()
    {
        bool result = StartBattlePhase();

        await Task.Yield();
        return result;

    }
    // 애니메이션 단계 처리 (전투 중 원하는 타이밍에 실행)
    private async Task HandleAnimation()
    {
        await Task.Yield(); // 입력된 시간만큼 대기
        currentState = BattleState.Preparation;
    }
    // 준비 페이즈 관리
    private async Task<bool> HandlePreparation()
    {
        abilityManager.SetMultipleDamage(myFrontUnit,enemyFrontUnit,ref myFinalDamage, ref enemyFinalDamage);
        UpdateUnitUI();
        bool result = PreparationPhase();
        await Task.Yield();
        return result;
    }

    //충돌 페이즈 관리
    private async Task<bool> HandleCrash()
    {
        ChrashPhase();

        await Task.Yield();
        return true;
    }

    //지원 페이즈 관리
    private async Task<bool> HandleSupport()
    {
        SupportPhase();

        await Task.Yield();
        return true;
    }
    //종료 확인
    private int CheckEnd()
    {
        if (myUnits.Count == 0 || enemyUnits.Count == 0)
        {
            if (enemyUnits.Count == 0 && myUnits.Count > 0)
            {
                Debug.Log("나의 승리");
                return 0;
            }
            else if (enemyUnits.Count > 0 && myUnits.Count == 0)
            {
                Debug.Log("나의 패배");
                return 1;
            }
            else
            {
                Debug.Log("무승부");
                return 2;
            }
        }

        return 3;
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

            foreach (var unit in enemyDeathUnits)
            {
                RogueLikeData.Instance.AddScore((int)unit.maxHealth);
            }

            RewardManager.AddBattleRewardByStage(result, myDeathUnits, enemyDeathUnits);
            UpdateUnitCount();
            UpdateUnitHp();

            // 0.5초 뒤에 실행되도록 코루틴 시작
            StartCoroutine(DelayedBattleEnd(result));

            return true;
        }
        return false;
    }

    // 전투 종료 처리 코루틴
    private IEnumerator DelayedBattleEnd(int result)
    {
        yield return new WaitForSeconds(waittingTime*0.001f);

        autoBattleUI.FightEnd();

        RogueLikeData.Instance.SetFieldId(0);
        RogueLikeData.Instance.ClearBuffDeBuff();
        RogueLikeData.Instance.SetBattleUnitCount(0);

        WarRelic relic = RogueLikeData.Instance.GetOwnedRelicById(109);
        if (relic != null) relic.Execute();

        if (RelicManager.CheckRelicById(78) && result == 0)
        {
            RogueLikeData.Instance.AddSariStack(3);
        }
        SaveData saveData = new SaveData();
        saveData.SaveDataBattaleEnd(myUnits, myDeathUnits);
    }


    //기본 데이터 초기화
    private void SetBaseData()
    {
        RogueLikeData.Instance.ResetFinalDamage();

        RelicManager.GetRelicData();

        RogueLikeData.Instance.SetAllMyUnits(myUnits);
        RogueLikeData.Instance.SetAllEnemyUnits(enemyUnits);
    }

    //유산 호출 및 초기화
    private void ProcessRelic()
    {
        //스탯 유산 적용
        RelicManager.RunStateRelic();

        //유산 이미지 생성
        autoBattleUI.CreateWarRelic();
    }
    
    private void SetTest()
    {
        isTest = true;
    }

}


