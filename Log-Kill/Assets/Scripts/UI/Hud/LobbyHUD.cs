using Cysharp.Threading.Tasks;
using LogKill.LobbySystem;
using LogKill.Network;
using System;
using System.Threading;
using TMPro;
using Unity.Netcode;
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

        private CancellationTokenSource _lobbyInfoRefreshToken;

        private int _maxPlayerCount;
        private int _currentPlayerCount;

        private bool _isHost;
        private bool _isPrivate;

        public override void OnShow()
        {
            var Lobby = LobbyManager.Instance.CurrentLobby;

            _isHost = LobbyManager.Instance.GetIsHost();

            _accessStateToggleButton.gameObject.SetActive(_isHost);

            if (_isHost)
            {
                _isPrivate = Lobby.IsPrivate;
                UpdateAccessStateText(_isPrivate);
            }

            _maxPlayerCount = Lobby.MaxPlayers;
            _currentPlayerCount = Lobby.Players.Count;

            UpdatePlayerCount(_currentPlayerCount, _maxPlayerCount);

            _lobbyCodeText.text = Lobby.LobbyCode;

            StartLobbyInfoRefresh();

            LobbyManager.Instance.LeaveLobbyEvent += OnLobbyLeaveComplete;
        }

        public override void OnHide()
        {
            _lobbyInfoRefreshToken?.Cancel();
            _lobbyInfoRefreshToken?.Dispose();
            _lobbyInfoRefreshToken = null;

            LobbyManager.Instance.LeaveLobbyEvent -= OnLobbyLeaveComplete;
        }

        private async UniTask StartLobbyInfoRefresh()
        {
            _lobbyInfoRefreshToken = new CancellationTokenSource();

            try
            {
                while (!_lobbyInfoRefreshToken.Token.IsCancellationRequested)
                {
                    var lobby = await LobbyManager.Instance.GetLobbyAsync();

                    UpdatePlayerCount(lobby.Players.Count, lobby.MaxPlayers);

                    await UniTask.Delay(NetworkConstants.LOBBY_INFOMATION_UPDATE_MS, cancellationToken: _lobbyInfoRefreshToken.Token);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("LobbyInfo loop safely cancelled");
            }
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
        }

        private void OnLobbyLeaveComplete()
        {
            // TODO Scene Move
            UIManager.Instance.HideCurrentHUD();
            UIManager.Instance.ShowWindow<OnlineModeWindow>();
        }

        public async void OnClickAccessStateToggle()
        {
            _isPrivate = !_isPrivate;

            UpdateAccessStateText(_isPrivate);

            await LobbyManager.Instance.UpdateIsPrivate(_isPrivate);
        }

        public void OnClickExit()
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
