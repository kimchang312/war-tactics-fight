using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FontManager : MonoBehaviour
{
    private const int JapaneseLanguageIndex = 2;

    public static FontManager Instance;

    [Header("Language Font Assets")]
    [SerializeField] private TMP_FontAsset fontKorean;
    [SerializeField] private TMP_FontAsset fontEnglish;
    [SerializeField] private TMP_FontAsset fontJapanese;

    private readonly HashSet<AutoLocalizedFont> targets = new HashSet<AutoLocalizedFont>();
    private TMP_FontAsset currentFont;

    public static event Action<TMP_FontAsset> OnFontChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            GameTextDB.LanguageChanged += OnGameLanguageChanged;
            ConfigureFallbacks();
            ApplyLanguageFont(GetSavedLanguageIndex());
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameTextDB.LanguageChanged -= OnGameLanguageChanged;
        Instance = null;
    }

    public void ApplyLanguageFont(int languageIndex)
    {
        TMP_FontAsset selectedFont = GetFont(languageIndex);
        if (selectedFont == null)
            return;

        PrepareFontForCurrentText(selectedFont);

        bool changed = currentFont != selectedFont;
        currentFont = selectedFont;

        if (changed)
            OnFontChanged?.Invoke(selectedFont);

        ApplyToRegisteredTargets(selectedFont);
        ApplyToLoadedTextObjects(selectedFont);
    }

    public TMP_FontAsset GetFont(int languageIndex)
    {
        switch (Mathf.Clamp(languageIndex, 0, 2))
        {
            case 1:
                return fontEnglish != null ? fontEnglish : fontKorean;
            case JapaneseLanguageIndex:
                return fontJapanese != null ? fontJapanese : fontKorean;
            default:
                return fontKorean;
        }
    }

    public void Register(AutoLocalizedFont target)
    {
        if (target == null)
            return;

        targets.Add(target);

        TMP_FontAsset font = currentFont != null
            ? currentFont
            : GetFont(GetSavedLanguageIndex());

        target.ApplyFont(font);
    }

    public void Unregister(AutoLocalizedFont target)
    {
        if (target != null)
            targets.Remove(target);
    }

    private void ApplyToRegisteredTargets(TMP_FontAsset font)
    {
        List<AutoLocalizedFont> staleTargets = null;
        foreach (AutoLocalizedFont target in targets)
        {
            if (target == null)
            {
                if (staleTargets == null)
                    staleTargets = new List<AutoLocalizedFont>();

                staleTargets.Add(target);
                continue;
            }

            target.ApplyFont(font);
        }

        if (staleTargets == null)
            return;

        for (int i = 0; i < staleTargets.Count; i++)
            targets.Remove(staleTargets[i]);
    }

    private void ApplyToLoadedTextObjects(TMP_FontAsset font)
    {
        TMP_Text[] texts = FindObjectsOfType<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
            ApplyFontToText(texts[i], font);

        TMP_InputField[] inputFields = FindObjectsOfType<TMP_InputField>(true);
        for (int i = 0; i < inputFields.Length; i++)
        {
            ApplyFontToText(inputFields[i].textComponent, font);
            ApplyFontToText(inputFields[i].placeholder as TMP_Text, font);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyLanguageFont(GetSavedLanguageIndex());
    }

    private void OnGameLanguageChanged()
    {
        ApplyLanguageFont(GetSavedLanguageIndex());
    }

    private void ConfigureFallbacks()
    {
        AddFallback(fontKorean, fontJapanese);
        AddFallback(fontEnglish, fontJapanese);
        AddFallback(fontJapanese, fontKorean);
    }

    private static void AddFallback(TMP_FontAsset font, TMP_FontAsset fallback)
    {
        if (font == null || fallback == null || font == fallback)
            return;

        if (font.fallbackFontAssetTable == null)
            font.fallbackFontAssetTable = new List<TMP_FontAsset>();

        if (!font.fallbackFontAssetTable.Contains(fallback))
            font.fallbackFontAssetTable.Add(fallback);
    }

    private static void ApplyFontToText(TMP_Text text, TMP_FontAsset font)
    {
        if (text == null || text.font == font)
            return;

        text.font = font;
    }

    private static void PrepareFontForCurrentText(TMP_FontAsset font)
    {
        if (font == null || font.atlasPopulationMode != AtlasPopulationMode.Dynamic)
            return;

        string characters = CollectCurrentTextCharacters();
        if (string.IsNullOrEmpty(characters))
            return;

        font.TryAddCharacters(characters, out _);
    }

    private static string CollectCurrentTextCharacters()
    {
        HashSet<char> characters = new HashSet<char>();

        TMP_Text[] texts = FindObjectsOfType<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
            AddCharacters(characters, texts[i] != null ? texts[i].text : null);

        AddCharacters(characters, "\u65E5\u672C\u8A9E\u6226\u95D8\u8A2D\u5B9A\u9078\u629E\u78BA\u8A8D\u9589\u3058\u308B");

        if (characters.Count == 0)
            return string.Empty;

        char[] buffer = new char[characters.Count];
        characters.CopyTo(buffer);
        return new string(buffer);
    }

    private static void AddCharacters(HashSet<char> characters, string text)
    {
        if (characters == null || string.IsNullOrEmpty(text))
            return;

        for (int i = 0; i < text.Length; i++)
            characters.Add(text[i]);
    }

    private static int GetSavedLanguageIndex()
    {
        return RogueLikeData.Instance != null
            ? RogueLikeData.Instance.GetLanguage()
            : PlayerPrefs.GetInt("Language", 0);
    }
}
