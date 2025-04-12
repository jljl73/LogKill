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

        [SerializeField] private LobbyPlayerListPanel _lobbyPlayerListPanel;
        [SerializeField] private Button _lobbyPlayerListPanelButton;

        [SerializeField] private Button _startButton;

        private EventBus EventBus => ServiceLocator.Get<EventBus>();
        private LobbyManager LobbyManager => ServiceLocator.Get<LobbyManager>();


        public override void OnShow()
        {
            var Lobby = LobbyManager.CurrentLobby;

            bool isHost = LobbyManager.GetIsHost();
            if (isHost)
            {
                _accessStateToggleButton.gameObject.SetActive(true);
                _startButton.gameObject.SetActive(true);

                _lobbyPlayerListPanel.Initialize();
                _lobbyPlayerListPanelButton.gameObject.SetActive(true);
                _lobbyPlayerListPanel.gameObject.SetActive(false);

                UpdateAccessStateText(Lobby.IsPrivate);
            }
            else
            {
                _accessStateToggleButton.gameObject.SetActive(false);
                _startButton.gameObject.SetActive(false);
                _lobbyPlayerListPanelButton.gameObject.SetActive(false);
            }

            UpdatePlayerCount(Lobby.Players.Count, Lobby.MaxPlayers);

            _lobbyCodeText.text = Lobby.LobbyCode;

            EventBus.Subscribe<LobbyChangedEvent>(OnLobbyChangedEvent);
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

        private void UpdateLobbyPlayerListPanel(ulong clientId)
        {
            if (!LobbyManager.GetIsHost()) return;

            _lobbyPlayerListPanel.UpdatePlayerList(clientId);
        }

        private void OnLobbyChangedEvent(LobbyChangedEvent context)
        {
            UpdatePlayerCount(context.CurrentPlayers, context.MaxPlayers);

            UpdateLobbyPlayerListPanel(context.ClientId);
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

        public void OnClickLobbyPlayerListPanel()
        {
            if (!_lobbyPlayerListPanel.gameObject.activeSelf)
            {
                _lobbyPlayerListPanel.OnShow();
            }
            else
            {
                _lobbyPlayerListPanel.OnHide();
            }
        }

        public void OnClickGameStart()
        {
            SessionManager.Instance.NotifyGameStartServerRpc();
        }

        public void OnClickExit()
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
