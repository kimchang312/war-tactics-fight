using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUnit
{
    public static UnitDataBase CreateLegionnaire()
    {
        return new UnitDataBase(
            idx: 0,
            unitName: "군단병",
            unitBranch: "보병",
            branchIdx: 0,
            unitId: 1001,
            unitExplain: "전투 시 부대에 있는 태그가 같은 유닛 하나 당 공격력 +5",
            unitImg: "Unit_Img_-1",
            unitFaction: "common",
            factionIdx: 0,
            unitPrice: 100,
            health: 130,
            armor: 4,
            attackDamage: 35,
            mobility: 3,
            range: 1,
            antiCavalry: 40,
            lightArmor: true,
            heavyArmor: false,
            rangedAttack: false,
            bluntWeapon: false,
            pierce: false,
            agility: false,
            strongCharge: false,
            perfectAccuracy: false,
            slaughter: false,
            charge: false,
            defense: true,
            throwSpear: false,
            guerrilla: false,
            guard: false,
            assassination: false,
            drain: false,
            overwhelm: false,
            blink: "빈칸",
            maxHealth: 130,
            unitTag: new int[] { 10 } // unitTag에 10 추가
        );
    }
}
