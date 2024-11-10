using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; } // �̱��� �ν��Ͻ�

    private Dictionary<UnitDataBase, int> purchasedUnits = new Dictionary<UnitDataBase, int>();
    private List<UnitDataBase> placedUnits = new List<UnitDataBase>(); // ��ġ�� ���� ���. ���� ���� ���� �ʿ��� ���·� �����ؾ���.
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
    // ���̵��� ���� �ڱ� ���
    private int CalculateEnemyFunds(string difficulty)
    {
        
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

        // �� ���� ���� 20�� �ʰ��ϴ��� Ȯ��
        int totalUnitCount = GetTotalUnitCount();

        if (totalUnitCount >= 20)
        {
            Debug.LogWarning("���� ���� 20���� �ʰ��� �� �����ϴ�.");
            return; // ���� �߰��� ����
        }

        if (purchasedUnits.ContainsKey(unit))
        {
            purchasedUnits[unit]++;
        }
        else
        {
            purchasedUnits[unit] = 1;
        }
        // ����� �α� �߰�
        Debug.Log($"{unit.unitName}��(��) �����߽��ϴ�. ���� �� ���� ��: {totalUnitCount + 1}");
    }

    // ��� ������ �� ���� ����ϴ� �޼���
    public int GetTotalUnitCount()
    {
        int totalCount = 0;
        foreach (var unit in purchasedUnits)
        {
            totalCount += unit.Value; // ������ ������ ����
        }
        return totalCount;
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

            // �ڱ� ���°� ����Ǹ� ShopManager���� UI ������Ʈ ȣ��
            ShopManager.Instance.UpdateUIState();

            //UI���� ���� ������ ������Ʈ�ϰų� ���� -------------���� �����ؾ���
            /*MyUnitUI myUnitUI = FindUnitUI(unit);
            if (myUnitUI != null)
            {
                myUnitUI.UpdateUnitCount();
                if (myUnitUI.GetUnitCount() == 0) // ���� ���� 0�̸� ����
                {
                    Destroy(myUnitUI.gameObject);
                }
            }*/
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
    // �÷��̾� ������ �ʱ�ȭ
    public void ResetPlayerData()
    {
        // ���� ��� �ʱ�ȭ
        purchasedUnits.Clear();

        // �ڱ� �ʱ�ȭ
        currency = 3000;

        // ��Ÿ �ʱ�ȭ�� �����Ͱ� �ִٸ� ���⿡ �߰�
        faction = "�⺻ ����";
        difficulty = "�⺻ ���̵�";
        enemyFunds = CalculateEnemyFunds(difficulty); // ���̵��� ���� �ڱ� �ʱ�ȭ
    }

    
    
    // ��ġ�� ���� ��Ͽ� ������ �߰��ϴ� �޼���
    public void AddPlacedUnit(UnitDataBase unit)
    {
        placedUnits.Add(unit);
        Debug.Log($"��ġ�� ���� �߰�: {unit.unitName}");
    }
    // ��ġ�� ���� ����� Ȯ��
    public void ShowPlacedUnitList()
    {
        Debug.Log("��ġ�� ���� ���:");
        foreach (var unit in placedUnits)
        {
            Debug.Log($"���� �̸�: {unit.unitName}");
        }
    }
}