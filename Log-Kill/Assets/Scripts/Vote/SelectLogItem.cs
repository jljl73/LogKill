using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.Vote
{
    public class SelectLogItem : MonoBehaviour
    {
        private Button _button;
        private TMP_Text _logText;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _logText = GetComponentInChildren<TMP_Text>();
        }

        public void Initialize(string logText)
        {
            if (logText != string.Empty)
            {
                _logText.text = logText;
                _button.image.color = new Color(1f, 1f, 1f, 0.5f);
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void OnSelect(bool isSelect)
        {
            if (isSelect)
            {
                _button.image.color = Color.black;
            }
            else
            {
                _button.image.color = new Color(1f, 1f, 1f, 0.5f);
            }
        }

        public string GetLogText()
        {
            return _logText.text;
        }
    }
}
