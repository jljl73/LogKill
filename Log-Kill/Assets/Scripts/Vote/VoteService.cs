using LogKill.Character;
using LogKill.Core;
using LogKill.Event;
using LogKill.Log;
using LogKill.UI;
using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.Vote
{
    public class VoteService : IService
    {
        public static readonly ulong SKIP_VOTE_ID = ulong.MaxValue;

        public static readonly int SELECT_LOG_TOTAL_TIME = 10;
        public static readonly int VOTE_TOTAL_TIME = 90;

        private EventBus EventBus => ServiceLocator.Get<EventBus>();
        private LogService LogService => ServiceLocator.Get<LogService>();
        private IVoteNetController VoteNetController => ServiceLocator.Get<IVoteNetController>();

        public void Initialize()
        {
            EventBus.Subscribe<VoteStartEvent>(OnVoteStart);
            EventBus.Subscribe<VoteEndEvent>(OnVoteEnd);
        }

        public void OnVoteStart(VoteStartEvent context)
        {
            VoteNetController.NotifyVoteStartServerRpc(context.ReportClientId);
        }

        public void OnVoteEnd(VoteEndEvent context)
        {
            UIManager.Instance.CloseAllWindows();
            Debug.Log(context.ResultMessage);
        }

        public void ShowSelectLogWindow()
        {
            var selectLogWindow = UIManager.Instance.ShowWindow<SelectLogWindow>();

            bool isDead = PlayerDataManager.Instance.Me.IsDead;
            if (isDead)
            {
                selectLogWindow.WaitSelectLog();
            }
            else
            {
                var logList = LogService.GetCreminalScoreWeightedRandomLogList();
                selectLogWindow.StartSelectLog(logList);
            }
        }

        public void ShowVoteWindow(VoteData[] voteDatas)
        {
            var voteWindow = UIManager.Instance.ShowWindow<VoteWindow>();

            VoteData[] sortedVoteDatas = GetSortedVoteDatas(NetworkManager.Singleton.LocalClientId, voteDatas);

            voteWindow.StartVoting(sortedVoteDatas);
        }

        public void ReportSelectLogMessage(string selectLogMessage)
        {
            VoteNetController.SubmitSelectLogMessageServerRpc(
                NetworkManager.Singleton.LocalClientId, 
                PlayerDataManager.Instance.Me.PlayerData, 
                selectLogMessage);
        }

        public void ReportVoteComplete(ulong targetClientId)
        {
            VoteNetController.SubmitVoteServerRpc(
                NetworkManager.Singleton.LocalClientId, 
                targetClientId);
        }

        private VoteData[] GetSortedVoteDatas(ulong localClientId, VoteData[] voteDatas)
        {
            return voteDatas
                .OrderBy(v => v.PlayerData.IsDead)
                .ThenByDescending(v => v.PlayerData.ClientId == localClientId && !v.PlayerData.IsDead)
                .ToArray();
        }
    }
}
