using UnityEngine;

namespace LogKill.Log
{
    public class KillWitnessLog : ILog
    {
        public ELogType LogType => ELogType.KillWitness;

        public string Content => string.Format("살인 목격한 횟수: {0}", Value);

        public int Value { get; private set; } = 1;

        public float CriminalScore => Value;

        public void AddLog(ILog log)
        {
            Value += log.Value;
        }
    }
}
