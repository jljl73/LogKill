using UnityEngine;

namespace LogKill.Log
{
    public class CrewmateEncounterLog : ILog
    {
        public ELogType LogType => ELogType.CrewmateEncounter;

        public string Content => string.Format("ũ����� ����ģ Ƚ�� : {0}", Value);
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
