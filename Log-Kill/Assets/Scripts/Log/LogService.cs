using System.Collections.Generic;
using System.Linq;
using LogKill.Core;
using UnityEngine;

namespace LogKill.Log
{
    public class LogService : IService
    {
        private Dictionary<ELogType, ILog> _logDicts = new();

        private System.Random _random = new();

        public void Initialize()
        {
        }

        public void Log(ILog newLog)
        {
            if (_logDicts.TryGetValue(newLog.LogType, out var log))
            {
                log.AddLog(newLog);
            }
            else
            {
                _logDicts.Add(newLog.LogType, newLog);
            }

            Debug.Log(newLog.Content);
        }

        public void Print(ELogType logType)
        {
            if (_logDicts.TryGetValue(logType, out var log))
                Debug.Log(log.Content);
        }

        public List<string> GetRandomLogList(int count = 3)
        {
            List<string> logList = _logDicts.Values
                .Select(log => log.Content)
                .ToList();

            // Suffle
            int logCount = logList.Count;
            for (int i = 0; i < logCount - 1; i++)
            {
                int j = _random.Next(i, logCount);
                (logList[i], logList[j]) = (logList[j], logList[i]);
            }

            int limitCount = Mathf.Min(logCount, count);
            return logList.GetRange(0, limitCount);
        }
    }
}
