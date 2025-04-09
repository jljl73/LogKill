using LogKill.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.Vote
{
    public class VotePanel : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _voteCompleteImage;

        [SerializeField] private Button _panelButton;

        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _logText;
        [SerializeField] private TMP_Text _voteCountText;

        [SerializeField] private GameObject _deadPanel;
        [SerializeField] private GameObject _loadingPanel;
        [SerializeField] private GameObject _disabledPanel;
        [SerializeField] private GameObject _buttonGroup;

        private EventBus EventBus => ServiceLocator.Get<EventBus>();

        private int _voteCount = 0;
        private ulong _targetClientId;

        public void Initialize(ulong clientId, VoteData voteData, bool isImposter)
        {
            _icon.color = voteData.PlayerData.GetColor();
            _voteCompleteImage.gameObject.SetActive(false);

            _nameText.text = voteData.PlayerData.Name.Value;
            _nameText.color = (isImposter) ? Color.red : Color.black;

            _logText.text = voteData.LogMessage;

            if (voteData.PlayerData.IsDead)
            {
                _panelButton.interactable = false;

                _deadPanel.SetActive(true);
                _loadingPanel.SetActive(false);
            }
            else
            {
                _panelButton.interactable = clientId != voteData.PlayerData.ClientId;

                _deadPanel.SetActive(false);
                _loadingPanel.SetActive(voteData.LogMessage == "");
            }

            _voteCount = 0;
            _targetClientId = voteData.PlayerData.ClientId;
        }

        public void OnSelect(bool isSelect)
        {
            _buttonGroup.SetActive(isSelect);
        }

        public void OnDisabled()
        {
            _panelButton.interactable = false;
            _buttonGroup.SetActive(false);
            _disabledPanel.SetActive(true);
        }

        public void OnSelectLogMessage(string logMessage)
        {
            _loadingPanel.SetActive(false);
            _logText.text = logMessage;
        }

        public void OnVoteComplete()
        {
            _voteCompleteImage.gameObject.SetActive(true);
        }

        public void AddVoteCount()
        {
            _voteCount++;
            _voteCountText.text = _voteCount.ToString();
        }


        public void OnClickVote()
        {
            _buttonGroup.SetActive(false);

            VoteManager.Instance.VoteCompleteToServerRpc(_targetClientId, false);
        }


        public void OnClickExit()
        {
            _buttonGroup.SetActive(false);
        }
    }
}