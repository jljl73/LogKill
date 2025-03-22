using UnityEngine;

namespace LogKill.Log
{
    public class MissionFailLog : ILog
    {
        public ELogType LogType => ELogType.MissionFail;

        public string Content => string.Format("미션 실패: {0}", Value);
        public int Value { get; private set; } = 1;
        public void AddLog(ILog log)
        {
            Value += log.Value;
        }
    }
}
