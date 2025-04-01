using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Rendering;
using UnityEngine;
using static RogueLikeData;
using static UnityEngine.UIElements.UxmlAttributeDescription;
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
    private float moraleMultiplier = 0f;
    private int plunderGold = 20;                    //약탈 골드

    private AutoBattleUI autoBattleUI;

    //영웅 유닛 id, 유무
    Dictionary<int, List<RogueUnitDataBase>> myHeroUnits = new();
    Dictionary<int, List<RogueUnitDataBase>> enemyHeroUnits = new();

    //입장 시 채크
    public void ProcessEnter()
    {
        StageType type = RogueLikeData.Instance.GetCurrentStageType();
        int morale = RogueLikeData.Instance.GetMorale();
        switch (type) 
        {
            case StageType.Battle:
                ReduceUnitEngery();
                break;
            case StageType.Elite:
                ReduceUnitEngery();
                break;
            case StageType.Boss:
                ReduceUnitEngery();
                if (RelicManager.CheckRelicById(39)) morale += 15;
                break;
            case StageType.Rest:
                break;
            case StageType.Unknown:
                break;
            case StageType.Chest:
                break;
            case StageType.Shop:
                if (RelicManager.CheckRelicById(40))
                {
                    var myUnits = RogueLikeData.Instance.GetMyUnits();
                    foreach (var unit in myUnits)
                    {
                        unit.energy = Math.Min(unit.maxEnergy, unit.energy + 1);
                    }
                }
                break;
            default:
                break;
        }
    }
    //유닛 기력 감소
    private void ReduceUnitEngery()
    {
        var myUnits= RogueLikeData.Instance.GetMyUnits();
        
        foreach (var unit in myUnits)
        {
            unit.energy = Math.Max(0, unit.energy - 1);
        }
    }
    //전투 전 발동(패시브)
    public void ProcessBeforeBattle(List<RogueUnitDataBase> units, List<RogueUnitDataBase> defenders, bool isTeam, float finalDamage,AutoBattleUI _autoBattleUI)
    {
        autoBattleUI = _autoBattleUI;

        //기타 유산
        if (RelicManager.CheckRelicById(68)) plunderGold += 20;

        CheckHeroUnit(units, isTeam);

        //사기 발동
        CalculateFirstMorale(units,isTeam);

        //유닛 효과
        CalculateBloodPriest(units);
        CalculateCorpsCommander(isTeam);
        CalculateNomadicChief(units, isTeam);
        CalculateWanderer(isTeam);
        CalculateRebelLeader(units, isTeam);
        CalculateIndomitableShield(units, isTeam);
        CalculateSpearOfStorm(units, isTeam);

        //시너지
        CalculateWarden(units);
        CalculateLongSwordMan(units);
        CalculateLongBowMan(units);
        CalculateSteelCastle(units);
        CalculateBattleHammer(units,isTeam);
        CalculateEmpire(units);
        CalculateDivinityCountry(units);
        CalculateSevenUnion(units);

        CalculataeSolidarity(units, isTeam);

        //유닛 강화
        CalculateUpgradeUnit(units, isTeam);
    }
    //전투당 한번(선재 타격 등)
    public bool ProcessStartBattle(List<RogueUnitDataBase> units, List<RogueUnitDataBase> defenders,float finalDamage,bool isTeam)
    {
        return (CalculateFirstStrike(units, defenders, finalDamage, isTeam) || CalculateManiac(defenders, isTeam));
    }

    //준비 페이즈 시 발동
    public void ProcessPreparationAbility(List<RogueUnitDataBase> attackers,List<RogueUnitDataBase> defenders,bool isFirstAttack,bool isTeam,float _finalDamage)
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

            float normalDamage = MathF.Round(damage);

            //충격
            if (isFirstAttack && frontAttaker.charge && frontAttaker.impact)
            {                
                RogueUnitDataBase target = CalculateBackAttack(defenders);
                if (target==null)
                {
                    foreach (var unit in defenders)
                    {
                        if (unit.health > 0)
                        {
                            target = unit;
                            break;
                        }
                    }
                    float impactDamage = MathF.Round(normalDamage * (1 - target.armor / (target.armor + 10)));
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

                // 흡혈 (한 번만 적용)
                if (frontAttaker.lifeDrain)
                {
                    float heal = HealHealth(frontAttaker, Mathf.Min(Mathf.Round(frontAttaker.health+(normalDamage * bloodSuckingValue)), frontAttaker.maxHealth));
                    frontAttaker.health = heal;

                    CallDamageText(-heal, "흡혈 ", isTeam, false);
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
                if (i == 0 && RelicManager.CheckRelicById(27))
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

        // Relic 54 저주인형 발동
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
    private void OnUnitDeath(List<RogueUnitDataBase> deadAttackers,List<RogueUnitDataBase> deadDefenders,ref List<RogueUnitDataBase> attackers,List<RogueUnitDataBase> defenders, bool isTeam, bool isFrontAttackerDead,bool isFrontDefendrDead)
    {
        if(attackers.Count == 0) return;
        RogueUnitDataBase frontAttacker = attackers[0];
        //유닛들 하나씩 순회 하며 특정 능력 유닛 채크
        //봉인 풀린 자 채크
        CalculateTheUnsealedOne(deadDefenders.Count, isTeam);
        //불굴의 방패
        CalculateIndomitableShieldDead(attackers, deadAttackers, isTeam);

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
            //노인기사
            CalculateOldKnight(frontAttacker, isFrontDefendrDead);
            //
            if (isFrontDefendrDead)
            {
                //약탈
                CalculatePlunder(frontAttacker, isTeam);
                //무한
                CalculateEndLess(frontAttacker, isTeam);
                //돌격대장
                CalculateAssaultLeader(attackers,isTeam);
            }
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
        // 암살단장(Assassination Leader, idx == 58)이 있을 경우 선제 타격 4회 실행
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
            if (minHealthIndex == -1) return; // 유효한 대상이 없으면 루프 종료

            float damage = MathF.Round(attacker.attackDamage * 2 * finalDamage);
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
        RogueLikeData.Instance.SetCurrentGold(RogueLikeData.Instance.GetCurrentGold()+plunderGold);
    }
    //무한
    private void CalculateEndLess(RogueUnitDataBase unit,bool isTeam)
    {
        if (!isTeam || !unit.endless) return;
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
    private void CalculateAssassination(RogueUnitDataBase attacker, List<RogueUnitDataBase> defenders, float finalDamage, ref float _damage, ref string text,bool isTeam,bool isFirstAttack)
    {
        int minHealthIndex = CalculateMinHealthIndex(defenders);
        if (minHealthIndex == -1) return;
        float damage = MathF.Round(attacker.attackDamage * assassinationValue * finalDamage);

        RogueUnitDataBase target= CalculateBackAttack(defenders);

        //수호 있을 때
        if (target != null) 
        {
            target.health -= MathF.Round(damage * (1 - (target.armor / (target.armor + 10))));
            _damage += damage;

            Debug.Log("수호");
            CallDamageText(damage, "암살 수호 ", !isTeam, true);
        }
        else //수호 없을때
        {
            defenders[minHealthIndex].health -= damage;

            CallDamageText(damage, "암살 ", !isTeam, true,minHealthIndex);
            //복수
            if (defenders[0].vengeance)
            {
                attacker.health -= damage;

                CallDamageText(damage, "복수 ", isTeam, true);
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
            text += "돌격 ";
            //유산
            if (RelicManager.CheckRelicById(77)) multiplier += 0.3f; 
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
        return ((0.95f / 100f) * (mobility * mobility))+1.05f;
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
    public float CalculateDodge(RogueUnitDataBase unit,bool isTeam,bool isFirstAttack)
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
        //폭풍의 창
        CalculateSpearOfStormDodge(unit, isTeam,isFirstAttack);
        foreach (var key in dodgeEffects.Keys)
        {
            if (unit.effectDictionary.ContainsKey(key)) dodgeEffects[key]();
        }

        dodge = (2 + ((mulityDodge / 9) * (unit.mobility - 1))) + (unit.agility ? 10.0f : 0) + addDodge;

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
        if (unit.effectDictionary.ContainsKey(1)) return 0;
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
                if(units[i].health>0)
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

        CallDamageText(damage, "작열 ", isTeam, false);
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
    private float CalculateRangeAttack(List<RogueUnitDataBase> attackers, List<RogueUnitDataBase> defenders, bool isTeam, float finalDamage,bool isFirstAttack)
    {
        float allDamage = 0;
        string text = "원거리 ";
        foreach (var attacker in attackers.Skip(1)) // 첫 번째 유닛(전열) 제외
        {
            if (!attacker.rangedAttack || attacker.health <= 0 || attacker.range - attackers.IndexOf(attacker) < 1)
                continue; // 유효한 원거리 공격자가 아니면 스킵

            float damage = attacker.attackDamage * finalDamage;

            for (int k = 0; k < 2; k++)
            {
                if (k == 1 && !attacker.doubleShot) break; // 연발 공격이 없으면 1회 공격만

                // 회피 여부 확인
                if (CalculateAccuracy(defenders[0], attacker,isTeam, isFirstAttack))
                    continue;

                // 중갑 적용 (heavyArmorValue 감소, 최소 0 보장)
                if (damage > 0 &&defenders[0].heavyArmor && !defenders[0].pierce)
                {
                    damage = Mathf.Max(0, damage - heavyArmorValue);
                    
                }
                
                // 작열 적용
                CalculateBurning(attacker, defenders,ref text);

                //추적자
                CalculateTracker(attacker, defenders[0]);

                //사신
                CalculateReaper(isTeam);

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
            CallDamageText(allDamage, text, !isTeam,false);
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
                if (RelicManager.CheckRelicById(74)) 
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
        for (int i = 1; i < units.Count; i++)  // 첫 유닛은 앞에 대상이 없으므로 시작 인덱스 1부터
        {
            if (units[i].idx == 50)  // 핏빛 사제 발견 
            {
                // 사거리에서 1을 빼고, 0 이하가 되지 않도록 보정
                int range = Mathf.Max(0, (int)units[i].range - 1);
                // 앞쪽 유닛의 개수를 고려하여 효과 적용 범위 제한
                int effectiveRange = Mathf.Min(range, i);

                for (int j = 1; j <= effectiveRange; j++)
                {
                    int targetIndex = i - j;
                    units[targetIndex].lifeDrain = true;  // 흡혈 특성 부여
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
            mybindingAttackDamage += 5 * heroList.Count;  // 보유한 영웅 수만큼 효과 적용
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
        RogueLikeData.Instance.SetMorale(morale + 10);
        CalculateFirstMorale(units, isTeam, true);

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
            // 현재 위치를 제외한 1 ~ units.Count 범위에서 랜덤한 위치 선택
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
                    hero.health += 40 * wandererList.Count;
                }
            }
        }
    }
    // 암살단장(Assassination Leader)의 선제타격 실행
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
        if (unit.idx != 60 || !isFrontDefenderDead || unit.health <= 0) return; // 생존 중인 노인 기사만 적용

        // 제외할 특성 목록 (배울 수 없는 것들)
        HashSet<string> excludedTraits = new()
    {
        "heavyArmor", "rangedAttack", "healing", "alive", "fStriked",
        "charge", "defense", "throwSpear", "guerrilla", "guard", "assassination",
        "drain", "overwhelm", "martyrdom", "wounding", "vengeance", "counter",
        "firstStrike", "challenge", "smokeScreen"
    };

        // 특성 필드만 필터링 (불필요한 bool 필드 제외)
        List<FieldInfo> traitFields = typeof(RogueUnitDataBase).GetFields()
            .Where(field => field.FieldType == typeof(bool) &&
                            !excludedTraits.Contains(field.Name) &&
                            field.DeclaringType == typeof(RogueUnitDataBase)) // 부모 클래스로부터 상속된 bool 필드 제외
            .ToList();

        // 현재 unit이 보유하지 않은 특성만 선택
        List<FieldInfo> availableTraits = traitFields
            .Where(field => !(bool)field.GetValue(unit)) // 현재 false인 특성만 추가
            .ToList();

        if (availableTraits.Count == 0) return; // 배울 수 있는 특성이 없으면 종료

        // 무작위 특성 선택
        FieldInfo selectedTrait = availableTraits[UnityEngine.Random.Range(0, availableTraits.Count)];

        // 특성 적용
        selectedTrait.SetValue(unit, true);

        // 적용된 특성은 다시 나오지 않도록 리스트에서 제거
        availableTraits.Remove(selectedTrait);

        Debug.Log($"노인 기사({unit.unitName})가 {selectedTrait.Name} 특성을 획득!");
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
    // 미치광이 전열 효과 (후열 유닛 3명에게 디버프 적용)
    private void CalculateFrontManiac(RogueUnitDataBase attacker, List<RogueUnitDataBase> defenders)
    {
        if (attacker.idx != 61) return; // 미치광이 전열이 아니면 종료
        if (!CheckBackUnit(defenders)) return; // 상대 후열 유닛이 없으면 종료

        List<RogueUnitDataBase> backUnits = defenders.Skip(1).Where(unit => unit.health > 0).ToList();
        int debuffTargetCount = Mathf.Min(3, backUnits.Count);

        // 후열 유닛 중에서 랜덤하게 3명 선택
        List<RogueUnitDataBase> selectedTargets = backUnits.OrderBy(x => UnityEngine.Random.value).Take(debuffTargetCount).ToList();

        // 선택된 유닛들에게 디버프 적용
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
    private void CalculateRebelLeader(List<RogueUnitDataBase> units,bool isTeam)
    {
        int heroId = 64;
        int heroCount = GetHeroUnitList(isTeam,heroId).Count*3;
        if (heroCount <= 0) return;
        RogueUnitDataBase spearmanData = RogueUnitDataBase.GetSpearmanData();
        if (spearmanData == null) Debug.Log("창병없음");
        for (int i = 0; i < heroCount; i++)
        {
            RogueUnitDataBase newUnit = new RogueUnitDataBase(
                spearmanData.idx,
                spearmanData.unitName,
                spearmanData.unitBranch,
                spearmanData.branchIdx,
                spearmanData.unitId,
                spearmanData.unitExplain,
                spearmanData.unitImg,
                spearmanData.unitFaction,
                spearmanData.factionIdx,
                spearmanData.tag,
                spearmanData.tagIdx,
                spearmanData.unitPrice,
                spearmanData.rarity,
                spearmanData.health,
                spearmanData.armor,
                spearmanData.attackDamage,
                spearmanData.mobility,
                spearmanData.range,
                spearmanData.antiCavalry,
                spearmanData.energy,
                spearmanData.baseHealth,
                spearmanData.baseArmor,
                spearmanData.baseAttackDamage,
                spearmanData.baseMobility,
                spearmanData.baseRange,
                spearmanData.baseAntiCavalry,
                spearmanData.baseEnergy,
                spearmanData.lightArmor,
                spearmanData.heavyArmor,
                spearmanData.rangedAttack,
                spearmanData.bluntWeapon,
                spearmanData.pierce,
                spearmanData.agility,
                spearmanData.strongCharge,
                spearmanData.perfectAccuracy,
                spearmanData.slaughter,
                spearmanData.bindingForce,
                spearmanData.bravery,
                spearmanData.suppression,
                spearmanData.plunder,
                spearmanData.doubleShot,
                spearmanData.scorching,
                spearmanData.thorns,
                spearmanData.endless,
                spearmanData.impact,
                spearmanData.healing,
                spearmanData.lifeDrain,
                spearmanData.charge,
                spearmanData.defense,
                spearmanData.throwSpear,
                spearmanData.guerrilla,
                spearmanData.guard,
                spearmanData.assassination,
                spearmanData.drain,
                spearmanData.overwhelm,
                spearmanData.martyrdom,
                spearmanData.wounding,
                spearmanData.vengeance,
                spearmanData.counter,
                spearmanData.firstStrike,
                spearmanData.challenge,
                spearmanData.smokeScreen,
                spearmanData.maxHealth,
                spearmanData.maxEnergy,
                true,
                false,
                AutoBattleManager.GenerateUniqueUnitId(spearmanData.branchIdx, isTeam, spearmanData.idx)
            );
            // 리스트 앞에 삽입
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
            if (unit.heavyArmor) unit.armor += 5 * heroCount;
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

        // 체력 증가 수치 계산
        int healthIncrease = 10 * deadHeavyArmorCount* heroCount;

        // 살아있는 아군 유닛들의 체력 증가
        foreach (var unit in units)
        {
            if (unit.health > 0) // 살아있는 유닛만 처리
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
    //사기 계산
    private void CalculateFirstMorale(List<RogueUnitDataBase> units, bool isTeam, bool isBattle = false)
    {
        if (!isTeam) return;
        int morale = RogueLikeData.Instance.GetMorale();
        int reduceMorale = RelicManager.CheckRelicById(28) ? -10 : 0;
        float newMultiplier = 0f;

        // 사기 수치에 따른 능력치 조정
        if (morale >= (90 + reduceMorale)) newMultiplier = 0.2f;
        else if (morale >= (70 + reduceMorale)) newMultiplier = 0.1f;
        else if (morale <= (30 + reduceMorale)) newMultiplier = -0.1f;

        if (newMultiplier == moraleMultiplier) return; // 변화 없으면 그대로 리턴

        // 새로운 사기 배율 적용
        float multiplierDifference = newMultiplier - moraleMultiplier;
        moraleMultiplier = newMultiplier; // 현재 사기 배율 업데이트

        // 유닛 능력치 조정
        foreach (var unit in units)
        {
            if (morale <= (30 + reduceMorale) && unit.bravery) continue; // '용맹' 특성을 가진 유닛은 디버프 적용 안 함

            float healthChange = Mathf.Round(unit.baseHealth * multiplierDifference);
            unit.maxHealth += healthChange;
            unit.health += healthChange;

            unit.attackDamage += Mathf.Round(unit.baseAttackDamage * multiplierDifference);
        }

        // 사기 10 이하이면 탈주 로직 실행
        if (morale <= (10 + reduceMorale) && !isBattle)
        {
            RemoveLowMoraleUnits(units);
        }

        autoBattleUI.UpdateMorale();
    }

    //사기 탈주
    private void RemoveLowMoraleUnits(List<RogueUnitDataBase> units)
    {
        List<RogueUnitDataBase> removableUnits = new List<RogueUnitDataBase>();

        // 희귀도 1~3 유닛 중 '용맹'이 없는 유닛만 탈주 후보로 추가
        foreach (var unit in units)
        {
            if (unit.rarity >= 1 && unit.rarity <= 3 && !unit.bravery)
            {
                removableUnits.Add(unit);
            }
        }

        // 탈주할 유닛이 있으면 무작위로 한 유닛 선택하여 제거
        if (removableUnits.Count > 0)
        {
            RogueUnitDataBase leavingUnit = removableUnits[UnityEngine.Random.Range(0, removableUnits.Count)];
            units.Remove(leavingUnit);
            Debug.Log($"{leavingUnit.unitName}이(가) 사기가 낮아 부대를 떠남!");
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
    //전투 종료 시 사기 계산
    public void EndBattleMorale(List<RogueUnitDataBase> deadUnits,List<RogueUnitDataBase> deadEnemyUnits)
    {
        int morale = RogueLikeData.Instance.GetMorale();
        int addMorale = 0;
        StageType stageType = RogueLikeData.Instance.GetCurrentStageType();
        foreach (var unit in deadUnits)
        {
            switch (unit.rarity)
            {
                case 1:
                    addMorale -= 1;
                    break;
                case 2:
                    addMorale -= 2;
                    break;
                case 3:
                    addMorale -= 2;
                    break;
                case 4:
                    addMorale -= 5;
                    break;
            }
        }
        //유산 33
        if(RelicManager.CheckRelicById(33)) addMorale = (int)(addMorale * 1.2);
        if (deadEnemyUnits.Count == 0) addMorale += 10; 
        if(stageType == StageType.Battle)
        {
            addMorale += 15;
        }
        else if (stageType ==StageType.Elite)
        {
            addMorale += 20;
        }
        else if(stageType == StageType.Boss)
        {
            addMorale += 35;
        }
        //유산 56
        if (RelicManager.CheckRelicById(56)) addMorale += deadEnemyUnits.Count;
        morale += addMorale;
        RogueLikeData.Instance.SetMorale(morale);
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
        foreach(var unit in units)
        {
            UpgradeManager.Instance.UpgradeRogueLikeUnit(unit);
        }
    }

    //데미지 ui 호출
    private void CallDamageText(float damage, string text, bool team,bool isAttack ,int unitIndex = 0)
    {
        autoBattleUI.ShowDamage(MathF.Round(damage), text, !team, isAttack,unitIndex);
    }
}
