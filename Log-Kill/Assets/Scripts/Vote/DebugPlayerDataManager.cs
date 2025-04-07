using LogKill.Character;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.Vote
{
    public class DebugPlayerDataManager : NetworkSingleton<DebugPlayerDataManager>
    {
        private Dictionary<ulong, PlayerData> _playerDataDict = new();
        public Dictionary<ulong, PlayerData> PlayerDataDict { get; private set; }


        [ServerRpc(RequireOwnership = false)]
        public void SubmitPlayerDataToServerRpc(PlayerData playerData, ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;

            ulong clientId = rpcParams.Receive.SenderClientId;
            SendPlayerDataToClientRpc(clientId, playerData);
        }

        [ClientRpc]
        private void SendPlayerDataToClientRpc(ulong clientId, PlayerData playerData)
        {
            Debug.Log("SendPlayerDataToClientRpc : " + clientId);

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
