using LogKill.Log;
using UnityEngine;

namespace LogKill
{
    public class MapEnterLog : ILog
    {
        public ELogType LogType => ELogType.MapEnterLog;

        public string Content => string.Format("마지막으로 있던 장소: {0}", ZoneName);

        public int Value => 0;

        public float CriminalScore => 0.5f;

        public string ZoneName { get; private set; } = "Unknown";

        public MapEnterLog(string zoneName)
        {
            ZoneName = zoneName;
        }

        public void AddLog(ILog log)
        {
            if (log is MapEnterLog mapEnterLog)
            {
                ZoneName = mapEnterLog.ZoneName;
            }
        }
    }
}
