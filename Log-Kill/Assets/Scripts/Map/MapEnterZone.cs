using LogKill.Core;
using LogKill.Log;
using NUnit.Framework.Constraints;
using UnityEngine;

namespace LogKill
{
    public class MapEnterZone : MonoBehaviour
    {
        [SerializeField] private string _zoneName;
        
        private LogService LogService => ServiceLocator.Get<LogService>();

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                LogService.Log(new MapEnterLog(_zoneName));
            }
        }
    }
}
