using TMPro;
using UnityEngine;

public class AutoLocalizedFont : MonoBehaviour
{
    private TMP_Text textComponent;
    private TMP_InputField inputField;
    private bool subscribedToEvent;

    private void Awake()
    {
        CacheComponents();
    }

    private void OnEnable()
    {
        CacheComponents();

        if (FontManager.Instance != null)
        {
            FontManager.Instance.Register(this);
            return;
        }

        FontManager.OnFontChanged += ApplyFont;
        subscribedToEvent = true;
    }

    private void OnDisable()
    {
        if (FontManager.Instance != null)
            FontManager.Instance.Unregister(this);

        if (subscribedToEvent)
        {
            FontManager.OnFontChanged -= ApplyFont;
            subscribedToEvent = false;
        }
    }

    public void ApplyFont(TMP_FontAsset newFont)
    {
        if (newFont == null)
            return;

        ApplyFontToText(textComponent, newFont);

        if (inputField == null)
            return;

        ApplyFontToText(inputField.textComponent, newFont);

        TMP_Text placeholderText = inputField.placeholder as TMP_Text;
        ApplyFontToText(placeholderText, newFont);
    }

    private void CacheComponents()
    {
        if (textComponent == null)
            textComponent = GetComponent<TMP_Text>();

        if (inputField == null)
            inputField = GetComponent<TMP_InputField>();
    }

    private static void ApplyFontToText(TMP_Text text, TMP_FontAsset font)
    {
        if (text == null || text.font == font)
            return;

        text.font = font;
    }
}
