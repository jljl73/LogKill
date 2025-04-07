using Cysharp.Threading.Tasks;
using LogKill.Log;
using LogKill.UI;
using System.Threading;
using TMPro;
using UnityEngine;

namespace LogKill.Vote
{
    public class SelectLogWindow : WindowBase
    {
        [SerializeField] private SelectLogItem[] _selectLogItems;

        [SerializeField] private TMP_Text _timerText;

        [SerializeField] private int _totalTime = 10;

        private CancellationTokenSource _timerToken;

        private int _selectLogIndex;
        private int _logCount;

        public override void OnShow()
        {
            StartTimer(_totalTime).Forget();
        }

        public override void OnHide()
        {
            _timerToken?.Cancel();
            _timerToken?.Dispose();
            _timerToken = null;
        }

        public void InitSelectLogList(ILog[] logs)
        {
            _logCount = Mathf.Min(logs.Length, _selectLogItems.Length);

            for (int i = 0; i < _selectLogItems.Length; i++)
            {
                if (i < _logCount)
                {
                    _selectLogItems[i].Initialize(logs[i].Content);
                }
                else
                {
                    _selectLogItems[i].Initialize("");
                }
            }

            _selectLogIndex = -1;
        }
        public void InitSelectLogList(params string[] logs)
        {
            _logCount = Mathf.Min(logs.Length, _selectLogItems.Length);

            for (int i = 0; i < _selectLogItems.Length; i++)
            {
                if (i < _logCount)
                {
                    _selectLogItems[i].Initialize(logs[i]);
                }
                else
                {
                    _selectLogItems[i].Initialize("");
                }
            }

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
                    Debug.Log("Timer cancelled");
                    return;
                }

                currentTime--;
                _timerText.text = $"Prooeeding In : {currentTime}s";
            }

            SubmitLogMessage();
        }

        private void SubmitLogMessage()
        {
            int logIndex = (_selectLogIndex == -1) ? Random.Range(0, _logCount) : _selectLogIndex;
            string logText = _selectLogItems[logIndex].GetLogText();

            VoteManager.Instance.SubmitLogMessageToServerRpc(logText);
        }


        public void OnClickSelectLog(int index)
        {
            if (_selectLogIndex == index) return;

            _selectLogIndex = index;

            for (int i = 0; i < _selectLogItems.Length; i++)
            {
                _selectLogItems[i].OnSelect(i == index);
            }
        }
    }
}
