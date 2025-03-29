using DG.Tweening;
using LogKill.Core;
using LogKill.Log;
using UnityEngine;

namespace LogKill.Mission
{
    public class GaugeMission : MissionBase
    {
        [SerializeField] private RectTransform _gaugeRect;
        [SerializeField] private RectTransform _gaugeBarRect;
        [SerializeField] private RectTransform _targetRect;

        private Tweener _tweener;
        private LogService LogService => ServiceLocator.Get<LogService>();
        private float _startTime = 0.0f;
        private float _stackTime = 0.0f;

        protected override void OnStart()
        {
            var width = _gaugeRect.sizeDelta.x * 0.5f - _gaugeBarRect.sizeDelta.x;
            _tweener = _gaugeBarRect.DOLocalMoveX(width, 1.0f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
            _startTime = Time.time;
        }

        protected override void OnCancel()
        {
            _stackTime += Time.time - _startTime;
        }

        protected override void OnClear()
        {
            _stackTime += Time.time - _startTime;
            LogService.Log(new MissionTimeLog(_stackTime));
        }

        private bool IsCatch()
        {
            var position = _gaugeBarRect.anchoredPosition.x;
            var targetPosition = _targetRect.anchoredPosition.x;

            var diff = Mathf.Abs(position - targetPosition);
            var width = _targetRect.sizeDelta.x * 0.5f;

            return diff < width;
        }

        public void OnClickCatch()
        {
            if (_tweener.IsPlaying())
            {
                _tweener.Pause();
                if (IsCatch())
                {
                    ClearMission();
                }
                else
                {
                    LogService.Log(new MissionFailLog());
                    LogService.Print(ELogType.MissionFail);
                    CancelMission();
                }
            }
            else
                _tweener.Play();
        }
    }
}
