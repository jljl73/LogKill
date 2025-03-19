using DG.Tweening;
using LogKill.Core;
using TMPro;
using UnityEngine;

namespace LogKill.Mission
{
    public class GaugeMission : MissionBase
    {
        [SerializeField] private RectTransform _gaugeRect;
        [SerializeField] private RectTransform _gaugeBarRect;

        private Tweener _tweener;

        protected override void OnStart()
        {
            var width = _gaugeRect.sizeDelta.x * 0.5f - _gaugeBarRect.sizeDelta.x;
            _tweener = _gaugeBarRect.DOLocalMoveX(width, 1.0f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        }

        public void OnClickCatch()
        {
            if (_tweener.IsPlaying())
                _tweener.Pause();
            else
                _tweener.Play();
        }
    }
}
