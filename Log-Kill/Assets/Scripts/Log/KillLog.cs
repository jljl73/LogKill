using UnityEngine;

namespace LogKill.Log
{
    public class KillLog : ILog
    {
        public ELogType LogType => ELogType.PlayerKill;

        public string Content => string.Format("죽인 횟수: {0}", Value);

        public int Value { get; private set; } = 1;

        public float CriminalScore => Value;

        public void AddLog(ILog log)
        {
            Value += log.Value;
        }
    }
}
