using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoBattleManager : MonoBehaviour
{
    // 예시 유닛, 추후 삭제
    private int[] _myUnitIds = { 1, 2 };
    private int[] _enemyUnitIds = { 3, 4 };


    // 유닛 id들로 유닛 데이터를 정리하는 함수
    // Tuple을 사용하여 두 개의 배열을 반환
    private (UnitDataBase[] myUnits, UnitDataBase[] enemyUnits) GetUnits(int[] myUnitIds, int[] enemyUnitIds)
    {
        // 내가 선택한 유닛들 저장 (List를 사용)
        List<UnitDataBase> myUnits = new List<UnitDataBase>();

        // 적의 유닛 저장
        List<UnitDataBase> enemyUnits = new List<UnitDataBase>();

        // 내 유닛 ID들을 기반으로 유닛을 가져와서 MyUnits에 저장
        foreach (int unitId in myUnitIds)
        {
            UnitDataBase unit = UnitDataBase.GetUnitById(unitId);
            if (unit != null)
            {
                myUnits.Add(unit);  // List에 유닛 추가
            }
        }

        // 적의 유닛 ID들을 기반으로 유닛을 가져와서 enemyUnits에 저장
        foreach (int unitId in enemyUnitIds)
        {
            UnitDataBase unit = UnitDataBase.GetUnitById(unitId);
            if (unit != null)
            {
                enemyUnits.Add(unit);  // List에 유닛 추가
            }
        }

        // List를 배열로 변환한 후 튜플로 반환
        return (myUnits.ToArray(), enemyUnits.ToArray());
    }

    private int AutoBattle()
    {
        //var= GetUnits(_myUnitIds, _enemyUnitIds);

        return 0;
    }

}
