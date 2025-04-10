using Cysharp.Threading.Tasks;
using LogKill.Character;
using LogKill.Core;
using LogKill.Event;
using LogKill.UI;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace LogKill.Vote
{
    public class VoteWindow : WindowBase
    {
        [SerializeField] private VotePanel[] _votePanels;
        [SerializeField] private TMP_Text _timerText;

        private EventBus EventBus => ServiceLocator.Get<EventBus>();
        private CancellationTokenSource _timerToken;

        private const int TOTAL_TIME = 90;

        private Dictionary<ulong, VotePanel> _votePanelDict = new();


        public void InitVotePanel(VoteData[] voteDatas)
        {
            _votePanelDict.Clear();

            ulong clientId = NetworkManager.Singleton.LocalClientId;

            SortedVoteDatas(clientId, ref voteDatas);

            int localIndex = Array.FindIndex(voteDatas, v => v.PlayerData.ClientId == clientId);

            VoteData localVote = voteDatas[localIndex];
            bool isImposter = localVote.PlayerData.PlayerType == EPlayerType.Imposter;

            for (int index = 0; index < _votePanels.Length; index++)
            {
                VotePanel votePanel = _votePanels[index];

                if (index < voteDatas.Length)
                {
                    votePanel.Initialize(clientId, voteDatas[index], isImposter);
                    votePanel.gameObject.SetActive(true);

                    _votePanelDict[voteDatas[index].PlayerData.ClientId] = votePanel;
                }
                else
                {
                    votePanel.gameObject.SetActive(false);
                }
            }

            _timerText.text = "Log Selecting...";
        }

        public override void OnShow()
        {
            EventBus.Subscribe<SelectLogMessageEvent>(OnSelectLogMessageEvent);
            EventBus.Subscribe<VoteStartEvent>(OnVoteStartEvent);
            EventBus.Subscribe<VoteCompleteEvent>(OnVoteCompleteEvent);
        }

        public override void OnHide()
        {
            EventBus.Unsubscribe<SelectLogMessageEvent>(OnSelectLogMessageEvent);
            EventBus.Unsubscribe<VoteStartEvent>(OnVoteStartEvent);
            EventBus.Unsubscribe<VoteCompleteEvent>(OnVoteCompleteEvent);

            _timerToken?.Cancel();
            _timerToken?.Dispose();
            _timerToken = null;
        }

        private void OnSelectLogMessageEvent(SelectLogMessageEvent selectLogMessage)
        {
            _votePanelDict[selectLogMessage.ClientId].OnSelectLogMessage(selectLogMessage.LogMessage);
        }

        private void OnVoteStartEvent(VoteStartEvent voteStart)
        {
            StartTimer(TOTAL_TIME).Forget();
        }

        private void OnVoteCompleteEvent(VoteCompleteEvent voteComplete)
        {
            ulong clientId = NetworkManager.Singleton.LocalClientId;

            if (clientId == voteComplete.VoterClientId)
            {
                foreach (var votePanel in _votePanelDict)
                {
                    votePanel.Value.OnDisabled();
                }
            }

            _votePanelDict[voteComplete.VoterClientId].OnVoteComplete();

            if (!voteComplete.IsSkip)
                _votePanelDict[voteComplete.TargetClientId].AddVoteCount();
        }

        private async UniTask StartTimer(int time)
        {
            int currentTime = time;

            _timerToken = new CancellationTokenSource();

            while (currentTime > 0)
            {
                try
                {
                    await UniTask.Delay(1000, cancellationToken: _timerToken.Token);
                }
                catch (System.OperationCanceledException)
                {
                    return;
                }

                currentTime--;
                _timerText.text = $"Prooeeding In : {currentTime}s";
            }
        }

        private void SortedVoteDatas(ulong clientId, ref VoteData[] voteDatas)
        {
            Array.Sort(voteDatas, (a, b) => a.PlayerData.IsDead.CompareTo(b.PlayerData.IsDead));

            int localIndex = Array.FindIndex(voteDatas, v =>
                v.PlayerData.ClientId == clientId &&
                v.PlayerData.IsDead == false);

            if (localIndex > 0)
            {
                (voteDatas[0], voteDatas[localIndex]) = (voteDatas[localIndex], voteDatas[0]);
            }
        }

        public void OnClickVotePanel(int index)
        {
            for (int i = 0; i < _votePanels.Length; i++)
            {
                if (!_votePanels[i].gameObject.activeSelf) continue;

                _votePanels[i].OnSelect(i == index);
            }
        }

        public void OnClickSkip()
        {
            VoteManager.Instance.VoteCompleteToServerRpc(0, true);
        }
    }
}
