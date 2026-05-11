using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine;

public class FontManager : MonoBehaviour
{
    public static FontManager Instance;

    [Header("Language Font Assets")]
    [SerializeField] private TMP_FontAsset fontKorean;   // 0
    [SerializeField] private TMP_FontAsset fontEnglish;  // 1
    [SerializeField] private TMP_FontAsset fontJapanese; // 2

    // 폰트 변경 이벤트를 구독할 델리게이트(방송국 역할)
    public static event Action<TMP_FontAsset> OnFontChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 설정창에서 언어를 변경할 때 호출할 함수
    public void ApplyLanguageFont(int languageIndex)
    {
        TMP_FontAsset selectedFont = fontKorean; // 기본값

        switch (languageIndex)
        {
            case 0: selectedFont = fontKorean; break;
            case 1: selectedFont = fontEnglish; break;
            case 2: selectedFont = fontJapanese; break;
        }

        // 폰트가 변경되었다고 씬 내의 모든 구독자에게 알림
        OnFontChanged?.Invoke(selectedFont);
    }
}
