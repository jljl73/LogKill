using UnityEngine;

namespace LogKill.Event
{
    public struct GameStartEvent
    {
        public int UserCount;
        public int MissionCount;
    }

    public struct SettingImposterEvent
    {
        public ulong ClientId;
    }
}
