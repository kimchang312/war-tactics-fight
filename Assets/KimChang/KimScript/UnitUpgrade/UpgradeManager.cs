using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager
{
    // 병종별 강화 수치를 저장하는 클래스
    public class UpgradeValues
    {
        public float healthBoost = 0;
        public float armorBoost = 0;
        public float attackDamageBoost = 0;
        public float mobilityBoost = 0;
        public float rangeBoost = 0;
        public float antiCavalryBoost = 0;
    }

    // 병종별 고정 리스트
    private readonly UpgradeValues[] upgradeValues;

    // 싱글톤 패턴 적용
    private static UpgradeManager instance;

    public static UpgradeManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new UpgradeManager();
            }
            return instance;
        }
    }

    // private 생성자로 외부에서 생성 방지
    private UpgradeManager()
    {
        // 병종 수 9개 고정
        upgradeValues = new UpgradeValues[9];
        for (int i = 0; i < 9; i++)
        {
            upgradeValues[i] = new UpgradeValues();
        }
    }

    // 특정 병종의 특정 강화 수치를 증가시키는 함수
    public void Upgrade(int branchIdx, int stat, float increment=5)
    {
        switch (stat)
        {
            case 0:
                upgradeValues[branchIdx].healthBoost += increment;
                break;
            case 1:
                upgradeValues[branchIdx].armorBoost += increment;
                break;
            case 2:
                upgradeValues[branchIdx].attackDamageBoost += increment;
                break;
            case 3:
                upgradeValues[branchIdx].mobilityBoost += increment;
                break;
            case 4:
                upgradeValues[branchIdx].rangeBoost += increment;
                break;
            case 5:
                upgradeValues[branchIdx].antiCavalryBoost += increment;
                break;
            default:
                Debug.LogError("Invalid stat type!");
                break;
        }
    }

    // 특정 병종의 강화 수치를 초기화하는 함수
    public void ResetUpgrade(int branchIdx)
    {
        upgradeValues[branchIdx] = new UpgradeValues();
    }

    // 특정 병종의 현재 강화 수치를 반환하는 함수
    public UpgradeValues GetUpgradeValues(int branchIdx)
    {
        return upgradeValues[branchIdx];
    }

    // 유닛을 넣으면 강화 수치만큼 증가해서 반환
    public UnitDataBase UpgradeUnit(UnitDataBase unit)
    {
        // 병종 인덱스를 가져옵니다.
        int branchIdx = unit.branchIdx;

        // 병종에 해당하는 강화 수치를 가져옵니다.
        var upgradeValues = UpgradeManager.Instance.GetUpgradeValues(branchIdx);

        // 유닛의 능력치를 강화 수치만큼 증가시킵니다.
        unit.maxHealth += upgradeValues.healthBoost;
        unit.health += upgradeValues.healthBoost;
        unit.armor += upgradeValues.armorBoost;
        unit.attackDamage += upgradeValues.attackDamageBoost;
        unit.mobility += upgradeValues.mobilityBoost;
        unit.range += upgradeValues.rangeBoost;
        unit.antiCavalry += upgradeValues.antiCavalryBoost;

        return unit;
    }

    public RogueUnitDataBase UpgradeRogueLikeUnit(RogueUnitDataBase unit)
    {
        // 병종 인덱스를 가져옵니다.
        int branchIdx = unit.branchIdx;

        // 병종에 해당하는 강화 수치를 가져옵니다.
        var upgradeValues = UpgradeManager.Instance.GetUpgradeValues(branchIdx);

        // 유닛의 능력치를 강화 수치만큼 증가시킵니다.
        unit.maxHealth += upgradeValues.healthBoost;
        unit.health += upgradeValues.healthBoost;
        unit.armor += upgradeValues.armorBoost;
        unit.attackDamage += upgradeValues.attackDamageBoost;
        unit.mobility += upgradeValues.mobilityBoost;
        unit.range += upgradeValues.rangeBoost;
        unit.antiCavalry += upgradeValues.antiCavalryBoost;

        return unit;
    }




    private void ProcessUpgrade()
    {
        var myUnits = RogueLikeData.Instance.GetMyUnits();

        foreach (var unit in myUnits)
        {
            int idx = unit.branchIdx;
            int atkLv = RogueLikeData.Instance.GetUpgrade(idx, true);
            int defLv = RogueLikeData.Instance.GetUpgrade(idx, false);

            // 기본 강화 적용
            if (atkLv > 0)
                unit.attackDamage += Mathf.Floor(unit.baseAttackDamage * atkLv * 0.1f);

            if (defLv > 0)
                unit.health += Mathf.Floor(unit.baseHealth * defLv * 0.1f);

            // 병종별 특수 강화
            switch (idx)
            {
                case 0:
                    if (atkLv == 5)
                        unit.antiCavalry += Mathf.Floor(unit.baseAntiCavalry * 0.3f);
                    break;

                case 1:
                    if (atkLv == 5)
                        unit.attackDamage += Mathf.Floor(unit.baseAttackDamage * 0.15f);
                    break;

                case 2:
                    if (atkLv == 5)
                        unit.range += 1;
                    if (defLv == 5)
                        unit.mobility += 5;
                    break;

                case 3:
                    if (atkLv == 5)
                        unit.attackDamage += Mathf.Floor(unit.baseAttackDamage * 0.15f);
                    break;

                case 4:
                    if (atkLv == 5)
                        unit.attackDamage += Mathf.Floor(unit.baseAttackDamage * 0.15f);
                    if (defLv == 5)
                        unit.mobility += 5;
                    break;

                case 5:
                case 6:
                    if (atkLv == 5)
                        unit.mobility += 5;
                    break;

                case 7:
                    if (atkLv == 5)
                        unit.range += 1;
                    break;
            }
        }
    }

}

[System.Serializable]
public class UnitUpgrade
{
    public int attackLevel = 0;
    public int defenseLevel = 0;
}
