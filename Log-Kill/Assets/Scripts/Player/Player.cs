using LogKill.Core;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.Character
{
    public class Player : NetworkBehaviour
    {
        private PlayerMovement _movement;
        private PlayerInputHandler _inputHandler;
        private PlayerAnimator _animator;
        private PlayerNetworkSync _networkSync;

        private PlayerData _playerData;

        private EventBus EventBus => ServiceLocator.Get<EventBus>();

        public bool IsDead => _playerData.IsDead;
        public ulong ClientId => _playerData.ClientId;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _inputHandler = GetComponent<PlayerInputHandler>();
            _animator = GetComponent<PlayerAnimator>();
            _networkSync = GetComponent<PlayerNetworkSync>();
        }

        private void Update()
        {
            if (_playerData.IsDead) return;

            Vector2 moveDir = _inputHandler.MoveDirection;
            _animator.UpdateSpeed(moveDir);
            _networkSync.UpdateDirection(moveDir);

            if (Input.GetKeyDown(KeyCode.R))
            {
                OnDead();
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                PlayerDataManager.Instance.RequestPlayerDataServerRpc();
            }
        }

        private void FixedUpdate()
        {
            _movement.Move(_inputHandler.MoveDirection);
        }

        public override void OnNetworkSpawn()
        {
            _animator.Initialize();
            _networkSync.Initialize();

            if (IsOwner)
            {
                _movement.Initialize();
                _inputHandler.Initialize(IsOwner);

                ulong clientId = NetworkManager.Singleton.LocalClientId;
                string playerName = $"Player {clientId}";
                _playerData = new PlayerData(clientId, playerName);
                EventBus.Publish<PlayerData>(_playerData);

                _networkSync.UpdateColorType(_playerData.ColorType);

                CameraController.Instance.SetTarget(transform);
            }
            else
            {
                enabled = false;
            }
        }

        public void OnDead()
        {
            if (_playerData.IsDead)
                return;

            _playerData.IsDead = true;
            EventBus.Publish<PlayerData>(_playerData);

            _animator.PlayDeadAnimation();
            _inputHandler.DiabledInput();
            CameraController.Instance.SetTarget(null);
        }
    }
}
