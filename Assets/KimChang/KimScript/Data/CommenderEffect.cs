using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public static class CommenderEffect
{

    //핸드릭슨
    public static void CalculateHendrix() { }
    //래논
    public static void CalculateLennon() { }
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
    public static void CalculateKurt() { }
    //잰더
    public static void CalculateZander() { }
    public static void CalculateOzzy() { }
    //슬래시
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

    public static void CalculateCobain() { }

    public static void CalculateClapton()
    {
        var allUnits = RogueLikeData.Instance.GetMyUnits();
        allUnits.AddRange(RogueLikeData.Instance.GetEnemyUnits());

        foreach (var unit in allUnits)
        {
            unit.antiCavalry *= 2;
        }
    }

    public static void CalculateAxl() { }

    public static void CalculateBowie()
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();
        foreach (var unit in myUnits)
        {
            int traitCount = CountUnitTrait(unit); // 유닛이 가진 특성 개수
            unit.health -= 5 * traitCount;
        }
        foreach (var unit in enemyUnits)
        {
            unit.suppression =true;
        }
    }

    public static void CalculateDylan() { }

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

    public static void CalculatePerry() { }

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
    public static void CalculateBruennar() { }
    public static void CalculateSirion() { }
    public static void CalculateValeric() { }
    public static void CalculateGrondal() { }
    public static void CalculateErebos()
    {
        var allUnits = RogueLikeData.Instance.GetMyUnits();
        allUnits.AddRange(RogueLikeData.Instance.GetEnemyUnits());
        foreach (var unit in allUnits)
        {
            ClearUnitSkill(unit);
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
                unit.armor += Mathf.Round(unit.baseArmor*0.15f);
            }
        }
    }
    public static void CalculateTorhdan()
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();
        foreach (var unit in enemyUnits)
        {
            if (unit.lightArmor)
            {
                unit.maxHealth += 50;
                unit.health = unit.maxHealth;
            }
            if (unit.branchIdx == 0)
            {
                unit.vengeance =true;
            }
            else if (unit.branchIdx == 2)
            {
                unit.attackDamage += 10;
            }
        }
        foreach(var unit in myUnits)
        {
            if (unit.lightArmor) { 
}           unit.maxHealth += 50;
            unit.health = unit.maxHealth;
        }



    }
    public static void CalculateOrtheon() { }
    public static void CalculateAsmodeus() { }
    public static void CalculateHosh(){}
    public static void CalculateStein()
    {
        var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();
        foreach (var unit in enemyUnits)
        {
            if (unit.idx == 60)
            {
                unit.maxHealth = unit.baseHealth * 2;
                unit.health = unit.maxHealth;
            }
            int traitCount = CountUnitTrait(unit);
            unit.attackDamage += (5 * traitCount);
        }
    }
    public static void CalculateAziras(){ }
    public static void CalculateChromehold()
    {
        var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();
        foreach (var unit in enemyUnits)
        {
            if (unit.branchIdx == 0) // 창병
            {
                unit.throwSpear = true;
            }
            else if (unit.branchIdx == 1) // 전사
            {
                unit.assassination = true;
            }
        }
    }
    //벨페고르
    public static void CalculateBelphegor()
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();

        if (myUnits.Count == 0) return;

        // myUnits에서 무작위 유닛 1개 선택
        var selectedUnit = myUnits[UnityEngine.Random.Range(0, myUnits.Count)];

        // 공격력 및 체력 30% 증가
        selectedUnit.attackDamage = Mathf.Round(selectedUnit.baseAttackDamage * 1.3f);
        selectedUnit.maxHealth = Mathf.RoundToInt(selectedUnit.baseHealth * 1.3f);
        selectedUnit.health = selectedUnit.maxHealth;

        // 적 유닛에 추가
        enemyUnits.Add(selectedUnit);
        myUnits.Remove(selectedUnit);
        RogueLikeData.Instance.SetAllEnemyUnits(enemyUnits);
        RogueLikeData.Instance.SetAllMyUnits(myUnits);
    }

    public static void CalculateMelsedec(){ }


    public static int CountUnitTrait(RogueUnitDataBase unit)
    {
        int count = 0;
        if (unit.lightArmor) count++;
        if (unit.heavyArmor) count++;
        if (unit.rangedAttack) count++;
        if(unit.bluntWeapon) count++;
        if(unit.pierce) count++;
        if(unit.agility) count++;
        if(unit.strongCharge) count++;
        if(unit.perfectAccuracy) count++;
        if(unit.slaughter) count++;
        if(unit.bindingForce) count++;
        if(unit.bravery) count++;
        if(unit.suppression) count++;
        if(unit.plunder) count++;
        if(unit.doubleShot) count++;
        if(unit.scorching) count++;
        if(unit.thorns) count++;
        if(unit.endless) count++;
        if(unit.impact) count++;
        if(unit.healing) count++;
        if(unit.lifeDrain) count++;
        return count;
    }

    public static void ClearUnitSkill(RogueUnitDataBase unit)
    {
        unit.charge = false;
        unit.defense = false;
        unit.throwSpear = false;
        unit.guerrilla = false;
        unit.guard = false;
        unit.assassination = false;
        unit.drain = false;
        unit.overwhelm = false;
        unit.martyrdom = false;
        unit.wounding = false;
        unit.vengeance = false;
        unit.counter = false;
        unit.firstStrike = false;
        unit.challenge = false;
    }

}
