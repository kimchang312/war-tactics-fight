using TMPro;
using UnityEngine.EventSystems;
using UnityEngine;
using System.Collections;

public class ExplainRelic : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject RelicToolTip;
    private Coroutine hideCoroutine;

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 비활성화 예약된 코루틴이 있다면 중지
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (RelicToolTip == null)
        {
            RelicToolTip = FindInactiveObject("RelicToolTip");
            if (RelicToolTip == null)
            {
                Debug.LogError("RelicToolTip object not found in the scene.");
                return;
            }
        }

        if (int.TryParse(gameObject.name, out int id))
        {
            var relic = WarRelicDatabase.GetRelicById(id);
            (string name, string description) = (relic.name, relic.tooltip);

            if (!RelicToolTip.activeSelf)
                RelicToolTip.SetActive(true);

            RectTransform tooltipRect = RelicToolTip.GetComponent<RectTransform>();
            Canvas canvas = tooltipRect.GetComponentInParent<Canvas>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out var mousePosition
            );

            Vector2 offset = new Vector2(110, -110);
            tooltipRect.anchoredPosition = mousePosition + offset;

            TextMeshProUGUI textComponent = RelicToolTip.transform.GetChild(RelicToolTip.transform.childCount - 1)
                .GetComponent<TextMeshProUGUI>();
            textComponent.text = $"{name}\n{description}";

            RelicToolTip.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogWarning("Object name is not a valid number.");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (RelicToolTip != null && RelicToolTip.activeSelf)
        {
            // 0.5초 후 비활성화하는 코루틴 시작
            hideCoroutine = StartCoroutine(DelayedHide());
        }
    }

    private IEnumerator DelayedHide()
    {
        yield return new WaitForSeconds(0.5f);
        if (RelicToolTip != null)
        {
            RelicToolTip.SetActive(false);
        }
        hideCoroutine = null;
    }

    private GameObject FindInactiveObject(string name)
    {
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (var t in allTransforms)
        {
            if (t.name == name && t.gameObject.hideFlags == HideFlags.None)
                return t.gameObject;
        }
        return null;
    }
}
