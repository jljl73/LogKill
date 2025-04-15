using Cysharp.Threading.Tasks;
using LogKill.Character;
using LogKill.Core;
using LogKill.Event;
using LogKill.UI;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.Vote
{
    public class VoteWindow : WindowBase
    {
        [SerializeField] private VotePanel[] _votePanels;
        [SerializeField] private Button _skipButton;
        [SerializeField] private TMP_Text _timerText;

        private CancellationTokenSource _timerToken;
        private Dictionary<ulong, VotePanel> _votePanelDict = new();

        private EventBus EventBus => ServiceLocator.Get<EventBus>();
        private VoteService VoteService => ServiceLocator.Get<VoteService>();

        public override void OnShow()
        {
            StartTimer(VoteService.VOTE_TOTAL_TIME).Forget();

            EventBus.Subscribe<VoteCompleteEvent>(OnVoteCompleteEvent);
            EventBus.Subscribe<UpdateVoteResultEvent>(OnUpdateVoteResultEvent);
        }

        public override void OnHide()
        {
            EventBus.Unsubscribe<VoteCompleteEvent>(OnVoteCompleteEvent);
            EventBus.Unsubscribe<UpdateVoteResultEvent>(OnUpdateVoteResultEvent);

            _timerToken?.Cancel();
            _timerToken?.Dispose();
            _timerToken = null;
        }

        public void StartVoting(VoteData[] voteDatas)
        {
            PlayerData localPlayerData = PlayerDataManager.Instance.Me.PlayerData;

            for (int i = 0; i < _votePanels.Length; i++)
            {
                VotePanel votePanel = _votePanels[i];

                if (i < voteDatas.Length)
                {
                    votePanel.InitVotePanel(localPlayerData, voteDatas[i]);
                    votePanel.gameObject.SetActive(true);

                    ulong clientId = voteDatas[i].PlayerData.ClientId;
                    if (!_votePanelDict.ContainsKey(clientId))
                        _votePanelDict.Add(clientId, votePanel);
                }
                else
                {
                    votePanel.gameObject.SetActive(false);
                }
            }

            _skipButton.interactable = !localPlayerData.IsDead;
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
                _timerText.text = $"남은 시간 : {currentTime}초";
            }

            OnClickSkip();
        }

        private void OnVoteCompleteEvent(VoteCompleteEvent context)
        {
            foreach (var votePanel in _votePanels)
            {
                if (!votePanel.gameObject.activeSelf) continue;

                votePanel.OnDisabledPanel();
            }

            _skipButton.interactable = false;
            VoteService.ReportVoteComplete(context.TargetClientId);
        }

        private void OnUpdateVoteResultEvent(UpdateVoteResultEvent context)
        {
            if (_votePanelDict.TryGetValue(context.TargetClientId, out VotePanel votePanel))
            {
                votePanel.OnUpdateVoteResult(context.VoteCount);
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
            EventBus.Publish<VoteCompleteEvent>(new VoteCompleteEvent
            {
                TargetClientId = VoteService.SKIP_VOTE_ID
            });
        }
    }
}
