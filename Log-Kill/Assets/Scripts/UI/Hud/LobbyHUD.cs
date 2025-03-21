using LogKill.LobbySystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.UI
{
    public class LobbyHUD : HUDBase
    {
        [SerializeField] private Button _accessStateToggleButton;
        [SerializeField] private TMP_Text _accessStateText;

        [SerializeField] private TMP_Text _playerCountText;
        [SerializeField] private TMP_Text _lobbyCodeText;


        private int _maxPlayerCount;
        private int _currentPlayerCount;

        private bool _isPrivate;

        public override void Initialize()
        {
            base.Initialize();

            _maxPlayerCount = LobbyManager.Instance.GetMaxPlayers();
            _currentPlayerCount = LobbyManager.Instance.GetPlayerCount();
            UpdatePlayerCount(_currentPlayerCount, _maxPlayerCount);

            _accessStateToggleButton.gameObject.SetActive(LobbyManager.Instance.GetIsHost());

            if (_accessStateToggleButton.gameObject.activeSelf)
            {
                _isPrivate = LobbyManager.Instance.GetIsPrivate();
                UpdateAccessStateText(_isPrivate);
            }

            _lobbyCodeText.text = LobbyManager.Instance.GetLobbyCode();
        }

        public async void OnClickAccessStateToggle()
        {
            _isPrivate = !_isPrivate;

            UpdateAccessStateText(_isPrivate);

            await LobbyManager.Instance.UpdateLobbyWithIsPrivate(_isPrivate);
        }

        private void UpdateAccessStateText(bool isPrivate)
        {
            if (isPrivate)
            {
                _accessStateText.text = "PRIVATE";
                _accessStateText.color = Color.red;
            }
            else
            {
                _accessStateText.text = "PUBLIC";
                _accessStateText.color = Color.green;
            }
        }

        private void UpdatePlayerCount(int currentPlayerCount, int maxPlayerCount)
        {
            _playerCountText.text = $"{currentPlayerCount} / {maxPlayerCount}";
        }
    }
}
