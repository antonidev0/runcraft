using System.Collections.Generic;
using UnityEngine;

namespace RunCraft
{
    /// <summary>
    /// Textos del juego en español e inglés.
    /// Para añadir un texto nuevo: añade una línea en la tabla con la clave, el español y el inglés.
    /// </summary>
    public static class Loc
    {
        public static bool Spanish { get; private set; } = true;
        public static event System.Action Changed;

        static readonly Dictionary<string, string[]> table = new Dictionary<string, string[]>
        {
            // clave             español                          inglés
            { "play",          new[] { "JUGAR",                     "PLAY" } },
            { "tagline",       new[] { "¡Corre, salta y escapa del golem!", "Run, jump and escape the golem!" } },
            { "best",          new[] { "Récord",                    "Best" } },
            { "gems",          new[] { "Gemas",                     "Gems" } },
            { "lang",          new[] { "Idioma: Español",           "Language: English" } },
            { "sound_on",      new[] { "Sonido: Sí",                "Sound: On" } },
            { "sound_off",     new[] { "Sonido: No",                "Sound: Off" } },
            { "help",          new[] { "Desliza el dedo: ← → cambiar carril, ↑ saltar, ↓ agacharse",
                                       "Swipe: ← → change lane, ↑ jump, ↓ roll" } },
            { "help_pc",       new[] { "Teclado: flechas o WASD, Espacio salta, Esc pausa",
                                       "Keyboard: arrows or WASD, Space jumps, Esc pauses" } },
            { "paused",        new[] { "PAUSA",                     "PAUSED" } },
            { "resume",        new[] { "CONTINUAR",                 "RESUME" } },
            { "menu",          new[] { "MENÚ",                      "MENU" } },
            { "retry",         new[] { "OTRA VEZ",                  "TRY AGAIN" } },
            { "caught",        new[] { "¡El golem te atrapó!",      "The golem caught you!" } },
            { "crashed",       new[] { "¡Choque!",                  "Crash!" } },
            { "score",         new[] { "Puntos",                    "Score" } },
            { "new_best",      new[] { "¡NUEVO RÉCORD!",            "NEW BEST!" } },
            { "magnet",        new[] { "Imán",                      "Magnet" } },
            { "boots",         new[] { "Botas de salto",            "Jump boots" } },
            { "ores_got",      new[] { "Gemas recogidas",           "Gems collected" } },
            { "distance",      new[] { "Distancia",                 "Distance" } },
            { "watch_out",     new[] { "¡Cuidado!",                 "Watch out!" } },
        };

        public static void Init()
        {
            if (PlayerPrefs.HasKey("lang")) Spanish = PlayerPrefs.GetInt("lang") == 0;
            else Spanish = Application.systemLanguage == SystemLanguage.Spanish
                           || Application.systemLanguage == SystemLanguage.Catalan
                           || Application.systemLanguage == SystemLanguage.Basque;
        }

        public static void Toggle()
        {
            Spanish = !Spanish;
            PlayerPrefs.SetInt("lang", Spanish ? 0 : 1);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        /// <summary>Devuelve el texto de la clave en el idioma actual.</summary>
        public static string T(string key)
        {
            if (table.TryGetValue(key, out var v)) return Spanish ? v[0] : v[1];
            return key;
        }
    }

    /// <summary>Datos guardados en el móvil (récord, gemas totales, sonido).</summary>
    public static class Save
    {
        public static int Best
        {
            get => PlayerPrefs.GetInt("best", 0);
            set { PlayerPrefs.SetInt("best", value); PlayerPrefs.Save(); }
        }

        public static int TotalGems
        {
            get => PlayerPrefs.GetInt("gems", 0);
            set { PlayerPrefs.SetInt("gems", value); PlayerPrefs.Save(); }
        }

        public static bool Sound
        {
            get => PlayerPrefs.GetInt("sound", 1) == 1;
            set { PlayerPrefs.SetInt("sound", value ? 1 : 0); PlayerPrefs.Save(); }
        }
    }
}
