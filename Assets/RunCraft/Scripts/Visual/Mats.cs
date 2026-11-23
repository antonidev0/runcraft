using System.Collections.Generic;
using UnityEngine;

namespace RunCraft
{
    /// <summary>
    /// Crea y guarda materiales para no repetirlos (mejor rendimiento en móvil).
    /// Todos parten de "RunCraftBaseMat" (Resources), que usa el shader URP Lit.
    /// </summary>
    public static class Mats
    {
        static Material baseMat;
        static readonly Dictionary<string, Material> cache = new Dictionary<string, Material>();


        // Unity 6 puede entrar en Play sin recargar los scripts, así que los datos "static"
        // sobreviven entre partidas de prueba aunque los objetos ya se hayan destruido.
        // Por eso vaciamos la caché al empezar cada Play.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            cache.Clear();
            baseMat = null;
        }

        static Material Base
        {
            get
            {
                if (baseMat != null) return baseMat;
                baseMat = Resources.Load<Material>("RunCraftBaseMat");
                if (baseMat == null)
                {
                    // Plan B: si el material no existe todavía, lo creamos desde el shader.
                    Shader s = Shader.Find("Universal Render Pipeline/Lit");
                    if (s == null) s = Shader.Find("Universal Render Pipeline/Simple Lit");
                    if (s == null) s = Shader.Find("Standard");
                    baseMat = new Material(s);
                }
                return baseMat;
            }
        }

        static Material Make(string key, Color color, Texture2D tex)
        {
            var m = new Material(Base);
            m.name = key;
            m.color = color;
            m.mainTexture = tex;
            m.enableInstancing = true;
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.05f);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.05f);
            return m;
        }

        /// <summary>Material de un solo color.</summary>
        public static Material Solid(Color c)
        {
            string key = "c" + ColorUtility.ToHtmlStringRGBA(c);
            if (!cache.TryGetValue(key, out var m) || m == null)
            {
                m = Make(key, c, null);
                cache[key] = m;
            }
            return m;
        }

        public static Material Solid(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return Solid(c);
        }

        /// <summary>Material con una textura pixelada generada por código (ver Textures).</summary>
        public static Material Tex(string textureName)
        {
            string key = "t" + textureName;
            if (!cache.TryGetValue(key, out var m) || m == null)
            {
                m = Make(key, UnityEngine.Color.white, Textures.Get(textureName));
                cache[key] = m;
            }
            return m;
        }
    }
}
