using Cysharp.Threading.Tasks;
using LogKill.LobbySystem;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.UI
{
    public class LobbyHUD : HUDBase
    {
        [SerializeField] private TMP_Text _playerCountText;
        [SerializeField] private TMP_Text _lobbyCodeText;

        [SerializeField] private Button _accessStateToggleButton;
        [SerializeField] private TMP_Text _accessStateText;

        [SerializeField] private Button _startButton;

        private int _maxPlayerCount;
        private int _currentPlayerCount;

        private bool _isHost;
        private bool _isPrivate;

        public override void OnShow()
        {
            var Lobby = LobbyManager.Instance.CurrentLobby;

            _isHost = LobbyManager.Instance.GetIsHost();

            _accessStateToggleButton.gameObject.SetActive(_isHost);
            _startButton.gameObject.SetActive(_isHost);

            if (_isHost)
            {
                _isPrivate = Lobby.IsPrivate;
                UpdateAccessStateText(_isPrivate);
            }

            _maxPlayerCount = Lobby.MaxPlayers;
            _currentPlayerCount = Lobby.Players.Count;

            UpdatePlayerCount(_currentPlayerCount, _maxPlayerCount);

            _lobbyCodeText.text = Lobby.LobbyCode;

            LobbyManager.Instance.LobbyChangedEvent += OnLobbyChangedEvent;
            LobbyManager.Instance.LobbyLeavedEvent += OnLobbyLeavedEvent;
        }

        public override void OnHide()
        {
            LobbyManager.Instance.LobbyChangedEvent -= OnLobbyChangedEvent;
            LobbyManager.Instance.LobbyLeavedEvent -= OnLobbyLeavedEvent;
        }

        private void OnLobbyChangedEvent(Lobby lobby)
        {
            if (lobby == null)
            {
                Debug.Log("Lobby is Null");
            }
            else
            {
                UpdatePlayerCount(lobby.Players.Count, lobby.MaxPlayers);
            }
        }

        private void OnLobbyLeavedEvent()
        {
            // TODO Scene Move
            UIManager.Instance.HideCurrentHUD();
            var onlineModeWindow = UIManager.Instance.ShowWindow<OnlineModeWindow>();
            onlineModeWindow.Initialize();
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
            _currentPlayerCount = currentPlayerCount;
            _maxPlayerCount = maxPlayerCount;

            _playerCountText.text = $"{currentPlayerCount} / {maxPlayerCount}";

            _startButton.interactable = _currentPlayerCount == _maxPlayerCount;
        }

        public async void OnClickAccessStateToggle()
        {
            _isPrivate = !_isPrivate;

            _accessStateToggleButton.interactable = false;

            await LobbyManager.Instance.UpdateIsPrivate(_isPrivate);
            await UniTask.Delay(1000);
            UpdateAccessStateText(_isPrivate);

            _accessStateToggleButton.interactable = true;
        }

        public void OnClickGameStart()
        {
            Debug.Log("GameStart");
        }

        public async void OnClickExit()
        {
            await LobbyManager.Instance.LeaveLobbyAsync();
        }
    }
}
