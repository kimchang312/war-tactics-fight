using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonOverlay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image overlayImage;  // 검은색 스프라이트
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        // 초기 투명도 설정
        SetAlpha(0.3f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 커서가 버튼 위로 올라올 때 0.5초에 걸쳐 투명도를 0%로 변경
        StartFade(0f, 0.5f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 커서가 버튼을 떠날 때 0.3초에 걸쳐 투명도를 30%로 변경
        StartFade(0.3f, 0.3f);
    }

    private void StartFade(float targetAlpha, float duration)
    {
        // 이미 진행 중인 페이드 효과가 있으면 중지
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        // 새로운 페이드 효과 시작
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

        SetAlpha(targetAlpha);  // 최종적으로 목표 알파 값 설정
    }

    private void SetAlpha(float alpha)
    {
        Color color = overlayImage.color;
        color.a = alpha;
        overlayImage.color = color;
    }
}
