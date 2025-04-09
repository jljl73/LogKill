using UnityEngine;

namespace LogKill.Event
{
    public struct SelectLogMessageEvent
    {
        public ulong ClientId;
        public string LogMessage;
    }

    public struct VoteStartEvent
    {

    }

    public struct VoteCompleteEvent
    {
        public ulong VoterClientId;
        public ulong TargetClientId;
        public bool IsSkip;
    }
}
