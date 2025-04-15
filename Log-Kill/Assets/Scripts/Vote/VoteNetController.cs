using LogKill.Character;
using LogKill.Core;
using LogKill.Event;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.Vote
{
    public interface IVoteNetController
    {
        void NotifyVoteStartServerRpc(ulong reportClientId);

        void SubmitSelectLogMessageServerRpc(ulong clientId, PlayerData playerData, string selctLogMessage);

        void SubmitVoteServerRpc(ulong clientId, ulong targetClientId);
    }

    public class VoteNetController : NetworkBehaviour, IVoteNetController
    {
        private Dictionary<ulong, VoteData> _clientVoteDataDicts = new();
        private Dictionary<ulong, HashSet<ulong>> _clientVoteResultDicts = new();

        private ulong _reportClientId;

        private EventBus EventBus => ServiceLocator.Get<EventBus>();
        private VoteService VoteService => ServiceLocator.Get<VoteService>();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            ServiceLocator.Register<IVoteNetController, VoteNetController>(this);
        }

        [ServerRpc(RequireOwnership = false)]
        public void NotifyVoteStartServerRpc(ulong reportClientId)
        {
            if (!IsServer) return;

            _clientVoteDataDicts.Clear();
            _clientVoteResultDicts.Clear();

            _reportClientId = reportClientId;

            BroadcastVoteStartToAllClientsClientRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void SubmitSelectLogMessageServerRpc(ulong clientId, PlayerData playerData, string selctLogMessage)
        {
            if (!IsServer) return;

            VoteData voteData = new VoteData()
            {
                PlayerData = playerData,
                LogMessage = selctLogMessage
            };

            if (!_clientVoteDataDicts.ContainsKey(clientId))
            {
                _clientVoteDataDicts.Add(clientId, voteData);
            }

            CheckAllClientsSelectLog();
        }

        [ServerRpc(RequireOwnership = false)]
        public void SubmitVoteServerRpc(ulong clientId, ulong targetClientId)
        {
            if (!IsServer) return;

            if (!_clientVoteResultDicts.ContainsKey(targetClientId))
                _clientVoteResultDicts[targetClientId] = new HashSet<ulong>();

            if (_clientVoteResultDicts[targetClientId].Add(clientId))
            {
                int voteCount = _clientVoteResultDicts[targetClientId].Count;

                BroadcastVoteToAllClientsClientRpc(targetClientId, voteCount);
                CheckAllClientsVote();
            }
        }

        [ClientRpc]
        private void BroadcastVoteStartToAllClientsClientRpc()
        {
            VoteService.ShowSelectLogWindow();
        }

        [ClientRpc]
        private void EndAllClientsSelectLogClientRpc(VoteData[] voteDatas)
        {
            VoteService.ShowVoteWindow(voteDatas);
        }

        [ClientRpc]
        private void BroadcastVoteToAllClientsClientRpc(ulong targetClientId, int voteCount)
        {
            EventBus.Publish<UpdateVoteResultEvent>(new UpdateVoteResultEvent
            {
                TargetClientId = targetClientId,
                VoteCount = voteCount
            });
        }

        [ClientRpc]
        private void EndAllClientsVoteResultClientRpc(string resultMessage)
        {
            EventBus.Publish<VoteEndEvent>(new VoteEndEvent
            { 
                ResultMessage = resultMessage
            });
        }


        private void CheckAllClientsSelectLog()
        {
            if (_clientVoteDataDicts.Count < NetworkManager.Singleton.ConnectedClientsIds.Count) return;

            VoteData[] voteDatas = _clientVoteDataDicts.Values.ToArray();
            EndAllClientsSelectLogClientRpc(voteDatas);
        }

        private void CheckAllClientsVote()
        {
            int totalCount = _clientVoteResultDicts.Values.Sum(hashSet => hashSet.Count);

            if (totalCount < PlayerDataManager.Instance.GetAlivePlayerCount()) return;

            ulong resultClientId = GetVoteResultClientId();

            CheckVoteResultToSubmitMessage(resultClientId);
        }

        private void CheckVoteResultToSubmitMessage(ulong resultClientId)
        {
            string resultMessage;

            if (resultClientId == VoteService.SKIP_VOTE_ID)
            {
                resultMessage = "투표 무효";
            }
            else
            {
                // TODO resultClientId Kick

                PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(resultClientId).Value;

                if (playerData.PlayerType == EPlayerType.Imposter)
                    resultMessage = $"{playerData.Name}은 임포스터가 맞았습니다.";
                else
                    resultMessage = $"{playerData.Name}은 임포스터가 아니였습니다.";
            }

            EndAllClientsVoteResultClientRpc(resultMessage);
        }

        private ulong GetVoteResultClientId()
        {
            int maxVoteCount = 0;
            ulong resultClientId = VoteService.SKIP_VOTE_ID;

            foreach (var voteResult in _clientVoteResultDicts)
            {
                ulong targetClientId = voteResult.Key;
                int voteCount = voteResult.Value.Count;

                if (voteCount > maxVoteCount)
                {
                    maxVoteCount = voteCount;
                    resultClientId = targetClientId;
                }
                else if (voteCount == maxVoteCount)
                {
                    resultClientId = VoteService.SKIP_VOTE_ID;
                }
            }

            return resultClientId;
        }
    }
}
