using ACaldeira.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ACaldeira.Input
{
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        private Vector2 touchOrigin;
        private bool dragging;
        public Vector2 ReadMovement()
        {
            Vector2 v = Vector2.zero;
            Keyboard k = Keyboard.current;
            if (k != null)
            {
                v.x = (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1 : 0) - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1 : 0);
                v.y = (k.wKey.isPressed || k.upArrowKey.isPressed ? 1 : 0) - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1 : 0);
            }
            if (Gamepad.current != null) v += Gamepad.current.leftStick.ReadValue();
            if (Touchscreen.current != null)
            {
                var t = Touchscreen.current.primaryTouch;
                if (t.press.wasPressedThisFrame && t.position.ReadValue().x < Screen.width * 0.5f)
                { touchOrigin = t.position.ReadValue(); dragging = true; }
                if (!t.press.isPressed) dragging = false;
                if (dragging) v += Vector2.ClampMagnitude((t.position.ReadValue() - touchOrigin) / 90f, 1f);
            }
            return Vector2.ClampMagnitude(v, 1f);
        }
        private void Update()
        {
            bool pressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
            pressed |= Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;
            if (pressed)
            {
                if (gameManager.State == Data.GameState.Playing) gameManager.Pause();
                else if (gameManager.State == Data.GameState.Paused) gameManager.Resume();
            }
        }
    }
}
