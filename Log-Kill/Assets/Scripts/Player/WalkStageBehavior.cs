using LogKill.Core;
using UnityEngine;

namespace LogKill.Character.State
{
    public class WalkStageBehavior : StateMachineBehaviour
    {
        private Player _player;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _player ??= animator.GetComponent<Player>();
            if (_player.IsOwner)
                SoundManager.Instance.PlaySFXLoop(ESFX.Walking, 2.0f);
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_player.IsOwner)
                SoundManager.Instance.StopSFX();
        }
    }
}
