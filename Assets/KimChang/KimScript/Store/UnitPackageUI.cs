using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 사용처: 상점 패키지(유닛 묶음) UI – 호버로 펼치고 클릭으로 열기/닫기
public class UnitPackageUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private StoreUI storeUI;                 // 사용처: 상점 상위 UI 연동
    [SerializeField] private Vector2 originPos;               // 사용처: 패키지 기본 위치(닫힐 때 돌아갈 위치)
    [SerializeField] private Transform unitBox;               // 사용처: 유닛 슬롯 부모
    [SerializeField] private TextMeshProUGUI packageName;     // 사용처: 패키지명 표시
    [SerializeField] private TextMeshProUGUI packagePrice;    // 사용처: 가격 표시

    // 데이터
    private ItemInfoData itemInfo = new();                    // 사용처: 가격/상품정보 캐시
    private List<RogueUnitDataBase> units = new();            // 사용처: 이 패키지에 포함된 유닛 목록

    // 캐시
    private RectTransform rect;
    private readonly List<RectTransform> activeChildren = new(8); // 사용처: 현재 활성 유닛 슬롯 RT 캐시
    private readonly List<Button> childButtons = new(8);          // 사용처: 유닛 슬롯 버튼 캐시

    // 상태
    private static UnitPackageUI currentHover;                // 사용처: 호버 소유자(다른 패키지 접기용)
    private bool isAnimating = false;                         // 사용처: 어떤 트윈이라도 수행 중일 때
    private bool isPackageOpened = false;                     // 사용처: 클릭으로 펼쳐진 상태 여부
    private int hoverRefCount = 0;                            // 사용처: 자식 단위 호버 누적 카운트
    private bool canBuyCached = false;                        // 사용처: 구매 가능 여부 캐시(클릭 가드에 사용)
    private bool hoverLocked = false;
    // 상수(애니/배치)
    private static readonly Vector2 CenterPos = new(0, 120);
    private const float AniTime = 0.5f;
    private const float HoverOffsetX = 150f;
    private const float ClickOffsetX = 365f;
    private const float ScreenMargin = 50f;
    private const float ExpandThresholdX = 720f;
    // 사용처: 현재 호버/접힘 상태 전환 트윈 추적
    private Tween stateTween;
    private void Awake()
    {
        rect = (RectTransform)transform;
        if (storeUI == null) storeUI = FindObjectOfType<StoreUI>(true);

        childButtons.Clear();

        for (int i = 0; i < unitBox.childCount; i++)
        {
            var tr = unitBox.GetChild(i);

            var relay = tr.GetComponent<ChildHoverRelay>();
            if (relay == null) relay = tr.gameObject.AddComponent<ChildHoverRelay>();
            relay.Init(this);

            Button btn = tr.GetComponent<Button>();
            if (btn == null) btn = tr.GetComponentInChildren<Button>(true);
            if (btn != null) childButtons.Add(btn);
        }
    }

    private void OnDisable()
    {
        if (currentHover == this) currentHover = null;
        hoverRefCount = 0;
        isAnimating = false;
    }

    // 사용처: 상점에서 패키지 데이터 세팅 시 호출
    public void SetUnitPackage(List<RogueUnitDataBase> _units, StoreItemData storeItem, int price)
    {
        if (rect != null) rect.anchoredPosition = originPos;
        ResetChildrenAnchorsToZero();

        units = _units;
        itemInfo.item = storeItem;
        itemInfo.price = price;

        packagePrice.text = itemInfo.price.ToString();
        packageName.gameObject.SetActive(true);
        packagePrice.gameObject.SetActive(true);

        // 사용처: 패키지명은 현재 화면상 앞에 보이는 유닛 이름 기준으로 갱신
        UpdatePackageNameByFrontUnit();


        packagePrice.text = itemInfo.price.ToString();
        packageName.gameObject.SetActive(true);
        packagePrice.gameObject.SetActive(true);

        int slotCount = unitBox.childCount;
        int unitCount = units.Count;
        for (int i = 0; i < slotCount; i++)
        {
            var child = unitBox.GetChild(i).gameObject;
            if (i < unitCount)
            {
                child.SetActive(true);
                var ui = child.GetComponent<OneUnitUI>();
                if (ui != null)
                {
                    ui.SetOneUnit(units[i]);
                    ui.SetDisableEnergyName(); // 사용처: 클릭 전까지 이름/에너지 숨김
                }
            }
            else child.SetActive(false);
        }

        // 버튼 재수집 생략 ...
        isPackageOpened = false;
        hoverLocked = false;
        hoverRefCount = 0;

        ToggleUnitDetailLabels(false); // 안전망(활성 슬롯 모두 숨김 유지)
        UpdateUnitPackage();
    }

    // 사용처: 골드 등 조건 변경 시 버튼 활성/리스너 갱신
    public void UpdateUnitPackage()
    {
        canBuyCached = RogueLikeData.Instance.CanSpendGold(itemInfo.price);

        for (int i = 0; i < childButtons.Count; i++)
        {
            var btn = childButtons[i];
            if (btn == null) continue;

            btn.interactable = btn.gameObject.activeSelf;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnUnitClick(btn));
        }
    }

    // 사용처: 자식 중 첫 진입 시 패키지 호버 시작
    internal void OnPackageHoverBegin()
    {
        if (hoverLocked || isPackageOpened) return;

        if (currentHover != null && currentHover != this && !currentHover.isPackageOpened)
        {
            currentHover.hoverRefCount = 0;
            currentHover.CollapseUnits(force: true);
        }

        currentHover = this;
        transform.SetAsLastSibling();
        SpreadUnits(force: true);
    }

    // 사용처: 호버 해제 시 패키지 접기
    internal void OnPackageHoverEnd()
    {
        if (hoverLocked || isPackageOpened) return;

        CollapseUnits(force: true);

        if (currentHover == this)
            currentHover = null;
    }

    // 사용처: 호버 펼침(위치 기준 단방향/대칭)
    private void SpreadUnits(bool force)
    {
        if (!force && isAnimating) return;

        isAnimating = true;

        KillAllAnimations();
        CollectActiveChildren();

        int n = activeChildren.Count;
        if (n == 0)
        {
            isAnimating = false;
            return;
        }

        float anchorX = rect != null ? rect.anchoredPosition.x : ((RectTransform)transform).anchoredPosition.x;
        float halfWidth = GetCanvasHalfWidth();

        if (anchorX > ExpandThresholdX)
        {
            for (int i = 0; i < n; i++)
            {
                int offsetIndex = n - 1 - i;
                float x = -HoverOffsetX * offsetIndex;
                x = Mathf.Clamp(x, -halfWidth + ScreenMargin, halfWidth - ScreenMargin);
                activeChildren[i].DOAnchorPos(new Vector2(x, 0f), AniTime).SetEase(Ease.OutCubic);
            }
        }
        else if (anchorX < -ExpandThresholdX)
        {
            for (int i = 0; i < n; i++)
            {
                int offsetIndex = n - 1 - i;
                float x = HoverOffsetX * offsetIndex;
                x = Mathf.Clamp(x, -halfWidth + ScreenMargin, halfWidth - ScreenMargin);
                activeChildren[i].DOAnchorPos(new Vector2(x, 0f), AniTime).SetEase(Ease.OutCubic);
            }
        }
        else
        {
            float startX = HoverOffsetX * (n - 1) * 0.5f;
            for (int i = 0; i < n; i++)
            {
                float x = startX - HoverOffsetX * i;
                x = Mathf.Clamp(x, -halfWidth + ScreenMargin, halfWidth - ScreenMargin);
                activeChildren[i].DOAnchorPos(new Vector2(x, 0f), AniTime).SetEase(Ease.OutCubic);
            }
        }

        stateTween = DOVirtual.DelayedCall(AniTime, () =>
        {
            isAnimating = false;
        }).SetTarget(this);
    }

    // 사용처: 호버 접기
    private void CollapseUnits(bool force)
    {
        if (!force && isAnimating) return;

        isAnimating = true;

        KillAllAnimations();
        CollectActiveChildren();

        if (activeChildren.Count == 0)
        {
            isAnimating = false;
            return;
        }

        for (int i = 0; i < activeChildren.Count; i++)
        {
            activeChildren[i].DOAnchorPos(Vector2.zero, AniTime).SetEase(Ease.OutCubic);
        }

        stateTween = DOVirtual.DelayedCall(AniTime, () =>
        {
            isAnimating = false;
        }).SetTarget(this);
    }
    // 사용처: 유닛 클릭(패키지 열기 트리거)
    private void OnUnitClick(Button btn)
    {
        if (isPackageOpened) return;

        // 호버 종료 및 호버 잠금
        hoverLocked = true;
        KillAllAnimations();     // 진행 중인 호버 트윈 즉시 중단
        isAnimating = true;      // 열기 전까지 입력/호버 억제
        hoverRefCount = 0;
        if (currentHover == this) currentHover = null;

        CollectActiveChildren();
        if (activeChildren.Count == 0) { isAnimating = false; return; }

        float collapseDur = 0.15f;
        var seq = DOTween.Sequence();
        for (int i = 0; i < activeChildren.Count; i++)
            seq.Join(activeChildren[i].DOAnchorPos(Vector2.zero, collapseDur).SetEase(Ease.OutCubic));
        seq.OnComplete(() => ClickUnitPackage());
    }


    // 사용처: 클릭 펼침(좌우로 벌리며 중앙 이동)
    private void ClickUnitPackage()
    {
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

        float startX = ClickOffsetX * (n - 1) * 0.5f;

        var seq = DOTween.Sequence();
        seq.Append(rect.DOAnchorPos(CenterPos, AniTime).SetEase(Ease.OutCubic));
        seq.AppendCallback(() =>
        {
            for (int i = 0; i < n; i++)
            {
                Vector2 targetPos = new(startX - ClickOffsetX * i, 0f);
                activeChildren[i].DOAnchorPos(targetPos, AniTime).SetEase(Ease.OutCubic);
            }
            ToggleUnitDetailLabels(true); // 사용처: 펼침 시 이름/에너지 보이기
        });
        seq.OnComplete(() => { isAnimating = false; });
    }

    public void ReturnUnitPackage()
    {
        if (isAnimating) return;
        isAnimating = true;

        KillAllAnimations();

        if (rect == null) { isAnimating = false; return; }

        CollectActiveChildren();
        if (activeChildren.Count == 0) { isAnimating = false; return; }
        ToggleUnitDetailLabels(false);
        var seq = DOTween.Sequence();

        for (int i = 0; i < activeChildren.Count; i++)
            seq.Join(activeChildren[i].DOAnchorPos(Vector2.zero, AniTime).SetEase(Ease.OutCubic));

        seq.Append(rect.DOAnchorPos(originPos, AniTime).SetEase(Ease.OutCubic))
           .OnComplete(() =>
           {

               packageName.gameObject.SetActive(true);
               packagePrice.gameObject.SetActive(true);

               isAnimating = false;
               isPackageOpened = false;
               hoverLocked = false;
               currentHover = null;
               hoverRefCount = 0;
               storeUI.VisiblePurchaseLeaveBtn();
           });
    }

    // 사용처: 유닛 카드의 이름/에너지 레이블 토글(접힘/펼침 전환)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ToggleUnitDetailLabels(bool show)
    {
        int total = unitBox.childCount;
        for (int i = 0; i < total; i++)
        {
            var t = unitBox.GetChild(i);
            if (!t.gameObject.activeSelf) continue;

            var ui = t.GetComponent<OneUnitUI>();
            if (ui == null) continue;

            if (show) ui.SetAbleEnergyName();
            else ui.SetDisableEnergyName();
        }
    }

    // 사용처: 현재 활성 유닛 RectTransform 수집
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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


    // 사용처: 관련 트윈 중단
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void KillAllAnimations()
    {
        if (stateTween != null && stateTween.IsActive())
            stateTween.Kill();

        DOTween.Kill(this);
        DOTween.Kill(rect);

        int total = unitBox.childCount;
        for (int i = 0; i < total; i++)
        {
            if (unitBox.GetChild(i) is RectTransform rt)
                DOTween.Kill(rt);
        }
    }

    // 사용처: 자식 앵커 위치 0으로 리셋(초기화)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ResetChildrenAnchorsToZero()
    {
        if (unitBox == null) return;

        for (int i = 0; i < unitBox.childCount; i++)
        {
            if (unitBox.GetChild(i) is RectTransform r)
                r.anchoredPosition = Vector2.zero;
        }
    }

    // 사용처: 화면 이탈 방지 기준(좌/우 여백 고려)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float GetCanvasHalfWidth()
    {
        var canvas = GetComponentInParent<Canvas>();
        return canvas != null ? canvas.pixelRect.width * 0.5f : Screen.width * 0.5f;
    }

    private sealed class ChildHoverRelay :
        MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        private UnitPackageUI owner;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init(UnitPackageUI ui) => owner = ui;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (owner == null || owner.hoverLocked || owner.isPackageOpened) return;
            if (owner.hoverRefCount == 0) owner.OnPackageHoverBegin();
            owner.hoverRefCount++;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (owner == null || owner.hoverLocked || owner.isPackageOpened) return;
            if (owner.hoverRefCount <= 0) return;
            owner.hoverRefCount--;
            if (owner.hoverRefCount == 0) owner.OnPackageHoverEnd();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (owner == null) return;
            owner.OnUnitClick(null);
        }
    }
    // 사용처: 접힌 상태에서 레이어상 가장 앞에 보이는 유닛 이름 조회
    private string GetFrontVisibleUnitName()
    {
        if (unitBox == null) return string.Empty;

        for (int i = unitBox.childCount - 1; i >= 0; i--)
        {
            Transform child = unitBox.GetChild(i);

            if (!child.gameObject.activeSelf)
                continue;

            OneUnitUI oneUnitUI = child.GetComponent<OneUnitUI>();
            if (oneUnitUI == null || oneUnitUI.unit == null)
                continue;

            if (!string.IsNullOrEmpty(oneUnitUI.unit.unitName))
                return oneUnitUI.unit.unitName;
        }

        return string.Empty;
    }

    // 사용처: 패키지 이름 텍스트를 레이어상 가장 앞에 보이는 유닛 이름으로 갱신
    private void UpdatePackageNameByFrontUnit()
    {
        string frontName = GetFrontVisibleUnitName();

        if (!string.IsNullOrEmpty(frontName))
            packageName.text = frontName;
        else
            packageName.text = itemInfo.item != null ? itemInfo.item.itemName : string.Empty;
    }
}
