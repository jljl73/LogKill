using LogKill.LobbySystem;
using LogKill.Vote;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.Character
{
    public class Player : NetworkBehaviour
    {
        private PlayerMovement _movement;
        private PlayerInputHandler _inputHandler;
        private PlayerAnimator _animator;
        private PlayerData _playerData = new();

        public PlayerData PlayerData { get ; private set; }

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _inputHandler = GetComponent<PlayerInputHandler>();
            _animator = GetComponent<PlayerAnimator>();
        }

        private void Update()
        {
            if (_playerData.IsDead) return;

            Vector2 moveDir = _inputHandler.MoveDirection;
            _animator.UpdateSpeed(moveDir);

            if (Input.GetKeyDown(KeyCode.R))
            {
                OnDead();
            }
        }

        private void FixedUpdate()
        {
            _movement.Move(_inputHandler.MoveDirection);
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                enabled = false;
                return;
            }

            _movement.Initialize();
            _inputHandler.Initialize(IsOwner);
            _animator.Initialize();
            CameraController.Instance.SetTarget(transform);
        }

        public void OnDead()
        {
            if (_playerData.IsDead)
                return;

            _playerData.IsDead = true;

            _animator.PlayDeadAnimation();
            _inputHandler.DiabledInput();
            CameraController.Instance.SetTarget(null);
        }

        private void GameStartInitialize()
        {
            Debug.Log("ClientId : " + NetworkManager.Singleton.LocalClientId);

            if (!IsOwner) return;

            _playerData.Initialize(EColorType.Red, NetworkManager.Singleton.LocalClientId.ToString());
            DebugPlayerDataManager.Instance.SubmitPlayerDataToServerRpc(_playerData);
        }
    }
}
