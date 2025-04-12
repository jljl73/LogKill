using LogKill.Core;
using LogKill.Event;
using Unity.Netcode;

namespace LogKill.Mission
{
    public class MissionService : IService
    {
        private int _userCount;
        private int _missionCount;
        public int UserCount => _userCount;
        public int MissionCount => _missionCount;

        private EventBus EventBus => ServiceLocator.Get<EventBus>();
        private IMissionNetController MissionNetController => ServiceLocator.Get<IMissionNetController>();

        public void Initialize()
        {
            EventBus.Subscribe<GameStartEvent>(OnGameStart);
        }

        public void SendProgress(int progress, int allProgress)
        {
            EventBus.Publish(new MissionProgressEvent
            {
                Progress = progress,
                AllProgress = allProgress
            });
        }

        public void OnGameStart(GameStartEvent context)
        {
            _userCount = context.UserCount;
            _missionCount = context.MissionCount;
            MissionNetController.OnGameStart();
        }

        public void ReportMissionClear(int missionId)
        {
            MissionNetController.ReportMissionClearServerRpc(NetworkManager.Singleton.LocalClientId, missionId);
        }
    }
}
