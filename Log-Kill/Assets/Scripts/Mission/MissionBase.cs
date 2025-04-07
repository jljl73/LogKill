using LogKill.Core;
using LogKill.Event;
using LogKill.Mission;
using LogKill.UI;
using Unity.Netcode;
using UnityEngine;

namespace LogKill
{
    public class MissionBase : MonoBehaviour, IMisison
    {
        private int _missionId;
        protected MissionService MissionService => ServiceLocator.Get<MissionService>();
        protected EventBus EventBus => ServiceLocator.Get<EventBus>();

        public void Initialize(int missionId)
        {
            _missionId = missionId;
            gameObject.SetActive(true);
            OnInitialize();
        }

        public void StartMission()
        {
            OnStart();
        }

        public void ClearMission()
        {
            OnClear();
            UIManager.Instance.CloseCurrentWindow();
            MissionService.ReportMissionClearServerRpc(NetworkManager.Singleton.LocalClientId, _missionId);
            EventBus.Publish(new MissionClearEvent { MissionId = _missionId, });
        }

        public void CancelMission()
        {
            OnCancel();
            UIManager.Instance.CloseCurrentWindow();
        }

        protected virtual void OnInitialize()
        {
        }

        protected virtual void OnStart()
        {
        }

        protected virtual void OnCancel()
        {
        }

        protected virtual void OnClear()
        {
        }
    }
}
