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
    public GameObject MyUnitPrefab;
    public Transform myUnitUIcontent;       // MyUnit UI ��ġ
    public TextMeshProUGUI currencyText;    // ���� �ڱ��� ǥ���� Text
    public TextMeshProUGUI factionText;     // �÷��̾��� ������ ǥ���� Text
    public PlayerData playerData;           // PlayerData�� ���� �ڱ� �� ������ ���� Ȯ��
    public UnitDataManager unitDataManager; // UnitDataManager�� ���� ���ֵ����� �ε��ϱ� ����
    public Button placeButton; // ��ġ��ư
    public GameObject FundsWarning; // �ڱ� ���� ���


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
    public void BuyUnit(UnitDataBase unit)
    {
        // �ڱ��� �����ص� ���� ���� ����
            PlayerData.currency -= unit.unitPrice; //�ڱ� ���� (������ ������)
            PlayerData.Instance.AddPurchasedUnit(unit);

            UpdateCurrencyDisplay();
            AddOrUpdateUnitInMyUnitUI(unit);

        // �ڱ��� ����� ���ƿ��� ��� �޽��� ����� ��ġ ��ư Ȱ��ȭ
        UpdateUIState();
        // �ڱ� ���� �� ��� ǥ��
        if (PlayerData.currency < 0)
        {
            ShowFundsWarning(true);    // �ڱ� ���� ��� ǥ��
            DisablePlaceButton(true);  // ��ġ ��ư ��Ȱ��ȭ
        }
       
    }

    // �ڱ� ������Ʈ UI ǥ��
    public void UpdateCurrencyDisplay()
        {
        currencyText.text =  PlayerData.currency.ToString()+"G";
        }
        private void FactionDisplay()
        { 
           factionText.text = "���� : "+ playerData.faction.ToString();
        }

    // MyUnit UI�� ���� �߰� �Ǵ� ���� ���� ����
    private void AddOrUpdateUnitInMyUnitUI(UnitDataBase unit)
    {
        bool unitExists = false;

        // �̹� MyUnitUI�� �ش� ������ �ִ��� Ȯ��
        foreach (Transform child in myUnitUIcontent)
        {
            MyUnitUI myUnitUI = child.GetComponent<MyUnitUI>();
            if (myUnitUI != null && myUnitUI.UnitData == unit)
            {
                // ���� ���� ������Ʈ
                myUnitUI.UpdateUnitCount();
                unitExists = true;
                break;
            }
        }

        // MyUnitUI�� �ش� ������ ���ٸ� ���� ����
        if (!unitExists)
        {
            GameObject unitObj = Instantiate(MyUnitPrefab, myUnitUIcontent);
            MyUnitUI myUnitUI = unitObj.GetComponent<MyUnitUI>();
            if (myUnitUI != null)
            {
                myUnitUI.Setup(unit);
            }
        }
    }
    // �ڱ� ���� ��� �޽��� ǥ��/�����
    public void ShowFundsWarning(bool show)
    {
        if (FundsWarning != null)
        {
            FundsWarning.SetActive(show);
        }
    }
    // ��ġ ��ư�� ��Ȱ��ȭ�ϴ� �޼���
    public void DisablePlaceButton(bool disable)
    {
        // ��ġ ��ư ��Ȱ��ȭ
        if (placeButton != null)
        {
            placeButton.interactable = !disable;
        }
    }
    // �ڱ� ���¿� ���� UI ������Ʈ (��� �޽����� ��ġ ��ư ����)
    public void UpdateUIState()
    {
        if (PlayerData.currency < 0)
        {
            ShowFundsWarning(true);
            DisablePlaceButton(true);
        }
        else
        {
            ShowFundsWarning(false);
            DisablePlaceButton(false);
        }
    }
}