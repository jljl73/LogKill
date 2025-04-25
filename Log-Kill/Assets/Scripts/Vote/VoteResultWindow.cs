using Cysharp.Threading.Tasks;
using LogKill.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.Vote
{
    public class VoteResultWindow : WindowBase
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image[] _cutScenes;
        [SerializeField] private TMP_Text _resultText;
        [SerializeField] private float _duration = 1.5f;
        [SerializeField] private int _textDelayTime = 300;

        public override void OnShow()
        {
            _resultText.text = string.Empty;
        }

        public async UniTask PlayScene(string resultText)
        {
            await DoText(resultText);

            for (int i = 0; i < _cutScenes.Length; i++)
            {
                _cutScenes[i].gameObject.SetActive(true);
                float alpha = 0f;

                while (alpha < 1f)
                {
                    alpha += Time.deltaTime * 2.0f;
                    _canvasGroup.alpha = alpha;
                    await UniTask.Yield();
                }

                await UniTask.Delay((int)(_duration * 1000f));

                while (alpha > 0)
                {
                    alpha -= Time.deltaTime * 2.0f;
                    _canvasGroup.alpha = alpha;
                    await UniTask.Yield();
                }

                _cutScenes[i].gameObject.SetActive(false);
            }

            UIManager.Instance.CloseCurrentWindow();
        }

        private async UniTask DoText(string resultText)
        {
            _resultText.text = string.Empty;
            for (int i = 0; i < resultText.Length; i++)
            {
                _resultText.text += resultText[i];
                await UniTask.Delay(_textDelayTime);
            }

            await UniTask.Delay(600);
            _resultText.text = string.Empty;
        }
    }
}
