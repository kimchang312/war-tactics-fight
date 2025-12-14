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

    private readonly Dictionary<int, Button> orderButtonMap = new Dictionary<int, Button>(10);
    private Sequence panelSeq;
    private Sequence listSeq;

    // 페이드 캐시(인스턴스 단위)
    private readonly List<CanvasGroup> cgCache = new List<CanvasGroup>(64);
    private readonly List<Graphic[]> graphicsCache = new List<Graphic[]>(64);

    // 선택 모드 상태
    private int _selectionRemain = 0; // 0이면 열람 모드
    private bool IsSelectionMode => _selectionRemain > 0;

    private void Awake()
    {
        AddOrderButtonListeners();

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
            objectPool = GameManager.Instance.objectPool;
    }

    private void OnEnable()
    {
        ApplyModeUI();
        PlayOpenAnimation();
        CreateUnitList();
        transform.SetAsFirstSibling();
    }

    private void OnDisable()
    {
        // 정렬 상태를 닫힐 때 한 번만 저장
        if (_unitOrder >= 0)
            RogueLikeData.Instance.SetUnitOrder(_unitOrder);

        CloseWithAnimation();
        RemoveSelectionListeners();
        _selectedUnits.Clear();
        _onSelectAction = null;
        _sourceUnits = null;
        _unitOrder = -1; // 다음 세션에서 다시 로드
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Show(int unitCount = 0, List<RogueUnitDataBase> source = null, Action onSelected = null)
    {
        if (unitCount > 0)
        {
            // 선택 모드
            _onSelectAction = onSelected;
            _sourceUnits = source ?? RogueLikeData.Instance.GetMyUnits();

            _selectedUnits.Clear();

            // 이전 선택 복원(후보군에 포함된 것만)
            var resume = RogueLikeData.Instance.GetSelectedUnits();
            if (resume != null && resume.Count > 0)
            {
                var allow = new HashSet<RogueUnitDataBase>(_sourceUnits);
                for (int i = 0; i < resume.Count; i++)
                    if (allow.Contains(resume[i]))
                        _selectedUnits.Add(resume[i]);
            }

            // 이미 충족 시 UI 없이 바로 완료
            if (_selectedUnits.Count >= unitCount)
            {
                RogueLikeData.Instance.SetSelectedUnits(_selectedUnits);
                _onSelectAction?.Invoke();
                _onSelectAction = null;
                _sourceUnits = null;
                return;
            }

            _selectionRemain = Math.Max(0, unitCount - _selectedUnits.Count);
        }
        else
        {
            // 열람 모드
            _onSelectAction = null;
            _sourceUnits = null;
            _selectedUnits.Clear();
            _selectionRemain = 0;
        }

        if (!enabled) enabled = true;
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        else ApplyModeUI();
    }

    // 사용처: 모드별 상단 UI 전환
    private void ApplyModeUI()
    {
        if (IsSelectionMode)
        {
            buttons.SetActive(false);
            selectUnitObj.SetActive(true);
            if (selectUnitText != null)
                selectUnitText.text = $"{_selectionRemain} 명 남음";
        }
        else
        {
            buttons.SetActive(true);
            selectUnitObj.SetActive(false);
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
        panelSeq.Join(rect.DOAnchorPosY(0f, openAnimTime).SetEase(Ease.OutCubic));
    }

    // 사용처: 유닛 목록 구성 및 등장 연출
    public void CreateUnitList()
    {
        // 선택 모드라면 source 우선, 아니면 보유 전체
        List<RogueUnitDataBase> units = _sourceUnits != null
            ? new List<RogueUnitDataBase>(_sourceUnits)
            : RogueLikeData.Instance.GetMyTeam(); // 항상 현재 보유 유닛 기준


        // 캐시된 정렬 기준으로 정렬
        GetSortedUnits(ref units, GetUnitOrderCached());

        if (objectPool == null)
            objectPool = GameManager.Instance.objectPool;

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

        for (int i = 0; i < units.Count; i++)
        {
            GameObject unitObj;
            if (i < childCount)
            {
                unitObj = unitList.GetChild(i).gameObject;
                unitObj.SetActive(true);
            }
            else
            {
                unitObj = objectPool.GetOrderUnit();
                unitObj.transform.SetParent(unitList, false);
            }

            // 데이터 바인딩
            OneUnitUI oneUnit = unitObj.GetComponent<OneUnitUI>();
            oneUnit.unit = units[i];
            UIMaker.CreateSelectUnitEnergy(units[i], unitObj);

            // 선택 프레임 초기화/복원
            if (oneUnit.selectFrame != null)
            {
                bool preSelected = selectionMode && _selectedUnits.Contains(units[i]);
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
        for (int i = units.Count; i < childCount; i++)
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
    }

    // 사용처: 목록 스크롤/페이드 연출
    private void AnimateScrollAndFade(RectTransform content)
    {
        listSeq = DOTween.Sequence();

        listSeq.Join(content.DOAnchorPosY(0f, sortAnimTime).SetEase(Ease.OutCubic));

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
            }, 1f, sortAnimTime)
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

        bool selected = one.selectFrame != null && one.selectFrame.activeSelf;

        if (selected)
        {
            if (one.selectFrame != null) one.selectFrame.SetActive(false);
            _selectedUnits.Remove(one.unit);
            _selectionRemain++;
        }
        else
        {
            if (_selectionRemain <= 0)
                return;

            if (one.selectFrame != null) one.selectFrame.SetActive(true);
            _selectedUnits.Add(one.unit);
            _selectionRemain--;
        }

        if (selectUnitText != null)
            selectUnitText.text = $"{_selectionRemain} 명 남음";

        if (_selectionRemain == 0)
        {
            RogueLikeData.Instance.SetSelectedUnits(_selectedUnits);

            var cb = _onSelectAction;
            _onSelectAction = null;
            _sourceUnits = null;

            if (cb != null) cb();

            CloseWithAnimation();
        }
    }

    // 사용처: 선택 모드용 per-item 리스너 일괄 제거
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

        if (orderButtonMap.TryGetValue(unitOrder, out var targetBtn))
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
        if (dateOrderBtn != null) dateOrderBtn.transform.GetChild(0).gameObject.SetActive(false);
        if (rarityOrderBtn != null) rarityOrderBtn.transform.GetChild(0).gameObject.SetActive(false);
        if (branchOrderBtn != null) branchOrderBtn.transform.GetChild(0).gameObject.SetActive(false);
        if (energyOrderBtn != null) energyOrderBtn.transform.GetChild(0).gameObject.SetActive(false);
        if (nameOrderBtn != null) nameOrderBtn.transform.GetChild(0).gameObject.SetActive(false);
    }

    // 사용처: 정렬 버튼 클릭 처리
    private void OnOrderButtonClicked(Button btn, int baseOrder)
    {
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
        dateOrderBtn.onClick.AddListener(() => OnOrderButtonClicked(dateOrderBtn, 0));
        rarityOrderBtn.onClick.AddListener(() => OnOrderButtonClicked(rarityOrderBtn, 2));
        branchOrderBtn.onClick.AddListener(() => OnOrderButtonClicked(branchOrderBtn, 4));
        energyOrderBtn.onClick.AddListener(() => OnOrderButtonClicked(energyOrderBtn, 6));
        nameOrderBtn.onClick.AddListener(() => OnOrderButtonClicked(nameOrderBtn, 8));
        //closeBtn.onClick.AddListener(CloseWithAnimation);
    }

    // 사용처: 닫기 애니메이션
    public void CloseWithAnimation()
    {
        var rect = (RectTransform)transform;
        rect.DOKill(false);
        panelSeq?.Kill();

        float h = rect.rect.height > 0 ? rect.rect.height : Screen.height;
        rect.DOAnchorPos(new Vector2(0f, -h), 0.5f)
            .SetEase(Ease.InCubic)
            .OnComplete(() => gameObject.SetActive(false));
    }

    // 사용처: 정렬 기준 캐시 조회
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetUnitOrderCached()
    {
        if (_unitOrder < 0)
            _unitOrder = RogueLikeData.Instance.GetUnitOrder();
        return _unitOrder;
    }

}
