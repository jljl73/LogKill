using Cysharp.Threading.Tasks;
using LogKill.Character;
using LogKill.Core;
using LogKill.Event;
using LogKill.LobbySystem;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.Room
{
    public class SessionManager : NetworkSingleton<SessionManager>
    {
        [SerializeField]
        private NetworkObject _playerPrefab;

        private EventBus EventBus => ServiceLocator.Get<EventBus>();
        private LobbyManager LobbyManager => ServiceLocator.Get<LobbyManager>();

        private Dictionary<ulong, bool> _playerLoadStatus = new Dictionary<ulong, bool>(); // ���� �ε� ����
        private Dictionary<ulong, Player> _playerDicts = new Dictionary<ulong, Player>();

        public Dictionary<ulong, Player> PlayerDicts { get => _playerDicts; }


        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnected;
            }

            if (IsClient)
            {
                GameManager.Instance.OnMoveLobbyScene();
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerDisconnected;
            }

            if (IsClient)
            {
                GameManager.Instance.OnMoveTitleScene();

                LobbyManager.LeaveLobbyAsync().Forget();
            }
        }

        #region Server

        private void OnPlayerConnected(ulong clientId)
        {
            if (!IsServer)
                return;

            _playerLoadStatus[clientId] = false;

            OnPlayerSpawn(clientId);

            Debug.Log("Before BroadcastLobbyChangedClientRpc");
            BroadcastLobbyChangedClientRpc(clientId);
        }

        private void OnPlayerDisconnected(ulong clientId)
        {
            if (!IsServer)
                return;

            if (_playerLoadStatus.ContainsKey(clientId))
            {
                _playerLoadStatus.Remove(clientId);
            }

            OnPlayerDespawn(clientId);
            BroadcastLobbyChangedClientRpc(clientId);
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

        [ServerRpc(RequireOwnership = false)]
        public void NotifyGameStartServerRpc()
        {
            if (!IsServer) return;

            BroadcastGameStartClientRpc();
        }

        private void CheckAllPlayersLoaded()
        {
            foreach (var status in _playerLoadStatus.Values)
            {
                if (!status)
                    return;
            }

            // TODO 임포스터 할당
            // LobbyManager.Instance.GetImposterCount();

            // NetworkManager.Singleton.ConnectedClients

            StartNextPhaseClientRpc();
        }

        #endregion


        [ClientRpc]
        private void BroadcastGameStartClientRpc()
        {
            GameManager.Instance.StartSession().Forget();
        }

        [ClientRpc]
        private void StartNextPhaseClientRpc()
        {
            var context = new GameStartEvent()
            {
                UserCount = NetworkManager.Singleton.ConnectedClientsIds.Count,
                MissionCount = 5,
            };
            EventBus.Publish(context);
        }

        [ClientRpc]
        public void BroadcastLobbyChangedClientRpc(ulong clientId)
        {
            var context = new LobbyChangedEvent()
            {
                ClientId = clientId,
                CurrentPlayers = NetworkManager.Singleton.ConnectedClients.Count,
                MaxPlayers = LobbyManager.GetMaxPlayers(),
                IsPrivate = LobbyManager.GetIsPrivate()
            };

            EventBus.Publish<LobbyChangedEvent>(context);
        }

        private void OnPlayerSpawn(ulong clientId)
        {
            var playerInstance = Instantiate(_playerPrefab);
            playerInstance.SpawnAsPlayerObject(clientId);

            var player = playerInstance.GetComponent<Player>();

            if (_playerDicts.ContainsKey(clientId))
            {
                _playerDicts[clientId] = player;
            }
            else
            {
                _playerDicts.Add(clientId, player);
            }

            PlayerDataManager.Instance.SubmitPlayerDataToServerRpc(new PlayerData(clientId));
        }

        private void OnPlayerDespawn(ulong clientId)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
            {
                if (client.PlayerObject != null && client.PlayerObject.IsSpawned)
                {
                    client.PlayerObject.Despawn();

                    if (_playerDicts.ContainsKey(clientId))
                    {
                        _playerLoadStatus.Remove(clientId);
                    }
                }
            }
        }
    }
}
