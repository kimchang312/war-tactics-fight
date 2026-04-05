using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PlacePanel의 유닛 스트립(UnitPrefabsP / EnemyPrefabsP)용.
/// ScrollRect와 같은 오브젝트(또는 인스펙터에서 연결)에 두고,
/// 자식(유닛 프리팹) 개수·뷰포트 대비 Content 폭에 따라 <see cref="ScrollRect.horizontal"/>을 켜고 끕니다.
/// </summary>
[DisallowMultipleComponent]
public class PlacePanelStripScroll : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;

    [Header("가로 드래그 허용 조건")]
    [Tooltip("이 개수 이상이면 가로 스크롤(드래그)을 켤 수 있습니다.")]
    [Min(0)]
    [SerializeField] private int minChildCountForHorizontalDrag = 4;

    [Tooltip("켜면: 개수 조건 + Content가 Viewport보다 넓을 때만 스크롤. 끄면 개수만 맞으면 가로 스크롤 허용(실제 이동은 넘칠 때만).")]
    [SerializeField] private bool alsoRequireContentWiderThanViewport = false;

    private void Reset()
    {
        scrollRect = GetComponent<ScrollRect>();
        if (scrollRect != null)
            content = scrollRect.content;
    }

    /// <summary>자식 추가/삭제 후 호출 — 레이아웃 재계산 후 ScrollRect.horizontal 갱신.</summary>
    public void RefreshAfterContentChange()
    {
        if (scrollRect == null || content == null)
            return;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        Canvas.ForceUpdateCanvases();

        int n = content.childCount;
        bool countOk = n >= minChildCountForHorizontalDrag;

        bool wider = false;
        if (scrollRect.viewport != null)
        {
            float cw = content.rect.width;
            float vw = scrollRect.viewport.rect.width;
            wider = cw > vw + 0.5f;
        }

        bool allow = countOk && (!alsoRequireContentWiderThanViewport || wider);

        scrollRect.horizontal = allow;
        if (!allow)
            scrollRect.horizontalNormalizedPosition = 0f;
    }
}
