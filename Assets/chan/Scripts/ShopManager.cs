using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    public GameObject emptyUnitPrefab;      // ���̾ƿ� �ڸ������� ���� �� ������
    public GameObject unitPrefab;           // ���� Prefab
    public Transform content;               // ������ ǥ�õ� ��ġ (ScrollView�� Content)
    public TextMeshProUGUI currencyText;    // ���� �ڱ��� ǥ���� Text
    public TextMeshProUGUI factionText;     // �÷��̾��� ������ ǥ���� Text
    public PlayerData playerData;           // PlayerData�� ���� �ڱ� �� ������ ���� Ȯ��
    public UnitDataManager unitDataManager; // UnitDataManager�� ���� ���ֵ����� �ε��ϱ� ����



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ���� �����͸� �ε��ϴ� ������ ���� DisplayUnits ȣ��
    // ���� ���� �� �����͸� �̸� �ε��ϰ� ���� ȭ���� �غ�
    private async void Start()
    {

        // PlayerData �̱��� �ν��Ͻ��� ����
        if (PlayerData.Instance != null)
        {
            playerData = PlayerData.Instance;
            Debug.Log("PlayerData ���� �Ϸ�: " + playerData.faction);
        }
        else
        {
            Debug.LogError("PlayerData �̱��� �ν��Ͻ��� �������� �ʽ��ϴ�.");
        }
        Debug.Log("ShopManager Start()");

        // ���� �����͸� �̸� �ε��Ͽ� ���� ����
        /*if (unitDataManager != null/* && unitDataManager.unitDataList.Count == 0)
        {
            unitDataManager = UnitDataManager.Instance;
            await unitDataManager.LoadUnitDataAsync(); // �񵿱������� �����͸� �̸� �ε�
            if (unitDataManager.unitDataList.Count > 0)
            {
                DisplayUnits(); // ���� �����Ͱ� �ε�� �� ������ ǥ��
            }*/
        // UnitDataManager �ν��Ͻ� ����
        while (UnitDataManager.Instance == null)
        {
            await Task.Yield();  // �񵿱� ���
        }
        unitDataManager = UnitDataManager.Instance;
        Debug.Log("UnitDataManager ���� �Ϸ�");

        await unitDataManager.LoadUnitDataAsync();

        if (unitDataManager.unitDataList.Count > 0)
        {
            DisplayUnits();
        }
        else
        {
            Debug.LogWarning("���� �����Ͱ� ��� �ֽ��ϴ�.");
        }
// �ε��� �Ϸ�� �� �ٷ� ���� ȭ���� ǥ��

    UpdateCurrencyDisplay(); // �ڱ� UI ������Ʈ
    FactionDisplay();// ���� UI ������Ʈ
    }

    

    /* �����Ͱ� �ε�Ǿ����� Ȯ���ϰ�, �ε�� �� DisplayUnits ȣ��
    if (UnitDataManager.Instance != null && UnitDataManager.Instance.unitDataList.Count == 0)
    {
        UnitDataManager = UnitDataManager.Instance;

        await UnitDataManager.Instance.LoadUnitDataAsync(); // �񵿱������� �����͸� �ε�

        DisplayUnits(); // �����͸� �ε��� �� ������ ǥ��
    }

    else
    {
        Debug.LogWarning("UnitDataManager�� �������� �ʽ��ϴ�.");
    }
    //�ڱ� �� ���� UI ������Ʈ
    FactionDisplay();
    UpdateCurrencyDisplay();*/
    
    // ���� �����͸� UI�� ǥ��
    public void DisplayUnits()
    {
        var units = UnitDataManager.Instance.GetAllUnits(); // UnitDataManager���� ���� ����Ʈ ��������
        if (units != null && units.Count > 0)
        {
            foreach (var unit in units)
            {
                GameObject unitObject = Instantiate(unitPrefab, content);
                UnitUI unitUI = unitObject.GetComponent<UnitUI>(); // ���� ������ ǥ���� UI ������Ʈ

                if (unitUI != null)
                {
                    unitUI.SetUnitData(unit); // ���� ������ UI�� ����
                    
                }
                else
                {
                    Debug.LogWarning("UnitUI ������Ʈ�� ã�� �� �����ϴ�.");
                }
            }
            // ���� �߰� �� �� ���� 5�� �߰�
            AddEmptyUnits();
        }
        else
        {
            Debug.LogWarning("���� �����Ͱ� ��� �ֽ��ϴ�.");
        }
    }

    // �� ���� 5�� �߰� (���̾ƿ� �ڸ���)
    private void AddEmptyUnits()
    {
        for (int i = 0; i < 5; i++)
        {
            // �� ���� �������� Content�� �߰�
            Instantiate(emptyUnitPrefab, content);
        }
    }
    // ���� ���� ��ư Ŭ��
    public void BuyUnit(UnitDataBase unit)
        {
        // �ڱ��� ������� Ȯ��
        if (PlayerData.currency >= unit.unitPrice) // PlayerData.Instance�� �ڱ� Ȯ��
        {
            // �ڱ� ����
            PlayerData.currency -= unit.unitPrice;

            // ���� ���� ��Ͽ� �߰�
            PlayerData.Instance.AddPurchasedUnit(unit); // PlayerData.Instance�� ���� ���� �߰�

            // UI ������Ʈ
            UpdateCurrencyDisplay();
            
        }
        else
        {
            Debug.Log("�ڱ��� �����մϴ�.");
        }
        }

        // �ڱ� ������Ʈ UI ǥ��
        private void UpdateCurrencyDisplay()
        {
        currencyText.text =  PlayerData.currency.ToString()+"G";
        }
        private void FactionDisplay()
        { 
           factionText.text = "���� : "+ playerData.faction.ToString();
        }
    
}