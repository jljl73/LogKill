using Unity.Netcode.Components;
using UnityEngine;

namespace LogKill.Network
{
    [DisallowMultipleComponent]
    public class ClientNetworkAnimator : NetworkAnimator
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }

}
