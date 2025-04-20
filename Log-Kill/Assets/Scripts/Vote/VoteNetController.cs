using Cysharp.Threading.Tasks;
using LogKill.Character;
using LogKill.Core;
using LogKill.Event;
using LogKill.LobbySystem;
using LogKill.UI;
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
        private void EndAllClientsVoteResultClientRpc(ulong targetClientId, string resultMessage)
        {
            EventBus.Publish(new PlayerKillEvent()
            {
                VictimId = targetClientId,
                IsBreak = false,
            });

            EventBus.Publish(new VoteEndEvent
            {
                TargetClientId = targetClientId,
                ResultMessage = resultMessage
            });
        }

        [ClientRpc]
        private void GameOverClientRpc(bool isImposterWin)
        {
            if (isImposterWin)
                UIManager.Instance.ShowWindow<GameResultWindow>().ShowResult(true);
            else
                UIManager.Instance.ShowWindow<GameResultWindow>().ShowResult(false);
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
                EndAllClientsVoteResultClientRpc(resultClientId, resultMessage);
                return;
            }

            // TODO resultClientId Kick
            var player = PlayerDataManager.Instance.GetPlayer(resultClientId);
            player.Die();
            PlayerData playerData = player.PlayerData;

            if (playerData.PlayerType == EPlayerType.Imposter)
                resultMessage = $"{playerData.Name}은 임포스터가 맞았습니다.";
            else
                resultMessage = $"{playerData.Name}은 임포스터가 아니였습니다.";

            if (PlayerDataManager.Instance.CheckGameOver(out bool isImposterWin))
                GameOverClientRpc(isImposterWin);
            else
                EndAllClientsVoteResultClientRpc(resultClientId, resultMessage);
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
