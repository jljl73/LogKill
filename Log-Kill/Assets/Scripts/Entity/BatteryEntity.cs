using LogKill.Core;
using LogKill.Event;
using LogKill.Item;
using UnityEngine;

namespace LogKill.Entity
{
    public class BatteryEntity : MonoBehaviour, IItem
    {
        private EventBus EventBus => ServiceLocator.Get<EventBus>();

        public void PickUp()
        {
            EventBus.Publish(new ItemPickupEvent() { ItemType = EItemType.Battery });
            gameObject.SetActive(false);
        }
    }
}