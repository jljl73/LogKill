namespace LogKill.Log
{
    public class NothingLog : ILog
    {
        public ELogType LogType => ELogType.None;

        public string Content => "아무것도 하지않음";
        public int Value { get; private set; } = 0;

        public float CriminalScore
        {
            get
            {
                return 0.0f;
            }
        }

        public void AddLog(ILog log)
        {
            Value += log.Value;
        }
    }
}
