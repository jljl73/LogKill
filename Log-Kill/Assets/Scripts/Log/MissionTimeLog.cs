using UnityEngine;

namespace LogKill.Log
{
    public class MissionTimeLog : ILog
    {
        public ELogType LogType => ELogType.MissionTime;
        public string Content => string.Format("미션 하는데 총 걸린 시간: {0}", Value);
        public int Value { get; private set; }
        public float CriminalScore => 0.2f;

        public MissionTimeLog(float time)
        {
            Value = (int)time;
        }

        public void AddLog(ILog log)
        {
            Value += log.Value;
        }
    }
}
