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
        [SerializeField] private LayerMask _obstacleMask;
        [SerializeField] private PlayerMovement _playerMovement;

        private Mesh _mesh;
        private Vector3 _origin;
        private float _startingAngle;

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

        private Vector3 DirFromAngle(float angle)
        {
            float rad = angle * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad));
        }
    }
}
