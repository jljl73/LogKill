using LogKill.Core;
using LogKill.Event;
using LogKill.Log;
using System.Collections.Generic;
using UnityEngine;

namespace LogKill.Item
{
    public class ItemService : IService
    {
        public Dictionary<EItemType, int> Items { get; private set; } = new();

        private EventBus EventBus => ServiceLocator.Get<EventBus>();
        private LogService LogService => ServiceLocator.Get<LogService>();

        public void Initialize()
        {
            EventBus.Subscribe<ItemPickupEvent>(OnItemPickupEvent);
            EventBus.Subscribe<ItemUsedEvent>(OnItemUsedEvent);
        }

        public int GetItemAmount(EItemType itemType)
        {
            if (!Items.ContainsKey(itemType))
                return 0;

            return Items[itemType];
        }

        private void OnItemPickupEvent(ItemPickupEvent context)
        {
            if (!Items.ContainsKey(context.ItemType))
                Items.Add(context.ItemType, 0);

            Items[context.ItemType]++;
            int itemCount = Items[context.ItemType];

            EventBus.Publish<ItemChangedEvent>(new ItemChangedEvent()
            {
                ItemType = context.ItemType,
                ItemCount = itemCount
            });

            LogService.Log(new ItemAcquireLog(context.ItemType, 1));
        }

        private void OnItemUsedEvent(ItemUsedEvent context)
        {
            if (!Items.ContainsKey(context.ItemType))
                return;

            Items[context.ItemType] = Mathf.Max(0, Items[context.ItemType] - context.UsedCount);
            int itemCount = Items[context.ItemType];

            EventBus.Publish<ItemChangedEvent>(new ItemChangedEvent()
            {
                ItemType = context.ItemType,
                ItemCount = itemCount
            });

            LogService.Log(new ItemUseLog(context.ItemType, context.UsedCount));
        }
    }
}
