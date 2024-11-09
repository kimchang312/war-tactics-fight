using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; } // �̱��� �ν��Ͻ�

    private Dictionary<UnitDataBase, int> purchasedUnits = new Dictionary<UnitDataBase, int>();

    public string faction;                // �÷��̾ ������ ����
    public string difficulty;             // �÷��̾ ������ ���̵�
    public int enemyFunds;                // ���̵��� ���� ���� �ڱ�
    
    public static int currency = 3000;    // �÷��̾� �ڱ� (static���� ����)

    private void Awake()
    {
        // �̱��� �ν��Ͻ� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �� ��ȯ �ÿ��� ����
            
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // �ʱ�ȭ �� �ʿ��� ���� ����
        faction = "�⺻ ����";  // �⺻ ���� �Ǵ� ���õ� ����
        difficulty = "�⺻ ���̵�"; // �⺻ ���̵� �Ǵ� ���õ� ���̵�
        enemyFunds = CalculateEnemyFunds(difficulty); // ���̵��� ���� �ڱ� ����
    }

    private int CalculateEnemyFunds(string difficulty)
    {
        // ���̵��� ���� �ڱ� ���
        return difficulty switch
        {
            "����" => 2500,
            "����" => 3500,
            "�����" => 5000,
            "����" => 6000,
            _ => 2500 // ������ ��쿡 ���� �⺻��  ����� ����
        };
    }
    // ������ �����ϰ� ����Ʈ�� �߰�
    public void AddPurchasedUnit(UnitDataBase unit)
    {
        if (purchasedUnits.ContainsKey(unit))
        {
            purchasedUnits[unit]++;
        }
        else
        {
            purchasedUnits[unit] = 1;
        }
    }
    // Ư�� ������ �Ǹ��Ͽ� �ڱ� ȯ�� �� ���� ����
    public void SellUnit(UnitDataBase unit)
    {
        if (purchasedUnits.ContainsKey(unit) && purchasedUnits[unit] > 0)
        {
            currency += unit.unitPrice;
            purchasedUnits[unit]--;

            if (purchasedUnits[unit] == 0)
            {
                purchasedUnits.Remove(unit);
            }
        }
        else
        {
            Debug.LogWarning("�Ǹ��� ������ �����ϴ�.");
        }
    }
    // Ư�� ������ ������ ������
    public int GetUnitCount(UnitDataBase unit)
    {
        return purchasedUnits.ContainsKey(unit) ? purchasedUnits[unit] : 0;
    }

    // �÷��̾��� ��� ���� ����� ��ȯ
    public Dictionary<UnitDataBase, int> GetAllPurchasedUnits()
    {
        return new Dictionary<UnitDataBase, int>(purchasedUnits);
    }
}