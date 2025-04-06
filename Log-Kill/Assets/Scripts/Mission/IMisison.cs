using UnityEngine;

namespace LogKill.Mission
{
    public interface IMisison
    {
        public void Initialize(int missionId);
        public void StartMission();
        public void ClearMission();
        public void CancelMission();
    }
}
