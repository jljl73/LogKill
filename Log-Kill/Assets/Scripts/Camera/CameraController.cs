using LogKill.Core;
using UnityEngine;

namespace LogKill
{
    // 나중에 ServiceLocator로 바꿔야 함
    public class CameraController : MonoSingleton<CameraController>
    {
        [SerializeField] private float _speed = 5.0f;
        [SerializeField] private Vector3 _offset = new Vector3(0, 1, 0);
        private Transform _target;

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        private void LateUpdate()
        {
            if (_target == null)
                return;

            Vector3 desiredPosition = _target.position + _offset;
            desiredPosition.z = transform.position.z;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * _speed);
        }
    }
}
