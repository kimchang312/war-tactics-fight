using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ExplainItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject ItemToolTip;

    private const int MaxUpgradeLevel = 5;

    private static readonly Dictionary<int, string> gradeText = new()
    {
        { 0, "저주" },
        { 1, "일반" },
        { 10, "전설" },
        { 20, "보스" },
        { 50, "고유" },
    };

    private static readonly string[] upgradeBranchTextIds =
    {
        "SPEARMAN",
        "WARRIOR",
        "ARCHER",
        "HEAVY_INFANTRY",
        "ASSASSIN",
        "LIGHT_CALVARY",
        "HEAVY_CALVARY",
        "SUPPORTER"
    };

    [Header("Tooltip Bounds")]
    private Vector2 tooltipOffset = new Vector2(20f, -100f);
    private float tooltipMaxWidth = 580f;
    private float tooltipScreenPadding = 20f;
    private float tooltipMaxHeightRatio = 0.85f;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!TryBuildTooltipText(out string tooltipText))
        {
            HideToolTip();
            return;
        }

        if (ItemToolTip == null)
            ItemToolTip = GameManager.Instance != null ? GameManager.Instance.itemToolTip : null;

        if (ItemToolTip == null)
            return;

        TextMeshProUGUI textComponent = ItemToolTip.GetComponentInChildren<TextMeshProUGUI>(true);
        if (textComponent == null)
            return;

        RectTransform tooltipRect = ItemToolTip.GetComponent<RectTransform>();
        if (tooltipRect == null)
            return;

        Canvas canvas = tooltipRect.GetComponentInParent<Canvas>();
        if (canvas == null)
            return;

        RectTransform canvasRect = canvas.transform as RectTransform;
        if (canvasRect == null)
            return;

        ItemToolTip.SetActive(true);
        ItemToolTip.transform.SetAsLastSibling();

        textComponent.enableWordWrapping = true;
        textComponent.overflowMode = TextOverflowModes.Ellipsis;
        textComponent.text = tooltipText;

        RefreshTooltipLayout(textComponent, tooltipRect);

        LimitTooltipSizeToCanvas(textComponent, tooltipRect, canvasRect);

        RefreshTooltipLayout(textComponent, tooltipRect);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            canvas.worldCamera,
            out Vector2 mousePosition
        );

        Vector2 desiredPosition = mousePosition + tooltipOffset;
        Vector2 tooltipSize = tooltipRect.rect.size;

        tooltipRect.anchoredPosition = ClampTooltipPosition(
            desiredPosition,
            tooltipSize,
            tooltipRect.pivot,
            canvasRect.rect.size
        );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideToolTip();
    }
    // 사용처: 툴팁 텍스트 변경 후 실제 UI 크기를 즉시 계산
    private static void RefreshTooltipLayout(TextMeshProUGUI textComponent, RectTransform tooltipRect)
    {
        textComponent.ForceMeshUpdate();
        LayoutRebuilder.ForceRebuildLayoutImmediate(textComponent.rectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
        Canvas.ForceUpdateCanvases();
    }

    // 사용처: 설명이 길어져도 툴팁 크기가 화면보다 커지지 않도록 제한
    private void LimitTooltipSizeToCanvas(TextMeshProUGUI textComponent, RectTransform tooltipRect, RectTransform canvasRect)
    {
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        float maxWidth = Mathf.Min(tooltipMaxWidth, canvasWidth - tooltipScreenPadding * 2f);
        float maxHeight = canvasHeight * tooltipMaxHeightRatio;

        maxWidth = Mathf.Max(100f, maxWidth);
        maxHeight = Mathf.Max(100f, maxHeight);

        Vector2 currentSize = tooltipRect.rect.size;

        if (currentSize.x > maxWidth)
            tooltipRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);

        if (currentSize.y > maxHeight)
            tooltipRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maxHeight);

        RectTransform textRect = textComponent.rectTransform;
        Vector2 limitedSize = tooltipRect.rect.size;

        if (textRect.rect.width > limitedSize.x)
            textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, limitedSize.x);

        if (textRect.rect.height > limitedSize.y)
            textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, limitedSize.y);
    }

    // 사용처: 툴팁 위치가 캔버스 밖으로 나가지 않도록 보정
    private Vector2 ClampTooltipPosition(Vector2 desiredPosition, Vector2 tooltipSize, Vector2 pivot, Vector2 canvasSize)
    {
        float halfCanvasWidth = canvasSize.x * 0.5f;
        float halfCanvasHeight = canvasSize.y * 0.5f;

        float minX = -halfCanvasWidth + tooltipScreenPadding + tooltipSize.x * pivot.x;
        float maxX = halfCanvasWidth - tooltipScreenPadding - tooltipSize.x * (1f - pivot.x);

        float minY = -halfCanvasHeight + tooltipScreenPadding + tooltipSize.y * pivot.y;
        float maxY = halfCanvasHeight - tooltipScreenPadding - tooltipSize.y * (1f - pivot.y);

        if (minX <= maxX)
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        else
            desiredPosition.x = 0f;

        if (minY <= maxY)
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        else
            desiredPosition.y = 0f;

        return desiredPosition;
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
            if (data.upgradeTooltipMode == UpgradeTooltipMode.BranchSummary)
                return TryBuildUpgradeBranchSummaryTooltip(data, out tooltipText);

            return TryBuildUpgradeTooltip(data, out tooltipText);
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

    // 사용처: 전술 개량 단계 UI에서 현재 효과와 다음 단계 효과를 툴팁으로 표시
    private static bool TryBuildUpgradeTooltip(ItemInfoData data, out string tooltipText)
    {
        tooltipText = string.Empty;

        int branchIdx = data.upgradeBranchIdx;
        bool isAttack = data.upgradeIsAttack;

        if (branchIdx < 0 && !TryResolveLegacyUpgradeId(data.upgradeId, out branchIdx, out isAttack))
            return false;

        if (branchIdx < 0 || branchIdx >= upgradeBranchTextIds.Length)
            return false;

        UnitUpgrade[] upgrades = RogueLikeData.Instance.GetUpgradeValue();
        if (upgrades == null || branchIdx >= upgrades.Length || upgrades[branchIdx] == null)
            return false;

        int currentLevel = isAttack ? upgrades[branchIdx].attackLevel : upgrades[branchIdx].defenseLevel;
        currentLevel = Mathf.Clamp(currentLevel, 0, MaxUpgradeLevel);

        string branchName = GameTextDB.GetByForeignKey(TextKind.Branch, branchIdx);
        if (string.IsNullOrWhiteSpace(branchName))
            branchName = GetFallbackBranchName(branchIdx);

        string upgradeTypeName = isAttack ? "공격 강화" : "방어 강화";
        string title = $"{branchName} {upgradeTypeName}";

        string noneText = GameTextDB.GetById("UI_NONE", "없음");

        string currentName = currentLevel > 0
            ? GameTextDB.GetById(GetUpgradeNameTextId(branchIdx, isAttack, currentLevel), string.Empty)
            : string.Empty;

        string currentEffect = currentLevel > 0
            ? GameTextDB.GetById(GetUpgradeTooltipTextId(branchIdx, isAttack, currentLevel), string.Empty)
            : noneText;

        StringBuilder sb = new StringBuilder(128);
        sb.Append(title);
        sb.Append('\n');
        sb.Append("현재 단계: ");
        sb.Append(currentLevel);
        sb.Append('/');
        sb.Append(MaxUpgradeLevel);

        if (!string.IsNullOrWhiteSpace(currentName))
        {
            sb.Append('\n');
            sb.Append("현재 전술: ");
            sb.Append(currentName);
        }

        if (!string.IsNullOrWhiteSpace(currentEffect))
        {
            sb.Append('\n');
            sb.Append("현재 효과: ");
            sb.Append(currentEffect);
        }

        if (currentLevel < MaxUpgradeLevel)
        {
            int nextLevel = currentLevel + 1;
            string nextName = GameTextDB.GetById(GetUpgradeNameTextId(branchIdx, isAttack, nextLevel), string.Empty);
            string nextEffect = GameTextDB.GetById(GetUpgradeTooltipTextId(branchIdx, isAttack, nextLevel), string.Empty);

            if (!string.IsNullOrWhiteSpace(nextName))
            {
                sb.Append('\n');
                sb.Append("다음 전술: ");
                sb.Append(nextName);
            }

            if (!string.IsNullOrWhiteSpace(nextEffect))
            {
                sb.Append('\n');
                sb.Append("다음 효과: ");
                sb.Append(nextEffect);
            }
        }

        tooltipText = sb.ToString();
        return !string.IsNullOrWhiteSpace(tooltipText);
    }

    // 사용처: 기존 upgradeId만 들어오던 호출을 새 branch/isAttack 구조로 변환
    private static bool TryResolveLegacyUpgradeId(int upgradeId, out int branchIdx, out bool isAttack)
    {
        branchIdx = -1;
        isAttack = true;

        if (upgradeId < 0)
            return false;

        if (upgradeId < 16)
        {
            branchIdx = upgradeId / 2;
            isAttack = (upgradeId % 2) == 0;
            return branchIdx >= 0 && branchIdx < upgradeBranchTextIds.Length;
        }

        int legacyBranch = upgradeId % 180;
        if (legacyBranch >= 0 && legacyBranch < upgradeBranchTextIds.Length)
        {
            branchIdx = legacyBranch;
            isAttack = true;
            return true;
        }

        return false;
    }

    // 사용처: GameTextDB의 병종 텍스트가 없을 때 전술 개량 툴팁의 제목을 최소한으로 표시
    private static string GetFallbackBranchName(int branchIdx)
    {
        switch (branchIdx)
        {
            case 0: return "창병";
            case 1: return "전사";
            case 2: return "궁병";
            case 3: return "중보병";
            case 4: return "암살자";
            case 5: return "경기병";
            case 6: return "중기병";
            case 7: return "지원";
            default: return "알 수 없음";
        }
    }

    // 사용처: 전술 개량 명칭의 GameTextDB ID를 생성
    private static string GetUpgradeNameTextId(int branchIdx, bool isAttack, int level)
    {
        return $"TI_{upgradeBranchTextIds[branchIdx]}_{(isAttack ? "ATTACK" : "DEFENSE")}_{level}";
    }

    // 사용처: 전술 개량 효과 설명의 GameTextDB ID를 생성
    private static string GetUpgradeTooltipTextId(int branchIdx, bool isAttack, int level)
    {
        return $"TI_{upgradeBranchTextIds[branchIdx]}_{(isAttack ? "ATTACK" : "DEFENSE")}_TOOLTIP_{level}";
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

    // 사용처: 좌측 전술 개량 병종 아이콘에 현재 공격/방어 강화 상태를 요약 표시
    private static bool TryBuildUpgradeBranchSummaryTooltip(ItemInfoData data, out string tooltipText)
    {
        tooltipText = string.Empty;

        int branchIdx = data.upgradeBranchIdx;
        if (branchIdx < 0 || branchIdx >= upgradeBranchTextIds.Length)
            return false;

        UnitUpgrade[] upgrades = RogueLikeData.Instance.GetUpgradeValue();
        if (upgrades == null || branchIdx >= upgrades.Length || upgrades[branchIdx] == null)
            return false;

        int attackLevel = Mathf.Clamp(upgrades[branchIdx].attackLevel, 0, MaxUpgradeLevel);
        int defenseLevel = Mathf.Clamp(upgrades[branchIdx].defenseLevel, 0, MaxUpgradeLevel);

        string branchName = GameTextDB.GetByForeignKey(TextKind.Branch, branchIdx);
        if (string.IsNullOrWhiteSpace(branchName))
            branchName = GetFallbackBranchName(branchIdx);

        string attackText = GetCurrentUpgradeValueText(branchIdx, true, attackLevel);
        string defenseText = GetCurrentUpgradeValueText(branchIdx, false, defenseLevel);

        tooltipText =
            $"{branchName}\n" +
            $"공격강화 : {attackText}\n" +
            $"방어강화 : {defenseText}";

        return true;
    }

    // 사용처: 현재 강화 단계에 맞는 전술 개량 수치 텍스트를 GameTextDB에서 조회
    private static string GetCurrentUpgradeValueText(int branchIdx, bool isAttack, int level)
    {
        if (level <= 0)
            return "0단계 / 없음";

        string textId = GetUpgradeTooltipTextId(branchIdx, isAttack, level);
        string valueText = GameTextDB.GetById(textId, string.Empty);

        if (string.IsNullOrWhiteSpace(valueText))
            return $"{level}단계";

        return $"{level}단계 / {valueText}";
    }

}
