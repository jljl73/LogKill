using DG.Tweening;
using LogKill.Core;
using LogKill.Log;
using UnityEngine;

namespace LogKill.Mission
{
    public class GaugeMission : MissionBase
    {
        private const float MAX_TARGET_SIZE = 60f;
        private const float MIN_TARGET_SIZE = 30f;

        private const float MAX_TWEEN_TIME = 1f;
        private const float MIN_TWEEN_TIME = 0.4f;

        [SerializeField][Range(0f, 1f)] private float _speedDifficultyFactor;
        [SerializeField][Range(0f, 1f)] private float _targetSizeDifficultyFactor;

        [SerializeField] private RectTransform _gaugeRect;
        [SerializeField] private RectTransform _gaugeBarRect;
        [SerializeField] private RectTransform _targetRect;

        private Tweener _tweener;
        private float _startTime = 0.0f;
        private float _stackTime = 0.0f;
        private Vector3 _defaultPosition;
        private Vector2 _defaultTargetSize;
        private LogService LogService => ServiceLocator.Get<LogService>();

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _defaultPosition = _gaugeBarRect.anchoredPosition;
            _defaultTargetSize = _targetRect.sizeDelta;
        }

        protected override void OnStart()
        {
            SetTargetRandomSize();
            StartGaugeBarTween();
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

        private void SetTargetRandomSize()
        {
            //_targetSizeDifficultyFactor = Random.Range(0f, 1f);
            float randomTargetSizeX = Mathf.Lerp(MAX_TARGET_SIZE, MIN_TARGET_SIZE, _targetSizeDifficultyFactor);
            _targetRect.sizeDelta = new Vector2(randomTargetSizeX, _defaultTargetSize.y);
        }

        private void StartGaugeBarTween()
        {
            var width = -_defaultPosition.x;

            //_speedDifficultyFactor = Random.Range(0f, 1f);
            float randomSpeed = Mathf.Lerp(MAX_TWEEN_TIME, MIN_TWEEN_TIME, _speedDifficultyFactor);
            _tweener = _gaugeBarRect.DOLocalMoveX(width, randomSpeed).ChangeStartValue(_defaultPosition).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
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
                if (IsCatch())
                {
                    LogService.Log(new MissionSuccessLog());
                    ClearMission();
                }
                else
                {
                    LogService.Log(new MissionFailLog());
                    CancelMission();
                }
                _tweener.Complete();
            }
        }
    }
}
