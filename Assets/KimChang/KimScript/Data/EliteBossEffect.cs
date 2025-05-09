using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EliteBossEffect
{

    //핸드릭슨
    public static void CalculateHendrix()
    {


    }
    //래논
    public static void CalculateLennon()
    {

    }
    //모리슨
    public static void CalcualteMorrison()
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();
        foreach (var unit in myUnits)
        {
            if (unit.branchIdx == 5 || unit.branchIdx == 6)
            {
                unit.mobility -= 2;
            }
        }
        foreach (var unit in enemyUnits)
        {
            if (unit.branchIdx == 2)
            {
                unit.range++;
                unit.attackDamage += Mathf.Round(unit.baseAttackDamage * 0.15f);
            }
            else if (unit.branchIdx == 5 || unit.branchIdx == 6)
            {
                unit.mobility -= 2;
            }

        }
    }
    //커트
    public static void CalculateKurt()
    {

    }
    //잰더
    public static void CalculateZander()
    {

    }
    public static void CalculateOzzy()
    {
        var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();

        foreach (var unit in enemyUnits)
        {
            if (unit.branchIdx == 0) // 중보병
            {
                unit.martyrdom = true; // 순교 특성 부여
            }
        }
    }

    public static void CalculateSlash()
    {
        var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();

        foreach (var unit in enemyUnits)
        {
            if (unit.branchIdx == 1 || unit.branchIdx == 6) // 전사, 암살자
            {
                unit.attackDamage += Mathf.Round(unit.baseAttackDamage * 0.2f);
                unit.maxHealth -= Mathf.Round(unit.baseHealth * 0.1f);
                unit.health = unit.maxHealth;
            }
        }
    }

    public static void CalculateCobain()
    {
        // 후열 체력/공격력 감소는 유닛 사망 시 처리 필요 → 생략
    }

    public static void CalculateClapton()
    {
        var allUnits = RogueLikeData.Instance.GetMyUnits();
        allUnits.AddRange(RogueLikeData.Instance.GetEnemyUnits());

        foreach (var unit in allUnits)
        {
            unit.antiCavalry *= 2;
        }
    }

    public static void CalculateAxl()
    {
        // 체력 50 이하 즉사 조건은 실시간 전투 조건 검사 필요 → 생략
    }

    public static void CalculateBowie()
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();
        foreach (var unit in myUnits)
        {
            //int traitCount = unit.GetTraitCount(); // 유닛이 가진 특성 개수
            //unit.health -= 5 * traitCount;
        }
        foreach (var unit in enemyUnits)
        {
            unit.suppression =true;
        }
    }

    public static void CalculateDylan()
    {
        var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();

        foreach (var unit in enemyUnits)
        {
            //unit.dodgeRate += 10;
        }
    }

    public static void CalculateUlrich()
    {
        var allUnits = RogueLikeData.Instance.GetMyUnits();
        allUnits.AddRange(RogueLikeData.Instance.GetEnemyUnits());

        foreach (var unit in allUnits)
        {
            if (unit.branchIdx == 0) // 중보병
            {
                unit.maxHealth += 150;
                unit.health = unit.maxHealth;
                unit.attackDamage -= Mathf.Round(unit.baseAttackDamage * 0.1f);
            }
        }
    }

    public static void CalculatePerry()
    {
        // 충돌 시 후열 피해는 전투 로직 필요 → 생략
    }

    public static void CalculateBonJovi()
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();

        foreach (var unit in myUnits)
        {
            unit.attackDamage -= 10;
        }
    }

    public static void CalculateTyler()
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();

        foreach (var unit in myUnits)
        {
            unit.effectDictionary[0] = new BuffDebuffData(0, 1, 1, 2); // 작열 상태 1 부여
        }
    }

    public static void CalculateVedder() { }
    public static void CalculatePage() { }
    public static void CalculateSyd() { }
    public static void CalculateAmarok() { }
    public static void CalculateBruennar()
    {
        var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();
        foreach (var unit in enemyUnits)
        {
            if (unit.branchIdx == 1) // 전사
            {
                unit.attackDamage += Mathf.Round(unit.baseAttackDamage * 0.15f);
            }
        }
    }
    public static void CalculateSirion() { }
    public static void CalculateValeric() { }
    public static void CalculateGrondal() { }
    public static void CalculateErebos()
    {
        var allUnits = RogueLikeData.Instance.GetMyUnits();
        allUnits.AddRange(RogueLikeData.Instance.GetEnemyUnits());
        foreach (var unit in allUnits)
        {
            //unit.DisableAllSkills(); // 모든 기술 무효화
        }
    }
    public static void CalculateLazaros()
    {
        var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();
        foreach (var unit in enemyUnits)
        {
            unit.drain = true;
            unit.lifeDrain = true;
        }
    }
    public static void CalculateAgmar()
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();
        foreach (var unit in myUnits)
        {
            unit.armor = Mathf.Max(0, unit.armor - 2);
        }
        foreach (var unit in enemyUnits)
        {
            if (unit.branchIdx == 0) // 중보병
            {
                unit.attackDamage += Mathf.Round(unit.baseAttackDamage * 0.15f);
                unit.armor += 15;
            }
        }
    }
    public static void CalculateTorhdan()
    {
        var allUnits = RogueLikeData.Instance.GetMyUnits();
        allUnits.AddRange(RogueLikeData.Instance.GetEnemyUnits());
        foreach (var unit in allUnits)
        {
            if (unit.lightArmor)
            {
                unit.health += 50;
            }
            if (unit.branchIdx == 2)
            {
                unit.attackDamage += 10;
            }
        }
    }
    public static void CalculateOrtheon() { }
    public static void CalculateAsmodeus() { }
    public static void CalculateHosh()
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in myUnits)
        {
            unit.mobility = 1;
            //unit.dodgeRate = 0;
        }
    }
    public static void CalculateStein()
    {
        var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();
        foreach (var unit in enemyUnits)
        {
            if (unit.idx == /* 노인 기사 id */ 99) // 임시 ID
            {
                unit.health *= 2;
            }
            //int traitCount = unit.GetTraitCount();
            //unit.attackDamage += 5 * traitCount;
        }
    }
    public static void CalculateAziras()
    {
        var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();
        foreach (var unit in enemyUnits)
        {
            unit.attackDamage = Mathf.Round(unit.attackDamage * 1.5f);
        }
    }
    public static void CalculateChromehold()
    {
        var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();
        foreach (var unit in enemyUnits)
        {
            if (unit.branchIdx == 3) // 창병
            {
                unit.throwSpear = true;
            }
            else if (unit.branchIdx == 1) // 전사
            {
                unit.assassination = true;
            }
        }
    }
    public static void CalculateBelphegor() { }
    public static void CalculateMelsedec()
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        foreach (var unit in myUnits)
        {
            unit.attackDamage = Mathf.Round(unit.attackDamage * 0.9f);
        }
    }




}
