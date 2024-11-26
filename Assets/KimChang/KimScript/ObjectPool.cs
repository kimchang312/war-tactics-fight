using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject damageTextPrefab;   //전투 데미지
    [SerializeField] private GameObject battleUnitPrefab;    //전투화면 유닛
    [SerializeField] private Transform canvasTransform;         //캔버스
    [SerializeField] private GameObject abilityPrefab;      //특성+기술 아이콘

    private readonly Queue<GameObject> damageTextPool = new();
    private readonly Queue<GameObject> battleUnitPool = new();
    private readonly Queue<GameObject> abilityPool = new();

    private readonly List<GameObject> activeBattleUnits = new(); // 활성화된 유닛을 추적
    private readonly List<GameObject> activeAbilitys= new();      //활성화된 능력 아이콘 추적
    private int poolSize = 20;

    // 초기 풀 생성
    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject damageInstance = Instantiate(damageTextPrefab, transform);
            GameObject unitInstance = Instantiate(battleUnitPrefab, transform);
            GameObject abilityInstance= Instantiate(abilityPrefab, transform);

            damageInstance.SetActive(false);
            unitInstance.SetActive(false);
            abilityInstance.SetActive(false);

            damageTextPool.Enqueue(damageInstance);
            battleUnitPool.Enqueue(unitInstance);
            abilityPool.Enqueue(abilityInstance);
        }
    }

    //능력 아이콘 가져오기
    public GameObject GetAbility()
    {
        GameObject instance;

        if (abilityPool.Count > 0)
        {
            instance = abilityPool.Dequeue();
        }
        else
        {
            instance =Instantiate(abilityPrefab, transform);
        }

        instance.SetActive(true);
        instance.transform.SetParent(canvasTransform,transform);
        activeAbilitys.Add(instance);
        return instance;

    }

    // 활성화된 아이콘 리스트 반환
    public List<GameObject> GetActiveAbilitys()
    {
        return new List<GameObject>(activeAbilitys); // 활성화된 유닛 복사본 반환
    }

    //능력 아이콘 반환
    public void ReturnAbility(GameObject gameObject)
    {
        gameObject.SetActive(false);
        gameObject.transform.SetParent(canvasTransform,transform);
        activeAbilitys.Remove(gameObject);
        abilityPool.Enqueue(gameObject);

    }

    //능력 아이콘 전부 비활성화
    public void ClearActiveAbilitys()
    {
        foreach (var unit in activeAbilitys)
        {
            unit.SetActive(false);
            unit.transform.SetParent(canvasTransform, false);
            abilityPool.Enqueue(unit);
        }
        activeAbilitys.Clear();
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
        instance.transform.SetParent(canvasTransform, false);
        activeBattleUnits.Add(instance); // 활성화된 유닛 리스트에 추가
        return instance;
    }

    // 활성화된 유닛 리스트 반환
    public List<GameObject> GetActiveBattleUnits()
    {
        return new List<GameObject>(activeBattleUnits); // 활성화된 유닛 복사본 반환
    }

    //유닛반환
    public void ReturnBattleUnit(GameObject unitImage)
    {
        unitImage.SetActive(false);
        unitImage.transform.SetParent(canvasTransform, false);
        activeBattleUnits.Remove(unitImage);
        battleUnitPool.Enqueue(unitImage);
    }

    //유닛 전부 비활성화
    public void ClearActiveBattleUnits()
    {
        foreach (var unit in activeBattleUnits)
        {
            unit.SetActive(false);
            unit.transform.SetParent(canvasTransform, false);
            battleUnitPool.Enqueue(unit);
        }
        activeBattleUnits.Clear();
    }


    //함수 호출 시 있다면 비활성화된 text반환 및 풀에서 제거 없다면 생성
    public GameObject GetDamageText()
    {
        GameObject instance;

        if (damageTextPool.Count > 0)
        {
            instance = damageTextPool.Dequeue();
        }
        else
        {
            instance = Instantiate(damageTextPrefab, transform);
        }

        instance.SetActive(true);
        instance.transform.SetParent(canvasTransform, false);
        RectTransform rectTransform = instance.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero; // 초기 위치 설정
        return instance;
    }

    //함수 호출 시 text비활성화 시키고 pooling
    public void ReturnDamageText(GameObject damageText)
    {
        damageText.SetActive(false);
        damageText.transform.SetParent(canvasTransform, false);
        damageTextPool.Enqueue(damageText);
    }


}
