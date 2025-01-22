using UnityEngine;

public class LanguageManager
{
    private static int language = 0;

    // 언어 값을 반환하는 정적 메서드
    public static int GetLanguage()
    {
        return language;
    }

    // 언어 값을 설정하는 정적 메서드
    public static void SetLanguage(int newLanguage)
    {
        language = newLanguage;
    }
}
