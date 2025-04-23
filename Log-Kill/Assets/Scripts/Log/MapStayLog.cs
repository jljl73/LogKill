using System.Collections.Generic;

namespace LogKill.Log
{
    public class MapStayLog : ILog
    {
        public ELogType LogType => ELogType.MapStayLog;

        public string Content
        {
            get
            {
                float maxStayTime = 0;
                foreach (var zone in _zoneStayTime)
                {
                    if (zone.Value > maxStayTime)
                    {
                        maxStayTime = zone.Value;
                        ZoneName = zone.Key;
                    }
                }

                return string.Format("가장 오래 머문 장소:{0}, {1}초", ZoneName, maxStayTime.ToString("N0"));
            }
        }

        public string ZoneName { get; private set; } = "Unknown";
        public float StayTime => _zoneStayTime[ZoneName];

        public int Value => 0;

        public float CriminalScore => 0.5f;


        private Dictionary<string, float> _zoneStayTime = new Dictionary<string, float>();

        public MapStayLog(string zoneName, float stayTime)
        {
            ZoneName = zoneName;
            _zoneStayTime.Add(zoneName, stayTime);
        }

        public void AddLog(ILog log)
        {
            if (log is MapStayLog mapEnterLog)
            {
                if (_zoneStayTime.ContainsKey(mapEnterLog.ZoneName))
                {
                    _zoneStayTime[mapEnterLog.ZoneName] += mapEnterLog.StayTime;
                }
                else
                {
                    _zoneStayTime.Add(mapEnterLog.ZoneName, mapEnterLog.StayTime);
                }
            }
        }
    }
}
