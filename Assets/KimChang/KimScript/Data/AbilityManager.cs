using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AbilityManager
{
    private float heavyArmorValue = 15.0f;
    private float myBluntWeaponValue = 0f;
    private float enemyBluntWeaponValue = 0f;
    private float throwSpearValue = 50.0f;
    private int overwhelmValue = 1;
    private float strongChargeValue = 0.5f;
    private float defenseValue = 15.0f;
    private float slaughterValue = 10.0f;
    private float assassinationValue = 2.0f;
    private float drainHealValue = 20.0f;
    private float drainGainAttackValue = 10.0f;
    private float suppressionValue = 1.1f;
    private float thornsDamageValue = 10.0f;
    private float fireDamageValue = 0.05f;
    private float bloodSuckingValue = 0.2f;
    private float martyrdomValue = 1.2f;
    //private float mybindingHealth = 15;
    //private float eneymybindingHealth = 15;
    private float mybindingAttackDamage = 5;
    private float enemybindingAttackDamage = 5;
    private int plunderGold = 20;
    private readonly HashSet<RogueUnitDataBase> rangedAttackDeathCandidates = new();
    private readonly HashSet<RogueUnitDataBase> lennonWarriorDamageTargets = new();
    private readonly Func<float> commanderRandomValue;
    private readonly Func<int, int, int> commanderRandomRange;
    private readonly Func<int> commanderIdProvider;
    private int cachedCommanderId;
    private bool hasCachedCommanderId;
    private int grondalAttackStacks;
    private int ortheonNextExecutionTurn = 4;
    private int lastCommanderTurnProcessed;
    private AutoBattleManager autoBattleManager; // 통로 추가

    private AutoBattleUI autoBattleUI;
    private List<RogueUnitDataBase> currentMyUnits;
    private List<RogueUnitDataBase> currentEnemyUnits;

    Dictionary<int, List<RogueUnitDataBase>> myHeroUnits = new();
    Dictionary<int, List<RogueUnitDataBase>> enemyHeroUnits = new();

    // 사용처: 전투 계산 중 판정형 효과음이 필요할 때 BGMManager를 통해 재생
    private void PlaySE(string seKey)
    {
        if (string.IsNullOrWhiteSpace(seKey))
            return;

        BGMManager.Instance?.PlaySE(seKey);
    }

    public AbilityManager()
        : this(null, null, null)
    {
    }

    public AbilityManager(
        Func<float> randomValue,
        Func<int, int, int> randomRange,
        Func<int> currentCommanderId = null)
    {
        commanderRandomValue = randomValue;
        commanderRandomRange = randomRange;
        commanderIdProvider = currentCommanderId;
    }

    private int GetCurrentCommanderId()
    {
        if (hasCachedCommanderId)
            return cachedCommanderId;

        cachedCommanderId = commanderIdProvider != null
            ? commanderIdProvider()
            : RogueLikeData.Instance?.GetCurrentCommanderId() ?? 0;
        hasCachedCommanderId = true;
        return cachedCommanderId;
    }

    private void RefreshCurrentCommanderId()
    {
        hasCachedCommanderId = false;
        GetCurrentCommanderId();
    }

    private bool IsCommander(int commanderId)
    {
        return GetCurrentCommanderId() == commanderId;
    }

    private float NextCommanderRandomValue()
    {
        if (commanderRandomValue != null)
            return commanderRandomValue();

        return RogueLikeData.Instance != null ? RogueLikeData.Instance.GetRandomFloat() : UnityEngine.Random.value;
    }

    private int NextCommanderRandomRange(int minInclusive, int maxExclusive)
    {
        if (commanderRandomRange != null)
            return commanderRandomRange(minInclusive, maxExclusive);

        return RogueLikeData.Instance != null
            ? RogueLikeData.Instance.GetRandomInt(minInclusive, maxExclusive)
            : UnityEngine.Random.Range(minInclusive, maxExclusive);
    }

    private bool ShouldTriggerCommanderChance(
        int commanderId,
        float probability,
        bool sourceIsMyTeam,
        bool enemyOnly)
    {
        if (!IsCommander(commanderId) || (enemyOnly && sourceIsMyTeam))
            return false;

        return NextCommanderRandomValue() < probability;
    }

    private static bool IsLightCavalryOrAssassin(RogueUnitDataBase unit)
    {
        return unit != null && (unit.branchIdx == 5 || unit.branchIdx == 4);
    }

    private static bool IsCavalry(RogueUnitDataBase unit)
    {
        return unit != null && (unit.branchIdx == 5 || unit.branchIdx == 6);
    }

    private bool IsEnemyUnit(RogueUnitDataBase unit)
    {
        if (unit == null)
            return false;

        if (currentEnemyUnits != null && currentEnemyUnits.Contains(unit))
            return true;

        if (RogueLikeData.Instance == null)
            return false;

        var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();
        return enemyUnits != null && enemyUnits.Contains(unit);
    }

    private bool IsPlayerUnit(RogueUnitDataBase unit)
    {
        if (unit == null)
            return false;

        if (currentMyUnits != null && currentMyUnits.Contains(unit))
            return true;

        if (RogueLikeData.Instance == null)
            return false;

        var myUnits = RogueLikeData.Instance.GetMyUnits();
        return myUnits != null && myUnits.Contains(unit);
    }

    private void TrackBattleUnits(List<RogueUnitDataBase> attackers, List<RogueUnitDataBase> defenders, bool attackersAreMyTeam)
    {
        if (attackersAreMyTeam)
        {
            currentMyUnits = attackers;
            currentEnemyUnits = defenders;
        }
        else
        {
            currentEnemyUnits = attackers;
            currentMyUnits = defenders;
        }
    }

    private void TrackBattleUnits(List<RogueUnitDataBase> myUnits, List<RogueUnitDataBase> enemyUnits)
    {
        currentMyUnits = myUnits;
        currentEnemyUnits = enemyUnits;
    }

    public void ProcessCommenderEffect(
        List<RogueUnitDataBase> myUnits,
        List<RogueUnitDataBase> enemyUnits)
    {
        RefreshCurrentCommanderId();
        TrackBattleUnits(myUnits, enemyUnits);
        CommenderEffect.ApplyBattleStart(
            GetCurrentCommanderId(),
            myUnits,
            enemyUnits,
            NextCommanderRandomRange);

        RogueLikeData.Instance?.SetAllMyUnits(myUnits);
        RogueLikeData.Instance?.SetAllEnemyUnits(enemyUnits);
    }

    //입장 시 채크
    public void ProcessEnter()
    {
        RefreshCurrentCommanderId();
        rangedAttackDeathCandidates.Clear();
        lennonWarriorDamageTargets.Clear();
        grondalAttackStacks = 0;
        ortheonNextExecutionTurn = 4;
        lastCommanderTurnProcessed = 0;
        plunderGold = 20;

        StageType type = RogueLikeData.Instance.GetCurrentStageType();
        int morale = RogueLikeData.Instance.GetMorale();
        // 전투 스테이지 에너지 차감
        if (type == StageType.Combat || type == StageType.Elite || type == StageType.Boss)
            ReduceUnitEngery();

        // 보스
        if (type == StageType.Boss)
        {
            if (RelicManager.CheckRelicById(39))
            {
                WarRelic relic = RelicManager.GetRelicById(39);
                var vals = relic.GetAllValuesAsFloatListOrNull();
                if (vals != null)
                {
                    RogueLikeData.Instance.ChangeMorale((int)vals[0]);
                }
            }

        }
    }
    public void CalculateFieldEffect()
    {
        int fieldId = RogueLikeData.Instance.GetFieldId();
        TrackBattleUnits(RogueLikeData.Instance.GetMyUnits(), RogueLikeData.Instance.GetEnemyUnits());

        switch (fieldId)
        {
            case 1:
                {
                    var allUnits = new List<RogueUnitDataBase>();
                    var myUnits = RogueLikeData.Instance.GetMyUnits();
                    var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();
                    if (myUnits != null) allUnits.AddRange(myUnits);
                    if (enemyUnits != null) allUnits.AddRange(enemyUnits);
                    foreach (var unit in allUnits)
                    {
                        if (unit == null || unit.stats == null)
                            continue;

                        unit.stats.AddModifier(new StatModifier
                        {
                            stat = StatType.Armor,
                            value = 1,
                            source = SourceType.Field,
                            modifierId = fieldId,
                            isPercent = false
                        });
                        unit.Armor++;
                    }
                    break;
                }

            case 2:
                {
                    int id = 9, type = 1, rank = 1, duration = -1;
                    var allUnits = new List<RogueUnitDataBase>();
                    var myUnits = RogueLikeData.Instance.GetMyUnits();
                    var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();
                    if (myUnits != null) allUnits.AddRange(myUnits);
                    if (enemyUnits != null) allUnits.AddRange(enemyUnits);
                    foreach (var unit in allUnits)
                    {
                        if (unit == null || unit.stats == null || unit.effectDictionary == null)
                            continue;

                        unit.stats.AddModifier(new StatModifier
                        {
                            stat = StatType.Mobility,
                            value = -2,
                            source = SourceType.Field,
                            modifierId = fieldId,
                            isPercent = false
                        });
                        unit.effectDictionary[id] = new BuffDebuffData(id, type, rank, duration);
                    }
                    break;
                }
            case 3:
                {
                    int id = 10, type = 0, rank = 1, duration = -1;
                    var allUnits = new List<RogueUnitDataBase>();
                    var myUnits = RogueLikeData.Instance.GetMyUnits();
                    var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();
                    if (myUnits != null) allUnits.AddRange(myUnits);
                    if (enemyUnits != null) allUnits.AddRange(enemyUnits);
                    foreach (var unit in allUnits)
                    {
                        if (unit == null || unit.stats == null || unit.effectDictionary == null)
                            continue;

                        if (unit.lightArmor) unit.effectDictionary[id] = new BuffDebuffData(id, type, rank, duration);
                        if (unit.rangedAttack)
                        {
                            unit.stats.AddModifier(new StatModifier
                            {
                                stat = StatType.AttackDamage,
                                value = -unit.baseAttackDamage * 0.1f,
                                source = SourceType.Field,
                                modifierId = fieldId,
                                isPercent = false
                            });
                        }
                    }

                    break;
                }
        }

    }

    public bool ProcessOneTurn()
    {
        bool isTurnEffect;
        isTurnEffect = CalculateStromMap();

        return isTurnEffect;
    }

    public bool ProcessCommanderTurnEnd(
        int battleTurn,
        List<RogueUnitDataBase> myUnits,
        List<RogueUnitDataBase> enemyUnits)
    {
        if (battleTurn <= 0 || battleTurn <= lastCommanderTurnProcessed)
            return false;

        lastCommanderTurnProcessed = battleTurn;
        TrackBattleUnits(myUnits, enemyUnits);

        switch (GetCurrentCommanderId())
        {
            case 201:
                return ApplyBrunoTurnDamage(myUnits);

            case 209:
                return ApplyOrtheonExecution(battleTurn, myUnits);

            case 213:
                if (battleTurn == 5)
                {
                    CommenderEffect.RemoveCommanderEffect(enemyUnits, 213);
                    return true;
                }
                break;

            case 216:
                if (battleTurn == 10)
                {
                    CommenderEffect.RemoveCommanderEffect(myUnits, 216);
                    CommenderEffect.RemoveCommanderEffect(enemyUnits, 216);
                    CommenderEffect.AddCommanderPercentAttack(enemyUnits, 216, 0.25f);
                    return true;
                }
                break;
        }

        return false;
    }

    private bool ApplyBrunoTurnDamage(List<RogueUnitDataBase> myUnits)
    {
        if (myUnits == null)
            return false;

        bool applied = false;
        for (int i = 0; i < myUnits.Count; i++)
        {
            RogueUnitDataBase unit = myUnits[i];
            if (unit == null || unit.health <= 0f)
                continue;

            unit.health -= 5f;
            ApplyCommanderDamageEffects(myUnits, i, 5f, true);
            CallDamageText(5f, "브루노 ", true, false, i);
            applied = true;
        }

        return applied;
    }

    private bool ApplyOrtheonExecution(int battleTurn, List<RogueUnitDataBase> myUnits)
    {
        if (battleTurn < ortheonNextExecutionTurn || myUnits == null || myUnits.Count == 0)
            return false;

        int livingCount = 0;
        for (int i = 0; i < myUnits.Count; i++)
        {
            if (myUnits[i] != null && myUnits[i].health > 0f)
                livingCount++;
        }

        if (livingCount == 0)
            return false;

        int selectedLivingIndex = Mathf.Clamp(
            NextCommanderRandomRange(0, livingCount),
            0,
            livingCount - 1);
        int targetIndex = -1;
        for (int i = 0; i < myUnits.Count; i++)
        {
            if (myUnits[i] == null || myUnits[i].health <= 0f)
                continue;

            if (selectedLivingIndex-- == 0)
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex < 0)
            return false;

        RogueUnitDataBase target = myUnits[targetIndex];
        float damage = target.health;
        target.health = 0f;
        ApplyCommanderDamageEffects(myUnits, targetIndex, damage, true);
        ortheonNextExecutionTurn = battleTurn + 4;

        autoBattleManager?.PlayAbilityEffect("S06_Assassination", targetIndex, 0, true, false);
        CallDamageText(damage, "오르테온 ", true, true, targetIndex);
        return true;
    }

    // 사용처: 전투 페이즈 진입 전 최소 전열 유닛 존재 여부 확인
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool HasFrontUnits(List<RogueUnitDataBase> attackers, List<RogueUnitDataBase> defenders)
    {
        return attackers != null && defenders != null && attackers.Count > 0 && defenders.Count > 0;
    }

    //폭풍우
    private bool CalculateStromMap()
    {
        int fieldId = RogueLikeData.Instance.GetFieldId();
        if (fieldId != 4)
            return false;

        var myUnits = RogueLikeData.Instance.GetMyUnits();
        var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();

        bool canHitMy = myUnits != null && myUnits.Count > 0;
        bool canHitEnemy = enemyUnits != null && enemyUnits.Count > 0;

        if (!canHitMy && !canHitEnemy)
            return false;

        bool isMyTeam = canHitMy && (!canHitEnemy || RogueLikeData.Instance.GetRandomInt(0, 2) == 0);
        List<RogueUnitDataBase> targetUnits = isMyTeam ? myUnits : enemyUnits;

        int randomIndex = RogueLikeData.Instance.GetRandomInt(0, targetUnits.Count);
        RogueUnitDataBase damagedUnit = targetUnits[randomIndex];

        if (damagedUnit == null)
            return false;

        if (autoBattleManager != null)
        {
            autoBattleManager.PlayAbilityEffect("F05_Storm", randomIndex, 0, isMyTeam, isMyTeam);
        }

        damagedUnit.health -= 30;
        ApplyCommanderDamageEffects(targetUnits, randomIndex, 30f, isMyTeam);
        CallDamageText(30, "폭풍우 ", isMyTeam, false, randomIndex);

        return true;
    }

    // 유닛 기력 감소
    private void ReduceUnitEngery()
    {
        if (RelicManager.CheckRelicById(63))
        {
            WarRelic relic = RelicManager.GetRelicById(63);
            List<float> vals = relic.GetAllValuesAsFloatListOrNull();

            if (vals != null && vals[0] > 0)
            {
                vals[0] -= 1f;

                string[] updated = vals.Select(v => v.ToString(CultureInfo.InvariantCulture)).ToArray();

                relic.SetValues(updated);

                return;
            }
        }
        var myUnits = RogueLikeData.Instance.GetMyUnits();

        for (int i = 0; i < myUnits.Count; i++)
        {
            int reduce = 1;
            int reduceMulty = 1;
            RogueUnitDataBase unit = myUnits[i];
            if (RelicManager.CheckRelicById(61))
            {
                if (StatBlock.HasModifier(unit.stats, SourceType.Relic, 61))

                    reduceMulty = 2;
            }
            if (unit.effectDictionary.ContainsKey(14))
            {
                reduce += 1;
            }

            unit.Energy -= reduce * reduceMulty;
        }
    }

    //전투 전 발동(패시브)
    public void ProcessBeforeBattle(List<RogueUnitDataBase> units, List<RogueUnitDataBase> defenders, bool isTeam, AutoBattleUI _autoBattleUI, AutoBattleManager _manager)
    {
        autoBattleUI = _autoBattleUI;
        autoBattleManager = _manager;

        if (units == null || defenders == null || units.Count == 0)
            return;

        if (isTeam)
            myHeroUnits.Clear();
        else
            enemyHeroUnits.Clear();

        //기타 유산
        if (isTeam && RelicManager.CheckRelicById(68))
        {
            WarRelic relic = RelicManager.GetRelicById(68);
            var vals = relic.GetAllValuesAsFloatListOrNull();
            if (vals != null)
            {
                plunderGold += (int)vals[0];
                ApplyLootBagPlunder(units, NextCommanderRandomRange);
            }
        }

        CheckHeroUnit(units, isTeam);

        //유닛 효과
        CalculateBloodPriest(units);
        CalculateCorpsCommander(isTeam);
        CalculateNomadicChief(units, isTeam);
        CalculateWanderer(isTeam);
        CalculateRebelLeader(units, isTeam);
        CalculateIndomitableShield(units, isTeam);
        CalculateBizarreBishop(units, isTeam);

        //시너지
        CalculateWarden(units);
        CalculateLongSwordMan(units);
        CalculateLongBowMan(units);
        CalculateSteelCastle(units);
        CalculateStrikeForce(units);
        CalculateBattleHammer(units, isTeam);
        CalculateEmpire(units);
        CalculateDivinityCountry(units);
        CalculateSevenUnion(units);

        //결속
        if (HasActiveBindingForce(units))
            PlaySE("se_BindingForce");

        CalculataeSolidarity(units, isTeam);

        foreach (RogueUnitDataBase unit in units)
        {
            unit.ApplyModifiers();
        }
    }
    // 사용처: 결속 효과음 재생 여부를 빠르게 판단
    private bool HasActiveBindingForce(List<RogueUnitDataBase> units)
    {
        if (units == null)
            return false;

        for (int i = 0; i < units.Count; i++)
        {
            RogueUnitDataBase unit = units[i];

            if (unit == null)
                continue;

            if (unit.bindingForce && unit.tagIdx != 0)
                return true;
        }

        return false;
    }

    //전투당 한번(선재 타격 등)
    public bool ProcessStartBattle(List<RogueUnitDataBase> attackers, List<RogueUnitDataBase> defenders, bool isTeam)
    {
        rangedAttackDeathCandidates.Clear();

        if (!HasFrontUnits(attackers, defenders))
            return false;

        TrackBattleUnits(attackers, defenders, isTeam);

        float finalDamage = SetMultipleDamage(attackers[0], defenders[0], isTeam);
        return (CalculateFirstStrike(attackers, defenders, finalDamage, isTeam) || CalculateManiac(defenders, isTeam));
    }

    //준비 페이즈 시 발동
    public bool ProcessPreparationAbility(List<RogueUnitDataBase> attackers, List<RogueUnitDataBase> defenders, bool isFirstAttack, bool isTeam)
    {
        if (!isFirstAttack || !HasFrontUnits(attackers, defenders))
            return false;

        TrackBattleUnits(attackers, defenders, isTeam);

        float _finalDamage = SetMultipleDamage(attackers[0], defenders[0], isTeam);
        RogueUnitDataBase frontAttacker = attackers[0];
        RogueUnitDataBase frontDefender = defenders[0];
        float finalDamage = _finalDamage + ((isTeam && RelicManager.CheckRelicById(46)) ? 1.2f : 1) - 1;
        string text = "";
        float damage = 0;

        var abilityActions = new List<Action>
{
    () => { if (frontAttacker.smokeScreen) CalculateSmokeScreen(attackers, isTeam); },
    () => { if (frontAttacker.overwhelm) CalculateOverwhelm(frontAttacker, frontDefender, ref text, isTeam); },
    () => { if (frontAttacker.throwSpear) CalculateThrowSpear(frontAttacker,attackers, defenders, ref damage, ref text,isTeam,isFirstAttack); },
    () => { if (frontAttacker.assassination) CalculateAssassination(frontAttacker,attackers, defenders, ref damage, ref text, isTeam, isFirstAttack); },
    () => { if (frontAttacker.wounding) CalculateWounding(frontAttacker, frontDefender, ref text, isTeam); }
};

        foreach (var action in abilityActions) action();

        if (damage > 0)
        {
            CallDamageText(damage, text, !isTeam, true);

            float relicDamage = RelicManager.RunPulsatingDoll(frontAttacker, isTeam);
            if (relicDamage > 0)
            {
                frontAttacker.health -= relicDamage;
                ApplyCommanderDamageEffects(attackers, 0, relicDamage, isTeam);
                CallDamageText(relicDamage, "맥동하는인형 ", !isTeam, true);
            }
        }

        return true;
    }

    //충돌 페이즈 시 발동
    public void ProcessChrashAbility(List<RogueUnitDataBase> attackers, List<RogueUnitDataBase> defenders, bool isFirstAttack, bool isTeam)
    {
        if (!HasFrontUnits(attackers, defenders))
            return;

        TrackBattleUnits(attackers, defenders, isTeam);

        float finalDamage = SetMultipleDamage(attackers[0], defenders[0], isTeam);
        float multiplier = 1;
        float reduceDamage = 0;
        RogueUnitDataBase frontAttacker = attackers[0];
        RogueUnitDataBase frontDefender = defenders[0];
        float allDamage = 0;
        string firstText = "충돌 ";
        bool isPierce = false;
        if (isFirstAttack)
        {
            ChrashIsFirstAttack(frontAttacker, defenders, ref multiplier, ref reduceDamage, ref firstText, isTeam, ref isPierce);
        }
        for (int i = 0; i < 2; i++)
        {
            string text = firstText;
            if (i == 1)
            {
                //연발
                if (!frontAttacker.doubleShot) break; // 한 번만 공격
                if (autoBattleManager != null)
                    autoBattleManager.PlayAbilityEffect("T14_DoubleShot", 0, 0, isTeam, isTeam);
                text += "연발 ";
            }
            (reduceDamage, text) = ApplyChrashAbility(frontAttacker, frontDefender, isTeam, reduceDamage, text);

            float damage = frontAttacker.attackDamage * multiplier;

            // 변경: 관통이면 방어력 보정 미적용. 보정식 괄호 수정.
            if (!(frontAttacker.pierce || isPierce))
            {
                float ar = frontDefender.Armor;
                damage *= 1f - (ar / (ar + 10f));
            }
            else if (i == 0)
            {
                text += "관통 ";
            }
            damage = (damage - reduceDamage) * finalDamage;

            float normalDamage = MathF.Round(damage);
            //충격
            if (isFirstAttack && frontAttacker.charge && frontAttacker.impact)
            {
                RogueUnitDataBase target = CalculateBackAttack(defenders);
                if (target != null)
                {
                    int unitIndex = defenders.IndexOf(target);
                    if (unitIndex < 0) return;

                    float ar = target.Armor;
                    float impactDamage = MathF.Round(normalDamage * (1f - (ar / (ar + 10f))));

                    impactDamage = ChangeBackMultiple(frontAttacker, target, impactDamage, isTeam);

                    //유산 127
                    float relicReduceDamage = impactDamage;
                    RelicManager.RunGuardiansCloak(defenders, !isTeam, ref unitIndex, ref relicReduceDamage);
                    target = defenders[unitIndex];

                    target.health -= relicReduceDamage;
                    ApplyCommanderDamageEffects(defenders, unitIndex, relicReduceDamage, !isTeam, frontAttacker);
                    TryApplyCommanderPerryBacklineDamage(defenders, relicReduceDamage, isTeam, unitIndex, frontAttacker);

                    if (autoBattleManager != null)
                        autoBattleManager.PlayAbilityEffect("T18_Impact", unitIndex, 0, !isTeam, isTeam);
                    CallDamageText(relicReduceDamage, "충격 ", !isTeam, true, unitIndex, frontAttacker);
                    TryRunSpectersCowlExecution(defenders, unitIndex, relicReduceDamage, isTeam);

                    //복수
                    if (frontDefender.vengeance && unitIndex > 0)
                    {
                        if (autoBattleManager != null)
                            autoBattleManager.PlayAbilityEffect("S11_Vengeance", 0, 0, !isTeam, !isTeam);

                        //유산 127
                        relicReduceDamage = impactDamage;
                        unitIndex = 0;
                        RelicManager.RunGuardiansCloak(attackers, isTeam, ref unitIndex, ref relicReduceDamage);
                        target = attackers[unitIndex];

                        target.health -= relicReduceDamage;
                        ApplyCommanderDamageEffects(attackers, unitIndex, relicReduceDamage, isTeam, frontDefender);
                        TryApplyCommanderPerryBacklineDamage(attackers, relicReduceDamage, !isTeam, unitIndex, frontDefender);

                        CallDamageText(relicReduceDamage, "복수 ", isTeam, true, unitIndex, frontDefender);

                        float relicDamage = RelicManager.RunPulsatingDoll(frontDefender, !isTeam);
                        if (relicDamage > 0)
                        {
                            //유산 127
                            relicReduceDamage = relicDamage;
                            unitIndex = 0;
                            RelicManager.RunGuardiansCloak(defenders, !isTeam, ref unitIndex, ref relicReduceDamage);
                            target = defenders[unitIndex];

                            target.health -= relicReduceDamage;
                            ApplyCommanderDamageEffects(defenders, unitIndex, relicReduceDamage, !isTeam);
                            CallDamageText(relicReduceDamage, "맥동하는 인형", isTeam, true, unitIndex);
                        }
                    }
                }
            }

            // 회피 판정
            if (CalculateAccuracy(frontDefender, frontAttacker, attackers, isTeam, isFirstAttack, 0))
            {
                normalDamage = 0;

                text = "회피 ";
            }
            else
            {
                //반격
                if (isFirstAttack && frontDefender.counter)
                {
                    if (!CalculateAccuracy(frontAttacker, frontDefender, defenders, isTeam, isFirstAttack, 0))
                    {
                        if (autoBattleManager != null)
                            autoBattleManager.PlayAbilityEffect("S12_Counter", 0, 0, !isTeam, !isTeam);

                        //유산 127
                        float relicReduceDamage = normalDamage;
                        int unitIndex = 0;
                        RelicManager.RunGuardiansCloak(attackers, isTeam, ref unitIndex, ref relicReduceDamage);
                        RogueUnitDataBase target = attackers[unitIndex];

                        target.health -= relicReduceDamage;
                        ApplyCommanderDamageEffects(attackers, unitIndex, relicReduceDamage, isTeam, frontDefender);
                        TryApplyCommanderPerryBacklineDamage(attackers, relicReduceDamage, !isTeam, unitIndex, frontDefender);

                        CallDamageText(relicReduceDamage, "반격 ", isTeam, true, unitIndex, frontDefender);

                        float relicDamage = RelicManager.RunPulsatingDoll(frontDefender, !isTeam);
                        if (relicDamage > 0)
                        {
                            //유산 127
                            relicReduceDamage = normalDamage;
                            unitIndex = 0;
                            RelicManager.RunGuardiansCloak(defenders, !isTeam, ref unitIndex, ref relicReduceDamage);
                            target = defenders[unitIndex];

                            target.health -= relicReduceDamage;
                            ApplyCommanderDamageEffects(defenders, unitIndex, relicReduceDamage, !isTeam);
                            CallDamageText(relicReduceDamage, "맥동하는 인형", isTeam, true, unitIndex);
                        }
                    }
                }
                else
                {
                    //유산 127
                    float relicReduceDamage = normalDamage;
                    int unitIndex = 0;
                    RelicManager.RunGuardiansCloak(defenders, !isTeam, ref unitIndex, ref relicReduceDamage);
                    RogueUnitDataBase target = defenders[unitIndex];

                    target.health -= relicReduceDamage;
                    ApplyCommanderDamageEffects(defenders, unitIndex, relicReduceDamage, !isTeam, frontAttacker);
                    TryApplyCommanderPerryBacklineDamage(defenders, relicReduceDamage, isTeam, unitIndex, frontAttacker);
                }

                //작열
                CalculateBurning(frontAttacker, defenders, isTeam, ref text);

                // 가시 피해
                if (frontDefender.thorns && normalDamage > 0)
                {
                    if (autoBattleManager != null)
                        autoBattleManager.PlayAbilityEffect("T16_Thorns", 0, 0, !isTeam, !isTeam);

                    //유산 127
                    float relicReduceDamage = normalDamage;
                    int unitIndex = 0;
                    RelicManager.RunGuardiansCloak(attackers, isTeam, ref unitIndex, ref relicReduceDamage);
                    RogueUnitDataBase target = attackers[unitIndex];

                    target.health -= relicReduceDamage;
                    ApplyCommanderDamageEffects(attackers, unitIndex, relicReduceDamage, isTeam, frontDefender);

                    CallDamageText(relicReduceDamage, "가시 ", isTeam, false, unitIndex, frontDefender);
                }

                // 흡혈
                if (frontAttacker.lifeDrain)
                {
                    if (autoBattleManager != null)
                        autoBattleManager.PlayAbilityEffect("T20_LifeDrain", 0, 0, false, false);

                    float healValue = Mathf.Round(normalDamage * bloodSuckingValue);
                    float heal = HealHealth(frontAttacker, Mathf.Min((frontAttacker.health + healValue), frontAttacker.maxHealth));

                    frontAttacker.health = heal;

                    CallDamageText(-healValue, "흡혈 ", isTeam, false);
                }

                //추적자
                CalculateTracker(frontAttacker, frontDefender);
            }

            allDamage += normalDamage;

            if (i == 0) firstText = text;
        }

        CallDamageText(allDamage, firstText, !isTeam, true, 0, frontAttacker);

        CalculateChallenge(frontAttacker, ref defenders, isTeam);

        float finalRelicDamage = RelicManager.RunPulsatingDoll(frontAttacker, isTeam);
        if (finalRelicDamage > 0)
        {
            //유산 127
            float relicReduceDamage = finalRelicDamage;
            int unitIndex = 0;
            RelicManager.RunGuardiansCloak(attackers, isTeam, ref unitIndex, ref relicReduceDamage);
            RogueUnitDataBase target = attackers[unitIndex];

            target.health -= relicReduceDamage;
            ApplyCommanderDamageEffects(attackers, unitIndex, relicReduceDamage, isTeam);
            CallDamageText(relicReduceDamage, "맥동하는 인형", isTeam, true, unitIndex);
        }
    }
    //지원 페이즈 시 발동
    public void ProcessSupportAbility(List<RogueUnitDataBase> attackers, List<RogueUnitDataBase> defenders, bool isTeam, bool isFirstAttack)
    {
        if (!HasFrontUnits(attackers, defenders))
            return;

        TrackBattleUnits(attackers, defenders, isTeam);

        float finalDamage = SetMultipleDamage(attackers[0], defenders[0], isTeam);
        //원거리 공격
        var value = CalculateRangeAttack(attackers, defenders, isTeam, finalDamage, isFirstAttack);
        if (value.damage > 0)
        {
            int unitIndex = 0;
            float damage = value.damage;

            //유산 127
            RelicManager.RunGuardiansCloak(defenders, !isTeam, ref unitIndex, ref damage);

            float beforeHealth = defenders[unitIndex].health;
            defenders[unitIndex].health -= damage;
            ApplyCommanderDamageEffects(defenders, unitIndex, damage, !isTeam, value.lastDamageSource);
            CallDamageText(Mathf.Round(damage), value.text, !isTeam, false, unitIndex);
            if (isTeam && beforeHealth > 0f && defenders[unitIndex].health <= 0f)
                rangedAttackDeathCandidates.Add(defenders[unitIndex]);

            TryApplyCommanderPerryBacklineDamage(defenders, damage, isTeam, unitIndex, value.lastDamageSource);
        }


        //치유
        ProcessHealing(attackers, isTeam);
        //지원 종료
        DamageBurning(attackers, isTeam);

    }

    // 유닛 사망 처리
    public bool ProcessDeath(
    ref List<RogueUnitDataBase> myUnits, ref List<RogueUnitDataBase> enemyUnits,
    ref List<RogueUnitDataBase> myDeathUnits, ref List<RogueUnitDataBase> enemyDeathUnits,
    ref bool isFirstAttack, RogueUnitDataBase myFrontUnit, RogueUnitDataBase enemyFrontUnit)
    {
        if (myUnits == null) myUnits = new List<RogueUnitDataBase>();
        if (enemyUnits == null) enemyUnits = new List<RogueUnitDataBase>();
        if (myDeathUnits == null) myDeathUnits = new List<RogueUnitDataBase>();
        if (enemyDeathUnits == null) enemyDeathUnits = new List<RogueUnitDataBase>();
        TrackBattleUnits(myUnits, enemyUnits);

        if (myUnits.Count == 0 || enemyUnits.Count == 0)
            return false;

        bool myUnitDied = false;
        bool enemyUnitDied = false;

        List<RogueUnitDataBase> tempMyDeathUnits = new();
        List<RogueUnitDataBase> tempEnemyDeathUnits = new();

        List<int> myDeathIndexes = new();
        List<int> enemyDeathIndexes = new();

        ApplyCommanderAxlExecution(myUnits, enemyUnits);

        // 내 유닛 사망 스캔
        for (int i = myUnits.Count - 1; i >= 0; i--)
        {
            if (myUnits[i].health <= 0)
            {
                // 사용처: 유물 27 전열 부활
                if (i == 0 && RelicManager.CheckRelicById(27))
                {
                    var relic = RelicManager.GetRelicById(27);
                    relic.used = true;
                    myUnits[i].health = myUnits[i].maxHealth;
                    continue;
                }

                if (ProcessUnitDeath(myUnits, i, tempMyDeathUnits, ref myUnitDied, autoBattleUI, true))
                    myDeathIndexes.Add(i);
            }
        }

        // 적 유닛 사망 스캔
        for (int i = enemyUnits.Count - 1; i >= 0; i--)
        {
            if (enemyUnits[i].health <= 0)
            {
                if (ProcessUnitDeath(enemyUnits, i, tempEnemyDeathUnits, ref enemyUnitDied, autoBattleUI, false))
                    enemyDeathIndexes.Add(i);
            }
        }

        for (int k = 0; k < myDeathIndexes.Count; k++) myUnits.RemoveAt(myDeathIndexes[k]);
        for (int k = 0; k < enemyDeathIndexes.Count; k++) enemyUnits.RemoveAt(enemyDeathIndexes[k]);

        ApplyCommanderSydSummons(myUnits, tempMyDeathUnits, myDeathIndexes, true);
        ApplyCommanderSydSummons(enemyUnits, tempEnemyDeathUnits, enemyDeathIndexes, false);


        if (tempEnemyDeathUnits.Count > 0 || tempMyDeathUnits.Count > 0)
        {
            OnUnitDeath(tempEnemyDeathUnits, tempMyDeathUnits, ref enemyUnits, false, enemyUnitDied, myUnitDied, isFirstAttack);
            OnUnitDeath(tempMyDeathUnits, tempEnemyDeathUnits, ref myUnits, true, myUnitDied, enemyUnitDied, isFirstAttack);
            ApplyCommanderDeathEffects(
                tempMyDeathUnits,
                tempEnemyDeathUnits,
                myUnits,
                enemyUnits,
                isFirstAttack);
            lennonWarriorDamageTargets.Clear();
        }

        // 사용처: 유물 54
        if (tempMyDeathUnits.Count > 0 && enemyUnits.Count > 0 && RelicManager.CheckRelicById(54))
        {
            var relic = RelicManager.GetRelicById(54);
            var vals = relic.GetAllValuesAsFloatListOrNull();
            if (vals != null)
            {
                float damage = 0f;
                for (int i = 0; i < tempMyDeathUnits.Count; i++)
                {
                    damage += Mathf.Max(0f, tempMyDeathUnits[i].maxHealth) * vals[0];
                }

                enemyUnits[0].health -= damage;

                if (enemyUnits[0].health <= 0)
                {
                    if (ProcessUnitDeath(enemyUnits, 0, tempEnemyDeathUnits, ref enemyUnitDied, autoBattleUI, false))
                        enemyUnits.RemoveAt(0);
                }
            }
        }

        // 사용처: 이번 틱 사망자들을 전투 전체 누적 로그에 합산(조건 없이 각각 추가)
        if (tempMyDeathUnits.Count > 0)
        {
            myDeathUnits.AddRange(tempMyDeathUnits);

            //유산 124
            if (enemyUnits.Count > 0)
            {
                RelicManager.RunPartingShot(tempMyDeathUnits, enemyUnits[0], myFrontUnit);
            }

        }
        if (tempEnemyDeathUnits.Count > 0)
        {
            enemyDeathUnits.AddRange(tempEnemyDeathUnits);

            //유산 105
            RelicManager.RunBloodSoakedDye(tempEnemyDeathUnits.Count);

            //유산 123
            int rangedDeathCount = ConsumeRangedAttackDeathCount(tempEnemyDeathUnits);
            if (rangedDeathCount > 0)
                RelicManager.RunEndlessBarrage(rangedDeathCount);

            //유산 129
            RelicManager.RunContractInvoice(tempEnemyDeathUnits, enemyFrontUnit);

        }
        //유산 104
        RelicManager.RunDoubleEdgedAxeOfPride(myUnits, myDeathUnits, enemyUnits, enemyDeathUnits);
        ApplyBattleStatModifiers(myUnits);
        ApplyBattleStatModifiers(enemyUnits);

        // 사용처: 이번 사망 정리에서 실제 사망자가 있었는지 저장한다.
        bool anyUnitDiedThisStep = tempMyDeathUnits.Count > 0 || tempEnemyDeathUnits.Count > 0;

        // 사용처: 전멸 체크
        if (myUnits.Count == 0 || enemyUnits.Count == 0)
            return anyUnitDiedThisStep;

        // 사용처: 전열 변경 체크(어느 쪽이든 전열이 바뀌었으면 스나이퍼 계산 및 선제권 리셋)
        if ((myUnitDied || enemyUnitDied) && (myFrontUnit != myUnits[0] || enemyFrontUnit != enemyUnits[0]))
        {
            if (myFrontUnit != myUnits[0]) CalculateSniper(myUnits);
            if (enemyFrontUnit != enemyUnits[0]) CalculateSniper(enemyUnits);

            isFirstAttack = true;
            return true;
        }

        // 사용처: 후열 사망만 있어도 연쇄 사망 처리를 위해 true를 반환한다.
        // 전열 변경 여부는 AutoBattleManager.ResolveDeathsAndCheckFrontPairChanged()에서 별도로 판정한다.
        return anyUnitDiedThisStep;
    }

    private int ConsumeRangedAttackDeathCount(List<RogueUnitDataBase> deadUnits)
    {
        if (deadUnits == null || deadUnits.Count == 0 || rangedAttackDeathCandidates.Count == 0)
            return 0;

        int count = 0;
        for (int i = 0; i < deadUnits.Count; i++)
        {
            RogueUnitDataBase unit = deadUnits[i];
            if (unit != null && rangedAttackDeathCandidates.Remove(unit))
                count++;
        }

        return count;
    }

    private void TryRunSpectersCowlExecution(List<RogueUnitDataBase> defenders, int unitIndex, float damage, bool isTeam)
    {
        if (!isTeam || damage <= 0f || defenders == null || unitIndex <= 0 || unitIndex >= defenders.Count)
            return;

        RogueUnitDataBase target = defenders[unitIndex];
        if (target == null || target.health <= 0f)
            return;

        if (!RelicManager.RunSpectersCowlExecution())
            return;

        float killDamage = target.health;
        target.health = 0f;
        CallDamageText(killDamage, "망령의 두건 ", !isTeam, true, unitIndex);
    }

    private void ApplyCommanderAxlExecution(List<RogueUnitDataBase> myUnits, List<RogueUnitDataBase> enemyUnits)
    {
        if (!IsCommander(110))
            return;

        MarkLowHealthUnitsForExecution(myUnits);
        MarkLowHealthUnitsForExecution(enemyUnits);
    }

    private static void MarkLowHealthUnitsForExecution(List<RogueUnitDataBase> units)
    {
        if (units == null)
            return;

        foreach (RogueUnitDataBase unit in units)
        {
            if (unit != null && unit.health > 0 && unit.health <= 50)
                unit.health = 0;
        }
    }

    private void ApplyCommanderSydSummons(List<RogueUnitDataBase> units, List<RogueUnitDataBase> deadUnits, List<int> deathIndexes, bool isMyTeam)
    {
        if (!IsCommander(119) || units == null || deadUnits == null || deathIndexes == null)
            return;

        var summonIndexes = new List<int>();
        int count = Mathf.Min(deadUnits.Count, deathIndexes.Count);
        for (int i = 0; i < count; i++)
        {
            RogueUnitDataBase deadUnit = deadUnits[i];
            if (deadUnit != null && deadUnit.branchIdx == 6)
                summonIndexes.Add(deathIndexes[i]);
        }

        if (summonIndexes.Count == 0)
            return;

        summonIndexes.Sort();
        foreach (int oldIndex in summonIndexes)
        {
            RogueUnitDataBase militia = UnitLoader.Instance.GetCloneUnitById(1, isMyTeam);
            if (militia == null)
                continue;

            int insertIndex = Mathf.Clamp(oldIndex, 0, units.Count);
            units.Insert(insertIndex, militia);
        }
    }

    private void ApplyCommanderDeathEffects(
        List<RogueUnitDataBase> deadMyUnits,
        List<RogueUnitDataBase> deadEnemyUnits,
        List<RogueUnitDataBase> currentMyUnits,
        List<RogueUnitDataBase> currentEnemyUnits,
        bool isFirstAttack)
    {
        int commanderId = GetCurrentCommanderId();
        if (commanderId == 0)
            return;

        int myDeathCount = deadMyUnits?.Count ?? 0;
        int enemyDeathCount = deadEnemyUnits?.Count ?? 0;

        if (commanderId == 102 && myDeathCount > 0)
        {
            int warriorKills = CountLennonWarriorKills(deadMyUnits);
            if (warriorKills > 0)
                RogueLikeData.Instance.ChangeMorale(-2 * warriorKills);
        }

        if (commanderId == 106 && myDeathCount > 0)
            RogueLikeData.Instance.ChangeMorale(-myDeathCount);

        if (commanderId == 108 && enemyDeathCount > 0)
            ApplyCommanderCobain(enemyDeathCount, currentMyUnits);

        if (commanderId == 117 && myDeathCount > 0)
        {
            int currentGold = RogueLikeData.Instance.GetCurrentGold();
            RogueLikeData.Instance.ReduceGold(Mathf.Min(currentGold, 10 * myDeathCount));
        }

        if (commanderId == 202 && enemyDeathCount > 0)
            ApplyCommanderSirionSupportFire(enemyDeathCount, currentMyUnits, currentEnemyUnits, isFirstAttack);

        if (commanderId == 204 && myDeathCount > 0)
            ApplyCommanderGrondal(myDeathCount, currentEnemyUnits);

        if (commanderId == 209 && enemyDeathCount > 0)
            ortheonNextExecutionTurn += enemyDeathCount;

        if (commanderId == 210 && enemyDeathCount > 0)
            ApplyCommanderAsmodeus(enemyDeathCount, currentMyUnits);
    }

    private int CountLennonWarriorKills(List<RogueUnitDataBase> deadMyUnits)
    {
        if (deadMyUnits == null || deadMyUnits.Count == 0)
            return 0;

        int count = 0;
        for (int i = 0; i < deadMyUnits.Count; i++)
        {
            if (deadMyUnits[i] != null && lennonWarriorDamageTargets.Contains(deadMyUnits[i]))
                count++;
        }

        return count;
    }

    // 전리품 주머니(68): 아군에 약탈 보유자가 있을 때 약탈이 없는 유닛 최대 둘에게 1회 부여한다.
    private static int ApplyLootBagPlunder(
        List<RogueUnitDataBase> units,
        Func<int, int, int> randomRange)
    {
        if (units == null || units.Count == 0)
            return 0;

        bool hasPlunder = false;
        int candidateCount = 0;
        for (int i = 0; i < units.Count; i++)
        {
            RogueUnitDataBase unit = units[i];
            if (unit == null)
                continue;

            if (unit.plunder)
                hasPlunder = true;
            else
                candidateCount++;
        }

        if (!hasPlunder || candidateCount == 0)
            return 0;

        int applied = 0;
        int targetCount = Mathf.Min(2, candidateCount);
        while (applied < targetCount)
        {
            int selectedCandidate = randomRange != null
                ? Mathf.Clamp(randomRange(0, candidateCount), 0, candidateCount - 1)
                : 0;

            for (int i = 0; i < units.Count; i++)
            {
                RogueUnitDataBase unit = units[i];
                if (unit == null || unit.plunder)
                    continue;

                if (selectedCandidate-- != 0)
                    continue;

                unit.plunder = true;
                candidateCount--;
                applied++;
                break;
            }
        }

        return applied;
    }

    private void ApplyCommanderCobain(int triggerCount, List<RogueUnitDataBase> myUnits)
    {
        if (triggerCount <= 0 || myUnits == null || myUnits.Count == 0)
            return;

        for (int trigger = 0; trigger < triggerCount; trigger++)
        {
            int targetIndex = -1;
            for (int i = myUnits.Count - 1; i >= 0; i--)
            {
                if (myUnits[i] != null && myUnits[i].health > 0)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex < 0)
                return;

            RogueUnitDataBase target = myUnits[targetIndex];
            target.health -= 20;
            target.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = -0.10f,
                source = SourceType.Commander,
                modifierId = 108,
                isPercent = true
            });
            target.ApplyModifiers(true);
            CallDamageText(20, "코베인 ", true, false, targetIndex);
        }
    }

    private void ApplyCommanderSirionSupportFire(
        int triggerCount,
        List<RogueUnitDataBase> myUnits,
        List<RogueUnitDataBase> enemyUnits,
        bool isFirstAttack)
    {
        if (triggerCount <= 0 || myUnits == null || enemyUnits == null)
            return;

        for (int trigger = 0; trigger < triggerCount; trigger++)
        {
            if (!HasFrontUnits(enemyUnits, myUnits))
                return;

            float finalDamage = SetMultipleDamage(enemyUnits[0], myUnits[0], false);
            var result = CalculateRangeAttack(enemyUnits, myUnits, false, finalDamage, isFirstAttack);
            if (result.damage <= 0f)
                continue;

            int targetIndex = 0;
            float damage = result.damage;
            RelicManager.RunGuardiansCloak(myUnits, true, ref targetIndex, ref damage);
            myUnits[targetIndex].health -= damage;
            ApplyCommanderDamageEffects(myUnits, targetIndex, damage, true, result.lastDamageSource);
            CallDamageText(Mathf.Round(damage), "시리온 지원사격 ", true, false, targetIndex);
        }
    }

    private void ApplyCommanderGrondal(int playerDeathCount, List<RogueUnitDataBase> enemyUnits)
    {
        grondalAttackStacks = Mathf.Min(5, grondalAttackStacks + playerDeathCount);
        CommenderEffect.RemoveCommanderEffect(enemyUnits, 204);
        CommenderEffect.AddCommanderPercentAttack(enemyUnits, 204, 0.10f * grondalAttackStacks);
    }

    private void ApplyCommanderAsmodeus(int enemyDeathCount, List<RogueUnitDataBase> myUnits)
    {
        if (myUnits == null)
            return;

        for (int death = 0; death < enemyDeathCount; death++)
        {
            for (int i = 0; i < myUnits.Count; i++)
            {
                RogueUnitDataBase unit = myUnits[i];
                if (unit == null || unit.health <= 0f || unit.stats == null)
                    continue;

                int previousArmor = unit.Armor;
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Armor,
                    value = -1f,
                    source = SourceType.Commander,
                    modifierId = 210,
                    isPercent = false
                });
                unit.ApplyModifiers(true);

                if (previousArmor > 0 && unit.Armor == 0)
                {
                    unit.health -= 30f;
                    CallDamageText(30f, "아스모데우스 ", true, false, i);
                }
            }
        }
    }
    // 사용처: 불사 버프가 있는 유닛은 사망 처리와 UI 페이드 예약 전에 체력을 복구한다.
    private bool TryConsumeImmortality(RogueUnitDataBase unit)
    {
        if (unit == null || unit.effectDictionary == null)
            return false;

        int id = 7;
        if (!unit.effectDictionary.ContainsKey(id))
            return false;

        unit.effectDictionary.Remove(id);
        unit.health = 10;
        unit.alive = true;
        return true;
    }

    // 개별 유닛 사망 처리
    private bool ProcessUnitDeath(
        List<RogueUnitDataBase> units, int index,
        List<RogueUnitDataBase> tempDeathUnits,
        ref bool unitDied, AutoBattleUI autoBattleUI, bool isMyUnit)
    {
        if (units == null || index < 0 || index >= units.Count)
            return false;

        RogueUnitDataBase deadUnit = units[index];

        if (TryConsumeImmortality(deadUnit))
            return false;

        CalculateMartyrdom(units, index, isMyUnit);

        deadUnit.alive = false;
        tempDeathUnits.Add(deadUnit); // 임시 리스트에 추가

        // 사용처: 사망 데이터는 즉시 처리하지만, 유닛 페이드는 페이즈 애니메이션 종료 후 AutoBattleManager에서 실행한다.
        if (autoBattleManager != null)
        {
            autoBattleManager.QueueDeathVisual(deadUnit, index, isMyUnit);
        }
        else if (autoBattleUI != null)
        {
            autoBattleUI.ChangeInvisibleUnit(deadUnit, index, isMyUnit);
        }

        if (index == 0) unitDied = true;
        return true;
    }


    // 유닛 사망 시 실행되는 함수 (추가 기능 확장 가능)
    private void OnUnitDeath(List<RogueUnitDataBase> deadAttackers, List<RogueUnitDataBase> deadDefenders, ref List<RogueUnitDataBase> attackers, bool isTeam, bool isFrontAttackerDead, bool isFrontDefendrDead, bool isFirstAttack)
    {
        if (attackers == null || attackers.Count == 0) return;
        RogueUnitDataBase frontAttacker = attackers[0];

        //봉인 풀린 자 채크
        CalculateTheUnsealedOne(deadDefenders.Count, isTeam);
        //불굴의 방패
        CalculateIndomitableShieldDead(attackers, deadAttackers, isTeam);
        //기괴한 주교의 불사
        CalculateImmortality(ref attackers, deadAttackers);

        //사리유산
        CalculateSariRelic(attackers, isTeam, isFrontDefendrDead);
        //결속
        CalculataeSolidarity(attackers, isTeam, true);
        //넝마떼기
        RelicManager.SurvivorOfRag(attackers, isTeam);

        //스킬 사용 유닛이 안죽었을 시
        if (!isFrontAttackerDead)
        {
            //유격
            if ((attackers[0].guerrilla || (attackers[0].effectDictionary.ContainsKey(12) && attackers[0].effectDictionary[12].Duration > 0)) && CheckBackUnit(attackers))
            {
                // 유격 이펙트: 위치가 바뀌기 전 해당 캐릭터 초상화 상단 (CasterTop)
                if (autoBattleManager != null)
                    autoBattleManager.PlayAbilityEffect("S04_Guerrilla", 0, 0, isTeam, isTeam);

                //1횟성 유격 유산
                if (attackers[0].effectDictionary.ContainsKey(12))
                {
                    attackers[0].effectDictionary[12].Duration--;
                }
                (attackers[1], attackers[0]) = (attackers[0], attackers[1]);

                CallDamageText(0, "유격", isTeam, false);
            }
            //착취
            if (frontAttacker.drain)
            {
                if (autoBattleManager != null)
                    autoBattleManager.PlayAbilityEffect("S07_Drain", 0, 0, isTeam, isTeam);
                //상흔 확인
                float heal = HealHealth(frontAttacker, drainHealValue);
                //착취 계산
                frontAttacker.health = MathF.Min(frontAttacker.maxHealth, heal + frontAttacker.health);
                frontAttacker.attackDamage += drainGainAttackValue;
            }
            if (isFrontDefendrDead)
            {
                //돌격대장
                CalculateAssaultLeader(attackers, isTeam);
                //노인기사
                CalculateOldKnight(frontAttacker, isFrontDefendrDead);
                //약탈
                CalculatePlunder(frontAttacker, isTeam);
            }
        }
        if (isFrontDefendrDead)
        {
            //무한
            CalculateEndLess(frontAttacker, isTeam);
        }

        ApplyBattleStatModifiers(attackers);
    }
    private static void ApplyBattleStatModifiers(List<RogueUnitDataBase> units)
    {
        if (units == null)
            return;

        foreach (RogueUnitDataBase unit in units)
        {
            if (unit == null || unit.stats == null || unit.health <= 0)
                continue;

            unit.ApplyModifiers(true);
        }
    }

    //선제 타격
    private bool CalculateFirstStrike(List<RogueUnitDataBase> attakers, List<RogueUnitDataBase> defenders, float finalDamage, bool isTeam)
    {
        bool use = false;
        foreach (RogueUnitDataBase attacker in attakers)
        {
            if ((attacker.firstStrike || (attacker.effectDictionary.ContainsKey(13) && attacker.effectDictionary[13].Duration > 0)) && !attacker.fStriked && CheckBackUnit(defenders))
            {
                if (attacker.effectDictionary.ContainsKey(13) && attacker.effectDictionary[13].Duration > 0)
                {
                    for (int i = attacker.effectDictionary[13].Duration; i > 0; i--)
                    {
                        CalculateDamageFirstStrike(attacker, defenders, isTeam, ref use);
                    }
                    attacker.effectDictionary[13].Duration = 0;
                }
                else
                {
                    CalculateDamageFirstStrike(attacker, defenders, isTeam, ref use);
                }
            }
        }

        use |= ExecuteAssassinationLeaderStrike(isTeam, defenders, finalDamage);
        return use;
    }
    //선제타격 데미지 계산
    private void CalculateDamageFirstStrike(RogueUnitDataBase attacker, List<RogueUnitDataBase> defenders, bool isTeam, ref bool use)
    {
        RogueUnitDataBase target = CalculateMinHealthBackAttack(defenders);
        if (target == null) return;

        int unitIndex = defenders.IndexOf(target);
        if (unitIndex < 0) return;

        if (autoBattleManager != null)
            autoBattleManager.PlayAbilityEffect("S13_FirstStrike", unitIndex, 0, !isTeam, isTeam);

        float damage = attacker.attackDamage * 2f;
        damage = ChangeBackMultiple(attacker, target, damage, isTeam);
        damage = RelicManager.RunSpectersCowlDamagePenalty(damage, isTeam);

        // 유산 127
        RelicManager.RunGuardiansCloak(defenders, !isTeam, ref unitIndex, ref damage);
        target = defenders[unitIndex];

        damage = MathF.Round(damage);
        target.health -= damage;
        ApplyCommanderDamageEffects(defenders, unitIndex, damage, !isTeam, attacker);
        TryApplyCommanderPerryBacklineDamage(defenders, damage, isTeam, unitIndex, attacker);
        TryRunSpectersCowlExecution(defenders, unitIndex, damage, isTeam);

        CallDamageText(damage, "선제타격 ", !isTeam, false, unitIndex, attacker);

        float relicDamage = RelicManager.RunPulsatingDoll(attacker, isTeam);
        if (relicDamage > 0)
        {
            attacker.health -= relicDamage;
            List<RogueUnitDataBase> attackerTeam = isTeam ? currentMyUnits : currentEnemyUnits;
            ApplyCommanderDamageEffects(attackerTeam, attackerTeam?.IndexOf(attacker) ?? -1, relicDamage, isTeam);
            CallDamageText(relicDamage, "맥동하는 인형", !isTeam, true);
        }

        attacker.fStriked = true;
        use = true;
    }
    //약탈
    private void CalculatePlunder(RogueUnitDataBase unit, bool isTeam)
    {
        //적이거나 약탈 없으면 반환
        if (!isTeam || !unit.plunder) return;

        if (autoBattleManager != null)
            autoBattleManager.PlayAbilityEffect("T13_Plunder", 0, 0, !isTeam, isTeam);

        RogueLikeData.Instance.AddGoldReward(plunderGold);
    }
    //무한
    private void CalculateEndLess(RogueUnitDataBase unit, bool isTeam)
    {
        if (!unit.endless) return;

        if (autoBattleManager != null)
            autoBattleManager.PlayAbilityEffect("T17_Endless", 0, 0, isTeam, isTeam);

        unit.Energy = Math.Min(unit.MaxEnergy, unit.Energy + 1);
    }
    //위압
    private void CalculateOverwhelm(RogueUnitDataBase attacker, RogueUnitDataBase defender, ref string text, bool isTeam)
    {
        int id = 8, type = 1, rank = 1, duration = -1;
        defender.effectDictionary[id] = new BuffDebuffData(id, type, rank, duration);

        if (autoBattleManager != null)
            autoBattleManager.PlayAbilityEffect("S08_Overwhelm", 0, 0, isTeam, isTeam);

        text += "위압 ";
    }
    //투창
    private void CalculateThrowSpear(RogueUnitDataBase attacker, List<RogueUnitDataBase> attackers, List<RogueUnitDataBase> defenders, ref float _damage, ref string text, bool isTeam, bool isFirstAttack)
    {
        float finalDamage = SetMultipleDamage(attacker, defenders[0], isTeam);
        float damage = throwSpearValue * finalDamage;
        if (!CalculateAccuracy(defenders[0], attacker, attackers, isTeam, isFirstAttack, 0))
        {
            int unitIndex = 0;
            //유산 127
            RelicManager.RunGuardiansCloak(defenders, !isTeam, ref unitIndex, ref damage);

            damage = MathF.Round(damage);

            if (autoBattleManager != null)
            {
                autoBattleManager.PlayAbilityEffect("T05_Pierce", unitIndex, 0, !isTeam, isTeam);
            }

            defenders[unitIndex].health -= damage;
            ApplyCommanderDamageEffects(defenders, unitIndex, damage, !isTeam, attacker);
            TryApplyCommanderPerryBacklineDamage(defenders, damage, isTeam, unitIndex, attacker);

            CallDamageText(damage, "투창 ", !isTeam, false, unitIndex, attacker);
            return;
        }
        text += "회피 ";
    }
    //암살
    // 암살
    private void CalculateAssassination(RogueUnitDataBase attacker, List<RogueUnitDataBase> attackers, List<RogueUnitDataBase> defenders, ref float _damage, ref string text, bool isTeam, bool isOnce = false)
    {
        RogueUnitDataBase target = CalculateMinHealthBackAttack(defenders);
        if (target == null) return;

        int unitIndex = defenders.IndexOf(target);
        if (unitIndex < 0) return;

        float damage = attacker.attackDamage * assassinationValue;

        // 사용처: 암살단장 52 즉사 효과
        if (ShouldTriggerCommanderChance(104, 0.5f, isTeam, false))
        {
            float killDamage = target.health;
            target.health = 0;
            ApplyCommanderDamageEffects(defenders, unitIndex, killDamage, !isTeam, attacker);
            CallDamageText(killDamage, "암살 ", !isTeam, true, unitIndex, attacker);
            return;
        }

        float ar = target.Armor;
        damage *= 1f - (ar / (ar + 10f));
        damage *= SetMultipleDamage(attacker, target, isTeam);
        damage = RelicManager.RunSpectersCowlDamagePenalty(damage, isTeam);

        // 유산 127
        RelicManager.RunGuardiansCloak(defenders, !isTeam, ref unitIndex, ref damage);
        target = defenders[unitIndex];

        damage = MathF.Round(damage);

        if (autoBattleManager != null)
        {
            autoBattleManager.PlayAbilityEffect("S06_Assassination", unitIndex, 0, !isTeam, isTeam);
        }

        target.health -= damage;
        ApplyCommanderDamageEffects(defenders, unitIndex, damage, !isTeam, attacker);

        CallDamageText(damage, "암살 ", !isTeam, true, unitIndex, attacker);
        TryApplyCommanderPerryBacklineDamage(defenders, damage, isTeam, unitIndex, attacker);
        TryRunSpectersCowlExecution(defenders, unitIndex, damage, isTeam);

        // 복수
        if (defenders.Count > 0 && defenders[0].vengeance)
        {
            float revengeDamage = damage;
            int revengeIndex = 0;

            RelicManager.RunGuardiansCloak(attackers, isTeam, ref revengeIndex, ref revengeDamage);
            RogueUnitDataBase revengeTarget = attackers[revengeIndex];

            revengeTarget.health -= revengeDamage;
            ApplyCommanderDamageEffects(attackers, revengeIndex, revengeDamage, isTeam, target);
            TryApplyCommanderPerryBacklineDamage(attackers, revengeDamage, !isTeam, revengeIndex, target);

            CallDamageText(revengeDamage, "복수 ", isTeam, true, revengeIndex);
        }

        // 33% 확률로 한 번 더 실행
        if (!isOnce && ShouldTriggerCommanderChance(200, 0.33f, isTeam, true))
        {
            CalculateAssassination(attacker, attackers, defenders, ref _damage, ref text, isTeam, true);
        }
    }
    // 도전
    private void CalculateChallenge(RogueUnitDataBase attaker, ref List<RogueUnitDataBase> defenders, bool isTeam)
    {
        if (!attaker.challenge || attaker.health <= 0)
            return;

        RogueUnitDataBase target = CalculateMinHealthBackAttack(defenders);
        if (target == null)
            return;

        int unitIndex = defenders.IndexOf(target);
        if (unitIndex <= 0)
            return;

        if (autoBattleManager != null)
            autoBattleManager.PlayAbilityEffect("S14_Challenge", 0, 0, isTeam, isTeam);

        defenders.RemoveAt(unitIndex);
        defenders.Insert(0, target);

        CallDamageText(0, "도전", !isTeam, false, unitIndex);
    }
    //상흔
    private void CalculateWounding(RogueUnitDataBase attacker, RogueUnitDataBase defender, ref string text, bool isTeam)
    {
        int scarId = 1, type = 1, rank = 1, duration = -1;
        defender.effectDictionary[scarId] = new BuffDebuffData(scarId, type, rank, duration);

        if (autoBattleManager != null)
            autoBattleManager.PlayAbilityEffect("S10_Wounding", 0, 0, !isTeam, isTeam);

        text += "상흔 ";
    }
    //충돌 isFirstAttack
    private void ChrashIsFirstAttack(RogueUnitDataBase attacker, List<RogueUnitDataBase> defenders, ref float multiplier, ref float reduceDamage, ref string text, bool isTeam, ref bool isPierce)
    {
        RogueUnitDataBase defender = defenders[0];
        //돌격
        if (attacker.charge)
        {
            if (isTeam && RelicManager.CheckRelicById(135))
            {
                isPierce = true;
            }

            // 돌격 이펙트 (강한 돌격 여부에 따라 분기)
            if (autoBattleManager != null)
            {
                string chargeEffectName = attacker.strongCharge ? "S01_Charge_strongCharge" : "S01_Charge";
                autoBattleManager.PlayAbilityEffect(chargeEffectName, 0, 0, isTeam, isTeam);
            }

            multiplier = CalculateCharge(attacker.Mobility);
            if (ShouldTriggerCommanderChance(200, 0.33f, isTeam, true))
            {
                multiplier *= 2;
                text += "아마록 ";
            }
            //유산
            if (isTeam && RelicManager.CheckRelicById(77))
            {
                WarRelic relic = RelicManager.GetRelicById(77);
                var vals = relic.GetAllValuesAsFloatListOrNull();
                if (vals != null)
                {
                    multiplier += vals[0];
                }

            }
            //강한 돌격
            if (attacker.strongCharge)
            {
                multiplier += strongChargeValue;

                text = "강한 ";
            }
            text += "돌격 ";
            //수비자 수비태세 시
            if (defender.defense)
            {
                reduceDamage += defenseValue;

                // 수비태세 이펙트 (방어자 CasterTop)
                if (autoBattleManager != null)
                    autoBattleManager.PlayAbilityEffect("S02_Defense", 0, 0, !isTeam, !isTeam);

                text += "수비태세 ";
            }
        }
        //공격자 수비태세
        if (attacker.defense)
        {
            //수비자 돌격 시
            if (defender.charge)
            {
                reduceDamage -= defenseValue;

                text += "수비태세 ";
            }
            else
            {
                if (!(isTeam && RelicManager.CheckRelicById(83)))
                {
                    reduceDamage += defenseValue;

                    text += "수비태세 ";
                }
            }
        }
        //미치광이
        CalculateFrontManiac(attacker, defenders, isTeam);
    }
    //충돌 특성 기술 발동
    private (float, string) ApplyChrashAbility(RogueUnitDataBase attacker, RogueUnitDataBase defender, bool isTeam, float _reduceDamage, string _text)
    {
        float reduceDamage = _reduceDamage;
        string text = _text;
        Dictionary<Func<RogueUnitDataBase, bool>, Action> traitEffects = new()
        {
            { unit => unit.bluntWeapon && defender.heavyArmor, () => CalculateBluntWeapon(attacker,isTeam,ref reduceDamage,ref text) }, // 둔기
            { unit => unit.slaughter && defender.lightArmor, () => CalculateSlaughter(ref reduceDamage,ref text,isTeam) }, // 도살
            { unit => unit.suppression && reduceDamage < 0, () => CalculateSuppression(defender,ref reduceDamage,ref text,isTeam) } // 제압
        };

        foreach (var trait in traitEffects)
        {
            if (trait.Key(attacker))
            {
                trait.Value();
            }
        }
        return (reduceDamage, text);
    }
    // 돌격 계산
    private float CalculateCharge(float mobility)
    {
        return ((0.95f / 100f) * (mobility * mobility)) + 1.05f;
    }
    //둔기
    private void CalculateBluntWeapon(RogueUnitDataBase unit, bool isTeam, ref float reduceDamage, ref string text)
    {
        float bluntDamage = 0.1f;
        if (RelicManager.CheckRelicById(126) && isTeam)
        {
            WarRelic relic = RelicManager.GetRelicById(126);
            var vals = relic.GetAllValuesAsFloatListOrNull();
            if (vals != null)
            {
                bluntDamage += vals[0];
            }
        }

        reduceDamage -= isTeam ? unit.baseAttackDamage * bluntDamage + myBluntWeaponValue : unit.baseAttackDamage * bluntDamage + enemyBluntWeaponValue;

        text += "둔기 ";
    }
    //도살
    private void CalculateSlaughter(ref float reduceDamage, ref string text, bool isTeam)
    {
        reduceDamage -= slaughterValue;

        if (autoBattleManager != null)
            autoBattleManager.PlayAbilityEffect("T09_Slaughter", 0, 0, !isTeam, isTeam);

        text += "도살 ";
    }
    //대기병
    private void CalculateAntiCavalry(ref float reduceDamage, ref string text, RogueUnitDataBase attaker)
    {
        reduceDamage -= attaker.antiCavalry;

        text += "대기병 ";
    }
    //제압
    private void CalculateSuppression(RogueUnitDataBase defender, ref float reduceDamage, ref string text, bool isTeam)
    {
        reduceDamage += defender.maxHealth * suppressionValue;

        if (autoBattleManager != null)
            autoBattleManager.PlayAbilityEffect("T12_Suppression", 0, 0, !isTeam, isTeam);

        text += "제압 ";
    }
    //회피율 계산
    public float CalculateDodge(RogueUnitDataBase unit, bool isTeam, bool isFirstAttack)
    {
        float dodge;
        int mobility = unit.Mobility;
        if (IsCommander(211) && IsPlayerUnit(unit))
        {
            dodge = 0;
        }
        else
        {
            float addDodge = 0;
            float mulityDodge = 13;
            Dictionary<int, Action> dodgeEffects = new()
        {
            { 2, () => addDodge += 15 },
            { 4, () => addDodge += 5 },
            { 5, () => mulityDodge *= 2 },
            { 6, () => addDodge += 5 },
            { 8, () => mobility = overwhelmValue },
            { 9, () => addDodge -=5 },
            { 10,() => addDodge +=5 },
            { 12,() => addDodge +=5 },
        };
            //폭풍의 창
            float extra = CalculateSpearOfStormDodge(unit, isTeam, isFirstAttack);
            foreach (var key in dodgeEffects.Keys)
            {
                if (unit.effectDictionary.ContainsKey(key)) dodgeEffects[key]();
            }

            if (IsCommander(101) && IsLightCavalryOrAssassin(unit))
                addDodge += 5;

            if (IsCommander(112) && IsEnemyUnit(unit))
                addDodge += 10;

            dodge = (2 + ((mulityDodge / 9) * (mobility - 1))) + (unit.agility ? 10.0f : 0) + addDodge + extra;
        }

        return MathF.Floor(Mathf.Clamp(dodge, 0, 100));
    }

    //회피 유무 계산
    private bool CalculateAccuracy(RogueUnitDataBase defender, RogueUnitDataBase attacker, List<RogueUnitDataBase> attackers, bool isTeam, bool isFirstAttack, int _unitIndex)
    {
        if (attacker.perfectAccuracy)
            return false; // 필중 특성인 경우 회피 불가

        float dogeRate = CalculateDodge(defender, isTeam, isFirstAttack);

        bool isDodge = dogeRate > RogueLikeData.Instance.GetRandomInt(0, 100);
        if (isDodge)
        {
            PlaySE("se_Dodge");

            //방어자가 회피 성공시 암살단장의 효과 발동 isTeam==true라는건 attacker가 내 유닛이라는것 defender는 이때 enemy가 됨
            if (isTeam && enemyHeroUnits.TryGetValue(58, out List<RogueUnitDataBase> heroList))
            {
                float damage = 20 * heroList.Count;
                //유산 127
                float relicReduceDamage = damage;
                int unitIndex = _unitIndex;
                RelicManager.RunGuardiansCloak(attackers, isTeam, ref unitIndex, ref relicReduceDamage);
                RogueUnitDataBase target = attackers[unitIndex];

                target.health -= relicReduceDamage;
                ApplyCommanderDamageEffects(attackers, unitIndex, relicReduceDamage, isTeam, defender);

                CallDamageText(relicReduceDamage, "암살단장 ", isTeam, true, unitIndex);
            }
            else if (!isTeam && myHeroUnits.TryGetValue(58, out List<RogueUnitDataBase> myHeroList))
            {
                float damage = 20 * myHeroList.Count;
                attacker.health -= damage;
                ApplyCommanderDamageEffects(attackers, _unitIndex, damage, isTeam, defender);

                CallDamageText(damage, "암살단장 ", !isTeam, true, _unitIndex);
            }

            //유산 18
            if (!isTeam && RelicManager.CheckRelicById(18))
            {
                WarRelic relic = RelicManager.GetRelicById(18);
                var vals = relic.GetAllValuesAsFloatListOrNull();
                if (vals != null)
                {
                    float relicDamage = defender.attackDamage * vals[0];
                    attacker.health -= relicDamage;
                    ApplyCommanderDamageEffects(attackers, _unitIndex, relicDamage, isTeam);

                    CallDamageText(relicDamage, "정예기병대안장 ", !isTeam, true);
                }
            }
        }
        return isDodge;
    }

    //체력 회복
    private float HealHealth(RogueUnitDataBase unit, float healValue)
    {
        if (unit.effectDictionary.ContainsKey(1) || unit.effectDictionary.ContainsKey(0)) return 0;
        return healValue;
    }

    //연막
    private void CalculateSmokeScreen(List<RogueUnitDataBase> units, bool isTeam)
    {
        if (CheckBackUnit(units))
        {
            if (autoBattleManager != null)
                autoBattleManager.PlayAbilityEffect("S15_SmokeScreen", 0, 0, isTeam, isTeam);

            int id = 2, type = 0, rank = 1, duration = -1;
            for (int i = 1; i < units.Count; i++)
            {
                if (units[i].health > 0 && !units[i].effectDictionary.ContainsKey(id))
                {
                    units[i].effectDictionary[id] = new BuffDebuffData(id, type, rank, duration);
                }
            }

            CallDamageText(0, "연막 ", isTeam, false, 1);
        }
    }
    //작열 
    private void ProcessBurning(RogueUnitDataBase defender, bool isTeam, ref string text)
    {
        int burningId = 0, type = 1, rank = 1, duration = 2;

        if (defender.effectDictionary.TryGetValue(burningId, out BuffDebuffData effect))
        {
            effect.EffectGrade += 1;
            effect.Duration += 1;
        }
        else
        {
            defender.effectDictionary[burningId] = new BuffDebuffData(burningId, type, rank, duration);
        }

        if (isTeam && RelicManager.CheckRelicById(46))
        {
            WarRelic relic = RelicManager.GetRelicById(46);
            var vals = relic.GetAllValuesAsFloatListOrNull();
            if (vals != null)
            {
                //상흔
                int scarId = 1, sType = 1, sRank = 1, sDuration = -1;
                if (!defender.effectDictionary.TryGetValue(scarId, out BuffDebuffData sEffect))
                {
                    defender.effectDictionary[scarId] = new BuffDebuffData(scarId, sType, sRank, sDuration);
                    text += "상흔 ";
                }

                //위압
                int oId = 8, oType = 1, oRank = 1, oDuration = -1;
                if (!defender.effectDictionary.TryGetValue(oId, out BuffDebuffData oEffect))
                {
                    defender.effectDictionary[oId] = new BuffDebuffData(oId, oType, oRank, oDuration);
                    text += "위압 ";
                }
            }
        }

        text += "작열 ";

    }

    //작열 적용
    private void CalculateBurning(RogueUnitDataBase attacker, List<RogueUnitDataBase> defenders, bool isTeam, ref string text)
    {
        if (attacker.scorching)
        {
            ProcessBurning(defenders[0], isTeam, ref text);
        }
    }
    //작열 데미지
    private void DamageBurning(List<RogueUnitDataBase> units, bool isTeam)
    {
        int burningId = 0;

        for (int i = 0; i < units.Count; i++)
        {
            RogueUnitDataBase unit = units[i];
            if (unit == null) continue;

            if (!unit.effectDictionary.TryGetValue(burningId, out BuffDebuffData burningEffect) || burningEffect.Duration == 0)
                continue;

            burningEffect.Duration = Mathf.Min(burningEffect.Duration, 2);
            burningEffect.EffectGrade = Mathf.Min(burningEffect.EffectGrade, 3);

            float fireDamage = fireDamageValue * unit.maxHealth;
            float damage = Mathf.Min(burningEffect.EffectGrade * fireDamage, fireDamage * 3);

            //유산 106
            damage += RelicManager.RunControlTorch();

            //유산 127
            int unitIndex = i;
            RelicManager.RunGuardiansCloak(units, isTeam, ref unitIndex, ref damage);
            RogueUnitDataBase target = units[unitIndex];

            target.health -= damage;
            ApplyCommanderDamageEffects(units, unitIndex, damage, isTeam);

            if (autoBattleManager != null)
                autoBattleManager.PlayAbilityEffect("T15_Scorching", unitIndex, 0, isTeam, isTeam);

            burningEffect.Duration--;
            if (burningEffect.Duration == 0)
            {
                unit.effectDictionary.Remove(burningId);
            }

            //유산 46
            if (RelicManager.CheckRelicById(46))
            {
                unit.Armor--;
            }

            CallDamageText(damage, "작열", isTeam, false, unitIndex);
        }
    }

    // 치유
    private void ProcessHealing(List<RogueUnitDataBase> units, bool isTeam)
    {
        RogueUnitDataBase frontUnit = units[0];
        float heal = 0;

        for (int i = 1; i < units.Count; i++)
        {
            RogueUnitDataBase healer = units[i];

            if (!healer.healing || healer.range < 2)
                continue;

            int rearPosition = i;

            if (healer.range - rearPosition >= 1 && frontUnit.health > 0)
            {
                float healAmount = healer.attackDamage;
                healAmount = HealHealth(frontUnit, healAmount);

                heal += healAmount;

                if (autoBattleManager != null)
                {
                    // T19_Healing: 힐러(시전자) 초상화 상단 (CasterTop)
                    autoBattleManager.PlayAbilityEffect("T19_Healing", 0, i, isTeam, isTeam);
                    // T19_Healing_effect: 치유 받는 대상 초상화 정중앙 (TargetCenter)
                    autoBattleManager.PlayAbilityEffect("T19_Healing_effect", 0, i, isTeam, isTeam);
                }

                frontUnit.health = Mathf.Min(frontUnit.maxHealth, frontUnit.health + healAmount);
            }
        }

        if (heal > 0) CallDamageText(-heal, "치유 ", isTeam, false);
    }

    //후열 유닛 생존 확인
    private bool CheckBackUnit(List<RogueUnitDataBase> units)
    {
        for (int i = 1; i < units.Count; i++)
        {
            if (units[i].health > 0)
            {
                return true;
            }
        }
        return false;
    }

    //체력이 가장 낮은 유닛의 인덱스를 반환
    private int CalculateMinHealthIndex(List<RogueUnitDataBase> units)
    {
        int minHealthNumber = -1;
        float lastHealth = float.MaxValue;

        for (int i = 1; i < units.Count; i++)
        {
            if (units[i].health > 0 && units[i].health < lastHealth)
            {
                lastHealth = units[i].health;
                minHealthNumber = i;
            }
        }

        return minHealthNumber;
    }

    private RogueUnitDataBase CalculateMinHealthBackAttack(List<RogueUnitDataBase> defenders)
    {
        if (defenders == null || defenders.Count < 2)
            return null;

        int targetIndex = -1;
        float minHealth = float.MaxValue;

        for (int i = 1; i < defenders.Count; i++)
        {
            if (defenders[i].health > 0 && defenders[i].health < minHealth)
            {
                minHealth = defenders[i].health;
                targetIndex = i;
            }
        }

        if (targetIndex == -1)
            return null;

        for (int i = 0; i < targetIndex; i++)
        {
            if (defenders[i].health > 0 && defenders[i].guard)
            {
                PlaySE("se_Guard");
                return defenders[i];
            }
        }

        return defenders[targetIndex];
    }


    // 원거리 공격 최적화 코드
    private (float damage, string text, RogueUnitDataBase lastDamageSource) CalculateRangeAttack(
        List<RogueUnitDataBase> attackers,
        List<RogueUnitDataBase> defenders,
        bool isTeam,
        float finalDamage,
        bool isFirstAttack)
    {
        float allDamage = 0f;
        string text = "원거리 ";
        RogueUnitDataBase lastDamageSource = null;

        for (int i = 1; i < attackers.Count; i++)
        {
            RogueUnitDataBase attacker = attackers[i];

            if (!CanUseRangedAttackUnit(attacker, i))
                continue;

            float damage = attacker.attackDamage;
            damage = ChangeBackMultiple(attackers[0], defenders[0], damage, isTeam, attacker);

            for (int k = 0; k < 2; k++)
            {
                if (k == 1 && !attacker.doubleShot)
                    break;

                if (CalculateAccuracy(defenders[0], attacker, attackers, isTeam, isFirstAttack, i))
                    continue;

                CalculateBurning(attacker, defenders, isTeam, ref text);
                CalculateTracker(attacker, defenders[0]);
                CalculateReaper(isTeam);

                if (damage > 0)
                {
                    if (defenders[0].thorns)
                    {
                        attacker.health -= thornsDamageValue;
                        ApplyCommanderDamageEffects(attackers, i, thornsDamageValue, isTeam);
                    }

                    float finalRelicDamage = RelicManager.RunPulsatingDoll(attacker, isTeam);
                    if (finalRelicDamage > 0)
                    {
                        attacker.health -= finalRelicDamage;
                        ApplyCommanderDamageEffects(attackers, i, finalRelicDamage, isTeam);
                        CallDamageText(finalRelicDamage, "맥동하는인형 ", !isTeam, true);
                    }
                }

                allDamage += damage;
                lastDamageSource = attacker;
            }
        }

        return (allDamage, text, lastDamageSource);
    }

    //순교 0,1번이 동시에 사망해도 1번에 버프
    private void CalculateMartyrdom(List<RogueUnitDataBase> defenders, int defenderIndex, bool isMyUnit)
    {
        int id = 8;
        if (defenders[defenderIndex].martyrdom)
        {
            if (defenderIndex + 1 < defenders.Count && defenders[defenderIndex + 1].health > 0)
            {
                if (autoBattleManager != null)
                    autoBattleManager.PlayAbilityEffect("S09_Martyrdom", 0, defenderIndex, isMyUnit, isMyUnit);
                defenders[defenderIndex + 1].stats.AddModifier(new StatModifier
                {
                    stat = StatType.AttackDamage,
                    value = defenders[defenderIndex + 1].baseAttackDamage * martyrdomValue,
                    source = SourceType.Skill,
                    modifierId = id,
                    isPercent = false
                });

                //유산74
                if (RelicManager.CheckRelicById(74))
                {
                    defenders[defenderIndex + 1].stats.AddModifier(new StatModifier
                    {
                        stat = StatType.Health,
                        value = defenders[defenderIndex].baseAttackDamage,
                        source = SourceType.Skill,
                        modifierId = id,
                        isPercent = false
                    });
                }
            }
        }
    }
    //추적자
    private void CalculateTracker(RogueUnitDataBase attacker, RogueUnitDataBase defender)
    {
        if (attacker.idx == 29 && !defender.effectDictionary.ContainsKey(3))
        {
            int id = 3, type = 1, rank = 1, durateion = -1;
            defender.Armor = Math.Max(defender.Armor - 3, 0);
            defender.effectDictionary[id] = new(id, type, rank, durateion);
        }
    }
    // 영웅 유닛을 체크하고 저장
    private void CheckHeroUnit(List<RogueUnitDataBase> units, bool isTeam)
    {
        var heroUnits = units.Where(unit => unit.branchIdx == 8).ToList();

        Dictionary<int, List<RogueUnitDataBase>> targetHeroUnits = isTeam ? myHeroUnits : enemyHeroUnits;

        foreach (var unit in heroUnits)
        {
            if (!targetHeroUnits.ContainsKey(unit.idx))
            {
                targetHeroUnits[unit.idx] = new List<RogueUnitDataBase>();
            }
            targetHeroUnits[unit.idx].Add(unit);
        }
    }
    // 영웅 유닛 검색 (없으면 빈 리스트 반환)
    private List<RogueUnitDataBase> GetHeroUnitList(bool isTeam, int id)
    {
        return (isTeam ? myHeroUnits : enemyHeroUnits).TryGetValue(id, out var heroList) ? heroList : new();
    }

    // 진홍 사제 효과 적용 (앞에 있는 유닛들에게 흡혈 부여)
    private void CalculateBloodPriest(List<RogueUnitDataBase> units)
    {
        for (int i = 1; i < units.Count; i++)
        {
            if (units[i].idx == 50)
            {
                int range = Mathf.Max(0, (int)units[i].range - 1);
                int effectiveRange = Mathf.Min(range, i);

                for (int j = 1; j <= effectiveRange; j++)
                {
                    int targetIndex = i - j;
                    units[targetIndex].lifeDrain = true;
                }
            }
        }
    }
    //군단장
    private void CalculateCorpsCommander(bool isTeam)
    {
        int heroId = 44;
        List<RogueUnitDataBase> heroList = GetHeroUnitList(isTeam, heroId);
        if (heroList.Count <= 0) return;
        if (isTeam)
        {
            mybindingAttackDamage += 5 * heroList.Count;
        }
        else if (!isTeam)
        {
            enemybindingAttackDamage += 5 * heroList.Count;
        }
        return;
    }
    //봉인 풀린 자
    private void CalculateTheUnsealedOne(int deadDefenderCount, bool isTeam)
    {
        int heroId = 45;
        List<RogueUnitDataBase> heroUnits = GetHeroUnitList(isTeam, heroId);
        if (heroUnits.Count <= 0) return;
        foreach (var unit in heroUnits)
        {
            if (unit.health > 0)
            {
                unit.maxHealth += 10 * deadDefenderCount;
                unit.health += 10 * deadDefenderCount;
                unit.attackDamage += 5 * deadDefenderCount;
            }
        }

    }
    //돌격대장
    private void CalculateAssaultLeader(List<RogueUnitDataBase> units, bool isTeam)
    {
        if (!isTeam || units[0].idx != 46 || units[0].health < 1) return;
        int morale = RogueLikeData.Instance.GetMorale();
        RogueLikeData.Instance.ChangeMorale(10);
        UnitStateChange.ApplyMoralState();

    }
    //유목민 족장
    private void CalculateNomadicChief(List<RogueUnitDataBase> units, bool isTeam)
    {
        int heroId = 47;
        List<RogueUnitDataBase> heroList = GetHeroUnitList(isTeam, heroId);
        if (heroList.Count <= 0) return;
        if (isTeam)
        {
            foreach (var unit in units)
            {
                if (unit.tagIdx == 1) unit.bindingForce = true;
            }
        }
        else if (!isTeam)
        {
            foreach (var unit in units)
            {
                if (unit.tagIdx == 1) unit.bindingForce = true;
            }
        }
    }
    // 저격수 유닛들을 찾아서 0이 아닌 랜덤한 위치로 이동
    private void CalculateSniper(List<RogueUnitDataBase> units)
    {
        int heroId = 48;
        List<int> sniperIndices = new List<int>();

        for (int i = 0; i < units.Count; i++)
        {
            if (units[i].idx == heroId && units[i].health > 0)
            {
                sniperIndices.Add(i);
            }
        }
        // 스나이퍼가 없다면 종료
        if (sniperIndices.Count == 0) return;

        foreach (int index in sniperIndices)
        {
            int newIndex = RogueLikeData.Instance.GetRandomInt(1, units.Count);

            var sniper = units[index];
            units.RemoveAt(index);

            if (newIndex > index) newIndex--;
            units.Insert(newIndex, sniper);
        }
    }
    // 떠도는 자 효과 적용
    private void CalculateWanderer(bool isTeam)
    {
        int heroId = 49;
        List<RogueUnitDataBase> wandererList = GetHeroUnitList(isTeam, heroId);
        if (wandererList.Count <= 0) return;
        Dictionary<int, List<RogueUnitDataBase>> targetHeroUnits = isTeam ? myHeroUnits : enemyHeroUnits;

        foreach (var heroList in targetHeroUnits.Values)
        {
            foreach (var hero in heroList)
            {
                if (hero.idx != heroId)  // 떠도는 자가 아닌 다른 영웅에게 효과 적용
                {
                    hero.stats.AddModifier(new StatModifier
                    {
                        stat = StatType.Health,
                        value = 40 * wandererList.Count,
                        source = SourceType.Passive,
                        modifierId = heroId,
                        isPercent = false
                    });
                }
            }
        }
    }
    // 암살단장
    private bool ExecuteAssassinationLeaderStrike(bool isTeam, List<RogueUnitDataBase> defenders, float finalDamage)
    {
        int heroId = 50;
        List<RogueUnitDataBase> heroList = GetHeroUnitList(isTeam, heroId);
        bool use = false;
        if (heroList.Count <= 0) return use;

        foreach (var hero in heroList)
        {
            int strikeCount = !isTeam && IsCommander(203) ? 5 : 4;
            for (int i = 0; i < strikeCount; i++)
            {
                CalculateDamageFirstStrike(hero, defenders, isTeam, ref use);
            }
        }

        return use;
    }
    //기괴한 주교
    private void CalculateBizarreBishop(List<RogueUnitDataBase> units, bool isTeam)
    {
        int heroId = 51;
        List<RogueUnitDataBase> heroList = GetHeroUnitList(isTeam, heroId);
        if (heroList.Count <= 0) return;
        int id = 7, type = 0, rank = 1, duration = -1;
        foreach (var unit in units)
        {
            unit.effectDictionary[id] = new BuffDebuffData(id, type, rank, duration);
            unit.PassiveBizarreBishop();
        }
    }
    //불사 효과
    private void CalculateImmortality(ref List<RogueUnitDataBase> units, List<RogueUnitDataBase> deadUnits)
    {
        if (deadUnits == null || deadUnits.Count == 0)
            return;

        if (units == null)
            units = new List<RogueUnitDataBase>();

        int id = 7;
        for (int i = 0; i < deadUnits.Count; i++)
        {
            RogueUnitDataBase unit = deadUnits[i];
            if (unit == null || unit.effectDictionary == null)
                continue;

            if (unit.effectDictionary.ContainsKey(id))
            {
                unit.effectDictionary.Remove(id);
                unit.health = 10;
                unit.alive = true;
                units.Add(unit);
            }

        }
    }
    // 노인 기사 효과 적용 (랜덤 특성 획득)
    private void CalculateOldKnight(RogueUnitDataBase unit, bool isFrontDefenderDead)
    {
        if (unit.idx != 52 || !isFrontDefenderDead || unit.health <= 0) return;

        unit.SetRandomTraits();
        if (IsCommander(212) && IsEnemyUnit(unit))
            CommenderEffect.RefreshSteinTraitAttack(unit);
    }
    //미치광이 전투 시작 시
    private bool CalculateManiac(List<RogueUnitDataBase> defenders, bool isTeam)
    {
        int heroId = 53;
        List<RogueUnitDataBase> heroList = GetHeroUnitList(isTeam, heroId);
        if (heroList.Count == 0) return false;

        for (int i = 0; i < defenders.Count; i++)
        {
            var unit = defenders[i];
            string text = "";

            ProcessBurning(unit, isTeam, ref text);

            CallDamageText(0, text, !isTeam, false, defenders.IndexOf(unit));
        }

        return true;
    }
    // 미치광이 전열 효과
    private void CalculateFrontManiac(RogueUnitDataBase attacker, List<RogueUnitDataBase> defenders, bool isTeam)
    {
        if (attacker.idx != 53) return;
        if (!CheckBackUnit(defenders)) return;

        List<RogueUnitDataBase> backUnits = defenders.Skip(1).Where(unit => unit.health > 0).ToList();
        int debuffTargetCount = Mathf.Min(3, backUnits.Count);

        List<RogueUnitDataBase> selectedTargets = backUnits.OrderBy(x => RogueLikeData.Instance.GetRandomFloat()).Take(debuffTargetCount).ToList();

        for (int i = 0; i < selectedTargets.Count; i++)
        {
            var unit = selectedTargets[i];
            string text = "";

            ProcessBurning(unit, isTeam, ref text);

            CallDamageText(0, text, !isTeam, false, i);
        }
    }
    //사신
    private float CalculateReaper(bool isTeam)
    {
        int heroId = 54;
        List<RogueUnitDataBase> heroUnits = GetHeroUnitList(isTeam, heroId);
        if (heroUnits.Count <= 0) return 0f;

        float damage = 0f;
        foreach (var unit in heroUnits)
        {
            if (unit.health > 0) damage += unit.attackDamage;
        }
        return damage;
    }
    //고승 유산 추가는 아직 없음
    private void CalculateHighPriest(bool isTeam)
    {
        int heroId = 55;
        List<RogueUnitDataBase> heroUnits = GetHeroUnitList(isTeam, heroId);
        if (heroUnits.Count <= 0) return;

    }
    //반란군 지도자 유닛 생성 부분
    private void CalculateRebelLeader(List<RogueUnitDataBase> units, bool isTeam)
    {
        int heroId = 56;
        int heroCount = GetHeroUnitList(isTeam, heroId).Count * 2;
        if (heroCount <= 0) return;

        for (int i = 0; i < heroCount; i++)
        {
            RogueUnitDataBase newUnit = UnitLoader.Instance.GetCloneUnitById(0, isTeam);

            units.Insert(0, newUnit);
        }
    }

    //불굴의 방패 전투 참여 시
    private void CalculateIndomitableShield(List<RogueUnitDataBase> units, bool isTeam)
    {
        int heroId = 57;
        int heroCount = GetHeroUnitList(isTeam, heroId).Count;
        if (heroCount <= 0) return;
        foreach (var unit in units)
        {
            if (unit.heavyArmor)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Armor,
                    value = 5,
                    source = SourceType.Passive,
                    modifierId = heroId,
                    isPercent = false
                });
            }
        }
    }
    // 불굴의 방패 중갑 사망 시
    private void CalculateIndomitableShieldDead(List<RogueUnitDataBase> units, List<RogueUnitDataBase> deadUnits, bool isTeam)
    {
        int heroId = 57;
        int heroCount = GetHeroUnitList(isTeam, heroId).Count;
        if (heroCount <= 0) return;

        int deadHeavyArmorCount = deadUnits.Count(unit => unit.heavyArmor);
        if (deadHeavyArmorCount <= 0) return;

        int healthIncrease = 10 * deadHeavyArmorCount * heroCount;

        foreach (var unit in units)
        {
            if (unit.health > 0)
            {
                unit.maxHealth += healthIncrease;
                unit.health += healthIncrease;
            }
        }
    }
    //폭풍의 창 전투 참여 시
    private void CalculateSpearOfStorm(List<RogueUnitDataBase> units, bool isTeam)
    {
        int heroId = 58;
        List<RogueUnitDataBase> heroUnits = GetHeroUnitList(isTeam, heroId);
        if (heroUnits.Count <= 0) return;
        foreach (var unit in units)
        {
            unit.attackDamage += unit.baseAntiCavalry * 0.5f;
        }
    }
    //폭풍의 창 회피율 증가
    private float CalculateSpearOfStormDodge(RogueUnitDataBase unit, bool isTeam, bool isFirstAttack)
    {
        if (!isFirstAttack) return 0;
        int heroId = 58;
        List<RogueUnitDataBase> heroUnits = GetHeroUnitList(isTeam, heroId);
        if (heroUnits.Count <= 0 || unit.branchIdx != 0) return 0f;
        return 0.5f;
    }
    //결속 발동
    public static void CalculataeSolidarity(List<RogueUnitDataBase> units, bool isTeam, bool isBattle = false)
    {
        int id = 9;
        foreach (var unit in units)
        {
            if (!unit.bindingForce || unit.tagIdx == 0) continue; // 태그 0이면 무시

            // 같은 태그를 가진 유닛 수 계산
            int sameTagCount = units.Count(u => u.tagIdx == unit.tagIdx);

            // 보정 값
            float attackBuff = sameTagCount * 5f;
            float healthBuff = sameTagCount * 15f;

            // 기존 동일 source 제거 후 재적용
            unit.stats.RemoveModifiersBySource(SourceType.Synergy);
            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.AttackDamage,
                value = attackBuff,
                source = SourceType.Synergy,
                modifierId = id,
                isPercent = false
            });
            if (isBattle) continue;

            unit.stats.AddModifier(new StatModifier
            {
                stat = StatType.Health,
                value = healthBuff,
                source = SourceType.Synergy,
                modifierId = id,
                isPercent = false
            });
        }
    }

    //파수꾼 시너지
    private void CalculateWarden(List<RogueUnitDataBase> units)
    {
        int id = 1;
        int idx = 19;
        if (units.Any(u => u.idx == idx))
        {
            var findUnits = units.Where(u => u.branchIdx == 0).ToList();
            if (findUnits.Count >= 4)
            {
                foreach (var unit in findUnits)
                {
                    if (unit.idx == idx)
                    {
                        unit.stats.AddModifier(new StatModifier
                        {
                            stat = StatType.Health,
                            value = 20,
                            source = SourceType.Synergy,
                            modifierId = id,
                            isPercent = false
                        });
                    }
                }
            }
        }

    }
    //장검병 시너지
    private void CalculateLongSwordMan(List<RogueUnitDataBase> units)
    {
        int idx = 20;
        if (units.Any(u => u.idx == idx))
        {
            var findUnits = units.Where(u => u.branchIdx == 1).ToList();
            if (findUnits.Count >= 4)
            {
                foreach (var unit in findUnits)
                {
                    if (unit.idx == idx)
                        unit.strongCharge = true;
                }
            }
        }

    }
    //장궁병 시너지
    private void CalculateLongBowMan(List<RogueUnitDataBase> units)
    {
        int idx = 21;
        int id = 3;
        if (units.Any(u => u.idx == idx))
        {
            var findUnits = units.Where(u => u.branchIdx == 2).ToList();
            if (findUnits.Count >= 3)
            {
                foreach (var unit in findUnits)
                {
                    if (unit.idx == idx)
                        unit.stats.AddModifier(new StatModifier
                        {
                            stat = StatType.AttackDamage,
                            value = 10,
                            source = SourceType.Synergy,
                            modifierId = id,
                            isPercent = false
                        });
                }
            }
        }

    }
    //철옹성
    private void CalculateSteelCastle(List<RogueUnitDataBase> units)
    {
        int idx = 22;
        int id = 4;
        if (units.Any(u => u.idx == idx))
        {
            var findUnits = units.Where(u => u.branchIdx == 3).ToList();
            if (findUnits.Count >= 3)
            {
                foreach (var unit in findUnits)
                {
                    if (unit.idx == idx)
                        unit.stats.AddModifier(new StatModifier
                        {
                            stat = StatType.Health,
                            value = 40,
                            source = SourceType.Synergy,
                            modifierId = id,
                            isPercent = false
                        });
                }
            }
        }
    }
    //타격대 시너지
    private void CalculateStrikeForce(List<RogueUnitDataBase> units)
    {
        int idx = 40;
        int id = 5;
        if (units.Any(u => u.idx == idx))
        {
            var findUnits = units.Where(u => u.branchIdx == 5).ToList();
            if (findUnits.Count >= 3)
            {
                foreach (var unit in findUnits)
                {
                    if (unit.idx == idx)
                        unit.stats.AddModifier(new StatModifier
                        {
                            stat = StatType.Mobility,
                            value = 3,
                            source = SourceType.Synergy,
                            modifierId = id,
                            isPercent = false
                        });
                }
            }
        }
    }

    //전투망치 시너지
    private void CalculateBattleHammer(List<RogueUnitDataBase> units, bool isTeam)
    {
        int idx = 23;
        if (units.Any(u => u.idx == idx))
        {
            var findUnits = units.Where(u => u.branchIdx == 6).ToList();
            if (findUnits.Count >= 3)
            {
                if (isTeam)
                {
                    myBluntWeaponValue += 15;
                }
                else
                {
                    enemyBluntWeaponValue += 15;
                }
            }
        }

    }
    //제국 시너지
    private void CalculateEmpire(List<RogueUnitDataBase> units)
    {
        var findUnits = units.Where(u => u.factionIdx == 1).ToList();
        if (findUnits.Count >= 5)
        {
            int id = 4, type = 0, rank = 1, duration = -1;
            foreach (var unit in units)
            {
                unit.effectDictionary[id] = new(id, type, rank, duration);
            }
        }
    }
    //신성국 시너지
    private void CalculateDivinityCountry(List<RogueUnitDataBase> units)
    {
        int id = 8;
        var findUnits = units.Where(u => u.factionIdx == 2).ToList();
        if (findUnits.Count >= 5)
        {
            foreach (var unit in units)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Armor,
                    value = 1,
                    source = SourceType.Synergy,
                    modifierId = id,
                    isPercent = false
                });

            }
        }
    }
    //칠왕연합 시너지
    private void CalculateSevenUnion(List<RogueUnitDataBase> units)
    {
        int id = 9;
        var findUnits = units.Where(u => u.factionIdx == 3).ToList();
        if (findUnits.Count >= 5)
        {
            foreach (var unit in units)
            {
                unit.stats.AddModifier(new StatModifier
                {
                    stat = StatType.AttackDamage,
                    value = unit.baseAttackDamage * 0.03f,
                    source = SourceType.Synergy,
                    modifierId = id,
                    isPercent = false
                });
            }
        }
    }

    //후열 타격
    private RogueUnitDataBase CalculateBackAttack(List<RogueUnitDataBase> defenders)
    {
        if (defenders == null || defenders.Count < 2)
            return null;

        int targetIndex = -1;

        for (int i = 1; i < defenders.Count; i++)
        {
            if (defenders[i].health > 0)
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex == -1)
            return null;

        for (int i = 0; i < targetIndex; i++)
        {
            if (defenders[i].health > 0 && defenders[i].guard)
            {
                PlaySE("se_Guard");
                return defenders[i];
            }
        }

        return defenders[targetIndex];
    }




    //데미지 ui 호출
    private void ApplyCommanderZanderOnDamage(float damage, bool damagedUnitIsMyTeam, RogueUnitDataBase unit)
    {
        if (!IsCommander(105) || damage <= 0 || !damagedUnitIsMyTeam)
            return;

        if (unit == null || unit.stats == null)
            return;

        unit.stats.AddModifier(new StatModifier
        {
            stat = StatType.Armor,
            value = -2f,
            source = SourceType.Commander,
            modifierId = 105,
            isPercent = false
        });
        unit.ApplyModifiers(true);
    }

    private void TryApplyCommanderPerryBacklineDamage(
        List<RogueUnitDataBase> defenders,
        float sourceDamage,
        bool attackerIsMyTeam,
        int excludedIndex = -1,
        RogueUnitDataBase sourceUnit = null)
    {
        if (!IsCommander(114) || attackerIsMyTeam || sourceDamage <= 0 || defenders == null || defenders.Count < 2)
            return;

        if (!ShouldTriggerCommanderChance(114, 0.25f, attackerIsMyTeam, true))
            return;

        int targetIndex = -1;
        for (int i = defenders.Count - 1; i >= 1; i--)
        {
            if (i != excludedIndex && defenders[i] != null && defenders[i].health > 0)
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex < 0)
            return;

        float damage = MathF.Round(sourceDamage);
        defenders[targetIndex].health -= damage;
        CallDamageText(damage, "페리 ", true, true, targetIndex, sourceUnit);
    }

    private void RecordLennonWarriorDamage(
        float damage,
        bool damagedUnitIsMyTeam,
        RogueUnitDataBase target,
        RogueUnitDataBase sourceUnit)
    {
        if (!IsCommander(102)
            || damage <= 0f
            || !damagedUnitIsMyTeam
            || target == null
            || target.health > 0f
            || sourceUnit == null
            || sourceUnit.branchIdx != 1)
        {
            return;
        }

        lennonWarriorDamageTargets.Add(target);
    }

    private void ApplyCommanderDamageEffects(
        List<RogueUnitDataBase> damagedUnits,
        int unitIndex,
        float damage,
        bool damagedUnitIsMyTeam,
        RogueUnitDataBase sourceUnit = null)
    {
        if (damage <= 0f
            || damagedUnits == null
            || unitIndex < 0
            || unitIndex >= damagedUnits.Count)
        {
            return;
        }

        RogueUnitDataBase target = damagedUnits[unitIndex];
        if (target == null)
            return;

        RecordLennonWarriorDamage(damage, damagedUnitIsMyTeam, target, sourceUnit);
        ApplyCommanderZanderOnDamage(damage, damagedUnitIsMyTeam, target);
    }

    private void CallDamageText(
        float damage,
        string text,
        bool team,
        bool isAttack,
        int unitIndex = 0,
        RogueUnitDataBase sourceUnit = null)
    {
        //team? 나의 공격 : 상대 공격
        if (autoBattleUI != null)
            autoBattleUI.ShowDamage(MathF.Round(damage), text, team, isAttack, unitIndex);
    }

    //사리유산
    private void CalculateSariRelic(List<RogueUnitDataBase> attackers, bool isTeam, bool isDefenderDead)
    {
        if (!isTeam) return;
        if (attackers[0].idx == 63 && isDefenderDead && RelicManager.CheckRelicById(78))
        {
            RogueLikeData.Instance.AddSariStack(1);
            int sariId = 78;
            int sariStack = RogueLikeData.Instance.GetSariStack();

            //초기화 및 추가 적용
            foreach (var attacker in attackers)
            {
                attacker.stats.RemoveModifiersBySourceAndId(SourceType.Relic, sariId);

                attacker.stats.AddModifier(new StatModifier
                {
                    stat = StatType.Health,
                    value = attacker.baseHealth * 0.01f * sariStack,
                    source = SourceType.Relic,
                    modifierId = sariId,
                    isPercent = false
                });
                attacker.stats.AddModifier(new StatModifier
                {
                    stat = StatType.AttackDamage,
                    value = attacker.baseAttackDamage * 0.01f * sariStack,
                    source = SourceType.Relic,
                    modifierId = sariId,
                    isPercent = false
                });
            }
        }
    }

    //유닛별 추가 데미지
    public float SetMultipleDamage(RogueUnitDataBase attacker, RogueUnitDataBase defender, bool isTeam)
    {
        float value = 1f;
        if (isTeam)
        {
            value = RogueLikeData.Instance.GetMyMultipleDamage();
        }
        else
        {
            value = RogueLikeData.Instance.GetEnemyMultipleDamage();
        }
        value += UpgradeManager.GetAffinityMultiplier(attacker.branchIdx, defender.branchIdx, isTeam);

        if (IsCommander(109) && IsCavalry(defender))
            value += 0.2f;

        return value;
    }

    //후열 공격 시 뎀증 변경
    private float ChangeBackMultiple(RogueUnitDataBase attacker, RogueUnitDataBase target, float damage, bool isTeam, RogueUnitDataBase backattacker = null)
    {
        if (backattacker == null)
        {
            float finalDamage = SetMultipleDamage(attacker, target, isTeam);
            damage *= finalDamage;
        }
        else
        {
            float finalDamage = SetMultipleDamage(backattacker, target, isTeam);
            damage *= finalDamage;
        }
        return damage;
    }

    // 사용처: 지원 페이즈에서 현재 위치 기준으로 원거리 공격 가능한 유닛인지 판정한다.
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

}
