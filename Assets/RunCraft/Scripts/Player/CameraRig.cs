using UnityEngine;

namespace RunCraft
{
    /// <summary>
    /// Cámara: en el menú mira a Toni de frente; al jugar se coloca detrás de él y le sigue.
    /// </summary>
    public class CameraRig : MonoBehaviour
    {
        public Transform target;
        bool menuView = true;
        float blend = 1f;   // 0..1 durante la transición entre vistas
        float shake;
        float smoothX, smoothY;

        static readonly Vector3 GameOffset = new Vector3(0, 4.2f, -6.2f);
        static readonly Vector3 GameLook = new Vector3(0, 1.0f, 7f);
        static readonly Vector3 MenuOffset = new Vector3(1.6f, 2.0f, 4.2f);
        static readonly Vector3 MenuLook = new Vector3(0, 1.4f, -1.5f);

        public void Shake(float amount) => shake = Mathf.Max(shake, amount);

        /// <summary>Cambia entre la vista del menú y la de juego con una transición suave.</summary>
        public void SetMenuView(bool menu)
        {
            if (menuView != menu) blend = 0f;
            menuView = menu;
        }

        /// <summary>Coloca la cámara al instante (al reiniciar).</summary>
        public void Snap()
        {
            blend = 1f;
            if (target != null) { smoothX = target.position.x; smoothY = target.position.y; }
            Place(0f);
        }

        void LateUpdate()
        {
            float dt = Time.unscaledDeltaTime;
            blend = Mathf.MoveTowards(blend, 1f, dt * 1.3f);
            Place(dt);
        }

        void Place(float dt)
        {
            if (target == null) return;
            Vector3 p = target.position;

            // X e Y se suavizan; Z se sigue exacto para que no haya tirones a mucha velocidad
            smoothX = Mathf.Lerp(smoothX, p.x * 0.8f, Mathf.Clamp01(dt * 8f));
            smoothY = Mathf.Lerp(smoothY, p.y * 0.6f, Mathf.Clamp01(dt * 5f));
            Vector3 follow = new Vector3(smoothX, smoothY, p.z);

            Vector3 gamePos = follow + GameOffset;
            Vector3 gameLook = follow + GameLook;
            Vector3 menuPos = p + MenuOffset;
            Vector3 menuLook = p + MenuLook;

            float k = Mathf.SmoothStep(0f, 1f, blend);
            Vector3 pos, look;
            if (menuView)
            {
                pos = Vector3.Lerp(gamePos, menuPos, k);
                look = Vector3.Lerp(gameLook, menuLook, k);
            }
            else
            {
                pos = Vector3.Lerp(menuPos, gamePos, k);
                look = Vector3.Lerp(menuLook, gameLook, k);
            }

            if (shake > 0f)
            {
                pos += Random.insideUnitSphere * shake;
                shake = Mathf.MoveTowards(shake, 0f, dt * 1.5f);
            }
            transform.position = pos;
            transform.rotation = Quaternion.LookRotation(look - pos);
        }
    }
}
