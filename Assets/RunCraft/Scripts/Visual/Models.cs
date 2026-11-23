using UnityEngine;

namespace RunCraft
{
    /// <summary>
    /// Modelo humanoide hecho de bloques con brazos y piernas que se balancean.
    /// Lo usan Toni, el golem, los aldeanos y los zombis.
    /// </summary>
    public class HumanoidModel : MonoBehaviour
    {
        public Transform body;      // todo lo que se inclina o rueda
        public Transform head;
        public Transform armL, armR, legL, legR;
        public float swing = 50f;   // grados máximos de balanceo
        public bool armsForward;    // los zombis caminan con los brazos estirados

        float phase;

        /// <summary>Anima la carrera. speed = rapidez del ciclo de pasos.</summary>
        public void Run(float dt, float speed)
        {
            phase += dt * speed;
            float s = Mathf.Sin(phase) * swing;
            if (legL) legL.localRotation = Quaternion.Euler(s, 0, 0);
            if (legR) legR.localRotation = Quaternion.Euler(-s, 0, 0);
            float armBase = armsForward ? -85f : 0f;
            float armAmp = armsForward ? 0.2f : 1f;
            if (armL) armL.localRotation = Quaternion.Euler(armBase - s * armAmp, 0, 0);
            if (armR) armR.localRotation = Quaternion.Euler(armBase + s * armAmp, 0, 0);
        }

        /// <summary>Pose quieta (respirando un poco).</summary>
        public void Idle(float dt)
        {
            phase += dt * 2f;
            float s = Mathf.Sin(phase) * 3f;
            if (legL) legL.localRotation = Quaternion.identity;
            if (legR) legR.localRotation = Quaternion.identity;
            float armBase = armsForward ? -85f : 0f;
            if (armL) armL.localRotation = Quaternion.Euler(armBase + s, 0, -3);
            if (armR) armR.localRotation = Quaternion.Euler(armBase - s, 0, 3);
        }

        /// <summary>Brazos en alto (para el golem cuando te atrapa).</summary>
        public void ArmsUp()
        {
            if (armL) armL.localRotation = Quaternion.Euler(-160, 0, -10);
            if (armR) armR.localRotation = Quaternion.Euler(-160, 0, 10);
        }

        // ---------- Constructores de personajes ----------

        static Transform Limb(Transform parent, string name, Vector3 pivotPos, Vector3 size, Material mat)
        {
            var p = Blocks.Pivot(parent, name, pivotPos);
            Blocks.Part(p, name + "Mesh", new Vector3(0, -size.y / 2f, 0), size, mat);
            return p;
        }

        /// <summary>
        /// Toni: el protagonista. Pelo castaño, camiseta naranja con franja,
        /// vaqueros, zapatillas y una pequeña mochila verde. Mide unos 1.9 m.
        /// </summary>
        public static HumanoidModel BuildToni(Transform parent)
        {
            var root = new GameObject("Toni").transform;
            root.SetParent(parent, false);
            var m = root.gameObject.AddComponent<HumanoidModel>();

            Material skin = Mats.Solid("#E0A57A");
            Material hair = Mats.Solid("#5A3A1E");
            Material shirt = Mats.Solid("#F07A1E");
            Material stripe = Mats.Solid("#FFFFFF");
            Material jeans = Mats.Solid("#2F5FA8");
            Material shoes = Mats.Solid("#2B2B2B");
            Material bag = Mats.Solid("#3E8E41");
            Material white = Mats.Solid("#FFFFFF");
            Material eye = Mats.Solid("#2A6FDB");
            Material mouth = Mats.Solid("#9C4A3A");

            m.body = Blocks.Pivot(root, "Body", Vector3.zero);
            Transform b = m.body;

            // Piernas (giran desde la cadera)
            m.legL = Limb(b, "LegL", new Vector3(-0.13f, 0.72f, 0), new Vector3(0.25f, 0.72f, 0.27f), jeans);
            m.legR = Limb(b, "LegR", new Vector3(0.13f, 0.72f, 0), new Vector3(0.25f, 0.72f, 0.27f), jeans);
            Blocks.Part(m.legL, "ShoeL", new Vector3(0, -0.66f, 0.03f), new Vector3(0.27f, 0.12f, 0.33f), shoes);
            Blocks.Part(m.legR, "ShoeR", new Vector3(0, -0.66f, 0.03f), new Vector3(0.27f, 0.12f, 0.33f), shoes);

            // Torso
            Blocks.Part(b, "Torso", new Vector3(0, 1.07f, 0), new Vector3(0.52f, 0.72f, 0.3f), shirt);
            Blocks.Part(b, "Stripe", new Vector3(0, 1.12f, 0.152f), new Vector3(0.52f, 0.1f, 0.01f), stripe, false);
            Blocks.Part(b, "Backpack", new Vector3(0, 1.08f, -0.24f), new Vector3(0.4f, 0.48f, 0.2f), bag);

            // Brazos (giran desde el hombro)
            m.armL = Limb(b, "ArmL", new Vector3(-0.37f, 1.4f, 0), new Vector3(0.22f, 0.7f, 0.24f), shirt);
            m.armR = Limb(b, "ArmR", new Vector3(0.37f, 1.4f, 0), new Vector3(0.22f, 0.7f, 0.24f), shirt);
            Blocks.Part(m.armL, "HandL", new Vector3(0, -0.6f, 0), new Vector3(0.23f, 0.2f, 0.25f), skin);
            Blocks.Part(m.armR, "HandR", new Vector3(0, -0.6f, 0), new Vector3(0.23f, 0.2f, 0.25f), skin);

            // Cabeza con cara
            m.head = Blocks.Pivot(b, "Head", new Vector3(0, 1.43f, 0));
            Transform h = m.head;
            Blocks.Part(h, "Skull", new Vector3(0, 0.25f, 0), new Vector3(0.5f, 0.5f, 0.5f), skin);
            Blocks.Part(h, "HairTop", new Vector3(0, 0.51f, -0.01f), new Vector3(0.54f, 0.1f, 0.54f), hair);
            Blocks.Part(h, "HairBack", new Vector3(0, 0.3f, -0.26f), new Vector3(0.54f, 0.42f, 0.06f), hair);
            Blocks.Part(h, "Fringe", new Vector3(0.06f, 0.44f, 0.26f), new Vector3(0.42f, 0.08f, 0.04f), hair, false);
            Blocks.Part(h, "EyeWL", new Vector3(-0.12f, 0.3f, 0.252f), new Vector3(0.12f, 0.07f, 0.01f), white, false);
            Blocks.Part(h, "EyeWR", new Vector3(0.12f, 0.3f, 0.252f), new Vector3(0.12f, 0.07f, 0.01f), white, false);
            Blocks.Part(h, "EyeL", new Vector3(-0.09f, 0.3f, 0.256f), new Vector3(0.06f, 0.07f, 0.01f), eye, false);
            Blocks.Part(h, "EyeR", new Vector3(0.09f, 0.3f, 0.256f), new Vector3(0.06f, 0.07f, 0.01f), eye, false);
            Blocks.Part(h, "Mouth", new Vector3(0, 0.13f, 0.252f), new Vector3(0.16f, 0.04f, 0.01f), mouth, false);
            return m;
        }

        /// <summary>Golem de piedra: grande, con musgo, ojos ámbar y brazos largos. Mide unos 3 m.</summary>
        public static HumanoidModel BuildGolem(Transform parent)
        {
            var root = new GameObject("Golem").transform;
            root.SetParent(parent, false);
            var m = root.gameObject.AddComponent<HumanoidModel>();
            m.swing = 35f;
            Material stone = Mats.Tex("mossy");
            Material dark = Mats.Solid("#5E5E5E");
            Material eyes = Mats.Solid("#FFB020");
            Material moss = Mats.Solid("#4C7A2E");

            m.body = Blocks.Pivot(root, "Body", Vector3.zero);
            Transform b = m.body;
            m.legL = Limb(b, "LegL", new Vector3(-0.32f, 1.0f, 0), new Vector3(0.5f, 1.0f, 0.55f), stone);
            m.legR = Limb(b, "LegR", new Vector3(0.32f, 1.0f, 0), new Vector3(0.5f, 1.0f, 0.55f), stone);
            Blocks.Part(b, "Chest", new Vector3(0, 1.65f, 0), new Vector3(1.3f, 0.9f, 0.75f), stone);
            Blocks.Part(b, "Waist", new Vector3(0, 1.1f, 0), new Vector3(0.9f, 0.3f, 0.55f), dark);
            Blocks.Part(b, "Moss", new Vector3(-0.3f, 1.95f, 0.38f), new Vector3(0.4f, 0.25f, 0.02f), moss, false);
            m.armL = Limb(b, "ArmL", new Vector3(-0.85f, 2.0f, 0), new Vector3(0.42f, 1.5f, 0.45f), stone);
            m.armR = Limb(b, "ArmR", new Vector3(0.85f, 2.0f, 0), new Vector3(0.42f, 1.5f, 0.45f), stone);
            m.head = Blocks.Pivot(b, "Head", new Vector3(0, 2.1f, 0.05f));
            Blocks.Part(m.head, "Skull", new Vector3(0, 0.3f, 0), new Vector3(0.62f, 0.6f, 0.6f), stone);
            Blocks.Part(m.head, "Brow", new Vector3(0, 0.44f, 0.31f), new Vector3(0.62f, 0.1f, 0.04f), dark, false);
            Blocks.Part(m.head, "EyeL", new Vector3(-0.15f, 0.34f, 0.31f), new Vector3(0.12f, 0.08f, 0.02f), eyes, false);
            Blocks.Part(m.head, "EyeR", new Vector3(0.15f, 0.34f, 0.31f), new Vector3(0.12f, 0.08f, 0.02f), eyes, false);
            Blocks.Part(m.head, "Nose", new Vector3(0, 0.2f, 0.34f), new Vector3(0.12f, 0.2f, 0.1f), dark);
            return m;
        }

        /// <summary>Aldeano: túnica marrón, cabeza alargada y nariz grande. Bloquea el carril.</summary>
        public static HumanoidModel BuildVillager(Transform parent)
        {
            var root = new GameObject("Villager").transform;
            root.SetParent(parent, false);
            var m = root.gameObject.AddComponent<HumanoidModel>();
            m.swing = 25f;
            Material robe = Mats.Solid("#7A5634");
            Material skin = Mats.Solid("#C78F6A");
            Material dark = Mats.Solid("#4A3120");
            Material eye = Mats.Solid("#2E7D32");

            m.body = Blocks.Pivot(root, "Body", Vector3.zero);
            Transform b = m.body;
            m.legL = Limb(b, "LegL", new Vector3(-0.13f, 0.6f, 0), new Vector3(0.25f, 0.6f, 0.27f), dark);
            m.legR = Limb(b, "LegR", new Vector3(0.13f, 0.6f, 0), new Vector3(0.25f, 0.6f, 0.27f), dark);
            Blocks.Part(b, "Robe", new Vector3(0, 1.0f, 0), new Vector3(0.56f, 0.9f, 0.36f), robe);
            Blocks.Part(b, "Arms", new Vector3(0, 1.1f, 0.24f), new Vector3(0.6f, 0.22f, 0.22f), robe);
            Blocks.Part(b, "Hands", new Vector3(0, 1.1f, 0.36f), new Vector3(0.28f, 0.2f, 0.04f), skin, false);
            m.head = Blocks.Pivot(b, "Head", new Vector3(0, 1.45f, 0));
            Blocks.Part(m.head, "Skull", new Vector3(0, 0.3f, 0), new Vector3(0.48f, 0.6f, 0.48f), skin);
            Blocks.Part(m.head, "Brow", new Vector3(0, 0.42f, 0.245f), new Vector3(0.44f, 0.06f, 0.01f), dark, false);
            Blocks.Part(m.head, "EyeL", new Vector3(-0.11f, 0.34f, 0.245f), new Vector3(0.08f, 0.07f, 0.01f), eye, false);
            Blocks.Part(m.head, "EyeR", new Vector3(0.11f, 0.34f, 0.245f), new Vector3(0.08f, 0.07f, 0.01f), eye, false);
            Blocks.Part(m.head, "Nose", new Vector3(0, 0.2f, 0.3f), new Vector3(0.12f, 0.24f, 0.12f), skin);
            return m;
        }

        /// <summary>Zombi: piel verde grisácea, ropa raída. Camina hacia ti con los brazos estirados.</summary>
        public static HumanoidModel BuildZombie(Transform parent)
        {
            var root = new GameObject("Zombie").transform;
            root.SetParent(parent, false);
            var m = root.gameObject.AddComponent<HumanoidModel>();
            m.swing = 35f;
            m.armsForward = true;
            Material skin = Mats.Solid("#6E9A5B");
            Material shirt = Mats.Solid("#2F6F6A");
            Material pants = Mats.Solid("#4B4B63");
            Material eye = Mats.Solid("#1B1B1B");

            m.body = Blocks.Pivot(root, "Body", Vector3.zero);
            Transform b = m.body;
            m.legL = Limb(b, "LegL", new Vector3(-0.13f, 0.72f, 0), new Vector3(0.25f, 0.72f, 0.27f), pants);
            m.legR = Limb(b, "LegR", new Vector3(0.13f, 0.72f, 0), new Vector3(0.25f, 0.72f, 0.27f), pants);
            Blocks.Part(b, "Torso", new Vector3(0, 1.07f, 0), new Vector3(0.52f, 0.72f, 0.3f), shirt);
            m.armL = Limb(b, "ArmL", new Vector3(-0.37f, 1.4f, 0), new Vector3(0.22f, 0.7f, 0.24f), skin);
            m.armR = Limb(b, "ArmR", new Vector3(0.37f, 1.4f, 0), new Vector3(0.22f, 0.7f, 0.24f), skin);
            m.head = Blocks.Pivot(b, "Head", new Vector3(0, 1.43f, 0));
            Blocks.Part(m.head, "Skull", new Vector3(0, 0.25f, 0), new Vector3(0.5f, 0.5f, 0.5f), skin);
            Blocks.Part(m.head, "EyeL", new Vector3(-0.11f, 0.3f, 0.252f), new Vector3(0.1f, 0.06f, 0.01f), eye, false);
            Blocks.Part(m.head, "EyeR", new Vector3(0.11f, 0.3f, 0.252f), new Vector3(0.1f, 0.06f, 0.01f), eye, false);
            Blocks.Part(m.head, "Mouth", new Vector3(0, 0.12f, 0.252f), new Vector3(0.22f, 0.05f, 0.01f), eye, false);
            return m;
        }
    }

    /// <summary>Animal de cuatro patas (cerdo, oveja) o araña de ocho patas.</summary>
    public class CritterModel : MonoBehaviour
    {
        public Transform[] legs;
        public float swing = 30f, rate = 8f;
        public bool bob;          // la araña sube y baja colgada de su hilo
        float phase, baseY;

        void Start() { baseY = transform.localPosition.y; phase = Random.value * 10f; }

        void Update()
        {
            phase += Time.deltaTime * rate;
            if (legs != null)
                for (int i = 0; i < legs.Length; i++)
                    legs[i].localRotation = Quaternion.Euler(Mathf.Sin(phase + i * 1.7f) * swing, 0, 0);
            if (bob)
            {
                var p = transform.localPosition;
                p.y = baseY + Mathf.Sin(phase * 0.3f) * 0.08f;
                transform.localPosition = p;
            }
        }

        static Transform Leg(Transform parent, Vector3 pivot, Vector3 size, Material mat)
        {
            var p = Blocks.Pivot(parent, "Leg", pivot);
            Blocks.Part(p, "LegMesh", new Vector3(0, -size.y / 2f, 0), size, mat);
            return p;
        }

        /// <summary>Cerdo rosa. Mide 1 m de alto: hay que saltarlo.</summary>
        public static CritterModel BuildPig(Transform parent)
        {
            var root = new GameObject("Pig").transform;
            root.SetParent(parent, false);
            var c = root.gameObject.AddComponent<CritterModel>();
            c.rate = 3f; c.swing = 10f;
            Material pink = Mats.Solid("#F0A0A8");
            Material snout = Mats.Solid("#E07F8B");
            Material eye = Mats.Solid("#222222");
            Blocks.Part(root, "Body", new Vector3(0, 0.6f, 0), new Vector3(0.9f, 0.7f, 1.2f), pink);
            Blocks.Part(root, "Head", new Vector3(0, 0.75f, -0.75f), new Vector3(0.65f, 0.6f, 0.45f), pink);
            Blocks.Part(root, "Snout", new Vector3(0, 0.66f, -1.0f), new Vector3(0.32f, 0.22f, 0.08f), snout);
            Blocks.Part(root, "EyeL", new Vector3(-0.17f, 0.86f, -0.98f), new Vector3(0.08f, 0.08f, 0.02f), eye, false);
            Blocks.Part(root, "EyeR", new Vector3(0.17f, 0.86f, -0.98f), new Vector3(0.08f, 0.08f, 0.02f), eye, false);
            c.legs = new[]
            {
                Leg(root, new Vector3(-0.28f, 0.3f, -0.4f), new Vector3(0.25f, 0.3f, 0.25f), pink),
                Leg(root, new Vector3(0.28f, 0.3f, -0.4f), new Vector3(0.25f, 0.3f, 0.25f), pink),
                Leg(root, new Vector3(-0.28f, 0.3f, 0.4f), new Vector3(0.25f, 0.3f, 0.25f), pink),
                Leg(root, new Vector3(0.28f, 0.3f, 0.4f), new Vector3(0.25f, 0.3f, 0.25f), pink),
            };
            return c;
        }

        /// <summary>Oveja con lana blanca y cara gris. También hay que saltarla.</summary>
        public static CritterModel BuildSheep(Transform parent)
        {
            var root = new GameObject("Sheep").transform;
            root.SetParent(parent, false);
            var c = root.gameObject.AddComponent<CritterModel>();
            c.rate = 3f; c.swing = 10f;
            Material wool = Mats.Tex("wool");
            Material face = Mats.Solid("#8F8478");
            Material eye = Mats.Solid("#222222");
            Blocks.Part(root, "Body", new Vector3(0, 0.65f, 0), new Vector3(0.95f, 0.75f, 1.25f), wool);
            Blocks.Part(root, "Head", new Vector3(0, 0.85f, -0.75f), new Vector3(0.5f, 0.5f, 0.45f), face);
            Blocks.Part(root, "WoolCap", new Vector3(0, 1.08f, -0.72f), new Vector3(0.54f, 0.1f, 0.4f), wool);
            Blocks.Part(root, "EyeL", new Vector3(-0.14f, 0.9f, -0.98f), new Vector3(0.08f, 0.07f, 0.02f), eye, false);
            Blocks.Part(root, "EyeR", new Vector3(0.14f, 0.9f, -0.98f), new Vector3(0.08f, 0.07f, 0.02f), eye, false);
            c.legs = new[]
            {
                Leg(root, new Vector3(-0.28f, 0.3f, -0.4f), new Vector3(0.2f, 0.3f, 0.2f), face),
                Leg(root, new Vector3(0.28f, 0.3f, -0.4f), new Vector3(0.2f, 0.3f, 0.2f), face),
                Leg(root, new Vector3(-0.28f, 0.3f, 0.4f), new Vector3(0.2f, 0.3f, 0.2f), face),
                Leg(root, new Vector3(0.28f, 0.3f, 0.4f), new Vector3(0.2f, 0.3f, 0.2f), face),
            };
            return c;
        }

        /// <summary>Araña negra con ojos rojos. Cuelga de un tronco: hay que agacharse.</summary>
        public static CritterModel BuildSpider(Transform parent)
        {
            var root = new GameObject("Spider").transform;
            root.SetParent(parent, false);
            var c = root.gameObject.AddComponent<CritterModel>();
            c.rate = 6f; c.swing = 18f; c.bob = true;
            Material body = Mats.Solid("#2B2522");
            Material leg = Mats.Solid("#3A322E");
            Material eye = Mats.Solid("#E0231E");
            Blocks.Part(root, "Abdomen", new Vector3(0, 0.0f, 0.35f), new Vector3(0.9f, 0.6f, 0.8f), body);
            Blocks.Part(root, "Head", new Vector3(0, 0.0f, -0.25f), new Vector3(0.6f, 0.5f, 0.5f), body);
            Blocks.Part(root, "EyeL", new Vector3(-0.15f, 0.06f, -0.51f), new Vector3(0.1f, 0.08f, 0.02f), eye, false);
            Blocks.Part(root, "EyeR", new Vector3(0.15f, 0.06f, -0.51f), new Vector3(0.1f, 0.08f, 0.02f), eye, false);
            Blocks.Part(root, "EyeL2", new Vector3(-0.08f, 0.16f, -0.51f), new Vector3(0.06f, 0.05f, 0.02f), eye, false);
            Blocks.Part(root, "EyeR2", new Vector3(0.08f, 0.16f, -0.51f), new Vector3(0.06f, 0.05f, 0.02f), eye, false);
            var legs = new Transform[8];
            for (int i = 0; i < 4; i++)
            {
                float z = -0.2f + i * 0.17f;
                var l = Blocks.Pivot(root, "LegL" + i, new Vector3(-0.3f, 0, z));
                Blocks.Part(l, "M", new Vector3(-0.45f, -0.15f, 0), new Vector3(0.9f, 0.08f, 0.08f), leg);
                l.localRotation = Quaternion.Euler(0, 0, -20);
                var r = Blocks.Pivot(root, "LegR" + i, new Vector3(0.3f, 0, z));
                Blocks.Part(r, "M", new Vector3(0.45f, -0.15f, 0), new Vector3(0.9f, 0.08f, 0.08f), leg);
                legs[i * 2] = l; legs[i * 2 + 1] = r;
            }
            c.legs = legs;
            return c;
        }
    }
}
