using System.Collections.Generic;
using UnityEngine;

namespace RunCraft
{
    /// <summary>
    /// Ayudas para construir todo con cubos (personajes, mobs, árboles, suelo...).
    /// No usamos colliders de física: las colisiones las calcula PlayerController a mano.
    /// </summary>
    public static class Blocks
    {
        static Mesh unitCube;
        static readonly Dictionary<Vector3, Mesh> tiledCache = new Dictionary<Vector3, Mesh>();


        // Unity 6 puede entrar en Play sin recargar los scripts, así que los datos "static"
        // sobreviven entre partidas de prueba aunque los objetos ya se hayan destruido.
        // Por eso vaciamos la caché al empezar cada Play.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            tiledCache.Clear();
            unitCube = null;
        }

        /// <summary>Objeto vacío que sirve como punto de giro (por ejemplo, una cadera o un hombro).</summary>
        public static Transform Pivot(Transform parent, string name, Vector3 localPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            return go.transform;
        }

        /// <summary>
        /// Cubo de tamaño "size" centrado en localPos. La textura se estira en cada cara.
        /// Ideal para piezas pequeñas de personajes.
        /// </summary>
        public static GameObject Part(Transform parent, string name, Vector3 localPos, Vector3 size, Material mat, bool shadows = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = size;
            go.AddComponent<MeshFilter>().sharedMesh = UnitCube;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = shadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        /// <summary>
        /// Bloque grande cuya textura se repite una vez por metro (suelo, muros...).
        /// </summary>
        public static GameObject Tiled(Transform parent, string name, Vector3 localPos, Vector3 size, Material mat, bool shadows = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.AddComponent<MeshFilter>().sharedMesh = TiledBox(size);
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = shadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        public static Mesh UnitCube
        {
            get
            {
                if (unitCube == null) unitCube = BuildBox(Vector3.one, false);
                return unitCube;
            }
        }

        static Mesh TiledBox(Vector3 size)
        {
            if (!tiledCache.TryGetValue(size, out var m) || m == null)
            {
                m = BuildBox(size, true);
                tiledCache[size] = m;
            }
            return m;
        }

        // Construye una caja con 24 vértices (4 por cara) para que cada cara tenga su propia UV.
        static Mesh BuildBox(Vector3 size, bool tiled)
        {
            Vector3 h = size * 0.5f;
            var verts = new List<Vector3>();
            var norms = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            void Face(Vector3 n, Vector3 u, Vector3 v, float uLen, float vLen)
            {
                // n = normal, u y v = ejes de la cara (con media longitud incluida)
                int i = verts.Count;
                Vector3 c = Vector3.Scale(n, h);
                verts.Add(c - u - v);
                verts.Add(c + u - v);
                verts.Add(c + u + v);
                verts.Add(c - u + v);
                for (int k = 0; k < 4; k++) norms.Add(n);
                float su = tiled ? uLen : 1f, sv = tiled ? vLen : 1f;
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(su, 0));
                uvs.Add(new Vector2(su, sv));
                uvs.Add(new Vector2(0, sv));
                tris.Add(i); tris.Add(i + 2); tris.Add(i + 1);
                tris.Add(i); tris.Add(i + 3); tris.Add(i + 2);
            }

            // Orden de vértices pensado para que los triángulos miren hacia fuera.
            Face(Vector3.forward, new Vector3(-h.x, 0, 0), new Vector3(0, h.y, 0), size.x, size.y);
            Face(Vector3.back, new Vector3(h.x, 0, 0), new Vector3(0, h.y, 0), size.x, size.y);
            Face(Vector3.right, new Vector3(0, 0, h.z), new Vector3(0, h.y, 0), size.z, size.y);
            Face(Vector3.left, new Vector3(0, 0, -h.z), new Vector3(0, h.y, 0), size.z, size.y);
            Face(Vector3.up, new Vector3(h.x, 0, 0), new Vector3(0, 0, h.z), size.x, size.z);
            Face(Vector3.down, new Vector3(-h.x, 0, 0), new Vector3(0, 0, h.z), size.x, size.z);

            var mesh = new Mesh { name = tiled ? "TiledBox" : "UnitCube" };
            mesh.SetVertices(verts);
            mesh.SetNormals(norms);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }
    }
}
