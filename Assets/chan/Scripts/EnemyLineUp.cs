using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyLineupManager : MonoBehaviour
{
    [Header("�� ����Ʈ ǥ��")]
    [SerializeField] private Transform enemyListParent; // �� ����Ʈ�� ǥ���� �θ� ������Ʈ
    [SerializeField] private GameObject enemyUnitPrefab; // ���� ������ ǥ���� ������

    [Header("����")]
    [SerializeField] private int maxUnits = 20; // �ִ� ���� ��
    private int enemyFunds; // �� ���� (PlayerData���� ������)

    
    public void Start()
    {
        GenerateEnemyLineup(); // �� ���� ����
        //DisplayEnemyLineupUI(); // ������ ������ UI�� ǥ��
    }

    public void GenerateEnemyLineup()
    {
        Debug.Log("GenerateEnemyLineup ȣ���");
        //ExcludeRandomBranches(); // ������ ���� ����
        Debug.Log("������ ���� ���� �Ϸ�");
        //PurchaseUnits(); // �� ���� ���� ���� ȣ��
        Debug.Log("�� ���� ���� �Ϸ�");
        List<UnitDataBase> purchasedUnits = UnitDataManager.Instance.GetAllUnits();

        if (purchasedUnits == null || purchasedUnits.Count == 0)
        {
            Debug.LogError("���� �����Ͱ� �����ϴ�.");
            return;
        }

        List<UnitDataBase> enemyUnits = PlaceUnits(purchasedUnits);

        // �� ���� ����Ʈ�� PlayerData�� ����
        PlayerData.Instance.SetEnemyUnits(enemyUnits);

        // ������ �� ������ ����׷� Ȯ��
        Debug.Log("�� ���� ���ξ�:");
        foreach (var unit in enemyUnits)
        {
            Debug.Log($"����: {unit.unitName}");
        }
    }

    private List<string> ExcludeRandomBranches(List<string> branches)
    {
        // Bowman�� �׻� ����
        branches.Remove("Bowman");

        // �������� 2���� ���� ����
        for (int i = 0; i < 2; i++)
        {
            if (branches.Count == 0) break;

            int randomIndex = Random.Range(0, branches.Count);
            Debug.Log($"���ܵ� ����: {branches[randomIndex]}");
            branches.RemoveAt(randomIndex);
        }

        // Bowman �ٽ� �߰�
        branches.Add("Bowman");
        return branches;
    }

    private List<UnitDataBase> PurchaseUnits(List<UnitDataBase> allUnits, List<string> branches)
    {
        List<UnitDataBase> purchasedUnits = new List<UnitDataBase>();
        int fundsPerUnit = enemyFunds / maxUnits;

        // �� ���� ���� ���� �� ������ ����
        bool prioritizeHighTier = Random.value > 0.5f;
        Debug.Log(prioritizeHighTier ? "���� ����: ��� ���� �켱��" : "���� ����: ������");

        foreach (string branch in branches)
        {
            List<UnitDataBase> branchUnits = allUnits.Where(u => u.unitBranch == branch).ToList();
            int remainingFunds = enemyFunds / branches.Count;

            while (remainingFunds > 0 && purchasedUnits.Count < maxUnits)
            {
                // ���� ���͸� (���/���� ���� �켱 ����)
                List<UnitDataBase> availableUnits = prioritizeHighTier
                    ? branchUnits.OrderByDescending(u => u.unitPrice).ToList()
                    : branchUnits.OrderBy(u => u.unitPrice).ToList();

                // ���� ������ ���� ����
                UnitDataBase unit = availableUnits.FirstOrDefault(u => u.unitPrice <= remainingFunds);
                if (unit == null) break;

                purchasedUnits.Add(unit);
                remainingFunds -= unit.unitPrice;
            }
        }

        return purchasedUnits;
    }

    private List<UnitDataBase> PlaceUnits(List<UnitDataBase> purchasedUnits)
    {
        List<UnitDataBase> finalLineup = new List<UnitDataBase>();

        // ����� ���� (����)
        List<UnitDataBase> defensiveUnits = purchasedUnits
            .Where(u => u.unitBranch == "Heavy_Infantry" || u.unitBranch == "Heavy_Cavalry")
            .ToList();
        finalLineup.AddRange(defensiveUnits.Take(5)); // �ִ� 5����� ������ ��ġ

        // ������ ���� (����/�߰���)
        List<UnitDataBase> meleeUnits = purchasedUnits
            .Where(u => u.unitBranch == "Spearman" || u.unitBranch == "Warrior")
            .ToList();
        finalLineup.AddRange(meleeUnits.Take(5)); // �߰� 5����� ��ġ


        // �Ŀ� ���� (��Ÿ� 2 �̻�)
        List<UnitDataBase> rangedUnits = purchasedUnits
            .Where(u => u.range >= 2)
            .ToList();
        finalLineup.AddRange(rangedUnits.Take(3)); // �ִ� 3����� �Ŀ��� ��ġ

        // ������ ���� ������ ��ġ
        foreach (var unit in purchasedUnits)
        {
            if (!finalLineup.Contains(unit))
            {
                finalLineup.Add(unit);
            }

            if (finalLineup.Count >= maxUnits) break;
        }

        return finalLineup;
    }

    private void DisplayEnemyLineupUI(List<UnitDataBase> enemyLineup)
    {
        // ������ ������ ����Ʈ ����
        foreach (Transform child in enemyListParent)
        {
            Destroy(child.gameObject);
        }

        // �� ����Ʈ ����
        foreach (UnitDataBase unit in enemyLineup)
        {
            GameObject unitUI = Instantiate(enemyUnitPrefab, enemyListParent);
            Image unitImage = unitUI.transform.Find("UnitImage").GetComponent<Image>();
            TextMeshProUGUI unitName = unitUI.transform.Find("UnitName").GetComponent<TextMeshProUGUI>();

            unitImage.sprite = Resources.Load<Sprite>($"UnitImages/{unit.unitImg}");
            unitName.text = unit.unitName;
        }
    }
}