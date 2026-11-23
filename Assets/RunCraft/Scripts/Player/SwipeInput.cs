using UnityEngine;
using UnityEngine.InputSystem;

namespace RunCraft
{
    public enum Move { None, Left, Right, Up, Down }

    /// <summary>
    /// Lee los controles: deslizar el dedo (móvil), arrastrar con el ratón o teclado (PC).
    /// Devuelve un movimiento por fotograma como máximo.
    /// </summary>
    public class SwipeInput
    {
        bool tracking, fired;
        Vector2 start;

        // Distancia mínima del deslizamiento: un 5% de la pantalla
        float Threshold => Mathf.Max(30f, Mathf.Min(Screen.width, Screen.height) * 0.05f);

        public bool PausePressed()
        {
            var k = Keyboard.current;
            return k != null && (k.escapeKey.wasPressedThisFrame || k.pKey.wasPressedThisFrame);
        }

        public Move Read()
        {
            // --- Teclado ---
            var k = Keyboard.current;
            if (k != null)
            {
                if (k.leftArrowKey.wasPressedThisFrame || k.aKey.wasPressedThisFrame) return Move.Left;
                if (k.rightArrowKey.wasPressedThisFrame || k.dKey.wasPressedThisFrame) return Move.Right;
                if (k.upArrowKey.wasPressedThisFrame || k.wKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame) return Move.Up;
                if (k.downArrowKey.wasPressedThisFrame || k.sKey.wasPressedThisFrame) return Move.Down;
            }

            // --- Pantalla táctil ---
            var ts = Touchscreen.current;
            if (ts != null)
            {
                var touch = ts.primaryTouch;
                bool pressed = touch.press.isPressed;
                Vector2 pos = touch.position.ReadValue();
                var m = Track(pressed, pos);
                if (m != Move.None) return m;
                if (pressed) return Move.None; // el dedo manda; ignoramos el ratón
            }

            // --- Ratón (para probar en el editor arrastrando) ---
            var mouse = Mouse.current;
            if (mouse != null)
                return Track(mouse.leftButton.isPressed, mouse.position.ReadValue());

            return Move.None;
        }

        Move Track(bool pressed, Vector2 pos)
        {
            if (pressed && !tracking)
            {
                tracking = true;
                fired = false;
                start = pos;
                return Move.None;
            }
            if (!pressed)
            {
                tracking = false;
                return Move.None;
            }
            if (fired) return Move.None;

            Vector2 d = pos - start;
            if (d.magnitude < Threshold) return Move.None;
            fired = true; // un solo movimiento por deslizamiento
            if (Mathf.Abs(d.x) > Mathf.Abs(d.y)) return d.x > 0 ? Move.Right : Move.Left;
            return d.y > 0 ? Move.Up : Move.Down;
        }
    }
}
