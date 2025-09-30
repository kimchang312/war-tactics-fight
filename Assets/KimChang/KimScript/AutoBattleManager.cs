using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;


public class AutoBattleManager : MonoBehaviour
{
    [SerializeField] private AutoBattleUI autoBattleUI;
    [SerializeField] private BattleCrashAnimation battleAnim;
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
        if (autoBattleUI == null)
            autoBattleUI = FindObjectOfType<AutoBattleUI>();
        if (isTest)
        {
            autoBattleUI.OpenGoTestBtn();
            return;
        }
            currentState = BattleState.None;
        if (battleAnim == null) battleAnim = FindObjectOfType<BattleCrashAnimation>();
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
            unit.NormalizeStateModifiers();
            unit.ApplyModifiers();
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
        for (int i = 0; i < myUnits.Count; i++)
            if (myUnits[i].health > 0) myUnitCount++;

        int enemyUnitCount = 0;
        for (int i = 0; i < enemyUnits.Count; i++)
            if (enemyUnits[i].health > 0) enemyUnitCount++;

        autoBattleUI.UpdateUnitCountUI(myUnitCount, enemyUnitCount);
    }

    // 유닛 생성UI 호출
    private void CallCreateUnit()
    {
        List<RogueUnitDataBase> myRangeUnits=new();
        List<RogueUnitDataBase> enemyRangUnits=new();

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
  
        RogueLikeData.Instance.SetMyTeam(myUnits);
        RogueLikeData.Instance.SetAllEnemyUnits(enemyUnits);

        isFirstAttack = true;

        //기본 데이터 설정
        SetBaseData();

        //데이터 저장
        //SaveData saveData = new SaveData();
        //saveData.SaveDataFile();

        //
        UnitStateChange.ChangeStateMyUnits();

        ProcessRelic();

        //유닛 생성
        UpdateUnitUI();

        currentState = BattleState.Check;
    }
    //로그라이크 모드일떄 초기화
    private void InitializeRogueLike()
    {
        if (UnitStateChange.CalculateRunMorale() != null)
        {
            
        }

        int presetId = RogueLikeData.Instance.GetPresetID();
        if (presetId == -1) 
        {
            Debug.Log("프리셋 아이디 오류");
            return; 
        }
        List<int> unitIds = StagePresetLoader.I.GetByID(presetId).UnitList;
        
        enemyUnits = GetUnitsById(unitIds);
        myUnits = RogueLikeData.Instance.GetMyUnits();

        RogueLikeData.Instance.ClearSavedMyUnits();

        RogueLikeData.Instance.SetBattleUnitCount(myUnits.Count);

        ProcessEnter();

        isFirstAttack = true;
        SetBaseData();

        //데이터 저장
        SaveData saveData = new SaveData();
        saveData.SaveDataFile();
        
        ProcessRelic();

        //사기로 안한 유닛 0
        if (myUnits.Count == 0)
        {
            HandleEnd(false);
        }
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
    private bool HandleEnd(bool isMyUnitExist = true)
    {
        int result = 3;
        if (!isMyUnitExist)
        {
            result = 1;
            myDeathUnits = null;
        }
        else
        {
            result = CheckEnd();
        }
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

            int gameResult = RewardManager.AddBattleRewardByStage(result, myDeathUnits, enemyDeathUnits);

            if (gameResult == 1)
            {
                autoBattleUI.GameEnd(false);
                return true;
            }
            else if (gameResult == 2)
            {
                autoBattleUI.GameEnd(true);
                return true;
            }
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
        //RelicManager.RunStateRelic();

        //유산 이미지 생성
        autoBattleUI.CreateWarRelic();
    }
    
    private void SetTest()
    {
        isTest = true;
    }

    // 화면 표시에 필요한 것만 담은 뷰 스냅샷
    public readonly struct HpViewData
    {
        public readonly bool MyActive, EnemyActive;
        public readonly int MyHp, MyMax, EnemyHp, EnemyMax;
        public readonly bool MySecondActive, EnemySecondActive;
        public readonly int MySecondHp, MySecondMax, EnemySecondHp, EnemySecondMax;

        public HpViewData(
            bool myActive, int myHp, int myMax,
            bool enemyActive, int enemyHp, int enemyMax,
            bool mySecondActive, int mySecondHp, int mySecondMax,
            bool enemySecondActive, int enemySecondHp, int enemySecondMax)
        {
            MyActive = myActive; EnemyActive = enemyActive;
            MyHp = myHp; MyMax = myMax; EnemyHp = enemyHp; EnemyMax = enemyMax;
            MySecondActive = mySecondActive; EnemySecondActive = enemySecondActive;
            MySecondHp = mySecondHp; MySecondMax = mySecondMax;
            EnemySecondHp = enemySecondHp; EnemySecondMax = enemySecondMax;
        }
    }

    // 화면에 표시할 체력 데이터 스냅샷을 생성
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ClampNonNegToInt(float v) => v <= 0 ? 0 : (int)v;

    // 전투 중 매 프레임 호출: 뷰 스냅샷 생성 후 UI에 반영
    private void UpdateUnitHp()
    {
        var data = BuildHpViewData();
        autoBattleUI.ApplyHp(data);           // UI는 스냅샷만 받아서 그림
    }

    // 도메인 규칙(예: 2번 유닛은 체력 0 이하면 숨김)을 적용한 뷰 스냅샷 생성
    private HpViewData BuildHpViewData()
    {
        var my0 = myUnits.Count > 0 ? myUnits[0] : null;
        var en0 = enemyUnits.Count > 0 ? enemyUnits[0] : null;

        bool myActive = my0 != null;
        bool enActive = en0 != null;

        int myHp = myActive ? ClampNonNegToInt(my0.health) : 0;
        int myMax = myActive ? ClampNonNegToInt(my0.maxHealth) : 1;
        int enHp = enActive ? ClampNonNegToInt(en0.health) : 0;
        int enMax = enActive ? ClampNonNegToInt(en0.maxHealth) : 1;

        var my1 = myUnits.Count > 1 ? myUnits[1] : null;
        var en1 = enemyUnits.Count > 1 ? enemyUnits[1] : null;

        // 규칙: 2번 유닛은 존재하고 체력 > 0일 때만 표시
        bool mySecondActive = my1 != null && my1.health > 0;
        bool enSecondActive = en1 != null && en1.health > 0;

        int my2Hp = mySecondActive ? ClampNonNegToInt(my1.health) : 0;
        int my2Max = mySecondActive ? ClampNonNegToInt(my1.maxHealth) : 1;
        int en2Hp = enSecondActive ? ClampNonNegToInt(en1.health) : 0;
        int en2Max = enSecondActive ? ClampNonNegToInt(en1.maxHealth) : 1;

        return new HpViewData(
            myActive, myHp, myMax,
            enActive, enHp, enMax,
            mySecondActive, my2Hp, my2Max,
            enSecondActive, en2Hp, en2Max
        );
    }





}


