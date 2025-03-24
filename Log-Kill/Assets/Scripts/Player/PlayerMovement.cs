using UnityEngine;

namespace LogKill.Character
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float _speed = 5f;

        private Rigidbody2D _rigid;
        public void Initialize()
        {
            _rigid = GetComponent<Rigidbody2D>();
        }

        public void Move(Vector2 direction)
        {
            Vector2 nextVec = direction * _speed * Time.fixedDeltaTime;
            _rigid.MovePosition(_rigid.position + nextVec);
        }
    }
}
