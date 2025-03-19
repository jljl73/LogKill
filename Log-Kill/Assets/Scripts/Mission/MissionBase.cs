using LogKill.Mission;
using UnityEngine;

namespace LogKill
{
    public class MissionBase : MonoBehaviour, IMisison
    {
        public void Initialize()
        {
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
        }

        public void CancelMission()
        {
            gameObject.SetActive(false);
            OnCancel();
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
