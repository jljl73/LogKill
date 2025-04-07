using System;
using UnityEngine;

namespace LogKill.Entity
{
    public class InteractableTrigger : MonoBehaviour
    {
        private IInteractable _interactable;

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
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.TryGetComponent<IInteractable>(out var interactable))
            {
                if (interactable == _interactable)
                    _interactable = null;
                interactable.DisableInteraction();
            }
        }
    }
}
