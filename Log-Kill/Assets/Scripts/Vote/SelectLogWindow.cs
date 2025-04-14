using Cysharp.Threading.Tasks;
using LogKill.Core;
using LogKill.UI;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

namespace LogKill.Vote
{
    public class SelectLogWindow : WindowBase
    {
        [SerializeField] private List<SelectLogItem>  _selectLogItemList = new();
        [SerializeField] private GameObject _disabeldText;
        [SerializeField] private TMP_Text _timerText;

        private CancellationTokenSource _timerToken;

        private int _logCount;
        private int _selectLogIndex;

        private VoteService VoteService => ServiceLocator.Get<VoteService>();

        public override void OnHide()
        {
            _timerToken?.Cancel();
            _timerToken?.Dispose();
            _timerToken = null;
        }

        public void StartSelectLog(List<string> logList, int totalTime = 10)
        {
            StartTimer(totalTime).Forget();

            for (int i = 0; i < _selectLogItemList.Count; i++)
            {
                if (i < logList.Count)
                {
                    _selectLogItemList[i].Initialize(logList[i]);
                }
                else
                {
                    _selectLogItemList[i].Initialize(string.Empty);
                }
            }

            _disabeldText.gameObject.SetActive(logList.Count == 0);

            _logCount = logList.Count;
            _selectLogIndex = -1;
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
                _timerText.text = $"���� �ð� : {currentTime}��";
            }

            SubmitLogMessage();
        }

        private void SubmitLogMessage()
        {
            if (_selectLogIndex == -1)
                _selectLogIndex = Random.Range(0, _logCount);

            string selectLogMessage = _selectLogItemList[_selectLogIndex].GetLogMessage();

            VoteService.ReportSelectLogMessage(selectLogMessage);
        }

        public void OnClickSelectLog(int index)
        {
            if (_selectLogIndex == index) return;

            _selectLogIndex = index;

            for (int i = 0; i < _selectLogItemList.Count; i++)
            {
                _selectLogItemList[i].OnSelect(i == index);
            }
        }
    }
}
