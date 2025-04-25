using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ExplainItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject ItemToolTip;
    private Coroutine hideCoroutine;

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 비활성화 예약된 코루틴 중지
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (ItemToolTip == null)
        {
            ItemToolTip = FindInactiveObject("ItemToolTip");
            if (ItemToolTip == null)
            {
                Debug.LogError("ItemToolTip object not found in the scene.");
                return;
            }
        }

        if (int.TryParse(gameObject.name, out int id))
        {
            var itemList = StoreItemDataLoader.Load();
            if (id < 0 || id >= itemList.Count)
            {
                Debug.LogWarning($"Item ID {id} is out of range.");
                return;
            }

            string name = itemList[id].itemName;

            if (!ItemToolTip.activeSelf)
                ItemToolTip.SetActive(true);

            RectTransform tooltipRect = ItemToolTip.GetComponent<RectTransform>();
            Canvas canvas = tooltipRect.GetComponentInParent<Canvas>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out var mousePosition
            );

            Vector2 offset = new Vector2(110, -110);
            tooltipRect.anchoredPosition = mousePosition + offset;

            TextMeshProUGUI textComponent = ItemToolTip.transform.GetChild(ItemToolTip.transform.childCount - 1)
                .GetComponent<TextMeshProUGUI>();
            textComponent.text = name;

            ItemToolTip.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogWarning("Object name is not a valid number.");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemToolTip != null && ItemToolTip.activeSelf)
        {
            hideCoroutine = StartCoroutine(DelayedHide());
        }
    }

    private IEnumerator DelayedHide()
    {
        yield return new WaitForSeconds(0.5f);
        if (ItemToolTip != null)
        {
            ItemToolTip.SetActive(false);
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
