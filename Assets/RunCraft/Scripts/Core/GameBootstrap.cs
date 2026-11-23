using UnityEngine;

namespace RunCraft
{
    /// <summary>
    /// Arranca RunCraft automáticamente al darle a Play, sin tener que montar nada en la escena.
    /// Crea la cámara, la luz, el mundo, a Toni, al golem, el sonido y los menús.
    /// </summary>
    public static class GameBootstrap
    {
        public static readonly Color Sky = new Color(0.53f, 0.78f, 0.98f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Start()
        {
            if (Object.FindAnyObjectByType<GameManager>() != null) return;

            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.orientation = ScreenOrientation.Portrait;
            Loc.Init();

            SetupLightAndSky();

            var game = new GameObject("RunCraft");
            Object.DontDestroyOnLoad(game);

            game.AddComponent<AudioManager>();
            var track = game.AddComponent<TrackGenerator>();

            var playerGo = new GameObject("Player");
            playerGo.transform.SetParent(game.transform, false);
            var player = playerGo.AddComponent<PlayerController>();
            player.model = HumanoidModel.BuildToni(playerGo.transform);

            var golemGo = new GameObject("GolemChaser");
            golemGo.transform.SetParent(game.transform, false);
            var golem = golemGo.AddComponent<Golem>();
            golem.model = HumanoidModel.BuildGolem(golemGo.transform);

            var cam = SetupCamera();
            cam.transform.SetParent(game.transform, true);
            var rig = cam.gameObject.AddComponent<CameraRig>();
            rig.target = playerGo.transform;

            var ui = game.AddComponent<UIManager>();
            ui.Build();

            var gm = game.AddComponent<GameManager>();
            gm.Init(player, golem, track, rig, ui);
        }

        static Camera SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Sky;
            cam.fieldOfView = 62f;
            cam.nearClipPlane = 0.2f;
            cam.farClipPlane = 170f;
            return cam;
        }

        static void SetupLightAndSky()
        {
            Light sun = RenderSettings.sun;
            if (sun == null)
            {
                var l = Object.FindAnyObjectByType<Light>();
                if (l != null && l.type == LightType.Directional) sun = l;
            }
            if (sun == null)
            {
                var go = new GameObject("Sun");
                sun = go.AddComponent<Light>();
                sun.type = LightType.Directional;
            }
            sun.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
            sun.color = new Color(1f, 0.96f, 0.86f);
            sun.intensity = 1.25f;
            sun.shadows = LightShadows.Hard;
            sun.shadowStrength = 0.6f;
            RenderSettings.sun = sun;

            // Luz ambiente suave de día
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.75f, 0.9f);
            RenderSettings.ambientEquatorColor = new Color(0.55f, 0.62f, 0.5f);
            RenderSettings.ambientGroundColor = new Color(0.35f, 0.3f, 0.22f);

            // Niebla del color del cielo para que el horizonte no se corte
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = Sky;
            RenderSettings.fogStartDistance = 55f;
            RenderSettings.fogEndDistance = 150f;
        }
    }
}
