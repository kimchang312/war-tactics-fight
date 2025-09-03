using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public static class CommenderEffect
{
    //프레디
    public static void CalculateFreddy()
    {
        //플레이어는 적보다 유닛을 많이 배치할 수 없다
    }

    //핸드릭슨
    public static void CalculateHendrix() 
    { 
        var myUnits= RogueLikeData.Instance.GetMyUnits();
        var enemyUnits =RogueLikeData.Instance.GetEnemyUnits();
        foreach (var unit in myUnits)
        {
            if(unit.branchIdx == 5)
            {
                unit.mobility = Math.Max(1, unit.mobility - 1);

            }
            else if(unit.branchIdx == 4)
            {
                unit.effectDictionary[12] = new(12, 0, 1, -1);
            }
        }
        foreach (var unit in enemyUnits)
        {
            if (unit.branchIdx == 5 || unit.branchIdx == 4)
            {
                unit.effectDictionary[12] = new(12, 0, 1, -1);
            }
        }
    }
    //레논
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
    public static void CalculateZander(bool isTeam,int index) 
    { 
        if(RogueLikeData.Instance.GetPresetID() == 54 && !isTeam)
        {
            var myUnits = RogueLikeData.Instance.GetMyUnits();
            RogueUnitDataBase unit = myUnits[index];
            unit.armor = Math.Max(0, unit.armor - 2);
        }
    }
    public static void CalculateOzzy() { }
    //슬래시
    public static void CalculateSlash()
    {
        var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();

        foreach (var unit in enemyUnits)
        {
            if (unit.branchIdx == 1 || unit.branchIdx == 4)
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
    //액슬
    public static void CalculateAxl() 
    { 
        if(RogueLikeData.Instance.GetPresetID() == 55)
        {
            var allUnits = RogueLikeData.Instance.GetMyUnits();
            allUnits.AddRange(RogueLikeData.Instance.GetEnemyUnits());
            foreach(var unit in allUnits)
            {
                if(unit.health <= 50)
                {
                    unit.health = 0;
                }
            }

        }
    }

    public static void CalculateBowie()
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();
        var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();
        foreach (var unit in myUnits)
        {
            int traitCount = CountUnitTrait(unit);
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
            if (unit.branchIdx == 3)
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
            unit.effectDictionary[0] = new BuffDebuffData(0, 1, 1, 2);
        }
    }

    public static void CalculateVedder() { }
    //페이지
    public static void CalculatePage() 
    { 
        //더 많은 부대가치
    }
    public static void CalculateSyd() { }
    //아마록
    public static void CalculateAmarok() 
    {
        var enemyUnits = RogueLikeData.Instance.GetEnemyUnits();
        foreach(var unit in enemyUnits)
        {
            unit.effectDictionary[11] = new(11, 0, 1, -1);
        }
    }
    public static void CalculateBruennar() { }
    //시리온
    public static void CalculateSirion() 
    {
        //적 유닛이 죽으면 적이 원거리 공격가능 유닛이 원거리 공격
        //구현
    }
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
    //라자루스
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
            if (unit.branchIdx == 3)
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
    //호쉬
    public static void CalculateHosh()
    {
        var allUnits = RogueLikeData.Instance.GetMyUnits();
        allUnits.AddRange(RogueLikeData.Instance.GetEnemyUnits());
        foreach (var unit in allUnits)
        {
            unit.mobility = 1;
        }
        //구현
    }
    //슈타인
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
            if (unit.branchIdx == 0)
            {
                unit.throwSpear = true;
            }
            else if (unit.branchIdx == 1)
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

        var selectedUnit = myUnits[RogueLikeData.Instance.GetRandomInt(0, myUnits.Count)];

        selectedUnit.attackDamage = Mathf.Round(selectedUnit.baseAttackDamage * 1.3f);
        selectedUnit.maxHealth = Mathf.RoundToInt(selectedUnit.baseHealth * 1.3f);
        selectedUnit.health = selectedUnit.maxHealth;

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
