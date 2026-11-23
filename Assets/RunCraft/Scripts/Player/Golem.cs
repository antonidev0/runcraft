using UnityEngine;

namespace RunCraft
{
    /// <summary>
    /// El golem de piedra que te persigue. Normalmente se queda lejos (fuera de cámara),
    /// pero si tropiezas se acerca. Si tropiezas otra vez mientras está cerca, te atrapa.
    /// </summary>
    public class Golem : MonoBehaviour
    {
        public HumanoidModel model;
        const float FarDistance = 16f;
        const float CloseDistance = 3.2f;

        float behind;
        float x;
        bool grabbing;

        public bool Close => behind < 6f;

        public void ResetGolem(Vector3 playerPos)
        {
            behind = 4.5f;
            x = playerPos.x;
            grabbing = false;
            model.transform.localRotation = Quaternion.identity;
            Place(playerPos);
        }

        void Place(Vector3 playerPos)
        {
            transform.position = new Vector3(x, 0f, playerPos.z - behind);
        }

        /// <summary>Mientras se juega.</summary>
        public void Tick(float dt, Vector3 playerPos, bool chasing)
        {
            float target = chasing ? CloseDistance : FarDistance;
            float rate = chasing ? 8f : 2.5f;
            behind = Mathf.MoveTowards(behind, target, rate * dt);
            x = Mathf.Lerp(x, playerPos.x, dt * 4f);
            Place(playerPos);
            model.Run(dt, 9f);
        }

        /// <summary>En el menú: quieto detrás de Toni.</summary>
        public void IdleTick(float dt, Vector3 playerPos)
        {
            behind = 4.5f;
            Place(playerPos);
            model.Idle(dt);
        }

        /// <summary>Cuando pierdes: el golem llega hasta Toni y levanta los brazos.</summary>
        public void GrabTick(float dt, Vector3 playerPos)
        {
            if (!grabbing)
            {
                grabbing = true;
                AudioManager.I.Play("roar");
            }
            behind = Mathf.MoveTowards(behind, 1.6f, 14f * dt);
            x = Mathf.Lerp(x, playerPos.x, dt * 8f);
            Place(playerPos);
            if (behind > 1.7f) model.Run(dt, 12f);
            else model.ArmsUp();
        }
    }
}
