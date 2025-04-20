using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.UI
{
    public class DeathWindow : WindowBase
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image[] _cutScenes;
        [SerializeField] private float _duration;

        public override void OnShow()
        {
            base.OnShow();
            PlayScene().Forget();
        }

        private async UniTask PlayScene()
        {
            for (int i = 0; i < _cutScenes.Length; i++)
            {
                _cutScenes[i].gameObject.SetActive(true);
                float alpha = 0f;

                while (alpha < 1f)
                {
                    alpha += Time.deltaTime;
                    _canvasGroup.alpha = alpha;
                    await UniTask.Yield();
                }

                await UniTask.Delay((int)(_duration * 1000f));

                while (alpha > 0)
                {
                    alpha -= Time.deltaTime;
                    _canvasGroup.alpha = alpha;
                    await UniTask.Yield();
                }

                _cutScenes[i].gameObject.SetActive(false);
            }

            UIManager.Instance.CloseCurrentWindow();
        }
    }
}
