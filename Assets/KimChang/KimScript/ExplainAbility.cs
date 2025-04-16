using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ExplainAbility : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject AbilityToolTip; // 연결된 AbilityToolTip 오브젝트

    public void OnPointerEnter(PointerEventData eventData)
    {
        // AbilityToolTip이 연결되어 있는지 확인
        if (AbilityToolTip == null)
        {
            AbilityToolTip = FindInactiveObject("AbilityToolTip");
            if (AbilityToolTip == null)
            {
                Debug.LogError("AbilityToolTip object not found in the scene.");
                return;
            }
        }

        // 오브젝트 이름을 숫자로 파싱
        if (int.TryParse(gameObject.name, out int idx))
        {
            // GameTextData에서 이름과 설명 가져오기
            var (name, description) = GameTextData.GetLocalizedText(idx); // 기본 언어 (예: 한국어)

            // AbilityToolTip 활성화 및 위치 업데이트
            if (!AbilityToolTip.activeSelf)
            {
                AbilityToolTip.SetActive(true);
            }

            // 마우스 위치로 AbilityToolTip 이동
            RectTransform tooltipRect = AbilityToolTip.GetComponent<RectTransform>();
            Canvas canvas = tooltipRect.GetComponentInParent<Canvas>();
            Vector2 mousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out mousePosition
            );
            // 마우스 기준 오프셋 적용
            Vector2 offset = new Vector2(110, -60); // 오른쪽으로 10px, 아래로 10px 이동
            tooltipRect.anchoredPosition = mousePosition + offset;

            // AbilityToolTip의 마지막 자식의 텍스트 설정
            TextMeshProUGUI textComponent = AbilityToolTip.transform.GetChild(AbilityToolTip.transform.childCount - 1)
                .GetComponent<TextMeshProUGUI>();
            textComponent.text = $"{name}\n{description}";

            // AbilityToolTip을 Canvas의 가장 앞으로 이동
            AbilityToolTip.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogWarning("Object name is not a valid number.");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // AbilityToolTip 비활성화
        if (AbilityToolTip != null && AbilityToolTip.activeSelf)
        {
            AbilityToolTip.SetActive(false);
        }
    }

    private GameObject FindInactiveObject(string name)
    {
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (var t in allTransforms)
        {
            if (t.name == name && t.gameObject.hideFlags == HideFlags.None)
            {
                return t.gameObject;
            }
        }
        return null; // 찾지 못한 경우
    }

}
