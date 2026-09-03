using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using static AutoBattleManager;


public class AutoBattleManager : MonoBehaviour
{
    [SerializeField] private AutoBattleUI autoBattleUI;
    [SerializeField] private BattleCrashAnimation battleAnim;
    [SerializeField] private EffectManager effectManager; // 이펙트 관리자 (Queue + Pool)

    [Header("페이즈별 이펙트 설정 (옵션)")]
    [SerializeField] private EffectCD preparationEffect; // 준비 페이즈 이펙트
    [SerializeField] private EffectCD crashEffect;       // 충돌 페이즈 이펙트 (돌격 등)
    [SerializeField] private EffectCD supportEffect;     // 지원 페이즈 이펙트 (원거리, 치유)

    private AbilityManager abilityManager = new AbilityManager();

    private const float BaseWaittingTime = 500f;
    private float waittingTime = BaseWaittingTime;

    private bool isBattlePaused;

    List<RogueUnitDataBase> myUnits = new();
    List<RogueUnitDataBase> enemyUnits = new();
    List<RogueUnitDataBase> myDeathUnits = new();
    List<RogueUnitDataBase> enemyDeathUnits = new();
    RogueUnitDataBase myFrontUnit;
    RogueUnitDataBase enemyFrontUnit;

    // 사용처: 현재 페이즈 시작 시점의 전열 쌍을 저장하여 새 만남 발생 여부를 판정한다.
    private RogueUnitDataBase phaseStartMyFrontUnit;
    private RogueUnitDataBase phaseStartEnemyFrontUnit;

    // 사용처: 현재 페이즈 도중 전열 쌍이 바뀌었는지 저장한다.
    private bool phaseFrontPairChanged;

    bool isFirstAttack = true;

    private bool isDestroyed;
    private bool isBattleEnding;
    private Coroutine delayedBattleEndCoroutine;

    private bool isProcessing = false;
    private int battleTurn = 0;

    private bool isTest = false;
    private BattleState currentState = BattleState.None;

    // 사용처: 사망 데이터 처리는 즉시 하되, 유닛 투명 연출은 페이즈 애니메이션 이후 실행하기 위해 저장한다.
    private readonly List<DeathVisualRequest> pendingDeathVisuals = new();

    private readonly struct DeathVisualRequest
    {
        public readonly RogueUnitDataBase Unit;
        public readonly int UnitIndex;
        public readonly bool IsMyUnit;

        public DeathVisualRequest(RogueUnitDataBase unit, int unitIndex, bool isMyUnit)
        {
            Unit = unit;
            UnitIndex = unitIndex;
            IsMyUnit = isMyUnit;
        }
    }

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



    //이 씬이 로드되었을 때== 구매 배치로 전투 씬 입장했을때
    private void Start()
    {
        var speedManager = GameSpeedManager.Instance;
        if (speedManager != null)
        {
            speedManager.OnGameSpeedChanged -= ChangeWaittingTime;
            speedManager.OnGameSpeedChanged += ChangeWaittingTime;
            ChangeWaittingTime(speedManager.GameSpeed);
        }

        if (autoBattleUI == null)
            autoBattleUI = FindObjectOfType<AutoBattleUI>();

        if (battleAnim == null)
            battleAnim = FindObjectOfType<BattleCrashAnimation>();

        if (effectManager == null)
            effectManager = EffectManager.Instance;

        if (isTest)
        {
            //autoBattleUI.OpenGoTestBtn();
            return;
        }

        currentState = BattleState.None;
        InitializeRogueLike();
        //RelicManager.HandleRandomRelic()
    }

    private async void Update()
    {
        if (IsManagerInvalid() || isProcessing || isBattlePaused || Time.timeScale == 0f)
            return;

        isProcessing = true;

        try
        {
            switch (currentState)
            {
                case BattleState.None:
                    break;

                case BattleState.Check:
                    await HandleCheck();
                    break;

                case BattleState.Start:
                    await HandlePhase(HandleStart);
                    break;

                case BattleState.Preparation:
                    await HandleOneTurn();

                    if (IsManagerInvalid())
                        return;

                    if (!HandleEnd())
                        await HandlePhase(HandlePreparation);
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
                    break;
            }
        }
        catch (MissingReferenceException)
        {
            isDestroyed = true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            if (!isDestroyed)
                isProcessing = false;
        }
    }

    private void OnDestroy()
    {
        var speedManager = GameSpeedManager.Instance;
        if (speedManager != null)
            speedManager.OnGameSpeedChanged -= ChangeWaittingTime;

        isDestroyed = true;
        pendingDeathVisuals.Clear();

        if (delayedBattleEndCoroutine != null)
        {
            StopCoroutine(delayedBattleEndCoroutine);
            delayedBattleEndCoroutine = null;
        }
    }

    //유닛 id를 기반으로 유닛 생성
    private List<RogueUnitDataBase> GetUnitsById(List<int> unitIds)
    {
        List<RogueUnitDataBase> units = new();

        if (unitIds == null || UnitLoader.Instance == null)
            return units;

        foreach (int unitId in unitIds)
        {
            RogueUnitDataBase unit = UnitLoader.Instance.GetCloneUnitById(unitId, false);
            if (unit == null)
            {
                Debug.LogWarning($"[AutoBattleManager] 유닛 생성 실패: unitId={unitId}");
                continue;
            }

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
        bool isTurnEffect = abilityManager.ProcessOneTurn();

        if (isTurnEffect)
        {
            await WaitBattleMilliseconds((int)waittingTime);

            UpdateUnitHp();

            // 사용처: 턴 시작 효과로 죽은 유닛을 준비 페이즈 실행 전에 정리한다.
            ResolveAllDeaths();

            bool playedDeathVisual = PlayPendingDeathVisuals();
            if (playedDeathVisual)
            {
                await WaitBattleMilliseconds((int)waittingTime);
            }

            UpdateUnitUI();
        }

        await Task.Yield();
    }

    //페이즈 관리
    private async Task HandlePhase(Func<Task<bool>> phaseHandler)
    {
        if (IsManagerInvalid())
            return;

        CapturePhaseStartFrontPair();

        bool phaseHadEffect = await phaseHandler();

        if (IsManagerInvalid())
            return;

        await WaitBattleMilliseconds((int)(waittingTime * 0.52f));

        if (IsManagerInvalid())
            return;

        UpdateUnitHp();

        if (!phaseFrontPairChanged)
        {
            ResolveDeathsAndCheckFrontPairChanged();
        }

        bool playedDeathVisual = PlayPendingDeathVisuals();

        if (playedDeathVisual)
        {
            await WaitBattleMilliseconds((int)waittingTime);

            if (IsManagerInvalid())
                return;
        }

        int endCode = CheckEnd();
        if (endCode == 3)
        {
            if (phaseFrontPairChanged && currentState != BattleState.Start)
            {
                isFirstAttack = true;
                currentState = BattleState.Preparation;
            }
            else
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

            if (playedDeathVisual)
            {
                UpdateUnitUI();
            }
        }
        else
        {
            HandleEnd();
        }

        if (IsManagerInvalid())
            return;

        bool skipWait = (currentState == BattleState.Crash && !phaseHadEffect);
        await WaitBattleMilliseconds(skipWait ? 0 : (int)waittingTime);

        if (IsManagerInvalid())
            return;

        isProcessing = false;
    }


    //유닛 갯수 최신화
    private void UpdateUnitCount()
    {
        if (autoBattleUI == null)
            return;

        int myUnitCount = 0;
        if (myUnits != null)
        {
            for (int i = 0; i < myUnits.Count; i++)
                if (myUnits[i] != null && myUnits[i].health > 0) myUnitCount++;
        }

        int enemyUnitCount = 0;
        if (enemyUnits != null)
        {
            for (int i = 0; i < enemyUnits.Count; i++)
                if (enemyUnits[i] != null && enemyUnits[i].health > 0) enemyUnitCount++;
        }

        autoBattleUI.UpdateUnitCountUI(myUnitCount, enemyUnitCount);
    }


    // 유닛 생성UI 호출
    private void CallCreateUnit()
    {
        if (autoBattleUI == null)
            return;

        List<RogueUnitDataBase> myRangeUnits = new();
        List<RogueUnitDataBase> enemyRangeUnits = new();

        if (myUnits != null)
        {
            for (int i = 1; i < myUnits.Count; i++)
            {
                if (CanUseRangedAttackUnit(myUnits[i], i))
                    myRangeUnits.Add(myUnits[i]);
            }
        }

        if (enemyUnits != null)
        {
            for (int i = 1; i < enemyUnits.Count; i++)
            {
                if (CanUseRangedAttackUnit(enemyUnits[i], i))
                    enemyRangeUnits.Add(enemyUnits[i]);
            }
        }

        float myDodge = (myUnits != null && myUnits.Count > 0)
            ? abilityManager.CalculateDodge(myUnits[0], true, isFirstAttack)
            : 0f;

        float enemyDodge = (enemyUnits != null && enemyUnits.Count > 0)
            ? abilityManager.CalculateDodge(enemyUnits[0], false, isFirstAttack)
            : 0f;

        autoBattleUI.CreateUnitBox(
            myUnits ?? new List<RogueUnitDataBase>(),
            enemyUnits ?? new List<RogueUnitDataBase>(),
            myDodge,
            enemyDodge,
            myRangeUnits,
            enemyRangeUnits
        );
    }

    // 사용처: 전투 중 현재 전열 유닛의 회피율 텍스트만 빠르게 갱신한다.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateDodgeUI()
    {
        if (autoBattleUI == null)
            return;

        RogueUnitDataBase myFront = GetFrontUnitOrNull(myUnits);
        RogueUnitDataBase enemyFront = GetFrontUnitOrNull(enemyUnits);

        float myDodge = myFront != null
            ? abilityManager.CalculateDodge(myFront, true, isFirstAttack)
            : 0f;

        float enemyDodge = enemyFront != null
            ? abilityManager.CalculateDodge(enemyFront, false, isFirstAttack)
            : 0f;

        autoBattleUI.UpdateDodgeText(myDodge, enemyDodge, myFront != null, enemyFront != null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static RogueUnitDataBase GetFrontUnitOrNull(List<RogueUnitDataBase> units)
    {
        return units != null && units.Count > 0 ? units[0] : null;
    }

    // 사용처: 전열을 제외한 유닛이 현재 위치에서 원거리 공격 가능한지 판정한다.
    // range 2 = 2번째 유닛만 가능, range 3 = 2~3번째 유닛 가능.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CanUseRangedAttackUnit(RogueUnitDataBase unit, int unitIndex)
    {
        if (unit == null || unit.health <= 0 || !unit.rangedAttack)
            return false;

        if (unitIndex <= 0)
            return false;

        int maxAttackIndex = Mathf.FloorToInt(unit.range) - 1;
        return unitIndex <= maxAttackIndex;
    }

    //전투 입장
    private void ProcessEnter()
    {
        abilityManager.ProcessEnter();
    }

    //전투 전 발동
    private void ProcessBeforeBattle(List<RogueUnitDataBase> units, List<RogueUnitDataBase> defenders, bool isTeam)
    {
        abilityManager.ProcessBeforeBattle(units, defenders, isTeam, autoBattleUI, this);
    }

    // 사용처: 각 전투 페이즈 진입 전에 전열 유닛이 실제로 존재하는지 확인
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CanRunBattlePhase()
    {
        return myUnits != null && enemyUnits != null && myUnits.Count > 0 && enemyUnits.Count > 0;
    }
    // 사용처: 현재 아군 전열 유닛을 빠르게 가져온다.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RogueUnitDataBase GetMyFrontUnitOrNull()
    {
        return myUnits != null && myUnits.Count > 0 ? myUnits[0] : null;
    }

    // 사용처: 현재 적군 전열 유닛을 빠르게 가져온다.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RogueUnitDataBase GetEnemyFrontUnitOrNull()
    {
        return enemyUnits != null && enemyUnits.Count > 0 ? enemyUnits[0] : null;
    }

    // 사용처: 페이즈 시작 시점의 전열 쌍을 저장한다.
    private void CapturePhaseStartFrontPair()
    {
        phaseStartMyFrontUnit = GetMyFrontUnitOrNull();
        phaseStartEnemyFrontUnit = GetEnemyFrontUnitOrNull();

        myFrontUnit = phaseStartMyFrontUnit;
        enemyFrontUnit = phaseStartEnemyFrontUnit;

        phaseFrontPairChanged = false;
    }

    // 사용처: 페이즈 시작 시점과 현재 전열 쌍이 달라졌는지 확인한다.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsPhaseStartFrontPairChanged()
    {
        return phaseStartMyFrontUnit != GetMyFrontUnitOrNull()
            || phaseStartEnemyFrontUnit != GetEnemyFrontUnitOrNull();
    }

    // 사용처: 사망 정리 후 새 전열 쌍이 만들어졌는지 확인하고, 새 만남이면 준비 페이즈 재진입 상태로 만든다.
    private bool ResolveDeathsAndCheckFrontPairChanged()
    {
        ResolveAllDeaths();

        if (!IsPhaseStartFrontPairChanged())
            return false;

        // 사용처: 전열 쌍 변경 여부만 기록하고, UI 갱신은 사망/충돌 연출이 끝난 뒤 HandlePhase에서 처리한다.
        phaseFrontPairChanged = true;
        isFirstAttack = true;

        return true;
    }
    //전투 당 한번
    private bool StartBattlePhase()
    {
        if (!CanRunBattlePhase())
            return false;

        myFrontUnit = myUnits[0];
        enemyFrontUnit = enemyUnits[0];

        return abilityManager.ProcessStartBattle(myUnits, enemyUnits, true)
            | abilityManager.ProcessStartBattle(enemyUnits, myUnits, false);
    }

    //준비 페이즈
    private bool PreparationPhase()
    {
        if (!CanRunBattlePhase())
            return false;

        myFrontUnit = myUnits[0];
        enemyFrontUnit = enemyUnits[0];

        bool isPreparation = false;

        isPreparation |= abilityManager.ProcessPreparationAbility(myUnits, enemyUnits, isFirstAttack, true);

        // 사용처: 아군 준비 능력으로 사망/교체가 발생하면 적 준비 능력을 실행하지 않고 새 만남으로 처리한다.
        if (ResolveDeathsAndCheckFrontPairChanged() || !CanRunBattlePhase())
            return isPreparation;

        isPreparation |= abilityManager.ProcessPreparationAbility(enemyUnits, myUnits, isFirstAttack, false);

        // 사용처: 적군 준비 능력 이후 전열 쌍 변경 여부를 확인한다.
        ResolveDeathsAndCheckFrontPairChanged();

        return isPreparation;
    }

    //충돌 페이즈
    private void ChrashPhase()
    {
        if (!CanRunBattlePhase())
            return;

        myFrontUnit = myUnits[0];
        enemyFrontUnit = enemyUnits[0];

        // 사용처: 충돌은 양쪽 전열이 동시에 공격하는 판정이므로, 중간 사망 정리를 하지 않는다.
        abilityManager.ProcessChrashAbility(myUnits, enemyUnits, isFirstAttack, true);

        if (CanRunBattlePhase())
        {
            abilityManager.ProcessChrashAbility(enemyUnits, myUnits, isFirstAttack, false);
        }

        // 사용처: 양쪽 충돌 계산이 끝난 뒤 한 번만 사망/전열 변경을 정리한다.
        ResolveDeathsAndCheckFrontPairChanged();
    }

    //지원 페이즈
    private void SupportPhase()
    {
        if (!CanRunBattlePhase())
            return;

        myFrontUnit = myUnits[0];
        enemyFrontUnit = enemyUnits[0];

        // 사용처: 지원 공격도 양쪽 후열이 동시에 발사하는 판정이므로, 중간 사망 정리를 하지 않는다.
        abilityManager.ProcessSupportAbility(myUnits, enemyUnits, true, isFirstAttack);

        if (CanRunBattlePhase())
        {
            abilityManager.ProcessSupportAbility(enemyUnits, myUnits, false, isFirstAttack);
        }

        // 사용처: 양쪽 지원 계산이 끝난 뒤 한 번만 사망/전열 변경을 정리한다.
        ResolveDeathsAndCheckFrontPairChanged();
    }

    //유닛 UI최신화
    private void UpdateUnitUI()
    {
        if (autoBattleUI == null)
            return;

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
        return abilityManager.ProcessDeath(ref myUnits, ref enemyUnits, ref myDeathUnits, ref enemyDeathUnits, ref isFirstAttack, myFrontUnit, enemyFrontUnit);
    }

    //유닛 데이터 초기화
    private void InitializeBattle(List<int> _myUnitIds, List<int> _enemyUnitIds)
    {
        ResetData();

        if (autoBattleUI == null)
        {
            autoBattleUI = FindObjectOfType<AutoBattleUI>();
        }

        // 유닛 데이터 받아옴
        myUnits = GetUnitsById(_myUnitIds);
        enemyUnits = GetUnitsById(_enemyUnitIds);

        RogueLikeData.Instance.SetMyTeam(myUnits);
        RogueLikeData.Instance.SetAllEnemyUnits(enemyUnits);

        //기본 데이터 설정
        SetBaseData();


        UnitStateChange.ChangeStateMyUnits();
        myUnits = RogueLikeData.Instance.GetMyUnits() ?? myUnits;

        ProcessRelic();

        //유닛 생성
        UpdateUnitUI();

        currentState = BattleState.Check;
    }
    //로그라이크 모드일떄 초기화
    private void InitializeRogueLike()
    {
        ResetData();

        if (RogueLikeData.Instance == null)
        {
            Debug.LogError("[AutoBattleManager] RogueLikeData.Instance가 없습니다.");
            if (GameManager.Instance != null)
                GameManager.Instance.CloseLoading();
            currentState = BattleState.End;
            return;
        }

        if (UnitStateChange.CalculateRunMorale() != null)
        {

        }

        int presetId = RogueLikeData.Instance.GetPresetID();
        int chapter = RogueLikeData.Instance.GetChapter();
        StageType stageType = RogueLikeData.Instance.GetCurrentStageType();
        var preset = (presetId != -1 && StagePresetLoader.I != null) ? StagePresetLoader.I.GetByID(presetId) : null;

        bool useRuntimeEnemyUnits = ShouldUseRuntimeEnemyUnits(chapter, stageType, presetId);
        if (useRuntimeEnemyUnits)
        {
            enemyUnits = RogueLikeData.Instance.GetEnemyUnits() ?? new List<RogueUnitDataBase>();
        }
        else if (preset != null && preset.UnitList != null && preset.UnitList.Count > 0)
        {
            enemyUnits = GetUnitsById(preset.UnitList) ?? new List<RogueUnitDataBase>();
        }
        else
        {
            enemyUnits = new List<RogueUnitDataBase>();
        }

        if (enemyUnits.Count == 0)
        {
            Debug.LogError($"[AutoBattleManager] 적 편성 데이터가 없습니다: chapter={chapter}, stageType={stageType}, presetId={presetId}, runtime={useRuntimeEnemyUnits}");

            if (GameManager.Instance != null)
                GameManager.Instance.CloseLoading();

            HandleEnd(false);
            return;
        }
        myUnits = RogueLikeData.Instance.GetMyUnits() ?? new List<RogueUnitDataBase>();

        RogueLikeData.Instance.ClearSavedMyUnits();
        RogueLikeData.Instance.SetBattleUnitCount(myUnits.Count);

        ProcessEnter();

        SetBaseData();

        // 로그라이크 전투 진입 시 사기/유산/전술개량 상태를 즉시 반영한다.
        UnitStateChange.ChangeStateMyUnits();
        // State relics can reorder or insert battle units (for example relics 11 and 72).
        // Keep the battle manager's structural list in sync with RogueLikeData.
        myUnits = RogueLikeData.Instance.GetMyUnits() ?? myUnits;

        RogueLikeData.Instance.SaveNow();

        ProcessRelic();

        //사기로 안한 유닛 0
        if (myUnits.Count == 0)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.CloseLoading();

            HandleEnd(false);
            return;
        }

        if (enemyUnits.Count == 0)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.CloseLoading();

            HandleEnd(true);
            return;
        }

        UpdateUnitUI();

        //로딩창 종료
        if (GameManager.Instance != null)
            GameManager.Instance.CloseLoading();

        currentState = BattleState.Check;
    }

    // 챕터 2+ 일반전투와 동적 엘리트 프리셋은 배치 화면에서 이미 생성한
    // 런타임 편성을 사용해야 한다. 정적 UnitList를 다시 읽으면 서로 다른 적이
    // 전투에 들어가거나 빈 편성으로 즉시 패배한다.
    private static bool ShouldUseRuntimeEnemyUnits(int chapter, StageType stageType, int presetId)
    {
        return (chapter >= 2 && stageType == StageType.Combat)
            || presetId < 0
            || (presetId >= 190 && presetId <= 192);
    }

    // 확인 단계 처리 (전투 시작 전에 필요한 확인 작업 수행)
    private async Task HandleCheck()
    {
        //맵 효과
        abilityManager.CalculateFieldEffect();

        //유산
        if (CanRunBattlePhase())
        {
            ProcessBeforeBattle(myUnits, enemyUnits, true);
            ProcessBeforeBattle(enemyUnits, myUnits, false);
            abilityManager.ProcessCommenderEffect(myUnits, enemyUnits);
            UpdateUnitUI();
            UpdateDodgeUI();
        }

        if (!CanRunBattlePhase())
        {
            HandleEnd(myUnits != null && myUnits.Count > 0);
            isProcessing = false;
            return;
        }

        await WaitBattleMilliseconds((int)waittingTime);
        currentState = BattleState.Start;

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
        battleTurn++;

        RelicManager.RunTyphoonCallingEye(battleTurn);

        // 준비 페이즈 이펙트 재생 (타임아웃 5초)
        await PlayPhaseEffect("Preparation");

        UpdateUnitUI();
        bool result = PreparationPhase();
        UpdateDodgeUI();
        await Task.Yield();
        return result;
    }

    //충돌 페이즈 관리
    private async Task<bool> HandleCrash()
    {
        PlaySE("se_Crash");

        // 충돌 전 이펙트 재생 (타임아웃 5초)
        await PlayPhaseEffect("Crash");

        // 기본 충돌 검/이동 연출은 피해 텍스트가 아니라 충돌 페이즈에서 한 번만 시작한다.
        autoBattleUI?.BeginCrashPhaseVisual();
        try
        {
            ChrashPhase();
        }
        finally
        {
            autoBattleUI?.EndCrashPhaseVisual();
        }

        await Task.Yield();
        return true;
    }

    //지원 페이즈 관리
    private async Task<bool> HandleSupport()
    {
        // 지원 전 이펙트 재생 (타임아웃 5초)
        await PlayPhaseEffect("Support");

        SupportPhase();
        bool commanderEffect = abilityManager.ProcessCommanderTurnEnd(battleTurn, myUnits, enemyUnits);
        if (commanderEffect)
            ResolveDeathsAndCheckFrontPairChanged();

        await Task.Yield();
        return true;
    }

    /// <summary>
    /// 페이즈별 이펙트 재생 (대기열 방식)
    /// </summary>
    /// <param name="phaseName">페이즈 이름 (Crash, Support, Preparation 등)</param>
    private async Task PlayPhaseEffect(string phaseName)
    {
        // EffectManager가 없으면 스킵
        if (effectManager == null)
            return;

        // 예제: Resources에서 이펙트 로드
        EffectCD effectCD = LoadEffectForPhase(phaseName);

        if (effectCD != null)
        {
            // 유닛 Transform 가져오기 (전열 기준)
            RectTransform targetTransform = autoBattleUI != null ? autoBattleUI.GetUnitCardTransform(0, false) : null; // 적 전열
            RectTransform casterTransform = autoBattleUI != null ? autoBattleUI.GetUnitCardTransform(0, true) : null;  // 아군 전열

            // 대기열에 이펙트 요청 (자동으로 순차 재생됨)
            effectManager.RequestEffect(
                effectCD,
                targetTransform,
                casterTransform,
                isTargetMyTeam: false, // 적군
                isCasterMyTeam: true   // 아군
            );

            // 대기열 처리 시간 확보 (이펙트 재생 시간만큼 대기)
            await WaitBattleMilliseconds((int)(effectCD.totalDuration * 1000));
        }
    }

    /// <summary>
    /// 페이즈별 이펙트 로드 (확장 가능)
    /// </summary>
    private EffectCD LoadEffectForPhase(string phaseName)
    {
        // Inspector에서 할당된 이펙트 반환
        switch (phaseName)
        {
            case "Preparation":
                return preparationEffect;
            case "Crash":
                return crashEffect;
            case "Support":
                return supportEffect;
            default:
                return null;
        }

        // 대안: Resources에서 동적 로드
        // return Resources.Load<EffectCD>($"EffectCD/ECD_{phaseName}");
    }

    /// <summary>
    /// 특정 능력에 대한 이펙트 재생 (대기열 방식)
    /// </summary>
    /// <param name="abilityName">능력 이름 (Charge, ThrowSpear, RangedAttack 등)</param>
    /// <param name="targetIndex">피격 유닛 인덱스 (기본값: 0)</param>
    /// <param name="casterIndex">시전 유닛 인덱스 (기본값: 0)</param>
    /// <param name="isTargetMyUnit">피격 유닛이 아군인지 (기본값: false)</param>
    /// <param name="isCasterMyUnit">시전 유닛이 아군인지 (기본값: true)</param>
    public void PlayAbilityEffect(
        string abilityName,
        int targetIndex = 0,
        int casterIndex = 0,
        bool isTargetMyUnit = false,
        bool isCasterMyUnit = true)
    {
        PlaySE(GetSEKeyByAbilityName(abilityName));

        if (effectManager == null)
        {
            Debug.LogWarning($"[AutoBattleManager] PlayAbilityEffect({abilityName}): effectManager가 null입니다.");
            return;
        }

        // Resources에서 능력별 이펙트 로드
        string path = $"EffectCD/ECD_{abilityName}";
        EffectCD abilityEffect = Resources.Load<EffectCD>(path);

        Debug.Log($"[AutoBattleManager] PlayAbilityEffect: name={abilityName} | path={path} | loaded={abilityEffect != null}");

        if (abilityEffect != null)
        {
            // 유닛 Transform 가져오기
            RectTransform targetTransform = autoBattleUI != null ? autoBattleUI.GetUnitCardTransform(targetIndex, isTargetMyUnit) : null;
            RectTransform casterTransform = autoBattleUI != null ? autoBattleUI.GetUnitCardTransform(casterIndex, isCasterMyUnit) : null;

            Debug.Log($"[AutoBattleManager] targetTransform={targetTransform?.name ?? "null"} | casterTransform={casterTransform?.name ?? "null"}");

            // 대기열에 이펙트 요청 (자동으로 순차 재생됨)
            effectManager.RequestEffect(
                abilityEffect,
                targetTransform,
                casterTransform,
                isTargetMyUnit,
                isCasterMyUnit
            );
        }
        else
        {
            Debug.LogWarning($"[AutoBattleManager] 이펙트를 찾을 수 없습니다: Resources/{path}");
        }
    }

    // 사용처: 능력 이펙트 이름에 대응되는 전투 효과음 키를 반환
    private string GetSEKeyByAbilityName(string abilityName)
    {
        switch (abilityName)
        {
            case "F05_Storm":
                return "se_Storm";

            case "S01_Charge":
            case "S01_Charge_strongCharge":
                return "se_Charge";

            case "S02_Defense":
                return "se_Defense";

            case "S03_Guard":
                return "se_Guard";

            case "S04_Guerrilla":
                return "se_Guerrilla";

            case "S06_Assassination":
                return "se_Assassination";

            case "S07_Drain":
                return "se_Drain";

            case "S08_Overwhelm":
                return "se_Overwhelm";

            case "S09_Martyrdom":
                return "se_Martyrdom";

            case "S10_Wounding":
                return "se_Wounding";

            case "S11_Vengeance":
                return "se_Vengeance";

            case "S12_Counter":
                return "se_Counter";

            case "S13_FirstStrike":
                return "se_FirstStrike";

            case "S14_Challenge":
                return "se_Challenge";

            case "S15_SmokeScreen":
                return "se_Smoke";

            case "T05_Pierce":
                return "se_Pierce";

            case "T09_Slaughter":
                return "se_Slaughter";

            case "T12_Suppression":
                return "se_Suppression";

            case "T13_Plunder":
                return "se_Plunder";

            case "T15_Scorching":
                return "se_Scorching";

            case "T16_Thorns":
                return "se_Thorns";

            case "T18_Impact":
                return "se_Impact";

            case "T19_Healing":
                return "se_Healing";

            case "T20_LifeDrain":
                return "se_LifeDrain";

            default:
                return null;
        }
    }

    // 사용처: 효과음 매니저가 존재할 때만 지정 key의 효과음을 재생
    private void PlaySE(string seKey)
    {
        if (string.IsNullOrWhiteSpace(seKey))
            return;

        BGMManager.Instance?.PlaySE(seKey);
    }

    /// <summary>
    /// 모든 이펙트 취소 (전투 종료 시)
    /// </summary>
    public void CancelAllEffects()
    {
        effectManager?.CancelAllEffects();
    }
    //종료 확인
    private int CheckEnd()
    {
        if (myUnits.Count == 0 || enemyUnits.Count == 0)
        {
            if (enemyUnits.Count == 0 && myUnits.Count > 0)
            {
                return 0;
            }
            else if (enemyUnits.Count > 0 && myUnits.Count == 0)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }

        return 3;
    }

    // 사용처: 설정창 등 외부 UI가 전투 진행을 일시정지/재개할 때 사용한다.
    public void SetBattlePaused(bool paused)
    {
        isBattlePaused = paused;
    }

    // 사용처: AutoBattleManager의 비동기 전투 대기시간을 설정창 일시정지 상태에 맞춰 대기한다.
    private async Task WaitBattleMilliseconds(int milliseconds)
    {
        if (milliseconds <= 0)
        {
            await WaitWhileBattlePaused();
            return;
        }

        float remainingSeconds = milliseconds * 0.001f;

        while (remainingSeconds > 0f)
        {
            if (IsManagerInvalid())
                return;

            if (!isBattlePaused && Time.timeScale > 0f)
                remainingSeconds -= Time.unscaledDeltaTime;

            await Task.Yield();
        }
    }

    // 사용처: 대기시간이 없는 구간에서도 설정창이 열린 상태라면 다음 전투 처리로 넘어가지 않게 막는다.
    private async Task WaitWhileBattlePaused()
    {
        while (!IsManagerInvalid() && (isBattlePaused || Time.timeScale == 0f))
        {
            await Task.Yield();
        }
    }


    //속도 관리
    public void ChangeWaittingTime(float multiple)
    {
        waittingTime = BaseWaittingTime * Mathf.Max(0.01f, multiple);
    }

    //종료관리 전투가 끝났을때 나오게 될것들
    private bool HandleEnd(bool isMyUnitExist = true)
    {
        if (IsManagerInvalid())
            return true;

        if (isBattleEnding)
            return true;

        int result = isMyUnitExist ? CheckEnd() : 1;

        if (result == 3)
        {
            currentState = BattleState.Preparation;
            return false;
        }

        isBattleEnding = true;

        CancelAllEffects();

        RelicManager.ResetBattleOnceRelic();
        currentState = BattleState.End;

        if (enemyDeathUnits != null)
        {
            foreach (var unit in enemyDeathUnits)
            {
                RogueLikeData.Instance.AddScore((int)unit.maxHealth);
            }
        }

        int gameResult = RewardManager.AddBattleRewardByStage(
            result,
            myUnits ?? new List<RogueUnitDataBase>(),
            myDeathUnits ?? new List<RogueUnitDataBase>(),
            enemyDeathUnits ?? new List<RogueUnitDataBase>());

        if (IsManagerInvalid())
            return true;

        if (gameResult == 1)
        {
            PlaySE("se_Defeat");
            if (autoBattleUI != null)
                autoBattleUI.GameEnd(false);
            return true;
        }
        else if (gameResult == 2)
        {
            PlaySE("se_Victory");
            if (autoBattleUI != null)
                autoBattleUI.GameEnd(true);
            return true;
        }

        PlaySE(result == 0 ? "se_Win" : "se_Lose");

        UpdateUnitCount();
        UpdateUnitHp();

        if (IsManagerInvalid())
            return true;

        if (!gameObject.activeInHierarchy || !isActiveAndEnabled)
            return true;

        delayedBattleEndCoroutine = StartCoroutine(DelayedBattleEnd(result));

        return true;
    }

    private IEnumerator DelayedBattleEnd(int result)
    {
        yield return new WaitForSeconds(waittingTime * 0.001f);

        if (IsManagerInvalid())
            yield break;

        if (autoBattleUI != null)
            autoBattleUI.FightEnd();

        if (RogueLikeData.Instance == null)
            yield break;

        RogueLikeData.Instance.SetFieldId(0);
        RogueLikeData.Instance.ClearBuffDeBuff();
        RogueLikeData.Instance.SetBattleUnitCount(0);

        WarRelic relic = RogueLikeData.Instance.GetOwnedRelicById(109);
        if (relic != null)
            relic.Execute();

        if (RelicManager.CheckRelicById(78) && result == 0)
        {
            RogueLikeData.Instance.AddSariStack(3);
        }

        SaveData saveData = new SaveData();
        List<RogueUnitDataBase> trackedMyUnits = BuildBattleEndTrackedMyUnits(
            myUnits,
            myDeathUnits,
            enemyUnits,
            enemyDeathUnits,
            RogueLikeData.Instance.GetMyTeam());
        saveData.SaveDataBattaleEnd(trackedMyUnits, myDeathUnits);
    }

    // 벨페고르처럼 플레이어 소유 유닛이 적 진영으로 이동해도 전투 참가에 따른
    // 기력 변화는 전투 종료 저장에 반영되어야 한다. 소유권은 UniqueId로 판별한다.
    private static List<RogueUnitDataBase> BuildBattleEndTrackedMyUnits(
        List<RogueUnitDataBase> livingMyUnits,
        List<RogueUnitDataBase> deadMyUnits,
        List<RogueUnitDataBase> livingEnemyUnits,
        List<RogueUnitDataBase> deadEnemyUnits,
        List<RogueUnitDataBase> ownedRoster)
    {
        var result = livingMyUnits != null
            ? new List<RogueUnitDataBase>(livingMyUnits)
            : new List<RogueUnitDataBase>();

        if (ownedRoster == null || ownedRoster.Count == 0)
            return result;

        var ownedIds = new HashSet<int>();
        for (int i = 0; i < ownedRoster.Count; i++)
        {
            RogueUnitDataBase unit = ownedRoster[i];
            if (unit != null && unit.UniqueId >= 0)
                ownedIds.Add(unit.UniqueId);
        }

        var trackedIds = new HashSet<int>();
        AddUnitIds(trackedIds, livingMyUnits);
        AddUnitIds(trackedIds, deadMyUnits);
        AddOwnedEnemyUnits(result, trackedIds, ownedIds, livingEnemyUnits);
        AddOwnedEnemyUnits(result, trackedIds, ownedIds, deadEnemyUnits);
        return result;
    }

    private static void AddUnitIds(HashSet<int> ids, List<RogueUnitDataBase> units)
    {
        if (units == null)
            return;

        for (int i = 0; i < units.Count; i++)
        {
            RogueUnitDataBase unit = units[i];
            if (unit != null && unit.UniqueId >= 0)
                ids.Add(unit.UniqueId);
        }
    }

    private static void AddOwnedEnemyUnits(
        List<RogueUnitDataBase> result,
        HashSet<int> trackedIds,
        HashSet<int> ownedIds,
        List<RogueUnitDataBase> enemySideUnits)
    {
        if (enemySideUnits == null)
            return;

        for (int i = 0; i < enemySideUnits.Count; i++)
        {
            RogueUnitDataBase unit = enemySideUnits[i];
            if (unit == null || !ownedIds.Contains(unit.UniqueId) || !trackedIds.Add(unit.UniqueId))
                continue;

            result.Add(unit);
        }
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
        RelicManager.RunBattleSetupRelic();
        if (myUnits != null)
        {
            foreach (var unit in myUnits)
            {
                if (unit != null)
                    unit.ApplyModifiers(true);
            }
        }

        //유산 이미지 생성
        if (autoBattleUI != null)
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
        if (autoBattleUI == null)
            return;

        var data = BuildHpViewData();
        autoBattleUI.ApplyHp(data, myUnits, enemyUnits);
        UpdateDodgeUI();
    }

    // 도메인 규칙(예: 2번 유닛은 체력 0 이하면 숨김)을 적용한 뷰 스냅샷 생성
    private HpViewData BuildHpViewData()
    {
        var my0 = myUnits != null && myUnits.Count > 0 ? myUnits[0] : null;
        var en0 = enemyUnits != null && enemyUnits.Count > 0 ? enemyUnits[0] : null;

        bool myActive = my0 != null;
        bool enActive = en0 != null;

        int myHp = myActive ? ClampNonNegToInt(my0.health) : 0;
        int myMax = myActive ? ClampNonNegToInt(my0.maxHealth) : 1;
        int enHp = enActive ? ClampNonNegToInt(en0.health) : 0;
        int enMax = enActive ? ClampNonNegToInt(en0.maxHealth) : 1;

        var my1 = myUnits != null && myUnits.Count > 1 ? myUnits[1] : null;
        var en1 = enemyUnits != null && enemyUnits.Count > 1 ? enemyUnits[1] : null;

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

    //채크페이즈 시 발동 유산
    private void CheckPhaseRelic()
    {
        RelicManager.RunCheckPhaseRelic();
    }

    private void ResetData()
    {
        myUnits = new();
        enemyUnits = new();
        myDeathUnits = new();
        enemyDeathUnits = new();
        myFrontUnit = null;
        enemyFrontUnit = null;

        isFirstAttack = true;
        battleTurn = 0;

        isProcessing = false;

        isTest = false;
        currentState = BattleState.None;

        isDestroyed = false;
        isBattleEnding = false;
        pendingDeathVisuals.Clear();
        delayedBattleEndCoroutine = null;

    }
    private bool ResolveAllDeaths()
    {
        bool anyDied = false;

        // 무한 루프 방지용 가드 (연쇄가 길어도 안정적으로 빠짐)
        int guard = 64;

        while (guard-- > 0)
        {
            // abilityManager.ProcessDeath가 true면 이번 턴에 사망이 발생했다는 뜻
            bool diedThisStep = abilityManager.ProcessDeath(
                ref myUnits, ref enemyUnits, ref myDeathUnits, ref enemyDeathUnits,
                ref isFirstAttack, myFrontUnit, enemyFrontUnit);

            if (!diedThisStep)
                break;

            anyDied = true;

            // 리스트 변동(전열 사망 등)로 인해 전열 참조가 바뀔 수 있으니 매 스텝 갱신
            myFrontUnit = (myUnits.Count > 0) ? myUnits[0] : null;
            enemyFrontUnit = (enemyUnits.Count > 0) ? enemyUnits[0] : null;
        }
        return anyDied;
    }

    // 사용처: AbilityManager에서 사망한 유닛의 UI 페이드를 즉시 실행하지 않고 예약한다.
    public void QueueDeathVisual(RogueUnitDataBase unit, int unitIndex, bool isMyUnit)
    {
        if (unit == null)
            return;

        for (int i = 0; i < pendingDeathVisuals.Count; i++)
        {
            if (ReferenceEquals(pendingDeathVisuals[i].Unit, unit))
                return;
        }

        pendingDeathVisuals.Add(new DeathVisualRequest(unit, unitIndex, isMyUnit));
    }

    // 사용처: 페이즈 애니메이션이 끝난 뒤 예약된 사망 유닛 페이드를 실행한다.
    private bool PlayPendingDeathVisuals()
    {
        if (pendingDeathVisuals.Count == 0)
            return false;

        for (int i = 0; i < pendingDeathVisuals.Count; i++)
        {
            DeathVisualRequest request = pendingDeathVisuals[i];
            if (autoBattleUI != null)
                autoBattleUI.ChangeInvisibleUnit(request.Unit, request.UnitIndex, request.IsMyUnit);
        }

        pendingDeathVisuals.Clear();
        return true;
    }

    // 사용처: async/await 이후 오브젝트가 파괴된 상태인지 확인한다.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsManagerInvalid()
    {
        return isDestroyed || this == null;
    }
}


