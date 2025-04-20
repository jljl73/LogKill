using LogKill.Character;
using LogKill.Core;
using LogKill.Event;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.Vote
{
    public class VotePanel : MonoBehaviour
    {
        [SerializeField] private Image _icon;

        [SerializeField] private Button _panelButton;

        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _logText;
        [SerializeField] private TMP_Text _voteCountText;

        [SerializeField] private GameObject _deadPanel;
        [SerializeField] private GameObject _buttonGroup;

        private EventBus EventBus => ServiceLocator.Get<EventBus>();

        private ulong _targetClientId;

        public void InitVotePanel(PlayerData localPlayerData, VoteData voteData)
        {
            _icon.sprite = GetColorSprite(voteData.PlayerData.ColorType);

            if (localPlayerData.GetIsImposter() && voteData.PlayerData.PlayerType == EPlayerType.Imposter)
                _nameText.color = Color.red;
            else
                _nameText.color = Color.white;
            _nameText.text = voteData.PlayerData.Name.Value;

            _logText.text = voteData.LogMessage;

            _deadPanel.SetActive(voteData.PlayerData.IsDead);

            if (localPlayerData.IsDead)
                _panelButton.interactable = false;
            else
            {
                if (voteData.PlayerData.IsDead || localPlayerData.ClientId == voteData.PlayerData.ClientId)
                    _panelButton.interactable = false;
                else
                    _panelButton.interactable = true;
            }

            _targetClientId = voteData.PlayerData.ClientId;
        }

        private Sprite GetColorSprite(EColorType colorType)
        {
            return SpriteResourceManager.Instance.GetPlayerSprite(colorType);
        }

        public void OnSelect(bool isSelect)
        {
            _buttonGroup.SetActive(isSelect);
        }

        public void OnDisabledPanel()
        {
            if (_buttonGroup.activeSelf)
                _buttonGroup.SetActive(false);

            _panelButton.interactable = false;
        }

        public void OnUpdateVoteResult(int voteCount)
        {
            _voteCountText.text = voteCount.ToString();
        }

        public void OnClickVote()
        {
            EventBus.Publish<VoteCompleteEvent>(new VoteCompleteEvent
            {
                TargetClientId = _targetClientId
            });
        }

        public void OnClickExit()
        {
            _buttonGroup.SetActive(false);
        }
    }
}