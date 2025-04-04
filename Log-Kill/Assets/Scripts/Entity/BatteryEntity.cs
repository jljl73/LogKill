using LogKill.Core;
using LogKill.Mission;
using LogKill.UI;
using UnityEngine;

namespace LogKill.Entity
{
    public class BatteryEntity : MonoBehaviour, IInteractable
    {
        [SerializeField] private MissionData _missionData;
        private EventBus EventBus => ServiceLocator.Get<EventBus>();

        public void Interact()
        {
            var window = UIManager.Instance.ShowWindow<MissionWindow>();
            window.StartMission(_missionData);
        }

        public void EnableInteraction()
        {
            var context = new InteractEvent()
            {
                Enable = true,
                InteractType = EInteractType.Battery,
                InteractableEntity = this,
            };
            EventBus.Publish(context);
        }

        public void DisableInteraction()
        {
            var context = new InteractEvent()
            {
                Enable = false,
                InteractType = EInteractType.Battery,
                InteractableEntity = this,
            };
            EventBus.Publish(context);
        }
    }
}
