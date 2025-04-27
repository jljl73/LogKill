using UnityEngine;

namespace LogKill.Log
{
    public class ImposterEncounterTimeLog : ILog
    {
        public ELogType LogType => ELogType.ImposterEncounterTime;
        public string Content => string.Format("임포스터와 같이 있던 시간: {0}초", Value);
        public int Value { get; private set; }
        public float CriminalScore => 0.3f;

        public ImposterEncounterTimeLog(float time)
        {
            Value = (int)time;
        }

        public void AddLog(ILog log)
        {
            Value += log.Value;
        }
    }
}
