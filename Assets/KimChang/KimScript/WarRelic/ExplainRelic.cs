using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ExplainRelic : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // 연결된 RelicToolTip 오브젝트
    [SerializeField] private GameObject RelicToolTip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        // RelicToolTip 연결되어 있는지 확인
        if (RelicToolTip == null)
        {
            RelicToolTip = FindInactiveObject("RelicToolTip");
            if (RelicToolTip == null)
            {
                Debug.LogError("RelicToolTip object not found in the scene.");
                return;
            }
        }
        
        // 오브젝트 이름을 숫자로 파싱
        if (int.TryParse(gameObject.name, out int id))
        {
            // GameTextData에서 유물 가져오기
            var relic = WarRelicDatabase.GetRelicById(id); // 기본 언어 (예: 한국어)
            (string name,string description) = (relic.name, relic.tooltip);

            // RelicToolTip 활성화 및 위치 업데이트
            if (!RelicToolTip.activeSelf)
            {
                RelicToolTip.SetActive(true);
            }

            // 마우스 위치로 RelicToolTip 이동
            RectTransform tooltipRect = RelicToolTip.GetComponent<RectTransform>();
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

            // RelicToolTip 마지막 자식의 텍스트 설정
            TextMeshProUGUI textComponent = RelicToolTip.transform.GetChild(RelicToolTip.transform.childCount - 1)
                .GetComponent<TextMeshProUGUI>();
            textComponent.text = $"{name}\n{description}";

            // RelicToolTip Canvas의 가장 앞으로 이동
            RelicToolTip.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogWarning("Object name is not a valid number.");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // RelicToolTip 비활성화
        if (RelicToolTip != null && RelicToolTip.activeSelf)
        {
            RelicToolTip.SetActive(false);
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
