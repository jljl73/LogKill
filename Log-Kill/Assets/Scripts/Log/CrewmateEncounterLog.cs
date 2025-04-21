using UnityEngine;

namespace LogKill.Log
{
    public class CrewmateEncounterLog : ILog
    {
        public ELogType LogType => ELogType.CrewmateEncounter;

        public string Content => string.Format("크루원과 마주친 횟수 : {0}", Value);
        public int Value { get; private set; } = 1;

        public float CriminalScore
        {
            get
            {
                return Value * 0.2f;
            }
        }

        public void AddLog(ILog log)
        {
            Value += log.Value;
        }
    }
}
