using LogKill.Core;
using LogKill.Entity;
using LogKill.Event;
using System.Collections.Generic;
using UnityEngine;

namespace LogKill.Item
{
    public class BatterySpawner : MonoBehaviour
    {
        [SerializeField] private List<BatteryEntity> _spawnedBatterys = new();

        private EventBus EventBus => ServiceLocator.Get<EventBus>();

        public void Initialize()
        {
            EventBus.Subscribe<ItemPickupEvent>(OnItemPickupEvent);

            foreach (var battery in _spawnedBatterys)
            {
                battery.gameObject.SetActive(false);
            }
        }

        public void Dipose()
        {
            EventBus.Unsubscribe<ItemPickupEvent>(OnItemPickupEvent);
        }

        public void Spawn()
        {
            var disabledMissions = _spawnedBatterys.FindAll(obj => !obj.gameObject.activeSelf);

            int randomIndex = Random.Range(0, disabledMissions.Count);

            disabledMissions[randomIndex].gameObject.SetActive(true);
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
