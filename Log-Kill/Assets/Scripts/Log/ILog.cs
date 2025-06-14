using UnityEngine;

namespace LogKill.Log
{
    public enum ELogType
    {
        None = 0,
        // Mission
        MissionFail = 1,
        MissionTime = 2,
        MissionSuccess = 3,

        // Player
        PlayerKill = 101,
        IgnoredBody = 102,
        KillWitness = 103,

        // Encounter
        ImposterEncounterTime = 201,
        ImposterEncounter = 202,
        CrewmateEncounter = 203,
        LastEncounter = 204,

        // Map
        MapEnterLog = 301,
        MapStayLog = 302,

        // Item
        ItemAcquire = 401,
        ItemUse = 402,
    }

    public interface ILog
    {
        public ELogType LogType { get; }
        public string Content { get; }
        public int Value { get; }
        public float CriminalScore { get; }
        public void AddLog(ILog log);
    }
}
