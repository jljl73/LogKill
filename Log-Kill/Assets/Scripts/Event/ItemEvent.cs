using UnityEngine;

namespace LogKill.Event
{
    public enum EItemType
    {
        Battery
    }

    public struct ItemPickupEvent
    {
        public EItemType ItemType;
    }

    public struct ItemUsedEvent
    {
        public EItemType ItemType;
        public int UsedCount;
    }

    public struct ItemChangedEvent
    {
        public EItemType ItemType;
        public int ItemCount;
    }
}
