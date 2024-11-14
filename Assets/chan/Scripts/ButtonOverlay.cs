using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonOverlay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image overlayImage;  // ������ ��������Ʈ
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        // �ʱ� ���� ����
        SetAlpha(0.3f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Ŀ���� ��ư ���� �ö�� �� 0.5�ʿ� ���� ������ 0%�� ����
        StartFade(0f, 0.5f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Ŀ���� ��ư�� ���� �� 0.3�ʿ� ���� ������ 30%�� ����
        StartFade(0.3f, 0.3f);
    }

    private void StartFade(float targetAlpha, float duration)
    {
        // �̹� ���� ���� ���̵� ȿ���� ������ ����
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        // ���ο� ���̵� ȿ�� ����
        fadeCoroutine = StartCoroutine(FadeTo(targetAlpha, duration));
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = overlayImage.color.a;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(targetAlpha);  // ���������� ��ǥ ���� �� ����
    }

    private void SetAlpha(float alpha)
    {
        Color color = overlayImage.color;
        color.a = alpha;
        overlayImage.color = color;
    }
}
