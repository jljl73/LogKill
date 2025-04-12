using LogKill.Core;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.Character
{
    public class PlayerDataManager : NetworkSingleton<PlayerDataManager>
    {
        public Dictionary<ulong, PlayerData> PlayerDataDicts { get; private set; } = new();

        public PlayerData ClientPlayerData { get; private set; }

        private EventBus EventBus => ServiceLocator.Get<EventBus>();

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerDisconnected;
            }

            EventBus.Subscribe<PlayerData>(OnUpdatePlayerData);
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerDisconnected;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SubmitPlayerDataToServerRpc(PlayerData playerData)
        {
            if (!IsServer) return;

            ulong clientId = playerData.ClientId;

            if (PlayerDataDicts.ContainsKey(clientId))
            {
                PlayerDataDicts[clientId] = playerData;
            }
            else
            {
                PlayerDataDicts.Add(clientId, playerData);
            }

            Debug.Log("SubmitPlayerDataToServerRpc : " + PlayerDataDicts.Count);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestPlayerKillServerRpc(ulong targetClientId, ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;

            // TODO : Check if the killer is impostor or not

            if (PlayerDataDicts.TryGetValue(targetClientId, out PlayerData playerData))
            {
                playerData.IsDead = true;
                PlayerDataDicts[targetClientId] = playerData;
                BroadcastPlayerKillClientRpc(targetClientId);
            }
        }

        [ClientRpc]
        private void BroadcastPlayerKillClientRpc(ulong targetClientId)
        {
            if (PlayerDataDicts.TryGetValue(targetClientId, out PlayerData playerData))
            {
                playerData.IsDead = true;
                PlayerDataDicts[targetClientId] = playerData;
            }
        }

        public PlayerData[] GetPlayerDataToArray()
        {
            PlayerData[] playerDatas = new PlayerData[PlayerDataDicts.Count];
            PlayerDataDicts.Values.CopyTo(playerDatas, 0);

            return playerDatas;
        }

        public PlayerData? GetPlayerData(ulong clietId)
        {
            if (PlayerDataDicts.TryGetValue(clietId, out PlayerData playerData))
            {
                Debug.Log("Name : " + playerData.Name);
                return playerData;
            }

            Debug.Log("PlayerData Is Null");
            return null;
        }

        public int GetAlivePlayerCount()
        {
            return PlayerDataDicts.Values.Count(data => !data.IsDead);
        }

        private void OnUpdatePlayerData(PlayerData playerData)
        {
            ClientPlayerData = playerData;
            SubmitPlayerDataToServerRpc(playerData);
        }

        private void OnPlayerDisconnected(ulong clientId)
        {
            if (!IsServer)
                return;

            if (PlayerDataDicts.ContainsKey(clientId))
            {
                PlayerDataDicts.Remove(clientId);
            }
        }
    }
}
