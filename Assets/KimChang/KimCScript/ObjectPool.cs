using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject damageTextPrefab;   //전투 데미지
    [SerializeField] private GameObject battleUnitPrefab;    //전투화면 유닛
    [SerializeField] private int poolSize = 10;

    private readonly Queue<GameObject> pool = new ();

    //초기 풀 갯수 선언
    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject damageinstance = Instantiate(damageTextPrefab, transform);
            GameObject unitInstance = Instantiate(battleUnitPrefab, transform);
            damageinstance.SetActive(false);
            unitInstance.SetActive(false);
            pool.Enqueue(unitInstance);
            pool.Enqueue(damageinstance);
        }
    }

    //함수 호출 시 있다면 비활성화된 text반환 및 풀에서 제거 없다면 생성
    public GameObject GetDamageText()
    {
        if (pool.Count > 0)
        {
            GameObject instance = pool.Dequeue();
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
        pool.Enqueue(damageText);
    }

    // 활성화된 유닛 이미지를 반환
    public GameObject GetUnitImage()
    {
        if (pool.Count > 0)
        {
            GameObject instance = pool.Dequeue();
            instance.SetActive(true);
            return instance;
        }
        else
        {
            GameObject newInstance = Instantiate(battleUnitPrefab, transform);
            newInstance.SetActive(true);
            return newInstance;
        }
    }

    // 비활성화된 유닛 이미지를 반환
    public void ReturnUnitImage(GameObject unitImage)
    {
        unitImage.SetActive(false);
        pool.Enqueue(unitImage);
    }

}
