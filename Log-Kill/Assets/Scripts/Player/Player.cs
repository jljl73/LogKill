using Unity.Netcode;
using UnityEngine;

namespace LogKill.Character
{
    public class Player : NetworkBehaviour
    {
        private PlayerMovement _movement;
        private PlayerInputHandler _inputHandler;
        private PlayerAnimator _animator;

        public bool IsDead { get; private set; } = false;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _inputHandler = GetComponent<PlayerInputHandler>();
            _animator = GetComponent<PlayerAnimator>();
        }

        private void Update()
        {
            if (IsDead)
                return;

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
            if (IsDead)
                return;

            IsDead = true;

            _animator.PlayDeadAnimation();
            _inputHandler.DiabledInput();
            CameraController.Instance.SetTarget(null);
        }
    }
}
