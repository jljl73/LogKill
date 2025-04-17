using Unity.Netcode;
using UnityEngine;

namespace LogKill.Character
{
    public class PlayerNetworkSync : NetworkBehaviour
    {
        private SpriteRenderer _renderer;

        private NetworkVariable<bool> _netFlipX = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public void Initialize()
        {
            _renderer = GetComponent<SpriteRenderer>();

            _netFlipX.OnValueChanged += (oldValue, newValue) =>
            {
                _renderer.flipX = newValue;
            };
            _renderer.flipX = _netFlipX.Value;
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
