using LogKill.Core;
using LogKill.Event;
using System.Collections.Generic;
using UnityEngine;

namespace LogKill.Item
{
    public class BatterySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _batteryPrefab;
        [SerializeField] private List<Transform> _spawnedPoints = new();

        private EventBus EventBus => ServiceLocator.Get<EventBus>();

        public void Initialize()
        {
            EventBus.Subscribe<ItemPickupEvent>(OnItemPickupEvent);
        }

        public void Dipose()
        {
            EventBus.Unsubscribe<ItemPickupEvent>(OnItemPickupEvent);
        }

        public void Spawn()
        {
            int randomIndex = Random.Range(0, _spawnedPoints.Count);

            var spawnPoint = _spawnedPoints[randomIndex];
            Instantiate(_batteryPrefab, spawnPoint.position, Quaternion.identity, spawnPoint);
        }

        private void OnItemPickupEvent(ItemPickupEvent context)
        {
            if (context.ItemType == EItemType.Battery)
            {
                Spawn();
            }
        }
    }
}
