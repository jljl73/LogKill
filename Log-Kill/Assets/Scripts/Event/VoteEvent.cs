using LogKill.Character;
using UnityEngine;

namespace LogKill.Event
{
    public struct VoteStartEvent
    {
        public ulong ReportClientId;
    }
    public struct VoteEndEvent
    {
        public string ResultMessage;
        public ulong TargetClientId;
    }

    public struct VoteCompleteEvent
    {
        public ulong TargetClientId;
    }

    public struct UpdateVoteResultEvent
    {
        public ulong TargetClientId;
        public int VoteCount;
    }
}
