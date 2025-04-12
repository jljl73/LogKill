using LogKill.Core;
using LogKill.Entity;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.Character
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] private InteractableTrigger _interactableTrigger;

        private PlayerMovement _movement;
        private PlayerInputHandler _inputHandler;
        private PlayerAnimator _animator;
        private PlayerNetworkSync _networkSync;

        private PlayerData _playerData;
        private EventBus EventBus => ServiceLocator.Get<EventBus>();

        public PlayerData PlayerData => _playerData;
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
                // OnDead();
            }
        }

        private void FixedUpdate()
        {
            if (_movement)
                _movement.Move(_inputHandler.MoveDirection);
        }

        public override void OnNetworkSpawn()
        {
            Initialize();
            PlayerDataManager.Instance.AddPlayer(this);
        }

        private void Initialize()
        {
            EventBus.Subscribe<PlayerKillEvent>(OnDead);

            _animator.Initialize();
            _networkSync.Initialize();
            _playerData = new PlayerData(OwnerClientId);

            if (IsOwner)
            {
                _movement.Initialize();
                _inputHandler.Initialize();

                PlayerDataManager.Instance.SubmitPlayerDataToServerRpc(_playerData);

                _networkSync.UpdateColorType(_playerData.ColorType);
                _interactableTrigger.Initalize(this);
                _interactableTrigger.gameObject.SetActive(true);

                CameraController.Instance.SetTarget(transform);
            }
            else
            {
                enabled = false;
                _interactableTrigger.gameObject.SetActive(false);
            }
        }

        public void OnDead(PlayerKillEvent context)
        {
            if (_playerData.IsDead || context.VictimId != _playerData.ClientId)
                return;

            _playerData.IsDead = true;
            EventBus.Publish<PlayerData>(_playerData);

            _animator.PlayDeadAnimation();
            _inputHandler.DiabledInput();

            if (IsOwner)
            {
                _interactableTrigger.gameObject.SetActive(false);

                var target = PlayerDataManager.Instance.GetRandomAlivePlayer();
                CameraController.Instance.SetTarget(target?.transform);
            }
        }
    }
}
