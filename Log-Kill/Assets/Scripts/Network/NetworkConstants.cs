using UnityEngine;

namespace LogKill.Network
{
    public class NetworkConstants
    {
        public static readonly int LOBBY_INFOMATION_UPDATE_MS = 1100;

        public static readonly int LOBBY_LIST_UPDATE_MS = 2000;

        public static readonly int LOBBY_HEARTBEAT_MS = 25000;

        public static readonly string PLAYERNAME_KEY = "PlayerName";

        public static readonly string IMPOSTER_COUNT_KEY = "ImposterCount";

        public static readonly string GAMESTART_KEY = "GameStart";

        public static readonly string JOINCODE_KEY = "JoinCode";
    }
}
