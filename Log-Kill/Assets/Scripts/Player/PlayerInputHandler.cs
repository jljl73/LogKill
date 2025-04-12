using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LogKill.Character
{
    public class PlayerInputHandler : NetworkBehaviour
    {
        public Vector2 MoveDirection { get; private set; } = Vector2.zero;

        public void Initialize()
        {
            GetComponent<PlayerInput>().SwitchCurrentControlScheme("Keyboard&Mouse", Keyboard.current, Mouse.current);
        }

        private void OnMove(InputValue value)
        {
            MoveDirection = value.Get<Vector2>();
        }

        public void DiabledInput()
        {
            GetComponent<PlayerInput>().enabled = false;
        }
    }
}
