using LogKill.Character;
using LogKill.Entity;
using UnityEngine;

namespace LogKill
{
    public enum EInteractType
    {
        Outlet, SDcard
    }

    public struct InteractEvent
    {
        public EInteractType InteractType;
        public IInteractable InteractableEntity; 
        public bool Enable;
    }

    public struct PlayerKillEvent
    {
        public ulong VictimId;
        public bool IsBreak;
    }
}
