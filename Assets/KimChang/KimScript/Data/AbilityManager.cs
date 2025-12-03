using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

    private AutoBattleUI autoBattleUI;

    Dictionary<int, List<RogueUnitDataBase>> myHeroUnits = new();
    Dictionary<int, List<RogueUnitDataBase>> enemyHeroUnits = new();

    public void ProcessCommenderEffect()
    {
        int presetId = RogueLikeData.Instance.GetPresetID();
        if (presetId < 48) return;
        var abilityActions = new List<Action>
        {
            ()=> {if(presetId == 49) CommenderEffect.CalculateSlash();  },
            ()=> {if (presetId ==57) CommenderEffect.CalculateLazaros(); },
            ()=>{if(presetId ==59)CommenderEffect.CalculateBelphegor(); },
            ()=>{if(presetId ==60)CommenderEffect.CalculateAmarok(); },
            ()=> {if(presetId == 62) CommenderEffect.CalculateStein(); },
        };

    }

    //입장 시 채크
    public void ProcessEnter()
    {
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
        
        switch (fieldId) 
        {
            case 2:
                {
                    var allUnits = new List<RogueUnitDataBase>();
                    allUnits.AddRange(RogueLikeData.Instance.GetMyUnits());
                    allUnits.AddRange(RogueLikeData.Instance.GetEnemyUnits());
                    foreach (var unit in allUnits)
                    {
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
                
            case 3:
                {
                    int id = 9, type = 1, rank = 1, duration = -1;
                    var allUnits = new List<RogueUnitDataBase>();
                    allUnits.AddRange(RogueLikeData.Instance.GetMyUnits());
                    allUnits.AddRange(RogueLikeData.Instance.GetEnemyUnits());
                    foreach (var unit in allUnits)
                    {
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
            case 4:
                {
                    int id = 10, type = 0, rank = 1, duration = -1;
                    var allUnits = new List<RogueUnitDataBase>();
                    allUnits.AddRange(RogueLikeData.Instance.GetMyUnits());
                    allUnits.AddRange(RogueLikeData.Instance.GetEnemyUnits());
                    foreach (var unit in allUnits)
                    {
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

        ProcessCommenderEffect();
    }
    
    public bool ProcessOneTurn()
    {
        bool isTurnEffect;
        isTurnEffect = CalculateStromMap();

        return isTurnEffect;
    }

    //폭풍우
    private bool CalculateStromMap()
    {
        int fieldId= RogueLikeData.Instance.GetFieldId();
        if(fieldId !=5) return false;

        bool isMyTeam = true;
        int randomIndex;
        RogueUnitDataBase damagedUnit;
        if (RogueLikeData.Instance.GetRandomInt(0, 2) ==0)
        {
            var myUnits = RogueLikeData.Instance.GetMyUnits();
            randomIndex = RogueLikeData.Instance.GetRandomInt(0, myUnits.Count);
            damagedUnit = myUnits[randomIndex];
        }
        else
        {
            isMyTeam = false;
            var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();
            randomIndex = RogueLikeData.Instance.GetRandomInt(0, enemyUnits.Count);
            damagedUnit = enemyUnits[randomIndex]; 
        }
        damagedUnit.health -= 30;
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
            if(RelicManager.CheckRelicById(61))
            {
                if (StatBlock.HasModifier(unit.stats, SourceType.Relic, 61))

                reduceMulty = 2;
            }
            if (unit.effectDictionary.ContainsKey(14))
            {
                reduce += 1;
            }

            unit.Energy -= reduce* reduceMulty;
        }
    }

    //전투 전 발동(패시브)
    public void ProcessBeforeBattle(List<RogueUnitDataBase> units, List<RogueUnitDataBase> defenders, bool isTeam,AutoBattleUI _autoBattleUI)
    {
        autoBattleUI = _autoBattleUI;

        //기타 유산
        if (RelicManager.CheckRelicById(68))
        {
            WarRelic relic = RelicManager.GetRelicById(68);
            var vals = relic.GetAllValuesAsFloatListOrNull();
            if (vals != null)
            {
                plunderGold += (int)vals[0];
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
        CalculateBattleHammer(units,isTeam);
        CalculateEmpire(units);
        CalculateDivinityCountry(units);
        CalculateSevenUnion(units);

        //결속
        CalculataeSolidarity(units, isTeam);


        foreach(RogueUnitDataBase unit in units)
        {
            unit.ApplyModifiers();
        }
    }
    //전투당 한번(선재 타격 등)
    public bool ProcessStartBattle(List<RogueUnitDataBase> attackers, List<RogueUnitDataBase> defenders,bool isTeam)
    {
        float finalDamage = SetMultipleDamage(attackers[0], defenders[0], isTeam);
        return (CalculateFirstStrike(attackers, defenders, finalDamage, isTeam) || CalculateManiac(defenders, isTeam));
    }

    //준비 페이즈 시 발동
    public bool ProcessPreparationAbility(List<RogueUnitDataBase> attackers,List<RogueUnitDataBase> defenders,bool isFirstAttack,bool isTeam)
    {
        if (isFirstAttack)
        {
            float _finalDamage = SetMultipleDamage(attackers[0], defenders[0], isTeam);
            RogueUnitDataBase frontAttacker = attackers[0];
            RogueUnitDataBase frontDefender = defenders[0];
            float finalDamage = _finalDamage + ((isTeam && RelicManager.CheckRelicById(46)) ? 1.2f : 1) - 1;
            string text = "";
            float damage = 0;

            var abilityActions = new List<Action>
{
    () => { if (frontAttacker.smokeScreen) CalculateSmokeScreen(attackers, isTeam); },
    () => { if (frontAttacker.overwhelm) CalculateOverwhelm(frontAttacker, frontDefender, ref text); },
    () => { if (frontAttacker.throwSpear) CalculateThrowSpear(frontAttacker,attackers, defenders, ref damage, ref text,isTeam,isFirstAttack); },
    () => { if (frontAttacker.assassination) CalculateAssassination(frontAttacker,attackers, defenders, ref damage, ref text, isTeam, isFirstAttack); },
    () => { if (frontAttacker.wounding) CalculateWounding(frontAttacker, frontDefender, ref text); }
};

            foreach (var action in abilityActions) action();

            if (damage > 0)
            {
                CallDamageText(damage, text, !isTeam, true);
                
                float relicDamage = RelicManager.RunPulsatingDoll(frontAttacker, isTeam);
                if (relicDamage > 0)
                {
                    frontAttacker.health -= relicDamage;
                    CallDamageText(relicDamage, "맥동하는인형 ", !isTeam, true);
                }

            }

                return true;
        }
        return false;
    }

    //충돌 페이즈 시 발동
    public void ProcessChrashAbility(List<RogueUnitDataBase> attackers, List<RogueUnitDataBase> defenders,bool isFirstAttack,bool isTeam)
    {
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
            ChrashIsFirstAttack(frontAttacker, defenders, ref multiplier, ref reduceDamage,ref firstText, isTeam, ref isPierce);
        }
        for (int i = 0; i < 2; i++)
        {
            string text = firstText;
            if (i == 1)
            {
                //연발
                if (!frontAttacker.doubleShot) break; // 한 번만 공격
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
                if (target==null && defenders.Count > 1)
                {
                    int unitIndex = 1;
                    for(int k=1; k < defenders.Count; k++)
                    {
                        if (defenders[k].health > 0)
                        {
                            unitIndex = k;
                            target = defenders[k];
                            break;
                        }
                    }
                    float ar = target.Armor;
                    float impactDamage = MathF.Round(normalDamage * (1f - (ar / (ar + 10f))));

                    impactDamage = ChangeBackMultiple(frontAttacker,target,impactDamage,isTeam);

                    //유산 127
                    float relicReduceDamage = impactDamage;
                    RelicManager.RunGuardiansCloak(defenders, !isTeam, ref unitIndex,ref relicReduceDamage);
                    target = defenders[unitIndex];

                    target.health -= relicReduceDamage;

                    CallDamageText(relicReduceDamage, "충격 ", !isTeam, true, unitIndex);
                    //복수
                    if (frontDefender.vengeance && unitIndex > 0)
                    {
                        //유산 127
                        relicReduceDamage = impactDamage;
                        unitIndex = 0;
                        RelicManager.RunGuardiansCloak(attackers, isTeam, ref unitIndex, ref relicReduceDamage);
                        target = attackers[unitIndex];

                        target.health -= relicReduceDamage;

                        CallDamageText(relicReduceDamage, "복수 ", isTeam, true, unitIndex);

                        float relicDamage = RelicManager.RunPulsatingDoll(frontDefender,!isTeam);
                        if (relicDamage > 0)
                        {
                            //유산 127
                            relicReduceDamage = relicDamage;
                            unitIndex = 0;
                            RelicManager.RunGuardiansCloak(defenders, !isTeam, ref unitIndex, ref relicReduceDamage);
                            target = defenders[unitIndex];

                            target.health -= relicReduceDamage;
                            CallDamageText(relicReduceDamage, "맥동하는 인형", isTeam, true,unitIndex);
                        }
                    }
                }
                else
                {
                    float impactDamage = normalDamage * (1 - target.Armor / (target.Armor + 10));
                    //유산 127
                    float relicReduceDamage = impactDamage;
                    int unitIndex = 0;
                    RelicManager.RunGuardiansCloak(defenders, !isTeam, ref unitIndex, ref relicReduceDamage);
                    target = defenders[unitIndex];

                    target.health -= MathF.Round(relicReduceDamage);

                    CallDamageText(relicReduceDamage, "충격 수호 ", isTeam, true, unitIndex);
                }
            }

            // 회피 판정
            if (CalculateAccuracy(frontDefender, frontAttacker,attackers,isTeam, isFirstAttack, 0))
            {
                normalDamage = 0;

                text = "회피 ";
            }
            else
            {
                //반격
                if (isFirstAttack && frontDefender.counter)
                {
                    if (!CalculateAccuracy(frontAttacker, frontDefender,defenders,isTeam, isFirstAttack,0))
                    {
                        //유산 127
                        float relicReduceDamage = normalDamage;
                        int unitIndex = 0;
                        RelicManager.RunGuardiansCloak(attackers, isTeam, ref unitIndex, ref relicReduceDamage);
                        RogueUnitDataBase target = defenders[unitIndex];

                        target.health -= relicReduceDamage;

                        CallDamageText(normalDamage, "반격 ", isTeam, true);

                        float relicDamage = RelicManager.RunPulsatingDoll(frontDefender, !isTeam);
                        if (relicDamage > 0)
                        {
                            //유산 127
                            relicReduceDamage = normalDamage;
                            unitIndex = 0;
                            RelicManager.RunGuardiansCloak(defenders, !isTeam, ref unitIndex, ref relicReduceDamage);
                            target = defenders[unitIndex];

                            target.health -= relicDamage;
                            CallDamageText(relicReduceDamage, "맥동하는 인형", isTeam, true,unitIndex);
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
                }

                //작열
                CalculateBurning(frontAttacker, defenders,isTeam, ref text);

                // 가시 피해
                if (frontDefender.thorns && normalDamage > 0)
                {
                    //유산 127
                    float relicReduceDamage = normalDamage;
                    int unitIndex = 0;
                    RelicManager.RunGuardiansCloak(attackers, isTeam, ref unitIndex, ref relicReduceDamage);
                    RogueUnitDataBase target = defenders[unitIndex];

                    target.health -= relicReduceDamage;

                    CallDamageText(relicReduceDamage, "가시 ", isTeam, false,unitIndex);
                }

                // 흡혈
                if (frontAttacker.lifeDrain)
                {
                    float healValue = Mathf.Round(normalDamage * bloodSuckingValue);
                    float heal = HealHealth(frontAttacker, Mathf.Min((frontAttacker.health+ healValue), frontAttacker.maxHealth));
                       
                    frontAttacker.health = heal;

                    CallDamageText(-healValue, "흡혈 ", isTeam, false);
                }

                //추적자
                CalculateTracker(frontAttacker, frontDefender);
            }

            allDamage += normalDamage;
            
            if(i==0) firstText = text;
        }

        CallDamageText(allDamage, firstText, !isTeam, true);

        CalculateChallenge(frontAttacker,ref defenders,isTeam);

        float finalRelicDamage = RelicManager.RunPulsatingDoll(frontAttacker, isTeam);
        if (finalRelicDamage > 0)
        {
            //유산 127
            float relicReduceDamage = finalRelicDamage;
            int unitIndex = 0;
            RelicManager.RunGuardiansCloak(attackers, isTeam, ref unitIndex, ref relicReduceDamage);
            RogueUnitDataBase target = defenders[unitIndex];

            target.health -= finalRelicDamage;
            CallDamageText(finalRelicDamage, "맥동하는 인형", !isTeam, true);
        }
    }
    //지원 페이즈 시 발동
    public void ProcessSupportAbility(List<RogueUnitDataBase> attackers, List<RogueUnitDataBase> defenders,bool isTeam, bool isFirstAttack) 
    {
        float finalDamage = SetMultipleDamage(attackers[0], defenders[0], isTeam);
        //원거리 공격
        var value = CalculateRangeAttack(attackers, defenders,isTeam,finalDamage, isFirstAttack);
        if (value.Item1 > 0)
        {
            int unitIndex = 0;
            float damage = value.Item1;
            
            //유산 127
            RelicManager.RunGuardiansCloak(defenders, !isTeam,ref unitIndex,ref damage);

            CallDamageText(Mathf.Round(value.Item1), value.Item2, !isTeam, false,unitIndex);
            defenders[unitIndex].health -= damage;
        }
       

        //치유
        ProcessHealing(attackers,isTeam);
        //지원 종료
        DamageBurning(attackers,isTeam);

    }
   
    // 유닛 사망 처리
    public bool ProcessDeath(
    ref List<RogueUnitDataBase> myUnits, ref List<RogueUnitDataBase> enemyUnits,
    ref List<RogueUnitDataBase> myDeathUnits, ref List<RogueUnitDataBase> enemyDeathUnits,
    ref bool isFirstAttack, RogueUnitDataBase myFrontUnit, RogueUnitDataBase enemyFrontUnit)
    {
        bool myUnitDied = false;
        bool enemyUnitDied = false;

        List<RogueUnitDataBase> tempMyDeathUnits = new();
        List<RogueUnitDataBase> tempEnemyDeathUnits = new();

        List<int> myDeathIndexes = new();
        List<int> enemyDeathIndexes = new();

        CommenderEffect.CalculateAxl();

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

                ProcessUnitDeath(myUnits, i, tempMyDeathUnits, ref myUnitDied, autoBattleUI, true);
                myDeathIndexes.Add(i);
            }
        }

        // 적 유닛 사망 스캔
        for (int i = enemyUnits.Count - 1; i >= 0; i--)
        {
            if (enemyUnits[i].health <= 0)
            {
                ProcessUnitDeath(enemyUnits, i, tempEnemyDeathUnits, ref enemyUnitDied, autoBattleUI, false);
                enemyDeathIndexes.Add(i);
            }
        }

        for (int k = 0; k < myDeathIndexes.Count; k++) myUnits.RemoveAt(myDeathIndexes[k]);
        for (int k = 0; k < enemyDeathIndexes.Count; k++) enemyUnits.RemoveAt(enemyDeathIndexes[k]);


        if (tempEnemyDeathUnits.Count > 0 || tempMyDeathUnits.Count > 0)
        {
            OnUnitDeath(tempEnemyDeathUnits, tempMyDeathUnits, ref enemyUnits, false, enemyUnitDied, myUnitDied, isFirstAttack);
            OnUnitDeath(tempMyDeathUnits, tempEnemyDeathUnits, ref myUnits, true, myUnitDied, enemyUnitDied, isFirstAttack);
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
                    damage += tempMyDeathUnits[i].health * vals[0];
                }

                enemyUnits[0].health -= damage;

                if (enemyUnits[0].health <= 0)
                {
                    ProcessUnitDeath(enemyUnits, 0, tempEnemyDeathUnits, ref enemyUnitDied, autoBattleUI, false);
                    enemyUnits.RemoveAt(0);
                }
            }
        }

        // 사용처: 이번 틱 사망자들을 전투 전체 누적 로그에 합산(조건 없이 각각 추가)
        if (tempMyDeathUnits.Count > 0) 
        {
            myDeathUnits.AddRange(tempMyDeathUnits);

            //유산 124
            if(enemyUnits.Count > 0)
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
            RelicManager.RunEndlessBarrage(tempEnemyDeathUnits.Count);

            //유산 129
            RelicManager.RunContractInvoice(tempEnemyDeathUnits, enemyFrontUnit);

        }
        //유산 104
        RelicManager.RunDoubleEdgedAxeOfPride(myUnits, myDeathUnits, enemyUnits, enemyDeathUnits);

        // 사용처: 전멸 체크
        if (myUnits.Count == 0 || enemyUnits.Count == 0)
            return true;

        // 사용처: 전열 변경 체크(어느 쪽이든 전열이 바뀌었으면 스나이퍼 계산 및 선제권 리셋)
        if ((myUnitDied || enemyUnitDied) && (myFrontUnit != myUnits[0] || enemyFrontUnit != enemyUnits[0]))
        {
            if (myFrontUnit != myUnits[0]) CalculateSniper(myUnits);
            if (enemyFrontUnit != enemyUnits[0]) CalculateSniper(enemyUnits);
            isFirstAttack = true;
            return true;
        }

        return false;
    }
    // 개별 유닛 사망 처리
    private void ProcessUnitDeath(
        List<RogueUnitDataBase> units, int index,
        List<RogueUnitDataBase> tempDeathUnits,
        ref bool unitDied, AutoBattleUI autoBattleUI, bool isMyUnit)
    {
        CalculateMartyrdom(units, index);

        units[index].alive = false;
        tempDeathUnits.Add(units[index]); // 임시 리스트에 추가

        autoBattleUI.ChangeInvisibleUnit(index, isMyUnit);

        if (index == 0) unitDied = true;
    }


    // 유닛 사망 시 실행되는 함수 (추가 기능 확장 가능)
    private void OnUnitDeath(List<RogueUnitDataBase> deadAttackers,List<RogueUnitDataBase> deadDefenders,ref List<RogueUnitDataBase> attackers, bool isTeam, bool isFrontAttackerDead,bool isFrontDefendrDead,bool isFirstAttack)
    {
        if(attackers.Count == 0) return;
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
        CalculataeSolidarity(attackers, isTeam,true);
        //넝마떼기
        RelicManager.SurvivorOfRag(attackers, isTeam);

        if (RogueLikeData.Instance.GetPresetID() == 50 && !isTeam && isFrontDefendrDead && frontAttacker.branchIdx == 1) 
        {
            RogueLikeData.Instance.ChangeMorale(-2);
        }
        else if (RogueLikeData.Instance.GetPresetID() == 61 && !isTeam && deadAttackers.Count > 0) 
        {
            float finalDamage = SetMultipleDamage(attackers[0], deadAttackers[0], isTeam);
            CalculateRangeAttack(attackers, deadAttackers,isTeam,finalDamage,isFirstAttack);
        }

        //스킬 사용 유닛이 안죽었을 시
        if (!isFrontAttackerDead)
        {
            //유격
            if ((attackers[0].guerrilla || (attackers[0].effectDictionary.ContainsKey(12) && attackers[0].effectDictionary[12].Duration > 0)) && CheckBackUnit(attackers))
            { 
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

        foreach (var unit in attackers)
        {
            unit.ApplyModifiers(true);
        }
    }
    //선제 타격
    private bool CalculateFirstStrike(List<RogueUnitDataBase> attakers, List<RogueUnitDataBase> defenders,float finalDamage,bool isTeam)
    {
        bool use = false;
        foreach (RogueUnitDataBase attacker in attakers)
        {
            if ((attacker.firstStrike || (attacker.effectDictionary.ContainsKey(13) && attacker.effectDictionary[13].Duration > 0 )  ) && !attacker.fStriked && CheckBackUnit(defenders))
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
    private void CalculateDamageFirstStrike(RogueUnitDataBase attacker, List<RogueUnitDataBase> defenders,bool isTeam,ref bool use)
    {
        RogueUnitDataBase target = CalculateBackAttack(defenders);
        if (target == null)
        {
            int minHealthIndex = CalculateMinHealthIndex(defenders);
            if (minHealthIndex == -1) return;
 
            float damage = attacker.attackDamage * 2;

            damage = ChangeBackMultiple(attacker, defenders[minHealthIndex], damage, isTeam);

            //유산 127
            RelicManager.RunGuardiansCloak(defenders, !isTeam,ref minHealthIndex,ref damage);

            damage = MathF.Round(damage);
            defenders[minHealthIndex].health -= damage;
            CallDamageText(damage, "선제타격 ", !isTeam, false, minHealthIndex);
        }
        else
        {
            int unitIndex = 0;
            float damage = attacker.attackDamage * (1 - (target.Armor / (target.Armor + 10)));
            float finaldamage = SetMultipleDamage(attacker, target, isTeam);
            damage *= finaldamage;

            //유산 127
            RelicManager.RunGuardiansCloak(defenders, !isTeam, ref unitIndex, ref damage);

            damage = MathF.Round(damage);
            target.health -= damage;

            CallDamageText(damage, "선제타격 수호 ", !isTeam, false, unitIndex);
        }

        float relicDamage = RelicManager.RunPulsatingDoll(attacker, isTeam);
        if (relicDamage > 0)
        {
            attacker.health -= relicDamage;
            CallDamageText(relicDamage, "맥동하는 인형", !isTeam, true);
        }

        attacker.fStriked = true;
        use = true;
    }
    //약탈
    private void CalculatePlunder(RogueUnitDataBase unit,bool isTeam)
    {
        //적이거나 약탈 없으면 반환
        if (!isTeam || !unit.plunder) return;
        RogueLikeData.Instance.AddGoldReward(plunderGold);
    }
    //무한
    private void CalculateEndLess(RogueUnitDataBase unit,bool isTeam)
    {
        if (!unit.endless) return;
        unit.Energy = Math.Min(unit.MaxEnergy, unit.Energy + 1);
    }
    //위압
    private void CalculateOverwhelm(RogueUnitDataBase attacker,RogueUnitDataBase defender,ref string text)
    {
        int id = 8, type = 1, rank = 1, duration = -1;
        defender.effectDictionary[id] = new BuffDebuffData(id, type, rank, duration);

        text += "위압 ";
    }
    //투창
    private void CalculateThrowSpear(RogueUnitDataBase attacker,List<RogueUnitDataBase> attackers, List<RogueUnitDataBase> defenders,ref float _damage,ref string text,bool isTeam,bool isFirstAttack)
    {
        float finalDamage = SetMultipleDamage(attacker, defenders[0],isTeam);
        float damage = throwSpearValue * finalDamage;
        if (!CalculateAccuracy(defenders[0], attacker,attackers,isTeam, isFirstAttack,0))
        {
            int unitIndex = 0;
            //유산 127
            RelicManager.RunGuardiansCloak(defenders, !isTeam, ref unitIndex, ref damage);

            damage = MathF.Round(damage);
            defenders[unitIndex].health -= damage;

            CallDamageText(damage, "투창 ", !isTeam, false, unitIndex);
            return;
        }
        text += "회피 ";
    }
    //암살
    private void CalculateAssassination(RogueUnitDataBase attacker,List<RogueUnitDataBase> attackers, List<RogueUnitDataBase> defenders, ref float _damage, ref string text,bool isTeam,bool isOnce=false)
    {
        int minHealthIndex = CalculateMinHealthIndex(defenders);
        if (minHealthIndex == -1) return;
        float damage = attacker.attackDamage * assassinationValue;

        RogueUnitDataBase target= CalculateBackAttack(defenders);

        //전열공격
        if (target != null) 
        {
            float ar = target.Armor;
            damage *= 1f - (ar / (ar + 10f));              // 방어 보정
            damage *= SetMultipleDamage(attacker, target, isTeam); // 상성/배수
            int unitIndex = 0;
            RelicManager.RunGuardiansCloak(defenders, !isTeam, ref unitIndex, ref damage);
            target = defenders[unitIndex];
            damage = MathF.Round(damage);
            target.health -= damage;
            CallDamageText(damage, "암살 수호 ", !isTeam, true, unitIndex);

            // 33% 확률로 한 번 더 실행
            if (!isOnce && RogueLikeData.Instance.GetPresetID() == 60 && !isTeam && RogueLikeData.Instance.GetRandomFloat() < 0.33f)
            {
                CalculateAssassination(attacker,attackers, defenders, ref _damage, ref text,isTeam,true);
            }
        }
        else 
        {
            if (RogueLikeData.Instance.GetPresetID() == 52)
            {
                if (RogueLikeData.Instance.GetRandomFloat() < 0.5f)
                {
                    defenders[minHealthIndex].health = 0;
                    CallDamageText(defenders[minHealthIndex].health, "암살 ", !isTeam, true, minHealthIndex);

                    return;
                }
            }

            damage = ChangeBackMultiple(attacker, defenders[minHealthIndex],damage, isTeam);
            damage = MathF.Round(damage);

            //유산 127
            RelicManager.RunGuardiansCloak(defenders, !isTeam, ref minHealthIndex, ref damage);

            defenders[minHealthIndex].health -= damage;

            CallDamageText(damage, "암살 ", !isTeam, true,minHealthIndex);
            //복수
            if (defenders[0].vengeance)
            {
                //유산 127
                RelicManager.RunGuardiansCloak(attackers, isTeam, ref minHealthIndex, ref damage);
                target = attackers[minHealthIndex];

                target.health -= damage;

                CallDamageText(damage, "복수 ", isTeam, true, minHealthIndex);
            }

            if (!isOnce && RogueLikeData.Instance.GetPresetID() == 60 && !isTeam && RogueLikeData.Instance.GetRandomFloat() < 0.33f)
            {
                CalculateAssassination(attacker,attackers, defenders, ref _damage, ref text, isTeam, true);
            }
        }
    }
    //도전
    private void CalculateChallenge(RogueUnitDataBase attaker, ref List<RogueUnitDataBase> defenders,bool isTeam)
    {
        if (attaker.challenge && attaker.health > 0 && CheckBackUnit(defenders))
        {
            int minHealthIndex = CalculateMinHealthIndex(defenders);

            RogueUnitDataBase selectUnit = defenders[minHealthIndex];
            defenders.RemoveAt(minHealthIndex);
            defenders.Insert(0, selectUnit);

            CallDamageText(0,"도전",!isTeam,false, minHealthIndex);
        }
    }
    //상흔
    private void CalculateWounding(RogueUnitDataBase attacker, RogueUnitDataBase defender, ref string text)
    {
        int scarId = 1, type = 1, rank = 1, duration = -1;
        defender.effectDictionary[scarId] = new BuffDebuffData(scarId, type, rank, duration);

        text += "상흔 ";
    }
    //충돌 isFirstAttack
    private void ChrashIsFirstAttack(RogueUnitDataBase attacker, List<RogueUnitDataBase> defenders,ref float multiplier,ref float reduceDamage,ref string text,bool isTeam, ref bool isPierce)
    {
        RogueUnitDataBase defender =defenders[0];
        //돌격
        if (attacker.charge)
        {
            if (isTeam && RelicManager.CheckRelicById(135))
            {
                isPierce = true;
            }

            multiplier = CalculateCharge(attacker.Mobility);
            if (attacker.effectDictionary.ContainsKey(11) && RogueLikeData.Instance.GetRandomFloat() < 0.33f)
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
    private (float,string) ApplyChrashAbility(RogueUnitDataBase attacker, RogueUnitDataBase defender, bool isTeam, float _reduceDamage,string _text)
    {
        float reduceDamage =_reduceDamage;
        string text = _text;
        Dictionary<Func<RogueUnitDataBase, bool>, Action> traitEffects = new()
        {
            { unit => unit.bluntWeapon && defender.heavyArmor, () => CalculateBluntWeapon(attacker,isTeam,ref reduceDamage,ref text) }, // 둔기
            { unit => unit.slaughter && defender.lightArmor, () => CalculateSlaughter(ref reduceDamage,ref text) }, // 도살
            { unit => unit.suppression && reduceDamage < 0, () => CalculateSuppression(defender,ref reduceDamage,ref text) } // 제압
        };

        foreach (var trait in traitEffects)
        {
            if (trait.Key(attacker))
            {
                trait.Value();
            }
        }
        return (reduceDamage,text);
    }
    // 돌격 계산
    private float CalculateCharge(float mobility)
    {
        return ((0.95f / 100f) * (mobility * mobility))+1.05f;
    }
    //둔기
    private void CalculateBluntWeapon(RogueUnitDataBase unit,bool isTeam,ref float reduceDamage,ref string text)
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
    private void CalculateSlaughter(ref float reduceDamage,ref string text)
    {
        reduceDamage -= slaughterValue;

        text += "도살 ";
    }
    //대기병
    private void CalculateAntiCavalry(ref float reduceDamage,ref string text, RogueUnitDataBase attaker)
    {
        reduceDamage -= attaker.antiCavalry;

        text += "대기병 ";
    }
    //제압
    private void CalculateSuppression(RogueUnitDataBase defender,ref float reduceDamage, ref string text)
    {
        reduceDamage += defender.maxHealth* suppressionValue;

        text += "제압 ";
    }
    //회피율 계산
    public float CalculateDodge(RogueUnitDataBase unit,bool isTeam,bool isFirstAttack)
    {
        float dodge;
        int mobility = unit.Mobility;
        if (RogueLikeData.Instance.GetPresetID()==58)
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
            dodge = (2 + ((mulityDodge / 9) * (unit.Mobility - 1))) + (unit.agility ? 10f : 0) + addDodge + extra;
            foreach (var key in dodgeEffects.Keys)
            {
                if (unit.effectDictionary.ContainsKey(key)) dodgeEffects[key]();
            }

            dodge = (2 + ((mulityDodge / 9) * (unit.Mobility - 1))) + (unit.agility ? 10.0f : 0) + addDodge;
        }

        return MathF.Floor(Mathf.Clamp(dodge, 0, 100));
    }

    //회피 유무 계산
    private bool CalculateAccuracy(RogueUnitDataBase defender, RogueUnitDataBase attacker,List<RogueUnitDataBase> attackers,bool isTeam,bool isFirstAttack, int _unitIndex)
    {
        if (attacker.perfectAccuracy)
            return false; // 필중 특성인 경우 회피 불가

        float dogeRate = CalculateDodge(defender,isTeam,isFirstAttack);

        bool isDodge = dogeRate >= RogueLikeData.Instance.GetRandomInt(0, 101);
        if (isDodge)
        {
            //방어자가 회피 성공시 암살단장의 효과 발동 isTeam==true라는건 attacker가 내 유닛이라는것 defender는 이때 enemy가 됨
            if(isTeam && enemyHeroUnits.TryGetValue(58, out List<RogueUnitDataBase> heroList))
            {
                float damage = 20 * heroList.Count;
                //유산 127
                float relicReduceDamage = damage;
                int unitIndex = _unitIndex;
                RelicManager.RunGuardiansCloak(attackers, isTeam, ref unitIndex, ref relicReduceDamage);
                RogueUnitDataBase target = attackers[unitIndex];

                target.health -= relicReduceDamage;

                CallDamageText(relicReduceDamage, "암살단장 ", isTeam, true,unitIndex);
            }
            else if(!isTeam && myHeroUnits.TryGetValue(58, out List<RogueUnitDataBase> myHeroList))
            {
                float damage = 20 * myHeroList.Count;
                attacker.health -= damage;

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
    private void CalculateSmokeScreen(List<RogueUnitDataBase> units,bool isTeam)
    {
        if (CheckBackUnit(units))
        {
            int id = 2, type = 0, rank = 1, duration = -1;
            for (int i = 1; i < units.Count; i++)
            {
                if(units[i].health>0 && !units[i].effectDictionary.ContainsKey(id))
                {
                    units[i].effectDictionary[id] = new BuffDebuffData(id, type, rank, duration);
                }
            }

            CallDamageText(0, "연막 ", isTeam, false,1);
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
                int scarId = 1, sType = 1, sRank =1, sDuration = -1;
                if (!defender.effectDictionary.TryGetValue(burningId, out BuffDebuffData sEffect))
                {
                    defender.effectDictionary[scarId] = new BuffDebuffData(scarId, sType, sRank, sDuration);
                    text += "상흔 ";
                }

                //위압
                int oId = 8, oType = 1, oRank = 1, oDuration = -1;
                if (!defender.effectDictionary.TryGetValue(burningId, out BuffDebuffData oEffect))
                {
                    defender.effectDictionary[oId] = new BuffDebuffData(oId, oType, oRank, oDuration);
                    text += "위압 ";
                }
            }
        }

        text += "작열 ";

    }

    //작열 적용
    private void CalculateBurning(RogueUnitDataBase attacker, List<RogueUnitDataBase> defenders,bool isTeam ,ref string text)
    {
        if (attacker.scorching)
        {
            ProcessBurning(defenders[0],isTeam,ref text);
        }
    }
    //작열 데미지
    private void DamageBurning(List<RogueUnitDataBase> units, bool isTeam)
    {
        int burningId = 0;

        for(int i=0; i<units.Count;i++)
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

            CallDamageText(damage, "작열", isTeam, false);
        }
    }

    // 치유
    private void ProcessHealing(List<RogueUnitDataBase> units,bool isTeam)
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

    // 원거리 공격 최적화 코드
    private (float,string) CalculateRangeAttack(List<RogueUnitDataBase> attackers, List<RogueUnitDataBase> defenders, bool isTeam, float finalDamage,bool isFirstAttack)
    {
        float allDamage = 0;
        string text = "원거리 ";
        for(int i =1; i< attackers.Count; i++)
        {
            RogueUnitDataBase attacker = attackers[i];
            if (!attacker.rangedAttack || attacker.health <= 0 || attacker.range - attackers.IndexOf(attacker) < 1)
                continue;

            float damage = attacker.attackDamage;
            damage = ChangeBackMultiple(attackers[0], defenders[0], damage, isTeam, attacker);

            for (int k = 0; k < 2; k++)
            {
                if (k == 1 && !attacker.doubleShot) break;

                if (CalculateAccuracy(defenders[0], attacker,attackers,isTeam, isFirstAttack,i))
                    continue;

                if (damage > 0 &&defenders[0].heavyArmor && !attacker.pierce)
                {
                    damage = Mathf.Max(0, damage - heavyArmorValue);
                }
                
                CalculateBurning(attacker, defenders,isTeam, ref text);
                CalculateTracker(attacker, defenders[0]);
                CalculateReaper(isTeam);

                if (damage > 0)
                {
                    if (defenders[0].thorns)
                    {
                        attacker.health -= thornsDamageValue;
                    }

                    float finalRelicDamage = RelicManager.RunPulsatingDoll(attacker, isTeam);
                    if (finalRelicDamage > 0)
                    {
                        attacker.health -= finalRelicDamage;
                        CallDamageText(finalRelicDamage, "맥동하는인형 ", !isTeam, true);
                    }
                }

                allDamage += damage;
            }
        }
        
        
        return (allDamage, text);
    }

    //순교 0,1번이 동시에 사망해도 1번에 버프
    private void CalculateMartyrdom(List<RogueUnitDataBase> defenders,int defenderIndex)
    {
        int id = 8;
        if (defenders[defenderIndex].martyrdom)
        {
            if(defenderIndex+1 < defenders.Count && defenders[defenderIndex + 1].health>0)
            {
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
    private void CalculateTracker(RogueUnitDataBase attacker,RogueUnitDataBase defender)
    {
        if(attacker.idx==48 && !defender.effectDictionary.ContainsKey(3))
        {
            int id = 3, type = 1, rank = 1, durateion = -1;
            defender.Armor = Math.Max(defender.Armor - 3, 0);
            defender.effectDictionary[id] = new(id,type, rank, durateion);
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
        List<RogueUnitDataBase> heroUnits= GetHeroUnitList(isTeam,heroId);
        if(heroUnits.Count <= 0) return ;
        foreach (var unit in heroUnits)
        {
            if (unit.health > 0)
            {
                unit.maxHealth += 10 * deadDefenderCount;
                unit.health += 10* deadDefenderCount;
                unit.attackDamage += 5 * deadDefenderCount;
            }
        }

    }
    //돌격대장
    private void CalculateAssaultLeader(List<RogueUnitDataBase> units,bool isTeam)
    {
        if (!isTeam || units[0].idx != 46 || units[0].health <1) return;
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
                if(unit.tagIdx ==1) unit.bindingForce = true;
            }
        }
        else if(!isTeam)
        {
            foreach (var unit in units)
            {
                if(unit.tagIdx ==1) unit.bindingForce = true;
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
                        value = 40* wandererList.Count,
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
            for (int i = 0; i < 4; i++) // 4회 실행
            {
                CalculateDamageFirstStrike(hero, defenders,isTeam,ref use);
            }
        }

        return use;
    }
    //기괴한 주교
    private void CalculateBizarreBishop(List<RogueUnitDataBase> units,bool isTeam)
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
    private void CalculateImmortality(ref List<RogueUnitDataBase> units,List<RogueUnitDataBase> deadUnits)
    {
        int id = 7;
        for (int i = 0; i < deadUnits.Count; i++) 
        {
            RogueUnitDataBase unit = deadUnits[i];
            if (unit.effectDictionary.ContainsKey(id))
            {
                unit.effectDictionary.Remove(id);
                unit.health = 10;
                units.Add(unit);
            }

        }
    }
    // 노인 기사 효과 적용 (랜덤 특성 획득)
    private void CalculateOldKnight(RogueUnitDataBase unit,bool isFrontDefenderDead)
    {
        if (unit.idx != 52 || !isFrontDefenderDead || unit.health <= 0) return;

        unit.SetRandomTraits();
    }
    //미치광이 전투 시작 시
    private bool CalculateManiac(List<RogueUnitDataBase> defenders,bool isTeam)
    {
        int heroId = 53;
        List<RogueUnitDataBase> heroList= GetHeroUnitList(isTeam,heroId);
        if (heroList.Count == 0) return false;

        for(int i = 0; i < defenders.Count; i++)
        {
            var unit = defenders[i];
            string text = "";

            ProcessBurning(unit, isTeam, ref text);

            CallDamageText(0, text, !isTeam, false, i);
        }

        return true;
    }
    // 미치광이 전열 효과
    private void CalculateFrontManiac(RogueUnitDataBase attacker, List<RogueUnitDataBase> defenders,bool isTeam)
    {
        if (attacker.idx != 53) return; 
        if (!CheckBackUnit(defenders)) return; 

        List<RogueUnitDataBase> backUnits = defenders.Skip(1).Where(unit => unit.health > 0).ToList();
        int debuffTargetCount = Mathf.Min(3, backUnits.Count);

        List<RogueUnitDataBase> selectedTargets = backUnits.OrderBy(x => RogueLikeData.Instance.GetRandomFloat()).Take(debuffTargetCount).ToList();

        for (int i = 0; i < selectedTargets.Count; i++)
        {
            var unit = defenders[i];
            string text = "";

            ProcessBurning(unit, isTeam, ref text);

            CallDamageText(0, text, !isTeam, false, i);
        }
    }
    //사신
    private float CalculateReaper(bool isTeam)
    {
        int heroId = 54;
        List<RogueUnitDataBase> heroUnits = GetHeroUnitList(isTeam,heroId);
        if(heroUnits.Count <= 0) return 0f;

        float damage = 0f;
        foreach (var unit in heroUnits)
        {
            if(unit.health > 0) damage += unit.attackDamage;
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
            RogueUnitDataBase newUnit = UnitLoader.Instance.GetCloneUnitById(0,isTeam);

            units.Insert(0, newUnit);
        }
    }

    //불굴의 방패 전투 참여 시
    private void CalculateIndomitableShield(List<RogueUnitDataBase> units,bool isTeam)
    {
        int heroId = 57;
        int heroCount = GetHeroUnitList(isTeam, heroId).Count;
        if (heroCount <= 0) return;
        foreach (var unit in units)
        {
            if (unit.heavyArmor) { 
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
        int heroCount = GetHeroUnitList(isTeam,heroId).Count;
        if (heroCount <= 0) return;

        int deadHeavyArmorCount = deadUnits.Count(unit => unit.heavyArmor);
        if (deadHeavyArmorCount <= 0) return;

        int healthIncrease = 10 * deadHeavyArmorCount* heroCount;

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
    private void CalculateSpearOfStorm(List<RogueUnitDataBase> units,bool isTeam)
    {
        int heroId = 58;
        List <RogueUnitDataBase> heroUnits = GetHeroUnitList(isTeam, heroId);
        if(heroUnits.Count <= 0) return;
        foreach (var unit in units)
        {
            unit.attackDamage += unit.baseAntiCavalry * 0.5f;
        }
    }
    //폭풍의 창 회피율 증가
    private float CalculateSpearOfStormDodge(RogueUnitDataBase unit,bool isTeam,bool isFirstAttack)
    {
        if(!isFirstAttack) return 0;
        int heroId = 58;
        List<RogueUnitDataBase> heroUnits = GetHeroUnitList(isTeam, heroId);
        if (heroUnits.Count <= 0 || unit.branchIdx !=0) return 0f;
        return 0.5f;
    }
    //결속 발동
    public static void CalculataeSolidarity(List<RogueUnitDataBase> units, bool isTeam,bool isBattle =false)
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
                    if(unit.idx == idx)
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
                    if(unit.idx == idx)
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
                foreach(var unit in findUnits)
                {
                    if(unit.idx == idx)
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
            foreach(var unit in units)
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
                    value = unit.baseAttackDamage*0.03f,
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
        // 첫 번째로 살아있는 유닛을 찾음
        foreach (var defender in defenders)
        {
            if (defender.health > 0) // 체력이 0보다 크다면 살아있는 유닛
            {
                return defender.guard ? defender : null; // guard가 있다면 해당 유닛 반환, 없다면 null 반환
            }
        }
        return null; // 모든 유닛이 죽어있다면 null 반환
    }

    //데미지 ui 호출
    private void CallDamageText(float damage, string text, bool team,bool isAttack ,int unitIndex = 0)
    {
        //team? 나의 공격 : 상대 공격
        CommenderEffect.CalculateZander(team, unitIndex);
        autoBattleUI.ShowDamage(MathF.Round(damage), text, team, isAttack,unitIndex);
    }

    //사리유산
    private void CalculateSariRelic(List<RogueUnitDataBase> attackers, bool isTeam, bool isDefenderDead)
    {
        if (!isTeam) return;
        if (attackers[0].idx ==63 && isDefenderDead && RelicManager.CheckRelicById(78))
        {
            RogueLikeData.Instance.AddSariStack(1);
            int sariId = 78;
            int sariStack = RogueLikeData.Instance.GetSariStack();

            //초기화 및 추가 적용
            foreach (var attacker in attackers)
            {
                attacker.stats.RemoveModifiersBySourceAndId(SourceType.Skill,sariId);

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
    public float SetMultipleDamage(RogueUnitDataBase attacker,RogueUnitDataBase defender,bool isTeam)
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
        return value += UpgradeManager.GetAffinityMultiplier(attacker.branchIdx, defender.branchIdx, isTeam);
    }

    //후열 공격 시 뎀증 변경
    private float ChangeBackMultiple(RogueUnitDataBase attacker, RogueUnitDataBase target,float damage,bool isTeam, RogueUnitDataBase backattacker=null)
    {
        if(backattacker == null)
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
}
