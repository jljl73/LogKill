using Unity.Netcode;
using UnityEngine;

namespace LogKill.Character
{
    public class PlayerNetworkSync : NetworkBehaviour
    {
        private PlayerAnimator _playerAnimator;

        private SpriteRenderer _renderer;

        private NetworkVariable<bool> _netFlipX = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkVariable<EColorType> _netColorType = new NetworkVariable<EColorType>(EColorType.White, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public void Initialize()
        {
            _playerAnimator = GetComponent<PlayerAnimator>();

            _renderer = GetComponent<SpriteRenderer>();

            _netFlipX.OnValueChanged += (oldValue, newValue) =>
            {
                _renderer.flipX = newValue;
            };
            _renderer.flipX = _netFlipX.Value;


            _playerAnimator.SetPlayerColor(_netColorType.Value);
            _netColorType.OnValueChanged += (oldValue, newValue) =>
            {
                _playerAnimator.SetPlayerColor(newValue);
            };
        }

        public void UpdateDirection(Vector2 moveDir)
        {
            if (moveDir.x != 0)
            {
                _netFlipX.Value = moveDir.x > 0;
            }
        }

        public void UpdateColorType(EColorType colorType)
        {
            _netColorType.Value = colorType;
        }
    }
}
