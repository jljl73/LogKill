using Cysharp.Threading.Tasks;
using LogKill.Character;
using LogKill.Utils;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.UI
{
    public class SDcardWindow : WindowBase
    {
        [SerializeField] private TMP_Text _nameText;
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
            int index = resultText.IndexOf('\n');
            string nameText = resultText.Substring(0, index);
            string logText = resultText.Substring(index + 1);

            if (Enum.TryParse(nameText, out EColorType colorType))
            {
                _nameText.color = Util.GetColor(colorType);
            }

            _nameText.text = string.Empty;
            for (int i = 0; i < nameText.Length; i++)
            {
                _nameText.text += nameText[i];
                await UniTask.Delay(_textDelayTime);
            }

            _logText.text = string.Empty;
            for (int i = 0; i < logText.Length; i++)
            {
                _logText.text += logText[i];
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
