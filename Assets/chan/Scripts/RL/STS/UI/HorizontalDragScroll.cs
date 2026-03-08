using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 배치 프리팹 컨테이너(가로)를 드래그로 스크롤할 수 있게 하는 경량 스크롤러.
/// - ScrollRect를 씬에서 건드리기 어려울 때 런타임으로 붙여 쓰는 용도
/// - parent를 viewport로 간주하고 content(본인)를 clamp 처리
/// </summary>
public class HorizontalDragScroll : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    [SerializeField] private RectTransform viewport; // 없으면 부모를 사용
    [SerializeField] private float dragSpeed = 1f;

    private RectTransform _content;
    private Vector2 _startAnchoredPos;
    private Vector2 _startPointerLocal;

    private void Awake()
    {
        _content = transform as RectTransform;
        if (_content == null)
        {
            enabled = false;
            return;
        }

        if (viewport == null)
        {
            viewport = _content.parent as RectTransform;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_content == null || viewport == null) return;
        _startAnchoredPos = _content.anchoredPosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            viewport,
            eventData.position,
            eventData.pressEventCamera,
            out _startPointerLocal
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_content == null || viewport == null) return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                viewport,
                eventData.position,
                eventData.pressEventCamera,
                out var pointerLocal))
            return;

        Vector2 delta = (pointerLocal - _startPointerLocal) * dragSpeed;
        Vector2 desired = _startAnchoredPos + new Vector2(delta.x, 0f);

        _content.anchoredPosition = ClampToViewport(desired);
    }

    private Vector2 ClampToViewport(Vector2 desiredAnchoredPos)
    {
        // content/viewport는 같은 부모 좌표계를 공유한다고 가정(일반 UI 구성)
        float viewportW = viewport.rect.width;
        float contentW = _content.rect.width;

        // content가 viewport보다 작으면 중앙(0) 고정
        if (contentW <= viewportW + 0.01f)
        {
            desiredAnchoredPos.x = 0f;
            return desiredAnchoredPos;
        }

        // anchoredPosition.x 범위를 clamp
        // pivot/anchor 구성에 따라 값이 조금 달라질 수 있어, 안전하게 넉넉히 clamp
        float halfOverflow = (contentW - viewportW) * 0.5f;
        float minX = -halfOverflow;
        float maxX = halfOverflow;
        desiredAnchoredPos.x = Mathf.Clamp(desiredAnchoredPos.x, minX, maxX);
        return desiredAnchoredPos;
    }
}


