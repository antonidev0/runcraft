using UnityEngine;

namespace RunCraft
{
    public enum GameState { Menu, Playing, Paused, Dying, GameOver }

    /// <summary>
    /// El "director" del juego: estados (menú, jugando, pausa, fin), velocidad,
    /// puntuación, power-ups y quién se actualiza en cada fotograma.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        // Ajustes de dificultad (puedes cambiarlos para probar)
        public float startSpeed = 11f;
        public float maxSpeed = 26f;
        public float speedRampMeters = 1800f;   // cuántos metros tarda en acercarse a la velocidad máxima
        public float powerDuration = 10f;
        public float magnetRange = 9f;

        public GameState State { get; private set; } = GameState.Menu;
        public float Distance { get; private set; }
        public int Score => Mathf.FloorToInt(Distance) + RunGems * 5;
        public int RunGems { get; private set; }
        public float Speed { get; private set; }
        public float MagnetTime { get; private set; }
        public float BootsTime { get; private set; }
        public bool NewBest { get; private set; }
        public bool WasCaught { get; private set; }

        PlayerController player;
        Golem golem;
        TrackGenerator track;
        CameraRig cam;
        UIManager ui;
        readonly SwipeInput input = new SwipeInput();
        float stumbleTimer;   // mientras es > 0, el golem está cerca
        float dyingTimer;

        public void Init(PlayerController p, Golem g, TrackGenerator t, CameraRig c, UIManager u)
        {
            I = this;
            player = p; golem = g; track = t; cam = c; ui = u;
            GoToMenu();
        }

        void Awake() { I = this; }

        public void GoToMenu()
        {
            Time.timeScale = 1f;
            State = GameState.Menu;
            track.ResetTrack();
            player.ResetPlayer();
            golem.ResetGolem(player.Position);
            cam.SetMenuView(true);
            cam.Snap();
            ui.ShowMenu();
        }

        public void StartRun()
        {
            Time.timeScale = 1f;
            if (State != GameState.Menu)
            {
                // "Otra vez": reiniciamos el mundo
                track.ResetTrack();
                player.ResetPlayer();
                golem.ResetGolem(player.Position);
                cam.Snap();
            }
            Distance = 0; RunGems = 0;
            MagnetTime = 0; BootsTime = 0;
            stumbleTimer = 2.5f;   // al empezar, el golem está cerca y luego se aleja
            NewBest = false; WasCaught = false;
            Speed = startSpeed;
            State = GameState.Playing;
            cam.SetMenuView(false);
            ui.ShowHud();
            AudioManager.I.Play("roar", 1.1f, 0.7f);
            AudioManager.I.PlayMusic();
        }

        public void Pause()
        {
            if (State != GameState.Playing) return;
            State = GameState.Paused;
            Time.timeScale = 0f;
            ui.ShowPause();
        }

        public void Resume()
        {
            if (State != GameState.Paused) return;
            State = GameState.Playing;
            Time.timeScale = 1f;
            ui.ShowHud();
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) Pause(); // si sales de la app en el móvil, se pausa sola
        }

        /// <summary>Tropiezo lateral: si el golem ya estaba cerca, te atrapa.</summary>
        public void Stumble()
        {
            cam.Shake(0.25f);
            AudioManager.I.Play("stumble");
            if (stumbleTimer > 0f && Distance > 30f)
            {
                WasCaught = true;
                player.Caught();
                EndRun();
                return;
            }
            stumbleTimer = 7f;
            ui.Flash(Loc.T("watch_out"));
        }

        /// <summary>Choque de frente.</summary>
        public void Crash()
        {
            cam.Shake(0.5f);
            AudioManager.I.Play("crash");
            WasCaught = false;
            EndRun();
        }

        void EndRun()
        {
            State = GameState.Dying;
            dyingTimer = 0f;
            int s = Score;
            if (s > Save.Best) { Save.Best = s; NewBest = true; }
            Save.TotalGems += RunGems;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            switch (State)
            {
                case GameState.Menu:
                    player.IdleAnim(dt);
                    golem.IdleTick(dt, player.Position);
                    track.Tick(dt, player.Z, 0f, false);
                    break;

                case GameState.Playing:
                    PlayingTick(dt);
                    break;

                case GameState.Paused:
                    if (input.PausePressed()) Resume();
                    break;

                case GameState.Dying:
                    dyingTimer += dt;
                    player.Tick(dt, 0f, track);
                    golem.GrabTick(dt, player.Position);
                    if (dyingTimer > 1.4f)
                    {
                        State = GameState.GameOver;
                        ui.ShowGameOver();
                    }
                    break;

                case GameState.GameOver:
                    golem.GrabTick(dt, player.Position);
                    break;
            }
        }

        void PlayingTick(float dt)
        {
            if (input.PausePressed()) { Pause(); return; }

            // La velocidad sube poco a poco con la distancia
            float k = 1f - Mathf.Exp(-Distance / speedRampMeters);
            Speed = Mathf.Lerp(startSpeed, maxSpeed, k);
            float difficulty = Mathf.Clamp01(Distance / 2500f);

            player.HandleMove(input.Read());
            player.Tick(dt, Speed, track);
            if (State != GameState.Playing) return; // ha chocado este fotograma

            Distance = player.Z;
            track.Tick(dt, player.Z, difficulty, true);

            if (stumbleTimer > 0f) stumbleTimer -= dt;
            golem.Tick(dt, player.Position, stumbleTimer > 0f);

            if (MagnetTime > 0f) MagnetTime -= dt;
            if (BootsTime > 0f) BootsTime -= dt;

            CollectPickups(dt);
        }

        void CollectPickups(float dt)
        {
            Vector3 center = player.Position + Vector3.up * (player.Rolling ? 0.5f : 0.9f);
            var list = track.Pickups;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var p = list[i];
                if (p == null) { list.RemoveAt(i); continue; }
                Vector3 pp = p.transform.position;

                // Imán: atrae los minerales cercanos de cualquier carril
                if (!p.IsPower && MagnetTime > 0f && pp.z - center.z < magnetRange && pp.z > center.z - 1f)
                    p.attracted = true;
                if (p.attracted)
                {
                    pp = Vector3.MoveTowards(pp, center, (Speed + 15f) * dt);
                    p.transform.position = pp;
                }

                Vector3 d = pp - center;
                bool hit = Mathf.Abs(d.x) < 0.9f && Mathf.Abs(d.z) < 0.9f && Mathf.Abs(d.y) < 1.1f;
                if (!hit) continue;

                if (p.IsPower)
                {
                    if (p.power == PowerType.Magnet) MagnetTime = powerDuration;
                    else BootsTime = powerDuration;
                    AudioManager.I.Play("power");
                    ui.Flash(Loc.T(p.power == PowerType.Magnet ? "magnet" : "boots") + "!");
                }
                else
                {
                    int v = Pickup.Value(p.ore);
                    RunGems += v;
                    AudioManager.I.Play("ore", 1f + Mathf.Min(v, 10) * 0.05f, 0.7f);
                }
                Destroy(p.gameObject);
                list.RemoveAt(i);
            }
        }
    }
}
