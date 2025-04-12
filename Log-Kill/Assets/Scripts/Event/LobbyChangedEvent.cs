using UnityEngine;

namespace LogKill.Event
{
    public struct LobbyChangedEvent
    {
        public ulong ClientId;
        public int CurrentPlayers;
        public int MaxPlayers;
        public bool IsPrivate;
    }
}
