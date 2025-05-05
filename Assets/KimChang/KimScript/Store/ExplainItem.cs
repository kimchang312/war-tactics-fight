using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ExplainItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject ItemToolTip;
    [SerializeField] private GameObject unitPackageToolTip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ItemToolTip == null)
        {
            ItemToolTip = FindInactiveObject("ItemToolTip");
        }
        if (unitPackageToolTip == null)
        {
            unitPackageToolTip = FindInactiveObject("UnitPackageToolTip");
        }
            ItemInformation info = GetComponent<ItemInformation>();
        if (info == null)
        {
            Debug.LogWarning("ItemInformation 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        StoreItemData item = info.item;

        if (info.units.Count != 0)
        {
            unitPackageToolTip.SetActive(true);
            ItemToolTip.SetActive(false);

            Transform unitPackage = unitPackageToolTip.transform.GetChild(0);
            int totalChildren = unitPackage.childCount;
            int activeCount = info.units.Count;

            for (int i = 0; i < totalChildren; i++)
            {
                GameObject child = unitPackage.GetChild(i).gameObject;

                if (i < activeCount)
                {
                    RogueUnitDataBase unit = info.units[i];
                    child.SetActive(true);
                    UIMaker.CreateSelectUnitEnergy(unit, child);
                }
                else
                {
                    child.SetActive(false);
                }
            }

            unitPackageToolTip.transform.SetAsLastSibling();
            return;

        }

        ItemToolTip.SetActive(true);
        unitPackageToolTip.SetActive(false);

        RectTransform tooltipRect = ItemToolTip.GetComponent<RectTransform>();
        Canvas canvas = tooltipRect.GetComponentInParent<Canvas>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.worldCamera,
            out var mousePosition
        );

        Vector2 offset = new Vector2(110, -110);
        tooltipRect.anchoredPosition = mousePosition + offset;

        TextMeshProUGUI textComponent = ItemToolTip.transform.GetChild(ItemToolTip.transform.childCount - 1)
            .GetComponent<TextMeshProUGUI>();

        if (item == null)
        {
            textComponent.text = "아이템 정보 없음";
        }
        else if (item.itemId >= 0 && item.itemId < 34)
        {
            textComponent.text = $"{item.itemName}\n{item.description}";
        }
        else if (info.relicId != -1)
        {
            var relic = WarRelicDatabase.GetRelicById(info.relicId);
            if (relic != null)
                textComponent.text = $"{relic.name}\n{relic.tooltip}";
            else
                textComponent.text = "유물 정보를 찾을 수 없습니다.";
        }
        else if (item.itemId > 59 && item.itemId < 63)
        {
            textComponent.text = $"주사위\n리롤을 {int.Parse(item.value)}회 추가한다";
        }
        else
        {
            textComponent.text = "설정되지 않은 아이템";
        }
        ItemToolTip.transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemToolTip.SetActive(false);
        unitPackageToolTip.SetActive(false);
    }

    private GameObject FindInactiveObject(string name)
    {
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (var t in allTransforms)
        {
            if (t.name == name && t.gameObject.hideFlags == HideFlags.None)
                return t.gameObject;
        }
        return null;
    }
}
