using Unity.Netcode.Components;
using UnityEngine;

namespace LogKill
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
