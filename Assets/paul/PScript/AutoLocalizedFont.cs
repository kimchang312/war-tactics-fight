using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AutoLocalizedFont : MonoBehaviour
{
    private TMP_Text textComponent;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        // 이벤트 구독 시작 (활성화될 때마다 최신 폰트 적용)
        FontManager.OnFontChanged += UpdateFont;

        // 켜질 때 현재 설정된 언어 폰트로 즉시 동기화
        if (FontManager.Instance != null)
        {
            FontManager.Instance.ApplyLanguageFont(RogueLikeData.Instance.GetLanguage());
        }
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        FontManager.OnFontChanged -= UpdateFont;
    }

    private void UpdateFont(TMP_FontAsset newFont)
    {
        if (textComponent != null && newFont != null)
        {
            textComponent.font = newFont;
        }
    }
}
