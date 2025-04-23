using LogKill.Core;
using LogKill.Event;
using LogKill.Log;
using NUnit.Framework.Constraints;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

namespace LogKill
{
    public class MapEnterZone : MonoBehaviour
    {
        [SerializeField] private string _zoneName;

        private LogService LogService => ServiceLocator.Get<LogService>();
        private bool _isPlayerInZone = false;
        private float _zoneEnterTime = 0f;

        private void Start()
        {
            ServiceLocator.Get<EventBus>().Subscribe<VoteStartEvent>(OnVoteStart);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                LogService.Log(new MapEnterLog(_zoneName));
                _isPlayerInZone = true;
                _zoneEnterTime = Time.time;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") && _isPlayerInZone)
            {
                LogService.Log(new MapStayLog(_zoneName, Time.time - _zoneEnterTime));
                _isPlayerInZone = false;
            }
        }

        private void OnVoteStart(VoteStartEvent context)
        {
            if (_isPlayerInZone)
            {
                LogService.Log(new MapStayLog(_zoneName, Time.time - _zoneEnterTime));
                _isPlayerInZone = false;
            }
        }
    }
}
