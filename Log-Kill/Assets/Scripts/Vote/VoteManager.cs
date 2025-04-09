using LogKill.Character;
using LogKill.Core;
using LogKill.Event;
using LogKill.Log;
using LogKill.UI;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.Vote
{
    public class VoteManager : NetworkSingleton<VoteManager>
    {
        private LogService LogService => ServiceLocator.Get<LogService>();
        private EventBus EventBus => ServiceLocator.Get<EventBus>();

        private Dictionary<ulong, string> _selectLogDicts = new();
        private Dictionary<ulong, HashSet<ulong>> _voteResultDicts = new();

        private int _checkCount = 0;
        private int _voteCount = 0;

        [ServerRpc(RequireOwnership = false)]
        public void OnStartVotingServerRpc()
        {
            if (!IsServer) return;

            _checkCount = 0;
            _voteCount = PlayerDataManager.Instance.GetAlivePlayerCount();

            var voteDatas = CreateVoteDataArray();
            OnStartVotingClientRpc(voteDatas);
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

            if (_selectLogDicts.ContainsKey(clientId))
            {
                _selectLogDicts[clientId] = logMessage;
            }
            else
            {
                _selectLogDicts.Add(clientId, logMessage);
            }

            BroadcastSelectLogMessageToAllClientsClientRpc(clientId, logMessage);

            var voteDatas = CreateVoteDataArray();
            ShowVoteWindowClientRpc(voteDatas, new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { clientId }
                }
            });

            _checkCount++;

            if (_checkCount == _voteCount)
            {
                OnEndSelectLogMessageClientRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void VoteCompleteToServerRpc(ulong targetClientId, bool isSkip, ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;

            ulong voterClientId = rpcParams.Receive.SenderClientId;

            if (!isSkip)
            {
                if (!_voteResultDicts.ContainsKey(targetClientId))
                {
                    _voteResultDicts[targetClientId] = new HashSet<ulong>();
                }

                _voteResultDicts[targetClientId].Add(voterClientId);
            }

            BroadcastVoteCompleteToAllClientsClientRpc(voterClientId, targetClientId, isSkip);
        }


        [ClientRpc]
        private void OnStartVotingClientRpc(VoteData[] voteDatas)
        {
            // TODO 회의 시작 애니메이션

            bool isDead = PlayerDataManager.Instance.ClientPlayerData.IsDead;

            if (isDead)
                ShowVoteWindow(voteDatas);
            else
                ShowSelectLogWindow();
        }

        [ClientRpc]
        private void ShowVoteWindowClientRpc(VoteData[] voteDatas, ClientRpcParams rpcParams = default)
        {
            ShowVoteWindow(voteDatas);
        }

        [ClientRpc]
        private void BroadcastSelectLogMessageToAllClientsClientRpc(ulong clientId, string logMessage)
        {
            EventBus.Publish<SelectLogMessageEvent>(new SelectLogMessageEvent
            {
                ClientId = clientId,
                LogMessage = logMessage
            });
        }

        [ClientRpc]
        private void BroadcastVoteCompleteToAllClientsClientRpc(ulong voterClientId, ulong targetClientId, bool isSkip)
        {
            EventBus.Publish<VoteCompleteEvent>(new VoteCompleteEvent
            {
                VoterClientId = voterClientId,
                TargetClientId = targetClientId,
                IsSkip = isSkip
            });
        }

        [ClientRpc]
        private void OnEndVotingClientRpc()
        {
            Debug.Log("OnEndVotingClientRpc");
        }

        [ClientRpc]
        private void OnEndSelectLogMessageClientRpc()
        {
            EventBus.Publish<VoteStartEvent>(new VoteStartEvent());
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
            voteWindow.InitVotePanel(voteDatas);
        }

        private VoteData[] CreateVoteDataArray()
        {
            if (!IsServer) return null;

            var playerDataDict = PlayerDataManager.Instance.PlayerDataDicts;
            var voteDataList = new List<VoteData>();

            foreach (var playerDatakvp in playerDataDict)
            {
                ulong clientId = playerDatakvp.Key;
                PlayerData playerData = playerDatakvp.Value;

                string logMessage = _selectLogDicts.TryGetValue(clientId, out string log) ? log : "";

                voteDataList.Add(new VoteData(playerData, logMessage));
            }

            return voteDataList.ToArray();
        }
    }
}