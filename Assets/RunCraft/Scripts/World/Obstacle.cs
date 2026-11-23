using UnityEngine;

namespace RunCraft
{
    /// <summary>Cómo se esquiva un obstáculo (lo usa el generador para crear filas justas).</summary>
    public enum Dodge
    {
        Jump,      // bajo: hay que saltar (cerdo, oveja, bala de heno)
        Duck,      // alto: hay que agacharse (araña colgando, tronco)
        Lane,      // bloquea el carril: hay que cambiar de carril (aldeano, zombi, muro)
        Ramp       // rampa: se puede subir corriendo
    }

    /// <summary>
    /// Caja de colisión de un obstáculo. Guardamos los límites a mano (sin física):
    /// es más simple, rápido en móvil y totalmente predecible.
    /// </summary>
    public class Obstacle : MonoBehaviour
    {
        public Dodge dodge;
        public float x;           // centro en X (el carril)
        public float halfWidth = 1.0f;
        public float yMin, yMax;  // altura inferior y superior de la caja
        public float zMin, zMax;  // principio y final en Z
        public float moveSpeed;   // > 0 si camina hacia el jugador (zombis)
        public HumanoidModel walker;

        public bool IsRamp => dodge == Dodge.Ramp;

        public void Setup(Dodge d, float laneX, float z, float depth, float bottom, float top, float halfW = 1.0f)
        {
            dodge = d;
            x = laneX;
            halfWidth = halfW;
            zMin = z - depth / 2f;
            zMax = z + depth / 2f;
            yMin = bottom;
            yMax = top;
            transform.position = new Vector3(laneX, 0, z);
        }

        /// <summary>Altura del "suelo" de este obstáculo en la posición z (las rampas suben poco a poco).</summary>
        public float TopAt(float z)
        {
            if (!IsRamp) return yMax;
            return yMax * Mathf.Clamp01(Mathf.InverseLerp(zMin, zMax, z));
        }

        public void Tick(float dt)
        {
            if (moveSpeed > 0f)
            {
                float d = moveSpeed * dt;
                zMin -= d; zMax -= d;
                transform.position += Vector3.back * d;
                if (walker) walker.Run(dt, 7f);
            }
            else if (walker) walker.Idle(dt);
        }
    }
}
