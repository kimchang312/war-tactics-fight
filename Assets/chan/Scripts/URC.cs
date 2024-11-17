using UnityEngine;
using UnityEngine.EventSystems;

public class URC : MonoBehaviour, IPointerClickHandler
{
    private UnitDataBase unitData; // �� ������ ������

    [SerializeField] private UnitDetailUI unitDetailUI; // ������ Inspector���� ���� ����

    // unitDetailUI�� �ܺο��� �а� ������ �� �ִ� �Ӽ�
    public UnitDetailUI UnitDetailUI
    {
        get => unitDetailUI;
        set => unitDetailUI = value;
    }



    private void Start()
    {
        // unitDetailUI�� null�� ��츸 FindObjectOfType�� �˻�
        if (unitDetailUI == null)
        {
            unitDetailUI = FindObjectOfType<UnitDetailUI>();
            if (unitDetailUI == null)
            {
                Debug.LogError("UnitDetailUI�� ã�� �� �����ϴ�. ���� �߰��Ǿ����� Ȯ���ϼ���.");
            }
        }
    }

    public void SetUnitData(UnitDataBase data)
    {
        unitData = data;
    }
    // IPointerClickHandler �������̽� ����
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right) // ��Ŭ�� ����
        {
            if (unitData != null && unitDetailUI != null)
            {
                unitDetailUI.ShowUnitDetails(unitData); // ���� ���� ǥ��
            }
            else
            {
                Debug.LogWarning("UnitData �Ǵ� UnitDetailUI�� �������� �ʾҽ��ϴ�!");
            }
        }
    }
}
