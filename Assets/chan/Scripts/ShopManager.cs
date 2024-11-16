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
    public GameObject linePrefab;           // ��ĭ�̹��� Prefab
    public Transform content;               // ������ ǥ�õ� ��ġ (ScrollView�� Content)
    public GameObject MyUnitPrefab;         // MyUnit ������
    public Transform myUnitUIcontent;       // MyUnit UI ��ġ
    public TextMeshProUGUI currencyText;    // ���� �ڱ��� ǥ���� Text
    public TextMeshProUGUI factionText;     // �÷��̾��� ������ ǥ���� Text
    public PlayerData playerData;           // PlayerData�� ���� �ڱ� �� ������ ���� Ȯ��
    public UnitDataManager unitDataManager; // UnitDataManager�� ���� ���ֵ����� �ε��ϱ� ����
    public Button placeButton; // ��ġ��ư = ��ġBTN
    public GameObject FundsWarning; // �ڱ� ���� ���

    private List<MyUnitUI> myUnitUIList = new List<MyUnitUI>(); // MyUnitUI ����Ʈ

    [SerializeField] private Transform unitPlacementArea;  // ���� ��ġ�� UI ����
    [SerializeField] private GameObject placeunitPrefab;   // ��ġ�� ���� ������
    [SerializeField] private Image currencyTextImg;
    public bool isPlacingUnits = false;                   // ��ġ ��� Ȯ��
    private GameObject lineObject; // ��ĭ�� �� ���� ������ ��ü
    public bool IsPlacingUnits => isPlacingUnits;
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
        placeButton.onClick.AddListener(TogglePlacingUnits);

        // ��ġ UI�� ��ĭ�� ���� 1���� ����
        lineObject = Instantiate(linePrefab, unitPlacementArea);
        Debug.Log("lineObject parent: " + lineObject.transform.parent.name);


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
                myUnitUIList.Add(myUnitUI);
            }
            
        }
    }
    // Ư�� ���ֿ� �ش��ϴ� MyUnitUI ã��
    private MyUnitUI FindMyUnitUI(UnitDataBase unit)
    {
        return myUnitUIList.Find(ui => ui.UnitData == unit);
    }

    // Ư�� ������ UI ������Ʈ
    public void UpdateUnitCountForUnit(UnitDataBase unit)
    {
        MyUnitUI myUnitUI = FindMyUnitUI(unit);
        if (myUnitUI != null)
        {
            myUnitUI.UpdateUnitCount();
        }
        else
        {
            Debug.LogWarning($"[UpdateUnitCountForUnit] {unit.unitName}�� �ش��ϴ� UI�� ã�� �� �����ϴ�.");
        }
    }

    // Ư�� ������ UI ����
    public void RemoveMyUnitUI(UnitDataBase unit)
    {
        MyUnitUI myUnitUI = FindMyUnitUI(unit);
        if (myUnitUI != null)
        {
            myUnitUIList.Remove(myUnitUI);
            Destroy(myUnitUI.gameObject); // UI ����
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
            ChangeBackgroundColor("#f4cccc"); //���ڵ�� �÷��̾� ��� ���� ����

        }
        else
        {
            ShowFundsWarning(false);
            DisablePlaceButton(false);
            ChangeBackgroundColor("#ffffFF"); //���ڵ�� �÷��̾� ��� ���� ����
        }
    }


    // ���� Ŭ���� ȣ��Ǵ� �޼���
    public void OnUnitClicked(UnitDataBase unit)
    {
        PlaceUnit(unit);  // ���� ��ġ
    }
    // ���õ� ������ ��ġ�ϴ� �޼���
    private void PlaceUnit(UnitDataBase unit)
    {
        // ������ �̹����� �̸��� ������ ���ο� UI �������� ����
        GameObject placeunitObject = Instantiate(placeunitPrefab, unitPlacementArea);
        
        // PlacedUnit ��ũ��Ʈ ������Ʈ�� ������
        PlacedUnit placedUnit = placeunitObject.GetComponent<PlacedUnit>();

        
        // ���� ������ ����
        placedUnit.SetUnitData(unit);  // PlacedUnit�� SetUnitData �޼��忡�� ���� �����Ϳ� UI ������Ʈ

        // ��ġ �� �ش� ������ PlayerData�� ��ġ�� ���� ����Ʈ�� �߰�
        PlayerData.Instance.AddPlacedUnit(unit);  // ������ ��ġ�� ���� ��Ͽ� �߰�

        // ��ġ �� ��ĭ�� ���������� �̵�
        MoveLineToLast();
        Debug.Log(lineObject.transform.GetSiblingIndex());

        Debug.Log($"[PlaceUnit] ���� {unit.unitName} ��ġ �Ϸ�. ��ĭ ��ġ ������Ʈ.");
        Debug.Log($"[PlaceUnit] ��ġ�� ����: {unit.unitName}");
        Debug.Log($"[PlaceUnit] ��ġ �� ���� ����: {PlayerData.Instance.GetUnitCount(unit)}");
        // ��ġ �� �ش� ������ ������ ����
        PlayerData.Instance.SellUnit(unit);
        Debug.Log($"[PlaceUnit] ��ġ �� ���� ����: {PlayerData.Instance.GetUnitCount(unit)}");
        

    }
    // ��ĭ�� ���������� �̵��ϴ� �޼���
    private void MoveLineToLast()
    {
        // lineObject�� ������ �ڽ��� �ǵ��� ������ ����
        int lastIndex = unitPlacementArea.childCount;  // ���� �ڽ��� ������ ������
        lineObject.transform.SetSiblingIndex(lastIndex);  // ������ �ε��� ��ġ�� ����

        // SetAsLastSibling ȣ�� �� ���� Ȯ��
        Debug.Log($"[After SetAsLastSibling] lineObject�� �ε���: {lineObject.transform.GetSiblingIndex()}, �θ� �ڽ� ��: {unitPlacementArea.childCount}");
        
    }
        // ��ġ ��� Ȱ��ȭ / ��Ȱ��ȭ ���
        public void TogglePlacingUnits()
    {
        isPlacingUnits = !isPlacingUnits;
        placeButton.interactable = isPlacingUnits;  // ��ġ ��ư Ȱ��ȭ / ��Ȱ��ȭ
    }
    public void ReturnUnit(UnitDataBase unit)
    {
        PlayerData.Instance.AddPurchasedUnit(unit);
        AddOrUpdateUnitInMyUnitUI(unit);
    }

    // ���ڵ�� ����� �����ϴ� �޼���
    public void ChangeBackgroundColor(string hexColor)
    {
        if (currencyTextImg != null)
        {
            Color newColor;
            if (ColorUtility.TryParseHtmlString(hexColor, out newColor))
            {
                currencyTextImg.color = newColor;
            }
            else
            {
                Debug.LogWarning("��ȿ���� ���� ���ڵ��Դϴ�: " + hexColor);
            }
        }
        else
        {
            Debug.LogWarning("backgroundImage�� �������� �ʾҽ��ϴ�.");
        }
    }
    
}