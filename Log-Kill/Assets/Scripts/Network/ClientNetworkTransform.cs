using Unity.Netcode.Components;
using UnityEngine;

namespace LogKill.Network
{
    [DisallowMultipleComponent]
    public class ClientNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}
