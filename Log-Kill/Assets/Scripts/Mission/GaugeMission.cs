using DG.Tweening;
using UnityEngine;

namespace LogKill.Mission
{
    public class GaugeMission : MissionBase
    {
        [SerializeField] private RectTransform _gaugeRect;
        [SerializeField] private RectTransform _gaugeBarRect;
        [SerializeField] private RectTransform _targetRect;

        private Tweener _tweener;

        protected override void OnStart()
        {
            var width = _gaugeRect.sizeDelta.x * 0.5f - _gaugeBarRect.sizeDelta.x;
            _tweener = _gaugeBarRect.DOLocalMoveX(width, 1.0f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
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
                Debug.Log(IsCatch() ? "Catch" : "Miss");
            }
            else
                _tweener.Play();
        }
    }
}
