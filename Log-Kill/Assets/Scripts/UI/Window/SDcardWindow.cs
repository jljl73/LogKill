using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.UI
{
    public class SDcardWindow : WindowBase
    {
        [SerializeField] private TMP_Text _logText;
        [SerializeField] private Button _closeButton;
        [SerializeField] private int _textDelayTime = 150;

        public void SetLog(string log)
        {
            _closeButton.interactable = false;
            DoText(log).Forget();
        }

        private async UniTask DoText(string resultText)
        {
            _logText.text = string.Empty;
            for (int i = 0; i < resultText.Length; i++)
            {
                _logText.text += resultText[i];
                await UniTask.Delay(_textDelayTime);
            }

            await UniTask.Delay(600);
            _closeButton.interactable = true;
        }

        public void OnCloseButtonClick()
        {
            UIManager.Instance.CloseCurrentWindow();
        }
    }
}
