using LogKill.Character;
using LogKill.Core;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.Vote
{
    public class DebugPlayerDataManager : NetworkSingleton<DebugPlayerDataManager>
    {
        [SerializeField]
        private NetworkObject _playerPrefab;


        private Dictionary<ulong, PlayerData> _playerDataDict = new();
        public Dictionary<ulong, PlayerData> PlayerDataDict { get; private set; }

        private EventBus EventBus => ServiceLocator.Get<EventBus>();

        public override void OnNetworkSpawn()
        {
            OnSpawnPlayerServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void OnSpawnPlayerServerRpc(ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;

            ulong clientId = rpcParams.Receive.SenderClientId;
            NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(_playerPrefab, clientId);
        }


        [ServerRpc(RequireOwnership = false)]
        public void SubmitPlayerDataToServerRpc(PlayerData playerData, ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;

            ulong clientId = rpcParams.Receive.SenderClientId;

            if (_playerDataDict.ContainsKey(clientId))
            {
                _playerDataDict[clientId] = playerData;
            }
            else
            {
                _playerDataDict.Add(clientId, playerData);
            }

            Debug.Log("Count : " + _playerDataDict.Count);
        }

        [ClientRpc]
        public void BroadcastPlayerDataToAllClientsClientRpc()
        {
            Debug.Log("BroadcastPlayerDataToAllClientsClientRpc");
        }


        public void PrintPlayerData()
        {
            if (!IsServer) return;

            foreach (var item in _playerDataDict)
            {
                Debug.Log($"Name : {item.Value.Name} | IsDead : {item.Value.IsDead}");
            }
        }


        //[ClientRpc]
        //private void SendPlayerDataToClientRpc(ulong clientId, PlayerData playerData)
        //{
        //    Debug.Log("SendPlayerDataToClientRpc : " + clientId);

        //    if (_playerDataDict.ContainsKey(clientId))
        //    {
        //        _playerDataDict[clientId] = playerData;
        //    }
        //    else
        //    {
        //        _playerDataDict.Add(clientId, playerData);
        //    }
        //}

        public int GetVotePlayerCount()
        {
            int count = 0;

            foreach (var player in _playerDataDict.Values)
            {
                if (!player.IsDead)
                    count++;
            }

            return count;
        }
    }
}
