using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitPackageUI : MonoBehaviour
{
    [SerializeField] private StoreUI storeUI;
    [SerializeField] Vector2 originPos;
    [SerializeField] private Transform unitBox;
    [SerializeField] private TextMeshProUGUI packageName;
    [SerializeField] private TextMeshProUGUI packagePrice;

    ItemInformation itemInfo = new();
    List<RogueUnitDataBase> units = new();
    RectTransform rect;

    private readonly Vector2 centerPos = new Vector2(0, 120);
    private const float aniTime = 0.5f;

    // 애니메이션 실행 여부 플래그
    private bool isAnimating = false;

    private void Awake()
    {
        rect = transform as RectTransform;
        if (storeUI == null) storeUI = FindObjectOfType<StoreUI>(true);
    }

    // 패키지 데이터 세팅
    public void SetUnitPackage(List<RogueUnitDataBase> _units, StoreItemData storeItem, int price)
    {
        rect = transform as RectTransform;
        if (rect != null)
            rect.anchoredPosition = originPos;

        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform childRect = transform.GetChild(i) as RectTransform;
            if (childRect != null)
                childRect.anchoredPosition = Vector2.zero;
        }

        units = _units;
        itemInfo.item = storeItem;
        itemInfo.price = price;

        packageName.text = itemInfo.item.itemName;
        packagePrice.text = itemInfo.price.ToString();
        packageName.gameObject.SetActive(true);
        packagePrice.gameObject.SetActive(true);

        int unitGroupCount = unitBox.childCount;
        int unitCount = units.Count;
        for (int i = 0; i < unitGroupCount; i++)
        {
            GameObject child = unitBox.GetChild(i).gameObject;

            if (i < unitCount)
            {
                RogueUnitDataBase unit = units[i];
                child.SetActive(true);
                OneUnitUI oneUnitUI = child.GetComponent<OneUnitUI>();
                oneUnitUI.SetOneUnit(unit);
            }
            else
            {
                child.SetActive(false);
            }
        }

        UpdateUnitPackage();
    }

    // 유닛 패키지 펼침
    private void ClickUnitPackage(Button btn)
    {
        if (isAnimating) return;
        isAnimating = true;

        KillAllAnimations();

        UnitPackageUI unitPackageUI = GetComponent<UnitPackageUI>();
        storeUI.AnimatePackageBackTrue();
        transform.SetAsLastSibling();
        storeUI.ClickUnitPackage(unitPackageUI, units, itemInfo.price);

        btn.onClick.RemoveAllListeners();
        packageName.gameObject.SetActive(false);
        packagePrice.gameObject.SetActive(false);

        rect = transform as RectTransform;
        if (rect == null) { isAnimating = false; return; }

        int totalChildCount = unitBox.childCount;
        if (totalChildCount == 0) { isAnimating = false; return; }

        List<RectTransform> activeChildren = new List<RectTransform>();
        for (int i = 0; i < totalChildCount; i++)
        {
            Transform t = unitBox.GetChild(i);
            if (t.gameObject.activeSelf)
            {
                RectTransform rt = t as RectTransform;
                if (rt != null)
                    activeChildren.Add(rt);
            }
        }

        int activeCount = activeChildren.Count;
        if (activeCount == 0) { isAnimating = false; return; }

        float offsetX = 365f;
        float startX = -offsetX * (activeCount - 1) / 2f;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(rect.DOAnchorPos(centerPos, aniTime).SetEase(Ease.OutCubic));

        sequence.AppendCallback(() =>
        {
            for (int i = 0; i < activeCount; i++)
            {
                Vector2 targetPos = new Vector2(startX + offsetX * i, 0f);
                activeChildren[i].DOAnchorPos(targetPos, aniTime).SetEase(Ease.OutCubic);
                activeChildren[i].GetComponent<OneUnitUI>().SetAbleUI();
            }
        });

        sequence.OnComplete(() => isAnimating = false);
    }

    // 유닛 패키지 모음
    public void ReturnUnitPackage()
    {
        if (isAnimating) return;
        isAnimating = true;

        KillAllAnimations();

        rect = transform as RectTransform;
        if (rect == null) { isAnimating = false; return; }

        int totalChildCount = unitBox.childCount;
        if (totalChildCount == 0) { isAnimating = false; return; }

        List<RectTransform> activeChildren = new List<RectTransform>();
        for (int i = 0; i < totalChildCount; i++)
        {
            Transform t = unitBox.GetChild(i);
            if (t.gameObject.activeSelf)
            {
                RectTransform rt = t as RectTransform;
                if (rt != null)
                    activeChildren.Add(rt);
                t.GetComponent<OneUnitUI>().SetDisableUI();
            }
        }

        if (activeChildren.Count == 0) { isAnimating = false; return; }

        Sequence sequence = DOTween.Sequence();

        foreach (var child in activeChildren)
        {
            sequence.Join(child.DOAnchorPos(Vector2.zero, aniTime).SetEase(Ease.OutCubic));
        }

        sequence.Append(rect.DOAnchorPos(originPos, aniTime).SetEase(Ease.OutCubic))
                .OnComplete(() =>
                {
                    packageName.gameObject.SetActive(true);
                    packagePrice.gameObject.SetActive(true);
                    isAnimating = false;
                    storeUI.VisiblePurchaseLeaveBtn();
                });
    }

    // 버튼 활성화 상태 갱신
    public void UpdateUnitPackage()
    {
        int gold = RogueLikeData.Instance.GetCurrentGold();
        if (gold < itemInfo.price)
        {
            ResetChildBtnInteractable(false);
        }
        else
        {
            ResetChildBtnInteractable(true);
            Button btn = GetLastChildBtn();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => ClickUnitPackage(btn));
        }
    }

    private void ResetChildBtnInteractable(bool isInteractable)
    {
        foreach (Transform child in unitBox)
        {
            child.GetComponent<Button>().interactable = isInteractable;
        }
    }

    private Button GetLastChildBtn()
    {
        for (int i = unitBox.childCount - 1; i >= 0; i--)
        {
            Transform child = unitBox.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                Button btn = child.GetComponent<Button>();
                if (btn != null)
                    return btn;
            }
        }
        return null;
    }

    // 모든 관련 DOTween 애니메이션 종료
    private void KillAllAnimations()
    {
        DOTween.Kill(rect);
        foreach (Transform child in unitBox)
        {
            DOTween.Kill(child as RectTransform);
        }
    }
}
