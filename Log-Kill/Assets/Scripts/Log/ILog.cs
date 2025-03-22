using UnityEngine;

namespace LogKill.Log
{
    public enum ELogType
    {
        None = 0,
        // Mission
        MissionFail = 1,
    }

    public interface ILog
    {
        public ELogType LogType { get; }
        public string Content { get; }
        public int Value { get; }
        public void AddLog(ILog log);
    }
}
