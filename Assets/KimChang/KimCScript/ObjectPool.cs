using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject damageTextPrefab;   //전투 데미지
    [SerializeField] private GameObject battleUnitPrefab;    //전투화면 유닛
    [SerializeField] private int poolSize = 20;

    private readonly Queue<GameObject> damageTextPool = new();
    private readonly Queue<GameObject> battleUnitPool = new();
    private readonly List<GameObject> activeBattleUnits = new(); // 활성화된 유닛을 추적

    // 초기 풀 생성
    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject damageInstance = Instantiate(damageTextPrefab, transform);
            GameObject unitInstance = Instantiate(battleUnitPrefab, transform);

            damageInstance.SetActive(false);
            unitInstance.SetActive(false);

            damageTextPool.Enqueue(damageInstance);
            battleUnitPool.Enqueue(unitInstance);
        }
    }

    // 유닛 가져오기
    public GameObject GetBattleUnit()
    {
        GameObject instance;

        if (battleUnitPool.Count > 0)
        {
            instance = battleUnitPool.Dequeue();
        }
        else
        {
            instance = Instantiate(battleUnitPrefab, transform);
        }

        instance.SetActive(true);
        activeBattleUnits.Add(instance); // 활성화된 유닛 리스트에 추가
        return instance;
    }

    // 활성화된 유닛 리스트 반환
    public List<GameObject> GetActiveBattleUnits()
    {
        return new List<GameObject>(activeBattleUnits); // 활성화된 유닛 복사본 반환
    }

    // 유닛 반환
    public void ReturnBattleUnit(GameObject unitImage)
    {
        unitImage.SetActive(false);
        activeBattleUnits.Remove(unitImage); // 활성화된 유닛 리스트에서 제거
        battleUnitPool.Enqueue(unitImage);
    }


    //함수 호출 시 있다면 비활성화된 text반환 및 풀에서 제거 없다면 생성
    public GameObject GetDamageText()
    {
        if (damageTextPool.Count > 0)
        {
            GameObject instance = damageTextPool.Dequeue();
            instance.SetActive(true);
            return instance;
        }
        else
        {
            GameObject newInstance = Instantiate(damageTextPrefab, transform);
            newInstance.SetActive(true);
            return newInstance;
        }
    }

    //함수 호출 시 text비활성화 시키고 pooling
    public void ReturnDamageText(GameObject damageText)
    {
        damageText.SetActive(false);
        damageTextPool.Enqueue(damageText);
    }


}
