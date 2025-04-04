using Cysharp.Threading.Tasks;
using LogKill.Core;
using LogKill.Entity;
using LogKill.UI;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.UI
{
    public class InGameHud : HUDBase
    {
        [SerializeField] private Button _interactButton;

        private EventBus EventBus => ServiceLocator.Get<EventBus>();
        private IInteractable _interactableEntity;

        public override async UniTask InitializeAsync()
        {
            _interactButton?.onClick.AddListener(OnClickInteract);
            EventBus.Subscribe<InteractEvent>(OnInteractEvent);
            _interactButton.interactable = _interactableEntity != null;
            
            await UniTask.Yield();
        }

        private void OnInteractEvent(InteractEvent context)
        {
            if (context.Enable)
            {
                _interactableEntity = context.InteractableEntity;
                _interactButton.interactable = true;
            }
            else
            {
                if (context.InteractableEntity == _interactableEntity)
                    _interactableEntity = null;
                _interactButton.interactable = false;
            }
        }

        public void OnClickInteract()
        {
            _interactableEntity?.Interact();
        }
    }
}
