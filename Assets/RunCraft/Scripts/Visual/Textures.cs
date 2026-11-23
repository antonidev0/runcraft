using System.Collections.Generic;
using UnityEngine;

namespace RunCraft
{
    /// <summary>
    /// Genera texturas pixeladas de 16x16 por código (arte propio, sin archivos externos).
    /// Cada textura se crea una sola vez y se reutiliza.
    /// </summary>
    public static class Textures
    {
        static readonly Dictionary<string, Texture2D> cache = new Dictionary<string, Texture2D>();
        const int S = 16;

        public static Texture2D Get(string name)
        {
            if (cache.TryGetValue(name, out var t)) return t;
            t = Build(name);
            cache[name] = t;
            return t;
        }

        static Color Hex(string h)
        {
            ColorUtility.TryParseHtmlString(h, out var c);
            return c;
        }

        // Elige uno de varios colores según un número aleatorio.
        static Color Pick(System.Random r, params string[] hexes) => Hex(hexes[r.Next(hexes.Length)]);

        static Texture2D Build(string name)
        {
            var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;   // aspecto pixelado
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.name = name;
            var r = new System.Random(name.GetHashCode());
            var px = new Color[S * S];

            for (int y = 0; y < S; y++)
            for (int x = 0; x < S; x++)
            {
                Color c;
                switch (name)
                {
                    case "grass":
                        c = Pick(r, "#5DA130", "#67AE38", "#559530", "#72B83F", "#4E8A2A");
                        break;
                    case "path":
                        c = Pick(r, "#9B7A4C", "#8C6B40", "#A88756", "#7E5F38");
                        if (r.NextDouble() < 0.06) c = Hex("#B7B2A8"); // piedrecitas
                        break;
                    case "dirt":
                        c = Pick(r, "#866043", "#795538", "#94704F", "#6B4A30");
                        break;
                    case "stone":
                        c = Pick(r, "#8A8A8A", "#7F7F7F", "#959595", "#747474");
                        break;
                    case "cobble":
                    {
                        // bloques de piedra irregulares con juntas oscuras
                        bool joint = (y % 5 == 0) || ((x + (y / 5) * 3) % 6 == 0);
                        c = joint ? Hex("#4F4F4F") : Pick(r, "#8E8E8E", "#7A7A7A", "#9C9C9C");
                        break;
                    }
                    case "mossy":
                    {
                        bool joint = (y % 5 == 0) || ((x + (y / 5) * 3) % 6 == 0);
                        c = joint ? Hex("#4F4F4F") : Pick(r, "#8E8E8E", "#7A7A7A", "#9C9C9C");
                        if (!joint && r.NextDouble() < 0.3) c = Pick(r, "#5B7F3A", "#4C6E30");
                        break;
                    }
                    case "log":
                        c = (x % 4 == 0) ? Hex("#4A3520") : Pick(r, "#6B4E2E", "#5E4327", "#765734");
                        break;
                    case "planks":
                        c = (y % 4 == 3) ? Hex("#6E5230") : Pick(r, "#B08C55", "#A3814D", "#BC975E");
                        if (y % 4 != 3 && (x + (y / 4) * 7) % 16 == 0) c = Hex("#6E5230");
                        break;
                    case "leaves":
                        c = Pick(r, "#3C7A23", "#2F6A1B", "#478A2B", "#285A16");
                        if (r.NextDouble() < 0.08) c = Hex("#1D4210");
                        break;
                    case "hay":
                        c = (y % 5 == 0) ? Hex("#8C6A1C") : Pick(r, "#D9B13B", "#C9A132", "#E6C04B");
                        break;
                    case "wool":
                        c = Pick(r, "#F2F2F2", "#E6E6E6", "#FFFFFF", "#DCDCDC");
                        break;
                    case "web":
                        c = ((x == y) || (x == S - 1 - y) || x == 8 || y == 8) ? Hex("#EDEDED") : new Color(1, 1, 1, 0);
                        break;
                    case "cloud":
                        c = Pick(r, "#FFFFFF", "#F4F7FA");
                        break;
                    default:
                        c = Color.magenta;
                        break;
                }
                px[y * S + x] = c;
            }
            tex.SetPixels(px);
            tex.Apply();
            return tex;
        }
    }
}
