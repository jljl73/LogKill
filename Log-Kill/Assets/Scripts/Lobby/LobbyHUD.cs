using Cysharp.Threading.Tasks;
using LogKill.Core;
using LogKill.Event;
using LogKill.LobbySystem;
using LogKill.Room;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace LogKill.UI
{
    public class LobbyHUD : HUDBase
    {
        [SerializeField] private TMP_Text _playerCountText;
        [SerializeField] private TMP_Text _lobbyCodeText;

        [SerializeField] private TMP_Text _accessStateText;
        [SerializeField] private Button _accessStateToggleButton;

        [SerializeField] private PlayerListPanel _playerListPanel;
        [SerializeField] private Button _playerListPanelButton;

        [SerializeField] private Button _startButton;

        private EventBus EventBus => ServiceLocator.Get<EventBus>();
        private LobbyManager LobbyManager => ServiceLocator.Get<LobbyManager>();


        public override void OnShow()
        {
            EventBus.Subscribe<LobbyChangedEvent>(OnLobbyChangedEvent);

            var Lobby = LobbyManager.CurrentLobby;

            bool isHost = LobbyManager.GetIsHost();

            _accessStateToggleButton.gameObject.SetActive(isHost);
            _startButton.gameObject.SetActive(isHost);
            _playerListPanelButton.gameObject.SetActive(isHost);

            _playerListPanel.Initialize();
            _playerListPanel.gameObject.SetActive(false);

            if (isHost)
            {
                UpdateAccessStateText(Lobby.IsPrivate);
            }

            UpdatePlayerCount(Lobby.Players.Count, Lobby.MaxPlayers);

            _lobbyCodeText.text = Lobby.LobbyCode;
        }

        public override void OnHide()
        {
            EventBus.Unsubscribe<LobbyChangedEvent>(OnLobbyChangedEvent);
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

            //_startButton.interactable = _currentPlayerCount == _maxPlayerCount;
            _startButton.interactable = true;
        }

        private void UpdatePlayerListPanel()
        {
            if (!LobbyManager.GetIsHost()) return;

            _playerListPanel.UpdatePlayerList();
        }

        private void OnLobbyChangedEvent(LobbyChangedEvent context)
        {
            UpdatePlayerCount(context.CurrentPlayers, context.MaxPlayers);
            UpdatePlayerListPanel();
        }

        public async void OnClickAccessStateToggle()
        {
            _accessStateToggleButton.interactable = false;

            bool changeIsPrivate = !LobbyManager.GetIsPrivate();
            await LobbyManager.UpdateIsPrivate(changeIsPrivate);
            await UniTask.Delay(500);
            UpdateAccessStateText(changeIsPrivate);

            _accessStateToggleButton.interactable = true;
        }

        public void OnClickPlayerListPanel()
        {
            _playerListPanel.TogglePanel();
        }

        public async void OnClickGameStart()
        {
            await LobbyManager.UpdateIsPrivate(true);
            SessionManager.Instance.NotifyGameStartServerRpc();
        }

        public void OnClickExit()
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
