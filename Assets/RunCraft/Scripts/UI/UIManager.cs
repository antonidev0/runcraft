using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace RunCraft
{
    /// <summary>
    /// Todas las pantallas (menú, marcador, pausa y fin de partida) creadas por código.
    /// Diseñadas para móvil en vertical (1080x1920) y se adaptan a otras pantallas.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        Font font;
        RectTransform root;
        GameObject menu, hud, pause, gameOver;
        Text scoreText, gemsText, powerText, flashText, menuStats, overTitle, overStats, overBest, speedText;
        Text soundLabel;
        float flashTimer;

        // Textos que cambian con el idioma
        readonly List<(Text text, string key)> localized = new List<(Text, string)>();

        static readonly Color Green = new Color(0.30f, 0.65f, 0.20f);
        static readonly Color Brown = new Color(0.45f, 0.32f, 0.20f);
        static readonly Color Grey = new Color(0.35f, 0.35f, 0.38f);
        static readonly Color Panel = new Color(0f, 0f, 0f, 0.55f);
        static readonly Color Gold = new Color(1f, 0.85f, 0.25f);

        public void Build()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Sistema de eventos para que los botones funcionen con el nuevo Input System
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<InputSystemUIInputModule>();
            }

            var canvasGo = new GameObject("UI");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            root = canvasGo.GetComponent<RectTransform>();

            // La zona segura evita el notch / cámara frontal de muchos Android
            var safe = NewRect("Safe", root);
            safe.gameObject.AddComponent<SafeArea>();
            Stretch(safe);

            BuildMenu(safe);
            BuildHud(safe);
            BuildPause(safe);
            BuildGameOver(safe);

            Loc.Changed += RefreshTexts;
            RefreshTexts();
        }

        void OnDestroy() { Loc.Changed -= RefreshTexts; }

        // ---------------- Pantallas ----------------

        void BuildMenu(RectTransform parent)
        {
            menu = NewRect("Menu", parent).gameObject;
            Stretch(menu.GetComponent<RectTransform>());

            var title = MakeText(menu.transform, "RUNCRAFT", 150, Color.white, new Vector2(0.5f, 1f), new Vector2(0, -260), new Vector2(1000, 200));
            title.fontStyle = FontStyle.Bold;
            var sh = title.gameObject.AddComponent<Shadow>();
            sh.effectColor = new Color(0.2f, 0.35f, 0.1f, 1f);
            sh.effectDistance = new Vector2(8, -8);
            Local(MakeText(menu.transform, "", 46, Gold, new Vector2(0.5f, 1f), new Vector2(0, -400), new Vector2(1000, 80)), "tagline");

            menuStats = MakeText(menu.transform, "", 44, Color.white, new Vector2(0.5f, 0f), new Vector2(0, 760), new Vector2(1000, 120));

            var play = MakeButton(menu.transform, "", Green, new Vector2(0.5f, 0f), new Vector2(0, 560), new Vector2(560, 170), 80,
                () => { AudioManager.I.Play("click"); GameManager.I.StartRun(); });
            Local(play, "play");

            var lang = MakeButton(menu.transform, "", Brown, new Vector2(0.5f, 0f), new Vector2(-220, 380), new Vector2(420, 110), 38,
                () => { AudioManager.I.Play("click"); Loc.Toggle(); });
            Local(lang, "lang");

            soundLabel = MakeButton(menu.transform, "", Brown, new Vector2(0.5f, 0f), new Vector2(220, 380), new Vector2(420, 110), 38,
                () =>
                {
                    Save.Sound = !Save.Sound;
                    AudioManager.I.ApplyMute();
                    AudioManager.I.Play("click");
                    RefreshTexts();
                });

            Local(MakeText(menu.transform, "", 34, Color.white, new Vector2(0.5f, 0f), new Vector2(0, 240), new Vector2(1000, 60)), "help");
            Local(MakeText(menu.transform, "", 28, new Color(1, 1, 1, 0.7f), new Vector2(0.5f, 0f), new Vector2(0, 190), new Vector2(1000, 50)), "help_pc");
        }

        void BuildHud(RectTransform parent)
        {
            hud = NewRect("Hud", parent).gameObject;
            Stretch(hud.GetComponent<RectTransform>());

            // Botón de pausa (arriba a la izquierda)
            MakeButton(hud.transform, "II", Panel, new Vector2(0f, 1f), new Vector2(90, -90), new Vector2(120, 120), 56,
                () => GameManager.I.Pause());

            scoreText = MakeText(hud.transform, "0", 80, Color.white, new Vector2(1f, 1f), new Vector2(-260, -80), new Vector2(480, 110));
            scoreText.alignment = TextAnchor.MiddleRight;
            scoreText.fontStyle = FontStyle.Bold;
            scoreText.gameObject.AddComponent<Outline>().effectDistance = new Vector2(3, -3);

            gemsText = MakeText(hud.transform, "", 56, Gold, new Vector2(1f, 1f), new Vector2(-260, -175), new Vector2(480, 80));
            gemsText.alignment = TextAnchor.MiddleRight;
            gemsText.gameObject.AddComponent<Outline>().effectDistance = new Vector2(3, -3);

            speedText = MakeText(hud.transform, "", 34, new Color(1, 1, 1, 0.8f), new Vector2(1f, 1f), new Vector2(-260, -245), new Vector2(480, 50));
            speedText.alignment = TextAnchor.MiddleRight;

            powerText = MakeText(hud.transform, "", 44, new Color(0.6f, 0.9f, 1f), new Vector2(0f, 1f), new Vector2(330, -200), new Vector2(560, 160));
            powerText.alignment = TextAnchor.UpperLeft;
            powerText.gameObject.AddComponent<Outline>().effectDistance = new Vector2(2, -2);

            flashText = MakeText(hud.transform, "", 80, Gold, new Vector2(0.5f, 0.5f), new Vector2(0, 300), new Vector2(1000, 140));
            flashText.fontStyle = FontStyle.Bold;
            flashText.gameObject.AddComponent<Outline>().effectDistance = new Vector2(4, -4);
        }

        void BuildPause(RectTransform parent)
        {
            pause = NewRect("Pause", parent).gameObject;
            Stretch(pause.GetComponent<RectTransform>());
            var bg = pause.AddComponent<Image>();
            bg.color = Panel;
            Local(MakeText(pause.transform, "", 110, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0, 300), new Vector2(1000, 160)), "paused");
            Local(MakeButton(pause.transform, "", Green, new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(560, 150), 64,
                () => { AudioManager.I.Play("click"); GameManager.I.Resume(); }), "resume");
            Local(MakeButton(pause.transform, "", Grey, new Vector2(0.5f, 0.5f), new Vector2(0, -150), new Vector2(560, 130), 56,
                () => { AudioManager.I.Play("click"); GameManager.I.GoToMenu(); }), "menu");
        }

        void BuildGameOver(RectTransform parent)
        {
            gameOver = NewRect("GameOver", parent).gameObject;
            Stretch(gameOver.GetComponent<RectTransform>());
            var bg = gameOver.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.45f);
            overTitle = MakeText(gameOver.transform, "", 90, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0, 520), new Vector2(1040, 160));
            overTitle.fontStyle = FontStyle.Bold;
            overTitle.gameObject.AddComponent<Outline>().effectDistance = new Vector2(4, -4);
            overBest = MakeText(gameOver.transform, "", 70, Gold, new Vector2(0.5f, 0.5f), new Vector2(0, 390), new Vector2(1000, 110));
            overStats = MakeText(gameOver.transform, "", 52, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0, 170), new Vector2(1000, 320));
            Local(MakeButton(gameOver.transform, "", Green, new Vector2(0.5f, 0.5f), new Vector2(0, -120), new Vector2(560, 160), 70,
                () => { AudioManager.I.Play("click"); GameManager.I.StartRun(); }), "retry");
            Local(MakeButton(gameOver.transform, "", Grey, new Vector2(0.5f, 0.5f), new Vector2(0, -310), new Vector2(560, 130), 56,
                () => { AudioManager.I.Play("click"); GameManager.I.GoToMenu(); }), "menu");
        }

        // ---------------- Cambiar de pantalla ----------------

        void Show(GameObject which)
        {
            menu.SetActive(which == menu);
            hud.SetActive(which == hud || which == pause);
            pause.SetActive(which == pause);
            gameOver.SetActive(which == gameOver);
        }

        public void ShowMenu() { Show(menu); RefreshTexts(); }
        public void ShowHud() { Show(hud); flashTimer = 0f; flashText.text = ""; }
        public void ShowPause() { Show(pause); }

        public void ShowGameOver()
        {
            Show(gameOver);
            RefreshTexts();
        }

        public void Flash(string msg)
        {
            flashText.text = msg;
            flashTimer = 1.3f;
        }

        void RefreshTexts()
        {
            foreach (var (text, key) in localized) text.text = Loc.T(key);
            if (soundLabel) soundLabel.text = Loc.T(Save.Sound ? "sound_on" : "sound_off");
            if (menuStats) menuStats.text = $"{Loc.T("best")}: {Save.Best}     {Loc.T("gems")}: {Save.TotalGems}";

            var gm = GameManager.I;
            if (gm != null && overTitle != null)
            {
                overTitle.text = Loc.T(gm.WasCaught ? "caught" : "crashed");
                overBest.text = gm.NewBest ? Loc.T("new_best") : $"{Loc.T("best")}: {Save.Best}";
                overStats.text = $"{Loc.T("score")}: {gm.Score}\n{Loc.T("distance")}: {Mathf.FloorToInt(gm.Distance)} m\n{Loc.T("ores_got")}: {gm.RunGems}";
            }
        }

        void Update()
        {
            var gm = GameManager.I;
            if (gm == null || !hud.activeSelf) return;

            scoreText.text = gm.Score.ToString();
            gemsText.text = $"{Loc.T("gems")}: {gm.RunGems}";
            speedText.text = Mathf.FloorToInt(gm.Distance) + " m";

            string p = "";
            if (gm.MagnetTime > 0f) p += $"{Loc.T("magnet")} {Mathf.CeilToInt(gm.MagnetTime)}s\n";
            if (gm.BootsTime > 0f) p += $"{Loc.T("boots")} {Mathf.CeilToInt(gm.BootsTime)}s";
            powerText.text = p;

            if (flashTimer > 0f)
            {
                flashTimer -= Time.unscaledDeltaTime;
                var c = flashText.color;
                c.a = Mathf.Clamp01(flashTimer / 0.4f);
                flashText.color = c;
                if (flashTimer <= 0f) flashText.text = "";
            }
        }

        // ---------------- Ayudas para crear elementos ----------------

        void Local(Text t, string key) => localized.Add((t, key));

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        static void Stretch(RectTransform r)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
        }

        Text MakeText(Transform parent, string text, int size, Color color, Vector2 anchor, Vector2 pos, Vector2 box)
        {
            var r = NewRect("Text", parent);
            r.anchorMin = r.anchorMax = anchor;
            r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = pos;
            r.sizeDelta = box;
            var t = r.gameObject.AddComponent<Text>();
            t.font = font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        /// <summary>Crea un botón con aspecto de bloque (borde oscuro) y devuelve su texto.</summary>
        Text MakeButton(Transform parent, string label, Color color, Vector2 anchor, Vector2 pos, Vector2 size, int fontSize, UnityEngine.Events.UnityAction onClick)
        {
            var r = NewRect("Button", parent);
            r.anchorMin = r.anchorMax = anchor;
            r.anchoredPosition = pos;
            r.sizeDelta = size;
            var img = r.gameObject.AddComponent<Image>();
            img.color = color;
            var outline = r.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.6f);
            outline.effectDistance = new Vector2(6, -6);
            var b = r.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            var colors = b.colors;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f);
            colors.pressedColor = new Color(0.75f, 0.75f, 0.75f);
            b.colors = colors;
            b.onClick.AddListener(onClick);

            var t = MakeText(r, label, fontSize, Color.white, new Vector2(0.5f, 0.5f), Vector2.zero, size);
            t.fontStyle = FontStyle.Bold;
            t.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(3, -3);
            return t;
        }
    }

    /// <summary>Ajusta la interfaz a la zona segura de la pantalla (notch, barras).</summary>
    public class SafeArea : MonoBehaviour
    {
        Rect last;
        void Update()
        {
            Rect s = Screen.safeArea;
            if (s == last || Screen.width == 0 || Screen.height == 0) return;
            last = s;
            var r = GetComponent<RectTransform>();
            r.anchorMin = new Vector2(s.xMin / Screen.width, s.yMin / Screen.height);
            r.anchorMax = new Vector2(s.xMax / Screen.width, s.yMax / Screen.height);
            r.offsetMin = r.offsetMax = Vector2.zero;
        }
    }
}
