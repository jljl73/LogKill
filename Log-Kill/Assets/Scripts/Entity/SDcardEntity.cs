using LogKill.Core;
using LogKill.Event;
using LogKill.Item;
using LogKill.Log;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.Entity
{
    public class SDcardEntity : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject _interactUI;

        private EItemType _requiredItemType = EItemType.Battery;
        private int _requiredCount = 5;

        private ItemService ItemService => ServiceLocator.Get<ItemService>();
        private EventBus EventBus => ServiceLocator.Get<EventBus>();

        private void Awake()
        {
            if (_interactUI.activeSelf)
                _interactUI.SetActive(false);
        }

        public void Interact()
        {
            int itemCount = ItemService.GetItemAmount(_requiredItemType);

            if (itemCount < _requiredCount)
                return;

            EventBus.Publish<ItemUsedEvent>(new ItemUsedEvent()
            {
                ItemType = _requiredItemType,
                UsedCount = _requiredCount
            });

            UpdateInteractUI();

            // Show Log UI
            ServiceLocator.Get<ILogNetController>().RequestRandomPlayerLogServerRpc(NetworkManager.Singleton.LocalClientId);
        }

        public void EnableInteraction()
        {
            _interactUI.gameObject.SetActive(true);
            UpdateInteractUI();

            var context = new InteractEvent()
            {
                Enable = true,
                InteractType = EInteractType.SDcard,
                InteractableEntity = this,
            };
            EventBus.Publish(context);
        }

        public void DisableInteraction()
        {
            _interactUI.gameObject.SetActive(false);

            var context = new InteractEvent()
            {
                Enable = false,
                InteractType = EInteractType.SDcard,
                InteractableEntity = this,
            };
            EventBus.Publish(context);
        }


        private void UpdateInteractUI()
        {
            var text = _interactUI.GetComponentInChildren<TMP_Text>();

            int itemCount = ItemService.GetItemAmount(_requiredItemType);

            if (itemCount < _requiredCount)
            {
                text.color = Color.red;
            }
            else
            {
                text.color = Color.white;
            }

            text.text = $"{itemCount} / {_requiredCount}";
        }
    }
}
