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
        public void SubmitPlayerDataToServerRpc(PlayerData playerData, ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;

            ulong clientId = rpcParams.Receive.SenderClientId;

            if (PlayerDataDicts.ContainsKey(clientId))
            {
                PlayerDataDicts[clientId] = playerData;
            }
            else
            {
                PlayerDataDicts.Add(clientId, playerData);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestPlayerDataServerRpc(ServerRpcParams rpcParams = default)
        {
            PlayerData[] playerDatas = new PlayerData[PlayerDataDicts.Count];
            PlayerDataDicts.Values.CopyTo(playerDatas, 0);

            BroadcastPlayerDataClientRpc(playerDatas);
        }

        [ClientRpc]
        public void BroadcastPlayerDataClientRpc(PlayerData[] playerDatas)
        {
            foreach (var item in playerDatas)
            {
                if (item.ClientId == NetworkManager.Singleton.LocalClientId)
                {
                    Debug.Log($"Name : {item.Name} | IsDead : {item.IsDead} | ColorType : {item.ColorType}");
                }
            }
        }

        public PlayerData[] GetPlayerDataToArray()
        {
            PlayerData[] playerDatas = new PlayerData[PlayerDataDicts.Count];
            PlayerDataDicts.Values.CopyTo(playerDatas, 0);

            return playerDatas;
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
