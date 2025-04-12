using UnityEngine;

namespace LogKill.Log
{
    public class MissionSuccessLog : ILog
    {
        public ELogType LogType => ELogType.MissionSuccess;

        public string Content => string.Format("미션 성공: {0}", Value);
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
