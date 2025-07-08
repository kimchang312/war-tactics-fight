using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

public class AbilityManager
{
    private float heavyArmorValue = 15.0f;               
    private float myBluntWeaponValue = 0.1f;            
    private float enemyBluntWeaponValue = 0.1f;             
    private float throwSpearValue = 50.0f;            
    private float overwhelmValue = 1.0f;           
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
    private float mybindingHealth = 15;
    private float eneymybindingHealth = 15;
    private float mybindingAttackDamage = 5;                  
    private float enemybindingAttackDamage = 5;
    private float moraleMultiplier = 0f;
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
        switch (type) 
        {
            case StageType.Combat:
                ReduceUnitEngery();
                break;
            case StageType.Elite:
                ReduceUnitEngery();
                break;
            case StageType.Boss:
                ReduceUnitEngery();
                if (RelicManager.CheckRelicById(39)) RogueLikeData.Instance.ChangeMorale(15);
                break;
            default:
                break;
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
                        unit.armor++;
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

    private bool CalculateStromMap()
    {
        int fieldId= RogueLikeData.Instance.GetFieldId();
        if(fieldId !=5) return false;
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();

        int randomIndex = UnityEngine.Random.Range(0, myUnits.Count);
        myUnits[randomIndex].health -= 30;
        myUnits[randomIndex].health = Mathf.Max(myUnits[randomIndex].health, 0);
        CallDamageText(30, "폭풍우 ", true, false, randomIndex);

        int randomI = UnityEngine.Random.Range(0, enemyUnits.Count);
        enemyUnits[randomI].health -= 30;
        enemyUnits[randomI].health = Mathf.Max(enemyUnits[randomI].health, 0);
        CallDamageText(30, "폭풍우 ", false, false, randomI);

        return true;
    }

    // 유닛 기력 감소
    private void ReduceUnitEngery()
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        bool hasRelic = RogueLikeData.Instance.GetOwnedRelicById(2) != null;
        bool isReduce = RelicManager.CheckRelicById(105);
        foreach (var unit in myUnits)
        {
            if (unit.branchIdx == 8) continue;
            if (hasRelic && Random.value < 0.25f)
                continue;
            int reduce = 1;
            if (isReduce && Random.value < 0.8f)
                reduce *= 2;
            unit.energy = Math.Max(0, unit.energy - reduce);
        }
    }

    //전투 전 발동(패시브)
    public void ProcessBeforeBattle(List<RogueUnitDataBase> units, List<RogueUnitDataBase> defenders, bool isTeam,AutoBattleUI _autoBattleUI)
    {
        autoBattleUI = _autoBattleUI;

        //기타 유산
        if (RelicManager.CheckRelicById(68)) plunderGold += 20;

        CheckHeroUnit(units, isTeam);

        //사기 발동
        //CalculateFirstMorale(units,isTeam);

        //유닛 효과
        CalculateBloodPriest(units);
        CalculateCorpsCommander(isTeam);
        CalculateNomadicChief(units, isTeam);
        CalculateWanderer(isTeam);
        CalculateRebelLeader(units, isTeam);
        CalculateIndomitableShield(units, isTeam);
        //CalculateSpearOfStorm(units, isTeam);

        //시너지
        //CalculateWarden(units);
        CalculateLongSwordMan(units);
        CalculateLongBowMan(units);
        CalculateSteelCastle(units);
        CalculateBattleHammer(units,isTeam);
        CalculateEmpire(units);
        CalculateDivinityCountry(units);
        CalculateSevenUnion(units);

        //결속
        CalculataeSolidarity(units, isTeam);

        //유닛 강화
        //CalculateUpgradeUnit(units, isTeam);

        foreach(RogueUnitDataBase unit in units)
        {
            unit.ApplyModifiers();
        }
    }
    //전투당 한번(선재 타격 등)
    public bool ProcessStartBattle(List<RogueUnitDataBase> units, List<RogueUnitDataBase> defenders,float finalDamage,bool isTeam)
    {
        return (CalculateFirstStrike(units, defenders, finalDamage, isTeam) || CalculateManiac(defenders, isTeam));
    }

    //준비 페이즈 시 발동
    public bool ProcessPreparationAbility(List<RogueUnitDataBase> attackers,List<RogueUnitDataBase> defenders,bool isFirstAttack,bool isTeam,float _finalDamage)
    {
        if (isFirstAttack)
        {
            RogueUnitDataBase frontAttacker = attackers[0];
            RogueUnitDataBase frontDefender = defenders[0];
            float finalDamage = _finalDamage + ((isTeam && RelicManager.CheckRelicById(46)) ? 1.2f : 1) - 1;
            string text = "";
            float damage = 0;

            // 특성과 기술을 한 번에 적용할 수 있도록 Dictionary 활용
            var abilityActions = new List<Action>
{
    () => { if (frontAttacker.smokeScreen) CalculateSmokeScreen(attackers, isTeam); },
    () => { if (frontAttacker.overwhelm) CalculateOverwhelm(frontAttacker, frontDefender, ref text); },
    () => { if (frontAttacker.throwSpear) CalculateThrowSpear(frontAttacker, frontDefender, finalDamage, ref damage, ref text,isTeam,isFirstAttack); },
    () => { if (frontAttacker.assassination) CalculateAssassination(frontAttacker, defenders, finalDamage, ref damage, ref text, isTeam, isFirstAttack); },
    () => { if (frontAttacker.wounding) CalculateWounding(frontAttacker, frontDefender, ref text); }
};

            foreach (var action in abilityActions) action();

            if (damage > 0) CallDamageText(damage, text, !isTeam,true);

            return true;
        }
        return false;
    }

    //충돌 페이즈 시 발동
    public void ProcessChrashAbility(List<RogueUnitDataBase> attackers, List<RogueUnitDataBase> defenders,bool isFirstAttack,float finalDamage,bool isTeam)
    {
        float multiplier=1;
        float reduceDamage = 0;
        RogueUnitDataBase frontAttaker = attackers[0];
        RogueUnitDataBase frontDefender = defenders[0];
        float allDamage = 0;
        string firstText = "충돌 ";
        if (isFirstAttack)
        {
            ChrashIsFirstAttack(frontAttaker, defenders, ref multiplier, ref reduceDamage,ref firstText, isTeam);
        }
        for (int i = 0; i < 2; i++)
        {
            string text = firstText;
            if (i == 1)
            {
                //연발
                if (!frontAttaker.doubleShot) break; // 한 번만 공격
                text += "연발 ";
            }
            (reduceDamage, text) = ApplyChrashAbility(frontAttaker, frontDefender, isTeam, reduceDamage, text);

            float damage = frontAttaker.attackDamage * multiplier;

            //관통
            if (!frontAttaker.pierce)
            {
                damage *= (1 - frontDefender.armor / (frontDefender.armor + 10));
            }
            else
            {
                if(i==0) text += "관통 ";  
            }
            damage = (damage - reduceDamage) * finalDamage;

            float normalDamage = MathF.Round(damage);
            //충격
            if (isFirstAttack && frontAttaker.charge && frontAttaker.impact)
            {                
                RogueUnitDataBase target = CalculateBackAttack(defenders);
                if (target==null && defenders.Count > 1)
                {
                    for(int k=1; k < defenders.Count; k++)
                    {
                        if (defenders[k].health > 0)
                        {
                            target = defenders[k];
                            break;
                        }
                    }
                    float impactDamage = MathF.Round(normalDamage * (1 - target.armor / (target.armor + 10)));

                    impactDamage = ChangeBackMultiple(frontAttaker,frontDefender,target,impactDamage);

                    target.health -= impactDamage;

                    CallDamageText(impactDamage, "충격 ", !isTeam, true, 1);
                    //복수
                    if (frontDefender.vengeance)
                    {
                        frontAttaker.health -= impactDamage;

                        CallDamageText(impactDamage, "복수 ", isTeam, true);
                    }
                }
                else
                {
                    float impactDamage = MathF.Round(normalDamage * (1 - target.armor / (target.armor + 10)));
                    target.health -= impactDamage;

                    CallDamageText(impactDamage, "충격 수호 ", isTeam, true);
                }
            }

            // 회피 판정
            if (CalculateAccuracy(frontDefender, frontAttaker,isTeam, isFirstAttack))
            {
                normalDamage = 0;

                text = "회피 ";
            }
            else
            {
                //반격
                if (isFirstAttack && frontDefender.counter)
                {
                    if (!CalculateAccuracy(frontAttaker, frontDefender,isTeam, isFirstAttack))
                    {
                        frontAttaker.health -= normalDamage;

                        CallDamageText(normalDamage, "반격 ", isTeam, true);
                    }
                }
                else
                {
                    frontDefender.health -= normalDamage;
                }

                //작열
                CalculateBurning(frontAttaker, defenders, ref text);

                // 가시 피해
                if (frontDefender.thorns && normalDamage > 0)
                {
                    frontAttaker.health -= thornsDamageValue;

                    CallDamageText(thornsDamageValue, "가시 ", isTeam, false);
                }

                // 흡혈
                if (frontAttaker.lifeDrain)
                {
                    float healValue = Mathf.Round(normalDamage * bloodSuckingValue);
                    float heal = HealHealth(frontAttaker, Mathf.Min((frontAttaker.health+ healValue), frontAttaker.maxHealth));
                       
                    frontAttaker.health = heal;

                    CallDamageText(-healValue, "흡혈 ", isTeam, false);
                }

                //추적자
                CalculateTracker(frontAttaker, frontDefender);
            }

            allDamage += normalDamage;
            
            if(i==0) firstText = text;
        }

        CallDamageText(allDamage, firstText, !isTeam, true);

        CalculateChallenge(frontAttaker,ref defenders,isTeam);
    }
    //지원 페이즈 시 발동
    public void ProcessSupportAbility(List<RogueUnitDataBase> attackers, List<RogueUnitDataBase> defenders,bool isTeam,float finalDamage, bool isFirstAttack) 
    {
        //원거리 공격
        float damage = MathF.Round(CalculateRangeAttack(attackers, defenders,isTeam,finalDamage, isFirstAttack));

        defenders[0].health -= damage;

        //치유
        ProcessHealing(attackers,isTeam);
        //지원 종료
        DamageBurning(attackers[0],isTeam);

    }

    // 유닛 사망 처리
    public bool ProcessDeath(
        ref List<RogueUnitDataBase> myUnits, ref List<RogueUnitDataBase> enemyUnits,
        ref List<RogueUnitDataBase> myDeathUnits, ref List<RogueUnitDataBase> enemyDeathUnits,
        ref bool isFirstAttack,RogueUnitDataBase myFrontUnit,RogueUnitDataBase enemyFrontUnit)
    {
        bool myUnitDied = false;
        bool enemyUnitDied = false;

        List<RogueUnitDataBase> tempMyDeathUnits = new();
        List<RogueUnitDataBase> tempEnemyDeathUnits = new();

        List<int> myDeathIndexes = new();
        List<int> enemyDeathIndexes = new();

        CommenderEffect.CalculateAxl();

        // 내 유닛 사망 처리
        for (int i = myUnits.Count - 1; i >= 0; i--)
        {
            if (myUnits[i].health <= 0)
            {
                if (i == 0 && RelicManager.CheckRelicById(27))
                {
                    myUnits[i].health = myUnits[i].maxHealth;
                    continue;
                }

                ProcessUnitDeath(myUnits, i, tempMyDeathUnits, ref myUnitDied, autoBattleUI, true);
                myDeathIndexes.Add(i);
            }
        }

        // 상대 유닛 사망 처리
        for (int i = enemyUnits.Count - 1; i >= 0; i--)
        {
            if (enemyUnits[i].health <= 0)
            {
                ProcessUnitDeath(enemyUnits, i, tempEnemyDeathUnits, ref enemyUnitDied, autoBattleUI, false);
                enemyDeathIndexes.Add(i);
            }
        }

        // 유닛 실제 삭제 (루프 내에서 삭제하지 않음 → 안정적인 삭제)
        foreach (int index in myDeathIndexes.OrderByDescending(i => i)) myUnits.RemoveAt(index);
        foreach (int index in enemyDeathIndexes.OrderByDescending(i => i)) enemyUnits.RemoveAt(index);

        // 사망한 유닛들에 대한 이벤트 한 번만 호출
        if (tempEnemyDeathUnits.Count > 0 || tempMyDeathUnits.Count > 0)
        {
            //공격자 적
            OnUnitDeath(tempEnemyDeathUnits,tempMyDeathUnits, ref enemyUnits, false, enemyUnitDied, myUnitDied,isFirstAttack);
            //공격자 나
            OnUnitDeath(tempMyDeathUnits, tempEnemyDeathUnits, ref myUnits, true, myUnitDied, enemyUnitDied, isFirstAttack);
        }

        if (tempMyDeathUnits.Count > 0 && enemyUnits.Count > 0 && RelicManager.CheckRelicById(54))
        {
            enemyUnits[0].health -= tempMyDeathUnits.Sum(unit => unit.health * 0.1f);

            if (enemyUnits[0].health <= 0)
            {
                ProcessUnitDeath(enemyUnits, 0, tempEnemyDeathUnits, ref enemyUnitDied, autoBattleUI, false);
                enemyDeathIndexes.Add(0);
            }
        }

        // 사망한 유닛을 최종 사망 리스트에 추가
        myDeathUnits.AddRange(tempMyDeathUnits);
        enemyDeathUnits.AddRange(tempEnemyDeathUnits);

        //둘중 한쪽의 유닛이 없을 때
        if (myUnits.Count == 0 || enemyUnits.Count == 0) return true;

        //첫번쨰 유닛 사망 채크 || 변경 채크
        if (myFrontUnit != myUnits[0] || enemyFrontUnit != enemyUnits[0])
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
        //사리유산
        CalculateSariRelic(attackers[0], isTeam, isFrontDefendrDead);
        //결속
        CalculataeSolidarity(attackers, isTeam);
        //넝마떼기
        RelicManager.SurvivorOfRag(attackers, isTeam);

        if (RogueLikeData.Instance.GetPresetID() == 50 && !isTeam && isFrontDefendrDead && frontAttacker.branchIdx == 1) 
        {
            RogueLikeData.Instance.ChangeMorale(-2);
        }
        else if (RogueLikeData.Instance.GetPresetID() == 61 && !isTeam && deadAttackers.Count>0) 
        {
            float finalDamage = RogueLikeData.Instance.GetEnemyMultipleDamage();
            CalculateRangeAttack(attackers, deadAttackers,isTeam,finalDamage,isFirstAttack);
        }

        //스킬 사용 유닛이 안죽었을 시
        if (!isFrontAttackerDead)
        {
            //유격
            if (attackers[0].guerrilla && CheckBackUnit(attackers))
            {
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
            unit.ApplyModifiers(false);
        }
    }
    //선제 타격
    private bool CalculateFirstStrike(List<RogueUnitDataBase> attakers, List<RogueUnitDataBase> defenders,float finalDamage,bool isTeam)
    {
        bool use = false;
        foreach (RogueUnitDataBase attacker in attakers)
        {
            if (attacker.firstStrike && !attacker.fStriked && CheckBackUnit(defenders))
            {
                CalculateDamageFirstStrike(attacker,defenders,finalDamage,isTeam,ref use);
            }
        }

        use |= ExecuteAssassinationLeaderStrike(isTeam, defenders, finalDamage);
        return use;
    }
    //선제타격 데미지 계산
    private void CalculateDamageFirstStrike(RogueUnitDataBase attacker, List<RogueUnitDataBase> defenders,float finalDamage,bool isTeam,ref bool use)
    {
        RogueUnitDataBase target = CalculateBackAttack(defenders);
        if (target == null)
        {
            int minHealthIndex = CalculateMinHealthIndex(defenders);
            if (minHealthIndex == -1) return;

            float damage = MathF.Round(attacker.attackDamage * 2 * finalDamage);

            damage = ChangeBackMultiple(attacker, defenders[0], defenders[minHealthIndex], damage);

            defenders[minHealthIndex].health -= damage;
            CallDamageText(damage, "선제타격 ", !isTeam, false, minHealthIndex);
        }
        else
        {
            float damage = MathF.Round(attacker.attackDamage * (1 - (target.armor / (target.armor + 10))));
            target.health -= damage;

            CallDamageText(damage, "선제타격 수호 ", !isTeam, false);
        }
        attacker.fStriked = true;
        use = true;
    }
    //약탈
    private void CalculatePlunder(RogueUnitDataBase unit,bool isTeam)
    {
        //적 or 약탈 없음
        if (!isTeam || !unit.plunder) return;
        RogueLikeData.Instance.AddGoldReward(plunderGold);
    }
    //무한
    private void CalculateEndLess(RogueUnitDataBase unit,bool isTeam)
    {
        if (!unit.endless) return;
        unit.energy = Math.Min(unit.maxEnergy, unit.energy + 1);
    }
    //위압
    private void CalculateOverwhelm(RogueUnitDataBase attacker,RogueUnitDataBase defender,ref string text)
    {
        defender.mobility = overwhelmValue;
        text += "위압 ";
    }
    //투창
    private void CalculateThrowSpear(RogueUnitDataBase attacker, RogueUnitDataBase defender,float finalDamage,ref float _damage,ref string text,bool isTeam,bool isFirstAttack)
    {
        float damage = MathF.Round(throwSpearValue * finalDamage);
        if (!CalculateAccuracy(defender, attacker,isTeam, isFirstAttack))
        {
            defender.health -= damage;

            text += "투창 ";
            _damage += damage;
            return;
        }
        text += "회피 ";
    }
    //암살
    private void CalculateAssassination(RogueUnitDataBase attacker, List<RogueUnitDataBase> defenders, float finalDamage, ref float _damage, ref string text,bool isTeam,bool isOnce=false)
    {
        int minHealthIndex = CalculateMinHealthIndex(defenders);
        if (minHealthIndex == -1) return;
        float damage = MathF.Round(attacker.attackDamage * assassinationValue * finalDamage);

        RogueUnitDataBase target= CalculateBackAttack(defenders);

        if (target != null) 
        {

            target.health -= MathF.Round(damage * (1 - (target.armor / (target.armor + 10))));
            _damage += damage;

            CallDamageText(damage, "암살 수호 ", !isTeam, true);

            // 33% 확률로 한 번 더 실행
            if (!isOnce && RogueLikeData.Instance.GetPresetID() == 60 && !isTeam && Random.value < 0.33f)
            {
                CalculateAssassination(attacker, defenders, finalDamage, ref _damage, ref text,isTeam,true);
            }
        
        }
        else 
        {
            if (RogueLikeData.Instance.GetPresetID() == 52)
            {
                if (Random.value < 0.5f)
                {
                    defenders[minHealthIndex].health = 0;
                    CallDamageText(defenders[minHealthIndex].health, "암살 ", !isTeam, true, minHealthIndex);

                    return;
                }
            }

            damage = ChangeBackMultiple(attacker, defenders[0], defenders[minHealthIndex],damage);

            defenders[minHealthIndex].health -= damage;

            CallDamageText(damage, "암살 ", !isTeam, true,minHealthIndex);
            //복수
            if (defenders[0].vengeance)
            {
                attacker.health -= damage;

                CallDamageText(damage, "복수 ", isTeam, true);
            }

            if (!isOnce && RogueLikeData.Instance.GetPresetID() == 60 && !isTeam && Random.value < 0.33f)
            {
                CalculateAssassination(attacker, defenders, finalDamage, ref _damage, ref text, isTeam, true);
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
    private void ChrashIsFirstAttack(RogueUnitDataBase attacker, List<RogueUnitDataBase> defenders,ref float multiplier,ref float reduceDamage,ref string text,bool isTeam)
    {
        RogueUnitDataBase defender =defenders[0];
        //돌격
        if (attacker.charge)
        {
            multiplier = CalculateCharge(attacker.mobility);
            if (attacker.effectDictionary.ContainsKey(11) && UnityEngine.Random.value < 0.33f)
            {
                multiplier *= 2;
                text += "아마록 ";
            }
            //유산
            if (RelicManager.CheckRelicById(77)) multiplier += 0.3f; 
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
        CalculateFrontManiac(attacker, defenders);
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
            //{ unit => (defender.branchIdx == 5 || defender.branchIdx == 6) && unit.antiCavalry > 0, () => CalculateAntiCavalry(ref reduceDamage,ref text, attacker) }, // 대기병
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
        reduceDamage -= isTeam? unit.attackDamage*myBluntWeaponValue : unit.attackDamage*enemyBluntWeaponValue;

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
            { 9, () => addDodge -=5 },
            { 10,() => addDodge +=5 },
            { 12,() => addDodge +=5 },
        };
            //폭풍의 창
            CalculateSpearOfStormDodge(unit, isTeam, isFirstAttack);
            foreach (var key in dodgeEffects.Keys)
            {
                if (unit.effectDictionary.ContainsKey(key)) dodgeEffects[key]();
            }

            dodge = (2 + ((mulityDodge / 9) * (unit.mobility - 1))) + (unit.agility ? 10.0f : 0) + addDodge;
        }

        return MathF.Floor(Mathf.Clamp(dodge, 0, 100));
    }

    //회피 유무 계산
    private bool CalculateAccuracy(RogueUnitDataBase defender, RogueUnitDataBase attacker,bool isTeam,bool isFirstAttack)
    {
        if (attacker.perfectAccuracy)
            return false; // 필중 특성인 경우 회피 불가

        float dogeRate = CalculateDodge(defender,isTeam,isFirstAttack);

        bool result = dogeRate >= Random.Range(0, 101);
        if (result)
        {
            //방어자가 회피 성공시 암살단장의 효과 발동 isTeam==true라는건 attacker가 내 유닛이라는것 defender는 이때 enemy가 됨
            if(isTeam && enemyHeroUnits.TryGetValue(58, out List<RogueUnitDataBase> heroList))
            {
                attacker.health -= 20*heroList.Count;
            }
            else if(!isTeam && myHeroUnits.TryGetValue(58, out List<RogueUnitDataBase> myHeroList))
            {
                attacker.health -=20*myHeroUnits.Count;
            }
        } 
        return result;
    }

    //체력 회복
    private float HealHealth(RogueUnitDataBase unit, float healValue)
    {
        if (unit.effectDictionary.ContainsKey(1) || unit.effectDictionary.ContainsKey(0)) return 0;
        if (RelicManager.CheckRelicById(84)) return healValue * 1.3f;
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
    //작열 발동
    private void CalculateBurning(RogueUnitDataBase attacker, List<RogueUnitDataBase> defenders,ref string text)
    {
        if (attacker.scorching)
        {
            int burningId = 0;
            int type = 1;
            int rank = 1;
            int duration = 2;

            if (defenders[0].effectDictionary.TryGetValue(burningId, out BuffDebuffData effect))
            {
                effect.EffectGrade += 1;
                effect.Duration += 1;
            }
            else
            {
                defenders[0].effectDictionary[burningId] = new BuffDebuffData(burningId, type, rank, duration);
            }

            text += "작열 ";
        }
    }
    //작열 데미지
    private void DamageBurning(RogueUnitDataBase unit,bool isTeam)
    {
        int burningId = 0;

        if (!unit.effectDictionary.TryGetValue(burningId, out BuffDebuffData burningEffect) || burningEffect.Duration == 0)
            return;

        burningEffect.Duration = Mathf.Min(burningEffect.Duration, 2);
        burningEffect.EffectGrade = Mathf.Min(burningEffect.EffectGrade, 3);

        //작열 데미지 공식 추후 최대 체력을 추가해서 변경
        float fireDamage = fireDamageValue * unit.maxHealth;
        float damage = Mathf.Min(burningEffect.EffectGrade * fireDamage, fireDamage * 3);
        unit.health -= damage;

        burningEffect.Duration--;
        if(burningEffect.Duration == 0)
        {
            unit.effectDictionary.Remove(burningId);
        }
        CallDamageText(damage, "작열 ", isTeam, false);
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
    private float CalculateRangeAttack(List<RogueUnitDataBase> attackers, List<RogueUnitDataBase> defenders, bool isTeam, float finalDamage,bool isFirstAttack)
    {
        float allDamage = 0;
        string text = "원거리 ";
        foreach (var attacker in attackers.Skip(1))
        {
            if (!attacker.rangedAttack || attacker.health <= 0 || attacker.range - attackers.IndexOf(attacker) < 1)
                continue;

            float damage = attacker.attackDamage * finalDamage;
            damage = ChangeBackMultiple(attackers[0], defenders[0], defenders[0], damage, attacker);

            for (int k = 0; k < 2; k++)
            {
                if (k == 1 && !attacker.doubleShot) break;

                if (CalculateAccuracy(defenders[0], attacker,isTeam, isFirstAttack))
                    continue;

                if (damage > 0 &&defenders[0].heavyArmor && !defenders[0].pierce)
                {
                    damage = Mathf.Max(0, damage - heavyArmorValue);
                    
                }
                
                CalculateBurning(attacker, defenders,ref text);
                CalculateTracker(attacker, defenders[0]);
                CalculateReaper(isTeam);

                if (damage > 0 && defenders[0].thorns)
                {
                    attacker.health -= thornsDamageValue;
                }

                allDamage += damage;
            }
        }
        if (allDamage > 0)
        {
            CallDamageText(allDamage, text, !isTeam,false);
        }
        
        return allDamage;
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

                //defenders[defenderIndex + 1].attackDamage = Mathf.Round(defenders[defenderIndex+1].attackDamage * martyrdomValue);
                //유산
                if (RelicManager.CheckRelicById(74)) 
                {
                    defenders[defenderIndex + 1].stats.AddModifier(new StatModifier
                    {
                        stat = StatType.Health,
                        value = defenders[defenderIndex + 1].baseAttackDamage * martyrdomValue,
                        source = SourceType.Skill,
                        modifierId = id,
                        isPercent = false
                    });
                    //defenders[defenderIndex + 1].maxHealth += defenders[defenderIndex].attackDamage;
                    //defenders[defenderIndex + 1].health += defenders[defenderIndex].attackDamage;
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
            defender.armor = Math.Max(defender.armor - 3, 0);
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
        int heroId = 52;
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
        int heroId = 53;
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
        if (!isTeam || units[0].idx != 54 || units[0].health <1) return;
        int morale = RogueLikeData.Instance.GetMorale();
        RogueLikeData.Instance.ChangeMorale(10);
        UnitStateChange.ApplyMoralState();

    }
    //유목민 족장
    private void CalculateNomadicChief(List<RogueUnitDataBase> units, bool isTeam)
    {
        int heroId = 55;
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
        int heroId = 56;
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
            int newIndex = UnityEngine.Random.Range(1, units.Count);

            var sniper = units[index];
            units.RemoveAt(index);

            if (newIndex > index) newIndex--;
            units.Insert(newIndex, sniper);
        }
    }
    // 떠도는 자 효과 적용
    private void CalculateWanderer(bool isTeam)
    {
        int heroId = 57;
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
        int heroId = 58;
        List<RogueUnitDataBase> heroList = GetHeroUnitList(isTeam, heroId);
        bool use = false;
        if (heroList.Count <= 0) return use; 

        foreach (var hero in heroList)
        {
            for (int i = 0; i < 4; i++) // 4회 실행
            {
                CalculateDamageFirstStrike(hero, defenders, finalDamage,isTeam,ref use);
            }
        }

        return use;
    }
    //기괴한 주교
    private void CalculateBizarreBishop(List<RogueUnitDataBase> units,bool isTeam)
    {
        int heroId = 59;
        List<RogueUnitDataBase> heroList = GetHeroUnitList(isTeam, heroId);
        if (heroList.Count <= 0) return;
        int id = 7, type = 0, rank = 1, duration = -1;
        foreach (var unit in units)
        {
            unit.effectDictionary[id] = new BuffDebuffData(id, type, rank, duration);
        }
    }
    //불사 효과
    private void CalculateImmortality(ref List<RogueUnitDataBase> units,int unitIndex)
    {
        int id = 7;
        if (!units[unitIndex].effectDictionary.ContainsKey(id)) return;
        
        // 불사 효과 제거
        units[unitIndex].effectDictionary.Remove(id);
        units[unitIndex].health = 10;

        // 유닛을 리스트에서 제거 후 후열 가장 뒤로 이동
        RogueUnitDataBase immortalUnit = units[unitIndex];
        units.RemoveAt(unitIndex);
        units.Add(immortalUnit);
    }
    // 노인 기사 효과 적용 (랜덤 특성 획득)
    private void CalculateOldKnight(RogueUnitDataBase unit,bool isFrontDefenderDead)
    {
        if (unit.idx != 60 || !isFrontDefenderDead || unit.health <= 0) return;

        HashSet<string> excludedTraits = new()
    {
        "heavyArmor", "rangedAttack", "healing", "alive", "fStriked",
        "charge", "defense", "throwSpear", "guerrilla", "guard", "assassination",
        "drain", "overwhelm", "martyrdom", "wounding", "vengeance", "counter",
        "firstStrike", "challenge", "smokeScreen"
    };

        List<FieldInfo> traitFields = typeof(RogueUnitDataBase).GetFields()
            .Where(field => field.FieldType == typeof(bool) &&
                            !excludedTraits.Contains(field.Name) &&
                            field.DeclaringType == typeof(RogueUnitDataBase))
            .ToList();

        List<FieldInfo> availableTraits = traitFields
            .Where(field => !(bool)field.GetValue(unit))
            .ToList();

        if (availableTraits.Count == 0) return; 

        FieldInfo selectedTrait = availableTraits[UnityEngine.Random.Range(0, availableTraits.Count)];

        selectedTrait.SetValue(unit, true);
    }
    //미치광이 전투 시작 시
    private bool CalculateManiac(List<RogueUnitDataBase> defenders,bool isTeam)
    {
        int heroId = 61;
        List<RogueUnitDataBase> heroList= GetHeroUnitList(isTeam,heroId);
        if (heroList.Count == 0) return false;
        foreach (var unit in defenders)
        {
            int id = 0, type = 1, rank = 1, duration = 2;
            unit.effectDictionary[id] = new BuffDebuffData(id, type, rank, duration);
        }
        return true;
    }
    // 미치광이 전열 효과
    private void CalculateFrontManiac(RogueUnitDataBase attacker, List<RogueUnitDataBase> defenders)
    {
        if (attacker.idx != 61) return; 
        if (!CheckBackUnit(defenders)) return; 

        List<RogueUnitDataBase> backUnits = defenders.Skip(1).Where(unit => unit.health > 0).ToList();
        int debuffTargetCount = Mathf.Min(3, backUnits.Count);

        List<RogueUnitDataBase> selectedTargets = backUnits.OrderBy(x => UnityEngine.Random.value).Take(debuffTargetCount).ToList();

        foreach (var unit in selectedTargets)
        {
            int id = 0, type = 1, rank = 1, duration = 2;
            unit.effectDictionary[id] = new BuffDebuffData(id, type, rank, duration);
            Debug.Log($"미치광이 전열 ({attacker.unitName})이 {unit.unitName}에게 디버프 적용!");
        }
    }
    //사신
    private float CalculateReaper(bool isTeam)
    {
        int heroId = 62;
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
        int heroId = 63;
        List<RogueUnitDataBase> heroUnits = GetHeroUnitList(isTeam, heroId);
        if (heroUnits.Count <= 0) return;

    }
    //반란군 지도자 유닛 생성 부분
    private void CalculateRebelLeader(List<RogueUnitDataBase> units, bool isTeam)
    {
        int heroId = 64;
        int heroCount = GetHeroUnitList(isTeam, heroId).Count * 3;
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
        int heroId = 65;
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
        int heroId = 65;
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
        int heroId = 66;
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
        int heroId = 66;
        List<RogueUnitDataBase> heroUnits = GetHeroUnitList(isTeam, heroId);
        if (heroUnits.Count <= 0 || unit.branchIdx !=0) return 0f;
        return 0.5f;
    }
    //결속 발동
    public static void CalculataeSolidarity(List<RogueUnitDataBase> units, bool isTeam)
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
        if (units.Any(u => u.idx == 21))
        {
            var findUnits = units.Where(u => u.branchIdx == 0).ToList();
            if (findUnits.Count >= 4)
            {
                foreach (var unit in findUnits)
                {
                    unit.antiCavalry += 20;
                }
            }
        }
        
    }
    //장검병 시너지
    private void CalculateLongSwordMan(List<RogueUnitDataBase> units)
    {
        if (units.Any(u => u.idx == 22))
        {
            var findUnits = units.Where(u => u.branchIdx == 1).ToList();
            if (findUnits.Count >= 4)
            {
                foreach (var unit in findUnits)
                {
                    unit.strongCharge = true;
                }
            }
        }
           
    }
    //장궁병 시너지
    private void CalculateLongBowMan(List<RogueUnitDataBase> units)
    {
        int id = 2;
        if (units.Any(u => u.idx == 23))
        {
            var findUnits = units.Where(u => u.branchIdx == 2).ToList();
            if (findUnits.Count >= 3)
            {
                foreach (var unit in findUnits)
                {
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
        int id = 3;
        if (units.Any(u => u.idx == 24))
        {
            var findUnits = units.Where(u => u.branchIdx == 3).ToList();
            if (findUnits.Count >= 3)
            {
                foreach (var unit in findUnits)
                {
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
    //전투망치 시너지
    private void CalculateBattleHammer(List<RogueUnitDataBase> units, bool isTeam)
    {
        if (units.Any(u => u.idx == 25))
        {
            var findUnits = units.Where(u => u.branchIdx == 6).ToList();
            if (findUnits.Count >= 4)
            {
                if (isTeam)
                {
                    myBluntWeaponValue += 5;
                }
                else
                {
                    enemyBluntWeaponValue += 5;
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
        int id = 6;
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
        int id = 7;
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
            if (defender.health > 1) // 체력이 0보다 크다면 살아있는 유닛
            {
                return defender.guard ? defender : null; // guard가 있다면 해당 유닛 반환, 없다면 null 반환
            }
        }
        return null; // 모든 유닛이 죽어있다면 null 반환
    }
    //유닛 강화
    private void CalculateUpgradeUnit(List<RogueUnitDataBase> units,bool isTeam)
    {
        if (!isTeam) return;
        UpgradeManager.Instance.ProcessUpgrade();
    }

    //데미지 ui 호출
    private void CallDamageText(float damage, string text, bool team,bool isAttack ,int unitIndex = 0)
    {
        //team? 나의 공격 : 상대 공격
        CommenderEffect.CalculateZander(team, unitIndex);
        autoBattleUI.ShowDamage(MathF.Round(damage), text, !team, isAttack,unitIndex);
    }

    //사리유산
    private void CalculateSariRelic(RogueUnitDataBase attacker, bool isTeam, bool isDefenderDead)
    {
        if (!isTeam) return;
        if(attacker.idx ==63 && isDefenderDead && RelicManager.CheckRelicById(78))
        {
            RogueLikeData.Instance.AddSariStack(1);
        }
    }

    public void SetMultipleDamage(RogueUnitDataBase myFront,RogueUnitDataBase enemyFront,ref float myFinalDamage,ref float enemyFinalDamage)
    {
        myFinalDamage = RogueLikeData.Instance.GetMyMultipleDamage();
        enemyFinalDamage = RogueLikeData.Instance.GetEnemyMultipleDamage();

        myFinalDamage += UpgradeManager.GetAffinityMultiplier(myFront.branchIdx, enemyFront.branchIdx);
        enemyFinalDamage += UpgradeManager.GetAffinityMultiplier(enemyFront.branchIdx, myFront.branchIdx);
    }

    private float ChangeBackMultiple(RogueUnitDataBase frontAttacker,RogueUnitDataBase frontDeffender, RogueUnitDataBase target,float damage,RogueUnitDataBase backattacker=null)
    {
        if(backattacker == null)
        {
            float baseMultiple = UpgradeManager.GetAffinityMultiplier(frontAttacker.branchIdx, frontDeffender.branchIdx);
            float backMultiple = UpgradeManager.GetAffinityMultiplier(frontAttacker.branchIdx, target.branchIdx);
            if (baseMultiple != backMultiple)
            {
                damage = (damage / (1 + baseMultiple)) * (1 + backMultiple);
            }
        }
        else
        {
            float baseMultiple = UpgradeManager.GetAffinityMultiplier(frontAttacker.branchIdx, frontDeffender.branchIdx);
            float backMultiple = UpgradeManager.GetAffinityMultiplier(backattacker.branchIdx, frontDeffender.branchIdx);
            if (baseMultiple != backMultiple)
            {
                damage = (damage / (1 + baseMultiple)) * (1 + backMultiple);
            }
        }
        return damage;
    }
}
