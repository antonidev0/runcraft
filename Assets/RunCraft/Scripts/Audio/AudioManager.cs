using System.Collections.Generic;
using UnityEngine;

namespace RunCraft
{
    /// <summary>
    /// Música y efectos de sonido.
    /// Por defecto los sonidos se GENERAN por código (estilo 8 bits), así no hace falta descargar nada.
    /// Si quieres usar sonidos propios o descargados (por ejemplo de kenney.nl, licencia CC0),
    /// mete archivos .wav/.ogg en Assets/RunCraft/Resources/Audio con estos nombres y se usarán solos:
    ///   jump, roll, lane, ore, power, stumble, crash, roar, click, music
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager I { get; private set; }

        AudioSource sfx, music;
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        const int Rate = 22050;

        void Awake()
        {
            I = this;
            sfx = gameObject.AddComponent<AudioSource>();
            sfx.playOnAwake = false;
            music = gameObject.AddComponent<AudioSource>();
            music.loop = true;
            music.volume = 0.35f;
            music.playOnAwake = false;

            string[] names = { "jump", "roll", "lane", "ore", "power", "stumble", "crash", "roar", "click", "music" };
            foreach (var n in names)
            {
                var c = Resources.Load<AudioClip>("Audio/" + n);
                clips[n] = c != null ? c : Synth(n);
            }
            music.clip = clips["music"];
            ApplyMute();
        }

        public void ApplyMute()
        {
            bool on = Save.Sound;
            sfx.mute = !on;
            music.mute = !on;
        }

        public void PlayMusic()
        {
            if (!music.isPlaying) music.Play();
        }

        public void Play(string name, float pitch = 1f, float volume = 1f)
        {
            if (!clips.TryGetValue(name, out var c) || c == null) return;
            sfx.pitch = pitch;
            sfx.PlayOneShot(c, volume);
        }

        // ---------------- Sintetizador sencillo ----------------

        static float Square(float t, float f) => Mathf.Repeat(t * f, 1f) < 0.5f ? 1f : -1f;
        static float Tri(float t, float f) => 1f - 4f * Mathf.Abs(Mathf.Repeat(t * f + 0.25f, 1f) - 0.5f);

        static AudioClip Make(string name, float seconds, System.Func<float, float> wave)
        {
            int n = Mathf.CeilToInt(seconds * Rate);
            var data = new float[n];
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(wave(i / (float)Rate), -1f, 1f);
            var clip = AudioClip.Create(name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip Synth(string name)
        {
            var rnd = new System.Random(7);
            float Noise() => (float)rnd.NextDouble() * 2f - 1f;

            switch (name)
            {
                case "jump":
                    return Make(name, 0.22f, t => Square(t, Mathf.Lerp(300, 750, t / 0.22f)) * 0.25f * (1 - t / 0.22f));
                case "roll":
                    return Make(name, 0.25f, t => Noise() * 0.25f * (1 - t / 0.25f) * Mathf.Sin(t * 40));
                case "lane":
                    return Make(name, 0.1f, t => Noise() * 0.18f * (1 - t / 0.1f));
                case "ore":
                    return Make(name, 0.16f, t => Square(t, t < 0.06f ? 988 : 1319) * 0.18f * (1 - t / 0.16f));
                case "power":
                    return Make(name, 0.5f, t =>
                    {
                        float[] notes = { 523, 659, 784, 1047, 1319 };
                        int k = Mathf.Min(notes.Length - 1, (int)(t / 0.1f));
                        return Square(t, notes[k]) * 0.2f * (1 - t / 0.5f);
                    });
                case "stumble":
                    return Make(name, 0.25f, t => (Noise() * 0.4f + Square(t, 90) * 0.3f) * (1 - t / 0.25f));
                case "crash":
                    return Make(name, 0.6f, t => (Noise() * 0.5f + Square(t, Mathf.Lerp(160, 40, t / 0.6f)) * 0.4f) * (1 - t / 0.6f));
                case "roar":
                    return Make(name, 0.9f, t =>
                    {
                        float f = 70 + Mathf.Sin(t * 18) * 12;
                        float env = Mathf.Min(1, t * 8) * (1 - t / 0.9f);
                        return (Square(t, f) * 0.35f + Noise() * 0.15f) * env;
                    });
                case "click":
                    return Make(name, 0.05f, t => Square(t, 1200) * 0.15f * (1 - t / 0.05f));
                case "music":
                    return Music();
            }
            return null;
        }

        /// <summary>Melodía alegre en bucle (unos 13 segundos) con bajo y percusión.</summary>
        static AudioClip Music()
        {
            // Notas en semitonos respecto a La 440 (0 = silencio). Cada paso es una corchea.
            int[] lead =
            {
                3,0,7,0,10,0,7,0, 8,0,12,0,10,0,7,0,
                5,0,8,0,12,0,8,0, 7,0,10,0,7,0,3,5,
                3,0,7,0,10,0,7,0, 8,0,12,0,15,0,12,0,
                10,0,8,0,7,0,5,0, 3,0,0,0,3,7,10,0,
            };
            int[] bass = { 3, 3, 8, 8, 5, 5, 10, 10, 3, 3, 8, 8, 10, 10, 3, 3 };
            float step = 0.2f;
            float len = lead.Length * step;
            var rnd = new System.Random(3);

            float Freq(int semis) => 440f * Mathf.Pow(2f, (semis - 12) / 12f);

            return Make("music", len, t =>
            {
                int i = Mathf.Min(lead.Length - 1, (int)(t / step));
                float local = t - i * step;
                float v = 0;
                if (lead[i] != 0)
                    v += Square(t, Freq(lead[i] + 12)) * 0.11f * Mathf.Clamp01(1 - local / step * 0.8f);
                int b = bass[Mathf.Min(bass.Length - 1, (int)(t / (step * 4)))];
                v += Tri(t, Freq(b - 12)) * 0.22f;
                // percusión: bombo en cada negra, charles en las corcheas impares
                float beat = Mathf.Repeat(t, step * 2);
                if (beat < 0.05f) v += Mathf.Sin(beat * 2 * Mathf.PI * Mathf.Lerp(120, 50, beat / 0.05f)) * 0.3f;
                if (i % 2 == 1 && local < 0.02f) v += ((float)rnd.NextDouble() * 2 - 1) * 0.06f;
                return v;
            });
        }
    }
}
