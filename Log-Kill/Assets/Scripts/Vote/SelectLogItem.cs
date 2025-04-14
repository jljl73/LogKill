using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.Vote
{
    public class SelectLogItem : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _logText;

        [SerializeField] private Color _normalColor;
        [SerializeField] private Color _selectedColor;

        public void Initialize(string logText)
        {
            if (string.IsNullOrWhiteSpace(logText))
            {
                gameObject.SetActive(false);
            }
            else
            {
                _logText.text = logText;
                _button.image.color = _normalColor;
                gameObject.SetActive(true);
            }
        }

        public void OnSelect(bool isSelect)
        {
            if (isSelect)
            {
                _button.image.color = _selectedColor;
            }
            else
            {
                _button.image.color = _normalColor;
            }
        }

        public string GetLogMessage()
        {
            return _logText.text;
        }
    }
}
