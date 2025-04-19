using LogKill.UI;
using UnityEngine;

namespace LogKill.Character
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float _speed = 5f;

        private Rigidbody2D _rigid;
        private Vector2 _moveDir;
        private float _povAngle;

        public void Initialize()
        {
            _rigid = GetComponent<Rigidbody2D>();
        }

        public void Move(Vector2 direction)
        {
            if (UIManager.Instance.IsWindowOpend)
                return;

            Vector2 nextVec = direction * _speed;
            _rigid.MovePosition(_rigid.position + nextVec * Time.fixedDeltaTime);

            if (nextVec.sqrMagnitude > 0.01f)
                _moveDir = nextVec;
        }

        public float GetAimDirectionAngle()
        {
            _povAngle = Mathf.LerpAngle(_povAngle, Mathf.Atan2(_moveDir.y, _moveDir.x) * Mathf.Rad2Deg, 0.1f);
            return _povAngle;
        }
    }
}
