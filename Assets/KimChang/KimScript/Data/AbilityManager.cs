using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class AbilityManager
{
    private float heavyArmorValue = 15.0f;                //중갑 피해 감소
    private float bluntWeaponValue = 15.0f;             //둔기 추가 피해
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

    //유닛 데이터 초기화
    public void ProcessInitialize()
    {

    }

    //준비 페이즈 시 발동
    public void ProcessPreparationAbility(List<RogueUnitDataBase> attackers,List<RogueUnitDataBase> defenders,bool istFirstAttack,bool isTeam,float _finalDamage)
    {
        RogueUnitDataBase frontAttacker = attackers[0];
        RogueUnitDataBase frontDefender = defenders[0];
        float finalDamage = _finalDamage +((isTeam && RelicManager.TechnicalManual()) ? 1.2f : 1)-1;
        CalculateFirstStrike(frontAttacker, defenders,finalDamage);
        if (istFirstAttack)
        {
            // 특성과 기술을 한 번에 적용할 수 있도록 Dictionary 활용
            Dictionary<Func<RogueUnitDataBase, bool>, Action> abilities = new()
        {
            { unit => unit.smokeScreen, () => CalculateSmokeScreen(defenders) },
            { unit => unit.overwhelm, () => CalculateOverwhelm(frontAttacker, frontDefender) },
            { unit => unit.throwSpear, () => CalculateThrowSpear(frontAttacker, frontDefender, finalDamage) },
            { unit => unit.assassination, () => CalculateAssassination(frontAttacker, defenders, finalDamage) },
            { unit => unit.wounding, () => CalculateWounding(frontAttacker, frontDefender) }
        };

            foreach (var ability in abilities)
            {
                if (ability.Key(frontAttacker))
                {
                    ability.Value();
                }
            }

        }
    }

    //충돌 페이즈 시 발동
    public float ProcessChrashAbility(List<RogueUnitDataBase> attackers, List<RogueUnitDataBase> defenders,bool isFirstAttack,float finalDamage,bool isTeam)
    {
        float multiplier=1;
        float reduceDamage = 0;
        RogueUnitDataBase frontAttaker = attackers[0];
        RogueUnitDataBase frontDefender = defenders[0];
        //isFirstAttak 시 발동
        if (isFirstAttack)
        {
            ChrashIsFirstAttack(frontAttaker, frontDefender, ref multiplier, ref reduceDamage);
        }
        reduceDamage =ApplyChrashAbility(frontAttaker,frontDefender,reduceDamage);

        float damage = frontAttaker.attackDamage * multiplier;
        //장갑 미 적용 데미지
        float actualDamage = (damage -reduceDamage)*finalDamage;
        //관통
        if (!frontAttaker.pierce)
        {
            damage *= (1 - frontDefender.armor / (frontDefender.armor + 10));
        }
        damage = (damage -reduceDamage) * finalDamage;
        //팔랑크스
        if(!isTeam && frontAttaker.branchIdx==2 && RelicManager.PhalanxTacticsBook())
        {
            damage *= 0.5f;
        }

        float totalDamage = 0;
        for (int i = 0; i < 2; i++)
        {
            if (i == 1)
            {
                //연발
                if (!frontAttaker.doubleShot) break; // 한 번만 공격
            }

            float normalDamage = damage;

            //충격
            if (isFirstAttack && frontAttaker.charge && frontAttaker.impact)
            {
                int impactIndex = -1;

                for (int k = 1; k < defenders.Count; k++)
                {
                    if (defenders[k].health > 0)
                    {
                        impactIndex = k;
                        break;
                    }
                }

                if (impactIndex > 0)
                {
                    float impactDamage = normalDamage * (1 - defenders[impactIndex].armor / (defenders[impactIndex].armor + 10));

                    defenders[impactIndex].health -= impactDamage;

                    //복수
                    if (frontDefender.vengeance)
                    {
                        frontAttaker.health -= impactDamage;
                    }

                }
            }

            // 회피 판정
            if (CalculateAccuracy(frontDefender, frontAttaker.perfectAccuracy))
            {
                normalDamage = 0;
            }
            else
            {
                //반격
                if (isFirstAttack && frontDefender.counter)
                {
                    if (!CalculateAccuracy(frontAttaker, frontDefender.perfectAccuracy))
                    {
                        frontAttaker.health -= normalDamage;
                    }
                }
                else
                {
                    frontDefender.health -= normalDamage;
                }

                //작열
                CalculateBurning(frontAttaker, defenders);

                // 가시 피해
                if (frontDefender.thorns && normalDamage>0)
                {
                    frontAttaker.health -= thornsDamageValue;
                }

                // 흡혈 (한 번만 적용)
                if (frontAttaker.lifeDrain)
                {
                    float heal = HealHealth(frontAttaker, Mathf.Min(Mathf.Round(frontAttaker.health * bloodSuckingValue), frontAttaker.maxHealth));
                    frontAttaker.health = heal;
                }
            }

            totalDamage += normalDamage;
        }

        return totalDamage;
    }
    //지원 페이즈 시 발동
    public void ProcessSupportAbility(List<RogueUnitDataBase> attackers, List<RogueUnitDataBase> defenders,bool isTeam,float finalDamage) 
    {
        //원거리 공격
        float damage =CalculateRangeAttack(attackers, defenders,isTeam,finalDamage);
        if (isTeam)
        {
            defenders[0].health -=damage;
        }
        else
        {
            if (damage > 0)
            {
                attackers[0].health -= RelicManager.ReactiveThornArmor(attackers[0]);
            }
        }
        //치유
        ProcessHealing(attackers);
        //지원 종료
        DamageBurning(attackers[0]);

    }

    // 유닛 사망 처리
    public bool ProcessDeath(
        ref List<RogueUnitDataBase> myUnits, ref List<RogueUnitDataBase> enemyUnits,
        ref List<RogueUnitDataBase> myDeathUnits, ref List<RogueUnitDataBase> enemyDeathUnits,
        ref bool isFirstAttack)
    {
        bool myUnitDied = false;   // 내 유닛 중 0번이 죽었는지 체크
        bool enemyUnitDied = false; // 상대 유닛 중 0번이 죽었는지 체크

        List<RogueUnitDataBase> tempMyDeathUnits = new();  // 임시 사망 유닛 리스트
        List<RogueUnitDataBase> tempEnemyDeathUnits = new(); // 임시 사망 유닛 리스트

        // 내 유닛 사망 처리 (삭제할 유닛을 리스트에 추가)
        for (int i = myUnits.Count - 1; i >= 0; i--) // 뒤에서부터 순회
        {
            if (myUnits[i].health <= 0)
            {
                if (i == 0 && RelicManager.HeartGemNecklace())
                {
                    myUnits[i].health = myUnits[i].maxHealth; // 유닛 체력 회복
                    continue; // 유닛이 살아났으므로 사망 처리를 하지 않음
                }

                CalculateMartyrdom(myUnits, i);

                myUnits[i].alive = false;
                tempMyDeathUnits.Add(myUnits[i]); // 임시 리스트에 추가

                // 0번 유닛이 사망했는지 체크
                if (i == 0) myUnitDied = true;

                myUnits.RemoveAt(i);
            }
        }

        // 상대 유닛 사망 처리 (삭제할 유닛을 리스트에 추가)
        for (int i = enemyUnits.Count - 1; i >= 0; i--) // 뒤에서부터 순회
        {
            if (enemyUnits[i].health <= 0)
            {
                CalculateMartyrdom(enemyUnits, i);

                enemyUnits[i].alive = false;
                tempEnemyDeathUnits.Add(enemyUnits[i]); // 임시 리스트에 추가

                // 0번 유닛이 사망했는지 체크
                if (i == 0) enemyUnitDied = true;

                enemyUnits.RemoveAt(i);
            }
        }        

        // 사망한 유닛들에 대한 이벤트 한 번만 호출
        if (tempMyDeathUnits.Count > 0)
        {
            OnUnitDeath(tempMyDeathUnits, ref enemyUnits, myUnits, false, enemyUnitDied,myUnitDied);
        }
        if (tempEnemyDeathUnits.Count > 0)
        {
            OnUnitDeath(tempEnemyDeathUnits, ref myUnits, enemyUnits, true, myUnitDied,enemyUnitDied);
        }
        //유산
        if (tempMyDeathUnits.Count > 0 && RelicManager.Relic55())
        {
            foreach (RogueUnitDataBase unit in tempMyDeathUnits)
            {
                enemyUnits[0].health -= unit.health * 0.1f;
            }
            if (enemyUnits[0].health < 0)
            {
                CalculateMartyrdom(enemyUnits, 0);
                enemyUnits[0].alive = false;
                tempEnemyDeathUnits.Add(enemyUnits[0]);
                enemyUnitDied = true;
                enemyUnits.RemoveAt(0);
                OnUnitDeath(tempEnemyDeathUnits, ref myUnits, enemyUnits, true, myUnitDied, enemyUnitDied);
            }
        }

        // 사망한 유닛을 최종 사망 리스트에 추가
        myDeathUnits.AddRange(tempMyDeathUnits);
        enemyDeathUnits.AddRange(tempEnemyDeathUnits);

        // 내 유닛 또는 상대 유닛의 0번째가 죽었으면 isFirstAttack 활성화
        if (myUnitDied || enemyUnitDied)
        {
            isFirstAttack = true;
            return true;
        }
        return false;
    }


    // 유닛 사망 시 실행되는 함수 (추가 기능 확장 가능)
    private void OnUnitDeath(List<RogueUnitDataBase> deadUnits,ref List<RogueUnitDataBase> attackers,List<RogueUnitDataBase> defenders, bool isMyUnit,bool isFrontAttackerDead,bool isFrontDefendrDead)
    {
        //봉인풀린자


        //스킬 사용 유닛이 안죽었을 시
        if (!isFrontAttackerDead)
        {
            //유격
            if (attackers[0].guerrilla && CheckBackUnit(attackers))
            {
                (attackers[1], attackers[0]) = (attackers[0], attackers[1]);
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
    private void CalculateFirstStrike(RogueUnitDataBase attaker, List<RogueUnitDataBase> defenders,float finalDamage)
    {
        if (attaker.firstStrike && !attaker.fStriked && CheckBackUnit(defenders))
        {
            int minHealthIndex= CalculateMinHealthIndex(defenders);

            defenders[minHealthIndex].health -= attaker.attackDamage * 2 * finalDamage;

            attaker.fStriked = true;
        }
    }
    //위압
    private void CalculateOverwhelm(RogueUnitDataBase attacker,RogueUnitDataBase defender)
    {
        defender.mobility = overwhelmValue;
    }
    //투창
    private void CalculateThrowSpear(RogueUnitDataBase attacker, RogueUnitDataBase defender,float finalDamage)
    {
        float damage = throwSpearValue * finalDamage;
        if (!CalculateAccuracy(defender, attacker.perfectAccuracy))
        {
            defender.health -= damage;
        }
    }
    //암살
    private void CalculateAssassination(RogueUnitDataBase attacker, List<RogueUnitDataBase> defenders, float finalDamage)
    {
        int minHealthIndex = CalculateMinHealthIndex(defenders);
        if (minHealthIndex == -1) return;
        float damage = attacker.attackDamage *assassinationValue* finalDamage;
        if (defenders[0].guard)
        {
            if (!CalculateAccuracy(defenders[0], attacker.perfectAccuracy))
            {
                defenders[0].health -= damage * (1 - (defenders[0].armor / (defenders[0].armor + 10)));
            }
        }
        else
        {
            defenders[minHealthIndex].health -= damage;
            //복수
            if (defenders[0].vengeance)
            {
                attacker.health -= damage;
            }
        }
    }
    //상흔
    private void CalculateWounding(RogueUnitDataBase attacker, RogueUnitDataBase defender)
    {
        int scarId = 1, type = 1, rank = 1, duration = -1;
        defender.effectDictionary[scarId] = new BuffDebuffData(scarId, type, rank, duration);
    }
    //충돌 isFirstAttack
    private void ChrashIsFirstAttack(RogueUnitDataBase attacker, RogueUnitDataBase defender,ref float multiplier,ref float reduceDamage)
    {
        //돌격
        if (attacker.charge)
        {
            multiplier = CalculateCharge(attacker.mobility);
            //강한 돌격
            if (attacker.strongCharge)
            {
                multiplier += strongChargeValue;
            }
            //수비자 수비태세 시
            if (defender.defense)
            {
                reduceDamage += defenseValue;
            }
        }
        //공격자 수비태세
        if (attacker.defense)
        {
            //수비자 돌격 시
            if (defender.charge)
            {
                reduceDamage -= defenseValue;
            }
            else
            {
                reduceDamage += defenseValue;
            }
        }
    }

    //충돌 특성 기술 발동
    private float ApplyChrashAbility(RogueUnitDataBase attacker, RogueUnitDataBase defender, float _reduceDamage)
    {
        float reduceDamage =_reduceDamage;
        Dictionary<Func<RogueUnitDataBase, bool>, Action<RogueUnitDataBase>> traitEffects = new()
        {
            { unit => unit.bluntWeapon && defender.heavyArmor, unit => reduceDamage -= bluntWeaponValue }, // 둔기
            { unit => unit.slaughter && defender.lightArmor, unit => reduceDamage -= slaughterValue }, // 도살
            { unit => (defender.branchIdx == 5 || defender.branchIdx == 6) && unit.antiCavalry > 0, unit => reduceDamage -= unit.antiCavalry }, // 대기병
            { unit => unit.suppression && reduceDamage < 0, unit => reduceDamage *= suppressionValue } // 제압
        };

        foreach (var trait in traitEffects)
        {
            if (trait.Key(attacker))
            {
                trait.Value(attacker);
            }
        }
        return reduceDamage;
    }

    //돌격 계산
    private float CalculateCharge(float mobility)
    {
        return 1.1f + (1.9f / (9 * (mobility - 1)));
    }

    //회피율 계산
    private float CalculateDodge(RogueUnitDataBase unit)
    {
        float dodge;
        float smoke = 0;
        // 키 2가 존재하는지 확인 후 값을 가져옴
        if (unit.effectDictionary.TryGetValue(2, out BuffDebuffData _))
        {
            smoke = 15;
        }
        dodge = (2 + (13 / 9) * (unit.mobility - 1)) + (unit.agility ? 10.0f : 0) + smoke;

        return Mathf.Clamp(dodge, 0, 100);
    }

    //회피 유무 계산
    private bool CalculateAccuracy(RogueUnitDataBase unit, bool isPerfectAccuracy)
    {
        if (isPerfectAccuracy)
            return false; // 필중 특성인 경우 회피 불가

        float dogeRate = CalculateDodge(unit);

        bool result = dogeRate > Random.Range(0, 100);

        return result;
    }

    //체력 회복
    private float HealHealth(RogueUnitDataBase unit, float healValue)
    {
        if (unit.effectDictionary[1] != null) return 0;
        return healValue;
    }

    //연막
    private void CalculateSmokeScreen(List<RogueUnitDataBase> units)
    {
        if (units[0].smokeScreen && CheckBackUnit(units))
        {
            int id = 2, type = 0, rank = 1, duration = -1;
            for (int i = 1; i < units.Count; i++)
            {
                if(units[i].health>0)
                {
                    units[i].effectDictionary[id] = new BuffDebuffData(id, type, rank, duration);
                }
            }
        }
    }
    //작열 발동
    private void CalculateBurning(RogueUnitDataBase attacker, List<RogueUnitDataBase> defenders)
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
        }
    }
    //작열 데미지
    private void DamageBurning(RogueUnitDataBase unit)
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
    }
    // 치유(Healing) 기능 - 후열 유닛이 전열 유닛을 치유
    private void ProcessHealing(List<RogueUnitDataBase> units)
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
                if (damage > 0 && defenders[0].heavyArmor)
                {
                    damage = Mathf.Max(0, damage - heavyArmorValue);
                }

                // 작열 적용
                CalculateBurning(attacker, defenders);

                // 가시 피해 (thorns)
                if (damage > 0 && defenders[0].thorns)
                {
                    attacker.health -= thornsDamageValue;
                }

                allDamage += damage;
            }
        }
        return allDamage;
    }

    //순교 0,1번이 동시에 사망해도 1번에 버프
    private void CalculateMartyrdom(List<RogueUnitDataBase> defenders,int defenderIndex)
    {
        if (defenders[defenderIndex].martyrdom)
        {
            if(defenderIndex+1 < defenders.Count)
            {
                defenders[defenderIndex + 1].attackDamage = Mathf.Round(defenders[defenderIndex].attackDamage * martyrdomValue);
            }
        }
    }
}
