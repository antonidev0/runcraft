using UnityEngine;

namespace RunCraft
{
    public enum OreType { Coal, Iron, Redstone, Lapis, Gold, Emerald, Diamond }
    public enum PowerType { None, Magnet, Boots }

    /// <summary>Mineral o power-up que se recoge al tocarlo.</summary>
    public class Pickup : MonoBehaviour
    {
        public OreType ore;
        public PowerType power = PowerType.None;
        public bool attracted;     // el imán lo está atrayendo
        float spin;

        public bool IsPower => power != PowerType.None;

        // Valor de cada mineral (cuánto suma a tus gemas)
        public static int Value(OreType t)
        {
            switch (t)
            {
                case OreType.Coal: return 1;
                case OreType.Iron: return 2;
                case OreType.Redstone: return 3;
                case OreType.Lapis: return 4;
                case OreType.Gold: return 5;
                case OreType.Emerald: return 8;
                case OreType.Diamond: return 10;
            }
            return 1;
        }

        public static Color OreColor(OreType t)
        {
            switch (t)
            {
                case OreType.Coal: return new Color(0.16f, 0.16f, 0.18f);
                case OreType.Iron: return new Color(0.85f, 0.72f, 0.62f);
                case OreType.Redstone: return new Color(0.9f, 0.08f, 0.06f);
                case OreType.Lapis: return new Color(0.12f, 0.28f, 0.85f);
                case OreType.Gold: return new Color(1f, 0.82f, 0.15f);
                case OreType.Emerald: return new Color(0.1f, 0.85f, 0.35f);
                case OreType.Diamond: return new Color(0.35f, 0.95f, 0.95f);
            }
            return Color.white;
        }

        /// <summary>Elige un mineral al azar: el carbón es común y el diamante muy raro.</summary>
        public static OreType RandomOre(System.Random r)
        {
            int n = r.Next(100);
            if (n < 34) return OreType.Coal;
            if (n < 58) return OreType.Iron;
            if (n < 72) return OreType.Redstone;
            if (n < 84) return OreType.Lapis;
            if (n < 93) return OreType.Gold;
            if (n < 97) return OreType.Emerald;
            return OreType.Diamond;
        }

        public static Pickup CreateOre(Transform parent, OreType t, Vector3 pos)
        {
            var go = new GameObject("Ore_" + t);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var p = go.AddComponent<Pickup>();
            p.ore = t;
            Color c = OreColor(t);
            // Gema: un cubo girado con un brillo claro encima
            var gem = Blocks.Part(go.transform, "Gem", Vector3.zero, new Vector3(0.42f, 0.42f, 0.42f), Mats.Solid(c), false);
            gem.transform.localRotation = Quaternion.Euler(45, 0, 45);
            Blocks.Part(gem.transform, "Shine", new Vector3(0.2f, 0.2f, -0.2f), new Vector3(0.35f, 0.35f, 0.35f),
                Mats.Solid(Color.Lerp(c, Color.white, 0.55f)), false);
            return p;
        }

        public static Pickup CreatePower(Transform parent, PowerType t, Vector3 pos)
        {
            var go = new GameObject("Power_" + t);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var p = go.AddComponent<Pickup>();
            p.power = t;
            var holder = Blocks.Pivot(go.transform, "Icon", Vector3.zero);
            if (t == PowerType.Magnet)
            {
                // Imán en forma de U: rojo con puntas plateadas
                Material red = Mats.Solid("#D32F2F");
                Material tip = Mats.Solid("#E0E0E0");
                Blocks.Part(holder, "Base", new Vector3(0, -0.3f, 0), new Vector3(0.8f, 0.22f, 0.22f), red);
                Blocks.Part(holder, "L", new Vector3(-0.3f, 0.0f, 0), new Vector3(0.22f, 0.5f, 0.22f), red);
                Blocks.Part(holder, "R", new Vector3(0.3f, 0.0f, 0), new Vector3(0.22f, 0.5f, 0.22f), red);
                Blocks.Part(holder, "TL", new Vector3(-0.3f, 0.32f, 0), new Vector3(0.22f, 0.16f, 0.22f), tip);
                Blocks.Part(holder, "TR", new Vector3(0.3f, 0.32f, 0), new Vector3(0.22f, 0.16f, 0.22f), tip);
            }
            else
            {
                // Bota azul con muelle
                Material blue = Mats.Solid("#1E88E5");
                Material sole = Mats.Solid("#FFEB3B");
                Blocks.Part(holder, "Shaft", new Vector3(-0.08f, 0.1f, 0), new Vector3(0.32f, 0.5f, 0.32f), blue);
                Blocks.Part(holder, "Foot", new Vector3(0.1f, -0.2f, 0), new Vector3(0.6f, 0.2f, 0.34f), blue);
                Blocks.Part(holder, "Spring", new Vector3(0.05f, -0.38f, 0), new Vector3(0.5f, 0.12f, 0.3f), sole);
            }
            // Aro dorado para que se vea que es especial
            Material ring = Mats.Solid("#FFD54F");
            Blocks.Part(go.transform, "RingT", new Vector3(0, 0.6f, 0), new Vector3(1.2f, 0.08f, 0.08f), ring, false);
            Blocks.Part(go.transform, "RingB", new Vector3(0, -0.6f, 0), new Vector3(1.2f, 0.08f, 0.08f), ring, false);
            Blocks.Part(go.transform, "RingL", new Vector3(-0.6f, 0, 0), new Vector3(0.08f, 1.2f, 0.08f), ring, false);
            Blocks.Part(go.transform, "RingR", new Vector3(0.6f, 0, 0), new Vector3(0.08f, 1.2f, 0.08f), ring, false);
            return p;
        }

        void Update()
        {
            spin += Time.deltaTime * 120f;
            transform.rotation = Quaternion.Euler(0, spin, 0);
        }
    }
}
