using UnityEngine;

public enum GameLanguage
{
    Spanish,
    English
}

public static class GameSettings
{
    private const string LanguageKey = "Game.Language";

    public static GameLanguage CurrentLanguage
    {
        get
        {
            return (GameLanguage)PlayerPrefs.GetInt(
                LanguageKey,
                (int)GameLanguage.English);
        }
    }

    public static void SetLanguage(GameLanguage language)
    {
        PlayerPrefs.SetInt(LanguageKey, (int)language);
        PlayerPrefs.Save();
    }
}
