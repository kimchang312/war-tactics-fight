using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitPackageUI : MonoBehaviour
{
    [SerializeField] private StoreUI storeUI;
    [SerializeField] private Vector2 originPos;
    [SerializeField] private Transform unitBox;
    [SerializeField] private TextMeshProUGUI packageName;
    [SerializeField] private TextMeshProUGUI packagePrice;

    private ItemInfoData itemInfo = new();
    private List<RogueUnitDataBase> units = new();
    private RectTransform rect;

    private static readonly Vector2 CenterPos = new Vector2(0, 120);
    private const float AniTime = 0.5f;
    private const float HoverOffsetX = 100f;
    private const float ClickOffsetX = 365f;
    private const float ScreenMargin = 50f;

    // 상태
    private bool isAnimating = false;     // 사용처: 어떤 트윈이라도 수행 중일 때 true
    private bool isPackageOpened = false; // 사용처: 클릭 펼침으로 열린 상태일 때 true
    private int hoverRefCount = 0;        // 사용처: 자식 단위의 호버 누적 카운트

    // 사용처: 다른 패키지로 호버 이동 시 이전 패키지를 접기 위한 전역 호버 소유자
    private static UnitPackageUI currentHover;

    // 캐시
    private readonly List<RectTransform> activeChildren = new(8);
    private readonly List<Button> childButtons = new(8);

    private void Awake()
    {
        rect = transform as RectTransform;
        if (storeUI == null) storeUI = FindObjectOfType<StoreUI>(true);

        childButtons.Clear();

        // 사용처: 유닛 자식에 호버/클릭 릴레이 구성
        for (int i = 0; i < unitBox.childCount; i++)
        {
            var tr = unitBox.GetChild(i);

            // 호버 릴레이
            var relay = tr.GetComponent<ChildHoverRelay>();
            if (relay == null) relay = tr.gameObject.AddComponent<ChildHoverRelay>();
            relay.Init(this);

            // 버튼 캐시
            var btn = tr.GetComponent<Button>();
            if (btn != null) childButtons.Add(btn);

            // 클릭 보강: 버튼이 막혀도 동작하도록 PointerClick 트리거 추가
            var trigger = tr.GetComponent<EventTrigger>();
            if (trigger == null) trigger = tr.gameObject.AddComponent<EventTrigger>();

            var clickEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            // 사용처: 자식 클릭 → 부모 클릭 로직 호출
            clickEntry.callback.AddListener(_ => OnUnitClick(btn));
            trigger.triggers.Add(clickEntry);
        }
    }

    private void OnDisable()
    {
        if (currentHover == this) currentHover = null;
        hoverRefCount = 0;
    }

    // 사용처: 상점에서 데이터 세팅 시 호출
    public void SetUnitPackage(List<RogueUnitDataBase> _units, StoreItemData storeItem, int price)
    {
        if (rect != null) rect.anchoredPosition = originPos;

        // 초기 위치 리셋
        for (int i = 0; i < transform.childCount; i++)
            if (transform.GetChild(i) is RectTransform r) r.anchoredPosition = Vector2.zero;

        units = _units;
        itemInfo.item = storeItem;
        itemInfo.price = price;
  
        packageName.text = itemInfo.item.itemName;
        packagePrice.text = itemInfo.price.ToString();
        packageName.gameObject.SetActive(true);
        packagePrice.gameObject.SetActive(true);

        // 유닛 슬롯 세팅
        int slotCount = unitBox.childCount;
        int unitCount = units.Count;
        for (int i = 0; i < slotCount; i++)
        {
            var child = unitBox.GetChild(i).gameObject;
            if (i < unitCount)
            {
                child.SetActive(true);
                child.GetComponent<OneUnitUI>().SetOneUnit(units[i]);
            }
            else
            {
                child.SetActive(false);
            }
        }

        isPackageOpened = false;
        hoverRefCount = 0;
        UpdateUnitPackage();
    }

    // 사용처: 자식 중 첫 진입 시(hoverRefCount 0->1) 패키지 호버 시작
    private void OnPackageHoverBegin()
    {
        if (isPackageOpened) return;

        // 다른 패키지가 호버 중이면 강제 접기
        if (currentHover != null && currentHover != this && !currentHover.isPackageOpened)
        {
            currentHover.hoverRefCount = 0;
            currentHover.CollapseUnits(force: true);
        }

        currentHover = this;
        transform.SetAsLastSibling();
        SpreadUnits(force: true);
    }

    // 사용처: 마지막 이탈 시(hoverRefCount 1->0) 패키지 호버 종료
    private void OnPackageHoverEnd()
    {
        if (isPackageOpened) return;
        CollapseUnits(force: true);
        if (currentHover == this) currentHover = null;
    }

    // 사용처: 호버 펼침(방향 반전 적용)
    private void SpreadUnits(bool force)
    {
        if (!force && isAnimating) return;
        isAnimating = true;

        KillAllAnimations();
        CollectActiveChildren();
        int n = activeChildren.Count;
        if (n == 0) { isAnimating = false; return; }

        // 반전: 오른쪽에서 왼쪽으로
        float startX = HoverOffsetX * (n - 1) * 0.5f;
        float halfWidth = GetCanvasHalfWidth();

        for (int i = 0; i < n; i++)
        {
            float x = startX - HoverOffsetX * i;
            x = Mathf.Clamp(x, -halfWidth + ScreenMargin, halfWidth - ScreenMargin);
            activeChildren[i].DOAnchorPos(new Vector2(x, 0f), AniTime).SetEase(Ease.OutCubic);
        }

        DOVirtual.DelayedCall(AniTime, () => isAnimating = false);
    }

    // 사용처: 호버 접기
    private void CollapseUnits(bool force)
    {
        if (!force && isAnimating) return;

        isAnimating = true;
        KillAllAnimations();
        CollectActiveChildren();
        if (activeChildren.Count == 0) { isAnimating = false; return; }

        for (int i = 0; i < activeChildren.Count; i++)
            activeChildren[i].DOAnchorPos(Vector2.zero, AniTime).SetEase(Ease.OutCubic);

        DOVirtual.DelayedCall(AniTime, () => isAnimating = false);
    }

    // 사용처: 유닛 클릭(언제든 클릭 보장)
    private void OnUnitClick(Button btn)
    {
        if (isPackageOpened) return;

        // 경합 제거
        KillAllAnimations();

        hoverRefCount = 0;
        if (currentHover == this) currentHover = null;

        CollectActiveChildren();
        if (activeChildren.Count == 0) return;

        // 먼저 모으기(짧게)
        float collapseDur = 0.25f;
        var seq = DOTween.Sequence();
        for (int i = 0; i < activeChildren.Count; i++)
            seq.Join(activeChildren[i].DOAnchorPos(Vector2.zero, collapseDur).SetEase(Ease.OutCubic));

        seq.OnComplete(() => ClickUnitPackage(btn));
    }

    // 사용처: 클릭 펼침(방향 반전 적용)
    private void ClickUnitPackage(Button clickedBtn)
    {
        isAnimating = true;
        isPackageOpened = true;

        KillAllAnimations();

        storeUI.AnimatePackageBackTrue();
        transform.SetAsLastSibling();
        storeUI.ClickUnitPackage(this, units, itemInfo.price);

        packageName.gameObject.SetActive(false);
        packagePrice.gameObject.SetActive(false);

        if (rect == null) { isAnimating = false; return; }

        CollectActiveChildren();
        int n = activeChildren.Count;
        if (n == 0) { isAnimating = false; return; }

        // 반전: 오른쪽에서 왼쪽으로
        float startX = ClickOffsetX * (n - 1) * 0.5f;

        var seq = DOTween.Sequence();
        seq.Append(rect.DOAnchorPos(CenterPos, AniTime).SetEase(Ease.OutCubic));
        seq.AppendCallback(() =>
        {
            for (int i = 0; i < n; i++)
            {
                Vector2 targetPos = new Vector2(startX - ClickOffsetX * i, 0f);
                activeChildren[i].DOAnchorPos(targetPos, AniTime).SetEase(Ease.OutCubic);
                var ui = activeChildren[i].GetComponent<OneUnitUI>();
                if (ui != null) ui.SetAbleUI();
            }
        });
        seq.OnComplete(() => isAnimating = false);
    }

    // 사용처: 외부에서 패키지를 닫을 때 호출
    public void ReturnUnitPackage()
    {
        if (isAnimating) return;
        isAnimating = true;

        KillAllAnimations();

        if (rect == null) { isAnimating = false; return; }

        CollectActiveChildren();
        if (activeChildren.Count == 0) { isAnimating = false; return; }

        var seq = DOTween.Sequence();

        for (int i = 0; i < activeChildren.Count; i++)
        {
            var ui = activeChildren[i].GetComponent<OneUnitUI>();
            if (ui != null) ui.SetDisableUI();
            seq.Join(activeChildren[i].DOAnchorPos(Vector2.zero, AniTime).SetEase(Ease.OutCubic));
        }

        seq.Append(rect.DOAnchorPos(originPos, AniTime).SetEase(Ease.OutCubic))
           .OnComplete(() =>
           {
               packageName.gameObject.SetActive(true);
               packagePrice.gameObject.SetActive(true);
               isAnimating = false;
               isPackageOpened = false;
               currentHover = null;
               hoverRefCount = 0;
               storeUI.VisiblePurchaseLeaveBtn();
           });
    }

    // 사용처: 골드 등 조건 변경 시 버튼 활성/리스너 세팅
    public void UpdateUnitPackage()
    {
        int gold = RogueLikeData.Instance.GetCurrentGold();
        bool canBuy = gold >= itemInfo.price;

        SetChildButtonsInteractable(canBuy);

        if (canBuy)
        {
            for (int i = 0; i < childButtons.Count; i++)
            {
                var btn = childButtons[i];
                if (btn == null || !btn.gameObject.activeSelf) continue;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnUnitClick(btn));
            }
        }
        else
        {
            for (int i = 0; i < childButtons.Count; i++)
            {
                var btn = childButtons[i];
                if (btn != null) btn.onClick.RemoveAllListeners();
            }
        }
    }

    // 사용처: 현재 활성 유닛 RectTransform 수집
    private void CollectActiveChildren()
    {
        activeChildren.Clear();
        int total = unitBox.childCount;
        for (int i = 0; i < total; i++)
        {
            var t = unitBox.GetChild(i);
            if (!t.gameObject.activeSelf) continue;
            if (t is RectTransform rt) activeChildren.Add(rt);
        }
    }

    // 사용처: 버튼 상호작용 설정
    private void SetChildButtonsInteractable(bool enabled)
    {
        for (int i = 0; i < childButtons.Count; i++)
        {
            var btn = childButtons[i];
            if (btn != null) btn.interactable = enabled && btn.gameObject.activeSelf;
        }
    }

    // 사용처: 관련 트윈 중단
    private void KillAllAnimations()
    {
        DOTween.Kill(rect);
        int total = unitBox.childCount;
        for (int i = 0; i < total; i++)
        {
            if (unitBox.GetChild(i) is RectTransform rt)
                DOTween.Kill(rt);
        }
    }

    // 사용처: 화면 이탈 방지 기준
    private float GetCanvasHalfWidth()
    {
        var canvas = GetComponentInParent<Canvas>();
        return canvas != null ? canvas.pixelRect.width * 0.5f : Screen.width * 0.5f;
    }

    // 사용처: 유닛 자식에서 포인터 이벤트를 부모로 안정 전달
    private sealed class ChildHoverRelay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private UnitPackageUI owner;

        public void Init(UnitPackageUI ui) => owner = ui;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (owner == null || owner.isPackageOpened) return;
            if (owner.hoverRefCount == 0) owner.OnPackageHoverBegin();
            owner.hoverRefCount++;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (owner == null || owner.isPackageOpened) return;
            if (owner.hoverRefCount <= 0) return;
            owner.hoverRefCount--;
            if (owner.hoverRefCount == 0) owner.OnPackageHoverEnd();
        }
    }
}
