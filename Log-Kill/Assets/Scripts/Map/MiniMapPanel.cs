using System.Collections;
using LogKill.Character;
using UnityEngine;

namespace LogKill.Map.UI
{
    public class MiniMapPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform _marker;

        private Transform _playerTransform;

        private void OnEnable()
        {
            _playerTransform = PlayerDataManager.Instance.Me.transform;
            StartCoroutine(OnMap());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator OnMap()
        {
            while (gameObject.activeSelf)
            {
                _marker.anchoredPosition = _playerTransform.position * 20.0f;
                yield return null;
            }
        }
    }
}
