using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.LobbySystem
{
    public class CountButton : MonoBehaviour
    {
        private Button _countButton;
        private TMP_Text _countText;

        private void Awake()
        {
            _countButton = GetComponent<Button>();
            _countText = GetComponentInChildren<TMP_Text>();
        }

        public void OnSelect(bool isSelect)
        {
            if (isSelect)
            {
                _countButton.image.color = Color.white;
            }
            else
            {
                _countButton.image.color = Color.clear;
            }
        }

        public void OnDisabled(bool isDisabled)
        {
            if (isDisabled)
            {
                _countButton.interactable = false;
                _countText.color = Color.grey;
            }
            else
            {
                _countButton.interactable = true;
                _countText.color = Color.white;
            }
        }
    }
}
