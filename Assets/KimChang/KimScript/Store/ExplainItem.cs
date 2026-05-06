using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ExplainItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject ItemToolTip;

    private static readonly Dictionary<int, string> gradeText = new()
    {
        { 0, "저주" },
        { 1, "일반" },
        { 10, "전설" },
        { 20, "보스" },
        { 50, "고유" },
    };

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!TryBuildTooltipText(out string tooltipText))
        {
            HideToolTip();
            return;
        }

        if (ItemToolTip == null)
            ItemToolTip = GameManager.Instance.itemToolTip;

        if (ItemToolTip == null)
            return;

        TextMeshProUGUI textComponent = ItemToolTip.GetComponentInChildren<TextMeshProUGUI>(true);
        if (textComponent == null)
            return;

        textComponent.text = tooltipText;

        RectTransform tooltipRect = ItemToolTip.GetComponent<RectTransform>();
        if (tooltipRect == null)
            return;

        Canvas canvas = tooltipRect.GetComponentInParent<Canvas>();
        if (canvas == null)
            return;

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
        if (canvasRect == null)
            return;

        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        float minX = -canvasWidth / 2 + tooltipSize.x / 2;
        float maxX = canvasWidth / 2 - tooltipSize.x / 2;
        float minY = -canvasHeight / 2 + tooltipSize.y / 2;
        float maxY = canvasHeight / 2 - tooltipSize.y / 2;

        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);

        tooltipRect.anchoredPosition = desiredPosition;

        Canvas.ForceUpdateCanvases();

        ItemToolTip.SetActive(true);
        ItemToolTip.transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideToolTip();
    }

    // 사용처: 설명 데이터가 없는 오브젝트는 툴팁을 열지 않도록 판정
    private bool TryBuildTooltipText(out string tooltipText)
    {
        tooltipText = string.Empty;

        ItemInformation info = GetComponent<ItemInformation>();
        if (info == null || info.data == null)
            return false;

        ItemInfoData data = info.data;

        if (data.isBuffDeBuff)
        {
            return TryMakeTooltip(data.buffDeBuffName, data.buffDeBuffDescription, out tooltipText);
        }

        if (data.relicId != -1)
        {
            WarRelic relic = WarRelicDatabase.GetRelicById(data.relicId);
            if (relic == null)
                return false;

            string grade = gradeText.TryGetValue(relic.grade, out string gradeName) ? gradeName : string.Empty;
            string title = string.IsNullOrEmpty(grade) ? relic.name : $"{relic.name} {grade}";
            return TryMakeTooltip(title, relic.description, out tooltipText);
        }

        if (data.isItem)
        {
            StoreItemData item = data.item;
            if (item == null)
                return false;

            if (item.itemId >= 0 && item.itemId < 34)
            {
                return TryMakeTooltip(item.itemName, item.description, out tooltipText);
            }

            if (item.itemId > 59 && item.itemId < 63)
            {
                if (string.IsNullOrEmpty(item.value))
                    return false;

                tooltipText = $"주사위\n리롤을 {int.Parse(item.value)}회 추가한다";
                return true;
            }

            return false;
        }

        if (data.isUpgrade)
        {
            return TryBuildUpgradeTooltip(data.upgradeId, out tooltipText);
        }

        if (data.abilityId != -1)
        {
            int id = data.abilityId;
            string name = GameTextDB.Get(id);
            string description = GameTextDB.GetByTitleKey(TextKind.Ability, id);
            return TryMakeTooltip(name, description, out tooltipText);
        }

        if (data.unitId > -1)
        {
            return false;
        }

        if (data.gameTextId != -1)
        {
            string directText = GameTextDB.Get(data.gameTextId);
            return TryMakeTooltip(directText, string.Empty, out tooltipText);
        }

        return false;
    }

    // 사용처: 전술 개량 툴팁 텍스트 생성
    private static bool TryBuildUpgradeTooltip(int upgradeId, out string tooltipText)
    {
        tooltipText = string.Empty;

        if (upgradeId < 0)
            return false;

        var (name, description, addOne, addTwo) = GameTextData.GetLocalizedTextFull(upgradeId);
        int branch = upgradeId % 180;

        if (branch < 0 || branch > 7)
            return false;

        string branchName = GameTextDB.GetByForeignKey(TextKind.Branch, branch);
        if (string.IsNullOrEmpty(branchName))
            return false;

        UnitUpgrade[] upgrades = RogueLikeData.Instance.GetUpgradeValue();
        if (upgrades == null || branch >= upgrades.Length)
            return false;

        int attackValue = upgrades[branch].attackLevel * 10;
        int defenseValue = upgrades[branch].defenseLevel * 10;

        string attackFull = upgrades[branch].attackLevel > 4 ? description : string.Empty;
        string defenseFull = upgrades[branch].defenseLevel > 4 ? addTwo : string.Empty;

        tooltipText = $"{branchName}의 추가 능력치\n{name} +{attackValue}%          {addOne} +{defenseValue}%\n{attackFull}           {defenseFull}";
        return !string.IsNullOrWhiteSpace(tooltipText);
    }

    // 사용처: 제목/설명 중 실제 출력할 내용이 있을 때만 툴팁 문장 구성
    private static bool TryMakeTooltip(string title, string description, out string tooltipText)
    {
        tooltipText = string.Empty;

        bool hasTitle = !string.IsNullOrWhiteSpace(title);
        bool hasDescription = !string.IsNullOrWhiteSpace(description);

        if (!hasTitle && !hasDescription)
            return false;

        if (hasTitle && hasDescription && title != description)
        {
            tooltipText = $"{title}\n{description}";
            return true;
        }

        tooltipText = hasDescription ? description : title;
        return !string.IsNullOrWhiteSpace(tooltipText);
    }

    // 사용처: 마우스가 설명 없는 대상에 올라갔거나 벗어났을 때 툴팁 UI 비활성화
    private void HideToolTip()
    {
        if (ItemToolTip == null)
            ItemToolTip = GameManager.Instance != null ? GameManager.Instance.itemToolTip : null;

        if (ItemToolTip != null)
            ItemToolTip.SetActive(false);
    }
}
