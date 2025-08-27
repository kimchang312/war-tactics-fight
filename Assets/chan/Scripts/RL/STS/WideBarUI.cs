using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class WideBarUI : MonoBehaviour
{
    [Header("WideBar UI 설정")]
    [SerializeField] private RectTransform wideBarPanel;        // WideBar 패널
    [SerializeField] private Button expandButton;               // 펼치기 버튼
    [SerializeField] private Button closeButton;                // 닫기 버튼 (선택사항)
    
    [Header("애니메이션 설정")]
    [SerializeField] private float animationDuration = 0.5f;    // 애니메이션 지속 시간
    [SerializeField] private float expandHeight = 300f;         // 펼쳐질 높이
    [SerializeField] private Ease expandEase = Ease.OutBack;    // 펼쳐질 때 이징
    [SerializeField] private Ease collapseEase = Ease.InBack;   // 접힐 때 이징
    
    [Header("LineUpBar 참조")]
    [SerializeField] private LineUpBar lineUpBar;               // LineUpBar 컴포넌트 참조
    
    // 애니메이션 상태 관리
    private bool isExpanded = false;
    private Vector2 collapsedPosition;  // 접힌 상태 위치
    private Vector2 expandedPosition;   // 펼쳐진 상태 위치
    private Vector2 originalSize;       // 원래 크기
    private Vector2 expandedSize;       // 펼쳐진 크기
    
    // 현재 실행 중인 애니메이션
    private Tween currentAnimation;

    private void Awake()
    {
        // 컴포넌트 검증
        if (wideBarPanel == null)
        {
            Debug.LogError("❌ WideBar 패널이 할당되지 않았습니다! Unity Inspector에서 연결하세요.");
            return;
        }

        if (expandButton == null)
        {
            Debug.LogError("❌ 펼치기 버튼이 할당되지 않았습니다! Unity Inspector에서 연결하세요.");
            return;
        }

        // 초기 위치 및 크기 저장
        InitializePositions();
        
        // 버튼 이벤트 연결
        SetupButtonEvents();
        
        // 초기 상태 설정 (접힌 상태)
        SetCollapsedState();
    }

    private void InitializePositions()
    {
        // 원래 크기 저장
        originalSize = wideBarPanel.sizeDelta;
        expandedSize = new Vector2(originalSize.x, expandHeight);
        
        // 현재 위치를 접힌 상태 위치로 설정
        collapsedPosition = wideBarPanel.anchoredPosition;
        
        // 펼쳐진 상태 위치 계산 (위로 올라가는 위치)
        expandedPosition = collapsedPosition + new Vector2(0, expandHeight);
    }

    private void SetupButtonEvents()
    {
        // 펼치기 버튼 이벤트 연결
        expandButton.onClick.AddListener(() => ToggleWideBar());
        
        // 닫기 버튼이 있다면 이벤트 연결
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(() => ToggleWideBar(false));
        }
    }

    // WideBar 토글 (기본값: 현재 상태의 반대)
    public void ToggleWideBar(bool? forceState = null)
    {
        bool targetState = forceState ?? !isExpanded;
        
        if (targetState)
        {
            ExpandWideBar();
        }
        else
        {
            CollapseWideBar();
        }
    }

    // WideBar 펼치기
    public void ExpandWideBar()
    {
        if (isExpanded || wideBarPanel == null) return;
        
        // 현재 실행 중인 애니메이션 중지
        currentAnimation?.Kill();
        
        // 애니메이션 실행
        currentAnimation = DOTween.Sequence()
            .Join(wideBarPanel.DOAnchorPos(expandedPosition, animationDuration).SetEase(expandEase))
            .Join(wideBarPanel.DOSizeDelta(expandedSize, animationDuration).SetEase(expandEase))
            .OnComplete(() => {
                isExpanded = true;
                OnWideBarExpanded();
            });
    }

    // WideBar 접기
    public void CollapseWideBar()
    {
        if (!isExpanded || wideBarPanel == null) return;
        
        // 현재 실행 중인 애니메이션 중지
        currentAnimation?.Kill();
        
        // 애니메이션 실행
        currentAnimation = DOTween.Sequence()
            .Join(wideBarPanel.DOAnchorPos(collapsedPosition, animationDuration).SetEase(collapseEase))
            .Join(wideBarPanel.DOSizeDelta(originalSize, animationDuration).SetEase(collapseEase))
            .OnComplete(() => {
                isExpanded = false;
                OnWideBarCollapsed();
            });
    }

    // WideBar가 펼쳐졌을 때 호출
    private void OnWideBarExpanded()
    {
        Debug.Log("🟢 WideBar가 펼쳐졌습니다.");
        
        // LineUpBar의 상태를 WideBar 펼침 상태로 설정
        if (lineUpBar != null)
        {
            lineUpBar.SetWideBarState(true);
        }
    }

    // WideBar가 접혔을 때 호출
    private void OnWideBarCollapsed()
    {
        Debug.Log("🔴 WideBar가 접혔습니다.");
        
        // LineUpBar의 상태를 WideBar 접힘 상태로 설정
        if (lineUpBar != null)
        {
            lineUpBar.SetWideBarState(false);
        }
    }

    // 초기 상태 설정 (접힌 상태)
    private void SetCollapsedState()
    {
        wideBarPanel.anchoredPosition = collapsedPosition;
        wideBarPanel.sizeDelta = originalSize;
        isExpanded = false;
    }

    // 현재 상태 반환
    public bool IsExpanded => isExpanded;

    // 애니메이션 중인지 확인
    public bool IsAnimating => currentAnimation != null && currentAnimation.IsActive();

    // 강제로 상태 설정 (애니메이션 없이)
    public void SetState(bool expanded, bool animate = false)
    {
        if (animate)
        {
            ToggleWideBar(expanded);
        }
        else
        {
            // 현재 애니메이션 중지
            currentAnimation?.Kill();
            
            if (expanded)
            {
                wideBarPanel.anchoredPosition = expandedPosition;
                wideBarPanel.sizeDelta = expandedSize;
                isExpanded = true;
                OnWideBarExpanded();
            }
            else
            {
                wideBarPanel.anchoredPosition = collapsedPosition;
                wideBarPanel.sizeDelta = originalSize;
                isExpanded = false;
                OnWideBarCollapsed();
            }
        }
    }

    // 컴포넌트가 파괴될 때 애니메이션 정리
    private void OnDestroy()
    {
        currentAnimation?.Kill();
    }
}
