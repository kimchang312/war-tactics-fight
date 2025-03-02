using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Random = UnityEngine.Random;

public class AbilityManager
{
    private float heavyArmorValue = 15.0f;                //중갑 피해 감소
    private float myBluntWeaponValue = 15.0f;             //둔기 추가 피해
    private float enemyBluntWeaponValue = 15.0f;             //둔기 추가 피해
    private float throwSpearValue = 50.0f;              //투창 추가 피해
    private float overwhelmValue = 1.0f;                //위압 기동력 고정
    private float strongChargeValue = 0.5f;             //강한 돌진 값
    private float defenseValue = 15.0f;                 //수비태세 값
    private float slaughterValue = 10.0f;               //도살 추가 피해
    private float assassinationValue = 2.0f;            //암살 배율
    private float drainHealValue = 20.0f;               //착취 회복량
    private float drainGainAttackValue = 10.0f;         //착취 공격력 증가량
    private float suppressionValue = 1.1f;                 //제압 데미지
    private float thornsDamageValue = 10.0f;                      //가시 데미지
    private float fireDamageValue = 10.0f;                       //작열 데미지
    private float bloodSuckingValue = 1.2f;                       //흡혈 회복량
    private float martyrdomValue = 1.2f;                          //순교 상승량
    private float mybindingAttackDamage = 5;                        //결속 추가 공격력
    private float enemybindingAttackDamage = 5;

    private AutoBattleUI autoBattleUI;

    //영웅 유닛 id, 유무
    Dictionary<int, RogueUnitDataBase> myHeroUnits = new();
    Dictionary<int, RogueUnitDataBase> enemyHeroUnits = new();

    //전투 전 발동(패시브)
    public void ProcessBeforeBattle(List<RogueUnitDataBase> units, List<RogueUnitDataBase> defenders, bool isTeam, float finalDamage,AutoBattleUI _autoBattleUI)
    {
        autoBattleUI = _autoBattleUI;

        CheckHeroUnit(units, isTeam);

        CalculateBloodPriest(units, isTeam);
        CalculateCorpsCommander(isTeam);
        CalculateNomadicChief(units, isTeam);
        CalculateWanderer(isTeam);

        CalculateWarden(units);
        CalculateLongSwordMan(units);
        CalculateLongBowMan(units);
        CalculateSteelCastle(units);
        CalculateBattleHammer(units,isTeam);
        CalculateEmpire(units);
        CalculateDivinityCountry(units);
        CalculateSevenUnion(units);

        CalculataeSolidarity(units, isTeam);

        //CalculateFirstStrike(units, defenders, finalDamage,isTeam);
    }
    //전투당 한번(선재 타격 등)
    //준비 페이즈 시 발동
    public void ProcessPreparationAbility(List<RogueUnitDataBase> attackers,List<RogueUnitDataBase> defenders,bool istFirstAttack,bool isTeam,float _finalDamage)
    {
        if (istFirstAttack)
        {
            RogueUnitDataBase frontAttacker = attackers[0];
            RogueUnitDataBase frontDefender = defenders[0];
            float finalDamage = _finalDamage + ((isTeam && RelicManager.TechnicalManual()) ? 1.2f : 1) - 1;
            string text = "";
            float damage = 0;

            // 특성과 기술을 한 번에 적용할 수 있도록 Dictionary 활용
            var abilityActions = new List<Action>
{
    () => { if (frontAttacker.smokeScreen) CalculateSmokeScreen(attackers, isTeam); },
    () => { if (frontAttacker.overwhelm) CalculateOverwhelm(frontAttacker, frontDefender, ref text); },
    () => { if (frontAttacker.throwSpear) CalculateThrowSpear(frontAttacker, frontDefender, finalDamage, ref damage, ref text); },
    () => { if (frontAttacker.assassination) CalculateAssassination(frontAttacker, defenders, finalDamage, ref damage, ref text, isTeam); },
    () => { if (frontAttacker.wounding) CalculateWounding(frontAttacker, frontDefender, ref text); }
};

            foreach (var action in abilityActions) action();


            if (damage > 0) CallDamageText(damage, text, !isTeam);
        }
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
        //isFirstAttak 시 발동
        if (isFirstAttack)
        {
            ChrashIsFirstAttack(frontAttaker, frontDefender, ref multiplier, ref reduceDamage,ref firstText, isTeam);
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
            //장갑 미 적용 데미지
            //float actualDamage = (damage -reduceDamage)*finalDamage;
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
            //팔랑크스
            if (!isTeam && frontAttaker.branchIdx == 2 && RelicManager.PhalanxTacticsBook())
            {
                damage *= 0.5f;
            }
            float normalDamage = damage;

            //충격
            if (isFirstAttack && frontAttaker.charge && frontAttaker.impact)
            {
                int impactIndex = CalculateMinHealthIndex(defenders);

                if (impactIndex > 0)
                {
                    float impactDamage = normalDamage * (1 - defenders[impactIndex].armor / (defenders[impactIndex].armor + 10));

                    defenders[impactIndex].health -= impactDamage;
                    
                    CallDamageText(impactDamage, "충격 ", !isTeam, 1);
                    //복수
                    if (frontDefender.vengeance)
                    {
                        frontAttaker.health -= impactDamage;

                        CallDamageText(impactDamage, "복수 ", isTeam);
                    }

                }
            }

            // 회피 판정
            if (CalculateAccuracy(frontDefender, frontAttaker.perfectAccuracy))
            {
                normalDamage = 0;

                text = "회피 ";
            }
            else
            {
                //반격
                if (isFirstAttack && frontDefender.counter)
                {
                    if (!CalculateAccuracy(frontAttaker, frontDefender.perfectAccuracy))
                    {
                        frontAttaker.health -= normalDamage;

                        CallDamageText(normalDamage, "반격 ", isTeam);
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

                    CallDamageText(thornsDamageValue, "가시 ", isTeam);
                }

                // 흡혈 (한 번만 적용)
                if (frontAttaker.lifeDrain)
                {
                    float heal = HealHealth(frontAttaker, Mathf.Min(Mathf.Round(frontAttaker.health * bloodSuckingValue), frontAttaker.maxHealth));
                    frontAttaker.health = heal;

                    CallDamageText(-heal, "흡혈 ", isTeam);
                }

                //추적자
                CalculateTracker(frontAttaker, frontDefender);
            }

            allDamage += normalDamage;
            
            if(i==0) firstText = text;
        }

        CallDamageText(allDamage, firstText, !isTeam);

        CalculateChallenge(frontAttaker,ref defenders,isTeam);
    }
    //지원 페이즈 시 발동
    public void ProcessSupportAbility(List<RogueUnitDataBase> attackers, List<RogueUnitDataBase> defenders,bool isTeam,float finalDamage) 
    {
        //원거리 공격
        float damage =CalculateRangeAttack(attackers, defenders,isTeam,finalDamage);

        defenders[0].health -= damage;

        if (!isTeam && damage > 0)
        {
            attackers[0].health -= RelicManager.ReactiveThornArmor(attackers[0]);
        }
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
        bool myUnitDied = false;   // 내 유닛 중 0번이 죽었는지 체크
        bool enemyUnitDied = false; // 상대 유닛 중 0번이 죽었는지 체크

        List<RogueUnitDataBase> tempMyDeathUnits = new();  // 임시 사망 유닛 리스트
        List<RogueUnitDataBase> tempEnemyDeathUnits = new(); // 임시 사망 유닛 리스트

        List<int> myDeathIndexes = new();
        List<int> enemyDeathIndexes = new();

        // 내 유닛 사망 처리
        for (int i = myUnits.Count - 1; i >= 0; i--)
        {
            if (myUnits[i].health <= 0)
            {
                if (i == 0 && RelicManager.HeartGemNecklace())
                {
                    myUnits[i].health = myUnits[i].maxHealth; // 유닛 체력 회복
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
            OnUnitDeath(tempEnemyDeathUnits,tempMyDeathUnits, ref enemyUnits, myUnits, false, enemyUnitDied, myUnitDied);
            OnUnitDeath(tempMyDeathUnits, tempEnemyDeathUnits, ref myUnits, enemyUnits, true, myUnitDied, enemyUnitDied);
        }

        // Relic 55 발동
        if (tempMyDeathUnits.Count > 0 && enemyUnits.Count > 0 && RelicManager.Relic55())
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
        if (myUnits.Count == 0 || enemyUnits.Count == 0)
        {
            return true;
        }
        //첫번쨰 유닛 사망 채크
        if (myFrontUnit != myUnits[0] || enemyFrontUnit != enemyUnits[0])
        {
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
    private void OnUnitDeath(List<RogueUnitDataBase> deadAttackers,List<RogueUnitDataBase> deadDefenders,ref List<RogueUnitDataBase> attackers,List<RogueUnitDataBase> defenders, bool isTeam, bool isFrontAttackerDead,bool isFrontDefendrDead)
    {
        //유닛들 하나씩 순회 하며 특정 능력 유닛 채크
        //봉풀자 채크
        if(isTeam && myHeroUnits.TryGetValue(53,out RogueUnitDataBase hero) && deadAttackers.Count>0)
        {
            hero.maxHealth += 10 * deadAttackers.Count;
            hero.health += 10 * deadAttackers.Count;
            hero.attackDamage += 5 * deadAttackers.Count;
        }
        if(!isTeam && enemyHeroUnits.TryGetValue(53,out RogueUnitDataBase unit) && deadAttackers.Count>0)
        {
            unit.maxHealth += 10 * deadAttackers.Count;
            unit.health += 10 * deadAttackers.Count;
            unit.attackDamage += 5 * deadAttackers.Count;
        }

        //스킬 사용 유닛이 안죽었을 시
        if (!isFrontAttackerDead)
        {
            //유격
            if (attackers[0].guerrilla && CheckBackUnit(attackers))
            {
                (attackers[1], attackers[0]) = (attackers[0], attackers[1]);

                CallDamageText(0, "유격", isTeam);
            }
            //착취
            if (attackers[0].drain)
            {
                //상흔 확인
                float heal = HealHealth(attackers[0], drainHealValue);
                //착취 계산
                attackers[0].health = MathF.Min(attackers[0].maxHealth, heal + attackers[0].health);
                attackers[0].attackDamage += drainGainAttackValue;
            }
            if (isFrontDefendrDead)
            {
                //무한
                if (attackers[0].endless)
                {
                    attackers[0].energy = Math.Min(attackers[0].maxEnergy, attackers[0].energy + 1);
                }
            }
        }
    }

    //선제 타격
    private void CalculateFirstStrike(List<RogueUnitDataBase> attakers, List<RogueUnitDataBase> defenders,float finalDamage,bool isTeam)
    {
        foreach (RogueUnitDataBase attacker in attakers)
        {
            if (attacker.firstStrike && !attacker.fStriked && CheckBackUnit(defenders))
            {
                int minHealthIndex = CalculateMinHealthIndex(defenders);
                float damage = attacker.attackDamage*2*finalDamage;
                defenders[minHealthIndex].health -= attacker.attackDamage * 2 * finalDamage;

                attacker.fStriked = true;

                CallDamageText(damage, "선제타격 ", !isTeam);
            }
        }
        
    }
    //위압
    private void CalculateOverwhelm(RogueUnitDataBase attacker,RogueUnitDataBase defender,ref string text)
    {
        defender.mobility = overwhelmValue;
        text += "위압 ";
    }
    //투창
    private void CalculateThrowSpear(RogueUnitDataBase attacker, RogueUnitDataBase defender,float finalDamage,ref float _damage,ref string text)
    {
        float damage = throwSpearValue * finalDamage;
        if (!CalculateAccuracy(defender, attacker.perfectAccuracy))
        {
            defender.health -= damage;

            text += "투창 ";
            _damage += damage;
            return;
        }
        text += "회피 ";
    }
    //암살
    private void CalculateAssassination(RogueUnitDataBase attacker, List<RogueUnitDataBase> defenders, float finalDamage, ref float _damage, ref string text,bool isTeam)
    {
        int minHealthIndex = CalculateMinHealthIndex(defenders);
        if (minHealthIndex == -1) return;
        float damage = attacker.attackDamage *assassinationValue* finalDamage;
        if (defenders[0].guard)
        {
            if (!CalculateAccuracy(defenders[0], attacker.perfectAccuracy))
            {
                defenders[0].health -= damage * (1 - (defenders[0].armor / (defenders[0].armor + 10)));

                _damage += damage;
                text += "암살수호 ";
            }
            text += "회피 ";
        }
        else
        {
            defenders[minHealthIndex].health -= damage;

            CallDamageText(damage, "암살 ", !isTeam, minHealthIndex);
            //복수
            if (defenders[0].vengeance)
            {
                attacker.health -= damage;

                CallDamageText(damage, "복수 ", isTeam);
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

            CallDamageText(0,"도전",!isTeam, minHealthIndex);
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
    private void ChrashIsFirstAttack(RogueUnitDataBase attacker, RogueUnitDataBase defender,ref float multiplier,ref float reduceDamage,ref string text,bool isTeam)
    {
        //돌격
        if (attacker.charge)
        {
            multiplier = CalculateCharge(attacker.mobility);

            text += "돌격 ";
            //유산
            if (RelicManager.Horn()) multiplier += 0.3f; 
            //강한 돌격
            if (attacker.strongCharge)
            {
                multiplier += strongChargeValue;

                text = "강한돌격 ";
            }
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
                if (isTeam && !RelicManager.SpearManual())
                {
                    reduceDamage += defenseValue;

                    text += "수비태세 ";
                }
            }
        }
    }

    //충돌 특성 기술 발동
    private (float,string) ApplyChrashAbility(RogueUnitDataBase attacker, RogueUnitDataBase defender, bool isTeam, float _reduceDamage,string _text)
    {
        float reduceDamage =_reduceDamage;
        string text = _text;
        Dictionary<Func<RogueUnitDataBase, bool>, Action> traitEffects = new()
        {
            { unit => unit.bluntWeapon && defender.heavyArmor, () => CalculateBluntWeapon(isTeam,ref reduceDamage,ref text) }, // 둔기
            { unit => unit.slaughter && defender.lightArmor, () => CalculateSlaughter(ref reduceDamage,ref text) }, // 도살
            { unit => (defender.branchIdx == 5 || defender.branchIdx == 6) && unit.antiCavalry > 0, () => CalculateAntiCavalry(ref reduceDamage,ref text, attacker) }, // 대기병
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
        return ((0.95f / 100f) * (mobility * mobility))+1.75f;
    }
    //둔기
    private void CalculateBluntWeapon(bool isTeam,ref float reduceDamage,ref string text)
    {
        reduceDamage -= isTeam?myBluntWeaponValue:enemyBluntWeaponValue;

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
    public float CalculateDodge(RogueUnitDataBase unit)
    {
        float dodge;
        float addDodge = 0;
        float mulityDodge = 13;
        Dictionary<int, Action> dodgeEffects = new()
        {
            { 2, () => addDodge += 15 },
            { 4, () => addDodge += 5 },
            { 5, () => mulityDodge *= 2 },
            { 6, () => addDodge += 5 }
        };

        foreach (var key in dodgeEffects.Keys)
        {
            if (unit.effectDictionary.ContainsKey(key)) dodgeEffects[key]();
        }

        dodge = (2 + ((mulityDodge / 9) * (unit.mobility - 1))) + (unit.agility ? 10.0f : 0) + addDodge;

        return MathF.Floor(Mathf.Clamp(dodge, 0, 100));
    }

    //회피 유무 계산
    private bool CalculateAccuracy(RogueUnitDataBase unit, bool isPerfectAccuracy)
    {
        if (isPerfectAccuracy)
            return false; // 필중 특성인 경우 회피 불가

        float dogeRate = CalculateDodge(unit);

        bool result = dogeRate >= Random.Range(0, 101);

        return result;
    }

    //체력 회복
    private float HealHealth(RogueUnitDataBase unit, float healValue)
    {
        if (unit.effectDictionary.ContainsKey(1)) return 0;
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
                if(units[i].health>0)
                {
                    units[i].effectDictionary[id] = new BuffDebuffData(id, type, rank, duration);
                }
            }

            CallDamageText(0, "연막 ", isTeam, 1);
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

        // 효과가 존재하지 않거나 지속시간이 0 이면 종료
        if (!unit.effectDictionary.TryGetValue(burningId, out BuffDebuffData burningEffect) || burningEffect.Duration == 0)
            return;

        // 지속 시간과 등급 제한 적용
        burningEffect.Duration = Mathf.Min(burningEffect.Duration, 2);
        burningEffect.EffectGrade = Mathf.Min(burningEffect.EffectGrade, 3);

        // 피해량 계산 (최대 fireDamageValue * 3 제한)
        float damage = Mathf.Min(burningEffect.EffectGrade * fireDamageValue, fireDamageValue * 3);

        // 체력 감소 적용
        unit.health -= damage;

        // 지속시간 감소
        burningEffect.Duration--;

        CallDamageText(damage, "작열 ", isTeam);
    }
    // 치유(Healing) 기능 - 후열 유닛이 전열 유닛을 치유
    private void ProcessHealing(List<RogueUnitDataBase> units,bool isTeam)
    {
        // 치유받을 전열 유닛 (현재 attackerIndex 위치의 유닛)
        RogueUnitDataBase frontUnit = units[0];
        float heal = 0;

        // 후열 유닛 탐색 (attackerIndex 이후의 유닛)
        for (int i = 1; i < units.Count; i++)
        {
            RogueUnitDataBase healer = units[i];

            // 치유 특성이 없거나 사거리가 2 미만이면 스킵
            if (!healer.healing || healer.range < 2)
                continue;

            // 후열에서의 순서 계산 (현재 위치 차이)
            int rearPosition = i;

            // 치유 조건: (사거리) - (후열에서의 순서) >= 1 확인
            if (healer.range - rearPosition >= 1 && frontUnit.health > 0)
            {
                // 치유량 = 치유하는 유닛의 공격력
                float healAmount = healer.attackDamage;
                //상흔 확인
                healAmount = HealHealth(frontUnit, healAmount);

                heal += healAmount;
                // 최대 체력을 넘지 않도록 제한
                frontUnit.health = Mathf.Min(frontUnit.maxHealth, frontUnit.health + healAmount);
            }
        }

        if (heal > 0) CallDamageText(-heal, "치유 ", isTeam);
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
        // 최소 체력 유닛 번호 (-1은 유효한 유닛이 없는 경우를 대비)
        int minHealthNumber = -1;
        // 최소 체력 기준 (초기값은 float.MaxValue로 설정)
        float lastHealth = float.MaxValue;

        for (int i = 1; i < units.Count; i++)
        {
            // 체력이 0보다 크고, 현재 최소 체력보다 작은 경우 갱신
            if (units[i].health > 0 && units[i].health < lastHealth)
            {
                lastHealth = units[i].health;
                minHealthNumber = i;
            }
        }

        // 최소 체력 유닛 번호 반환 (유효한 유닛이 없으면 -1 반환)
        return minHealthNumber;
    }

    // 원거리 공격 최적화 코드
    private float CalculateRangeAttack(List<RogueUnitDataBase> attackers, List<RogueUnitDataBase> defenders, bool isTeam, float finalDamage)
    {
        float allDamage = 0;
        string text = "원거리 ";
        foreach (var attacker in attackers.Skip(1)) // 첫 번째 유닛(전열) 제외
        {
            if (!attacker.rangedAttack || attacker.health <= 0 || attacker.range - attackers.IndexOf(attacker) < 1)
                continue; // 유효한 원거리 공격자가 아니면 스킵

            float damage = attacker.attackDamage * finalDamage;

            // 팔랑크스 전술 적용 (아군이 아닐 때)
            if (!isTeam && RelicManager.PhalanxTacticsBook())
            {
                damage *= 0.5f;
            }

            for (int k = 0; k < 2; k++)
            {
                if (k == 1 && !attacker.doubleShot) break; // 연발 공격이 없으면 1회 공격만

                // 회피 여부 확인
                if (CalculateAccuracy(defenders[0], attacker.perfectAccuracy))
                    continue;

                // 중갑 적용 (heavyArmorValue 감소, 최소 0 보장)
                if (damage > 0 && defenders[0].heavyArmor && attacker.pierce)
                {
                    damage = Mathf.Max(0, damage - heavyArmorValue);
                }

                // 작열 적용
                CalculateBurning(attacker, defenders,ref text);

                //추적자
                CalculateTracker(attacker, defenders[0]);

                // 가시 피해 (thorns)
                if (damage > 0 && defenders[0].thorns)
                {
                    attacker.health -= thornsDamageValue;
                }

                allDamage += damage;
            }
        }
        if (allDamage > 0)
        {
            CallDamageText(allDamage, text, !isTeam);
        }
        
        return allDamage;
    }

    //순교 0,1번이 동시에 사망해도 1번에 버프
    private void CalculateMartyrdom(List<RogueUnitDataBase> defenders,int defenderIndex)
    {
        if (defenders[defenderIndex].martyrdom)
        {
            if(defenderIndex+1 < defenders.Count && defenders[defenderIndex + 1].health>0)
            {
                defenders[defenderIndex + 1].attackDamage = Mathf.Round(defenders[defenderIndex+1].attackDamage * martyrdomValue);
                //유산
                if (RelicManager.SacredDocument()) 
                {
                    defenders[defenderIndex + 1].maxHealth += defenders[defenderIndex].attackDamage;
                    defenders[defenderIndex + 1].health += defenders[defenderIndex].attackDamage;
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

    //영웅 유닛 채크
    private void CheckHeroUnit(List<RogueUnitDataBase> units, bool isTeam)
    {
        var heroUnits = units.Where(unit => unit.branchIdx == 8).ToList();
        if (isTeam)
            heroUnits.ForEach(unit => myHeroUnits.TryAdd(unit.idx, unit));
        else
            heroUnits.ForEach(unit => enemyHeroUnits.TryAdd(unit.idx, unit));

    }
    //핏빛 사제
    private void CalculateBloodPriest(List<RogueUnitDataBase> units,bool isTeam)
    {
        //if(isTeam && myHeroUnits.ContainsKey())
    }
    //군단장
    private void CalculateCorpsCommander(bool isTeam)
    {
        if (isTeam && myHeroUnits.ContainsKey(52))
        {
            mybindingAttackDamage += 5;
        }
        else if (!isTeam && enemyHeroUnits.ContainsKey(52))
        {
            enemybindingAttackDamage += 5;
        }
        return;
    }
    //유목민 족장
    private void CalculateNomadicChief(List<RogueUnitDataBase> units, bool isTeam)
    {
        if(isTeam && myHeroUnits.ContainsKey(55))
        {
            foreach (var unit in units)
            {
                if(unit.tagIdx ==1) unit.bindingForce = true;
            }
        }
        else if(!isTeam && enemyHeroUnits.ContainsKey(55))
        {
            foreach (var unit in units)
            {
                if(unit.tagIdx ==1) unit.bindingForce = true;
            }
        }
    }
    //떠도는자
    private void CalculateWanderer(bool isTeam)
    {
        if(isTeam && myHeroUnits.ContainsKey(57) && myHeroUnits.Count>1)
        {
            foreach(var hero in myHeroUnits)
            {
                if (hero.Value.idx != 57) hero.Value.health += 40;
            }
        }
        else if(!isTeam && enemyHeroUnits.ContainsKey(57) && enemyHeroUnits.Count >1)
        {
            foreach (var hero in enemyHeroUnits)
            {
                if (hero.Value.idx != 57) hero.Value.health += 40;
            }
        }
    }
    //결속 발동
    private void CalculataeSolidarity(List<RogueUnitDataBase> units, bool isTeam)
    {
        foreach (var unit in units)
        {
            if (unit.bindingForce) // 결속이 활성화된 유닛만 체크
            {
                foreach (var checkUnit in units)
                {
                    // checkUnit.unitTag와 unit.unitTag에 공통된 태그가 하나라도 있으면 공격력 +5
                    if (unit.tag == checkUnit.tag)
                    {
                        unit.attackDamage += isTeam ? mybindingAttackDamage : enemybindingAttackDamage;
                    }
                }
            }
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
        if (units.Any(u => u.idx == 23))
        {
            var findUnits = units.Where(u => u.branchIdx == 2).ToList();
            if (findUnits.Count >= 3)
            {
                foreach (var unit in findUnits)
                {
                    unit.attackDamage += 10;
                }
            }
        }
            
    }
    //철옹성
    private void CalculateSteelCastle(List<RogueUnitDataBase> units)
    {
        if (units.Any(u => u.idx == 24))
        {
            var findUnits = units.Where(u => u.branchIdx == 3).ToList();
            if (findUnits.Count >= 3)
            {
                foreach (var unit in findUnits)
                {
                    unit.maxHealth += 40;
                    unit.health += 40;
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
        var findUnits = units.Where(u => u.factionIdx == 2).ToList();
        if (findUnits.Count >= 5)
        {
            foreach (var unit in units)
            {
                unit.armor += 1;
            }
        }
    }
    //칠왕연합 시너지
    private void CalculateSevenUnion(List<RogueUnitDataBase> units)
    {
        var findUnits = units.Where(u => u.factionIdx == 3).ToList();
        if (findUnits.Count >= 5)
        {
            foreach (var unit in units)
            {
                unit.attackDamage *= 1.03f;
            }
        }
    }
    //데미지 ui 호출
    private void CallDamageText(float damage, string text, bool team, int unitIndex = 0)
    {
        autoBattleUI.ShowDamage(MathF.Floor(damage), text, !team, unitIndex);
    }
}
