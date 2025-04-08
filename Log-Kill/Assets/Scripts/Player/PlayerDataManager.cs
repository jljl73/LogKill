using LogKill.Core;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.Character
{
    public class PlayerDataManager : NetworkSingleton<PlayerDataManager>
    {
        public Dictionary<ulong, PlayerData> PlayerDataDict { get; private set; } = new();

        private EventBus EventBus => ServiceLocator.Get<EventBus>();

        public override void OnNetworkSpawn()
        {
            EventBus.Subscribe<PlayerData>(OnUpdatePlayerData);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SubmitPlayerDataToServerRpc(PlayerData playerData, ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;

            ulong clientId = rpcParams.Receive.SenderClientId;

            if (PlayerDataDict.ContainsKey(clientId))
            {
                PlayerDataDict[clientId] = playerData;
            }
            else
            {
                PlayerDataDict.Add(clientId, playerData);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestPlayerDataServerRpc(ServerRpcParams rpcParams = default)
        {
            PlayerData[] playerDatas = new PlayerData[PlayerDataDict.Count];
            PlayerDataDict.Values.CopyTo(playerDatas, 0);

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

        public int GetAlivePlayerCount()
        {
            return PlayerDataDict.Values.Count(data => !data.IsDead);
        }

        private void OnUpdatePlayerData(PlayerData playerData)
        {
            SubmitPlayerDataToServerRpc(playerData);
        }
    }
}
