using System.Collections.Generic;
using LogKill.Core;
using UnityEngine;

namespace LogKill.Log
{
    public class LogService : IService
    {
        private Dictionary<ELogType, ILog> _logDicts = new();

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
        }

        public void Print(ELogType logType)
        {
            if (_logDicts.TryGetValue(logType, out var log))
                Debug.Log(log.Content);
        }
    }
}
