using LogKill.Character;

namespace LogKill.Event
{
    public struct PlayerRangeChagnedEvent
    {
        public Player TargetPlayer { get; private set; }
        public bool IsNearby { get; private set; }

        public PlayerRangeChagnedEvent(Player targetPlayer, bool isNearby)
        {
            TargetPlayer = targetPlayer;
            IsNearby = isNearby;
        }
    }
}
