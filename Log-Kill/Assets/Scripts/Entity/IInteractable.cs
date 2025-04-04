using UnityEngine;

namespace LogKill.Entity
{
    public interface IInteractable
    {
        void Interact();
        void EnableInteraction();
        void DisableInteraction();
    }
}
