using Cysharp.Threading.Tasks;
using LogKill.UI;
using TMPro;
using UnityEngine;

namespace LogKill.Vote
{
    public class VoteResultWindow : WindowBase
    {
        [SerializeField] private TMP_Text _resultText;
        [SerializeField] private int _textDelayTime = 300;

        public override void OnShow()
        {
            _resultText.text = string.Empty;
        }

        public void SetResultText(string resultText)
        {
            DoText(resultText).Forget();
        }

        private async UniTask DoText(string resultText)
        {
            _resultText.text = string.Empty;
            for (int i = 0; i < resultText.Length; i++)
            {
                _resultText.text += resultText[i];
                await UniTask.Delay(_textDelayTime);
            }

            UIManager.Instance.CloseCurrentWindow();
        }
    }
}
