using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private int poolSize = 10;
    private readonly Queue<GameObject> pool = new ();

    //초기 풀 갯수 선언
    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject instance = Instantiate(damageTextPrefab, transform);
            instance.SetActive(false);
            pool.Enqueue(instance);
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
}
