using Cysharp.Threading.Tasks;
using LogKill.UI;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.Room
{
    public class SessionManager : NetworkSingleton<SessionManager>
    {
        private Dictionary<ulong, bool> _playerLoadStatus = new Dictionary<ulong, bool>(); // ���� �ε� ����

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnected;
            }

            if(IsClient)
            {
                GameManager.Instance.StartSession().Forget();
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerDisconnected;
            }
        }

        #region Server

        private void OnPlayerConnected(ulong clientId)
        {
            if (!IsServer)
                return;

            _playerLoadStatus[clientId] = false;
        }

        private void OnPlayerDisconnected(ulong clientId)
        {
            if (!IsServer)
                return;

            if (_playerLoadStatus.ContainsKey(clientId))
            {
                _playerLoadStatus.Remove(clientId);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void NotifyPlayerLoadedServerRpc(ulong clientId)
        {
            if (!IsServer) return;

            if (_playerLoadStatus.ContainsKey(clientId))
            {
                _playerLoadStatus[clientId] = true;
                CheckAllPlayersLoaded();
            }
        }

        private void CheckAllPlayersLoaded()
        {
            foreach (var status in _playerLoadStatus.Values)
            {
                if (!status) 
                    return;
            }

            StartNextPhaseClientRpc();
        }

        #endregion

        [ClientRpc]
        private void StartNextPhaseClientRpc()
        {
            // Debug.Log("!!");
            UIManager.Instance.ShowHUD<InGameHud>();
        }
    }
}
