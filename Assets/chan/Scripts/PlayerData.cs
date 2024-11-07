using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; } // �̱��� �ν��Ͻ�

    public string faction;                // �÷��̾ ������ ����
    public string difficulty;             // �÷��̾ ������ ���̵�
    public int enemyFunds;                // ���̵��� ���� ���� �ڱ�
    public List<UnitDataBase> purchasedUnits; // �������� ������ ���� ����Ʈ
    public static int currency = 3000;    // �÷��̾� �ڱ� (static���� ����)

    private void Awake()
    {
        // �̱��� �ν��Ͻ� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �� ��ȯ �ÿ��� ����
            purchasedUnits = new List<UnitDataBase>(); // ������ ���� ����Ʈ �ʱ�ȭ
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

    // �������� ���� ���� �� ���� ������ ����Ʈ�� �߰�
    public void AddPurchasedUnit(UnitDataBase unit)
    {
        purchasedUnits.Add(unit);
    }

}