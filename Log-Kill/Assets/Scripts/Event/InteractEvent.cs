using LogKill.Entity;
using UnityEngine;

namespace LogKill
{
    public enum EInteractType
    {
        Battery,
    }

    public struct InteractEvent
    {
        public EInteractType InteractType;
        public IInteractable InteractableEntity; 
        public bool Enable;
    }
}
