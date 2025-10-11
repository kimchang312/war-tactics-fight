using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UnitListUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform unitList;     // 함수 사용처: 유닛 아이템들이 들어가는 ScrollRect의 Content
    [SerializeField] private Button dateOrderBtn;
    [SerializeField] private Button rarityOrderBtn;
    [SerializeField] private Button branchOrderBtn;
    [SerializeField] private Button energyOrderBtn;
    [SerializeField] private Button nameOrderBtn;
    [SerializeField] private Button closeBtn;
    [SerializeField] private ObjectPool objectPool;

    [Header("Anim")]
    [SerializeField] private float openAnimTime = 0.5f;     // 함수 사용처: 패널 열릴 때 슬라이드 시간
    [SerializeField] private float sortAnimTime = 0.5f;     // 함수 사용처: 정렬 시 스크롤+페이드 시간
    [SerializeField] private float scrollStartYOffset = -75f; // 함수 사용처: 정렬 시작 시 Content의 시작 Y 오프셋(음수면 아래)

    private readonly Dictionary<int, Button> orderButtonMap = new Dictionary<int, Button>(10);
    private Sequence panelSeq;   // 함수 사용처: 패널 열릴 때 슬라이드 시퀀스
    private Sequence listSeq;    // 함수 사용처: 정렬 시 스크롤+페이드 동시 시퀀스

    // 성능: 인터랙션 토글을 위한 부모 CanvasGroup 캐시, 시각 페이드를 위한 자식 Graphic 배열 캐시
    private static readonly List<CanvasGroup> cgCache = new List<CanvasGroup>(64);
    private static readonly List<Graphic[]> graphicsCache = new List<Graphic[]>(64);

    private void Awake()
    {
        AddOrderButtonListeners();

        orderButtonMap[0] = dateOrderBtn;   // 획득 오름
        orderButtonMap[1] = dateOrderBtn;   // 획득 내림
        orderButtonMap[2] = rarityOrderBtn; // 희귀도 오름
        orderButtonMap[3] = rarityOrderBtn; // 희귀도 내림
        orderButtonMap[4] = branchOrderBtn; // 병종 오름
        orderButtonMap[5] = branchOrderBtn; // 병종 내림
        orderButtonMap[6] = energyOrderBtn; // 기력 오름
        orderButtonMap[7] = energyOrderBtn; // 기력 내림
        orderButtonMap[8] = nameOrderBtn;   // 이름 오름
        orderButtonMap[9] = nameOrderBtn;   // 이름 내림

        gameObject.SetActive(false);
    }

    private void Start()
    {
        if (objectPool == null)
            objectPool = GameManager.Instance.objectPool;
    }

    private void OnEnable()
    {
        // 함수 사용처: 패널이 활성화될 때 아래→위 슬라이드
        PlayOpenAnimation();

        // 함수 사용처: 리스트 생성 및 동시 애니메이션
        CreateUnitList();

        transform.SetAsFirstSibling();
    }

    // 함수 사용처: 패널 열릴 때 패널 자체 슬라이드
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

    // 함수 사용처: 유닛 리스트 생성/정렬 + 스크롤/페이드 동시 재생
    public void CreateUnitList()
    {
        List<RogueUnitDataBase> units = RogueLikeData.Instance.GetMyUnits();
        GetSortedUnits(ref units);

        if (objectPool == null)
            objectPool = GameManager.Instance.objectPool;

        ResetOrderBtn();

        listSeq?.Kill();
        var content = (RectTransform)unitList;
        content.DOKill(false);

        Canvas.ForceUpdateCanvases();
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, scrollStartYOffset);

        PrepareUnits(units);
        AnimateScrollAndFade(content);
    }

    // 함수 사용처: 유닛 오브젝트를 재사용/생성하고 자식들까지 전부 투명으로 초기화
    private void PrepareUnits(List<RogueUnitDataBase> units)
    {
        int childCount = unitList.childCount;
        cgCache.Clear();
        graphicsCache.Clear();

        // 재사용/생성
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

            // 부모 CanvasGroup은 클릭/레이캐스트 제어용으로만 사용(알파는 1로 고정)
            var cg = unitObj.GetComponent<CanvasGroup>();
            if (cg == null) cg = unitObj.AddComponent<CanvasGroup>();
            cg.DOKill(false);
            cg.alpha = 1f;
            cg.interactable = false;
            cg.blocksRaycasts = false;

            // 자식 Graphic 전체를 0 알파로 초기화(Image 2, TextMeshProUGUI 2 포함)
            var graphics = unitObj.GetComponentsInChildren<Graphic>(true);
            for (int g = 0; g < graphics.Length; g++)
            {
                var gr = graphics[g];
                if (gr == null) continue;
                Color c = gr.color;
                c.a = 0f;
                gr.color = c;
            }

            cgCache.Add(cg);
            graphicsCache.Add(graphics);
        }

        // 남는 자식 반환
        for (int i = units.Count; i < childCount; i++)
        {
            objectPool.ReturnOrderUnit(unitList.GetChild(i).gameObject);
        }
    }

    // 함수 사용처: 콘텐츠가 아래에서 0으로 올라오면서 동시에 모든 자식 Graphic 알파 상승
    private void AnimateScrollAndFade(RectTransform content)
    {
        listSeq = DOTween.Sequence();

        // 스크롤 이동
        listSeq.Join(content.DOAnchorPosY(0f, sortAnimTime).SetEase(Ease.OutCubic));

        // 자식 Graphic 일괄 트윈(성능 우선: 트윈 1개에서 전파)
        float masterAlpha = 0f;
        listSeq.Join(
            DOTween.To(() => masterAlpha, v =>
            {
                masterAlpha = v;

                // 유닛별 Graphics에 알파 적용
                for (int i = 0, uCnt = graphicsCache.Count; i < uCnt; i++)
                {
                    var arr = graphicsCache[i];
                    // 배열 길이 캐시
                    for (int j = 0, gCnt = arr.Length; j < gCnt; j++)
                    {
                        var gr = arr[j];
                        if (gr == null) continue;
                        Color c = gr.color;
                        c.a = masterAlpha;
                        gr.color = c;
                    }
                }
            }, 1f, sortAnimTime)
        );

        // 완료 후 상호작용 복원
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

    // 함수 사용처: 정렬 로직
    public static void GetSortedUnits(ref List<RogueUnitDataBase> units)
    {
        int order = RogueLikeData.Instance.GetUnitOrder();
        IOrderedEnumerable<RogueUnitDataBase> ordered;

        switch (order)
        {
            case 0: // 획득 오름
                ordered = units.OrderBy(u => u.acquiredDate);
                break;
            case 1: // 획득 내림
                ordered = units.OrderByDescending(u => u.acquiredDate);
                break;
            case 2: // 희귀도 오름
                ordered = units.OrderBy(u => u.rarity)
                               .ThenBy(u => u.idx)
                               .ThenBy(u => u.Energy)
                               .ThenBy(u => u.acquiredDate);
                break;
            case 3: // 희귀도 내림
                ordered = units.OrderByDescending(u => u.rarity)
                               .ThenBy(u => u.idx)
                               .ThenBy(u => u.Energy)
                               .ThenBy(u => u.acquiredDate);
                break;
            case 4: // 병종 오름
                ordered = units.OrderBy(u => u.branchIdx)
                               .ThenBy(u => u.idx)
                               .ThenBy(u => u.Energy)
                               .ThenBy(u => u.acquiredDate);
                break;
            case 5: // 병종 내림
                ordered = units.OrderByDescending(u => u.branchIdx)
                               .ThenBy(u => u.idx)
                               .ThenBy(u => u.Energy)
                               .ThenBy(u => u.acquiredDate);
                break;
            case 6: // 기력 오름
                ordered = units.OrderBy(u => u.Energy)
                               .ThenBy(u => u.idx)
                               .ThenBy(u => u.acquiredDate);
                break;
            case 7: // 기력 내림
                ordered = units.OrderByDescending(u => u.Energy)
                               .ThenBy(u => u.idx)
                               .ThenBy(u => u.acquiredDate);
                break;
            case 8: // 이름 오름
                ordered = units.OrderBy(u => u.unitName)
                               .ThenBy(u => u.Energy)
                               .ThenBy(u => u.acquiredDate);
                break;
            case 9: // 이름 내림
                ordered = units.OrderByDescending(u => u.unitName)
                               .ThenBy(u => u.Energy)
                               .ThenBy(u => u.acquiredDate);
                break;
            default: // 기본값: 획득 오름
                ordered = units.OrderBy(u => u.acquiredDate);
                break;
        }
        units = ordered.ToList();
    }

    // 함수 사용처: 정렬 버튼 상태 초기화(아이콘 및 방향)
    private void ResetOrderBtn()
    {
        int unitOrder = RogueLikeData.Instance.GetUnitOrder();
        DisableAllOrderImages();

        if (orderButtonMap.TryGetValue(unitOrder, out var targetBtn))
        {
            Transform imgTr = targetBtn.transform.GetChild(0);
            if (imgTr != null)
            {
                imgTr.gameObject.SetActive(true);
                Vector3 scale = imgTr.localScale;
                scale.y = (unitOrder % 2 == 0) ? 1f : -1f; // 짝수=오름, 홀수=내림
                imgTr.localScale = scale;
            }
        }
    }

    // 함수 사용처: 모든 정렬 버튼의 아이콘 비활성화
    private void DisableAllOrderImages()
    {
        if (dateOrderBtn != null) dateOrderBtn.transform.GetChild(0).gameObject.SetActive(false);
        if (rarityOrderBtn != null) rarityOrderBtn.transform.GetChild(0).gameObject.SetActive(false);
        if (branchOrderBtn != null) branchOrderBtn.transform.GetChild(0).gameObject.SetActive(false);
        if (energyOrderBtn != null) energyOrderBtn.transform.GetChild(0).gameObject.SetActive(false);
        if (nameOrderBtn != null) nameOrderBtn.transform.GetChild(0).gameObject.SetActive(false);
    }

    // 함수 사용처: 정렬 버튼 클릭 처리
    private void OnOrderButtonClicked(Button btn, int baseOrder)
    {
        Transform imgTr = btn.transform.GetChild(0);
        if (imgTr == null) return;

        if (imgTr.gameObject.activeSelf)
        {
            // 이미 활성화 → 방향 반전
            Vector3 scale = imgTr.localScale;
            scale.y *= -1f;
            imgTr.localScale = scale;

            int newOrder = (scale.y > 0f) ? baseOrder : baseOrder + 1;
            RogueLikeData.Instance.SetUnitOrder(newOrder);
        }
        else
        {
            DisableAllOrderImages();
            imgTr.gameObject.SetActive(true);
            Vector3 scale = imgTr.localScale;
            scale.y = 1f;
            imgTr.localScale = scale;

            RogueLikeData.Instance.SetUnitOrder(baseOrder);
        }

        // 정렬 갱신과 동시에 스크롤+페이드 동시 실행
        CreateUnitList();
    }

    // 함수 사용처: 버튼 리스너 등록
    private void AddOrderButtonListeners()
    {
        dateOrderBtn.onClick.AddListener(() => OnOrderButtonClicked(dateOrderBtn, 0));
        rarityOrderBtn.onClick.AddListener(() => OnOrderButtonClicked(rarityOrderBtn, 2));
        branchOrderBtn.onClick.AddListener(() => OnOrderButtonClicked(branchOrderBtn, 4));
        energyOrderBtn.onClick.AddListener(() => OnOrderButtonClicked(energyOrderBtn, 6));
        nameOrderBtn.onClick.AddListener(() => OnOrderButtonClicked(nameOrderBtn, 8));
        closeBtn.onClick.AddListener(CloseWithAnimation);
    }

    // 함수 사용처: 닫기 애니메이션(아래로 슬라이드 후 비활성)
    private void CloseWithAnimation()
    {
        var rect = (RectTransform)transform;
        rect.DOKill(false);
        panelSeq?.Kill();

        float h = rect.rect.height > 0 ? rect.rect.height : Screen.height;
        rect.DOAnchorPos(new Vector2(0f, -h), 0.5f)
            .SetEase(Ease.InCubic)
            .OnComplete(() => gameObject.SetActive(false));
    }
}
