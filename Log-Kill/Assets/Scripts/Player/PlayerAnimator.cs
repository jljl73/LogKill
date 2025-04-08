using LogKill.Network;
using UnityEngine;

namespace LogKill.Character
{
    public class PlayerAnimator : MonoBehaviour
    {
        public RuntimeAnimatorController[] RuntimeAnimatorControllers;

        private Animator _anim;
        private ClientNetworkAnimator _clientNetworkAnimator;

        public void Initialize()
        {
            _anim = GetComponent<Animator>();
            _clientNetworkAnimator = GetComponent<ClientNetworkAnimator>();
        }

        public void UpdateSpeed(Vector2 moveDir)
        {
            _anim.SetFloat("Speed", moveDir.magnitude);
        }

        public void PlayDeadAnimation()
        {
            _anim.SetTrigger("Dead");
        }

        public void SetPlayerColor(EColorType colorType)
        {
            _anim.runtimeAnimatorController = RuntimeAnimatorControllers[(int)colorType];
            _clientNetworkAnimator.Animator = _anim;
        }
    }
}
