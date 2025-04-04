using LogKill.Mission;
using LogKill.UI;
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
            // gameObject.SetActive(false);
            UIManager.Instance.CloseCurrentWindow();
        }

        public void CancelMission()
        {
            OnCancel();
            // gameObject.SetActive(false);
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
