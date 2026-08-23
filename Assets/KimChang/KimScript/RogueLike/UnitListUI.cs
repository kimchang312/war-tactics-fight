using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UnitListUI : MonoBehaviour
{
    [SerializeField] private Transform unitList;
    [SerializeField] private GameObject buttons;
    [SerializeField] private Button dateOrderBtn;
    [SerializeField] private Button rarityOrderBtn;
    [SerializeField] private Button branchOrderBtn;
    [SerializeField] private Button energyOrderBtn;
    [SerializeField] private Button nameOrderBtn;
    [SerializeField] private Button closeBtn;
    [SerializeField] private GameObject selectUnitObj;
    [SerializeField] private TextMeshProUGUI selectUnitText;
    [SerializeField] private ObjectPool objectPool;

    private readonly List<RogueUnitDataBase> _selectedUnits = new List<RogueUnitDataBase>(8);
    private readonly Dictionary<Button, UnityAction> _unitClickMap = new Dictionary<Button, UnityAction>(64);
    private Action _onSelectAction;
    private List<RogueUnitDataBase> _sourceUnits; // 선택 후보군(null이면 보유 전체)

    // 현재 UI 세션에서 사용하는 정렬 기준 캐시 (닫힐 때 RogueLikeData에 반영)
    private int _unitOrder = -1;
    // 애니메이션 파라미터
    private float openAnimTime = 0.5f;
    private float sortAnimTime = 0.5f;
    private float scrollStartYOffset = -75f;
    [SerializeField, Range(0.05f, 1f)] private float scrollbarHandleSize = 0.18f;
    private readonly Dictionary<int, Button> orderButtonMap = new Dictionary<int, Button>(10);
    private Sequence panelSeq;
    private Sequence listSeq;
    private Scrollbar activeVerticalScrollbar;

    // 페이드 캐시(인스턴스 단위)
    private readonly List<CanvasGroup> cgCache = new List<CanvasGroup>(64);
    private readonly List<Graphic[]> graphicsCache = new List<Graphic[]>(64);

    // 선택 모드 상태
    private int _selectionRemain = 0; // 0이면 열람 모드
    private int _requiredSelectionCount = 0;
    private bool _isSelectionMode = false;
    private Button _selectUnitButton;
    private bool IsSelectionMode => _isSelectionMode;
    public bool IsSelectionModeActive => IsSelectionMode && gameObject.activeInHierarchy;
    // 사용처: 외부(UI 호출자)에서 UnitListUI 닫힘 시점 후처리
    private Action _onClosedAction;
    private void Awake()
    {
        EnsureReferences();
        AddOrderButtonListeners();
        BindCloseButton();
        BindSelectionProceedButton();

        orderButtonMap[0] = dateOrderBtn;
        orderButtonMap[1] = dateOrderBtn;
        orderButtonMap[2] = rarityOrderBtn;
        orderButtonMap[3] = rarityOrderBtn;
        orderButtonMap[4] = branchOrderBtn;
        orderButtonMap[5] = branchOrderBtn;
        orderButtonMap[6] = energyOrderBtn;
        orderButtonMap[7] = energyOrderBtn;
        orderButtonMap[8] = nameOrderBtn;
        orderButtonMap[9] = nameOrderBtn;

    }

    private void Start()
    {
        if (objectPool == null)
            objectPool = GameManager.Instance != null ? GameManager.Instance.objectPool : null;
    }

    private void OnEnable()
    {
        Canvas.willRenderCanvases += ApplyActiveScrollbarHandleSize;
        ApplyModeUI();
        PlayOpenAnimation();
        CreateUnitList();
        transform.SetAsLastSibling();
    }

    // 사용처: 비활성화 시 세션 상태 정리 및 외부 닫힘 콜백 호출
    private void OnDisable()
    {
        Canvas.willRenderCanvases -= ApplyActiveScrollbarHandleSize;
        activeVerticalScrollbar = null;

        if (_unitOrder >= 0)
            RogueLikeData.Instance.SetUnitOrder(_unitOrder);

        RemoveSelectionListeners();
        _selectedUnits.Clear();
        _onSelectAction = null;
        _sourceUnits = null;
        _selectionRemain = 0;
        _requiredSelectionCount = 0;
        _isSelectionMode = false;
        _unitOrder = -1;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetUnitListToggleVisible(true);
            GameManager.Instance.SetUnitListToggleOpenState(false);
        }

        _onClosedAction?.Invoke();
        _onClosedAction = null;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // 사용처: 유닛 리스트 열기(선택 완료/닫힘 콜백 포함)
    public void Show(int unitCount = 0, List<RogueUnitDataBase> source = null, Action onSelected = null, Action onClosed = null)
    {
        EnsureReferences();
        _onClosedAction = onClosed;

        if (unitCount > 0)
        {
            _isSelectionMode = true;
            _requiredSelectionCount = unitCount;
            _onSelectAction = onSelected;
            List<RogueUnitDataBase> defaultUnits = RogueLikeData.Instance != null
                ? RogueLikeData.Instance.GetMyTeam()
                : null;
            _sourceUnits = source != null
                ? new List<RogueUnitDataBase>(source)
                : new List<RogueUnitDataBase>(defaultUnits ?? new List<RogueUnitDataBase>());
            _sourceUnits.RemoveAll(unit => unit == null);
            _selectedUnits.Clear();

            var resume = RogueLikeData.Instance != null
                ? RogueLikeData.Instance.GetSelectedUnits()
                : null;
            if (resume != null && resume.Count > 0)
            {
                var allow = new HashSet<RogueUnitDataBase>(_sourceUnits);
                for (int i = 0; i < resume.Count; i++)
                {
                    if (_selectedUnits.Count >= unitCount)
                        break;

                    if (allow.Contains(resume[i]) && !_selectedUnits.Contains(resume[i]))
                        _selectedUnits.Add(resume[i]);
                }
            }

            _selectionRemain = Math.Max(0, unitCount - _selectedUnits.Count);
        }
        else
        {
            _isSelectionMode = false;
            _requiredSelectionCount = 0;
            _onSelectAction = null;
            _sourceUnits = null;
            _selectedUnits.Clear();
            _selectionRemain = 0;
        }

        if (!enabled) enabled = true;
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        else
        {
            ApplyModeUI();
            CreateUnitList();
            transform.SetAsLastSibling();
        }
    }
    // 사용처: 모드별 상단 UI 전환
    private void ApplyModeUI()
    {
        EnsureReferences();
        if (IsSelectionMode)
        {
            if (buttons != null)
                buttons.SetActive(false);
            if (closeBtn != null)
                closeBtn.gameObject.SetActive(false);
            if (selectUnitObj != null)
                selectUnitObj.SetActive(true);
            UpdateSelectionProceedUI();

            if (GameManager.Instance != null)
                GameManager.Instance.SetUnitListToggleVisible(false);
        }
        else
        {
            if (buttons != null)
                buttons.SetActive(true);
            if (closeBtn != null)
                closeBtn.gameObject.SetActive(true);
            if (selectUnitObj != null)
                selectUnitObj.SetActive(false);

            if (GameManager.Instance != null)
                GameManager.Instance.SetUnitListToggleVisible(true);
        }
    }

    // 사용처: 패널 오픈 애니메이션
    private void PlayOpenAnimation()
    {
        var rect = (RectTransform)transform;
        rect.DOKill(false);
        panelSeq?.Kill();

        float h = rect.rect.height > 0 ? rect.rect.height : Screen.height;
        rect.anchoredPosition = new Vector2(0f, -h);

        panelSeq = DOTween.Sequence();
        panelSeq.SetUpdate(true);
        panelSeq.Join(rect.DOAnchorPosY(0f, openAnimTime).SetEase(Ease.OutCubic).SetUpdate(true));
    }

    // 사용처: 유닛 목록 구성 및 등장 연출
    public void CreateUnitList()
    {
        EnsureReferences();
        if (unitList == null)
        {
            Debug.LogError("[UnitListUI] unitList reference is missing.");
            return;
        }

        EnsureScrollViewport();
        // 선택 모드라면 source 우선, 아니면 보유 전체
        List<RogueUnitDataBase> units = _sourceUnits != null
            ? new List<RogueUnitDataBase>(_sourceUnits)
            : new List<RogueUnitDataBase>(RogueLikeData.Instance != null
                ? RogueLikeData.Instance.GetMyTeam() ?? new List<RogueUnitDataBase>()
                : new List<RogueUnitDataBase>()); // 항상 현재 보유 유닛 기준
        units.RemoveAll(unit => unit == null);

        // 캐시된 정렬 기준으로 정렬
        GetSortedUnits(ref units, GetUnitOrderCached());

        if (objectPool == null)
            objectPool = GameManager.Instance != null ? GameManager.Instance.objectPool : null;
        if (objectPool == null)
        {
            Debug.LogError("[UnitListUI] ObjectPool reference is missing.");
            return;
        }

        if (units.Count == 0)
        {
            int teamCount = RogueLikeData.Instance != null && RogueLikeData.Instance.GetMyTeam() != null
                ? RogueLikeData.Instance.GetMyTeam().Count
                : -1;
            Debug.LogWarning($"[UnitListUI] No units to render. selectionMode={IsSelectionMode}, sourceCount={_sourceUnits?.Count ?? -1}, myTeamCount={teamCount}");
        }

        if (!IsSelectionMode)
            ResetOrderBtn();


        listSeq?.Kill();
        var content = (RectTransform)unitList;
        content.DOKill(false);

        Canvas.ForceUpdateCanvases();
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, scrollStartYOffset);

        PrepareUnits(units);
        AnimateScrollAndFade(content);
    }

    // 사용처: 유닛 아이템 풀에서 재사용/생성하고 클릭 리스너/그래픽 초기화
    private void PrepareUnits(List<RogueUnitDataBase> units)
    {
        int childCount = unitList.childCount;
        cgCache.Clear();
        graphicsCache.Clear();

        bool selectionMode = IsSelectionMode;
        Debug.Log($"[UnitListUI] Render start. units={units.Count}, existingChildren={childCount}, selectionMode={selectionMode}");

        for (int i = 0; i < units.Count; i++)
        {
            var unit = units[i];
            GameObject unitObj;
            if (i < childCount)
            {
                unitObj = unitList.GetChild(i).gameObject;
                unitObj.SetActive(true);
            }
            else
            {
                unitObj = objectPool.GetOrderUnit();
                if (unitObj == null)
                {
                    Debug.LogError($"[UnitListUI] ObjectPool.GetOrderUnit returned null. index={i}, unitId={unit?.idx}");
                    continue;
                }
                unitObj.transform.SetParent(unitList, false);
            }

            ResetCardTransform(unitObj);

            // 데이터 바인딩
            OneUnitUI oneUnit = unitObj.GetComponent<OneUnitUI>();
            if (oneUnit == null)
            {
                Debug.LogError($"[UnitListUI] OrderUnit prefab has no OneUnitUI component. object={unitObj.name}, index={i}, unitId={unit?.idx}");
                continue;
            }

            oneUnit.unit = unit;
            try
            {
                oneUnit.SetOneUnit(unit);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UnitListUI] Failed to bind OrderUnit. object={unitObj.name}, index={i}, unitId={unit?.idx}\n{ex}");
            }
            //UIMaker.CreateSelectUnitEnergy(units[i], unitObj);

            // 선택 프레임 초기화/복원
            if (oneUnit.selectFrame != null)
            {
                bool preSelected = selectionMode && _selectedUnits.Contains(unit);
                oneUnit.selectFrame.SetActive(preSelected);
            }

            // 선택 모드용 클릭 리스너 최신 바인딩
            var btn = unitObj.GetComponent<Button>();
            if (btn != null)
            {
                if (_unitClickMap.TryGetValue(btn, out var old))
                {
                    btn.onClick.RemoveListener(old);
                    _unitClickMap.Remove(btn);
                }

                if (selectionMode)
                {
                    UnityAction act = () => OnClick_UnitSelect(oneUnit);
                    _unitClickMap[btn] = act;
                    btn.onClick.AddListener(act);
                }
            }

            // 페이드 준비
            var cg = unitObj.GetComponent<CanvasGroup>();
            if (cg == null) cg = unitObj.AddComponent<CanvasGroup>();
            cg.DOKill(false);
            cg.alpha = 1f;
            cg.interactable = false;
            cg.blocksRaycasts = false;

            var graphics = unitObj.GetComponentsInChildren<Graphic>(true);
            for (int g = 0; g < graphics.Length; g++)
            {
                var gr = graphics[g];
                if (gr == null) continue;
                var c = gr.color;
                c.a = 0f;
                gr.color = c;
            }

            cgCache.Add(cg);
            graphicsCache.Add(graphics);
        }

        // 남는 아이템 반환
        for (int i = childCount - 1; i >= units.Count; i--)
        {
            var go = unitList.GetChild(i).gameObject;

            var b = go.GetComponent<Button>();
            if (b != null && _unitClickMap.TryGetValue(b, out var act))
            {
                b.onClick.RemoveListener(act);
                _unitClickMap.Remove(b);
            }

            objectPool.ReturnOrderUnit(go);
        }

        var content = (RectTransform)unitList;
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        Canvas.ForceUpdateCanvases();
        UpdateScrollContentSizeAndScrollbar(units.Count);
        Canvas.ForceUpdateCanvases();

        string firstInfo = "none";
        if (unitList.childCount > 0)
        {
            var first = unitList.GetChild(0) as RectTransform;
            if (first != null)
                firstInfo = $"parent={first.parent?.name}, active={first.gameObject.activeSelf}, pos={first.anchoredPosition}, size={first.rect.size}";
        }
        Debug.Log($"[UnitListUI] Render end. childCount={unitList.childCount}, requestedUnits={units.Count}, first={firstInfo}");
    }

    // 사용처: 목록 스크롤/페이드 연출
    private void AnimateScrollAndFade(RectTransform content)
    {
        listSeq = DOTween.Sequence();
        listSeq.SetUpdate(true);

        listSeq.Join(content.DOAnchorPosY(0f, sortAnimTime).SetEase(Ease.OutCubic).SetUpdate(true));

        float masterAlpha = 0f;
        listSeq.Join(
            DOTween.To(() => masterAlpha, v =>
            {
                masterAlpha = v;

                for (int i = 0, uCnt = graphicsCache.Count; i < uCnt; i++)
                {
                    var arr = graphicsCache[i];
                    for (int j = 0, gCnt = arr.Length; j < gCnt; j++)
                    {
                        var gr = arr[j];
                        if (gr == null) continue;
                        var c = gr.color;
                        c.a = masterAlpha;
                        gr.color = c;
                    }
                }
            }, 1f, sortAnimTime).SetUpdate(true)
        );

        listSeq.OnComplete(() =>
        {
            for (int i = 0, cnt = cgCache.Count; i < cnt; i++)
            {
                var cg = cgCache[i];
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
            cgCache.Clear();
            graphicsCache.Clear();
        });

        listSeq.Play();
    }

    // 사용처: 선택 모드 클릭 – 선택/해제 토글, 남은 수/완료 처리
    private void OnClick_UnitSelect(OneUnitUI one)
    {
        if (!IsSelectionMode || one == null || one.unit == null)
            return;

        bool selected = _selectedUnits.Contains(one.unit);

        if (selected)
        {
            if (one.selectFrame != null) one.selectFrame.SetActive(false);
            _selectedUnits.Remove(one.unit);
            _selectionRemain = Math.Min(_requiredSelectionCount, _selectionRemain + 1);
        }
        else
        {
            if (_selectionRemain <= 0)
                return;

            if (one.selectFrame != null) one.selectFrame.SetActive(true);
            if (!_selectedUnits.Contains(one.unit))
                _selectedUnits.Add(one.unit);
            _selectionRemain--;
        }

        UpdateSelectionProceedUI();


        if (_selectionRemain == 0)
            RogueLikeData.Instance.SetSelectedUnits(new List<RogueUnitDataBase>(_selectedUnits));
        else
            RogueLikeData.Instance.SetSelectedUnits(new List<RogueUnitDataBase>());
    }

    // Selection mode confirmation button.
    private void OnClickProceedSelection()
    {
        if (!IsSelectionMode || _selectionRemain > 0)
            return;

        RogueLikeData.Instance.SetSelectedUnits(new List<RogueUnitDataBase>(_selectedUnits));

        var cb = _onSelectAction;
        _onSelectAction = null;
        _sourceUnits = null;
        _isSelectionMode = false;
        _requiredSelectionCount = 0;
        _selectionRemain = 0;

        if (cb != null)
            cb();

        CloseWithAnimation();
    }

    private void UpdateSelectionProceedUI()
    {
        if (selectUnitText != null)
            selectUnitText.text = _selectionRemain <= 0 ? "\uC120\uD0DD\uC644\uB8CC" : $"{_selectionRemain}\uBA85 \uC120\uD0DD";

        if (_selectUnitButton != null)
            _selectUnitButton.interactable = _selectionRemain <= 0;
    }

    private void RemoveSelectionListeners()
    {
        if (_unitClickMap.Count == 0) return;

        for (int i = 0, n = unitList.childCount; i < n; i++)
        {
            var go = unitList.GetChild(i).gameObject;
            var b = go.GetComponent<Button>();
            if (b != null && _unitClickMap.TryGetValue(b, out var act))
                b.onClick.RemoveListener(act);
        }
        _unitClickMap.Clear();
    }

    // 사용처: 정렬 로직
    public static void GetSortedUnits(ref List<RogueUnitDataBase> units, int order)
    {
        IOrderedEnumerable<RogueUnitDataBase> ordered;

        switch (order)
        {
            case 0: ordered = units.OrderBy(u => u.acquiredDate); break;
            case 1: ordered = units.OrderByDescending(u => u.acquiredDate); break;
            case 2: ordered = units.OrderBy(u => u.rarity).ThenBy(u => u.idx).ThenBy(u => u.Energy).ThenBy(u => u.acquiredDate); break;
            case 3: ordered = units.OrderByDescending(u => u.rarity).ThenBy(u => u.idx).ThenBy(u => u.Energy).ThenBy(u => u.acquiredDate); break;
            case 4: ordered = units.OrderBy(u => u.branchIdx).ThenBy(u => u.idx).ThenBy(u => u.Energy).ThenBy(u => u.acquiredDate); break;
            case 5: ordered = units.OrderByDescending(u => u.branchIdx).ThenBy(u => u.idx).ThenBy(u => u.Energy).ThenBy(u => u.acquiredDate); break;
            case 6: ordered = units.OrderBy(u => u.Energy).ThenBy(u => u.idx).ThenBy(u => u.acquiredDate); break;
            case 7: ordered = units.OrderByDescending(u => u.Energy).ThenBy(u => u.idx).ThenBy(u => u.acquiredDate); break;
            case 8: ordered = units.OrderBy(u => u.unitName).ThenBy(u => u.Energy).ThenBy(u => u.acquiredDate); break;
            case 9: ordered = units.OrderByDescending(u => u.unitName).ThenBy(u => u.Energy).ThenBy(u => u.acquiredDate); break;
            default: ordered = units.OrderBy(u => u.acquiredDate); break;
        }
        units = ordered.ToList();
    }

    // 기존 외부 코드 호환용 – 필요하면 계속 사용 가능
    public static void GetSortedUnits(ref List<RogueUnitDataBase> units)
    {
        int order = RogueLikeData.Instance.GetUnitOrder();
        GetSortedUnits(ref units, order);
    }
    // 사용처: 정렬 버튼 상태 초기화
    private void ResetOrderBtn()
    {
        int unitOrder = GetUnitOrderCached();
        DisableAllOrderImages();

        if (orderButtonMap.TryGetValue(unitOrder, out var targetBtn) && targetBtn != null && targetBtn.transform.childCount > 0)
        {
            Transform imgTr = targetBtn.transform.GetChild(0);
            if (imgTr != null)
            {
                imgTr.gameObject.SetActive(true);
                Vector3 scale = imgTr.localScale;
                scale.y = (unitOrder % 2 == 0) ? 1f : -1f;
                imgTr.localScale = scale;
            }
        }
    }

    // 사용처: 모든 정렬 버튼 아이콘 끄기
    private void DisableAllOrderImages()
    {
        SetOrderImageActive(dateOrderBtn, false);
        SetOrderImageActive(rarityOrderBtn, false);
        SetOrderImageActive(branchOrderBtn, false);
        SetOrderImageActive(energyOrderBtn, false);
        SetOrderImageActive(nameOrderBtn, false);
    }

    private static void SetOrderImageActive(Button button, bool active)
    {
        if (button == null || button.transform.childCount == 0)
            return;

        button.transform.GetChild(0).gameObject.SetActive(active);
    }

    // 사용처: 정렬 버튼 클릭 처리
    private void OnOrderButtonClicked(Button btn, int baseOrder)
    {
        if (btn == null || btn.transform.childCount == 0)
            return;

        Transform imgTr = btn.transform.GetChild(0);
        if (imgTr == null) return;

        if (imgTr.gameObject.activeSelf)
        {
            Vector3 scale = imgTr.localScale;
            scale.y *= -1f;
            imgTr.localScale = scale;

            _unitOrder = (scale.y > 0f) ? baseOrder : baseOrder + 1;
        }
        else
        {
            DisableAllOrderImages();
            imgTr.gameObject.SetActive(true);
            Vector3 scale = imgTr.localScale;
            scale.y = 1f;
            imgTr.localScale = scale;

            _unitOrder = baseOrder;
        }

        CreateUnitList();
    }
    // 사용처: 정렬 버튼 리스너 등록
    private void AddOrderButtonListeners()
    {
        if (dateOrderBtn != null) dateOrderBtn.onClick.AddListener(() => OnOrderButtonClicked(dateOrderBtn, 0));
        if (rarityOrderBtn != null) rarityOrderBtn.onClick.AddListener(() => OnOrderButtonClicked(rarityOrderBtn, 2));
        if (branchOrderBtn != null) branchOrderBtn.onClick.AddListener(() => OnOrderButtonClicked(branchOrderBtn, 4));
        if (energyOrderBtn != null) energyOrderBtn.onClick.AddListener(() => OnOrderButtonClicked(energyOrderBtn, 6));
        if (nameOrderBtn != null) nameOrderBtn.onClick.AddListener(() => OnOrderButtonClicked(nameOrderBtn, 8));
    }

    // Bind buttons that may be present only on the prefab.
    private void BindCloseButton()
    {
        if (closeBtn == null)
        {
            Transform close = FindChildByName(transform, "CloseBtn");
            if (close != null)
                closeBtn = close.GetComponent<Button>();
        }

        if (closeBtn == null)
            return;

        closeBtn.onClick.RemoveListener(CloseWithAnimation);
        closeBtn.onClick.AddListener(CloseWithAnimation);
    }

    private void BindSelectionProceedButton()
    {
        if (selectUnitObj == null)
        {
            Transform select = FindChildByName(transform, "SelectUnitObj");
            if (select != null)
                selectUnitObj = select.gameObject;
        }

        if (selectUnitObj == null)
            return;

        _selectUnitButton = selectUnitObj.GetComponent<Button>();
        if (_selectUnitButton == null)
            _selectUnitButton = selectUnitObj.AddComponent<Button>();

        if (_selectUnitButton.targetGraphic == null)
            _selectUnitButton.targetGraphic = selectUnitObj.GetComponent<Graphic>();

        _selectUnitButton.onClick.RemoveListener(OnClickProceedSelection);
        _selectUnitButton.onClick.AddListener(OnClickProceedSelection);
    }

    public void CloseWithAnimation()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (IsSelectionMode && _selectionRemain > 0)
            return;

        var rect = (RectTransform)transform;
        rect.DOKill(false);
        panelSeq?.Kill();

        float h = rect.rect.height > 0 ? rect.rect.height : Screen.height;
        rect.DOAnchorPos(new Vector2(0f, -h), 0.5f)
            .SetUpdate(true)
            .SetEase(Ease.InCubic)
            .OnComplete(() => gameObject.SetActive(false));
    }
    // 사용처: 정렬 기준 캐시 조회
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetUnitOrderCached()
    {
        if (_unitOrder < 0)
            _unitOrder = RogueLikeData.Instance != null ? RogueLikeData.Instance.GetUnitOrder() : 0;
        return _unitOrder;
    }

    private void EnsureReferences()
    {
        if (unitList == null)
        {
            Transform found = FindChildByName(transform, "SelectUnitParent");
            if (found == null)
                found = FindChildByName(transform, "UnitList");
            if (found != null)
                unitList = found;
        }

        if (buttons == null)
        {
            Transform found = FindChildByName(transform, "Buttons");
            if (found == null)
                found = FindChildByName(transform, "Button");
            if (found != null)
                buttons = found.gameObject;
        }

        if (closeBtn == null)
        {
            Transform found = FindChildByName(transform, "CloseBtn");
            if (found != null)
                closeBtn = found.GetComponent<Button>();
        }

        if (selectUnitObj == null)
        {
            Transform found = FindChildByName(transform, "SelectUnitObj");
            if (found != null)
                selectUnitObj = found.gameObject;
        }

        if (selectUnitText == null)
        {
            Transform found = FindChildByName(transform, "SelectUnitText");
            if (found != null)
                selectUnitText = found.GetComponent<TextMeshProUGUI>();
        }

        if (objectPool == null && GameManager.Instance != null)
            objectPool = GameManager.Instance.objectPool;
    }

    private void EnsureScrollViewport()
    {
        if (unitList == null)
            return;

        ScrollRect scrollRect = unitList.GetComponentInParent<ScrollRect>(true);
        if (scrollRect == null)
            return;

        RectTransform content = unitList as RectTransform;
        if (content != null && scrollRect.content != content)
            scrollRect.content = content;

        RectTransform viewport = scrollRect.viewport;
        if (viewport == null)
        {
            Transform found = FindChildByName(scrollRect.transform, "Viewport");
            viewport = found as RectTransform;
            if (viewport != null)
                scrollRect.viewport = viewport;
        }

        if (viewport == null)
            return;

        bool collapsed =
            Mathf.Abs(viewport.rect.width) <= 1f ||
            Mathf.Abs(viewport.rect.height) <= 1f ||
            (viewport.anchorMin == Vector2.zero && viewport.anchorMax == Vector2.zero && viewport.sizeDelta == Vector2.zero);

        if (!collapsed)
            return;

        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = Vector2.zero;
        viewport.anchoredPosition = Vector2.zero;
        viewport.pivot = new Vector2(0.5f, 0.5f);
        Debug.LogWarning($"[UnitListUI] Scroll viewport was collapsed and has been resized. scrollRect={scrollRect.name}, viewport={viewport.name}");
    }

    private void UpdateScrollContentSizeAndScrollbar(int itemCount)
    {
        if (unitList == null)
            return;

        ScrollRect scrollRect = unitList.GetComponentInParent<ScrollRect>(true);
        if (scrollRect == null)
            return;

        RectTransform content = scrollRect.content != null
            ? scrollRect.content
            : unitList as RectTransform;
        RectTransform viewport = scrollRect.viewport;

        if (content == null || viewport == null)
            return;

        float viewportHeight = viewport.rect.height;
        if (viewportHeight <= 1f)
            return;

        float requiredHeight = CalculateGridContentHeight(content, itemCount);
        content.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            Mathf.Max(requiredHeight, viewportHeight));

        bool needsScroll = requiredHeight > viewportHeight + 1f;
        scrollRect.vertical = needsScroll;
        if (needsScroll)
        {
            scrollRect.verticalNormalizedPosition = 1f;
            Canvas.ForceUpdateCanvases();
        }

        if (scrollRect.verticalScrollbar != null)
        {
            activeVerticalScrollbar = needsScroll ? scrollRect.verticalScrollbar : null;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            scrollRect.verticalScrollbar.gameObject.SetActive(needsScroll);

            if (needsScroll)
                ApplyScrollbarHandleSize(scrollRect.verticalScrollbar);

            if (!needsScroll)
            {
                scrollRect.verticalNormalizedPosition = 1f;
                scrollRect.verticalScrollbar.size = 1f;
            }
        }
    }

    private void LateUpdate()
    {
        ApplyActiveScrollbarHandleSize();
    }

    private void ApplyActiveScrollbarHandleSize()
    {
        ApplyScrollbarHandleSize(activeVerticalScrollbar);
    }

    private void ApplyScrollbarHandleSize(Scrollbar scrollbar)
    {
        if (scrollbar == null || !scrollbar.gameObject.activeInHierarchy)
            return;

        float targetSize = Mathf.Clamp01(scrollbarHandleSize);
        if (targetSize <= 0f || targetSize >= 1f)
            return;

        scrollbar.size = Mathf.Min(scrollbar.size, targetSize);
    }

    private static float CalculateGridContentHeight(RectTransform content, int itemCount)
    {
        if (content == null || itemCount <= 0)
            return 0f;

        GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
        if (grid == null)
            return content.rect.height;

        int rows;
        if (grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
        {
            int columns = Mathf.Max(1, grid.constraintCount);
            rows = Mathf.CeilToInt(itemCount / (float)columns);
        }
        else if (grid.constraint == GridLayoutGroup.Constraint.FixedRowCount)
        {
            rows = Mathf.Max(1, Mathf.Min(itemCount, grid.constraintCount));
        }
        else
        {
            float availableWidth = content.rect.width - grid.padding.horizontal;
            float cellWidth = Mathf.Max(1f, grid.cellSize.x + grid.spacing.x);
            int columns = Mathf.Max(1, Mathf.FloorToInt((availableWidth + grid.spacing.x) / cellWidth));
            rows = Mathf.CeilToInt(itemCount / (float)columns);
        }

        rows = Mathf.Max(1, rows);
        float gridHeight = grid.padding.vertical +
               rows * grid.cellSize.y +
               Mathf.Max(0, rows - 1) * grid.spacing.y;

        return Mathf.Max(gridHeight, CalculateActiveChildVisualHeight(content, itemCount));
    }

    private static float CalculateActiveChildVisualHeight(RectTransform content, int itemCount)
    {
        if (content == null || itemCount <= 0)
            return 0f;

        int count = Mathf.Min(itemCount, content.childCount);
        if (count <= 0)
            return 0f;

        bool hasBounds = false;
        float minY = 0f;
        float maxY = 0f;

        for (int i = 0; i < count; i++)
        {
            RectTransform child = content.GetChild(i) as RectTransform;
            if (child == null || !child.gameObject.activeSelf)
                continue;

            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(content, child);
            if (!hasBounds)
            {
                minY = bounds.min.y;
                maxY = bounds.max.y;
                hasBounds = true;
            }
            else
            {
                minY = Mathf.Min(minY, bounds.min.y);
                maxY = Mathf.Max(maxY, bounds.max.y);
            }
        }

        if (!hasBounds)
            return 0f;

        return Mathf.Max(0f, -minY) + Mathf.Max(0f, maxY);
    }

    private static void ResetCardTransform(GameObject unitObj)
    {
        if (unitObj == null)
            return;

        var rect = unitObj.transform as RectTransform;
        if (rect == null)
            return;

        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.anchoredPosition3D = Vector3.zero;
    }

    private static Transform FindChildByName(Transform root, string childName)
    {
        if (root == null || string.IsNullOrEmpty(childName))
            return null;

        if (root.name == childName)
            return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindChildByName(root.GetChild(i), childName);
            if (found != null)
                return found;
        }

        return null;
    }

}
