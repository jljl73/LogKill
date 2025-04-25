using System.Collections.Generic;
using LogKill.Event;

namespace LogKill.Log
{
    public class ItemAcquireLog : ILog
    {
        public ELogType LogType => ELogType.ItemAcquire;

        public string Content => GetMaxItem();
        public int Value { get; private set; } = 1;

        private Dictionary<EItemType, int> _itemDicts = new();
        public EItemType ItemType { get; private set; }

        public float CriminalScore
        {
            get
            {
                return Value * 0.2f;
            }
        }

        public ItemAcquireLog(EItemType itemType, int value)
        {
            ItemType = itemType;
            Value = value;
            if (_itemDicts.ContainsKey(itemType))
                _itemDicts[itemType] += value;
            else
                _itemDicts.Add(itemType, value);
        }

        public void AddLog(ILog log)
        {
            if (log is ItemAcquireLog itemAcquireLog)
            {
                if (_itemDicts.ContainsKey(itemAcquireLog.ItemType))
                    _itemDicts[itemAcquireLog.ItemType] += itemAcquireLog.Value;
                else
                    _itemDicts.Add(itemAcquireLog.ItemType, itemAcquireLog.Value);
            }
        }

        private string GetMaxItem()
        {
            int maxValue = 0;
            EItemType maxItemType = EItemType.Battery;

            foreach (var item in _itemDicts)
            {
                if (item.Value > maxValue)
                {
                    maxValue = item.Value;
                    maxItemType = item.Key;
                }
            }

            return string.Format("[{0}] {1}개 획득", maxItemType, maxValue);
        }
    }
}
