using UnityEngine;
using UnityEngine.InputSystem;

namespace LogKill.Character
{
    public class PlayerInputHandler : MonoBehaviour
    {
        private bool _isOwner;

        public Vector2 MoveDirection { get; private set; } = Vector2.zero;

        public void Initialize(bool isOwner)
        {
            _isOwner = isOwner;
            if (_isOwner)
            {
                GetComponent<PlayerInput>().SwitchCurrentControlScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);
            }
        }

        private void OnMove(InputValue value)
        {
            if (!_isOwner)
                return;

            MoveDirection = value.Get<Vector2>();
        }

        public void DiabledInput()
        {
            enabled = true;
        }
    }
}
