using System.Collections.Generic;
using LogKill.Event;

namespace LogKill.Log
{
    public class ItemUseLog : ILog
    {
        public ELogType LogType => ELogType.ItemUse;

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

        public ItemUseLog(EItemType itemType, int value)
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
            if (log is ItemUseLog itemUseLog)
            {
                if (_itemDicts.ContainsKey(itemUseLog.ItemType))
                    _itemDicts[itemUseLog.ItemType] += itemUseLog.Value;
                else
                    _itemDicts.Add(itemUseLog.ItemType, itemUseLog.Value);
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

            return string.Format("[{0}] {1}개 사용", maxItemType, maxValue);
        }
    }
}
