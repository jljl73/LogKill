using LogKill.Character;
using LogKill.Core;
using LogKill.Log;
using LogKill.UI;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.Vote
{
    public class VoteManager : NetworkSingleton<VoteManager>
    {
        private Dictionary<ulong, string> _clientSelectLogDicts = new();

        private LogService LogService => ServiceLocator.Get<LogService>();

        private int _checkCount = 0;
        private int _voteCount = 0;

        [ServerRpc(RequireOwnership = false)]
        public void OnStartVotingServerRpc()
        {
            if (!IsServer) return;

            _checkCount = 0;
            _voteCount = PlayerDataManager.Instance.GetAlivePlayerCount();

            OnStartVotingClientRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void OnEndVotingServerRpc()
        {
            if (!IsServer) return;

            OnEndVotingClientRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void SubmitLogMessageToServerRpc(string logMessage, ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;

            ulong clientId = rpcParams.Receive.SenderClientId;

            if (_clientSelectLogDicts.ContainsKey(clientId))
            {
                _clientSelectLogDicts[clientId] = logMessage;
            }
            else
            {
                _clientSelectLogDicts.Add(clientId, logMessage);
            }

            _checkCount++;

            CheckAllPlayerSelectLogMessage();
        }

        [ServerRpc(RequireOwnership = false)]
        public void CreateVoteDataServerRpc()
        {
            if (!IsServer) return;

            List<VoteData> voteDataList = new();

            var playerDataDict = PlayerDataManager.Instance.PlayerDataDict;

            foreach (var selectLog in _clientSelectLogDicts)
            {
                ulong clientId = selectLog.Key;
                string logMessage = selectLog.Value;

                if (playerDataDict.TryGetValue(clientId, out PlayerData playerData))
                {
                    voteDataList.Add(new VoteData(playerData, logMessage));
                }
            }

            BroadcastVoteDataToAllClientsClientRpc(voteDataList.ToArray());
        }

        [ClientRpc]
        public void BroadcastVoteDataToAllClientsClientRpc(VoteData[] voteDatas)
        {

            foreach (var voteData in voteDatas)
            {
                Debug.Log($"{voteData.PlayerData.Name} - {voteData.LogMessage}");
            }
        }

        [ClientRpc]
        private void OnStartVotingClientRpc()
        {
            ShowSelectLogWindow();
        }

        [ClientRpc]
        private void OnEndVotingClientRpc()
        {
            Debug.Log("OnEndVotingClientRpc");
        }

        private void ShowSelectLogWindow()
        {
            var logList = LogService.GetRandomLogList();

            var debugLogList = new List<string>() { "TestLog 1", "TestLog 2", "Test Log3" };

            var selectLogWindow = UIManager.Instance.ShowWindow<SelectLogWindow>();
            selectLogWindow.InitLogList(debugLogList);
        }

        private void ShowVoteWindow(VoteData[] voteDatas)
        {
            var voteWindow = UIManager.Instance.ShowWindow<VoteWindow>();
        }


        private void CheckAllPlayerSelectLogMessage()
        {
            if (_checkCount == _voteCount)
            {
                CreateVoteDataServerRpc();
            }
        }
    }
}