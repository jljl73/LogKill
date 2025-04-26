using System.Collections.Generic;
using System.Linq;
using LogKill.Core;
using LogKill.Event;
using LogKill.Utils;
using UnityEngine;

namespace LogKill.Log
{
    public class LogService : IService
    {
        private Dictionary<ELogType, ILog> _logDicts = new();
        private List<ILog> _permenantLogs = new List<ILog>();

        public void Initialize()
        {
            _permenantLogs.Clear();
            ServiceLocator.Get<EventBus>().Subscribe<VoteEndEvent>(OnVoteEndEvent);
            ServiceLocator.Get<EventBus>().Subscribe<DisposeEvent>(Dispose);
        }

        public void Dispose(DisposeEvent _)
        {
            _logDicts.Clear();
            _permenantLogs.Clear();
            Clear();
        }

        private void OnVoteEndEvent(VoteEndEvent _)
        {
            Debug.Log("All logs cleared.");
            Clear();
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

        public void Clear()
        {
            foreach (var log in _logDicts.Values)
            {
                _permenantLogs.Add(log);
            }
            _logDicts.Clear();
        }

        public void Print(ELogType logType)
        {
            if (_logDicts.TryGetValue(logType, out var log))
                Debug.Log(log.Content);
        }

        public List<string> GetCreminalScoreWeightedRandomLogList(int count = 3)
        {
            var result = new List<string>();
            var logCandidates = new List<ILog>(_logDicts.Values);

            if (logCandidates.Count == 0)
                return result;

            count = Mathf.Min(count, logCandidates.Count);

            Util.Suffle<ILog>(logCandidates);

            for (int i = 0; i < count; i++)
            {
                float totalWeight = logCandidates.Sum(log => log.CriminalScore);
                float randWeight = Random.Range(0f, totalWeight);

                float cumulative = 0f;
                for (int j = 0; j < logCandidates.Count; j++)
                {
                    cumulative += logCandidates[j].CriminalScore;

                    if (randWeight <= cumulative)
                    {
                        result.Add(logCandidates[j].Content);
                        logCandidates.RemoveAt(j);
                        break;
                    }
                }
            }

            return result;
        }

        public List<string> GetRandomLogList(int count = 3)
        {
            var result = new List<string>();
            var logList = new List<ILog>(_logDicts.Values);

            if (count == 0)
                return result;

            count = Mathf.Min(count, logList.Count);

            Util.Suffle<ILog>(logList);

            for (int i = 0; i < count; i++)
            {
                result.Add(logList[i].Content);
            }

            return result;
        }
    }
}
