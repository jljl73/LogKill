using TMPro;
using UnityEngine;

namespace LogKill.UI
{
    public class MessageBoxWindow : MonoBehaviour
    {
        [SerializeField] private TMP_Text _messageText;

        public void OnShow(string message)
        {
            _messageText.text = message;
            gameObject.SetActive(true);
        }

        public void OnHide()
        {
            gameObject.SetActive(false);
        }
    }
}
