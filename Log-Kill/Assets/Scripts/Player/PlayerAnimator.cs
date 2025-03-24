using UnityEngine;

namespace LogKill.Character
{
    public class PlayerAnimator : MonoBehaviour
    {
        private Animator _anim;

        public void Initialize()
        {
            _anim = GetComponent<Animator>();
        }

        public void UpdateSpeed(Vector2 moveDir)
        {
            _anim.SetFloat("Speed", moveDir.magnitude);
        }

        public void PlayDeadAnimation()
        {
            _anim.SetTrigger("Dead");
        }
    }
}
