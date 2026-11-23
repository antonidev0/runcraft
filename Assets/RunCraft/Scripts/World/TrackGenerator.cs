using System.Collections.Generic;
using UnityEngine;

namespace RunCraft
{
    /// <summary>
    /// Crea la pradera infinita por trozos: suelo, caminos, árboles, vallas, nubes,
    /// filas de obstáculos (mobs y bloques), minerales y power-ups.
    /// Lo que queda detrás del jugador se borra.
    /// </summary>
    public class TrackGenerator : MonoBehaviour
    {
        const float ChunkLength = 40f;
        const float ViewAhead = 150f;
        const float SpawnAhead = 120f;
        const float FirstRowZ = 45f;

        public readonly List<Obstacle> Obstacles = new List<Obstacle>();
        public readonly List<Pickup> Pickups = new List<Pickup>();
        readonly List<Transform> chunks = new List<Transform>();

        Transform worldRoot;
        float nextChunkZ, nextRowZ, lastPowerZ;
        System.Random rng;

        static float LaneX(int lane) => lane * PlayerController.LaneWidth;

        public void ResetTrack()
        {
            if (worldRoot != null) Destroy(worldRoot.gameObject);
            worldRoot = new GameObject("World").transform;
            worldRoot.SetParent(transform, false);
            Obstacles.Clear();
            Pickups.Clear();
            chunks.Clear();
            rng = new System.Random();
            nextChunkZ = -40f;
            nextRowZ = FirstRowZ;
            lastPowerZ = 0f;
            while (nextChunkZ < ViewAhead) BuildChunk(nextChunkZ);
        }

        public void Tick(float dt, float playerZ, float difficulty, bool spawnRows)
        {
            while (nextChunkZ < playerZ + ViewAhead) BuildChunk(nextChunkZ);
            if (spawnRows)
                while (nextRowZ < playerZ + SpawnAhead) SpawnRow(nextRowZ, difficulty);

            foreach (var o in Obstacles) o.Tick(dt);

            // Borrar lo que ya quedó atrás
            for (int i = Obstacles.Count - 1; i >= 0; i--)
                if (Obstacles[i].zMax < playerZ - 15f) { Destroy(Obstacles[i].gameObject); Obstacles.RemoveAt(i); }
            for (int i = Pickups.Count - 1; i >= 0; i--)
                if (Pickups[i] == null) Pickups.RemoveAt(i);
                else if (Pickups[i].transform.position.z < playerZ - 10f) { Destroy(Pickups[i].gameObject); Pickups.RemoveAt(i); }
            for (int i = chunks.Count - 1; i >= 0; i--)
                if (chunks[i].position.z + ChunkLength < playerZ - 30f) { Destroy(chunks[i].gameObject); chunks.RemoveAt(i); }
        }

        /// <summary>
        /// Altura del suelo bajo el jugador. Solo cuenta superficies que estén por debajo de maxY
        /// (así un muro alto no te "teletransporta" encima).
        /// </summary>
        public float GroundAt(float x, float halfW, float z, float halfD, float maxY)
        {
            float g = 0f;
            foreach (var o in Obstacles)
            {
                if (Mathf.Abs(x - o.x) >= halfW + o.halfWidth) continue;
                if (z + halfD <= o.zMin || z - halfD >= o.zMax) continue;
                float top = o.IsRamp ? o.TopAt(z + halfD) : o.yMax;
                if (top <= maxY && top > g) g = top;
            }
            return g;
        }

        // ---------------------------------------------------------------
        // Escenario
        // ---------------------------------------------------------------

        void BuildChunk(float z0)
        {
            var chunk = new GameObject("Chunk").transform;
            chunk.SetParent(worldRoot, false);
            chunk.position = new Vector3(0, 0, z0);
            chunks.Add(chunk);
            float L = ChunkLength;
            float mid = L / 2f;

            // Hierba (debajo de todo) y tierra a los lados
            Blocks.Tiled(chunk, "Grass", new Vector3(0, -0.5f, mid), new Vector3(60f, 1f, L), Mats.Tex("grass"), false);
            // Caminos de tierra, uno por carril
            for (int lane = -1; lane <= 1; lane++)
                Blocks.Tiled(chunk, "Path", new Vector3(LaneX(lane), -0.48f, mid), new Vector3(2.1f, 1f, L), Mats.Tex("path"), false);

            // Vallas de madera a los dos lados
            Material wood = Mats.Tex("planks");
            for (int side = -1; side <= 1; side += 2)
            {
                float fx = side * 4.3f;
                for (float z = 2f; z < L; z += 4f)
                    Blocks.Part(chunk, "Post", new Vector3(fx, 0.5f, z), new Vector3(0.2f, 1f, 0.2f), wood);
                Blocks.Tiled(chunk, "Rail", new Vector3(fx, 0.75f, mid), new Vector3(0.12f, 0.14f, L), wood);
                Blocks.Tiled(chunk, "Rail2", new Vector3(fx, 0.4f, mid), new Vector3(0.12f, 0.14f, L), wood);
            }

            // Árboles, flores y algún bloque suelto
            int trees = 4 + rng.Next(4);
            for (int i = 0; i < trees; i++)
            {
                float side = rng.Next(2) == 0 ? -1 : 1;
                float x = side * (6.5f + (float)rng.NextDouble() * 14f);
                float z = (float)rng.NextDouble() * L;
                BuildTree(chunk, new Vector3(x, 0, z));
            }
            string[] flowers = { "#E53935", "#FDD835", "#FFFFFF", "#8E24AA", "#1E88E5" };
            for (int i = 0; i < 14; i++)
            {
                float side = rng.Next(2) == 0 ? -1 : 1;
                float x = side * (4.8f + (float)rng.NextDouble() * 18f);
                float z = (float)rng.NextDouble() * L;
                Blocks.Part(chunk, "Stem", new Vector3(x, 0.15f, z), new Vector3(0.06f, 0.3f, 0.06f), Mats.Solid("#2E7D32"), false);
                Blocks.Part(chunk, "Flower", new Vector3(x, 0.34f, z), new Vector3(0.16f, 0.12f, 0.16f), Mats.Solid(flowers[rng.Next(flowers.Length)]), false);
            }
            if (rng.Next(3) == 0)
            {
                float side = rng.Next(2) == 0 ? -1 : 1;
                Blocks.Tiled(chunk, "Rock", new Vector3(side * (8f + rng.Next(10)), 0.5f, rng.Next((int)L)), Vector3.one, Mats.Tex("cobble"));
            }

            // Nubes
            if (rng.Next(2) == 0)
            {
                float cx = -30f + (float)rng.NextDouble() * 60f;
                Blocks.Part(chunk, "Cloud", new Vector3(cx, 28f + rng.Next(8), mid), new Vector3(10f + rng.Next(8), 2f, 6f + rng.Next(6)), Mats.Solid("#FFFFFF"), false);
            }

            nextChunkZ = z0 + L;
        }

        void BuildTree(Transform parent, Vector3 p)
        {
            int h = 3 + rng.Next(3);
            var t = Blocks.Pivot(parent, "Tree", p);
            Blocks.Tiled(t, "Trunk", new Vector3(0, h / 2f, 0), new Vector3(1, h, 1), Mats.Tex("log"));
            Material leaves = Mats.Tex("leaves");
            Blocks.Tiled(t, "Leaves1", new Vector3(0, h + 0.5f, 0), new Vector3(5, 2, 5), leaves);
            Blocks.Tiled(t, "Leaves2", new Vector3(0, h + 2f, 0), new Vector3(3, 1, 3), leaves);
        }

        // ---------------------------------------------------------------
        // Obstáculos
        // ---------------------------------------------------------------

        Obstacle NewObstacle(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(worldRoot, false);
            var o = go.AddComponent<Obstacle>();
            Obstacles.Add(o);
            return o;
        }

        /// <summary>Obstáculo bajo: cerdo, oveja o bala de heno. Se salta.</summary>
        Obstacle SpawnJump(int lane, float z)
        {
            int kind = rng.Next(3);
            var o = NewObstacle("Jump");
            o.Setup(Dodge.Jump, LaneX(lane), z, 1.3f, 0f, 1.0f, 0.9f);
            if (kind == 0) CritterModel.BuildPig(o.transform);
            else if (kind == 1) CritterModel.BuildSheep(o.transform);
            else
            {
                Blocks.Tiled(o.transform, "Hay", new Vector3(-0.5f, 0.5f, 0), Vector3.one, Mats.Tex("hay"));
                Blocks.Tiled(o.transform, "Hay2", new Vector3(0.5f, 0.5f, 0), Vector3.one, Mats.Tex("hay"));
                o.zMin = z - 0.5f; o.zMax = z + 0.5f;
            }
            return o;
        }

        /// <summary>Obstáculo alto: araña colgando de un tronco. Hay que agacharse.</summary>
        Obstacle SpawnDuck(int lane, float z)
        {
            var o = NewObstacle("Duck");
            o.Setup(Dodge.Duck, LaneX(lane), z, 0.9f, 1.05f, 3.2f, 1.0f);
            Material log = Mats.Tex("log");
            // Postes a los lados (solo decoración) y tronco horizontal
            Blocks.Tiled(o.transform, "PostL", new Vector3(-1.05f, 1.6f, 0), new Vector3(0.3f, 3.2f, 0.3f), log);
            Blocks.Tiled(o.transform, "PostR", new Vector3(1.05f, 1.6f, 0), new Vector3(0.3f, 3.2f, 0.3f), log);
            Blocks.Tiled(o.transform, "Beam", new Vector3(0, 3.0f, 0), new Vector3(2.4f, 0.4f, 0.4f), log);
            if (rng.Next(3) > 0)
            {
                Blocks.Part(o.transform, "Thread", new Vector3(0, 2.3f, 0), new Vector3(0.03f, 1.2f, 0.03f), Mats.Solid("#F0F0F0"), false);
                var s = CritterModel.BuildSpider(o.transform);
                s.transform.localPosition = new Vector3(0, 1.55f, 0);
            }
            else
            {
                // Telaraña ancha hecha de hilos
                Material silk = Mats.Solid("#ECECEC");
                for (int i = 0; i < 5; i++)
                    Blocks.Part(o.transform, "Silk", new Vector3(-0.9f + i * 0.45f, 2.0f, 0), new Vector3(0.04f, 1.9f, 0.04f), silk, false);
                for (int i = 0; i < 4; i++)
                    Blocks.Part(o.transform, "SilkH", new Vector3(0, 1.2f + i * 0.5f, 0), new Vector3(2.0f, 0.04f, 0.04f), silk, false);
            }
            return o;
        }

        /// <summary>Bloquea el carril: aldeano, zombi que camina hacia ti o muro de piedra.</summary>
        Obstacle SpawnBlock(int lane, float z, float difficulty)
        {
            int kind = rng.Next(3);
            var o = NewObstacle("Block");
            if (kind == 0)
            {
                o.Setup(Dodge.Lane, LaneX(lane), z, 0.8f, 0f, 2.1f, 0.6f);
                var v = HumanoidModel.BuildVillager(o.transform);
                v.transform.localRotation = Quaternion.Euler(0, 180, 0); // mira hacia el jugador
                o.walker = v;
            }
            else if (kind == 1)
            {
                o.Setup(Dodge.Lane, LaneX(lane), z + 10f, 0.8f, 0f, 1.95f, 0.6f);
                var zb = HumanoidModel.BuildZombie(o.transform);
                zb.transform.localRotation = Quaternion.Euler(0, 180, 0);
                o.walker = zb;
                o.moveSpeed = Mathf.Lerp(2f, 4f, difficulty);
            }
            else
            {
                o.Setup(Dodge.Lane, LaneX(lane), z, 1.5f, 0f, 2.6f, 1.05f);
                Blocks.Tiled(o.transform, "Wall", new Vector3(0, 1.3f, 0), new Vector3(2.1f, 2.6f, 1.5f), Mats.Tex("cobble"));
            }
            return o;
        }

        /// <summary>Muro largo de piedra (como un tren). Puede tener una rampa de tierra para subir.</summary>
        float SpawnLongWall(int lane, float z, float length, bool ramp)
        {
            float start = z;
            if (ramp)
            {
                var r = NewObstacle("Ramp");
                r.Setup(Dodge.Ramp, LaneX(lane), z + 3f, 6f, 0f, 2.6f, 1.05f);
                // Escalones visuales de la rampa
                for (int i = 0; i < 6; i++)
                {
                    float h = 2.6f * (i + 1) / 6f;
                    Blocks.Tiled(r.transform, "Step", new Vector3(0, h / 2f, -2.5f + i), new Vector3(2.1f, h, 1f), Mats.Tex("dirt"));
                }
                start = z + 6f;
            }
            var o = NewObstacle("LongWall");
            o.Setup(Dodge.Lane, LaneX(lane), start + length / 2f, length, 0f, 2.6f, 1.05f);
            Blocks.Tiled(o.transform, "Wall", new Vector3(0, 1.3f, 0), new Vector3(2.1f, 2.6f, length), Mats.Tex("stone"));
            Blocks.Tiled(o.transform, "Top", new Vector3(0, 2.65f, 0), new Vector3(2.1f, 0.1f, length), Mats.Tex("grass"));
            return start + length;
        }

        // ---------------------------------------------------------------
        // Filas: cada fila es un "reto" en los 3 carriles, siempre con una salida posible
        // ---------------------------------------------------------------

        void SpawnRow(float z, float difficulty)
        {
            float gap = Mathf.Lerp(20f, 13f, difficulty);
            double p = rng.NextDouble();
            var blocked = new Dodge?[3]; // qué hay en cada carril (-1,0,1 => 0,1,2)
            float rowEnd = z;

            if (p < 0.30)
            {
                // Un solo obstáculo
                int lane = rng.Next(3) - 1;
                blocked[lane + 1] = RandomSmall(lane, z, difficulty);
            }
            else if (p < 0.55)
            {
                // Dos obstáculos, un carril libre
                int free = rng.Next(3) - 1;
                for (int lane = -1; lane <= 1; lane++)
                    if (lane != free) blocked[lane + 1] = RandomSmall(lane, z, difficulty);
            }
            else if (p < 0.70)
            {
                // Los tres carriles: hay que saltar o agacharse en uno de ellos
                int special = rng.Next(3) - 1;
                bool allSame = rng.NextDouble() < 0.4;
                Dodge pass = rng.Next(2) == 0 ? Dodge.Jump : Dodge.Duck;
                for (int lane = -1; lane <= 1; lane++)
                {
                    if (allSame || lane == special)
                    {
                        if (pass == Dodge.Jump) SpawnJump(lane, z); else SpawnDuck(lane, z);
                        blocked[lane + 1] = pass;
                    }
                    else
                    {
                        SpawnBlock(lane, z, 0f);  // quietos para que sea justo
                        blocked[lane + 1] = Dodge.Lane;
                    }
                }
            }
            else if (p < 0.88)
            {
                // Muros largos en 1 o 2 carriles (con rampa a veces)
                int count = rng.Next(2) + 1;
                int free = rng.Next(3) - 1;
                float length = 10f + (float)rng.NextDouble() * 14f;
                int placed = 0;
                for (int lane = -1; lane <= 1 && placed < count; lane++)
                {
                    if (lane == free) continue;
                    bool ramp = rng.NextDouble() < 0.55;
                    float end = SpawnLongWall(lane, z, length, ramp);
                    rowEnd = Mathf.Max(rowEnd, end);
                    blocked[lane + 1] = Dodge.Lane;
                    // Minerales encima del muro como premio
                    if (ramp) OreLine(lane, z + 7f, Mathf.FloorToInt(length / 1.6f), 2.6f + 0.8f);
                    placed++;
                }
            }
            // else: fila vacía, solo minerales

            // Minerales en un carril libre (o en arco sobre un obstáculo bajo)
            var options = new List<int>();
            for (int lane = -1; lane <= 1; lane++)
                if (blocked[lane + 1] == null || blocked[lane + 1] == Dodge.Jump) options.Add(lane);
            if (options.Count > 0 && rng.NextDouble() < 0.85)
            {
                int lane = options[rng.Next(options.Count)];
                if (blocked[lane + 1] == Dodge.Jump) OreArc(lane, z);
                else OreLine(lane, z - 6f, 5 + rng.Next(4), 0.8f);
            }

            // Power-up de vez en cuando, en un carril sin nada
            if (z - lastPowerZ > 220f && rng.NextDouble() < 0.35)
            {
                var free = new List<int>();
                for (int lane = -1; lane <= 1; lane++) if (blocked[lane + 1] == null) free.Add(lane);
                if (free.Count > 0)
                {
                    int lane = free[rng.Next(free.Count)];
                    var type = rng.Next(2) == 0 ? PowerType.Magnet : PowerType.Boots;
                    Pickups.Add(Pickup.CreatePower(worldRoot, type, new Vector3(LaneX(lane), 1.1f, z + 9f)));
                    lastPowerZ = z;
                }
            }

            nextRowZ = rowEnd + gap + (float)rng.NextDouble() * 4f;
        }

        Dodge RandomSmall(int lane, float z, float difficulty)
        {
            int k = rng.Next(3);
            if (k == 0) { SpawnJump(lane, z); return Dodge.Jump; }
            if (k == 1) { SpawnDuck(lane, z); return Dodge.Duck; }
            SpawnBlock(lane, z, difficulty);
            return Dodge.Lane;
        }

        void OreLine(int lane, float z, int count, float y)
        {
            OreType t = Pickup.RandomOre(rng);
            for (int i = 0; i < count; i++)
                Pickups.Add(Pickup.CreateOre(worldRoot, t, new Vector3(LaneX(lane), y, z + i * 1.6f)));
        }

        void OreArc(int lane, float z)
        {
            OreType t = Pickup.RandomOre(rng);
            for (int i = 0; i < 7; i++)
            {
                float u = (i - 3) / 3f;              // de -1 a 1
                float y = 0.8f + (1f - u * u) * 1.4f; // parábola sobre el obstáculo
                Pickups.Add(Pickup.CreateOre(worldRoot, t, new Vector3(LaneX(lane), y, z + u * 4.5f)));
            }
        }
    }
}
