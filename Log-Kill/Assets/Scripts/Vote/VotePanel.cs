using LogKill.Character;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.Vote
{
    public class VotePanel : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Button _buton;

        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _logText;

        [SerializeField] private GameObject _deadPanel;
        [SerializeField] private GameObject _loadingPanel;
        [SerializeField] private GameObject _buttonGroup;

        public void Initialize(PlayerData playerData, string logMessage, bool isImposter)
        {
            _buton.interactable = true;
            // _icon.color = playerData.GetColor();

            // _nameText.text = playerData.Name.Value;
            _nameText.color = (isImposter) ? Color.red : Color.black;

            _logText.text = logMessage;

            _deadPanel.SetActive(playerData.IsDead);
        }

        public void OnSelect(bool isSelect)
        {
            _buttonGroup.SetActive(isSelect);
        }

        public void OnDisabledPanelButton()
        {
            _buton.interactable = false;
        }

        public void OnClickVote()
        {

        }

        public void OnClickExit()
        {
            _buttonGroup.SetActive(false);
        }
    }
}