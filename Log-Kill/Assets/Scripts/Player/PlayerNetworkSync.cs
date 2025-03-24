using Unity.Netcode;
using UnityEngine;

namespace LogKill.Character
{
    public class PlayerNetworkSync : NetworkBehaviour
    {
        private PlayerInputHandler _inputHandler;

        private SpriteRenderer _renderer;

        private NetworkVariable<bool> _netFlipX = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private void Update()
        {
            UpdateDirection(_inputHandler.MoveDirection);
        }

        public void Initialize()
        {
            _inputHandler = GetComponent<PlayerInputHandler>();

            _renderer = GetComponent<SpriteRenderer>();
            _renderer.flipX = _netFlipX.Value;

            _netFlipX.OnValueChanged += (oldValue, newValue) =>
            {
                _renderer.flipX = newValue;
            };
        }

        public override void OnNetworkSpawn()
        {
            Initialize();

            enabled = IsOwner;
        }

        public void UpdateDirection(Vector2 moveDir)
        {
            if (moveDir.x != 0)
            {
                _netFlipX.Value = moveDir.x > 0;
            }
        }
    }
}
