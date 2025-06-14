using System;
using LogKill.Core;
using LogKill.Entity;
using LogKill.Event;
using LogKill.Log;
using LogKill.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LogKill.Character
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private InteractableTrigger _interactableTrigger;
        [SerializeField] private EncounterTrigger _encounterTrigger;
        [SerializeField] private FieldOfView2D _fieldOfView2D;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private PlayerMovement _movement;
        private PlayerInputHandler _inputHandler;
        private PlayerAnimator _animator;
        private PlayerNetworkSync _networkSync;

        private PlayerData _playerData;
        private NetworkVariable<EColorType> _colorType = new NetworkVariable<EColorType>(EColorType.White, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public PlayerData PlayerData => _playerData;
        public EPlayerType PlayerType => _playerData.PlayerType;
        public EColorType ColorType => _colorType.Value;
        public bool IsDead => _playerData.IsDead;
        public ulong ClientId => _playerData.ClientId;
        public SpriteRenderer SpriteRenderer => _spriteRenderer;

        private EventBus EventBus => ServiceLocator.Get<EventBus>();
        private LogService LogService => ServiceLocator.Get<LogService>();

        public Action<Player, bool> OnDisableRender { get; set; }

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

        public override void OnNetworkDespawn()
        {
            EventBus.Unsubscribe<PlayerKillEvent>(OnDead);
            EventBus.Unsubscribe<SettingImposterEvent>(OnSettingImposter);

            PlayerDataManager.Instance.RemovePlayer(OwnerClientId);
        }

        private void Initialize()
        {
            EventBus.Subscribe<PlayerKillEvent>(OnDead);
            EventBus.Subscribe<SettingImposterEvent>(OnSettingImposter);

            _animator.Initialize();
            _networkSync.Initialize();

            _playerData = new PlayerData(OwnerClientId, _colorType.Value, _colorType.Value.ToString());
            _animator.SetPlayerColor(_playerData.ColorType);

            if (IsOwner)
            {
                _playerInput.enabled = true;
                _movement.Initialize();
                _inputHandler.Initialize();

                _interactableTrigger.Initalize(this);
                _interactableTrigger.gameObject.SetActive(true);
                _encounterTrigger.Initalize(this);
                _encounterTrigger.gameObject.SetActive(true);
                _fieldOfView2D.gameObject.SetActive(true);

                CameraController.Instance.SetTarget(transform);
                gameObject.tag = "Player";
                LogService.Log(new NothingLog());
            }
            else
            {
                _playerInput.enabled = false;
                gameObject.tag = "Untagged";
                enabled = false;
                _interactableTrigger.gameObject.SetActive(false);
                _encounterTrigger.gameObject.SetActive(false);
                _fieldOfView2D.gameObject.SetActive(false);
            }
        }

        public void Die()
        {
            _playerData.IsDead = true;
        }

        public void OnDead(PlayerKillEvent context)
        {
            if (_playerData.IsDead || context.VictimId != _playerData.ClientId)
                return;

            Die();

            if (IsOwner == false && _spriteRenderer.enabled)
            {
                LogService.Log(new KillWitnessLog());
            }

            _animator.PlayDeadAnimation();
            _inputHandler.DiabledInput();

            if (IsOwner)
            {
                _interactableTrigger.gameObject.SetActive(false);
                _encounterTrigger.gameObject.SetActive(false);

                PlayerDataManager.Instance.WatchRandomAlivePlayer();
                SoundManager.Instance.PlaySFX(ESFX.Death);

                if (context.IsBreak)
                    UIManager.Instance.ShowWindow<DeathWindow>();
            }

            if (PlayerDataManager.Instance.CheckGameOver(out bool isImposterWin))
                UIManager.Instance.ShowWindow<GameResultWindow>().ShowResult(isImposterWin);
        }

        public void SetColor(EColorType colorType)
        {
            _colorType.Value = colorType;
        }

        public void ShowFov(bool isShow)
        {
            _fieldOfView2D.gameObject.SetActive(isShow);
        }

        public void SetRendererVisible(bool visible)
        {
            if (_spriteRenderer.enabled != visible)
                OnDisableRender?.Invoke(this, !visible);
            
            _spriteRenderer.enabled = visible;
        }

        private void OnSettingImposter(SettingImposterEvent context)
        {
            if (context.ClientId != _playerData.ClientId)
                return;

            _playerData.PlayerType = EPlayerType.Imposter;
        }
    }
}