using System.Collections.Generic;
using UnityEngine;

namespace LogKill.Character
{
    [RequireComponent(typeof(MeshFilter))]
    public class FieldOfView2D : MonoBehaviour
    {
        [Header("View Settings")]
        [SerializeField] private float _viewRadius = 5f;

        [Range(0, 360)]
        [SerializeField] private float _viewAngle = 90f;
        [SerializeField] private int _rayCount = 100;
        [SerializeField] private LayerMask _targetMask;
        [SerializeField] private LayerMask _obstacleMask;
        [SerializeField] private PlayerMovement _playerMovement;

        private Mesh _mesh;
        private Vector3 _origin;
        private float _startingAngle;
        private List<GameObject> _visibleTargets = new();

        private void Start()
        {
            _mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = _mesh;
        }

        private void LateUpdate()
        {
            _origin = transform.position;
            _startingAngle = _playerMovement.GetAimDirectionAngle();
            CreateViewMesh();
            FindVisibleTargets();
        }

        private void CreateViewMesh()
        {
            Vector3[] vertices = new Vector3[_rayCount + 2];
            int[] triangles = new int[_rayCount * 3];

            vertices[0] = Vector3.zero;

            float angleStep = _viewAngle / _rayCount;
            float angle = _startingAngle - _viewAngle / 2;

            for (int i = 0; i <= _rayCount; i++)
            {
                Vector3 dir = DirFromAngle(angle);
                RaycastHit2D hit = Physics2D.Raycast(_origin, dir, _viewRadius, _obstacleMask);

                Vector3 vertex = hit.collider == null
                    ? _origin + dir * _viewRadius
                    : (Vector3)hit.point;

                vertices[i + 1] = transform.InverseTransformPoint(vertex);

                if (i < _rayCount)
                {
                    int vertIndex = i + 1;
                    triangles[i * 3] = 0;
                    triangles[i * 3 + 1] = vertIndex + 1;
                    triangles[i * 3 + 2] = vertIndex;
                }

                angle += angleStep;
            }

            _mesh.Clear();
            _mesh.vertices = vertices;
            _mesh.triangles = triangles;
            _mesh.RecalculateNormals();
        }

        private void FindVisibleTargets()
        {
            var prevVisibleTargets = new List<GameObject>(_visibleTargets);
            _visibleTargets.Clear();

            Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(transform.position, _viewRadius, _targetMask);

            foreach (var col in targetsInViewRadius)
            {
                Transform target = col.transform;
                Vector3 dirToTarget = (target.position - transform.position).normalized;

                float angleToTarget = Vector3.Angle(GetForward(), dirToTarget);

                if (angleToTarget < _viewAngle / 2f)
                {
                    float distToTarget = Vector3.Distance(transform.position, target.position);

                    RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToTarget, distToTarget, _obstacleMask);
                    if (hit.collider == null)
                    {
                        SetRendererVisible(target.gameObject, true);
                    }
                    else
                    {
                        SetRendererVisible(target.gameObject, false);
                    }
                }
                else
                {
                    SetRendererVisible(target.gameObject, false);
                }
            }

            foreach (var target in prevVisibleTargets)
            {
                if (_visibleTargets.Contains(target) == false)
                    SetRendererVisible(target, false);
            }
        }

        private Vector3 GetForward()
        {
            return _playerMovement.MoveDirection;
        }

        private Vector3 DirFromAngle(float angle)
        {
            float rad = angle * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad));
        }

        private void SetRendererVisible(GameObject go, bool visible)
        {
            var player = go.GetComponentInChildren<Player>();
            if (player != null)
            {
                if (visible)
                    _visibleTargets.Add(go);
                else
                    _visibleTargets.Remove(go);
                player.SetRendererVisible(visible);
                return;
            }

            var spriteRenderer = go.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                if (visible)
                    _visibleTargets.Add(go);
                else
                    _visibleTargets.Remove(go);
                spriteRenderer.enabled = visible;
            }
        }
    }
}
