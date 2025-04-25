using System;
using LogKill.Character;
using LogKill.Core;
using LogKill.Event;
using LogKill.Item;
using UnityEngine;

namespace LogKill.Entity
{
    public class InteractableTrigger : MonoBehaviour
    {
        private IInteractable _interactable;
        private EventBus EventBus => ServiceLocator.Get<EventBus>();
        private Player _player;

        public void Initalize(Player player)
        {
            _player = player;
        }

        public void Interact()
        {
            _interactable?.Interact();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent<IInteractable>(out var interactable))
            {
                _interactable = interactable;
                interactable.EnableInteraction();
            }

            if (collision.TryGetComponent<IItem>(out var item))
            {
                item.PickUp();
            }

            if (collision.TryGetComponent<Player>(out var player))
            {
                if (player != _player)
                    EventBus.Publish(new PlayerRangeChagnedEvent(player, true));
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.TryGetComponent<IInteractable>(out var interactable))
            {
                if (interactable == _interactable)
                {
                    _interactable = null;
                    interactable.DisableInteraction();
                }
            }

            if (collision.TryGetComponent<Player>(out var player))
            {
                if (player != _player)
                    EventBus.Publish(new PlayerRangeChagnedEvent(player, false));
            }
        }
    }
}
