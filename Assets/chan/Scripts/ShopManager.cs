using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    public GameObject unitPrefab;           // ���� Prefab
    public Transform content;               // ������ ǥ�õ� ��ġ (ScrollView�� Content)
    public TextMeshProUGUI currencyText;    // ���� �ڱ��� ǥ���� Text
    public PlayerData playerData;           // PlayerData�� ���� �ڱ� �� ������ ���� Ȯ��


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ���� �����͸� �ε��ϴ� ������ ���� DisplayUnits ȣ��
    private async void Start()
    {   
        Debug.Log("ShopManager Start()");
        

        // �����Ͱ� �ε�Ǿ����� Ȯ���ϰ�, �ε�� �� DisplayUnits ȣ��
        if (UnitDataManager.Instance != null && UnitDataManager.Instance.unitDataList.Count == 0)
        {
            await UnitDataManager.Instance.LoadUnitDataAsync(); // �񵿱������� �����͸� �ε�
            DisplayUnits(); // �����͸� �ε��� �� ������ ǥ��
        }
        
        else
        {
            Debug.LogWarning("UnitDataManager�� �������� �ʽ��ϴ�.");
        }
    }
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
        }
        else
        {
            Debug.LogWarning("���� �����Ͱ� ��� �ֽ��ϴ�.");
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
}