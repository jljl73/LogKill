using UnityEngine;

namespace LogKill.Event
{
    public struct MissionProgressEvent
    {
        public int Progress;
        public int AllProgress;
    }

    public struct MissionClearEvent
    {
        public int MissionId;
    }
}
