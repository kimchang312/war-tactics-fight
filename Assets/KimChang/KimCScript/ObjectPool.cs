using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private int poolSize = 10;
    private readonly Queue<GameObject> pool = new ();

    //�ʱ� Ǯ ���� ����
    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject instance = Instantiate(damageTextPrefab, transform);
            instance.SetActive(false);
            pool.Enqueue(instance);
        }
    }

    //�Լ� ȣ�� �� �ִٸ� ��Ȱ��ȭ�� text��ȯ �� Ǯ���� ���� ���ٸ� ����
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

    //�Լ� ȣ�� �� text��Ȱ��ȭ ��Ű�� pooling
    public void ReturnDamageText(GameObject damageText)
    {
        damageText.SetActive(false);
        pool.Enqueue(damageText);
    }
}
