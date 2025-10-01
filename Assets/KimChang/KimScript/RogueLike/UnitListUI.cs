using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitListUI : MonoBehaviour
{
    [SerializeField] private Transform unitList;
    private ObjectPool objectPool;
    private bool isSelectMode = false;


    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void Start()
    {
        objectPool = GameManager.Instance.objectPool;
    }

    private void OnEnable()
    {
        CreateUnitList();
    }

    public void CreateUnitList()
    {
        // 유닛 가져오고 정렬
        List<RogueUnitDataBase> units = RogueLikeData.Instance.GetMyUnits();
        GetSortedUnits(ref units);

        int childCount = unitList.childCount;
        
        // 1. 기존 자식 재사용
        for (int i = 0; i < units.Count; i++)
        {
            GameObject unitObj;
            if (i < childCount)
            {
                // 기존 오브젝트 재활용
                unitObj = unitList.GetChild(i).gameObject;
                unitObj.SetActive(true);
            }
            else
            {
                // 부족하면 새로 생성
                unitObj = objectPool.GetOrderUnit();
                unitObj.transform.SetParent(unitList, false);
            }

            // 데이터 갱신
            OneUnitUI oneUnit = unitObj.GetComponent<OneUnitUI>();
            oneUnit.unit = units[i];
            UIMaker.CreateSelectUnitEnergy(units[i], unitObj);
        }

        // 2. 남는 오브젝트는 풀로 반환
        for (int i = units.Count; i < childCount; i++)
        {
            objectPool.ReturnOrderUnit(unitList.GetChild(i).gameObject);
        }
    }
    public static void GetSortedUnits(ref List<RogueUnitDataBase> units)
    {
        int order = RogueLikeData.Instance.GetUnitOrder(); // 현재 정렬 모드 불러오기
        IOrderedEnumerable<RogueUnitDataBase> ordered;

        switch (order)
        {
            case 0: // 획득 오름
                ordered = units.OrderBy(u => u.acquiredDate);
                break;
            case 1: // 획득 내림
                ordered = units.OrderByDescending(u => u.UniqueId);
                break;

            case 2: // 희귀도 오름
                ordered = units.OrderBy(u => u.rarity)
                               .ThenBy(u => u.idx)
                               .ThenBy(u => u.energy)
                               .ThenBy(u => u.acquiredDate);
                break;
            case 3: // 희귀도 내림
                ordered = units.OrderByDescending(u => u.rarity)
                               .ThenBy(u => u.idx)
                               .ThenBy(u => u.energy)
                               .ThenBy(u => u.acquiredDate);
                break;

            case 4: // 병종 오름 (branchIdx 0~8)
                ordered = units.OrderBy(u => u.branchIdx)
                               .ThenBy(u => u.idx)
                               .ThenBy(u => u.energy)
                               .ThenBy(u => u.acquiredDate);
                break;
            case 5: // 병종 내림
                ordered = units.OrderByDescending(u => u.branchIdx)
                               .ThenBy(u => u.idx)
                               .ThenBy(u => u.energy)
                               .ThenBy(u => u.acquiredDate);
                break;

            case 6: // 기력 오름
                ordered = units.OrderBy(u => u.energy)
                               .ThenBy(u => u.idx)
                               .ThenBy(u => u.acquiredDate);
                break;
            case 7: // 기력 내림
                ordered = units.OrderByDescending(u => u.energy)
                               .ThenBy(u => u.idx)
                               .ThenBy(u => u.acquiredDate);
                break;

            case 8: // 이름 오름
                ordered = units.OrderBy(u => u.unitName)
                               .ThenBy(u => u.energy)
                               .ThenBy(u => u.acquiredDate);
                break;
            case 9: // 이름 내림
                ordered = units.OrderByDescending(u => u.unitName)
                               .ThenBy(u => u.energy)
                               .ThenBy(u => u.acquiredDate);
                break;

            default: // 기본값: 획득 오름
                ordered = units.OrderBy(u => u.acquiredDate);
                break;
        }
    }

}
