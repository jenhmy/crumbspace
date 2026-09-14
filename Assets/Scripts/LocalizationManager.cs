using System;
using System.Collections.Generic;

public static class LocalizationManager
{
    public static event Action OnLanguageChanged;

    private static readonly Dictionary<string, string[]> translations =
        new Dictionary<string, string[]>
        {
            { "PLAY", new[] { "JUGAR", "PLAY" } },
            { "OPTIONS", new[] { "OPCIONES", "OPTIONS" } },
            { "RETURN", new[] { "VOLVER", "RETURN" } },
            { "LEVEL", new[] { "NIVEL", "LVL" } },
            { "LEVEL_SCORE", new[] { "PUNTOS NIVEL", "LVL SCORE" } },
            { "TOTAL_SCORE", new[] { "\nPUNTOS TOTALES:", "\nTOTAL SCORE:" } },
            { "NEXT", new[] { "SIGUIENTE", "NEXT" } },
            { "FINAL_SCORE", new[] { "PUNTOS FINALES: ", "FINAL SCORE: " } },
            { "SAVE", new[] { "GUARDAR?", "SAVE?" } },

            { "OPTIONS_AUDIO_LANGUAGE", new[] {
                "SILENCIO:\nMUSICA:\nEFECTOS:\nIDIOMA:",
                "MUTE:\nMUSIC:\nSFX:\nLANGUAGE:"
            } },

            { "ENTER_NAME", new[] { "Nombre", "Name" } },
            { "LEVEL_SCORE_LABEL", new[] { "PUNTOS NIVEL {0}: ", "LVL {0} SCORE: " } },
            { "BONUS_SCORE", new[] { "NIVEL BONUS: ", "BONUS LVL: " } },

            { "DEATH_ZONE", new[] { "¡Te tostaste!", "You got toasted!" } },
            { "DEATH_ASTEROID", new[] { "¡No me rompas la corteza!", "Don't crack my crust!" } },

            { "LEVEL_COMPLETE_1", new[] {
                "¡Bah! Eso fue solo el aperitivo.",
                "Bah! That was just the appetizer."
            } },

            { "LEVEL_COMPLETE_2", new[] {
                "¡No! Aquí no se comparte el desayuno.",
                "No! We don't share breakfast here."
            } },

            { "LEVEL_COMPLETE_3", new[] {
                "¡Oh! Ese imán tenía mucha masa.",
                "Oh! That magnet had a lot of mass."
            } },

            { "LEVEL_COMPLETE_4", new[] {
                "Esto se está enfriando demasiado.",
                "Things are getting a little too cold."
            } },

            { "LEVEL_COMPLETE_5", new[] {
                "¡Ups! Eso dejó más migas de la cuenta.",
                "Oops! That left way too many crumbs."
            } },

            { "LEVEL_COMPLETE_6", new[] {
                "Esto se está poniendo crujiente.",
                "Things are getting crispy."
            } },

            { "LEVEL_COMPLETE_7", new[] {
                "Demasiado caos para tan poca miga.",
                "Too much chaos for so few crumbs."
            } },

            { "LEVEL_COMPLETE_ZERO", new[] {
                "Eso sí que fue una dieta estricta.",
                "Now that's what I call a strict diet."
            } },

            { "WIN_ZERO", new[] {
                "¿Cero? Menos mal que\nguardé algunos para emergencias.",
                "Zero? Good thing\nI saved a few for emergencies."
            } },

            { "WIN_NORMAL", new[] {
                "Una pequeña victoria para ti,\nmás croissants para mí.",
                "One small win for you,\nmore croissants for me!"
            } },

            { "WIN_HIGH", new[] {
                "No hacía falta vaciar la galaxia.\nCon unos pocos me bastaba.",
                "You didn't have to empty the whole galaxy.\nA few would've been enough."
            } },
        };

    public static string Get(string key)
    {
        if (string.IsNullOrEmpty(key) ||
            !translations.TryGetValue(key, out string[] values))
        {
            return key;
        }

        int languageIndex = (int)GameSettings.CurrentLanguage;
        if (languageIndex < 0 || languageIndex >= values.Length)
        {
            languageIndex = 0;
        }

        return values[languageIndex];
    }

    public static void RefreshAll()
    {
        OnLanguageChanged?.Invoke();
    }
}
