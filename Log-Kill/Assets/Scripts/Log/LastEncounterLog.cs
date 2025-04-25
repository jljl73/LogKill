using Unity.Collections;
using UnityEngine;

namespace LogKill.Log
{
    public class LastEncounterLog : ILog
    {
        public ELogType LogType => ELogType.LastEncounter;

        public string Content => string.Format("마지막으로 같이 있던 크루원: {0}", _playerName);
        public int Value { get; private set; } = 1;

        public float CriminalScore
        {
            get
            {
                return Value * 0.2f;
            }
        }

        private FixedString32Bytes _playerName = "Unknown";

        public LastEncounterLog(FixedString32Bytes playerName)
        {
            _playerName = playerName;
        }

        public void AddLog(ILog log)
        {
            if(log is LastEncounterLog lastEncounterLog)
            {
                _playerName = lastEncounterLog._playerName;
            }
        }
    }
}
