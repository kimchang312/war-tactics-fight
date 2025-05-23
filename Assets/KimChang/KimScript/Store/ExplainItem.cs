using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ExplainItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject ItemToolTip;
    [SerializeField] private GameObject unitPackageToolTip;
    private static readonly Dictionary<int, string> gradeText = new() 
    {
        {0,"저주" },
        {1,"일반" },
        {10,"전설" },
        {20,"보스" },
        {50,"고유" },
    };

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

        RectTransform tooltipRect = ItemToolTip.GetComponent<RectTransform>();
        Canvas canvas = tooltipRect.GetComponentInParent<Canvas>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.worldCamera,
            out var mousePosition
        );

        Vector2 offset = new Vector2(110, -110);
        Vector2 desiredPosition = mousePosition + offset;

        // 툴팁의 크기와 캔버스 크기 가져오기
        Vector2 tooltipSize = tooltipRect.sizeDelta;
        RectTransform canvasRect = canvas.transform as RectTransform;

        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        // 제한 영역 계산
        float minX = -canvasWidth / 2 + tooltipSize.x / 2;
        float maxX = canvasWidth / 2 - tooltipSize.x / 2;
        float minY = -canvasHeight / 2 + tooltipSize.y / 2;
        float maxY = canvasHeight / 2 - tooltipSize.y / 2;

        // 위치 조정
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);

        tooltipRect.anchoredPosition = desiredPosition;


        TextMeshProUGUI textComponent = ItemToolTip.transform.GetChild(ItemToolTip.transform.childCount - 1)
            .GetComponent<TextMeshProUGUI>();

        textComponent.text = "설정되지 않은 아이템";

        if (info.relicId != -1)
        {
            var relic = WarRelicDatabase.GetRelicById(info.relicId);
            if (relic != null)
                textComponent.text = $"{relic.name}+ ${gradeText[relic.grade]}\n{relic.tooltip}";
            else
                textComponent.text = "유산 정보를 찾을 수 없습니다.";
        }
        else if (info.isItem)
        {
            if (item.itemId >= 0 && item.itemId < 34)
            {
                textComponent.text = $"{item.itemName}\n{item.description}";
            }
            else if (item.itemId > 59 && item.itemId < 63)
            {
                textComponent.text = $"주사위\n리롤을 {int.Parse(item.value)}회 추가한다";
            }
        }
        else
        {
            if (info.abilityId != -1)
            {
                var (name, description) = GameTextData.GetLocalizedText(info.abilityId);
                textComponent.text = $"{name}\n{description}";
            }
            else if(info.unitId > -1)
            {
                return;
            }
        }

        ItemToolTip.SetActive(true);
        unitPackageToolTip.SetActive(false);
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
