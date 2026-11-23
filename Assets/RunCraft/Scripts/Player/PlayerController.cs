using UnityEngine;

namespace RunCraft
{
    /// <summary>
    /// Toni: corre solo hacia delante (+Z), cambia entre 3 carriles, salta y se agacha rodando.
    /// Las colisiones se calculan con cajas (sin física) contra los obstáculos del TrackGenerator.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        public const float LaneWidth = 2.4f;
        const float HalfWidth = 0.35f;
        const float HalfDepth = 0.3f;
        const float StandHeight = 1.7f;
        const float RollHeight = 0.8f;
        const float Gravity = 32f;
        const float JumpHeight = 1.6f;
        const float BootsJumpHeight = 3.6f;
        const float RollTime = 0.65f;
        const float LaneSpeed = 16f;   // metros por segundo al cambiar de carril
        const float StepUp = 0.35f;    // sube escalones pequeños (y el final de las rampas) sin chocar

        public HumanoidModel model;
        public int Lane { get; private set; }
        public bool Grounded { get; private set; } = true;
        public bool Rolling => rollTimer > 0f;
        public bool Dead { get; private set; }
        public float Z => pos.z;
        public Vector3 Position => pos;
        public bool HasBoots => GameManager.I.BootsTime > 0f;

        Vector3 pos;
        int prevLane;
        float vy, rollTimer, deadTimer;
        float rollSpin;

        public void ResetPlayer()
        {
            Lane = 0; prevLane = 0;
            pos = Vector3.zero;
            vy = 0; rollTimer = 0; Dead = false; deadTimer = 0;
            Grounded = true;
            transform.position = pos;
            model.body.localRotation = Quaternion.identity;
            model.body.localPosition = Vector3.zero;
            model.transform.localScale = Vector3.one;
        }

        float LaneX(int lane) => lane * LaneWidth;

        public void HandleMove(Move m)
        {
            if (Dead) return;
            switch (m)
            {
                case Move.Left:
                case Move.Right:
                    int target = Lane + (m == Move.Left ? -1 : 1);
                    if (target < -1 || target > 1)
                    {
                        AudioManager.I.Play("stumble", 1.4f, 0.4f); // choque suave contra el borde
                        return;
                    }
                    prevLane = Lane;
                    Lane = target;
                    AudioManager.I.Play("lane", 1f, 0.6f);
                    break;
                case Move.Up:
                    if (Grounded)
                    {
                        float h = HasBoots ? BootsJumpHeight : JumpHeight;
                        vy = Mathf.Sqrt(2f * Gravity * h);
                        Grounded = false;
                        rollTimer = 0f;
                        AudioManager.I.Play("jump", HasBoots ? 0.8f : 1f);
                    }
                    break;
                case Move.Down:
                    if (!Grounded) vy = Mathf.Min(vy, -18f); // caída rápida si estás en el aire
                    rollTimer = RollTime;
                    AudioManager.I.Play("roll");
                    break;
            }
        }

        /// <summary>Se llama cada fotograma mientras se juega.</summary>
        public void Tick(float dt, float speed, TrackGenerator track)
        {
            if (Dead)
            {
                DeadAnim(dt);
                return;
            }

            Vector3 prev = pos;

            // Avanzar y moverse hacia el carril objetivo
            pos.z += speed * dt;
            pos.x = Mathf.MoveTowards(pos.x, LaneX(Lane), LaneSpeed * dt);

            // Gravedad y suelo
            vy -= Gravity * dt;
            pos.y += vy * dt;
            float ground = track.GroundAt(pos.x, HalfWidth, pos.z, HalfDepth, Mathf.Max(pos.y, prev.y) + StepUp);
            if (pos.y <= ground)
            {
                if (!Grounded && vy < -4f) AudioManager.I.Play("lane", 0.6f, 0.4f); // aterrizaje
                pos.y = ground;
                vy = 0f;
                Grounded = true;
            }
            else if (Grounded && pos.y - ground > 0.05f)
            {
                Grounded = false; // nos hemos salido por el borde de un muro
            }

            if (rollTimer > 0f) rollTimer -= dt;

            // Colisiones
            float height = Rolling ? RollHeight : StandHeight;
            foreach (var o in track.Obstacles)
            {
                if (o.IsRamp) continue;
                if (o.zMax < pos.z - 2f || o.zMin > pos.z + 2f) continue;

                bool overX = Mathf.Abs(pos.x - o.x) < HalfWidth + o.halfWidth;
                bool overZ = pos.z + HalfDepth > o.zMin && pos.z - HalfDepth < o.zMax;
                bool overY = pos.y + height > o.yMin + 0.02f && pos.y < o.yMax - 0.02f;
                if (!(overX && overZ && overY)) continue;

                bool wasAlignedX = Mathf.Abs(prev.x - o.x) < HalfWidth + o.halfWidth;
                if (wasAlignedX)
                {
                    // Choque de frente: fin de la partida
                    pos.z = o.zMin - HalfDepth - 0.05f;
                    Die();
                    break;
                }
                else
                {
                    // Choque lateral al cambiar de carril: tropiezas y vuelves a tu carril
                    Lane = prevLane;
                    pos.x = prev.x;
                    GameManager.I.Stumble();
                    break;
                }
            }

            transform.position = pos;
            Animate(dt);
        }

        void Animate(float dt)
        {
            if (Rolling && Grounded)
            {
                // Hecho una bola: encogido y girando
                rollSpin += dt * 900f;
                model.transform.localScale = new Vector3(1f, 0.45f, 1f);
                model.body.localRotation = Quaternion.Euler(rollSpin, 0, 0);
                model.body.localPosition = new Vector3(0, 0.9f, 0) - model.body.localRotation * new Vector3(0, 0.9f, 0);
                model.Run(dt, 0f);
            }
            else
            {
                rollSpin = 0f;
                model.transform.localScale = Vector3.one;
                model.body.localRotation = Quaternion.Euler(Grounded ? 8f : -5f, 0, 0); // un poco inclinado al correr
                model.body.localPosition = Vector3.zero;
                if (Grounded) model.Run(dt, 14f);
                else model.Run(dt, 3f);
            }
        }

        public void IdleAnim(float dt)
        {
            model.transform.localScale = Vector3.one;
            model.body.localRotation = Quaternion.identity;
            model.body.localPosition = Vector3.zero;
            model.Idle(dt);
        }

        void Die()
        {
            Dead = true;
            deadTimer = 0f;
            model.transform.localScale = Vector3.one;
            model.body.localPosition = Vector3.zero;
            transform.position = pos;
            GameManager.I.Crash();
        }

        void DeadAnim(float dt)
        {
            // cae hacia atrás
            deadTimer += dt;
            float t = Mathf.Clamp01(deadTimer / 0.35f);
            model.body.localRotation = Quaternion.Euler(Mathf.Lerp(0, -80, t), 0, 0);
            if (pos.y > 0f)
            {
                vy -= Gravity * dt;
                pos.y = Mathf.Max(0f, pos.y + vy * dt);
                transform.position = pos;
            }
        }

        /// <summary>Fin forzado (el golem te atrapa tras tropezar dos veces).</summary>
        public void Caught()
        {
            Dead = true;
            deadTimer = 0f;
            model.transform.localScale = Vector3.one;
            model.body.localPosition = Vector3.zero;
        }
    }
}
