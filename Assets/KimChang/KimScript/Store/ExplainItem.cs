using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ExplainItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject ItemToolTip;
    //[SerializeField] private GameObject unitPackageToolTip;
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
            ItemToolTip = GameManager.Instance.itemToolTip;
        }/*
        if (unitPackageToolTip == null)
        {
            unitPackageToolTip = FindInactiveObject("UnitPackageToolTip");
        }*/
        ItemInformation info = GetComponent<ItemInformation>();
        if (info == null)
        {
            Debug.LogWarning("ItemInformation 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        StoreItemData item = info.data.item;
        /*
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

        }*/
        TextMeshProUGUI textComponent = ItemToolTip.GetComponentInChildren<TextMeshProUGUI>();

        textComponent.text = "설정되지 않은 아이템";

        if (info.data.relicId != -1)
        {
            var relic = WarRelicDatabase.GetRelicById(info.data.relicId);
            if (relic != null)
                textComponent.text = $"{relic.name} {gradeText[relic.grade]}\n{relic.tooltip}";
            else
                textComponent.text = "유산 정보를 찾을 수 없습니다.";
        }
        else if (info.data.isItem)
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
        else if (info.data.isUpgrade)
        {
            var (name, description, addOne, addTwo) = GameTextData.GetLocalizedTextFull(info.data.upgradeId);
            int branch = info.data.upgradeId % 180;
            string branchName = branch switch
            {
                0 => "창병",
                1 => "전사",
                2 => "궁병",
                3 => "중보병",
                4 => "암살자",
                5 => "경기병",
                6 => "중기병",
                7 => "지원",
                _ => "",
            };
            UnitUpgrade[] upgrades = RogueLikeData.Instance.GetUpgradeValue();
            int attackValue = upgrades[branch].attackLevel * 10;
            int defenseValue = upgrades[branch].defenseLevel * 10;
            string attackFull = "";
            string defenseFull = "";
            if (upgrades[branch].attackLevel > 4) attackFull = description;
            if (upgrades[branch].defenseLevel > 4) defenseFull = addTwo;
            textComponent.text = $"{branchName}의 추가 능력치\n{name} +{attackValue}%          {addOne} +{defenseValue}%\n{attackFull}           {defenseFull}";
        }
        else
        {
            if (info.data.abilityId != -1)
            {
                var (name, description) = GameTextData.GetLocalizedText(info.data.abilityId);
                textComponent.text = $"{name}\n{description}";
            }
            else if (info.data.unitId > -1)
            {
                return;
            }
        }
        RectTransform tooltipRect = ItemToolTip.GetComponent<RectTransform>();
        Canvas canvas = tooltipRect.GetComponentInParent<Canvas>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.worldCamera,
            out var mousePosition
        );

        Vector2 offset = new Vector2(20, -100);
        Vector2 desiredPosition = mousePosition + offset;
        
        Vector2 tooltipSize = tooltipRect.sizeDelta;
        RectTransform canvasRect = canvas.transform as RectTransform;

        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        float minX = -canvasWidth / 2 + tooltipSize.x / 2;
        float maxX = canvasWidth / 2 - tooltipSize.x / 2;
        float minY = -canvasHeight / 2 + tooltipSize.y / 2;
        float maxY = canvasHeight / 2 - tooltipSize.y / 2;

        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);

        tooltipRect.anchoredPosition = desiredPosition;

        Canvas.ForceUpdateCanvases(); // ← 꼭 추가!

        ItemToolTip.SetActive(true);
        //unitPackageToolTip.SetActive(false);
        ItemToolTip.transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemToolTip.SetActive(false);
        //unitPackageToolTip.SetActive(false);
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
