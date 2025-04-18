using UnityEngine;

namespace LogKill.Log
{
    public class IgnoredBodyLog : ILog
    {
        public ELogType LogType => ELogType.IgnoredBody;

        public string Content => string.Format("��ü�� {0}�� ���������� �Ű����� ����", Value);
        public int Value { get; private set; } = 0;
        public float CriminalScore
        {
            get
            {
                return Value * 0.5f;
            }
        }

        public void AddLog(ILog log)
        {
            Value += log.Value;
        }
    }
}
