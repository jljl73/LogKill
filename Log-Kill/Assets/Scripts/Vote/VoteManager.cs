using LogKill.Character;
using LogKill.UI;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.Vote
{
    public class VoteManager : NetworkSingleton<VoteManager>
    {
        private Dictionary<ulong, string> _clientSelectLogDicts = new();

        [ServerRpc(RequireOwnership = false)]
        public void SubmitLogMessageToServerRpc(string logMessage, ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;

            ulong clientId = rpcParams.Receive.SenderClientId;
            SendLogMessageToClientRpc(clientId, logMessage);
        }

        [ServerRpc(RequireOwnership = false)]
        public void OnStartVotingServerRpc()
        {
            if (!IsServer) return;

            OnStartVotingClientRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void OnEndVotingServerRpc()
        {
            if (!IsServer) return;

            OnEndVotingClientRpc();
        }

        [ClientRpc]
        private void SendLogMessageToClientRpc(ulong clientId, string logMessage)
        {
            if (_clientSelectLogDicts.ContainsKey(clientId))
            {
                _clientSelectLogDicts[clientId] = logMessage;
            }
            else
            {
                _clientSelectLogDicts.Add(clientId, logMessage);
            }

            int votePlayerCount = DebugPlayerDataManager.Instance.GetVotePlayerCount();

            if (_clientSelectLogDicts.Count == votePlayerCount)
            {
                var voteWindow = UIManager.Instance.ShowWindow<VoteWindow>();
                voteWindow.InitVotePanel(_clientSelectLogDicts);
            }
        }

        [ClientRpc]
        private void OnStartVotingClientRpc()
        {
            _clientSelectLogDicts.Clear();

            ShowSelectLogWindow();
        }

        [ClientRpc]
        private void OnEndVotingClientRpc()
        {
            Debug.Log("OnEndVotingClientRpc");
        }

        private void ShowSelectLogWindow()
        {
            var selectLogWindow = UIManager.Instance.ShowWindow<SelectLogWindow>();

            // TODO Get Random LogList
            selectLogWindow.InitSelectLogList("LogText1", "LogText2", "LogText3");
        }
    }
}